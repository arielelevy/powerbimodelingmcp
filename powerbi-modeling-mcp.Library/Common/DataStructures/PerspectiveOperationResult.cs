// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class PerspectiveOperationResult
{
  public bool Success { get; set; }

  public string PerspectiveName { get; set; } = string.Empty;

  public string? Message { get; set; }

  public bool HasChanges { get; set; }
}
