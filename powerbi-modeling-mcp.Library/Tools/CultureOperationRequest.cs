// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using PowerBIModelingMCP.Library.Common.DataStructures;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

#nullable enable
namespace PowerBIModelingMCP.Library.Tools;

public class CultureOperationRequest
{
  [Description("The connection name to use for the operation (optional - uses last used connection if not provided)")]
  public string? ConnectionName { get; set; }

  [Required]
  [Description("The operation to perform: Help, Create, Update, Delete, Get, List, Rename, GetValidNames, GetValidDetails, GetDetailsByName, GetDetailsByLCID, ExportTMDL")]
  public required string Operation { get; set; }

  [Description("Culture name (required for all operations except List, GetValidNames, and GetValidDetails)")]
  public string? CultureName { get; set; }

  [Description("New culture name (required for Rename operation)")]
  public string? NewCultureName { get; set; }

  [Description("Culture definition for Create operation")]
  public CultureCreate? CreateDefinition { get; set; }

  [Description("Culture update definition for Update operation")]
  public CultureUpdate? UpdateDefinition { get; set; }

  [Description("Include neutral cultures like 'en', 'fr' (for GetValidNames operation, default: true)")]
  public bool IncludeNeutralCultures { get; set; } = true;

  [Description("Include user-defined custom cultures (for GetValidNames operation, default: false)")]
  public bool IncludeUserCustomCultures { get; set; }

  [Description("LCID (Locale Identifier) for GetDetailsByLCID operation")]
  public int? LCID { get; set; }

  [Description("TMDL (YAML-like) export parameters for ExportTMDL operation")]
  public CultureExportTmdl? TmdlExportOptions { get; set; }
}
