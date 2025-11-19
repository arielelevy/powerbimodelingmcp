// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class BatchDeleteTablesRequest
{
  [Description("Connection name (optional, uses last used connection if not provided)")]
  public string? ConnectionName { get; set; }

  [Required]
  [Description("List of table names to delete")]
  public required List<string> Items { get; set; } = new List<string>();

  [Description("Indicates whether to cascade delete dependencies for each table (true by default)")]
  public bool ShouldCascadeDelete { get; set; } = true;

  [Description("Batch operation options")]
  public BatchOptions Options { get; set; } = new BatchOptions();
}
