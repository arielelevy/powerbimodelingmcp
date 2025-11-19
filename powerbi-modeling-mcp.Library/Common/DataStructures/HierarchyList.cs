// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
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
