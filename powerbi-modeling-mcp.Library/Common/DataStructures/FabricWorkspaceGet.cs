// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.DataStructures.FabricWorkspaceGet
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using System;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class FabricWorkspaceGet
{
  public required Guid Id { get; set; }

  public string? Name { get; set; }

  public string? Description { get; set; }

  public string? Type { get; set; }

  public string? State { get; set; }

  public Guid? CapacityId { get; set; }

  public object? Raw { get; set; }
}
