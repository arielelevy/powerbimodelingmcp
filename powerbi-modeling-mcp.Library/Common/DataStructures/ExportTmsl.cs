// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.DataStructures.ExportTmsl
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using System.ComponentModel;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class ExportTmsl : ExportOptionsBase
{
  [Description("TMSL operation type (Create, CreateOrReplace, Alter, Delete, Refresh)")]
  public string? TmslOperationType { get; set; }

  [Description("Refresh type for TMSL Refresh operations (Full, ClearValues, Calculate, DataOnly, Automatic, Add, Defragment)")]
  public string? RefreshType { get; set; }

  [Description("Whether to include restricted properties in TMSL script (default: false)")]
  public bool? IncludeRestricted { get; set; }

  [Description("Whether to format JSON output for readability (default: true)")]
  public bool FormatJson { get; set; } = true;
}
