// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.DataStructures.TablePermissionOperationResult
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class TablePermissionOperationResult
{
  public string State { get; set; } = string.Empty;

  public string? ErrorMessage { get; set; }

  public string RoleName { get; set; } = string.Empty;

  public string TableName { get; set; } = string.Empty;

  public bool HasChanges { get; set; }
}
