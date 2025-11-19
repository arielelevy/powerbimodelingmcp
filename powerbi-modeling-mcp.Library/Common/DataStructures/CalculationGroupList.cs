// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System.Collections.Generic;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class CalculationGroupList : ObjectListBase
{
  [System.ComponentModel.Description("List of calculation items in the group")]
  public List<CalculationItemList>? CalculationItems { get; set; }
}
