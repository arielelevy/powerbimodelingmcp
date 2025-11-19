// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System.ComponentModel.DataAnnotations;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class TmdlDeployRequest
{
  [Required]
  public required string SourceConnectionName { get; set; }

  [Required]
  public required string TargetConnectionName { get; set; }
}
