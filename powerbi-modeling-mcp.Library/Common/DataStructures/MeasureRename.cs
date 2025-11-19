// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System.ComponentModel;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class MeasureRename : ObjectRenameBase
{
  [Description("Table name containing the measure (optional if measure name is unique)")]
  public string? TableName { get; set; }
}
