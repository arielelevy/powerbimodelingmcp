// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Tools.BatchColumnOperationRequest
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using PowerBIModelingMCP.Library.Common.DataStructures;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

#nullable enable
namespace PowerBIModelingMCP.Library.Tools;

public class BatchColumnOperationRequest
{
  [Description("Connection name (optional, uses last used connection if not provided)")]
  public string? ConnectionName { get; set; }

  [Required]
  [Description("The batch operation to perform: Help, BatchCreate, BatchUpdate, BatchDelete, BatchGet, BatchRename")]
  public required string Operation { get; set; }

  [Description("Batch create columns request")]
  public BatchCreateColumnsRequest? BatchCreateRequest { get; set; }

  [Description("Batch update columns request")]
  public BatchUpdateColumnsRequest? BatchUpdateRequest { get; set; }

  [Description("Batch delete columns request")]
  public BatchDeleteColumnsRequest? BatchDeleteRequest { get; set; }

  [Description("Batch get columns request")]
  public BatchGetColumnsRequest? BatchGetRequest { get; set; }

  [Description("Batch rename columns request")]
  public BatchRenameColumnsRequest? BatchRenameRequest { get; set; }
}
