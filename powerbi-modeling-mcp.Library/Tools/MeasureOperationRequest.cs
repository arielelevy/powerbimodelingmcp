// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using PowerBIModelingMCP.Library.Common.DataStructures;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

#nullable enable
namespace PowerBIModelingMCP.Library.Tools;

public class MeasureOperationRequest
{
  [Description("Connection name (optional, uses last used connection if not provided)")]
  public string? ConnectionName { get; set; }

  [Required]
  [Description("The operation to perform: Help, Create, Update, Delete, Get, List, Rename, Move, ExportTMDL")]
  public required string Operation { get; set; }

  [Description("Measure name (required for all operations except List)")]
  public string? MeasureName { get; set; }

  [Description("Table name (optional for List operation to filter measures by table)")]
  public string? TableName { get; set; }

  [Description("Maximum number of items to return for List operation (default: 200)")]
  public int? MaxResults { get; set; } = new int?(200);

  [Description("Measure rename definition for Rename operation")]
  public MeasureRename? RenameDefinition { get; set; }

  [Description("Measure move definition for Move operation")]
  public MeasureMove? MoveDefinition { get; set; }

  [Description("Measure definition for Create operation")]
  public MeasureCreate? CreateDefinition { get; set; }

  [Description("Measure update definition for Update operation")]
  public MeasureUpdate? UpdateDefinition { get; set; }

  [Description("Indicates whether to cascade delete the column for Delete operation (true by default)")]
  public bool? ShouldCascadeDelete { get; set; }

  [Description("TMDL (YAML-like) export parameters for ExportTMDL operation")]
  public MeasureExportTmdl? TmdlExportOptions { get; set; }
}
