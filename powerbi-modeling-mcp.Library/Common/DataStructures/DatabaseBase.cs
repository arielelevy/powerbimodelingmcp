// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System.Collections.Generic;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class DatabaseBase
{
  public string? Name { get; set; }

  public string? Description { get; set; }

  public int? CompatibilityLevel { get; set; }

  public string? Collation { get; set; }

  public int? Language { get; set; }

  public List<KeyValuePair<string, string>>? Annotations { get; set; }
}
