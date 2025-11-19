// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.DataStructures.TimeUnitColumnAssociationInfo
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

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
