// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
#nullable enable
namespace PowerBIModelingMCP.Library.Core;

public class TraceEventJSONExport
{
  public string TraceName { get; set; } = string.Empty;

  public int EventCount { get; set; }

  public bool Cleared { get; set; }

  public string FilePath { get; set; } = string.Empty;
}
