// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.DataStructures.CalendarList
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using System.Collections.Generic;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class CalendarList : ObjectListBase
{
  [System.ComponentModel.Description("Table containing this calendar")]
  public string? TableName { get; set; }

  [System.ComponentModel.Description("List of column groups in the calendar")]
  public List<ColumnGroupList>? ColumnGroups { get; set; }
}
