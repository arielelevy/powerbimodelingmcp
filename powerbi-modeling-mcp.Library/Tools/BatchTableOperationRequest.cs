// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using PowerBIModelingMCP.Library.Common.DataStructures;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

#nullable enable
namespace PowerBIModelingMCP.Library.Tools;

public class BatchTableOperationRequest
{
  [Description("Connection name (optional, uses last used connection if not provided)")]
  public string? ConnectionName { get; set; }

  [Required]
  [Description("The batch operation to perform: Help, BatchCreate, BatchUpdate, BatchDelete, BatchGet, BatchRename")]
  public required string Operation { get; set; }

  [Description("Batch create tables request")]
  public BatchCreateTablesRequest? BatchCreateRequest { get; set; }

  [Description("Batch update tables request")]
  public BatchUpdateTablesRequest? BatchUpdateRequest { get; set; }

  [Description("Batch delete tables request")]
  public BatchDeleteTablesRequest? BatchDeleteRequest { get; set; }

  [Description("Batch get tables request")]
  public BatchGetTablesRequest? BatchGetRequest { get; set; }

  [Description("Batch rename tables request")]
  public BatchRenameTablesRequest? BatchRenameRequest { get; set; }
}
