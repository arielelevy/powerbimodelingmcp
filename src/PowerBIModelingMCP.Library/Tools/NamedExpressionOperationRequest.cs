// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Tools.NamedExpressionOperationRequest
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using PowerBIModelingMCP.Library.Common.DataStructures;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

#nullable enable
namespace PowerBIModelingMCP.Library.Tools;

public class NamedExpressionOperationRequest
{
  [Description("The connection name to use for the operation (optional - uses last used connection if not provided)")]
  public string? ConnectionName { get; set; }

  [Required]
  [Description("The operation to perform: Help, Create, Update, Delete, Get, List, Rename, CreateParameter, UpdateParameter, ExportTMDL")]
  public required string Operation { get; set; }

  [Description("Named expression name (required for all operations except List)")]
  public string? NamedExpressionName { get; set; }

  [Description("Named expression rename definition for Rename operation")]
  public NamedExpressionRename? RenameDefinition { get; set; }

  [Description("Named expression definition for Create operation")]
  public NamedExpressionCreate? CreateDefinition { get; set; }

  [Description("Named expression update definition for Update operation")]
  public NamedExpressionUpdate? UpdateDefinition { get; set; }

  [Description("TMDL export options for ExportTMDL operation")]
  public NamedExpressionExportTmdl? TmdlExportOptions { get; set; }
}
