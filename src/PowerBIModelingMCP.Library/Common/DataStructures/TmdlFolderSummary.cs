// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.DataStructures.TmdlFolderSummary
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using System;
using System.Collections.Generic;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class TmdlFolderSummary
{
  public string FolderPath { get; set; } = string.Empty;

  public List<TmdlFileInfo> Files { get; set; } = new List<TmdlFileInfo>();

  public int TotalFiles { get; set; }

  public long TotalSize { get; set; }

  public DateTime LastModified { get; set; }

  public bool IsValidTmdlFolder { get; set; }

  public List<string> ValidationErrors { get; set; } = new List<string>();
}
