// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using PowerBIModelingMCP.Library.Common.DataStructures;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

#nullable enable
namespace PowerBIModelingMCP.Library.Tools;

public class DataSourceOperationRequest
{
  [Description("Connection name (optional, uses last used connection if not provided)")]
  public string? ConnectionName { get; set; }

  [Required]
  [Description("The operation to perform: Help, Create, Update, Delete, Get, List, Rename, Test, ExportTMDL")]
  public required string Operation { get; set; }

  [Description("SSAS data source name (required for all operations except List)")]
  public string? DataSourceName { get; set; }

  [Description("SSAS data source rename definition for Rename operation")]
  public DataSourceRename? RenameDefinition { get; set; }

  [Description("SSAS data source definition for Create operation")]
  public DataSourceCreate? CreateDefinition { get; set; }

  [Description("SSAS data source update definition for Update operation")]
  public DataSourceUpdate? UpdateDefinition { get; set; }

  [Description("TMDL export options for ExportTMDL operation")]
  public DataSourceExportTmdl? TmdlExportOptions { get; set; }
}
