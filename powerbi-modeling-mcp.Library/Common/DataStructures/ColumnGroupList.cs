// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System.Collections.Generic;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class ColumnGroupList : ObjectListBase
{
  [System.ComponentModel.Description("Type of column group: TimeRelated or TimeUnitAssociation")]
  public string? GroupType { get; set; }

  [System.ComponentModel.Description("List of column names in the column group")]
  public List<string>? ColumnNames { get; set; }

  [System.ComponentModel.Description("Primary column name (for TimeUnitAssociation groups)")]
  public string? PrimaryColumnName { get; set; }
}
