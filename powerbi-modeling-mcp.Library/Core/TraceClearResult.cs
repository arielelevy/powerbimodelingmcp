// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Core.TraceClearResult
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

#nullable enable
namespace PowerBIModelingMCP.Library.Core;

public class TraceClearResult
{
  public string TraceName { get; set; } = string.Empty;

  public int EventsCleared { get; set; }

  public string Status { get; set; } = string.Empty;
}
