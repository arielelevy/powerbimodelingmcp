// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
namespace PowerBIModelingMCP.Library.Common;

public static class ElicitationRequestHandler
{
  private const string ConfirmPropertyName = "Confirm the operation";
  private const string SuccessResponseAction = "accept";
  private const string RejectResponseAction = "decline";
  private const string CancelResponseAction = "cancel";

  public static async Task<bool> HandleConfirmRequest(
    McpServer server,
    string databaseName,
    string message,
    ConfirmationType confirmationType)
  {
    if (server == null)
      throw new ArgumentNullException(nameof (server));
    if (server.ClientCapabilities == null || server.ClientCapabilities.Elicitation == null)
      return true;
    try
    {
      McpServer mcpServer = server;
      ElicitRequestParams elicitRequestParams1 = new ElicitRequestParams { Message = message };
      ElicitRequestParams elicitRequestParams2 = elicitRequestParams1;
      ElicitRequestParams.RequestSchema requestSchema1 = new ElicitRequestParams.RequestSchema();
      ElicitRequestParams.RequestSchema requestSchema2 = requestSchema1;
      Dictionary<string, ElicitRequestParams.PrimitiveSchemaDefinition> dictionary1 = new Dictionary<string, ElicitRequestParams.PrimitiveSchemaDefinition>();
      Dictionary<string, ElicitRequestParams.PrimitiveSchemaDefinition> dictionary2 = dictionary1;
      ElicitRequestParams.EnumSchema enumSchema1 = new ElicitRequestParams.EnumSchema();
      enumSchema1.Type = "string";
      enumSchema1.Enum = (IList<string>) new string[2]
      {
        "Yes",
        "No"
      };
      enumSchema1.EnumNames = (IList<string>) new string[2]
      {
        "Continue the operation",
        "Decline the operation"
      };
      enumSchema1.Description = "Confirm the operation";
      ElicitRequestParams.EnumSchema enumSchema2 = enumSchema1;
      dictionary2["Confirm the operation"] = (ElicitRequestParams.PrimitiveSchemaDefinition) enumSchema2;
      Dictionary<string, ElicitRequestParams.PrimitiveSchemaDefinition> dictionary3 = dictionary1;
      requestSchema2.Properties = (IDictionary<string, ElicitRequestParams.PrimitiveSchemaDefinition>) dictionary3;
      requestSchema1.Required = (IList<string>) new string[1]
      {
        "Confirm the operation"
      };
      ElicitRequestParams.RequestSchema requestSchema3 = requestSchema1;
      elicitRequestParams2.RequestedSchema = requestSchema3;
      ElicitRequestParams request = elicitRequestParams1;
      CancellationToken cancellationToken = new CancellationToken();
      ElicitResult elicitResult = await mcpServer.ElicitAsync(request, cancellationToken);
      if ((elicitResult.Action == "accept"))
      {
        JsonElement jsonElement;
        if (elicitResult.Content.TryGetValue("Confirm the operation", out jsonElement))
        {
          string str = jsonElement.GetString();
          if (!string.IsNullOrEmpty(str))
          {
            if (!str.Equals("Yes", StringComparison.OrdinalIgnoreCase))
            {
              if (!str.Equals("Y", StringComparison.OrdinalIgnoreCase))
                goto label_14;
            }
            if (confirmationType != ConfirmationType.GenericOperation)
              ConfirmationService.CacheConfirmedOperation(databaseName, confirmationType);
            return true;
          }
        }
      }
    }
    catch (Exception ex)
    {
      throw new McpException("Failed to handle confirm request. " + ex.Message);
    }
label_14:
    return false;
  }
}
