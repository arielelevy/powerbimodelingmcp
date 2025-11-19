// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

#nullable enable
namespace PowerBIModelingMCP.Library.Tools;

public class FabricOperationRequest
{
  [Required]
  [Description("The operation to perform: Help, ListWorkspaces, GetWorkspace, ListItems, GetItem, GetSemanticModel")]
  public required string Operation { get; set; }

  [Description("Optional access token override for authentication")]
  public string? AccessToken { get; set; }

  [Description("Workspace ID - required for GetWorkspace and all item and semantic model operations: ListItems, GetItem, ListSemanticModels, GetSemanticModel")]
  public Guid? WorkspaceId { get; set; }

  [Description("Item ID for GetItem operation")]
  public Guid? ItemId { get; set; }

  [Description("Item type for GetItem operation (e.g., SemanticModel, Report, Dashboard, etc.)")]
  public string? ItemType { get; set; }

  [Description("Semantic model ID for GetSemanticModel operation")]
  public Guid? ModelId { get; set; }
}
