// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System;

#nullable enable
namespace PowerBIModelingMCP.Library.Core;

public class LocalAnalysisServicesInstance
{
  public int ProcessId { get; set; }

  public int Port { get; set; }

  public string ConnectionString { get; set; } = string.Empty;

  public string? ParentProcessName { get; set; }

  public string? ParentWindowTitle { get; set; }

  public DateTime StartTime { get; set; }
}
