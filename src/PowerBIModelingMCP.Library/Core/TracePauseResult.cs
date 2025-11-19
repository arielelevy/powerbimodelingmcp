// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Core.TracePauseResult
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using System;
using System.Collections.Generic;

#nullable enable
namespace PowerBIModelingMCP.Library.Core;

public class TracePauseResult
{
  public string TraceName { get; set; } = string.Empty;

  public string Status { get; set; } = string.Empty;

  public DateTime StartTime { get; set; }

  public double Duration { get; set; }

  public int EventsCaptured { get; set; }

  public int EventsDiscarded { get; set; }

  public List<string> SubscribedEvents { get; set; } = new List<string>();

  public List<string>? Warnings { get; set; }

  public bool FilterCurrentSessionOnly { get; set; }
}
