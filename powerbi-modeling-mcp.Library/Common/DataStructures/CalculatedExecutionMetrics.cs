// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.DataStructures.CalculatedExecutionMetrics
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using System;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class CalculatedExecutionMetrics
{
  public bool Success { get; set; }

  public string? ErrorMessage { get; set; }

  public long TotalDuration { get; set; }

  public long TotalCpuTime { get; set; }

  public DateTime? QueryStartDateTime { get; set; }

  public DateTime? QueryEndDateTime { get; set; }

  public long StorageEngineDuration { get; set; }

  public long StorageEngineNetParallelDuration { get; set; }

  public long StorageEngineCpuTime { get; set; }

  public int StorageEngineQueryCount { get; set; }

  public int VertipaqCacheMatches { get; set; }

  public long FormulaEngineDuration { get; set; }

  public long TotalDirectQueryDuration { get; set; }

  public int TotalDirectQueryCount { get; set; }

  public string? ActivityId { get; set; }

  public string? QueryText { get; set; }

  public bool ParallelStorageEngineEventsDetected { get; set; }

  public double TotalCpuFactor { get; set; }

  public double StorageEngineCpuFactor { get; set; }

  public double StorageEngineDurationPercentage { get; set; }

  public double FormulaEngineDurationPercentage { get; set; }

  public double VertipaqCacheMatchesPercentage { get; set; }
}
