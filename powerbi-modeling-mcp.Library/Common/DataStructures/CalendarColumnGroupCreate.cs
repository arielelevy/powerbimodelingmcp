// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class CalendarColumnGroupCreate
{
  [Required]
  [Description("Type of column group: TimeRelated or TimeUnitAssociation")]
  public required string GroupType { get; set; }

  [Description("Time-related column group information")]
  public TimeRelatedColumnGroupInfo? TimeRelatedGroup { get; set; }

  [Description("Time unit association information")]
  public TimeUnitColumnAssociationInfo? TimeUnitAssociation { get; set; }
}
