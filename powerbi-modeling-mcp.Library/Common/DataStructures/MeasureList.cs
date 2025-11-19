// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class MeasureList : ObjectListBase
{
  [System.ComponentModel.Description("Display folder for organizing measures")]
  public string? DisplayFolder { get; set; }
}
