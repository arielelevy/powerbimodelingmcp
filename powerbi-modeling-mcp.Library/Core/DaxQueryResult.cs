// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System.Collections.Generic;

#nullable enable
namespace PowerBIModelingMCP.Library.Core;

public class DaxQueryResult
{
  public bool Success { get; set; }

  public string? ErrorMessage { get; set; }

  public int RowCount { get; set; }

  public List<DaxColumnInfo> Columns { get; set; } = new List<DaxColumnInfo>();

  public List<Dictionary<string, object?>> Rows { get; set; } = new List<Dictionary<string, object>>();

  public long ExecutionTimeMs { get; set; }
}
