// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System.Collections.Generic;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class HierarchyCreate : HierarchyBase
{
  public List<LevelCreate> Levels { get; set; } = new List<LevelCreate>();
}
