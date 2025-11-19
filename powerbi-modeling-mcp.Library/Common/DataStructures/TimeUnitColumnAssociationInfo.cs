// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class TimeUnitColumnAssociationInfo : CalendarColumnGroupBase
{
  [Required]
  [Description("The TimeUnit used in the association (Years, Quarters, Months, Days, etc.)")]
  public required string TimeUnit { get; set; }

  [Description("Reference to the primary column in the association")]
  public string? PrimaryColumnName { get; set; }

  [Description("Collection of associated column names in the association")]
  public List<string> AssociatedColumns { get; set; } = new List<string>();
}
