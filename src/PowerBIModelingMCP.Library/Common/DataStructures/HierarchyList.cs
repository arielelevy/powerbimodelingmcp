// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.DataStructures.HierarchyList
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using System.Collections.Generic;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class HierarchyList : ObjectListBase
{
  [System.ComponentModel.Description("List of levels in the hierarchy")]
  public List<LevelList> Levels { get; set; } = new List<LevelList>();

  [System.ComponentModel.Description("Display folder for organizing hierarchies")]
  public string? DisplayFolder { get; set; }
}
