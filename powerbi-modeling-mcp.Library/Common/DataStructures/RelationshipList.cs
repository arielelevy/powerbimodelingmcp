// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
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
