// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System.Collections.Generic;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class SemanticModelsListResult
{
  public List<FabricSemanticModelGet> SemanticModels { get; set; } = new List<FabricSemanticModelGet>();

  public int Count { get; set; }
}
