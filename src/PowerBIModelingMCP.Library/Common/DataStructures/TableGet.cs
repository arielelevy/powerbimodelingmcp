// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.DataStructures.TableGet
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

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
