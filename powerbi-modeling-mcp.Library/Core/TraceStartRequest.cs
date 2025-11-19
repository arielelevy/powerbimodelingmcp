// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System.Collections.Generic;

#nullable enable
namespace PowerBIModelingMCP.Library.Core;

public class TraceStartRequest
{
  public List<string>? Events { get; set; }

  public bool FilterCurrentSessionOnly { get; set; } = true;
}
