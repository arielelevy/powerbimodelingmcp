// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class BatchRemovePerspectiveColumnsRequest
{
  [Description("Connection name (optional, uses last used connection if not provided)")]
  public string? ConnectionName { get; set; }

  [Required]
  [Description("Name of the perspective to remove columns from")]
  public required string PerspectiveName { get; set; }

  [Required]
  [Description("List of perspective column identifiers to remove (structured with TableName and ColumnName)")]
  public required List<PerspectiveColumnBase> Items { get; set; } = new List<PerspectiveColumnBase>();

  [Description("Batch operation options")]
  public BatchOptions Options { get; set; } = new BatchOptions();
}
