// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.DataStructures.TableBase
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using System.Collections.Generic;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class TableBase
{
  public string? Name { get; set; }

  public string? DataCategory { get; set; }

  public string? Description { get; set; }

  public bool? IsHidden { get; set; }

  public bool? ShowAsVariationsOnly { get; set; }

  public bool? IsPrivate { get; set; }

  public int? AlternateSourcePrecedence { get; set; }

  public bool? ExcludeFromModelRefresh { get; set; }

  public string? LineageTag { get; set; }

  public string? SourceLineageTag { get; set; }

  public bool? SystemManaged { get; set; }

  public List<KeyValuePair<string, string>>? Annotations { get; set; }

  public List<ExtendedProperty>? ExtendedProperties { get; set; }
}
