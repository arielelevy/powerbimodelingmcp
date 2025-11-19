// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System.Collections.Generic;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class AlternateOfDefinition
{
  public string? BaseTable { get; set; }

  public string? BaseColumn { get; set; }

  public string? Summarization { get; set; }

  public List<KeyValuePair<string, string>> Annotations { get; set; } = new List<KeyValuePair<string, string>>();
}
