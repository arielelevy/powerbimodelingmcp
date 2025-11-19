// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class MeasureMove
{
  [Required]
  [Description("Name of the measure to move")]
  public required string Name { get; set; }

  [Description("Current table name (optional if measure name is unique)")]
  public string? CurrentTableName { get; set; }

  [Required]
  [Description("Destination table name")]
  public required string DestinationTableName { get; set; }
}
