// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System.Collections.Generic;
using System.ComponentModel;

#nullable enable
namespace PowerBIModelingMCP.Library.Tools;

public class ColumnOperationResponse
{
  [Description("Indicates whether the operation was successful")]
  public bool Success { get; set; }

  [Description("Descriptive message about the operation result")]
  public string Message { get; set; } = string.Empty;

  [Description("The operation that was performed")]
  public string Operation { get; set; } = string.Empty;

  [Description("The name of the table containing the column")]
  public string? TableName { get; set; }

  [Description("The name of the column that was operated on")]
  public string? ColumnName { get; set; }

  [Description("Result data for Get and List operations")]
  public object? Data { get; set; }

  [Description("Help information for the operation")]
  public object? Help { get; set; }

  [Description("Any warnings encountered during the operation")]
  public List<string>? Warnings { get; set; }

  public static ColumnOperationResponse Forbidden(
    string op,
    string msg,
    string? tableName = null,
    string? columnName = null)
  {
    return new ColumnOperationResponse()
    {
      Success = false,
      Message = msg,
      Operation = op,
      TableName = tableName,
      ColumnName = columnName
    };
  }
}
