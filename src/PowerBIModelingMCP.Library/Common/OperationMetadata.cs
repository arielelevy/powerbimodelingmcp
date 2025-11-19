// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.OperationMetadata
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using System;
using System.Collections.Generic;

#nullable enable
namespace PowerBIModelingMCP.Library.Common;

public class OperationMetadata
{
  public string[] RequiredParams { get; set; } = Array.Empty<string>();

  public string[] OptionalParams { get; set; } = Array.Empty<string>();

  public string[] ForbiddenParams { get; set; } = Array.Empty<string>();

  public string Description { get; set; } = "";

  public string[] CommonMistakes { get; set; } = Array.Empty<string>();

  public string[] Tips { get; set; } = Array.Empty<string>();

  public List<string>? ExampleRequests { get; set; }
}
