// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
#nullable disable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class AutomaticAggregationOptions
{
  public long? AggregationTableMaxRows { get; set; }

  public long? AggregationTableSizeLimit { get; set; }

  public long? DetailTableMinRows { get; set; }

  public double? QueryCoverage { get; set; }
}
