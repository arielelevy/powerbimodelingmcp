// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
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
