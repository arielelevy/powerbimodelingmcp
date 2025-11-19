// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Contracts.ToolsConfiguration
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

#nullable disable
namespace PowerBIModelingMCP.Library.Contracts;

public class ToolsConfiguration
{
  public bool EnableDatabaseOperationsTool { get; set; } = true;

  public bool EnableTableOperationsTool { get; set; } = true;

  public bool EnableColumnOperationsTool { get; set; } = true;

  public bool EnableMeasureOperationsTool { get; set; } = true;

  public bool EnableBatchMeasureOperationsTool { get; set; } = true;

  public bool EnableBatchColumnOperationsTool { get; set; } = true;

  public bool EnableBatchTableOperationsTool { get; set; } = true;

  public bool EnableCalculationGroupOperationsTool { get; set; } = true;

  public bool EnableCalendarOperationsTool { get; set; } = true;

  public bool EnableQueryGroupOperationsTool { get; set; } = true;

  public bool EnableRelationshipOperationsTool { get; set; } = true;

  public bool EnableDataSourceOperationsTool { get; set; } = true;

  public bool EnablePartitionOperationsTool { get; set; } = true;

  public bool EnableSecurityRoleOperationsTool { get; set; } = true;

  public bool EnableUserHierarchyOperationsTool { get; set; } = true;

  public bool EnableCultureOperationsTool { get; set; } = true;

  public bool EnableModelOperationsTool { get; set; } = true;

  public bool EnableNamedExpressionOperationsTool { get; set; } = true;

  public bool EnableFunctionOperationsTool { get; set; } = true;

  public bool EnableBatchFunctionOperationsTool { get; set; } = true;

  public bool EnableObjectTranslationOperationsTool { get; set; } = true;

  public bool EnableBatchObjectTranslationOperationsTool { get; set; } = true;

  public bool EnablePerspectiveOperationsTool { get; set; } = true;

  public bool EnableBatchPerspectiveOperationsTool { get; set; } = true;

  public bool EnableConnectionOperationsTool { get; set; } = true;

  public bool EnableDaxQueryOperationsTool { get; set; } = true;

  public bool EnableTransactionOperationsTool { get; set; } = true;

  public bool EnableTraceOperationsTool { get; set; } = true;

  public bool EnableFabricOperationsTool { get; set; }
}
