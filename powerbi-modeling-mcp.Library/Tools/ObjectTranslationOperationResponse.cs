// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Tools.ObjectTranslationOperationResponse
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using System.Collections.Generic;
using System.ComponentModel;

#nullable enable
namespace PowerBIModelingMCP.Library.Tools;

public class ObjectTranslationOperationResponse
{
  public static ObjectTranslationOperationResponse Forbidden(string op, string msg)
  {
    return new ObjectTranslationOperationResponse()
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

  [Description("The culture name that was operated on")]
  public string? CultureName { get; set; }

  [Description("The object type that was operated on")]
  public string? ObjectType { get; set; }

  [Description("The object display name that was operated on")]
  public string? ObjectDisplayName { get; set; }

  [Description("The property that was operated on")]
  public string? Property { get; set; }

  [Description("The translation value (for Get, Create, Update operations)")]
  public string? Value { get; set; }

  [Description("Result data for Get and List operations")]
  public object? Data { get; set; }

  [Description("Help information for the operation")]
  public object? Help { get; set; }

  [Description("Any warnings encountered during the operation")]
  public List<string>? Warnings { get; set; }
}
