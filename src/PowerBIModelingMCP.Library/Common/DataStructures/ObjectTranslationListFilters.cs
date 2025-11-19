// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.DataStructures.ObjectTranslationListFilters
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using System.ComponentModel;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class ObjectTranslationListFilters
{
  [Description("Optional culture name filter for List operation")]
  public string? FilterCultureName { get; set; }

  [Description("Optional object type filter for List operation")]
  public string? FilterObjectType { get; set; }

  [Description("Optional object name filter for List operation")]
  public string? FilterObjectName { get; set; }
}
