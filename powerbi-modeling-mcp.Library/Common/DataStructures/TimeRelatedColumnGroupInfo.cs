// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System.Collections.Generic;
using System.ComponentModel;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class TimeRelatedColumnGroupInfo : CalendarColumnGroupBase
{
  [Description("Collection of column names in the group")]
  public List<string> Columns { get; set; } = new List<string>();
}
