// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System.Collections.Generic;
using System.ComponentModel;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class TableColumnList
{
  [Description("The name of the table")]
  public required string TableName { get; set; }

  [Description("The list of columns in this table")]
  public required List<ColumnList> Columns { get; set; }
}
