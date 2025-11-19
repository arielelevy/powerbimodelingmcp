// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.DataStructures.ObjectTranslationBase
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using System.ComponentModel.DataAnnotations;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class ObjectTranslationBase
{
  [Required]
  public required string CultureName { get; set; }

  [Required]
  public required string Property { get; set; }

  [Required]
  public required string ObjectType { get; set; }

  public string? ModelName { get; set; }

  public string? TableName { get; set; }

  public string? MeasureName { get; set; }

  public string? ColumnName { get; set; }

  public string? HierarchyName { get; set; }

  public string? LevelName { get; set; }
}
