// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.DataStructures.BatchDeleteColumnsRequest
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class BatchDeleteColumnsRequest
{
  [Description("Connection name (optional, uses last used connection if not provided)")]
  public string? ConnectionName { get; set; }

  [Required]
  [Description("List of column identifiers to delete")]
  public required List<ColumnIdentifier> Items { get; set; } = new List<ColumnIdentifier>();

  [Description("Indicates whether to cascade delete dependencies for each column (true by default)")]
  public bool ShouldCascadeDelete { get; set; } = true;

  [Description("Batch operation options")]
  public BatchOptions Options { get; set; } = new BatchOptions();
}
