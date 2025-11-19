// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.DataStructures.CultureBase
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

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
