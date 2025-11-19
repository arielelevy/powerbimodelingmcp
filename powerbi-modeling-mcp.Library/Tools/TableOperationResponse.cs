// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System.Collections.Generic;
using System.ComponentModel;

#nullable enable
namespace PowerBIModelingMCP.Library.Tools;

public class TableOperationResponse
{
  public static TableOperationResponse Forbidden(string op, string msg)
  {
    return new TableOperationResponse()
    {
      Success = false,
      Message = msg,
      Operation = op
    };
  }

  [Description("Indicates whether the operation was successful")]
  public bool Success { get; set; }

  [Description("Descriptive message about the operation result")]
  public string Message { get; set; } = string.Empty;

  [Description("The operation that was performed")]
  public string Operation { get; set; } = string.Empty;

  [Description("The name of the table that was operated on")]
  public string? TableName { get; set; }

  [Description("Result data for Get and List operations")]
  public object? Data { get; set; }

  [Description("Help information for the operation")]
  public object? Help { get; set; }

  [Description("Any warnings encountered during the operation")]
  public List<string>? Warnings { get; set; }
}
