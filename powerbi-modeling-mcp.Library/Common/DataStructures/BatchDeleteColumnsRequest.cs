// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class BatchDeleteColumnsRequest
{
  [Description("Connection name (optional, uses last used connection if not provided)")]
  public string? ConnectionName { get; set; }

  [Required]
  [Description("List of column identifiers to delete")]
  public required List<ColumnIdentifier> Items { get; set; } = new List<ColumnIdentifier>();

  [Description("Indicates whether to cascade delete dependencies for each column (true by default)")]
  public bool ShouldCascadeDelete { get; set; } = true;

  [Description("Batch operation options")]
  public BatchOptions Options { get; set; } = new BatchOptions();
}
