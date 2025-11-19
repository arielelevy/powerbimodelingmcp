// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.DataStructures.ColumnBase
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using System.Collections.Generic;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class ColumnBase
{
  public string? TableName { get; set; }

  public string? Name { get; set; }

  public string? SourceColumn { get; set; }

  public string? Expression { get; set; }

  public string? DataType { get; set; }

  public string? DataCategory { get; set; }

  public string? FormatString { get; set; }

  public string? SummarizeBy { get; set; }

  public bool? DefaultLabel { get; set; }

  public bool? DefaultImage { get; set; }

  public bool? IsHidden { get; set; }

  public bool? IsUnique { get; set; }

  public bool? IsKey { get; set; }

  public bool? IsNullable { get; set; }

  public string? DisplayFolder { get; set; }

  public string? SortByColumn { get; set; }

  public string? SourceProviderType { get; set; }

  public string? Description { get; set; }

  public bool? IsAvailableInMDX { get; set; }

  public string? Alignment { get; set; }

  public int? TableDetailPosition { get; set; }

  public List<KeyValuePair<string, string>>? Annotations { get; set; }

  public List<ExtendedProperty>? ExtendedProperties { get; set; }

  public AlternateOfDefinition? AlternateOf { get; set; }

  public List<string>? GroupByColumns { get; set; }
}
