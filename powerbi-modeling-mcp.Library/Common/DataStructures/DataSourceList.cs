// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class DataSourceList : ObjectListBase
{
  [System.ComponentModel.Description("Type of data source (Provider, Structured, etc.)")]
  public string? Type { get; set; }
}
