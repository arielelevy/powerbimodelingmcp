// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Core.TraceEventDefinition
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using System;
using System.Collections.Generic;

#nullable enable
namespace PowerBIModelingMCP.Library.Core;

internal class TraceEventDefinition
{
  public string Name { get; set; } = string.Empty;

  public string Category { get; set; } = string.Empty;

  public string Description { get; set; } = string.Empty;

  public HashSet<string> Columns { get; set; } = new HashSet<string>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
}
