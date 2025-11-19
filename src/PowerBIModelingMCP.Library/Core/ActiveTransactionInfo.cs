// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Core.ActiveTransactionInfo
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

#nullable enable
namespace PowerBIModelingMCP.Library.Core;

public class ActiveTransactionInfo
{
  public string TransactionId { get; set; } = string.Empty;

  public string StartTime { get; set; } = string.Empty;

  public double Duration { get; set; }

  public int OperationCount { get; set; }

  public string Database { get; set; } = string.Empty;

  public string Server { get; set; } = string.Empty;

  public bool IsCurrent { get; set; }

  public string TransactionType { get; set; } = string.Empty;
}
