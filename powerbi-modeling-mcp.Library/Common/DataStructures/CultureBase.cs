// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System;
using System.Collections.Generic;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class CultureBase
{
  public required string Name { get; set; }

  public List<KeyValuePair<string, string>>? Annotations { get; set; }

  public List<ExtendedProperty>? ExtendedProperties { get; set; }

  public string? LinguisticMetadataReference { get; set; }

  public List<string> ObjectTranslationReferences { get; set; } = new List<string>();

  public DateTime? ModifiedTime { get; set; }

  public DateTime? StructureModifiedTime { get; set; }

  public bool? IsRemoved { get; set; }
}
