// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.DataStructures.MeasureMove
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

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
