// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.DataStructures.TmslOperationType
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

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
