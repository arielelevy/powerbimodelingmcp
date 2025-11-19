// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.DataStructures.TmdlFileInfo
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using System;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class TmdlFileInfo
{
  public string FileName { get; set; } = string.Empty;

  public string RelativePath { get; set; } = string.Empty;

  public string ObjectType { get; set; } = string.Empty;

  public long FileSize { get; set; }

  public DateTime LastModified { get; set; }
}
