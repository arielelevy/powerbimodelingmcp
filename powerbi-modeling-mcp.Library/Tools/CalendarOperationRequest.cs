// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using PowerBIModelingMCP.Library.Common.DataStructures;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

#nullable enable
namespace PowerBIModelingMCP.Library.Tools;

public class CalendarOperationRequest
{
  [Description("The connection name to use for the operation (optional - uses last used connection if not provided)")]
  public string? ConnectionName { get; set; }

  [Required]
  [Description("The operation to perform: Help, Create, Update, Delete, Get, List, Rename, ExportTMDL, CreateColumnGroup, UpdateColumnGroup, DeleteColumnGroup, GetColumnGroup, ListColumnGroups, RenameColumnGroup")]
  public required string Operation { get; set; }

  [Description("Calendar name (required for most operations)")]
  public string? CalendarName { get; set; }

  [Description("Table name (required for Create operations, optional for others as search hint)")]
  public string? TableName { get; set; }

  [Description("New calendar name (required for Rename operation)")]
  public string? NewCalendarName { get; set; }

  [Description("Type of column group for column group operations: TimeRelated or TimeUnitAssociation")]
  public string? ColumnGroupType { get; set; }

  [Description("Index of the column group for operations targeting specific column group by index")]
  public int? ColumnGroupIndex { get; set; }

  [Description("New column group name (for RenameColumnGroup operations)")]
  public string? NewColumnGroupName { get; set; }

  [Description("Calendar rename definition for Rename operation")]
  public CalendarRename? RenameDefinition { get; set; }

  [Description("Calendar definition for Create operation")]
  public CalendarCreate? CreateDefinition { get; set; }

  [Description("Calendar update definition for Update operation")]
  public CalendarUpdate? UpdateDefinition { get; set; }

  [Description("Calendar column group definition for CreateColumnGroup operation")]
  public CalendarColumnGroupCreate? ColumnGroupCreateDefinition { get; set; }

  [Description("Calendar column group update definition for UpdateColumnGroup operation")]
  public CalendarColumnGroupUpdate? ColumnGroupUpdateDefinition { get; set; }

  [Description("TMDL export options for ExportTMDL operation")]
  public CalendarExportTmdl? TmdlExportOptions { get; set; }
}
