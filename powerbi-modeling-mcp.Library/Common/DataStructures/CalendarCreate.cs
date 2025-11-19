// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System.Collections.Generic;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class CalendarCreate : CalendarBase
{
  [System.ComponentModel.Description("Initial column groups to create with the calendar")]
  public List<CalendarColumnGroupCreate>? CalendarColumnGroups { get; set; }
}
