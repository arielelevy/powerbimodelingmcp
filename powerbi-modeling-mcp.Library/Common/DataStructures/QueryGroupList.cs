// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class QueryGroupList : ObjectListBase
{
  [System.ComponentModel.Description("Folder path for organizing query groups")]
  public string? Folder { get; set; }
}
