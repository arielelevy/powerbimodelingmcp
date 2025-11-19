// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Tools.BatchMeasureOperationRequest
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using PowerBIModelingMCP.Library.Common.DataStructures;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

#nullable enable
namespace PowerBIModelingMCP.Library.Tools;

public class BatchMeasureOperationRequest
{
  [Description("Connection name (optional, uses last used connection if not provided)")]
  public string? ConnectionName { get; set; }

  [Required]
  [Description("The batch operation to perform: Help, BatchCreate, BatchUpdate, BatchDelete, BatchGet, BatchRename, BatchMove")]
  public required string Operation { get; set; }

  [Description("Batch create measures request")]
  public BatchCreateMeasuresRequest? BatchCreateRequest { get; set; }

  [Description("Batch update measures request")]
  public BatchUpdateMeasuresRequest? BatchUpdateRequest { get; set; }

  [Description("Batch delete measures request")]
  public BatchDeleteMeasuresRequest? BatchDeleteRequest { get; set; }

  [Description("Batch get measures request")]
  public BatchGetMeasuresRequest? BatchGetRequest { get; set; }

  [Description("Batch rename measures request")]
  public BatchRenameMeasuresRequest? BatchRenameRequest { get; set; }

  [Description("Batch move measures request")]
  public BatchMoveMeasuresRequest? BatchMoveRequest { get; set; }
}
