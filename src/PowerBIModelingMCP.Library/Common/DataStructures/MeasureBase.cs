// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.DataStructures.MeasureBase
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using System;
using System.Collections.Generic;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class MeasureBase
{
  public string? TableName { get; set; }

  public required string Name { get; set; }

  public string? Expression { get; set; }

  public string? Description { get; set; }

  public string? FormatString { get; set; }

  public bool? IsHidden { get; set; }

  public bool? IsSimpleMeasure { get; set; }

  public string? DisplayFolder { get; set; }

  public string? DataType { get; set; }

  public string? DataCategory { get; set; }

  public string? LineageTag { get; set; }

  public string? SourceLineageTag { get; set; }

  public string? KPI { get; set; }

  public string? DetailRowsExpression { get; set; }

  public string? FormatStringExpression { get; set; }

  [System.ComponentModel.Description("Collection of annotations as key-value pairs")]
  public List<KeyValuePair<string, string>>? Annotations { get; set; }

  [System.ComponentModel.Description("Collection of extended properties for storing custom metadata")]
  public List<ExtendedProperty>? ExtendedProperties { get; set; }

  public string? State { get; set; }

  public string? ErrorMessage { get; set; }

  public DateTime? ModifiedTime { get; set; }

  public DateTime? StructureModifiedTime { get; set; }
}
