// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using Microsoft.AnalysisServices.Tabular;
using System.Collections.Generic;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class TableGet : TableBase
{
  public ulong? ID { get; set; }

  public ModeType? Mode { get; set; }

  public List<string> Columns { get; set; } = new List<string>();

  public List<string> Measures { get; set; } = new List<string>();

  public List<string> Hierarchies { get; set; } = new List<string>();

  public List<PartitionGet> PartitionDetails { get; set; } = new List<PartitionGet>();
}
