// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.DataStructures.DatabaseGet
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using System;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class DatabaseGet : DatabaseBase
{
  public string? Id { get; set; }

  public string? State { get; set; }

  public DateTime CreatedTimestamp { get; set; }

  public DateTime LastProcessed { get; set; }

  public DateTime LastUpdate { get; set; }

  public DateTime LastSchemaUpdate { get; set; }

  public long EstimatedSize { get; set; }

  public string? Model { get; set; }

  public string? ModelType { get; set; }
}
