// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.DataStructures.TableColumnList
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using System.Collections.Generic;
using System.ComponentModel;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class TableColumnList
{
  [Description("The name of the table")]
  public required string TableName { get; set; }

  [Description("The list of columns in this table")]
  public required List<ColumnList> Columns { get; set; }
}
