// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
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
