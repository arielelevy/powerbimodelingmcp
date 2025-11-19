// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class BatchGetPerspectiveHierarchiesRequest
{
  [Description("Connection name (optional, uses last used connection if not provided)")]
  public string? ConnectionName { get; set; }

  [Required]
  [Description("Name of the perspective to retrieve hierarchies from")]
  public required string PerspectiveName { get; set; }

  [Required]
  [Description("List of perspective hierarchy identifiers to retrieve (structured with TableName and HierarchyName)")]
  public required List<PerspectiveHierarchyBase> Items { get; set; } = new List<PerspectiveHierarchyBase>();
}
