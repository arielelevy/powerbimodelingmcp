// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System.ComponentModel;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class ObjectTranslationListFilters
{
  [Description("Optional culture name filter for List operation")]
  public string? FilterCultureName { get; set; }

  [Description("Optional object type filter for List operation")]
  public string? FilterObjectType { get; set; }

  [Description("Optional object name filter for List operation")]
  public string? FilterObjectName { get; set; }
}
