// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class CalendarColumnGroupRename : ObjectRenameBase
{
  [Required]
  [Description("The calendar containing the column group")]
  public required string CalendarName { get; set; }

  [Required]
  [Description("The type of column group being renamed")]
  public required string GroupType { get; set; }
}
