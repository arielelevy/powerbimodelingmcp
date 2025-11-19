// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Tools.TransactionOperationRequest
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

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
