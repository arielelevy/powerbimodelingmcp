// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System.ComponentModel;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class CultureList
{
  [Description("Name of the culture")]
  public string? Name { get; set; }

  [Description("Locale Identifier (LCID) for the culture")]
  public int LCID { get; set; }

  [Description("Number of object translations in this culture")]
  public int TranslationCount { get; set; }
}
