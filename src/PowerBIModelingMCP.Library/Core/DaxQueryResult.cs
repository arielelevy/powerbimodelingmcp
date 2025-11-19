// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Core.DaxQueryResult
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using System.Collections.Generic;

#nullable enable
namespace PowerBIModelingMCP.Library.Core;

public class DaxQueryResult
{
  public bool Success { get; set; }

  public string? ErrorMessage { get; set; }

  public int RowCount { get; set; }

  public List<DaxColumnInfo> Columns { get; set; } = new List<DaxColumnInfo>();

  public List<Dictionary<string, object?>> Rows { get; set; } = new List<Dictionary<string, object>>();

  public long ExecutionTimeMs { get; set; }
}
