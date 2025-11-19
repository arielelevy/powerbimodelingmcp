// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.DataStructures.HierarchyGet
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using Microsoft.AnalysisServices.Tabular;
using System;
using System.Collections.Generic;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class HierarchyGet : HierarchyBase
{
  public ObjectState? State { get; set; }

  public DateTime? ModifiedTime { get; set; }

  public DateTime? StructureModifiedTime { get; set; }

  public DateTime? RefreshedTime { get; set; }

  public List<LevelGet> Levels { get; set; } = new List<LevelGet>();
}
