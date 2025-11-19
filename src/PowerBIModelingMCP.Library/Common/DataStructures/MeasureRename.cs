// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.DataStructures.MeasureRename
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using System.ComponentModel;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class MeasureRename : ObjectRenameBase
{
  [Description("Table name containing the measure (optional if measure name is unique)")]
  public string? TableName { get; set; }
}
