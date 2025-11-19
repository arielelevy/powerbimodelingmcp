// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.DataStructures.TraceContext
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using Microsoft.AnalysisServices.Tabular;
using System;
using System.Collections.Generic;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class TraceContext
{
  public string TraceName { get; set; } = string.Empty;

  public DateTime StartTime { get; set; } = DateTime.UtcNow;

  public required Trace Trace { get; set; }

  public required Server Server { get; set; }

  public bool IsActive { get; set; }

  public bool IsPaused { get; set; }

  public List<string> SubscribedEvents { get; set; } = new List<string>();

  public List<CapturedTraceEvent> CapturedEvents { get; set; } = new List<CapturedTraceEvent>();

  public int TotalEventsCaptured { get; set; }

  public int TotalEventsDiscarded { get; set; }

  public bool FilterCurrentSessionOnly { get; set; } = true;
}
