// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.SyncStateInfo
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using System;

#nullable enable
namespace PowerBIModelingMCP.Library.Common;

public class SyncStateInfo
{
  public bool CanSync { get; set; }

  public bool HasLocalChanges { get; set; }

  public bool IsOffline { get; set; }

  public bool IsInTransaction { get; set; }

  public DateTime? LastSynced { get; set; }

  public string Message { get; set; } = string.Empty;
}
