// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.DataStructures.ItemResult
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using System.Collections.Generic;
using System.ComponentModel;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class ItemResult
{
  [Description("Index of the item in the original batch")]
  public int Index { get; set; }

  [Description("Whether this individual item operation was successful")]
  public bool Success { get; set; }

  [Description("Message about this individual item operation")]
  public string Message { get; set; } = string.Empty;

  [Description("Operation-specific result data (e.g., retrieved measure data for BatchGet)")]
  public object? Data { get; set; }

  [Description("Item identifier (e.g., measure name)")]
  public string? ItemIdentifier { get; set; }

  [Description("List of warnings for this individual item operation")]
  public List<string> Warnings { get; set; } = new List<string>();
}
