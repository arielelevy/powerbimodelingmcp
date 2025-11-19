// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class ColumnList : ObjectListBase
{
  [System.ComponentModel.Description("Data type of the column (String, Integer, DateTime, etc.)")]
  public string? DataType { get; set; }

  [System.ComponentModel.Description("Whether the column is calculated (has an expression)")]
  public bool? IsCalculated { get; set; }

  [System.ComponentModel.Description("Display folder for organizing columns")]
  public string? DisplayFolder { get; set; }
}
