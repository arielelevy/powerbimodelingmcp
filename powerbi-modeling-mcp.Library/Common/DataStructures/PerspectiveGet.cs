// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.DataStructures.PerspectiveGet
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using System;
using System.Collections.Generic;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class PerspectiveGet : PerspectiveBase
{
  public DateTime? ModifiedTime { get; set; }

  public List<PerspectiveTableGet> PerspectiveTables { get; set; } = new List<PerspectiveTableGet>();

  public List<string> Tables { get; set; } = new List<string>();

  public List<string> Columns { get; set; } = new List<string>();

  public List<string> Measures { get; set; } = new List<string>();

  public List<string> Hierarchies { get; set; } = new List<string>();
}
