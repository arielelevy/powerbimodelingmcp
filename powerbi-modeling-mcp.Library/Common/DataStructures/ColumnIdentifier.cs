// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class ColumnIdentifier
{
  [Required]
  [Description("The name of the table containing the column")]
  public required string TableName { get; set; }

  [Required]
  [Description("The name of the column")]
  public required string Name { get; set; }
}
