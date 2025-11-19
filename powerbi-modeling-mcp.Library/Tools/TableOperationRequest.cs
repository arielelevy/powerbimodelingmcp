// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Tools.TableOperationRequest
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using PowerBIModelingMCP.Library.Common.DataStructures;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

#nullable enable
namespace PowerBIModelingMCP.Library.Tools;

public class TableOperationRequest
{
  [Description("Connection name (optional, uses last used connection if not provided)")]
  public string? ConnectionName { get; set; }

  [Required]
  [Description("The operation to perform: Help, Create, Update, Delete, Get, List, Refresh, Rename, GetSchema, ExportTMDL (YAML-like format), ExportTMSL (JSON script format)")]
  public required string Operation { get; set; }

  [Description("Table name (required for all operations except List)")]
  public string? TableName { get; set; }

  [Description("Table rename definition for Rename operation")]
  public TableRename? RenameDefinition { get; set; }

  [Description("Table definition for Create operation")]
  public TableCreate? CreateDefinition { get; set; }

  [Description("Table update definition for Update operation")]
  public TableUpdate? UpdateDefinition { get; set; }

  [Description("If true, will cascade delete dependent objects (columns, relationships, etc.) when deleting a table (true by default)")]
  public bool? ShouldCascadeDelete { get; set; }

  [Description("TMDL (YAML-like) export parameters for ExportTMDL operation")]
  public TableExportTmdl? TmdlExportOptions { get; set; }

  [Description("TMSL (JSON script) export parameters for ExportTMSL operation")]
  public TableExportTmsl? TmslExportOptions { get; set; }
}
