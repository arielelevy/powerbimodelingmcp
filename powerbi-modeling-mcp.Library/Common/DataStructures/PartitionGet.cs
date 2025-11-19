// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
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
