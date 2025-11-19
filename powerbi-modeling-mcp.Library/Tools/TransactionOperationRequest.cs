// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

#nullable enable
namespace PowerBIModelingMCP.Library.Tools;

public class TransactionOperationRequest
{
  [Required]
  [Description("The operation to perform: Help, Begin, Commit, Rollback, GetStatus, ListActive")]
  public required string Operation { get; set; }

  [Description("Connection name (required for Begin operation)")]
  public string? ConnectionName { get; set; }

  [Description("Transaction ID (required for Commit, Rollback, GetStatus operations)")]
  public string? TransactionId { get; set; }
}
