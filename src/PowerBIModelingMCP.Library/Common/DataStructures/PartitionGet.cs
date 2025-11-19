// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.DataStructures.PartitionGet
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using System;
using System.Collections.Generic;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class PartitionGet : PartitionBase
{
  public DateTime? ModifiedTime { get; set; }

  public string? State { get; set; }

  public string? DataView { get; set; }

  public string? ErrorMessage { get; set; }

  public PartitionGet()
  {
    this.Annotations = new List<KeyValuePair<string, string>>();
    this.ExtendedProperties = new List<ExtendedProperty>();
  }
}
