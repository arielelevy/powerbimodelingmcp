// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Tools.RelationshipOperationRequest
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using PowerBIModelingMCP.Library.Common.DataStructures;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

#nullable enable
namespace PowerBIModelingMCP.Library.Tools;

public class RelationshipOperationRequest
{
  [Description("Connection name (optional, uses last used connection if not provided)")]
  public string? ConnectionName { get; set; }

  [Required]
  [Description("The operation to perform: Help, List, Get, Create, Update, Delete, Rename, Activate, Deactivate, Find, ExportTMDL")]
  public required string Operation { get; set; }

  [Description("Relationship name (required for Get, Delete, Rename, Activate, Deactivate operations)")]
  public string? RelationshipName { get; set; }

  [Description("Relationship rename definition for Rename operation")]
  public RelationshipRename? RenameDefinition { get; set; }

  [Description("Table name for Find operation")]
  public string? TableName { get; set; }

  [Description("Relationship definition for Create operation")]
  public RelationshipCreate? RelationshipDefinition { get; set; }

  [Description("Relationship update definition for Update operation")]
  public RelationshipUpdate? RelationshipUpdate { get; set; }

  [Description("TMDL (YAML-like) export parameters for ExportTMDL operation")]
  public RelationshipExportTmdl? TmdlExportOptions { get; set; }
}
