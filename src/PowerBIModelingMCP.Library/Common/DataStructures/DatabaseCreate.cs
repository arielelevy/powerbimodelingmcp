// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.DataStructures.DatabaseCreate
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class DatabaseCreate : DatabaseBase
{
  public string? ModelName { get; set; }

  public bool? IsOffline { get; set; }
}
