// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using PowerBIModelingMCP.Library.Common.DataStructures;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

#nullable enable
namespace PowerBIModelingMCP.Library.Tools;

public class QueryGroupOperationRequest
{
  [Description("The connection name to use for the operation (optional - uses last used connection if not provided)")]
  public string? ConnectionName { get; set; }

  [Required]
  [Description("The operation to perform: HELP, CREATE, UPDATE, DELETE, GET, LIST, ExportTMDL")]
  public required string Operation { get; set; }

  [Description("Query group name (required for most operations)")]
  public string? QueryGroupName { get; set; }

  [Description("Query group definition for CREATE operation")]
  public QueryGroupCreate? CreateDefinition { get; set; }

  [Description("Query group update definition for UPDATE operation")]
  public QueryGroupUpdate? UpdateDefinition { get; set; }

  [Description("TMDL (YAML-like) export parameters for ExportTMDL operation")]
  public QueryGroupExportTmdl? TmdlExportOptions { get; set; }
}
