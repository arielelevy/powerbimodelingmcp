// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using PowerBIModelingMCP.Library.Common.DataStructures;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

#nullable enable
namespace PowerBIModelingMCP.Library.Tools;

public class FunctionOperationRequest
{
  [Description("The connection name to use for the operation (optional - uses last used connection if not provided)")]
  public string? ConnectionName { get; set; }

  [Required]
  [Description("The operation to perform: Help, Create, Update, Delete, Get, List, Rename, ExportTMDL")]
  public required string Operation { get; set; }

  [Description("Function name (required for Get, Delete, ExportTMDL operations; optional for Create and Rename operations as fallback if not specified in definition)")]
  public string? FunctionName { get; set; }

  [Description("Function rename definition for Rename operation")]
  public FunctionRename? RenameDefinition { get; set; }

  [Description("Function definition for Create operation")]
  public FunctionCreate? CreateDefinition { get; set; }

  [Description("Function update definition for Update operation")]
  public FunctionUpdate? UpdateDefinition { get; set; }

  [Description("TMDL export options for ExportTMDL operation")]
  public FunctionExportTmdl? TmdlExportOptions { get; set; }
}
