// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.DataStructures.ObjectTranslationList
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using System.Collections.Generic;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class ObjectTranslationList
{
  public required string CultureName { get; set; }

  public required string ObjectType { get; set; }

  public required string Property { get; set; }

  public string? Value { get; set; }

  public Dictionary<string, string> ObjectIdentifiers { get; set; } = new Dictionary<string, string>();
}
