// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using PowerBIModelingMCP.Library.Common.DataStructures;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

#nullable enable
namespace PowerBIModelingMCP.Library.Tools;

public class UserHierarchyOperationRequest
{
  [Required]
  [Description("The connection name for the Analysis Services instance")]
  public required string ConnectionName { get; set; }

  [Required]
  [Description("The operation to perform: Help, List, Get, Create, Update, Delete, Rename, GetColumns, AddLevel, RemoveLevel, UpdateLevel, RenameLevel, ReorderLevels, ExportTMDL")]
  public required string Operation { get; set; }

  [Description("Table name (required for all operations except Help)")]
  public string? TableName { get; set; }

  [Description("Hierarchy name (required for most operations except List)")]
  public string? HierarchyName { get; set; }

  [Description("New name for Rename operation")]
  public string? NewName { get; set; }

  [Description("Level name (required for level-specific operations)")]
  public string? LevelName { get; set; }

  [Description("New level name for RenameLevel operation")]
  public string? NewLevelName { get; set; }

  [Description("Hierarchy definition for Create operation")]
  public HierarchyCreate? CreateDefinition { get; set; }

  [Description("Hierarchy update definition for Update operation")]
  public HierarchyUpdate? UpdateDefinition { get; set; }

  [Description("Level definition for AddLevel operation")]
  public LevelCreate? LevelCreateDefinition { get; set; }

  [Description("Level update definition for UpdateLevel operation")]
  public LevelUpdate? LevelUpdateDefinition { get; set; }

  [Description("Ordered list of level names for ReorderLevels operation")]
  public List<string>? LevelNamesInOrder { get; set; }

  [Description("If true, will cascade delete dependent objects (columns, relationships, etc.) when deleting a hierarchy (true by default)")]
  public bool? ShouldCascadeDelete { get; set; }

  [Description("TMDL (YAML-like) export parameters for ExportTMDL operation")]
  public HierarchyExportTmdl? TmdlExportOptions { get; set; }
}
