// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
#nullable enable
namespace PowerBIModelingMCP.Library.Services;

public class ParsedResourceDefinition
{
  public string Name { get; set; } = string.Empty;

  public string Description { get; set; } = string.Empty;

  public string UriTemplate { get; set; } = string.Empty;

  public string MimeType { get; init; } = "text/plain";

  public string Text { get; set; } = string.Empty;
}
