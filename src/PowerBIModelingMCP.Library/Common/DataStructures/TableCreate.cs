// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.DataStructures.TableCreate
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using System.Collections.Generic;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class TableCreate : TableBase
{
  public string? DaxExpression { get; set; }

  public string? MExpression { get; set; }

  public string? SqlQuery { get; set; }

  public string? EntityName { get; set; }

  public string? SchemaName { get; set; }

  public string? ExpressionSourceName { get; set; }

  public string? DataSourceName { get; set; }

  public string? PartitionName { get; set; }

  public string? Mode { get; set; }

  public List<ColumnCreate>? Columns { get; set; }
}
