// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class ObjectTranslationGet : ObjectTranslationBase
{
  public string? Value { get; set; }

  public DateTime? ModifiedTime { get; set; }
}
