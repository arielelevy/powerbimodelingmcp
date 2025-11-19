// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Tools.FabricOperationRequest
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

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
