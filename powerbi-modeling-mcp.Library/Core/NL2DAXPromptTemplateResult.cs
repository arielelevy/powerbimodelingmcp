// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
#nullable enable
namespace PowerBIModelingMCP.Library.Core;

public class NL2DAXPromptTemplateResult
{
  public bool Success { get; set; }

  public string? ErrorMessage { get; set; }

  public string? TemplateContent { get; set; }
}
