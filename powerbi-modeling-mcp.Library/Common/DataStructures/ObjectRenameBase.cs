// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public abstract class ObjectRenameBase
{
  [Required]
  [Description("Current name of the object")]
  public required string CurrentName { get; set; }

  [Required]
  [Description("New name for the object")]
  public required string NewName { get; set; }
}
