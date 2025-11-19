// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.DataStructures.HierarchyBase
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using System.Collections.Generic;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class HierarchyBase
{
  public string? TableName { get; set; }

  public string? Name { get; set; }

  public string? Description { get; set; }

  public bool? IsHidden { get; set; }

  public string? DisplayFolder { get; set; }

  public string? HideMembers { get; set; }

  public string? LineageTag { get; set; }

  public string? SourceLineageTag { get; set; }

  public List<KeyValuePair<string, string>>? Annotations { get; set; }

  public List<ExtendedProperty>? ExtendedProperties { get; set; }
}
