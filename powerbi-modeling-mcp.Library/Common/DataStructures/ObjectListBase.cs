// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class ObjectListBase
{
  [System.ComponentModel.Description("Name of the object")]
  public string? Name { get; set; }

  [System.ComponentModel.Description("Description of the object")]
  public string? Description { get; set; }
}
