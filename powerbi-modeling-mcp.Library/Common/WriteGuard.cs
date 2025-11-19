// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using ModelContextProtocol.Server;
using PowerBIModelingMCP.Library.Contracts;

#nullable enable
namespace PowerBIModelingMCP.Library.Common;

public static class WriteGuard
{
  private static MCPServerConfiguration? _cachedConfig;
  private static readonly object _lock = new object();
  private const string ReadOnlyErrorMessage = "The MCP Server is currently running in read-only mode. To perform this operation, user must explicitly start the MCP Server in read-write mode.";

  public static void Initialize(MCPServerConfiguration config)
  {
    lock (WriteGuard._lock)
      WriteGuard._cachedConfig = config;
  }

  public static void AssertWriteAllowed(string opName)
  {
    (bool allowed, string message) = WriteGuard.IsWriteAllowed(opName);
    if (!allowed)
      throw new WriteForbiddenException(message);
  }

  public static (bool allowed, string message) IsWriteAllowed(string opName)
  {
    if (WriteGuard._cachedConfig == null)
      return (false, $"Operation '{opName}' is not permitted: {"The MCP Server is currently running in read-only mode. To perform this operation, user must explicitly start the MCP Server in read-write mode."} (no configuration set)");
    return WriteGuard._cachedConfig.Mode == ToolMode.ReadWrite ? (true, "Write allowed.") : (false, $"Operation '{opName}' is not permitted: The MCP Server is currently running in read-only mode. To perform this operation, user must explicitly start the MCP Server in read-write mode.");
  }

  public static WriteOperationResult ExecuteWriteOperationWithGuards(
    McpServer mcpServer,
    string? connectionName,
    string operationName)
  {
    try
    {
      WriteGuard.AssertWriteAllowed(operationName);
    }
    catch (WriteForbiddenException ex)
    {
      return new WriteOperationResult()
      {
        Success = false,
        Message = ex.Message
      };
    }
    if (!ConfirmationService.ConfirmRequest(mcpServer, connectionName, ConfirmationType.WriteOperation))
      return new WriteOperationResult()
      {
        Success = false,
        Message = "The user requested a write operation but declined when asked to confirm. Do not retry or initiate any write operations on your own. Wait until the user explicitly confirms or requests a write operation again.",
        UserDeclinedConfirmation = true
      };
    (bool flag, string str) = SyncService.EnsureFreshMetadataForOperation(mcpServer, connectionName, operationName);
    if (!flag)
      return new WriteOperationResult()
      {
        Success = false,
        Message = "Failed to ensure fresh metadata from server before write operation: " + (str ?? "Unknown error")
      };
    return new WriteOperationResult()
    {
      Success = true,
      Message = "Write operation guards passed successfully"
    };
  }

  public static void AssertPowerBICompatible(string opName, string reason)
  {
    if (WriteGuard._cachedConfig != null && WriteGuard._cachedConfig.Compatibility == CompatibilityMode.PowerBI)
      throw new CompatibilityException($"Operation '{opName}' is not supported in PowerBI compatibility mode: {reason}");
  }

  public static bool IsPowerBICompatible(string opName, string reason)
  {
    return WriteGuard._cachedConfig == null || WriteGuard._cachedConfig.Compatibility != 0;
  }

  public static bool IsSkipConfirmationEnabled()
  {
    MCPServerConfiguration cachedConfig = WriteGuard._cachedConfig;
    return cachedConfig != null && cachedConfig.SkipConfirmation;
  }
}
