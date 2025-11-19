// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.DataStructures.VariationDefinition
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using System.Collections.Generic;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class VariationDefinition
{
  public string RelationshipName { get; set; } = string.Empty;

  public string HiddenTableName { get; set; } = string.Empty;

  public string HierarchyName { get; set; } = string.Empty;

  public List<string> HierarchyLevelNames { get; set; } = new List<string>();
}
