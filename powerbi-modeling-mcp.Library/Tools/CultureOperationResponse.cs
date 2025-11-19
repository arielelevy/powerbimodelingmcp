// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System.Collections.Generic;
using System.ComponentModel;

#nullable enable
namespace PowerBIModelingMCP.Library.Tools;

public class CultureOperationResponse
{
  [Description("Indicates whether the operation was successful")]
  public bool Success { get; set; }

  [Description("Descriptive message about the operation result")]
  public string Message { get; set; } = string.Empty;

  [Description("The operation that was performed")]
  public string Operation { get; set; } = string.Empty;

  [Description("The name of the culture that was operated on")]
  public string? CultureName { get; set; }

  [Description("Result data for Get, List, and GetValidNames operations")]
  public object? Data { get; set; }

  [Description("Help information for the operation")]
  public object? Help { get; set; }

  [Description("Any warnings encountered during the operation")]
  public List<string>? Warnings { get; set; }

  public static CultureOperationResponse Forbidden(string op, string msg, string? cultureName = null)
  {
    return new CultureOperationResponse()
    {
      Success = false,
      Message = msg,
      Operation = op,
      CultureName = cultureName
    };
  }
}
