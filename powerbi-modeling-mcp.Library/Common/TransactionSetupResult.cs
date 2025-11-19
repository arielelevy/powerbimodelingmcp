// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.TransactionSetupResult
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

#nullable enable
namespace PowerBIModelingMCP.Library.Common;

public class TransactionSetupResult
{
  public string? TransactionId { get; set; }

  public bool OwnsTransaction { get; set; }
}
