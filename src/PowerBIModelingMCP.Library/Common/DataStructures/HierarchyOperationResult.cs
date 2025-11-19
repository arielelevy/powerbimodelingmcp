// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.DataStructures.HierarchyOperationResult
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using System.Collections.Generic;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class HierarchyOperationResult
{
  public string? State { get; set; }

  public string? HierarchyName { get; set; }

  public string? TableName { get; set; }

  public int LevelCount { get; set; }

  public List<string> LevelNames { get; set; } = new List<string>();

  public bool HasChanges { get; set; }
}
