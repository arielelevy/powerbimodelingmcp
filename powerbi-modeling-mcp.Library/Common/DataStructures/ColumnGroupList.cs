// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.DataStructures.ColumnGroupList
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using System.Collections.Generic;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class ColumnGroupList : ObjectListBase
{
  [System.ComponentModel.Description("Type of column group: TimeRelated or TimeUnitAssociation")]
  public string? GroupType { get; set; }

  [System.ComponentModel.Description("List of column names in the column group")]
  public List<string>? ColumnNames { get; set; }

  [System.ComponentModel.Description("Primary column name (for TimeUnitAssociation groups)")]
  public string? PrimaryColumnName { get; set; }
}
