// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System.Collections.Generic;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class QueryGroupBase
{
  public string? Description { get; set; }

  public string? Folder { get; set; }

  public List<KeyValuePair<string, string>>? Annotations { get; set; }
}
