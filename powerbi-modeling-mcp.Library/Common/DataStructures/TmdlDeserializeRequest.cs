// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System.ComponentModel.DataAnnotations;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class TmdlDeserializeRequest
{
  [Required]
  public required string FolderPath { get; set; }

  public string? ConnectionName { get; set; }
}
