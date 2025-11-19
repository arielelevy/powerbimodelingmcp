// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.DataStructures.TmdlDeserializeResult
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using System;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class TmdlDeserializeResult
{
  public bool Success { get; set; }

  public string ConnectionName { get; set; } = string.Empty;

  public string DatabaseName { get; set; } = string.Empty;

  public string FolderPath { get; set; } = string.Empty;

  public int TablesLoaded { get; set; }

  public int MeasuresLoaded { get; set; }

  public int RelationshipsLoaded { get; set; }

  public DateTime LoadedAt { get; set; }

  public string? Message { get; set; }
}
