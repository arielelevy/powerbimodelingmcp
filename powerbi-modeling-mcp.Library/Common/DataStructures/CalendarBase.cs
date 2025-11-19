// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System;
using System.ComponentModel.DataAnnotations;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class CalendarBase
{
  [Required]
  [System.ComponentModel.Description("The name of the calendar")]
  public required string Name { get; set; }

  [System.ComponentModel.Description("The description of the calendar, visible to developers at design time and to administrators in management tools")]
  public string? Description { get; set; }

  [Required]
  [System.ComponentModel.Description("Reference to the table that owns this calendar")]
  public required string TableName { get; set; }

  [System.ComponentModel.Description("A tag that represents the lineage of the object")]
  public string? LineageTag { get; set; }

  [System.ComponentModel.Description("A tag that represents the lineage of the source for the object")]
  public string? SourceLineageTag { get; set; }

  [System.ComponentModel.Description("The time that the object was last modified")]
  public DateTime? ModifiedTime { get; set; }
}
