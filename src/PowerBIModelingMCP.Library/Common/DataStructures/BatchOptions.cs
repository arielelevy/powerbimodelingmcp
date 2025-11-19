// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.DataStructures.BatchOptions
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using System.ComponentModel;

#nullable disable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class BatchOptions
{
  [Description("Continue processing remaining items when an error occurs")]
  public bool ContinueOnError { get; set; }

  [Description("Wrap the batch operation in a transaction for atomicity")]
  public bool UseTransaction { get; set; } = true;
}
