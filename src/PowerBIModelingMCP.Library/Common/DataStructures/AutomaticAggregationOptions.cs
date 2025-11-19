// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.DataStructures.AutomaticAggregationOptions
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

#nullable disable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class AutomaticAggregationOptions
{
  public long? AggregationTableMaxRows { get; set; }

  public long? AggregationTableSizeLimit { get; set; }

  public long? DetailTableMinRows { get; set; }

  public double? QueryCoverage { get; set; }
}
