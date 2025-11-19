// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Tools.DatabaseOperationRequest
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using PowerBIModelingMCP.Library.Common.DataStructures;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

#nullable enable
namespace PowerBIModelingMCP.Library.Tools;

public class DatabaseOperationRequest
{
  [Description("The connection name to use for the operation. For most operations, this specifies which existing connection to use (optional - uses last used connection if not provided). For ImportFromTmdlFolder and Create, this specifies the name for the new offline connection (optional - auto-generated if not provided). For Create, defaults to the database name with numeric suffixes added if needed to ensure uniqueness.")]
  public string? ConnectionName { get; set; }

  [Required]
  [Description("The operation to perform: Help, List, Update, ImportFromTmdlFolder, ExportToTmdlFolder, DeployToFabric, Create, ExportTMDL (YAML-like format), ExportTMSL (JSON script format)")]
  public required string Operation { get; set; }

  [Description("Database name (required for Update operation)")]
  public string? DatabaseName { get; set; }

  [Description("Database update definition for Update operation")]
  public DatabaseUpdate? UpdateDefinition { get; set; }

  [Description("Database creation definition for Create operation")]
  public DatabaseCreate? CreateDefinition { get; set; }

  [Description("TMDL folder path (required for ImportFromTmdlFolder and ExportToTmdlFolder operations)")]
  public string? TmdlFolderPath { get; set; }

  [Description("Deploy to Fabric parameters for DeployToFabric operation")]
  public DeployToFabricRequest? DeployToFabricRequest { get; set; }

  [Description("TMDL (YAML-like) export parameters for ExportTMDL operation")]
  public DatabaseExportTmdl? TmdlExportOptions { get; set; }

  [Description("TMSL (JSON script) export parameters for ExportTMSL operation")]
  public DatabaseExportTmsl? TmslExportOptions { get; set; }
}
