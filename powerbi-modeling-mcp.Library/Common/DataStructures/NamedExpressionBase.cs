// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.DataStructures.NamedExpressionBase
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class NamedExpressionBase
{
  [Required]
  [System.ComponentModel.Description("The name of the named expression")]
  public required string Name { get; set; }

  [System.ComponentModel.Description("The expression content")]
  public string? Expression { get; set; }

  [System.ComponentModel.Description("The kind/type of expression (M for PowerQuery, DAX for calculations)")]
  public string? Kind { get; set; }

  [System.ComponentModel.Description("The description of the named expression")]
  public string? Description { get; set; }

  [System.ComponentModel.Description("A tag that represents the lineage of the object")]
  public string? LineageTag { get; set; }

  [System.ComponentModel.Description("A tag that represents the lineage of the source for the object")]
  public string? SourceLineageTag { get; set; }

  [System.ComponentModel.Description("Name of the QueryGroup to associate with the named expression (optional)")]
  public string? QueryGroupName { get; set; }

  [System.ComponentModel.Description("Collection of annotations as key-value pairs")]
  public List<KeyValuePair<string, string>>? Annotations { get; set; }

  [System.ComponentModel.Description("Collection of extended properties for storing custom metadata")]
  public List<ExtendedProperty>? ExtendedProperties { get; set; }

  public string? State { get; set; }

  public string? ErrorMessage { get; set; }

  public DateTime? ModifiedTime { get; set; }
}
