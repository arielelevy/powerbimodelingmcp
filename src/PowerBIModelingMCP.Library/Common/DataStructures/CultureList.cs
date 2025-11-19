// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.DataStructures.CultureList
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using System.ComponentModel;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class CultureList
{
  [Description("Name of the culture")]
  public string? Name { get; set; }

  [Description("Locale Identifier (LCID) for the culture")]
  public int LCID { get; set; }

  [Description("Number of object translations in this culture")]
  public int TranslationCount { get; set; }
}
