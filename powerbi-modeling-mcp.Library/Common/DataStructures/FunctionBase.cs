// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class FunctionBase
{
  [Required]
  [System.ComponentModel.Description("The name of the function")]
  public required string Name { get; set; }

  [System.ComponentModel.Description("The DAX function parameters and expression")]
  public string? Expression { get; set; }

  [System.ComponentModel.Description("The description of the user-defined function")]
  public string? Description { get; set; }

  [System.ComponentModel.Description("Whether the function is treated as hidden by client visualization tools")]
  public bool? IsHidden { get; set; }

  [System.ComponentModel.Description("A tag that represents the lineage of the object")]
  public string? LineageTag { get; set; }

  [System.ComponentModel.Description("A tag that represents the lineage of the source for the object")]
  public string? SourceLineageTag { get; set; }

  [System.ComponentModel.Description("Collection of annotations as key-value pairs")]
  public List<KeyValuePair<string, string>>? Annotations { get; set; }

  [System.ComponentModel.Description("Collection of extended properties for storing custom metadata")]
  public List<ExtendedProperty>? ExtendedProperties { get; set; }

  [System.ComponentModel.Description("The state of the function (Ready, SemanticError, SyntaxError)")]
  public string? State { get; set; }

  [System.ComponentModel.Description("Error message when state indicates an error")]
  public string? ErrorMessage { get; set; }

  [System.ComponentModel.Description("The time that the object was last modified")]
  public DateTime? ModifiedTime { get; set; }

  [System.ComponentModel.Description("The time that the structure was last modified")]
  public DateTime? StructureModifiedTime { get; set; }
}
