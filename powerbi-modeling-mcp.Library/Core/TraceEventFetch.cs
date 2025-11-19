// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
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
