// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Core.TransactionContext
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using Microsoft.AnalysisServices.Tabular;
using System;
using System.Collections.Generic;

#nullable enable
namespace PowerBIModelingMCP.Library.Core;

public class TransactionContext
{
  public string TransactionId { get; set; } = Guid.NewGuid().ToString();

  public DateTime StartTime { get; set; } = DateTime.UtcNow;

  public List<string> Operations { get; set; } = new List<string>();

  public required Server Server { get; set; }

  public required Database Database { get; set; }
}
