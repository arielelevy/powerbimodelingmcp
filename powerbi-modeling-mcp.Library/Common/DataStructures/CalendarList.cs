// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System.Collections.Generic;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class CalendarList : ObjectListBase
{
  [System.ComponentModel.Description("Table containing this calendar")]
  public string? TableName { get; set; }

  [System.ComponentModel.Description("List of column groups in the calendar")]
  public List<ColumnGroupList>? ColumnGroups { get; set; }
}
