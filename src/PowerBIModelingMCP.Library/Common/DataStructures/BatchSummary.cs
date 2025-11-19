// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.DataStructures.BatchSummary
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

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
