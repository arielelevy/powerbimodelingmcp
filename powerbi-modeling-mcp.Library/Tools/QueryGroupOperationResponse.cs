// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using PowerBIModelingMCP.Library.Common.DataStructures;
using System.Collections.Generic;
using System.ComponentModel;

#nullable enable
namespace PowerBIModelingMCP.Library.Tools;

public class QueryGroupOperationResponse
{
  public static QueryGroupOperationResponse Forbidden(string op, string msg)
  {
    return new QueryGroupOperationResponse()
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

  [Description("The name of the query group that was operated on")]
  public string? QueryGroupName { get; set; }

  [Description("Result data for Get and List operations")]
  public object? Data { get; set; }

  [Description("Operation result for operations that return operation results")]
  public QueryGroupOperationResult? OperationResult { get; set; }

  [Description("Help information for the operation")]
  public object? Help { get; set; }

  [Description("Any warnings encountered during the operation")]
  public List<string>? Warnings { get; set; }
}
