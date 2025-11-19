// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class ColumnGet : ColumnBase
{
  public string? ColumnType { get; set; }

  public string? State { get; set; }

  public string? ErrorMessage { get; set; }

  public VariationDefinition? Variation { get; set; }
}
