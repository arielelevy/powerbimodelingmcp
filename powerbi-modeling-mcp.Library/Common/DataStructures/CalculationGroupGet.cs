// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System;
using System.Collections.Generic;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class CalculationGroupGet : CalculationGroupBase
{
  public DateTime? ModifiedTime { get; set; }

  public DateTime? StructureModifiedTime { get; set; }

  public List<CalculationItemGet> CalculationItems { get; set; } = new List<CalculationItemGet>();
}
