// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.DataStructures.BatchOperationResponse
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

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
