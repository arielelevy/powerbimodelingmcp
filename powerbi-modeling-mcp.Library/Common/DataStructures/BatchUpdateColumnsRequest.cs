// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class BatchUpdateColumnsRequest
{
  [Description("Connection name (optional, uses last used connection if not provided)")]
  public string? ConnectionName { get; set; }

  [Required]
  [Description("List of column updates")]
  public required List<ColumnUpdate> Items { get; set; } = new List<ColumnUpdate>();

  [Description("Batch operation options")]
  public BatchOptions Options { get; set; } = new BatchOptions();
}
