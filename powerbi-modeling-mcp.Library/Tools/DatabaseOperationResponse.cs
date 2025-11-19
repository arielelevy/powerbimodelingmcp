// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Tools.DatabaseOperationResponse
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using System.Collections.Generic;
using System.ComponentModel;

#nullable enable
namespace PowerBIModelingMCP.Library.Tools;

public class DatabaseOperationResponse
{
  [Description("Indicates whether the operation was successful")]
  public bool Success { get; set; }

  [Description("Descriptive message about the operation result")]
  public string Message { get; set; } = string.Empty;

  [Description("The operation that was performed")]
  public string Operation { get; set; } = string.Empty;

  [Description("The name of the database that was operated on")]
  public string? DatabaseName { get; set; }

  [Description("Result data for List and Update operations")]
  public object? Data { get; set; }

  [Description("Help information for the operation")]
  public object? Help { get; set; }

  [Description("Any warnings encountered during the operation")]
  public List<string>? Warnings { get; set; }

  public static DatabaseOperationResponse Forbidden(string op, string msg, string? dbName = null)
  {
    return new DatabaseOperationResponse()
    {
      Success = false,
      Message = msg,
      Operation = op,
      DatabaseName = dbName
    };
  }
}
