// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using PowerBIModelingMCP.Library.Common.DataStructures;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

#nullable enable
namespace PowerBIModelingMCP.Library.Tools;

public class ModelOperationRequest
{
  [Description("The connection name to use for the operation (optional - uses last used connection if not provided)")]
  public string? ConnectionName { get; set; }

  [Required]
  [Description("The operation to perform: Help, Get, Create, Update, Refresh, GetStats, Rename, ExportTMDL")]
  public required string Operation { get; set; }

  [Description("Model name (optional, defaults to current model)")]
  public string? ModelName { get; set; }

  [Description("New name for Rename operation")]
  public string? NewName { get; set; }

  [Description("Refresh type for Refresh operation (Automatic, Full, ClearValues, Calculate, DataOnly, Defragment)")]
  public string? RefreshType { get; set; }

  [Description("Model update definition for Update operation")]
  public ModelUpdate? UpdateDefinition { get; set; }

  [Description("Model creation definition for Create operation")]
  public ModelCreate? CreateDefinition { get; set; }

  [Description("TMDL (YAML-like) export parameters for ExportTMDL operation")]
  public ModelExportTmdl? TmdlExportOptions { get; set; }
}
