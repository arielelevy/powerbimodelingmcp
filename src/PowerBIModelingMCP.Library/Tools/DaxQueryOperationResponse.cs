// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Tools.DaxQueryOperationResponse
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using PowerBIModelingMCP.Library.Common.DataStructures;
using System.Collections.Generic;
using System.ComponentModel;

#nullable enable
namespace PowerBIModelingMCP.Library.Tools;

public class DaxQueryOperationResponse
{
  [Description("Indicates whether the operation was successful")]
  public bool Success { get; set; }

  [Description("Descriptive message about the operation result")]
  public string Message { get; set; } = string.Empty;

  [Description("The operation that was performed")]
  public string Operation { get; set; } = string.Empty;

  [Description("Help information for the operation")]
  public object? Help { get; set; }

  [Description("Any warnings encountered during the operation")]
  public List<string>? Warnings { get; set; }

  [Description("Query execution metrics (only populated when GetExecutionMetrics is true for Execute operation)")]
  public QueryExecutionMetrics? ExecutionMetrics { get; set; }

  [Description("Result data for the operation")]
  public object? Data { get; set; }
}
