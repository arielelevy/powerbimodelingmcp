// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System.ComponentModel;

#nullable disable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class TmslOperationRequest
{
  [Description("The type of TMSL operation to perform")]
  public TmslOperationType OperationType { get; set; }

  [Description("Refresh type for Refresh operations (Full, ClearValues, Calculate, DataOnly, Automatic, Add, Defragment)")]
  public Microsoft.AnalysisServices.Tabular.RefreshType? RefreshType { get; set; }

  [Description("Whether to include restricted properties in the TMSL script")]
  public bool IncludeRestricted { get; set; }
}
