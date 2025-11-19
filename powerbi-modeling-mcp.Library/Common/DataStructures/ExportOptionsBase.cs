// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System.ComponentModel;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public abstract class ExportOptionsBase
{
  [Description("Optional file path to save the generated content")]
  public string? FilePath { get; set; }

  [Description("Maximum characters to return (-1=no limit, 0=don't return, >0=limit). Default: 10000")]
  public int MaxReturnCharacters { get; set; } = 10000;
}
