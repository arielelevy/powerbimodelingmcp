// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Tools.CalendarOperationResponse
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using System.Collections.Generic;
using System.ComponentModel;

#nullable enable
namespace PowerBIModelingMCP.Library.Tools;

public class CalendarOperationResponse
{
  [Description("Indicates whether the operation was successful")]
  public bool Success { get; set; }

  [Description("Descriptive message about the operation result")]
  public string Message { get; set; } = string.Empty;

  [Description("The operation that was performed")]
  public string Operation { get; set; } = string.Empty;

  [Description("The name of the calendar that was operated on")]
  public string? CalendarName { get; set; }

  [Description("The name of the table containing the calendar")]
  public string? TableName { get; set; }

  [Description("The type of column group that was operated on (for column group operations)")]
  public string? ColumnGroupType { get; set; }

  [Description("Result data for Get and List operations")]
  public object? Data { get; set; }

  [Description("Operation result for operations that return operation results")]
  public object? OperationResult { get; set; }

  [Description("Help information for the operation")]
  public object? Help { get; set; }

  [Description("Any warnings encountered during the operation")]
  public List<string>? Warnings { get; set; }

  public static CalendarOperationResponse Forbidden(string op, string msg)
  {
    return new CalendarOperationResponse()
    {
      Success = false,
      Message = msg,
      Operation = op
    };
  }
}
