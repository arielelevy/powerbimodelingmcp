// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using PowerBIModelingMCP.Library.Common.DataStructures;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

#nullable enable
namespace PowerBIModelingMCP.Library.Tools;

public class PartitionOperationRequest
{
  [Description("The connection name to use for the operation (optional - uses last used connection if not provided)")]
  public string? ConnectionName { get; set; }

  [Required]
  [Description("The operation to perform: Help, List, Get, Create, Update, Delete, Refresh, Rename, ExportTMDL (YAML-like format), ExportTMSL (JSON script format)")]
  public required string Operation { get; set; }

  [Description("Table name (required for most operations, optional for List to filter by table)")]
  public string? TableName { get; set; }

  [Description("Partition name (required for Get, Delete, Refresh, Rename operations)")]
  public string? PartitionName { get; set; }

  [Description("New name for Rename operation")]
  public string? NewName { get; set; }

  [Description("Refresh type for Refresh operation (Automatic, Full, ClearValues, Calculate, DataOnly, Defragment)")]
  public string? RefreshType { get; set; }

  [Description("Partition definition for Create operation")]
  public PartitionCreate? CreateDefinition { get; set; }

  [Description("Partition update definition for Update operation")]
  public PartitionUpdate? UpdateDefinition { get; set; }

  [Description("TMDL (YAML-like) export parameters for ExportTMDL operation")]
  public PartitionExportTmdl? TmdlExportOptions { get; set; }

  [Description("TMSL (JSON script) export parameters for ExportTMSL operation")]
  public PartitionExportTmsl? TmslExportOptions { get; set; }
}
