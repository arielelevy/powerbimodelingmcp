// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.DataStructures.TmdlDeployResult
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using System;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class TmdlDeployResult
{
  public bool Success { get; set; }

  public string SourceConnectionName { get; set; } = string.Empty;

  public string TargetConnectionName { get; set; } = string.Empty;

  public string SourceDatabaseName { get; set; } = string.Empty;

  public string TargetDatabaseName { get; set; } = string.Empty;

  public DateTime DeploymentTimestamp { get; set; }

  public string? Message { get; set; }
}
