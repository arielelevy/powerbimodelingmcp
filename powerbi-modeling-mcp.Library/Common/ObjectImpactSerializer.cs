// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.ObjectImpactSerializer
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using Microsoft.AnalysisServices.Tabular;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

#nullable enable
namespace PowerBIModelingMCP.Library.Common;

public static class ObjectImpactSerializer
{
  public static string? SerializeToString(ObjectImpact? impact)
  {
    if (impact == null)
      return null;

    List<string> stringList = new List<string>();

    if (impact.PropertyChanges != null && impact.PropertyChanges.Any())
    {
      var strings = impact.PropertyChanges.Select(change => "  - " + SerializePropertyChange(change));
      stringList.Add("Property Changes:\n" + string.Join("\n", strings));
    }

    if (impact.RemovedObjects != null && impact.RemovedObjects.Any())
    {
      var strings = impact.RemovedObjects.Select(obj => "  - " + SerializeMetadataObject(obj));
      stringList.Add("Removed Objects:\n" + string.Join("\n", strings));
    }

    if (impact.RemovedSubtreeRoots != null && impact.RemovedSubtreeRoots.Any())
    {
      var strings = impact.RemovedSubtreeRoots.Select(root => "  - " + SerializeRemovedSubtreeEntry(root));
      stringList.Add("Removed Subtree Roots:\n" + string.Join("\n", strings));
    }

    return !stringList.Any() ? null : string.Join("\n\n", stringList);
  }

  private static string SerializePropertyChange(PropertyChangeEntry change)
  {
    if (change == null)
      return "null";
    return $"Property '{change.PropertyName}' changed on {SerializeMetadataObject(change.Object)}";
  }

  private static string SerializeRemovedSubtreeEntry(RemovedSubtreeEntry entry)
  {
    if (entry == null)
      return "null";

    StringBuilder stringBuilder = new StringBuilder();
    PropertyInfo? property1 = entry.GetType().GetProperty("Object");

    if (property1 != null)
    {
      object? obj = property1.GetValue(entry);
      if (obj != null)
      {
        return $"Removed subtree for {SerializeMetadataObject(obj as MetadataObject)}";
      }
    }

    PropertyInfo? property2 = entry.GetType().GetProperty("ID");
    if (property2 != null)
    {
      string? str = property2.GetValue(entry)?.ToString();
      if (!string.IsNullOrEmpty(str))
      {
        return $"Removed subtree (ID: {str})";
      }
    }

    return "Removed subtree entry";
  }

  private static string SerializeMetadataObject(MetadataObject? obj)
  {
    if (obj == null)
      return "null";

    StringBuilder stringBuilder = new StringBuilder(obj.GetType().Name ?? "");
    PropertyInfo? property1 = obj.GetType().GetProperty("Name");

    if (property1 != null)
    {
      string? str = property1.GetValue(obj)?.ToString();
      if (!string.IsNullOrEmpty(str))
      {
        stringBuilder.Append($" '{str}'");
      }
    }

    PropertyInfo? property2 = obj.GetType().GetProperty("ID");
    if (property2 != null)
    {
      string? str = property2.GetValue(obj)?.ToString();
      if (!string.IsNullOrEmpty(str))
      {
        stringBuilder.Append($" (ID: {str})");
      }
    }

    switch (obj)
    {
      case Table table:
        int columnsCount = table.Columns?.Count ?? 0;
        int measuresCount = table.Measures?.Count ?? 0;
        stringBuilder.Append($" [Columns: {columnsCount}, Measures: {measuresCount}]");
        break;

      case Column column:
        stringBuilder.Append($" [Type: {column.DataType}, Table: {column.Table?.Name ?? "Unknown"}]");
        break;

      case Measure measure:
        stringBuilder.Append($" [Table: {measure.Table?.Name ?? "Unknown"}]");
        break;

      case Relationship relationship:
        stringBuilder.Append($" [From: {relationship.FromTable?.Name ?? "Unknown"} To: {relationship.ToTable?.Name ?? "Unknown"}]");
        break;

      case Partition partition:
        stringBuilder.Append($" [Table: {partition.Table?.Name ?? "Unknown"}, Type: {partition.SourceType}]");
        break;

      case DataSource dataSource:
        stringBuilder.Append($" [Type: {dataSource.Type}]");
        break;
    }

    return stringBuilder.ToString();
  }
}
