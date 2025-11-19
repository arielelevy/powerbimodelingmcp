// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using PowerBIModelingMCP.Library.Common.DataStructures;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

#nullable enable
namespace PowerBIModelingMCP.Library.Tools;

public class PerspectiveOperationRequest
{
  [Description("The connection name to use for the operation (optional - uses last used connection if not provided)")]
  public string? ConnectionName { get; set; }

  [Required]
  [Description("The operation to perform: help, list_perspectives, get_perspective, create_perspective, update_perspective, delete_perspective, rename_perspective, list_perspective_tables, get_perspective_table, add_table_to_perspective, remove_table_from_perspective, update_perspective_table, list_perspective_columns, get_perspective_column, add_column_to_perspective_table, remove_column_from_perspective_table, list_perspective_measures, get_perspective_measure, add_measure_to_perspective_table, remove_measure_from_perspective_table, list_perspective_hierarchies, get_perspective_hierarchy, add_hierarchy_to_perspective_table, remove_hierarchy_from_perspective_table")]
  public required string Operation { get; set; }

  [Description("Perspective name (required for get_perspective, update_perspective, delete_perspective, rename_perspective, and all table/column/measure/hierarchy operations)")]
  public string? PerspectiveName { get; set; }

  [Description("New perspective name (required for rename_perspective operation)")]
  public string? NewPerspectiveName { get; set; }

  [Description("Perspective definition for create_perspective operation")]
  public PerspectiveCreate? CreateDefinition { get; set; }

  [Description("Perspective update definition for update_perspective operation")]
  public PerspectiveUpdate? UpdateDefinition { get; set; }

  [Description("Table name (required for get_perspective_table, remove_table_from_perspective, and all column/measure/hierarchy operations)")]
  public string? TableName { get; set; }

  [Description("Perspective table definition for add_table_to_perspective operation")]
  public PerspectiveTableCreate? TableCreateDefinition { get; set; }

  [Description("Perspective table update definition for update_perspective_table operation")]
  public PerspectiveTableUpdate? TableUpdateDefinition { get; set; }

  [Description("Column name (required for get_perspective_column, remove_column_from_perspective_table operations)")]
  public string? ColumnName { get; set; }

  [Description("Perspective column definition for add_column_to_perspective_table operation")]
  public PerspectiveColumnCreate? ColumnCreateDefinition { get; set; }

  [Description("Measure name (required for get_perspective_measure, remove_measure_from_perspective_table operations)")]
  public string? MeasureName { get; set; }

  [Description("Perspective measure definition for add_measure_to_perspective_table operation")]
  public PerspectiveMeasureCreate? MeasureCreateDefinition { get; set; }

  [Description("Hierarchy name (required for get_perspective_hierarchy, remove_hierarchy_from_perspective_table operations)")]
  public string? HierarchyName { get; set; }

  [Description("Perspective hierarchy definition for add_hierarchy_to_perspective_table operation")]
  public PerspectiveHierarchyCreate? HierarchyCreateDefinition { get; set; }

  [Description("TMDL export options for ExportTMDL operation")]
  public PerspectiveExportTmdl? TmdlExportOptions { get; set; }
}
