// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Contracts.PromptsConfiguration
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

#nullable disable
namespace PowerBIModelingMCP.Library.Contracts;

public class PromptsConfiguration
{
  public bool EnableConnectionPrompts { get; set; } = true;

  public bool EnableDaxQueryPrompts { get; set; } = true;
}
