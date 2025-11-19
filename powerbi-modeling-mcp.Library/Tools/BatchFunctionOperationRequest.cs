// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using PowerBIModelingMCP.Library.Common.DataStructures;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

#nullable enable
namespace PowerBIModelingMCP.Library.Tools;

public class BatchFunctionOperationRequest
{
  [Description("Connection name (optional, uses last used connection if not provided)")]
  public string? ConnectionName { get; set; }

  [Required]
  [Description("The batch operation to perform: Help, BatchCreate, BatchUpdate, BatchDelete, BatchGet, BatchRename")]
  public required string Operation { get; set; }

  [Description("Batch create functions request")]
  public BatchCreateFunctionsRequest? BatchCreateRequest { get; set; }

  [Description("Batch update functions request")]
  public BatchUpdateFunctionsRequest? BatchUpdateRequest { get; set; }

  [Description("Batch delete functions request")]
  public BatchDeleteFunctionsRequest? BatchDeleteRequest { get; set; }

  [Description("Batch get functions request")]
  public BatchGetFunctionsRequest? BatchGetRequest { get; set; }

  [Description("Batch rename functions request")]
  public BatchRenameFunctionsRequest? BatchRenameRequest { get; set; }
}
