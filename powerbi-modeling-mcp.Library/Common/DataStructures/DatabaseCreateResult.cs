// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class DatabaseCreateResult
{
  public bool Success { get; set; }

  public string ConnectionName { get; set; } = string.Empty;

  public string DatabaseName { get; set; } = string.Empty;

  public string ModelName { get; set; } = string.Empty;

  public DateTime CreatedAt { get; set; }

  public string? Message { get; set; }
}
