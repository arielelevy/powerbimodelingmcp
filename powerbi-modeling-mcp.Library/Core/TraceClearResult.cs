// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
#nullable enable
namespace PowerBIModelingMCP.Library.Core;

public class TraceClearResult
{
  public string TraceName { get; set; } = string.Empty;

  public int EventsCleared { get; set; }

  public string Status { get; set; } = string.Empty;
}
