// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class BatchGetPerspectiveMeasuresRequest
{
  [Description("Connection name (optional, uses last used connection if not provided)")]
  public string? ConnectionName { get; set; }

  [Required]
  [Description("Name of the perspective to retrieve measures from")]
  public required string PerspectiveName { get; set; }

  [Required]
  [Description("List of perspective measure identifiers to retrieve (structured with TableName and MeasureName)")]
  public required List<PerspectiveMeasureBase> Items { get; set; } = new List<PerspectiveMeasureBase>();
}
