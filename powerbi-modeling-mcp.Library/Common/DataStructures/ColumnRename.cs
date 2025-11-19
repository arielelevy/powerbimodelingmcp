// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class ColumnRename : ObjectRenameBase
{
  [Required]
  [Description("Table name containing the column")]
  public required string TableName { get; set; }
}
