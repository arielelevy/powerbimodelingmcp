// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Core.ConnectionValidator
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using ModelContextProtocol;
using PowerBIModelingMCP.Library.Common.DataStructures;

#nullable enable
namespace PowerBIModelingMCP.Library.Core;

public static class ConnectionValidator
{
  public static void ValidateForTransactions(ConnectionInfo connection)
  {
    if (connection == null)
      throw new McpException("Connection cannot be null");
    if (connection.IsOffline)
      throw new McpException("Transaction operations are not supported on offline connections");
    if (connection.TabularServer == null)
      throw new McpException("Active server connection is required for transaction operations");
  }

  public static void ValidateForDaxQueries(ConnectionInfo connection)
  {
    if (connection == null)
      throw new McpException("Connection cannot be null");
    if (connection.IsOffline)
      throw new McpException("DAX query operations are not supported on offline connections");
    if (connection.AdomdConnection == null)
      throw new McpException("ADOMD connection is required for DAX queries");
  }

  public static void ValidateForTrace(ConnectionInfo connection)
  {
    if (connection == null)
      throw new McpException("Connection cannot be null");
    if (connection.IsOffline)
      throw new McpException("Trace operations are not supported on offline (TMDL) connections");
    if (connection.TabularServer == null)
      throw new McpException("Connection does not have an active server connection for trace operations");
  }

  public static void ValidateForModelOperations(ConnectionInfo connection)
  {
    if (connection == null)
      throw new McpException("Connection cannot be null");
    if (connection.Database == null)
      throw new McpException("Database reference is required for model operations");
  }

  public static void ValidateOnlineConnection(ConnectionInfo connection)
  {
    if (connection == null)
      throw new McpException("Connection cannot be null");
    if (connection.IsOffline)
      throw new McpException("This operation requires an online connection");
    if (connection.TabularServer == null)
      throw new McpException("Active server connection is required for this operation");
  }

  public static void ValidateOfflineConnection(ConnectionInfo connection)
  {
    if (connection == null)
      throw new McpException("Connection cannot be null");
    if (!connection.IsOffline)
      throw new McpException("This operation requires an offline connection");
    if (connection.Database == null)
      throw new McpException("Database reference is required for offline operations");
  }
}
