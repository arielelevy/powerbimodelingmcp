// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.DataStructures.QueryExecutionStatistics
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using System;
using System.Collections.Generic;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class QueryExecutionStatistics
{
  public bool Success { get; set; }

  public string? ErrorMessage { get; set; }

  public long TotalDuration { get; set; }

  public long TotalCpuTime { get; set; }

  public DateTime? QueryStartDateTime { get; set; }

  public DateTime? QueryEndDateTime { get; set; }

  public long TotalVertipaqDuration { get; set; }

  public long TotalVertipaqCpuTime { get; set; }

  public int TotalVertipaqQueryCount { get; set; }

  public int TotalVertipaqCacheMatches { get; set; }

  public long TotalDirectQueryDuration { get; set; }

  public int TotalDirectQueryCount { get; set; }

  public string? ActivityId { get; set; }

  public string? QueryText { get; set; }

  public List<CapturedTraceEvent>? DetailedEvents { get; set; }
}
