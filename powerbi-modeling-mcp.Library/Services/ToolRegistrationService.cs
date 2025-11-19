// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Services.ToolRegistrationService
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PowerBIModelingMCP.Library.Common;
using PowerBIModelingMCP.Library.Contracts;
using PowerBIModelingMCP.Library.Tools;
using System;

#nullable enable
namespace PowerBIModelingMCP.Library.Services;

public class ToolRegistrationService
{
  private readonly MCPServerConfiguration _config;
  private readonly ILogger<ToolRegistrationService> _logger;

  public ToolRegistrationService(
    MCPServerConfiguration config,
    ILogger<ToolRegistrationService> logger)
  {
    this._config = config;
    this._logger = logger;
  }

  public void RegisterTools(IMcpServerBuilder mcpBuilder)
  {
    this._logger.LogInformation("=== Tool Registration Started ===");
    this._logger.LogInformation("Tool Configuration at registration time:");
    this._logger.LogInformation("  Tool Mode: {Mode}", (object) this._config.Mode);
    this._logger.LogInformation("  Current Tool Mode: {ToolMode}", (object) this._config.GetEnabledToolMode());
    if (!this._config.IsValid())
    {
      this._logger.LogError("Invalid tool configuration: Tool mode must be valid.");
      this._logger.LogError("Current settings - Mode: {Mode}", (object) this._config.Mode);
      throw new InvalidOperationException("Invalid tool configuration: Tool mode must be valid.");
    }
    WriteGuard.Initialize(this._config);
    ToolsConfiguration tools = this._config.Tools;
    this._logger.LogInformation("Registering tools (all tools registered, write operations controlled by WriteGuard)...");
    if (tools.EnableDatabaseOperationsTool)
    {
      this._logger.LogInformation("Loading tool: {ToolName}", (object) "DatabaseOperationsTool");
      mcpBuilder.WithTools<DatabaseOperationsTool>();
    }
    if (tools.EnableTableOperationsTool)
    {
      this._logger.LogInformation("Loading tool: {ToolName}", (object) "TableOperationsTool");
      mcpBuilder.WithTools<TableOperationsTool>();
    }
    if (tools.EnableColumnOperationsTool)
    {
      this._logger.LogInformation("Loading tool: {ToolName}", (object) "ColumnOperationsTool");
      mcpBuilder.WithTools<ColumnOperationsTool>();
    }
    if (tools.EnableMeasureOperationsTool)
    {
      this._logger.LogInformation("Loading tool: {ToolName}", (object) "MeasureOperationsTool");
      mcpBuilder.WithTools<MeasureOperationsTool>();
    }
    if (tools.EnableBatchMeasureOperationsTool)
    {
      this._logger.LogInformation("Loading tool: {ToolName}", (object) "BatchMeasureOperationsTool");
      mcpBuilder.WithTools<BatchMeasureOperationsTool>();
    }
    if (tools.EnableBatchColumnOperationsTool)
    {
      this._logger.LogInformation("Loading tool: {ToolName}", (object) "BatchColumnOperationsTool");
      mcpBuilder.WithTools<BatchColumnOperationsTool>();
    }
    if (tools.EnableBatchTableOperationsTool)
    {
      this._logger.LogInformation("Loading tool: {ToolName}", (object) "BatchTableOperationsTool");
      mcpBuilder.WithTools<BatchTableOperationsTool>();
    }
    if (tools.EnableNamedExpressionOperationsTool)
    {
      this._logger.LogInformation("Loading tool: {ToolName}", (object) "NamedExpressionOperationsTool");
      mcpBuilder.WithTools<NamedExpressionOperationsTool>();
    }
    if (tools.EnableFunctionOperationsTool)
    {
      this._logger.LogInformation("Loading tool: {ToolName}", (object) "FunctionOperationsTool");
      mcpBuilder.WithTools<FunctionOperationsTool>();
    }
    if (tools.EnableBatchFunctionOperationsTool)
    {
      this._logger.LogInformation("Loading tool: {ToolName}", (object) "BatchFunctionOperationsTool");
      mcpBuilder.WithTools<BatchFunctionOperationsTool>();
    }
    if (tools.EnableObjectTranslationOperationsTool)
    {
      this._logger.LogInformation("Loading tool: {ToolName}", (object) "ObjectTranslationOperationsTool");
      mcpBuilder.WithTools<ObjectTranslationOperationsTool>();
    }
    if (tools.EnableBatchObjectTranslationOperationsTool)
    {
      this._logger.LogInformation("Loading tool: {ToolName}", (object) "BatchObjectTranslationOperationsTool");
      mcpBuilder.WithTools<BatchObjectTranslationOperationsTool>();
    }
    if (tools.EnableCalculationGroupOperationsTool)
    {
      this._logger.LogInformation("Loading tool: {ToolName}", (object) "CalculationGroupOperationsTool");
      mcpBuilder.WithTools<CalculationGroupOperationsTool>();
    }
    if (tools.EnableCalendarOperationsTool)
    {
      this._logger.LogInformation("Loading tool: {ToolName}", (object) "CalendarOperationsTool");
      mcpBuilder.WithTools<CalendarOperationsTool>();
    }
    if (tools.EnableQueryGroupOperationsTool)
    {
      this._logger.LogInformation("Loading tool: {ToolName}", (object) "QueryGroupOperationsTool");
      mcpBuilder.WithTools<QueryGroupOperationsTool>();
    }
    if (tools.EnableRelationshipOperationsTool)
    {
      this._logger.LogInformation("Loading tool: {ToolName}", (object) "RelationshipOperationsTool");
      mcpBuilder.WithTools<RelationshipOperationsTool>();
    }
    if (tools.EnableDataSourceOperationsTool && this._config.Compatibility == CompatibilityMode.Full)
    {
      this._logger.LogInformation("Loading tool: {ToolName}", (object) "DataSourceOperationsTool");
      mcpBuilder.WithTools<DataSourceOperationsTool>();
    }
    else if (tools.EnableDataSourceOperationsTool && this._config.Compatibility == CompatibilityMode.PowerBI)
      this._logger.LogInformation("Skipping tool: {ToolName} (not supported in PowerBI compatibility mode)", (object) "DataSourceOperationsTool");
    if (tools.EnablePartitionOperationsTool)
    {
      this._logger.LogInformation("Loading tool: {ToolName}", (object) "PartitionOperationsTool");
      mcpBuilder.WithTools<PartitionOperationsTool>();
    }
    if (tools.EnableSecurityRoleOperationsTool)
    {
      this._logger.LogInformation("Loading tool: {ToolName}", (object) "SecurityRoleOperationsTool");
      mcpBuilder.WithTools<SecurityRoleOperationsTool>();
    }
    if (tools.EnableUserHierarchyOperationsTool)
    {
      this._logger.LogInformation("Loading tool: {ToolName}", (object) "UserHierarchyOperationsTool");
      mcpBuilder.WithTools<UserHierarchyOperationsTool>();
    }
    if (tools.EnableCultureOperationsTool)
    {
      this._logger.LogInformation("Loading tool: {ToolName}", (object) "CultureOperationsTool");
      mcpBuilder.WithTools<CultureOperationsTool>();
    }
    if (tools.EnableModelOperationsTool)
    {
      this._logger.LogInformation("Loading tool: {ToolName}", (object) "ModelOperationsTool");
      mcpBuilder.WithTools<ModelOperationsTool>();
    }
    if (tools.EnablePerspectiveOperationsTool)
    {
      this._logger.LogInformation("Loading tool: {ToolName}", (object) "PerspectiveOperationsTool");
      mcpBuilder.WithTools<PerspectiveOperationsTool>();
    }
    if (tools.EnableBatchPerspectiveOperationsTool)
    {
      this._logger.LogInformation("Loading tool: {ToolName}", (object) "BatchPerspectiveOperationsTool");
      mcpBuilder.WithTools<BatchPerspectiveOperationsTool>();
    }
    if (tools.EnableConnectionOperationsTool)
    {
      this._logger.LogInformation("Loading tool: {ToolName}", (object) "ConnectionOperationsTool");
      mcpBuilder.WithTools<ConnectionOperationsTool>();
    }
    if (tools.EnableDaxQueryOperationsTool)
    {
      this._logger.LogInformation("Loading tool: {ToolName}", (object) "DaxQueryOperationsTool");
      mcpBuilder.WithTools<DaxQueryOperationsTool>();
    }
    if (tools.EnableTransactionOperationsTool)
    {
      this._logger.LogInformation("Loading tool: {ToolName}", (object) "TransactionOperationsTool");
      mcpBuilder.WithTools<TransactionOperationsTool>();
    }
    if (tools.EnableTraceOperationsTool)
    {
      this._logger.LogInformation("Loading tool: {ToolName}", (object) "TraceOperationsTool");
      mcpBuilder.WithTools<TraceOperationsTool>();
    }
    this._logger.LogInformation("=== Tool Registration Completed ===");
  }
}
