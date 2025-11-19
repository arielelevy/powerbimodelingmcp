// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Services.ParsedResourceDefinition
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

#nullable enable
namespace PowerBIModelingMCP.Library.Services;

public class ParsedResourceDefinition
{
  public string Name { get; set; } = string.Empty;

  public string Description { get; set; } = string.Empty;

  public string UriTemplate { get; set; } = string.Empty;

  public string MimeType { get; init; } = "text/plain";

  public string Text { get; set; } = string.Empty;
}
