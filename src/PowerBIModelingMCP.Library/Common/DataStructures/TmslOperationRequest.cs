// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.DataStructures.TmslOperationRequest
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

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
