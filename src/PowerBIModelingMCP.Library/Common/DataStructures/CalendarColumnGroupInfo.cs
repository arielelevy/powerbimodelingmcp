// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.DataStructures.CalendarColumnGroupInfo
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class CalendarColumnGroupInfo
{
  [Required]
  [Description("Type of column group: TimeRelated or TimeUnitAssociation")]
  public required string GroupType { get; set; }

  [Description("Time-related column group information")]
  public TimeRelatedColumnGroupInfo? TimeRelatedGroup { get; set; }

  [Description("Time unit association information")]
  public TimeUnitColumnAssociationInfo? TimeUnitAssociation { get; set; }
}
