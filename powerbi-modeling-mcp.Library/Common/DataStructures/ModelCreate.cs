// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class ModelCreate : ModelBase
{
  public string? ModelName { get; set; }

  public bool? IsOffline { get; set; }
}
