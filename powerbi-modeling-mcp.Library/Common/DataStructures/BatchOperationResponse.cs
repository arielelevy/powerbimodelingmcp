// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System.Collections.Generic;
using System.ComponentModel;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class BatchOperationResponse
{
  [Description("Indicates whether the overall batch operation was successful")]
  public bool Success { get; set; }

  [Description("List of results for each item in the batch")]
  public List<ItemResult> Results { get; set; } = new List<ItemResult>();

  [Description("Overall message about the batch operation")]
  public string Message { get; set; } = string.Empty;

  [Description("Summary statistics for the batch operation")]
  public BatchSummary Summary { get; set; } = new BatchSummary();

  [Description("The batch operation that was performed")]
  public string Operation { get; set; } = string.Empty;

  [Description("Any warnings encountered during the operation")]
  public List<string>? Warnings { get; set; }
}
