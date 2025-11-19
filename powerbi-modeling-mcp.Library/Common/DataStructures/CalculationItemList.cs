// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
#nullable disable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class CalculationItemList : ObjectListBase
{
  [System.ComponentModel.Description("Ordinal position within the calculation group")]
  public int Ordinal { get; set; }
}
