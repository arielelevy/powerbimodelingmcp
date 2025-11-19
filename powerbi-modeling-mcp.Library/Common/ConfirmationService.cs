// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using ModelContextProtocol.Server;
using PowerBIModelingMCP.Library.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

#nullable enable
namespace PowerBIModelingMCP.Library.Common;

public static class ConfirmationService
{
  private static readonly ConcurrentDictionary<string, IList<ConfirmationType>> confirmedDatabases = new ConcurrentDictionary<string, IList<ConfirmationType>>();

  public static void CacheConfirmedOperation(string databaseName, ConfirmationType confirmationType)
  {
    IList<ConfirmationType> confirmationTypeList1;
    if (ConfirmationService.confirmedDatabases.TryGetValue(databaseName, out confirmationTypeList1))
    {
      if (((ICollection<ConfirmationType>) confirmationTypeList1).Contains(confirmationType))
        return;
      ((ICollection<ConfirmationType>) confirmationTypeList1).Add(confirmationType);
    }
    else
    {
      ConcurrentDictionary<string, IList<ConfirmationType>> confirmedDatabases = ConfirmationService.confirmedDatabases;
      string str = databaseName;
      List<ConfirmationType> confirmationTypeList2 = new List<ConfirmationType>();
      confirmationTypeList2.Add(confirmationType);
      confirmedDatabases[str] = (IList<ConfirmationType>) confirmationTypeList2;
    }
  }

  public static bool CheckForConfirmedAction(
    string databaseName,
    ConfirmationType requestedConfirmationType)
  {
    IList<ConfirmationType> confirmationTypeList;
    return ConfirmationService.confirmedDatabases.TryGetValue(databaseName, out confirmationTypeList) && ((ICollection<ConfirmationType>) confirmationTypeList).Contains(requestedConfirmationType);
  }

  public static async Task<bool> RequestConfirmationAsync(
    McpServer server,
    string databaseName,
    string message,
    ConfirmationType confirmationType)
  {
    return await ElicitationRequestHandler.HandleConfirmRequest(server, databaseName, message, confirmationType);
  }

  public static async Task<bool> ValidateConfirmationAsync(
    McpServer server,
    string databaseName,
    string message,
    ConfirmationType confirmationType)
  {
    return WriteGuard.IsSkipConfirmationEnabled() || confirmationType != ConfirmationType.GenericOperation && ConfirmationService.CheckForConfirmedAction(databaseName, confirmationType) || await ConfirmationService.RequestConfirmationAsync(server, databaseName, message, confirmationType);
  }

  public static bool ConfirmRequest(
    McpServer server,
    string? connectionName,
    ConfirmationType confirmationType)
  {
    if (server == null)
      throw new ArgumentNullException(nameof (server));
    if (WriteGuard.IsSkipConfirmationEnabled())
      return true;
    string databaseName;
    bool isLLMCreated;
    ConnectionOperations.GetConnectionDetails(connectionName, out string _, out databaseName, out isLLMCreated);
    if (isLLMCreated)
      return true;
    if (string.IsNullOrEmpty(databaseName))
      throw new ArgumentException("Connection does not have a database specified.", nameof (connectionName));
    string message;
    if (confirmationType != ConfirmationType.WriteOperation)
    {
      if (confirmationType != ConfirmationType.DaxOperation)
        throw new ArgumentOutOfRangeException(nameof (confirmationType), "Unsupported confirmation type.");
      message = $"Are you sure you want to execute dax queries on your database: '{databaseName}'?";
    }
    else
      message = $"Are you sure you want to perform operations that will modify your database: '{databaseName}'?";
    return ConfirmationService.ValidateConfirmationAsync(server, databaseName, message, confirmationType).Result;
  }

  public static bool ConfirmGenericRequest(McpServer server, string? connectionName, string request)
  {
    if (server == null)
      throw new ArgumentNullException(nameof (server));
    if (WriteGuard.IsSkipConfirmationEnabled())
      return true;
    string databaseName;
    ConnectionOperations.GetConnectionDetails(connectionName, out string _, out databaseName, out bool _);
    if (string.IsNullOrEmpty(databaseName))
      throw new ArgumentException("Connection does not have a database specified.", nameof (connectionName));
    return ElicitationRequestHandler.HandleConfirmRequest(server, databaseName, request, ConfirmationType.GenericOperation).Result;
  }
}
