// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.WriteOperationResult
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using System.Collections.Generic;

#nullable enable
namespace PowerBIModelingMCP.Library.Common;

public class WriteOperationResult
{
  public bool Success { get; set; }

  public string? Message { get; set; }

  public List<string>? Warnings { get; set; }

  public bool UserDeclinedConfirmation { get; set; }

  public bool UserDeclinedDiscardLocalChanges { get; set; }
}
