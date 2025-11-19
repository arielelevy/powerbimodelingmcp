// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System;
using System.Collections.Generic;

#nullable enable
namespace PowerBIModelingMCP.Library.Common;

public class OperationMetadata
{
  public string[] RequiredParams { get; set; } = Array.Empty<string>();

  public string[] OptionalParams { get; set; } = Array.Empty<string>();

  public string[] ForbiddenParams { get; set; } = Array.Empty<string>();

  public string Description { get; set; } = "";

  public string[] CommonMistakes { get; set; } = Array.Empty<string>();

  public string[] Tips { get; set; } = Array.Empty<string>();

  public List<string>? ExampleRequests { get; set; }
}
