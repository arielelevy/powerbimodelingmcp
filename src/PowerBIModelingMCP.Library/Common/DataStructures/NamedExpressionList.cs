// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.DataStructures.NamedExpressionList
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class NamedExpressionList : ObjectListBase
{
  [System.ComponentModel.Description("Kind of expression (M for PowerQuery, DAX for calculations)")]
  public string? Kind { get; set; }

  [System.ComponentModel.Description("Name of the QueryGroup associated with the named expression (if any)")]
  public string? QueryGroupName { get; set; }
}
