// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class ObjectTranslationCreate : ObjectTranslationBase
{
  public string? Value { get; set; }

  public bool CreateCultureIfNotExists { get; set; } = true;
}
