// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.DataStructures.RelationshipList
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class RelationshipList : ObjectListBase
{
  [System.ComponentModel.Description("From table name")]
  public string? FromTable { get; set; }

  [System.ComponentModel.Description("From column name")]
  public string? FromColumn { get; set; }

  [System.ComponentModel.Description("To table name")]
  public string? ToTable { get; set; }

  [System.ComponentModel.Description("To column name")]
  public string? ToColumn { get; set; }

  [System.ComponentModel.Description("Whether the relationship is active")]
  public bool? IsActive { get; set; }

  [System.ComponentModel.Description("Cross-filtering behavior (OneDirection, BothDirections, etc.)")]
  public string? CrossFilteringBehavior { get; set; }

  [System.ComponentModel.Description("From cardinality (One, Many)")]
  public string? FromCardinality { get; set; }

  [System.ComponentModel.Description("To cardinality (One, Many)")]
  public string? ToCardinality { get; set; }
}
