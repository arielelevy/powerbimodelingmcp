// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.DataStructures.CalendarColumnGroupOperationResult
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class CalendarColumnGroupOperationResult
{
  public string CalendarName { get; set; } = string.Empty;

  public string GroupType { get; set; } = string.Empty;

  public int GroupIndex { get; set; }

  public int ColumnCount { get; set; }

  public string? TimeUnit { get; set; }

  public string? PrimaryColumnName { get; set; }
}
