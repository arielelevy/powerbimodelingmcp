// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.DataStructures.CalendarColumnGroupBase
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

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
