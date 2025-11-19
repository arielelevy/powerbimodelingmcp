// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.DataStructures.BatchUpdateFunctionsRequest
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class BatchUpdateFunctionsRequest
{
  [Description("Connection name (optional, uses last used connection if not provided)")]
  public string? ConnectionName { get; set; }

  [Required]
  [Description("List of function updates")]
  public required List<FunctionUpdate> Items { get; set; } = new List<FunctionUpdate>();

  [Description("Batch operation options")]
  public BatchOptions Options { get; set; } = new BatchOptions();
}
