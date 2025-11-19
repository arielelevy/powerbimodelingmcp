// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class QueryExecutionMetrics
{
  public CalculatedExecutionMetrics? CalculatedExecutionMetrics { get; set; }

  public ReportedExecutionMetrics? ReportedExecutionMetrics { get; set; }
}
