// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System.Collections.Generic;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class PartitionOperationResult
{
  public string State { get; set; } = string.Empty;

  public string? ErrorMessage { get; set; }

  public string PartitionName { get; set; } = string.Empty;

  public string TableName { get; set; } = string.Empty;

  public bool HasChanges { get; set; }

  public List<string>? Warnings { get; set; }
}
