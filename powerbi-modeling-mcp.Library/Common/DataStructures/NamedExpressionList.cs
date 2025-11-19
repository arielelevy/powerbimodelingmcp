// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class NamedExpressionList : ObjectListBase
{
  [System.ComponentModel.Description("Kind of expression (M for PowerQuery, DAX for calculations)")]
  public string? Kind { get; set; }

  [System.ComponentModel.Description("Name of the QueryGroup associated with the named expression (if any)")]
  public string? QueryGroupName { get; set; }
}
