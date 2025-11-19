// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.DataStructures.TmdlSerializeResult
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using System;
using System.Collections.Generic;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class TmdlSerializeResult
{
  public bool Success { get; set; }

  public string FolderPath { get; set; } = string.Empty;

  public string DatabaseName { get; set; } = string.Empty;

  public List<string> FilesCreated { get; set; } = new List<string>();

  public int FileCount { get; set; }

  public DateTime SerializedAt { get; set; }

  public string? Message { get; set; }
}
