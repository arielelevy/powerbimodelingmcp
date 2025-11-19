// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.DataStructures.CalculationItemBase
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using System.Collections.Generic;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class CalculationItemBase
{
  public string? Name { get; set; }

  public string? Description { get; set; }

  public string? Expression { get; set; }

  public int? Ordinal { get; set; }

  public string? FormatStringExpression { get; set; }

  public List<KeyValuePair<string, string>>? Annotations { get; set; }
}
