// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.DataStructures.DataSourceBase
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using System.Collections.Generic;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class DataSourceBase
{
  public string? Name { get; set; }

  public string? Description { get; set; }

  public string? ConnectionString { get; set; }

  public string? ImpersonationMode { get; set; }

  public string? Password { get; set; }

  public string? Account { get; set; }

  public int? MaxConnections { get; set; }

  public string? Isolation { get; set; }

  public int? Timeout { get; set; }

  public string? Provider { get; set; }

  public List<KeyValuePair<string, string>>? Annotations { get; set; }

  public List<ExtendedProperty>? ExtendedProperties { get; set; }
}
