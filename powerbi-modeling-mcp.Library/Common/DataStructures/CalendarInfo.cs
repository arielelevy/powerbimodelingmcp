// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class CalendarInfo
{
  [Required]
  [Description("The name of the calendar")]
  public required string CalendarName { get; set; }

  [Required]
  [Description("The name of the table that contains the calendar")]
  public required string TableName { get; set; }
}
