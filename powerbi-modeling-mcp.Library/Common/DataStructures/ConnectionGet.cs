// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.DataStructures.ConnectionGet
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using System;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class ConnectionGet
{
  public string ConnectionName { get; set; } = string.Empty;

  public string? ServerConnectionString { get; set; }

  public string? DatabaseName { get; set; }

  public string? ServerName { get; set; }

  public bool IsCloudConnection { get; set; }

  public bool IsOffline { get; set; }

  public string? TmdlFolderPath { get; set; }

  public DateTime? ConnectedAt { get; set; }

  public DateTime? LastUsedAt { get; set; }

  public bool HasTransaction { get; set; }

  public bool HasTrace { get; set; }

  public string? SessionId { get; set; }
}
