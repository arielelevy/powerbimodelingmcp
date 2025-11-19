// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.DataStructures.PerspectiveList
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

#nullable disable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class PerspectiveList : ObjectListBase
{
  [System.ComponentModel.Description("Number of tables in the perspective")]
  public int? TableCount { get; set; }

  [System.ComponentModel.Description("Number of measures across all tables in the perspective")]
  public int? MeasureCount { get; set; }

  [System.ComponentModel.Description("Number of columns across all tables in the perspective")]
  public int? ColumnCount { get; set; }

  [System.ComponentModel.Description("Number of hierarchies across all tables in the perspective")]
  public int? HierarchyCount { get; set; }
}
