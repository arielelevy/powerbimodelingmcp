// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System;
using System.Collections.Generic;

#nullable enable
namespace PowerBIModelingMCP.Library.Core;

internal class TraceEventDefinition
{
  public string Name { get; set; } = string.Empty;

  public string Category { get; set; } = string.Empty;

  public string Description { get; set; } = string.Empty;

  public HashSet<string> Columns { get; set; } = new HashSet<string>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
}
