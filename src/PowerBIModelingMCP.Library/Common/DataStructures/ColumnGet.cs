// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.DataStructures.ColumnGet
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class ColumnGet : ColumnBase
{
  public string? ColumnType { get; set; }

  public string? State { get; set; }

  public string? ErrorMessage { get; set; }

  public VariationDefinition? Variation { get; set; }
}
