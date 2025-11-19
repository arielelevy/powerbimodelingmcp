// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.DataStructures.ExportOptionsBase
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

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
