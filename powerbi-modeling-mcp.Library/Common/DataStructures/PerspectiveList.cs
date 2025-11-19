// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
#nullable disable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class PerspectiveList : ObjectListBase
{
  [System.ComponentModel.Description("Number of tables in the perspective")]
  public int? TableCount { get; set; }

  [System.ComponentModel.Description("Number of measures across all tables in the perspective")]
  public int? MeasureCount { get; set; }

  [System.ComponentModel.Description("Number of columns across all tables in the perspective")]
  public int? ColumnCount { get; set; }

  [System.ComponentModel.Description("Number of hierarchies across all tables in the perspective")]
  public int? HierarchyCount { get; set; }
}
