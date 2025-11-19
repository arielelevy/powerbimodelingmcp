// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.DataStructures.ColumnList
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class ColumnList : ObjectListBase
{
  [System.ComponentModel.Description("Data type of the column (String, Integer, DateTime, etc.)")]
  public string? DataType { get; set; }

  [System.ComponentModel.Description("Whether the column is calculated (has an expression)")]
  public bool? IsCalculated { get; set; }

  [System.ComponentModel.Description("Display folder for organizing columns")]
  public string? DisplayFolder { get; set; }
}
