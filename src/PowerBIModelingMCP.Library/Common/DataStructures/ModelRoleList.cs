// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.DataStructures.ModelRoleList
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using System.Collections.Generic;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class ModelRoleList : ObjectListBase
{
  [System.ComponentModel.Description("The model permission level (None, Read, ReadRefresh, Refresh, Administrator)")]
  public string? ModelPermission { get; set; }

  [System.ComponentModel.Description("List of table names that have permissions defined in this role")]
  public List<string>? TableNames { get; set; }
}
