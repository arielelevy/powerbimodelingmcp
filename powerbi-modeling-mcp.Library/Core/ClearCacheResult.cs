// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
#nullable enable
namespace PowerBIModelingMCP.Library.Core;

public class ClearCacheResult
{
  public bool Success { get; set; }

  public string? ErrorMessage { get; set; }

  public string DatabaseName { get; set; } = string.Empty;

  public string ConnectionName { get; set; } = string.Empty;

  public int RowsAffected { get; set; }
}
