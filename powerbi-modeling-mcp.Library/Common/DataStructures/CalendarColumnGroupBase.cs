// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class CalendarColumnGroupBase
{
  [Required]
  [Description("The name of the calendar this column group belongs to")]
  public required string CalendarName { get; set; }

  [Required]
  [Description("Type of column group: TimeRelated or TimeUnitAssociation")]
  public required string GroupType { get; set; }

  [Description("The time that the object was last modified")]
  public DateTime? ModifiedTime { get; set; }
}
