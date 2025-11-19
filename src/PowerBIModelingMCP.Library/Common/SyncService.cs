// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.SyncService
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using Microsoft.AnalysisServices.Tabular;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using PowerBIModelingMCP.Library.Common.DataStructures;
using PowerBIModelingMCP.Library.Core;
using System;

#nullable enable
namespace PowerBIModelingMCP.Library.Common;

public static class SyncService
{
  public static void SyncWithServer(string? connectionName)
  {
    SyncService.SyncWithServer(connectionName, SyncService.SyncMode.SkipIfLocalChanges);
  }

  public static bool SyncWithServer(string? connectionName, SyncService.SyncMode mode)
  {
    return SyncService.SyncWithServerInternal(ConnectionOperations.Get(connectionName), mode);
  }

  public static SyncStateInfo GetSyncState(string? connectionName)
  {
    try
    {
      ConnectionInfo connectionInfo = ConnectionOperations.Get(connectionName);
      if (connectionInfo?.Database?.Model == null)
        return new SyncStateInfo()
        {
          CanSync = false,
          HasLocalChanges = false,
          IsOffline = connectionInfo == null || connectionInfo.IsOffline,
          LastSynced = (DateTime?) connectionInfo?.LastSynced,
          Message = "No valid connection or model available"
        };
      bool hasLocalChanges = connectionInfo.Database.Model.HasLocalChanges;
      bool canSync = !connectionInfo.IsOffline && connectionInfo.TabularServer != null;
      return new SyncStateInfo()
      {
        CanSync = canSync,
        HasLocalChanges = hasLocalChanges,
        IsOffline = connectionInfo.IsOffline,
        IsInTransaction = TransactionOperations.IsInTransaction(connectionName),
        LastSynced = connectionInfo.LastSynced,
        Message = SyncService.GetSyncStateMessage(canSync, hasLocalChanges, connectionInfo.IsOffline)
      };
    }
    catch (Exception ex)
    {
      return new SyncStateInfo()
      {
        CanSync = false,
        HasLocalChanges = false,
        IsOffline = true,
        Message = "Error checking sync state: " + ex.Message
      };
    }
  }

  public static (bool, string?) EnsureFreshMetadataForOperation(
    McpServer mcpServer,
    string? connectionName,
    string operationName)
  {
    string message;
    if (!SyncService.EnsureFreshMetadataForOperationImpl(connectionName, operationName, out message))
    {
      if (!ConfirmationService.ConfirmGenericRequest(mcpServer, connectionName, "There are local changes that have not been saved. Do you want to discard local changes and refresh metadata from the server?"))
        return (false, message + " The user declined to discard local changes. Do not retry or initiate any further write operations on your own. Wait until user explicitly confirms or requests a write operation again. The user must save local changes on the model and re-apply changes manually.");
      if (!SyncService.SyncWithServer(connectionName, SyncService.SyncMode.DiscardLocalChanges))
        return (false, "Failed to discard local changes and sync with server.");
    }
    return (true, string.Empty);
  }

  private static bool EnsureFreshMetadataForOperationImpl(
    string? connectionName,
    string operationName,
    out string? message)
  {
    message = (string) null;
    try
    {
      SyncStateInfo syncState = SyncService.GetSyncState(connectionName);
      if (!syncState.CanSync)
        return true;
      if (!syncState.HasLocalChanges)
        return SyncService.SyncWithServer(connectionName, SyncService.SyncMode.SkipIfLocalChanges);
      if (syncState.IsInTransaction)
      {
        message = $"Operation '{operationName}': Skipping sync due to active transaction with local changes. Save local work and re-apply changes manually";
        return true;
      }
      message = $"Operation '{operationName}': There are local changes that haven't been saved. Either commit the transaction or discard local changes";
      return false;
    }
    catch (Exception ex)
    {
      throw new McpException("Failed to ensure fresh metadata because: " + ex.Message);
    }
  }

  private static bool SyncWithServerInternal(ConnectionInfo info, SyncService.SyncMode mode)
  {
    if (info.Database?.Model == null)
      throw new McpException("Model is not present");
    Model model = info.Database.Model;
    bool hasLocalChanges = model.HasLocalChanges;
    switch (mode)
    {
      case SyncService.SyncMode.SkipIfLocalChanges:
        if (hasLocalChanges)
          return true;
        break;
      case SyncService.SyncMode.DiscardLocalChanges:
        if (hasLocalChanges)
        {
          try
          {
            model.UndoLocalChanges();
            break;
          }
          catch (Exception ex)
          {
            throw new McpException("Failed to discard local changes: " + ex.Message);
          }
        }
        else
          break;
      case SyncService.SyncMode.CommitLocalChangesThenSync:
        if (hasLocalChanges)
        {
          try
          {
            model.SaveChanges();
            break;
          }
          catch (Exception ex)
          {
            throw new McpException("Failed to commit local changes before sync: " + ex.Message);
          }
        }
        else
          break;
    }
    ConnectionOperations.Sync(info);
    return true;
  }

  private static string GetSyncStateMessage(bool canSync, bool hasLocalChanges, bool isOffline)
  {
    if (isOffline)
      return "Connection is offline - sync not available";
    if (!canSync)
      return "No server connection available for sync";
    return hasLocalChanges ? "Has local changes - sync will be skipped in default mode" : "Ready to sync";
  }

  public enum SyncMode
  {
    SkipIfLocalChanges,
    DiscardLocalChanges,
    CommitLocalChangesThenSync,
  }
}
