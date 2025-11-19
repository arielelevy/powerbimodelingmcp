// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using PowerBIModelingMCP.Library.Core;
using System.Collections.Generic;
using System.ComponentModel;

#nullable enable
namespace PowerBIModelingMCP.Library.Tools;

public class DataSourceOperationResponse
{
  [Description("Indicates whether the operation was successful")]
  public bool Success { get; set; }

  [Description("Descriptive message about the operation result")]
  public string Message { get; set; } = string.Empty;

  [Description("The operation that was performed")]
  public string Operation { get; set; } = string.Empty;

  [Description("The name of the SSAS data source that was operated on")]
  public string? DataSourceName { get; set; }

  [Description("Result data for Get and List operations")]
  public object? Data { get; set; }

  [Description("Operation result for operations that return OperationResult")]
  public OperationResult? OperationResult { get; set; }

  [Description("Help information for the operation")]
  public object? Help { get; set; }

  [Description("Any warnings encountered during the operation")]
  public List<string>? Warnings { get; set; }

  public static DataSourceOperationResponse Forbidden(string op, string msg, string? dataSourceName = null)
  {
    return new DataSourceOperationResponse()
    {
      Success = false,
      Message = msg,
      Operation = op,
      DataSourceName = dataSourceName
    };
  }
}
