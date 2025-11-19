// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System.Collections.Generic;

#nullable enable
namespace PowerBIModelingMCP.Library.Common;

public class ToolMetadata
{
  public Dictionary<string, OperationMetadata> Operations { get; set; } = new Dictionary<string, OperationMetadata>();
}
