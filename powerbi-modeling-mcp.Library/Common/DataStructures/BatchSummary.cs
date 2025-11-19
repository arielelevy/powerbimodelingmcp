// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System;
using System.ComponentModel;

#nullable disable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class BatchSummary
{
  [Description("Total number of items processed")]
  public int TotalItems { get; set; }

  [Description("Number of successful operations")]
  public int SuccessCount { get; set; }

  [Description("Number of failed operations")]
  public int FailureCount { get; set; }

  [Description("Total execution time for the batch operation")]
  public TimeSpan ExecutionTime { get; set; }
}
