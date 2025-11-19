// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

#nullable enable
namespace PowerBIModelingMCP.Library.Tools;

public class DaxQueryOperationRequest
{
  [Required]
  [Description("The operation to perform: Help, Execute, Validate")]
  public required string Operation { get; set; }

  [Description("The connection name to use for the operation (optional - uses last used connection if not provided)")]
  public string? ConnectionName { get; set; }

  [Description("The DAX query to execute or validate")]
  public string? Query { get; set; }

  [Description("Optional timeout in seconds (default: 200 for execute, 10 for validate)")]
  public int? TimeoutSeconds { get; set; }

  [Description("Maximum number of rows to return (Execute operation only)")]
  public int? MaxRows { get; set; }

  [Description("Whether to capture and return query execution metrics (Execute operation only, default: false)")]
  public bool GetExecutionMetrics { get; set; }

  [Description("When true and GetExecutionMetrics is true, only return execution metrics without row data. All rows will still be read to reflect accurate query execution time. (Execute operation only, default: true)")]
  public bool ExecutionMetricsOnly { get; set; } = true;
}
