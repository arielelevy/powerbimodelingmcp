// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System.Collections.Generic;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class CalendarGet : CalendarBase
{
  [System.ComponentModel.Description("Collection of column groups in the calendar")]
  public List<CalendarColumnGroupInfo> CalendarColumnGroups { get; set; } = new List<CalendarColumnGroupInfo>();
}
