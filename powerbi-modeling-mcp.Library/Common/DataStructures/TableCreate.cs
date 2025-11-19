// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
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
