// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using PowerBIModelingMCP.Library.Common.DataStructures;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

#nullable enable
namespace PowerBIModelingMCP.Library.Tools;

public class ColumnOperationRequest
{
  [Description("Connection name (optional, uses last used connection if not provided)")]
  public string? ConnectionName { get; set; }

  [Required]
  [Description("The operation to perform: Help, Create, Update, Delete, Get, List, Rename")]
  public required string Operation { get; set; }

  [Description("Table name (required for all column operations except Help)")]
  public string? TableName { get; set; }

  [Description("Column name (required for all operations except List)")]
  public string? ColumnName { get; set; }

  [Description("Maximum number of items to return for List operation (default: 200)")]
  public int? MaxResults { get; set; } = new int?(200);

  [Description("Column rename definition for Rename operation")]
  public ColumnRename? RenameDefinition { get; set; }

  [Description("Column definition for Create operation")]
  public ColumnCreate? CreateDefinition { get; set; }

  [Description("Column update definition for Update operation")]
  public ColumnUpdate? UpdateDefinition { get; set; }

  [Description("Indicates whether to cascade delete the column for Delete operation (true by default)")]
  public bool? ShouldCascadeDelete { get; set; }

  [Description("TMDL (YAML-like) export parameters for ExportTMDL operation")]
  public ColumnExportTmdl? TmdlExportOptions { get; set; }
}
