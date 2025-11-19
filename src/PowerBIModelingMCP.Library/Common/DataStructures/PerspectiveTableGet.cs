// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.DataStructures.PerspectiveTableGet
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using System;
using System.Collections.Generic;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class PerspectiveTableGet : PerspectiveTableBase
{
  public DateTime? ModifiedTime { get; set; }

  public List<PerspectiveColumnGet> PerspectiveColumns { get; set; } = new List<PerspectiveColumnGet>();

  public List<PerspectiveMeasureGet> PerspectiveMeasures { get; set; } = new List<PerspectiveMeasureGet>();

  public List<PerspectiveHierarchyGet> PerspectiveHierarchies { get; set; } = new List<PerspectiveHierarchyGet>();
}
