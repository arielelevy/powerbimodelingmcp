// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System.Collections.Generic;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class PerspectiveHierarchyBase
{
  public string? Name { get; set; }

  public string? HierarchyName { get; set; }

  public string? TableName { get; set; }

  public List<KeyValuePair<string, string>>? Annotations { get; set; }

  public List<KeyValuePair<string, string>>? ExtendedProperties { get; set; }
}
