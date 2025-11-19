// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
#nullable disable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class TableList : ObjectListBase
{
  [System.ComponentModel.Description("Number of columns in the table")]
  public int? ColumnCount { get; set; }

  [System.ComponentModel.Description("Number of measures in the table")]
  public int? MeasureCount { get; set; }

  [System.ComponentModel.Description("Number of hierarchies in the table")]
  public int? HierarchyCount { get; set; }

  [System.ComponentModel.Description("Number of partitions in the table")]
  public int? PartitionCount { get; set; }

  [System.ComponentModel.Description("Number of calendars in the table")]
  public int? CalendarCount { get; set; }
}
