// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Core.LocalAnalysisServicesInstance
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using System;

#nullable enable
namespace PowerBIModelingMCP.Library.Core;

public class LocalAnalysisServicesInstance
{
  public int ProcessId { get; set; }

  public int Port { get; set; }

  public string ConnectionString { get; set; } = string.Empty;

  public string? ParentProcessName { get; set; }

  public string? ParentWindowTitle { get; set; }

  public DateTime StartTime { get; set; }
}
