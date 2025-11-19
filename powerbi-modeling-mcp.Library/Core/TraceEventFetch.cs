// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Core.TraceEventFetch
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using PowerBIModelingMCP.Library.Common.DataStructures;
using System.Collections.Generic;

#nullable enable
namespace PowerBIModelingMCP.Library.Core;

public class TraceEventFetch
{
  public string TraceName { get; set; } = string.Empty;

  public int EventCount { get; set; }

  public bool Cleared { get; set; }

  public List<CapturedTraceEvent> Events { get; set; } = new List<CapturedTraceEvent>();
}
