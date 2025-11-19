// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class TmdlDeployResult
{
  public bool Success { get; set; }

  public string SourceConnectionName { get; set; } = string.Empty;

  public string TargetConnectionName { get; set; } = string.Empty;

  public string SourceDatabaseName { get; set; } = string.Empty;

  public string TargetDatabaseName { get; set; } = string.Empty;

  public DateTime DeploymentTimestamp { get; set; }

  public string? Message { get; set; }
}
