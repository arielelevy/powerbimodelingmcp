// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System.ComponentModel;

#nullable disable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public enum TmslOperationType
{
  [Description("Generate TMSL Create script")] Create,
  [Description("Generate TMSL CreateOrReplace script")] CreateOrReplace,
  [Description("Generate TMSL Alter script")] Alter,
  [Description("Generate TMSL Delete script")] Delete,
  [Description("Generate TMSL Refresh script")] Refresh,
}
