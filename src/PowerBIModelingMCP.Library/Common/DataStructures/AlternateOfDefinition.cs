// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.DataStructures.AlternateOfDefinition
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using System.Collections.Generic;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class AlternateOfDefinition
{
  public string? BaseTable { get; set; }

  public string? BaseColumn { get; set; }

  public string? Summarization { get; set; }

  public List<KeyValuePair<string, string>> Annotations { get; set; } = new List<KeyValuePair<string, string>>();
}
