// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.DataStructures.RelationshipBase
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using System.Collections.Generic;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class RelationshipBase
{
  public string? Name { get; set; }

  public bool? IsActive { get; set; }

  public string? Type { get; set; }

  public string? CrossFilteringBehavior { get; set; }

  public string? JoinOnDateBehavior { get; set; }

  public bool? RelyOnReferentialIntegrity { get; set; }

  public string? FromTable { get; set; }

  public string? FromColumn { get; set; }

  public string? FromCardinality { get; set; }

  public string? ToTable { get; set; }

  public string? ToColumn { get; set; }

  public string? ToCardinality { get; set; }

  public string? SecurityFilteringBehavior { get; set; }

  public List<KeyValuePair<string, string>>? Annotations { get; set; }

  public List<ExtendedProperty>? ExtendedProperties { get; set; }
}
