// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System.Collections.Generic;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class WorkspacesListResult
{
  public List<FabricWorkspaceGet> Workspaces { get; set; } = new List<FabricWorkspaceGet>();

  public int Count { get; set; }
}
