// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.DataStructures.KPIDefinition
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using System.Collections.Generic;
using System.ComponentModel;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class KPIDefinition
{
  public string? StatusExpression { get; set; }

  public string? StatusGraphic { get; set; }

  public string? TrendExpression { get; set; }

  public string? TrendGraphic { get; set; }

  public string? TargetExpression { get; set; }

  public string? TargetFormatString { get; set; }

  public string? TargetDescription { get; set; }

  public string? StatusDescription { get; set; }

  public string? TrendDescription { get; set; }

  [Description("Collection of annotations as key-value pairs")]
  public List<KeyValuePair<string, string>>? Annotations { get; set; }
}
