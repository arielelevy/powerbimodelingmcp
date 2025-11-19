// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using Microsoft.AnalysisServices.Tabular;
using Microsoft.AnalysisServices.Tabular.Serialization;
using ModelContextProtocol;
using PowerBIModelingMCP.Library.Common.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable
namespace PowerBIModelingMCP.Library.Core;

public static class PerspectiveOperations
{
  public static List<PerspectiveList> ListPerspectives(string? connectionName)
  {
    Microsoft.AnalysisServices.Tabular.Database database = ConnectionOperations.Get(connectionName).Database;
    List<PerspectiveList> perspectiveListList1 = new List<PerspectiveList>();
    foreach (Microsoft.AnalysisServices.Tabular.Perspective perspective in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.Perspective, Microsoft.AnalysisServices.Tabular.Model>) database.Model.Perspectives)
    {
      List<PerspectiveList> perspectiveListList2 = perspectiveListList1;
      PerspectiveList perspectiveList = new PerspectiveList { Name = perspective.Name };
      perspectiveList.Description = !string.IsNullOrEmpty(perspective.Description) ? perspective.Description : (string) null;
      perspectiveList.TableCount = perspective.PerspectiveTables.Count > 0 ? new int?(perspective.PerspectiveTables.Count) : new int?();
      perspectiveList.MeasureCount = Enumerable.Sum<Microsoft.AnalysisServices.Tabular.PerspectiveTable>((IEnumerable<Microsoft.AnalysisServices.Tabular.PerspectiveTable>) perspective.PerspectiveTables, (pt => pt.PerspectiveMeasures.Count)) > 0 ? new int?(Enumerable.Sum<Microsoft.AnalysisServices.Tabular.PerspectiveTable>((IEnumerable<Microsoft.AnalysisServices.Tabular.PerspectiveTable>) perspective.PerspectiveTables, (pt => pt.PerspectiveMeasures.Count))) : new int?();
      perspectiveList.ColumnCount = Enumerable.Sum<Microsoft.AnalysisServices.Tabular.PerspectiveTable>((IEnumerable<Microsoft.AnalysisServices.Tabular.PerspectiveTable>) perspective.PerspectiveTables, (pt => pt.PerspectiveColumns.Count)) > 0 ? new int?(Enumerable.Sum<Microsoft.AnalysisServices.Tabular.PerspectiveTable>((IEnumerable<Microsoft.AnalysisServices.Tabular.PerspectiveTable>) perspective.PerspectiveTables, (pt => pt.PerspectiveColumns.Count))) : new int?();
      perspectiveList.HierarchyCount = Enumerable.Sum<Microsoft.AnalysisServices.Tabular.PerspectiveTable>((IEnumerable<Microsoft.AnalysisServices.Tabular.PerspectiveTable>) perspective.PerspectiveTables, (pt => pt.PerspectiveHierarchies.Count)) > 0 ? new int?(Enumerable.Sum<Microsoft.AnalysisServices.Tabular.PerspectiveTable>((IEnumerable<Microsoft.AnalysisServices.Tabular.PerspectiveTable>) perspective.PerspectiveTables, (pt => pt.PerspectiveHierarchies.Count))) : new int?();
      perspectiveListList2.Add(perspectiveList);
    }
    return perspectiveListList1;
  }

  public static PerspectiveGet GetPerspective(string? connectionName, string perspectiveName)
  {
    if (string.IsNullOrWhiteSpace(perspectiveName))
      throw new McpException("Perspective name is required");
    Microsoft.AnalysisServices.Tabular.Perspective perspective1 = ConnectionOperations.Get(connectionName).Database.Model.Perspectives.Find(perspectiveName);
    if (perspective1 == null)
      throw new McpException($"Perspective '{perspectiveName}' not found");
    PerspectiveGet perspectiveGet = new PerspectiveGet { Name = perspective1.Name };
    perspectiveGet.Description = perspective1.Description;
    perspectiveGet.ModifiedTime = new DateTime?(perspective1.ModifiedTime);
    perspectiveGet.Annotations = new List<KeyValuePair<string, string>>();
    PerspectiveGet perspective2 = perspectiveGet;
    foreach (Microsoft.AnalysisServices.Tabular.Annotation annotation in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.Annotation, Microsoft.AnalysisServices.Tabular.Perspective>) perspective1.Annotations)
      perspective2.Annotations.Add(new KeyValuePair<string, string>(annotation.Name, annotation.Value));
    foreach (Microsoft.AnalysisServices.Tabular.PerspectiveTable perspectiveTable in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.PerspectiveTable, Microsoft.AnalysisServices.Tabular.Perspective>) perspective1.PerspectiveTables)
    {
      PerspectiveTableGet perspectiveTableGet1 = new PerspectiveTableGet { Name = perspectiveTable.Name };
      perspectiveTableGet1.TableName = perspectiveTable.Table?.Name;
      perspectiveTableGet1.Annotations = new List<KeyValuePair<string, string>>();
      PerspectiveTableGet perspectiveTableGet2 = perspectiveTableGet1;
      foreach (Microsoft.AnalysisServices.Tabular.Annotation annotation in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.Annotation, Microsoft.AnalysisServices.Tabular.PerspectiveTable>) perspectiveTable.Annotations)
        perspectiveTableGet2.Annotations.Add(new KeyValuePair<string, string>(annotation.Name, annotation.Value));
      foreach (Microsoft.AnalysisServices.Tabular.PerspectiveColumn perspectiveColumn in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.PerspectiveColumn, Microsoft.AnalysisServices.Tabular.PerspectiveTable>) perspectiveTable.PerspectiveColumns)
      {
        PerspectiveColumnGet perspectiveColumnGet1 = new PerspectiveColumnGet { Name = perspectiveColumn.Name };
        perspectiveColumnGet1.ColumnName = perspectiveColumn.Column?.Name;
        perspectiveColumnGet1.TableName = perspectiveTable.Table?.Name;
        perspectiveColumnGet1.Annotations = new List<KeyValuePair<string, string>>();
        PerspectiveColumnGet perspectiveColumnGet2 = perspectiveColumnGet1;
        foreach (Microsoft.AnalysisServices.Tabular.Annotation annotation in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.Annotation, Microsoft.AnalysisServices.Tabular.PerspectiveColumn>) perspectiveColumn.Annotations)
          perspectiveColumnGet2.Annotations.Add(new KeyValuePair<string, string>(annotation.Name, annotation.Value));
        perspectiveTableGet2.PerspectiveColumns.Add(perspectiveColumnGet2);
        perspective2.Columns.Add(perspectiveColumn.Name);
      }
      foreach (Microsoft.AnalysisServices.Tabular.PerspectiveMeasure perspectiveMeasure in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.PerspectiveMeasure, Microsoft.AnalysisServices.Tabular.PerspectiveTable>) perspectiveTable.PerspectiveMeasures)
      {
        PerspectiveMeasureGet perspectiveMeasureGet1 = new PerspectiveMeasureGet { Name = perspectiveMeasure.Name };
        perspectiveMeasureGet1.MeasureName = perspectiveMeasure.Measure?.Name;
        perspectiveMeasureGet1.TableName = perspectiveTable.Table?.Name;
        perspectiveMeasureGet1.Annotations = new List<KeyValuePair<string, string>>();
        PerspectiveMeasureGet perspectiveMeasureGet2 = perspectiveMeasureGet1;
        foreach (Microsoft.AnalysisServices.Tabular.Annotation annotation in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.Annotation, Microsoft.AnalysisServices.Tabular.PerspectiveMeasure>) perspectiveMeasure.Annotations)
          perspectiveMeasureGet2.Annotations.Add(new KeyValuePair<string, string>(annotation.Name, annotation.Value));
        perspectiveTableGet2.PerspectiveMeasures.Add(perspectiveMeasureGet2);
        perspective2.Measures.Add(perspectiveMeasure.Name);
      }
      foreach (Microsoft.AnalysisServices.Tabular.PerspectiveHierarchy perspectiveHierarchy in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.PerspectiveHierarchy, Microsoft.AnalysisServices.Tabular.PerspectiveTable>) perspectiveTable.PerspectiveHierarchies)
      {
        PerspectiveHierarchyGet perspectiveHierarchyGet1 = new PerspectiveHierarchyGet { Name = perspectiveHierarchy.Name };
        perspectiveHierarchyGet1.HierarchyName = perspectiveHierarchy.Hierarchy?.Name;
        perspectiveHierarchyGet1.TableName = perspectiveTable.Table?.Name;
        perspectiveHierarchyGet1.Annotations = new List<KeyValuePair<string, string>>();
        PerspectiveHierarchyGet perspectiveHierarchyGet2 = perspectiveHierarchyGet1;
        foreach (Microsoft.AnalysisServices.Tabular.Annotation annotation in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.Annotation, Microsoft.AnalysisServices.Tabular.PerspectiveHierarchy>) perspectiveHierarchy.Annotations)
          perspectiveHierarchyGet2.Annotations.Add(new KeyValuePair<string, string>(annotation.Name, annotation.Value));
        perspectiveTableGet2.PerspectiveHierarchies.Add(perspectiveHierarchyGet2);
        perspective2.Hierarchies.Add(perspectiveHierarchy.Name);
      }
      perspective2.PerspectiveTables.Add(perspectiveTableGet2);
      perspective2.Tables.Add(perspectiveTable.Name);
    }
    return perspective2;
  }

  public static PerspectiveOperationResult CreatePerspective(
    string? connectionName,
    PerspectiveCreate def)
  {
    if (def == null)
      throw new McpException("Perspective definition cannot be null");
    if (string.IsNullOrWhiteSpace(def.Name))
      throw new McpException("Perspective name is required");
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Database database = info.Database;
    if (database.Model.Perspectives.Find(def.Name) != null)
      throw new McpException($"Perspective '{def.Name}' already exists");
    Microsoft.AnalysisServices.Tabular.Perspective perspective1 = new Microsoft.AnalysisServices.Tabular.Perspective();
    perspective1.Name = def.Name;
    perspective1.Description = def.Description;
    Microsoft.AnalysisServices.Tabular.Perspective perspective2 = perspective1;
    if (def.Annotations != null)
      AnnotationHelpers.ApplyAnnotations<Microsoft.AnalysisServices.Tabular.Perspective>(perspective2, def.Annotations, (Func<Microsoft.AnalysisServices.Tabular.Perspective, ICollection<Microsoft.AnalysisServices.Tabular.Annotation>>) (p => (ICollection<Microsoft.AnalysisServices.Tabular.Annotation>) p.Annotations));
    database.Model.Perspectives.Add(perspective2);
    TransactionOperations.RecordOperation(info, $"Created perspective '{def.Name}'");
    ConnectionOperations.SaveChangesWithRollback(info, "create perspective");
    return new PerspectiveOperationResult()
    {
      Success = true,
      PerspectiveName = def.Name,
      Message = $"Perspective '{def.Name}' created successfully"
    };
  }

  public static PerspectiveOperationResult UpdatePerspective(
    string? connectionName,
    string perspectiveName,
    PerspectiveUpdate update)
  {
    if (string.IsNullOrWhiteSpace(perspectiveName))
      throw new McpException("Perspective name is required");
    if (update == null)
      throw new McpException("Update definition cannot be null");
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Perspective target = info.Database.Model.Perspectives.Find(perspectiveName);
    if (target == null)
      throw new McpException($"Perspective '{perspectiveName}' not found");
    bool flag = false;
    if (update.Description != null)
    {
      string description = string.IsNullOrEmpty(update.Description) ? (string) null : update.Description;
      if ((target.Description != description))
      {
        target.Description = description;
        flag = true;
      }
    }
    if (update.Annotations != null && AnnotationHelpers.ReplaceAnnotations<Microsoft.AnalysisServices.Tabular.Perspective>(target, update.Annotations, (Func<Microsoft.AnalysisServices.Tabular.Perspective, ICollection<Microsoft.AnalysisServices.Tabular.Annotation>>) (p => (ICollection<Microsoft.AnalysisServices.Tabular.Annotation>) p.Annotations)))
      flag = true;
    if (!flag)
      return new PerspectiveOperationResult()
      {
        Success = true,
        PerspectiveName = perspectiveName,
        Message = $"Perspective '{perspectiveName}' is already in the requested state",
        HasChanges = false
      };
    TransactionOperations.RecordOperation(info, $"Updated perspective '{perspectiveName}'");
    ConnectionOperations.SaveChangesWithRollback(info, "update perspective");
    return new PerspectiveOperationResult()
    {
      Success = true,
      PerspectiveName = perspectiveName,
      Message = $"Perspective '{perspectiveName}' updated successfully",
      HasChanges = true
    };
  }

  public static PerspectiveOperationResult DeletePerspective(
    string? connectionName,
    string perspectiveName)
  {
    if (string.IsNullOrWhiteSpace(perspectiveName))
      throw new McpException("Perspective name is required");
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Database database = info.Database;
    database.Model.Perspectives.Remove(database.Model.Perspectives.Find(perspectiveName) ?? throw new McpException($"Perspective '{perspectiveName}' not found"));
    TransactionOperations.RecordOperation(info, $"Deleted perspective '{perspectiveName}'");
    ConnectionOperations.SaveChangesWithRollback(info, "delete perspective");
    return new PerspectiveOperationResult()
    {
      Success = true,
      PerspectiveName = perspectiveName,
      Message = $"Perspective '{perspectiveName}' deleted successfully"
    };
  }

  public static PerspectiveOperationResult RenamePerspective(
    string? connectionName,
    string oldName,
    string newName)
  {
    if (string.IsNullOrWhiteSpace(oldName))
      throw new McpException("Old perspective name is required");
    if (string.IsNullOrWhiteSpace(newName))
      throw new McpException("New perspective name is required");
    if ((oldName == newName))
      throw new McpException("New name must be different from current name");
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Database database = info.Database;
    Microsoft.AnalysisServices.Tabular.Perspective perspective = database.Model.Perspectives.Find(oldName);
    if (perspective == null)
      throw new McpException($"Perspective '{oldName}' not found");
    if (database.Model.Perspectives.Find(newName) != null && !string.Equals(oldName, newName, StringComparison.OrdinalIgnoreCase))
      throw new McpException($"Perspective '{newName}' already exists");
    perspective.RequestRename(newName);
    TransactionOperations.RecordOperation(info, $"Renamed perspective '{oldName}' to '{newName}'");
    ConnectionOperations.SaveChangesWithRollback(info, "rename perspective", ConnectionOperations.CheckpointMode.AfterRequestRename);
    PerspectiveOperationResult perspectiveOperationResult = new PerspectiveOperationResult { Success = true };
    perspectiveOperationResult.PerspectiveName = newName;
    perspectiveOperationResult.Message = $"Perspective renamed from '{oldName}' to '{newName}' successfully";
    return perspectiveOperationResult;
  }

  public static List<Dictionary<string, string>> ListPerspectiveTables(
    string? connectionName,
    string perspectiveName)
  {
    if (string.IsNullOrWhiteSpace(perspectiveName))
      throw new McpException("Perspective name is required");
    Microsoft.AnalysisServices.Tabular.Perspective perspective = ConnectionOperations.Get(connectionName).Database.Model.Perspectives.Find(perspectiveName);
    if (perspective == null)
      throw new McpException($"Perspective '{perspectiveName}' not found");
    List<Dictionary<string, string>> dictionaryList = new List<Dictionary<string, string>>();
    foreach (Microsoft.AnalysisServices.Tabular.PerspectiveTable perspectiveTable in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.PerspectiveTable, Microsoft.AnalysisServices.Tabular.Perspective>) perspective.PerspectiveTables)
      dictionaryList.Add(new Dictionary<string, string>()
      {
        ["Name"] = perspectiveTable.Name,
        ["TableName"] = perspectiveTable.Table?.Name ?? "",
        ["ModifiedTime"] = perspectiveTable.ModifiedTime.ToString("yyyy-MM-dd HH:mm:ss"),
        ["IncludeAll"] = perspectiveTable.IncludeAll.ToString(),
        ["ColumnCount"] = perspectiveTable.PerspectiveColumns.Count.ToString(),
        ["MeasureCount"] = perspectiveTable.PerspectiveMeasures.Count.ToString(),
        ["HierarchyCount"] = perspectiveTable.PerspectiveHierarchies.Count.ToString()
      });
    return dictionaryList;
  }

  public static PerspectiveTableGet GetPerspectiveTable(
    string? connectionName,
    string perspectiveName,
    string tableName)
  {
    if (string.IsNullOrWhiteSpace(perspectiveName))
      throw new McpException("Perspective name is required");
    if (string.IsNullOrWhiteSpace(tableName))
      throw new McpException("Table name is required");
    Microsoft.AnalysisServices.Tabular.PerspectiveTable perspectiveTable1 = (ConnectionOperations.Get(connectionName).Database.Model.Perspectives.Find(perspectiveName) ?? throw new McpException($"Perspective '{perspectiveName}' not found")).PerspectiveTables.Find(tableName);
    if (perspectiveTable1 == null)
      throw new McpException($"Table '{tableName}' not found in perspective '{perspectiveName}'");
    PerspectiveTableGet perspectiveTableGet = new PerspectiveTableGet { Name = perspectiveTable1.Name };
    perspectiveTableGet.TableName = perspectiveTable1.Table?.Name;
    perspectiveTableGet.ModifiedTime = new DateTime?(perspectiveTable1.ModifiedTime);
    perspectiveTableGet.IncludeAll = new bool?(perspectiveTable1.IncludeAll);
    perspectiveTableGet.Annotations = new List<KeyValuePair<string, string>>();
    PerspectiveTableGet perspectiveTable2 = perspectiveTableGet;
    foreach (Microsoft.AnalysisServices.Tabular.Annotation annotation in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.Annotation, Microsoft.AnalysisServices.Tabular.PerspectiveTable>) perspectiveTable1.Annotations)
      perspectiveTable2.Annotations.Add(new KeyValuePair<string, string>(annotation.Name, annotation.Value));
    foreach (Microsoft.AnalysisServices.Tabular.PerspectiveColumn perspectiveColumn in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.PerspectiveColumn, Microsoft.AnalysisServices.Tabular.PerspectiveTable>) perspectiveTable1.PerspectiveColumns)
    {
      PerspectiveColumnGet perspectiveColumnGet1 = new PerspectiveColumnGet { Name = perspectiveColumn.Name };
      perspectiveColumnGet1.ColumnName = perspectiveColumn.Column?.Name;
      perspectiveColumnGet1.TableName = perspectiveTable1.Table?.Name;
      perspectiveColumnGet1.Annotations = new List<KeyValuePair<string, string>>();
      PerspectiveColumnGet perspectiveColumnGet2 = perspectiveColumnGet1;
      foreach (Microsoft.AnalysisServices.Tabular.Annotation annotation in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.Annotation, Microsoft.AnalysisServices.Tabular.PerspectiveColumn>) perspectiveColumn.Annotations)
        perspectiveColumnGet2.Annotations.Add(new KeyValuePair<string, string>(annotation.Name, annotation.Value));
      perspectiveTable2.PerspectiveColumns.Add(perspectiveColumnGet2);
    }
    foreach (Microsoft.AnalysisServices.Tabular.PerspectiveMeasure perspectiveMeasure in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.PerspectiveMeasure, Microsoft.AnalysisServices.Tabular.PerspectiveTable>) perspectiveTable1.PerspectiveMeasures)
    {
      PerspectiveMeasureGet perspectiveMeasureGet1 = new PerspectiveMeasureGet { Name = perspectiveMeasure.Name };
      perspectiveMeasureGet1.MeasureName = perspectiveMeasure.Measure?.Name;
      perspectiveMeasureGet1.TableName = perspectiveTable1.Table?.Name;
      perspectiveMeasureGet1.Annotations = new List<KeyValuePair<string, string>>();
      PerspectiveMeasureGet perspectiveMeasureGet2 = perspectiveMeasureGet1;
      foreach (Microsoft.AnalysisServices.Tabular.Annotation annotation in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.Annotation, Microsoft.AnalysisServices.Tabular.PerspectiveMeasure>) perspectiveMeasure.Annotations)
        perspectiveMeasureGet2.Annotations.Add(new KeyValuePair<string, string>(annotation.Name, annotation.Value));
      perspectiveTable2.PerspectiveMeasures.Add(perspectiveMeasureGet2);
    }
    foreach (Microsoft.AnalysisServices.Tabular.PerspectiveHierarchy perspectiveHierarchy in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.PerspectiveHierarchy, Microsoft.AnalysisServices.Tabular.PerspectiveTable>) perspectiveTable1.PerspectiveHierarchies)
    {
      PerspectiveHierarchyGet perspectiveHierarchyGet1 = new PerspectiveHierarchyGet { Name = perspectiveHierarchy.Name };
      perspectiveHierarchyGet1.HierarchyName = perspectiveHierarchy.Hierarchy?.Name;
      perspectiveHierarchyGet1.TableName = perspectiveTable1.Table?.Name;
      perspectiveHierarchyGet1.Annotations = new List<KeyValuePair<string, string>>();
      PerspectiveHierarchyGet perspectiveHierarchyGet2 = perspectiveHierarchyGet1;
      foreach (Microsoft.AnalysisServices.Tabular.Annotation annotation in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.Annotation, Microsoft.AnalysisServices.Tabular.PerspectiveHierarchy>) perspectiveHierarchy.Annotations)
        perspectiveHierarchyGet2.Annotations.Add(new KeyValuePair<string, string>(annotation.Name, annotation.Value));
      perspectiveTable2.PerspectiveHierarchies.Add(perspectiveHierarchyGet2);
    }
    return perspectiveTable2;
  }

  public static PerspectiveOperationResult AddTableToPerspective(
    string? connectionName,
    string perspectiveName,
    PerspectiveTableCreate def)
  {
    if (string.IsNullOrWhiteSpace(perspectiveName))
      throw new McpException("Perspective name is required");
    if (def == null)
      throw new McpException("Perspective table definition cannot be null");
    if (string.IsNullOrWhiteSpace(def.TableName))
      throw new McpException("Table name is required");
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Database database = info.Database;
    Microsoft.AnalysisServices.Tabular.Perspective perspective1 = database.Model.Perspectives.Find(perspectiveName);
    if (perspective1 == null)
      throw new McpException($"Perspective '{perspectiveName}' not found");
    Microsoft.AnalysisServices.Tabular.Table table = database.Model.Tables.Find(def.TableName);
    if (table == null)
      throw new McpException($"Table '{def.TableName}' not found in model");
    if (perspective1.PerspectiveTables.Find(def.TableName) != null)
      throw new McpException($"Table '{def.TableName}' already exists in perspective '{perspectiveName}'");
    Microsoft.AnalysisServices.Tabular.PerspectiveTable perspectiveTable = new Microsoft.AnalysisServices.Tabular.PerspectiveTable()
    {
      Table = table,
      IncludeAll = def.IncludeAll.GetValueOrDefault()
    };
    if (def.Annotations != null)
      AnnotationHelpers.ApplyAnnotations<Microsoft.AnalysisServices.Tabular.PerspectiveTable>(perspectiveTable, def.Annotations, (Func<Microsoft.AnalysisServices.Tabular.PerspectiveTable, ICollection<Microsoft.AnalysisServices.Tabular.Annotation>>) (pt => (ICollection<Microsoft.AnalysisServices.Tabular.Annotation>) pt.Annotations));
    perspective1.PerspectiveTables.Add(perspectiveTable);
    TransactionOperations.RecordOperation(info, $"Added table '{def.TableName}' to perspective '{perspectiveName}'");
    ConnectionOperations.SaveChangesWithRollback(info, "add table to perspective");
    PerspectiveOperationResult perspective2 = new PerspectiveOperationResult { Success = true };
    perspective2.PerspectiveName = perspectiveName;
    perspective2.Message = $"Table '{def.TableName}' added to perspective '{perspectiveName}' successfully";
    return perspective2;
  }

  public static PerspectiveOperationResult RemoveTableFromPerspective(
    string? connectionName,
    string perspectiveName,
    string tableName)
  {
    if (string.IsNullOrWhiteSpace(perspectiveName))
      throw new McpException("Perspective name is required");
    if (string.IsNullOrWhiteSpace(tableName))
      throw new McpException("Table name is required");
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Perspective perspective = info.Database.Model.Perspectives.Find(perspectiveName);
    if (perspective == null)
      throw new McpException($"Perspective '{perspectiveName}' not found");
    perspective.PerspectiveTables.Remove(perspective.PerspectiveTables.Find(tableName) ?? throw new McpException($"Table '{tableName}' not found in perspective '{perspectiveName}'"));
    TransactionOperations.RecordOperation(info, $"Removed table '{tableName}' from perspective '{perspectiveName}'");
    ConnectionOperations.SaveChangesWithRollback(info, "remove table from perspective");
    PerspectiveOperationResult perspectiveOperationResult = new PerspectiveOperationResult { Success = true };
    perspectiveOperationResult.PerspectiveName = perspectiveName;
    perspectiveOperationResult.Message = $"Table '{tableName}' removed from perspective '{perspectiveName}' successfully";
    return perspectiveOperationResult;
  }

  public static PerspectiveOperationResult UpdatePerspectiveTable(
    string? connectionName,
    string perspectiveName,
    string tableName,
    PerspectiveTableUpdate update)
  {
    if (string.IsNullOrWhiteSpace(perspectiveName))
      throw new McpException("Perspective name is required");
    if (string.IsNullOrWhiteSpace(tableName))
      throw new McpException("Table name is required");
    if (update == null)
      throw new McpException("Update definition cannot be null");
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.PerspectiveTable target = (info.Database.Model.Perspectives.Find(perspectiveName) ?? throw new McpException($"Perspective '{perspectiveName}' not found")).PerspectiveTables.Find(tableName);
    if (target == null)
      throw new McpException($"Table '{tableName}' not found in perspective '{perspectiveName}'");
    bool flag = false;
    bool? includeAll = update.IncludeAll;
    if (includeAll.HasValue)
    {
      int num1 = target.IncludeAll ? 1 : 0;
      includeAll = update.IncludeAll;
      int num2 = includeAll.Value ? 1 : 0;
      if (num1 != num2)
      {
        Microsoft.AnalysisServices.Tabular.PerspectiveTable perspectiveTable = target;
        includeAll = update.IncludeAll;
        int num3 = includeAll.Value ? 1 : 0;
        perspectiveTable.IncludeAll = num3 != 0;
        flag = true;
      }
    }
    if (update.Annotations != null && AnnotationHelpers.ReplaceAnnotations<Microsoft.AnalysisServices.Tabular.PerspectiveTable>(target, update.Annotations, (Func<Microsoft.AnalysisServices.Tabular.PerspectiveTable, ICollection<Microsoft.AnalysisServices.Tabular.Annotation>>) (pt => (ICollection<Microsoft.AnalysisServices.Tabular.Annotation>) pt.Annotations)))
      flag = true;
    if (!flag)
    {
      PerspectiveOperationResult perspectiveOperationResult = new PerspectiveOperationResult { Success = true };
      perspectiveOperationResult.PerspectiveName = perspectiveName;
      perspectiveOperationResult.Message = $"Table '{tableName}' in perspective '{perspectiveName}' is already in the requested state";
      perspectiveOperationResult.HasChanges = false;
      return perspectiveOperationResult;
    }
    TransactionOperations.RecordOperation(info, $"Updated table '{tableName}' in perspective '{perspectiveName}'");
    ConnectionOperations.SaveChangesWithRollback(info, "update perspective table");
    PerspectiveOperationResult perspectiveOperationResult1 = new PerspectiveOperationResult { Success = true };
    perspectiveOperationResult1.PerspectiveName = perspectiveName;
    perspectiveOperationResult1.Message = $"Table '{tableName}' in perspective '{perspectiveName}' updated successfully";
    perspectiveOperationResult1.HasChanges = true;
    return perspectiveOperationResult1;
  }

  public static List<Dictionary<string, string>> ListPerspectiveColumns(
    string? connectionName,
    string perspectiveName,
    string tableName)
  {
    if (string.IsNullOrWhiteSpace(perspectiveName))
      throw new McpException("Perspective name is required");
    if (string.IsNullOrWhiteSpace(tableName))
      throw new McpException("Table name is required");
    Microsoft.AnalysisServices.Tabular.PerspectiveTable perspectiveTable = (ConnectionOperations.Get(connectionName).Database.Model.Perspectives.Find(perspectiveName) ?? throw new McpException($"Perspective '{perspectiveName}' not found")).PerspectiveTables.Find(tableName);
    if (perspectiveTable == null)
      throw new McpException($"Table '{tableName}' not found in perspective '{perspectiveName}'");
    List<Dictionary<string, string>> dictionaryList = new List<Dictionary<string, string>>();
    foreach (Microsoft.AnalysisServices.Tabular.PerspectiveColumn perspectiveColumn in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.PerspectiveColumn, Microsoft.AnalysisServices.Tabular.PerspectiveTable>) perspectiveTable.PerspectiveColumns)
      dictionaryList.Add(new Dictionary<string, string>()
      {
        ["Name"] = perspectiveColumn.Name,
        ["ColumnName"] = perspectiveColumn.Column?.Name ?? "",
        ["TableName"] = perspectiveTable.Table?.Name ?? "",
        ["ModifiedTime"] = perspectiveColumn.ModifiedTime.ToString("yyyy-MM-dd HH:mm:ss"),
        ["DataType"] = perspectiveColumn.Column?.DataType.ToString() ?? "",
        ["IsHidden"] = perspectiveColumn.Column?.IsHidden.ToString() ?? "false"
      });
    return dictionaryList;
  }

  public static PerspectiveColumnGet GetPerspectiveColumn(
    string? connectionName,
    string perspectiveName,
    string tableName,
    string columnName)
  {
    if (string.IsNullOrWhiteSpace(perspectiveName))
      throw new McpException("Perspective name is required");
    if (string.IsNullOrWhiteSpace(tableName))
      throw new McpException("Table name is required");
    if (string.IsNullOrWhiteSpace(columnName))
      throw new McpException("Column name is required");
    Microsoft.AnalysisServices.Tabular.PerspectiveTable perspectiveTable = (ConnectionOperations.Get(connectionName).Database.Model.Perspectives.Find(perspectiveName) ?? throw new McpException($"Perspective '{perspectiveName}' not found")).PerspectiveTables.Find(tableName);
    if (perspectiveTable == null)
      throw new McpException($"Table '{tableName}' not found in perspective '{perspectiveName}'");
    Microsoft.AnalysisServices.Tabular.PerspectiveColumn perspectiveColumn1 = perspectiveTable.PerspectiveColumns.Find(columnName);
    if (perspectiveColumn1 == null)
      throw new McpException($"Column '{columnName}' not found in perspective table '{tableName}'");
    PerspectiveColumnGet perspectiveColumnGet = new PerspectiveColumnGet { Name = perspectiveColumn1.Name };
    perspectiveColumnGet.ColumnName = perspectiveColumn1.Column?.Name;
    perspectiveColumnGet.TableName = perspectiveTable.Table?.Name;
    perspectiveColumnGet.ModifiedTime = new DateTime?(perspectiveColumn1.ModifiedTime);
    perspectiveColumnGet.Annotations = new List<KeyValuePair<string, string>>();
    PerspectiveColumnGet perspectiveColumn2 = perspectiveColumnGet;
    foreach (Microsoft.AnalysisServices.Tabular.Annotation annotation in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.Annotation, Microsoft.AnalysisServices.Tabular.PerspectiveColumn>) perspectiveColumn1.Annotations)
      perspectiveColumn2.Annotations.Add(new KeyValuePair<string, string>(annotation.Name, annotation.Value));
    return perspectiveColumn2;
  }

  public static PerspectiveOperationResult AddColumnToPerspectiveTable(
    string? connectionName,
    string perspectiveName,
    PerspectiveColumnCreate def)
  {
    if (string.IsNullOrWhiteSpace(perspectiveName))
      throw new McpException("Perspective name is required");
    if (def == null)
      throw new McpException("Perspective column definition cannot be null");
    if (string.IsNullOrWhiteSpace(def.TableName))
      throw new McpException("Table name is required");
    if (string.IsNullOrWhiteSpace(def.ColumnName))
      throw new McpException("Column name is required");
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.PerspectiveTable perspectiveTable1 = (info.Database.Model.Perspectives.Find(perspectiveName) ?? throw new McpException($"Perspective '{perspectiveName}' not found")).PerspectiveTables.Find(def.TableName);
    if (perspectiveTable1 == null)
      throw new McpException($"Table '{def.TableName}' not found in perspective '{perspectiveName}'");
    Microsoft.AnalysisServices.Tabular.Column column = perspectiveTable1.Table?.Columns.Find(def.ColumnName);
    if (column == null)
      throw new McpException($"Column '{def.ColumnName}' not found in table '{def.TableName}'");
    if (perspectiveTable1.PerspectiveColumns.Find(def.ColumnName) != null)
      throw new McpException($"Column '{def.ColumnName}' already exists in perspective table '{def.TableName}'");
    Microsoft.AnalysisServices.Tabular.PerspectiveColumn perspectiveColumn = new Microsoft.AnalysisServices.Tabular.PerspectiveColumn()
    {
      Column = column
    };
    if (def.Annotations != null)
      AnnotationHelpers.ApplyAnnotations<Microsoft.AnalysisServices.Tabular.PerspectiveColumn>(perspectiveColumn, def.Annotations, (Func<Microsoft.AnalysisServices.Tabular.PerspectiveColumn, ICollection<Microsoft.AnalysisServices.Tabular.Annotation>>) (pc => (ICollection<Microsoft.AnalysisServices.Tabular.Annotation>) pc.Annotations));
    perspectiveTable1.PerspectiveColumns.Add(perspectiveColumn);
    TransactionOperations.RecordOperation(info, $"Added column '{def.ColumnName}' to perspective table '{def.TableName}' in perspective '{perspectiveName}'");
    ConnectionOperations.SaveChangesWithRollback(info, "add column to perspective table");
    PerspectiveOperationResult perspectiveTable2 = new PerspectiveOperationResult { Success = true };
    perspectiveTable2.PerspectiveName = perspectiveName;
    perspectiveTable2.Message = $"Column '{def.ColumnName}' added to perspective table '{def.TableName}' successfully";
    return perspectiveTable2;
  }

  public static PerspectiveOperationResult RemoveColumnFromPerspectiveTable(
    string? connectionName,
    string perspectiveName,
    string tableName,
    string columnName)
  {
    if (string.IsNullOrWhiteSpace(perspectiveName))
      throw new McpException("Perspective name is required");
    if (string.IsNullOrWhiteSpace(tableName))
      throw new McpException("Table name is required");
    if (string.IsNullOrWhiteSpace(columnName))
      throw new McpException("Column name is required");
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.PerspectiveTable perspectiveTable = (info.Database.Model.Perspectives.Find(perspectiveName) ?? throw new McpException($"Perspective '{perspectiveName}' not found")).PerspectiveTables.Find(tableName);
    if (perspectiveTable == null)
      throw new McpException($"Table '{tableName}' not found in perspective '{perspectiveName}'");
    perspectiveTable.PerspectiveColumns.Remove(perspectiveTable.PerspectiveColumns.Find(columnName) ?? throw new McpException($"Column '{columnName}' not found in perspective table '{tableName}'"));
    TransactionOperations.RecordOperation(info, $"Removed column '{columnName}' from perspective table '{tableName}' in perspective '{perspectiveName}'");
    ConnectionOperations.SaveChangesWithRollback(info, "remove column from perspective table");
    PerspectiveOperationResult perspectiveOperationResult = new PerspectiveOperationResult { Success = true };
    perspectiveOperationResult.PerspectiveName = perspectiveName;
    perspectiveOperationResult.Message = $"Column '{columnName}' removed from perspective table '{tableName}' successfully";
    return perspectiveOperationResult;
  }

  public static List<Dictionary<string, string>> ListPerspectiveMeasures(
    string? connectionName,
    string perspectiveName,
    string tableName)
  {
    if (string.IsNullOrWhiteSpace(perspectiveName))
      throw new McpException("Perspective name is required");
    if (string.IsNullOrWhiteSpace(tableName))
      throw new McpException("Table name is required");
    Microsoft.AnalysisServices.Tabular.PerspectiveTable perspectiveTable = (ConnectionOperations.Get(connectionName).Database.Model.Perspectives.Find(perspectiveName) ?? throw new McpException($"Perspective '{perspectiveName}' not found")).PerspectiveTables.Find(tableName);
    if (perspectiveTable == null)
      throw new McpException($"Table '{tableName}' not found in perspective '{perspectiveName}'");
    List<Dictionary<string, string>> dictionaryList = new List<Dictionary<string, string>>();
    foreach (Microsoft.AnalysisServices.Tabular.PerspectiveMeasure perspectiveMeasure in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.PerspectiveMeasure, Microsoft.AnalysisServices.Tabular.PerspectiveTable>) perspectiveTable.PerspectiveMeasures)
      dictionaryList.Add(new Dictionary<string, string>()
      {
        ["Name"] = perspectiveMeasure.Name,
        ["MeasureName"] = perspectiveMeasure.Measure?.Name ?? "",
        ["TableName"] = perspectiveTable.Table?.Name ?? "",
        ["ModifiedTime"] = perspectiveMeasure.ModifiedTime.ToString("yyyy-MM-dd HH:mm:ss"),
        ["DataType"] = perspectiveMeasure.Measure?.DataType.ToString() ?? "",
        ["FormatString"] = perspectiveMeasure.Measure?.FormatString ?? "",
        ["IsHidden"] = perspectiveMeasure.Measure?.IsHidden.ToString() ?? "false",
        ["Expression"] = perspectiveMeasure.Measure?.Expression ?? ""
      });
    return dictionaryList;
  }

  public static PerspectiveMeasureGet GetPerspectiveMeasure(
    string? connectionName,
    string perspectiveName,
    string tableName,
    string measureName)
  {
    if (string.IsNullOrWhiteSpace(perspectiveName))
      throw new McpException("Perspective name is required");
    if (string.IsNullOrWhiteSpace(tableName))
      throw new McpException("Table name is required");
    if (string.IsNullOrWhiteSpace(measureName))
      throw new McpException("Measure name is required");
    Microsoft.AnalysisServices.Tabular.PerspectiveTable perspectiveTable = (ConnectionOperations.Get(connectionName).Database.Model.Perspectives.Find(perspectiveName) ?? throw new McpException($"Perspective '{perspectiveName}' not found")).PerspectiveTables.Find(tableName);
    if (perspectiveTable == null)
      throw new McpException($"Table '{tableName}' not found in perspective '{perspectiveName}'");
    Microsoft.AnalysisServices.Tabular.PerspectiveMeasure perspectiveMeasure1 = perspectiveTable.PerspectiveMeasures.Find(measureName);
    if (perspectiveMeasure1 == null)
      throw new McpException($"Measure '{measureName}' not found in perspective table '{tableName}'");
    PerspectiveMeasureGet perspectiveMeasureGet = new PerspectiveMeasureGet { Name = perspectiveMeasure1.Name };
    perspectiveMeasureGet.MeasureName = perspectiveMeasure1.Measure?.Name;
    perspectiveMeasureGet.TableName = perspectiveTable.Table?.Name;
    perspectiveMeasureGet.ModifiedTime = new DateTime?(perspectiveMeasure1.ModifiedTime);
    perspectiveMeasureGet.Annotations = new List<KeyValuePair<string, string>>();
    PerspectiveMeasureGet perspectiveMeasure2 = perspectiveMeasureGet;
    foreach (Microsoft.AnalysisServices.Tabular.Annotation annotation in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.Annotation, Microsoft.AnalysisServices.Tabular.PerspectiveMeasure>) perspectiveMeasure1.Annotations)
      perspectiveMeasure2.Annotations.Add(new KeyValuePair<string, string>(annotation.Name, annotation.Value));
    return perspectiveMeasure2;
  }

  public static PerspectiveOperationResult AddMeasureToPerspectiveTable(
    string? connectionName,
    string perspectiveName,
    PerspectiveMeasureCreate def)
  {
    if (string.IsNullOrWhiteSpace(perspectiveName))
      throw new McpException("Perspective name is required");
    if (def == null)
      throw new McpException("Perspective measure definition cannot be null");
    if (string.IsNullOrWhiteSpace(def.TableName))
      throw new McpException("Table name is required");
    if (string.IsNullOrWhiteSpace(def.MeasureName))
      throw new McpException("Measure name is required");
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.PerspectiveTable perspectiveTable1 = (info.Database.Model.Perspectives.Find(perspectiveName) ?? throw new McpException($"Perspective '{perspectiveName}' not found")).PerspectiveTables.Find(def.TableName);
    if (perspectiveTable1 == null)
      throw new McpException($"Table '{def.TableName}' not found in perspective '{perspectiveName}'");
    Microsoft.AnalysisServices.Tabular.Measure measure = perspectiveTable1.Table?.Measures.Find(def.MeasureName);
    if (measure == null)
      throw new McpException($"Measure '{def.MeasureName}' not found in table '{def.TableName}'");
    if (perspectiveTable1.PerspectiveMeasures.Find(def.MeasureName) != null)
      throw new McpException($"Measure '{def.MeasureName}' already exists in perspective table '{def.TableName}'");
    Microsoft.AnalysisServices.Tabular.PerspectiveMeasure perspectiveMeasure = new Microsoft.AnalysisServices.Tabular.PerspectiveMeasure()
    {
      Measure = measure
    };
    if (def.Annotations != null)
      AnnotationHelpers.ApplyAnnotations<Microsoft.AnalysisServices.Tabular.PerspectiveMeasure>(perspectiveMeasure, def.Annotations, (Func<Microsoft.AnalysisServices.Tabular.PerspectiveMeasure, ICollection<Microsoft.AnalysisServices.Tabular.Annotation>>) (pm => (ICollection<Microsoft.AnalysisServices.Tabular.Annotation>) pm.Annotations));
    perspectiveTable1.PerspectiveMeasures.Add(perspectiveMeasure);
    TransactionOperations.RecordOperation(info, $"Added measure '{def.MeasureName}' to perspective table '{def.TableName}' in perspective '{perspectiveName}'");
    ConnectionOperations.SaveChangesWithRollback(info, "add measure to perspective table");
    PerspectiveOperationResult perspectiveTable2 = new PerspectiveOperationResult { Success = true };
    perspectiveTable2.PerspectiveName = perspectiveName;
    perspectiveTable2.Message = $"Measure '{def.MeasureName}' added to perspective table '{def.TableName}' successfully";
    return perspectiveTable2;
  }

  public static PerspectiveOperationResult RemoveMeasureFromPerspectiveTable(
    string? connectionName,
    string perspectiveName,
    string tableName,
    string measureName)
  {
    if (string.IsNullOrWhiteSpace(perspectiveName))
      throw new McpException("Perspective name is required");
    if (string.IsNullOrWhiteSpace(tableName))
      throw new McpException("Table name is required");
    if (string.IsNullOrWhiteSpace(measureName))
      throw new McpException("Measure name is required");
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.PerspectiveTable perspectiveTable = (info.Database.Model.Perspectives.Find(perspectiveName) ?? throw new McpException($"Perspective '{perspectiveName}' not found")).PerspectiveTables.Find(tableName);
    if (perspectiveTable == null)
      throw new McpException($"Table '{tableName}' not found in perspective '{perspectiveName}'");
    perspectiveTable.PerspectiveMeasures.Remove(perspectiveTable.PerspectiveMeasures.Find(measureName) ?? throw new McpException($"Measure '{measureName}' not found in perspective table '{tableName}'"));
    TransactionOperations.RecordOperation(info, $"Removed measure '{measureName}' from perspective table '{tableName}' in perspective '{perspectiveName}'");
    ConnectionOperations.SaveChangesWithRollback(info, "remove measure from perspective table");
    PerspectiveOperationResult perspectiveOperationResult = new PerspectiveOperationResult { Success = true };
    perspectiveOperationResult.PerspectiveName = perspectiveName;
    perspectiveOperationResult.Message = $"Measure '{measureName}' removed from perspective table '{tableName}' successfully";
    return perspectiveOperationResult;
  }

  public static List<Dictionary<string, string>> ListPerspectiveHierarchies(
    string? connectionName,
    string perspectiveName,
    string tableName)
  {
    if (string.IsNullOrWhiteSpace(perspectiveName))
      throw new McpException("Perspective name is required");
    if (string.IsNullOrWhiteSpace(tableName))
      throw new McpException("Table name is required");
    Microsoft.AnalysisServices.Tabular.PerspectiveTable perspectiveTable = (ConnectionOperations.Get(connectionName).Database.Model.Perspectives.Find(perspectiveName) ?? throw new McpException($"Perspective '{perspectiveName}' not found")).PerspectiveTables.Find(tableName);
    if (perspectiveTable == null)
      throw new McpException($"Table '{tableName}' not found in perspective '{perspectiveName}'");
    List<Dictionary<string, string>> dictionaryList = new List<Dictionary<string, string>>();
    foreach (Microsoft.AnalysisServices.Tabular.PerspectiveHierarchy perspectiveHierarchy in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.PerspectiveHierarchy, Microsoft.AnalysisServices.Tabular.PerspectiveTable>) perspectiveTable.PerspectiveHierarchies)
      dictionaryList.Add(new Dictionary<string, string>()
      {
        ["Name"] = perspectiveHierarchy.Name,
        ["HierarchyName"] = perspectiveHierarchy.Hierarchy?.Name ?? "",
        ["TableName"] = perspectiveTable.Table?.Name ?? "",
        ["ModifiedTime"] = perspectiveHierarchy.ModifiedTime.ToString("yyyy-MM-dd HH:mm:ss"),
        ["IsHidden"] = perspectiveHierarchy.Hierarchy?.IsHidden.ToString() ?? "false",
        ["LevelCount"] = perspectiveHierarchy.Hierarchy?.Levels.Count.ToString() ?? "0",
        ["DisplayFolder"] = perspectiveHierarchy.Hierarchy?.DisplayFolder ?? "",
        ["Description"] = perspectiveHierarchy.Hierarchy?.Description ?? ""
      });
    return dictionaryList;
  }

  public static PerspectiveHierarchyGet GetPerspectiveHierarchy(
    string? connectionName,
    string perspectiveName,
    string tableName,
    string hierarchyName)
  {
    if (string.IsNullOrWhiteSpace(perspectiveName))
      throw new McpException("Perspective name is required");
    if (string.IsNullOrWhiteSpace(tableName))
      throw new McpException("Table name is required");
    if (string.IsNullOrWhiteSpace(hierarchyName))
      throw new McpException("Hierarchy name is required");
    Microsoft.AnalysisServices.Tabular.PerspectiveTable perspectiveTable = (ConnectionOperations.Get(connectionName).Database.Model.Perspectives.Find(perspectiveName) ?? throw new McpException($"Perspective '{perspectiveName}' not found")).PerspectiveTables.Find(tableName);
    if (perspectiveTable == null)
      throw new McpException($"Table '{tableName}' not found in perspective '{perspectiveName}'");
    Microsoft.AnalysisServices.Tabular.PerspectiveHierarchy perspectiveHierarchy1 = perspectiveTable.PerspectiveHierarchies.Find(hierarchyName);
    if (perspectiveHierarchy1 == null)
      throw new McpException($"Hierarchy '{hierarchyName}' not found in perspective table '{tableName}'");
    PerspectiveHierarchyGet perspectiveHierarchyGet = new PerspectiveHierarchyGet { Name = perspectiveHierarchy1.Name };
    perspectiveHierarchyGet.HierarchyName = perspectiveHierarchy1.Hierarchy?.Name;
    perspectiveHierarchyGet.TableName = perspectiveTable.Table?.Name;
    perspectiveHierarchyGet.ModifiedTime = new DateTime?(perspectiveHierarchy1.ModifiedTime);
    perspectiveHierarchyGet.Annotations = new List<KeyValuePair<string, string>>();
    PerspectiveHierarchyGet perspectiveHierarchy2 = perspectiveHierarchyGet;
    foreach (Microsoft.AnalysisServices.Tabular.Annotation annotation in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.Annotation, Microsoft.AnalysisServices.Tabular.PerspectiveHierarchy>) perspectiveHierarchy1.Annotations)
      perspectiveHierarchy2.Annotations.Add(new KeyValuePair<string, string>(annotation.Name, annotation.Value));
    return perspectiveHierarchy2;
  }

  public static PerspectiveOperationResult AddHierarchyToPerspectiveTable(
    string? connectionName,
    string perspectiveName,
    PerspectiveHierarchyCreate def)
  {
    if (string.IsNullOrWhiteSpace(perspectiveName))
      throw new McpException("Perspective name is required");
    if (def == null)
      throw new McpException("Perspective hierarchy definition cannot be null");
    if (string.IsNullOrWhiteSpace(def.TableName))
      throw new McpException("Table name is required");
    if (string.IsNullOrWhiteSpace(def.HierarchyName))
      throw new McpException("Hierarchy name is required");
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.PerspectiveTable perspectiveTable1 = (info.Database.Model.Perspectives.Find(perspectiveName) ?? throw new McpException($"Perspective '{perspectiveName}' not found")).PerspectiveTables.Find(def.TableName);
    if (perspectiveTable1 == null)
      throw new McpException($"Table '{def.TableName}' not found in perspective '{perspectiveName}'");
    Microsoft.AnalysisServices.Tabular.Hierarchy hierarchy = perspectiveTable1.Table?.Hierarchies.Find(def.HierarchyName);
    if (hierarchy == null)
      throw new McpException($"Hierarchy '{def.HierarchyName}' not found in table '{def.TableName}'");
    if (perspectiveTable1.PerspectiveHierarchies.Find(def.HierarchyName) != null)
      throw new McpException($"Hierarchy '{def.HierarchyName}' already exists in perspective table '{def.TableName}'");
    Microsoft.AnalysisServices.Tabular.PerspectiveHierarchy perspectiveHierarchy = new Microsoft.AnalysisServices.Tabular.PerspectiveHierarchy()
    {
      Hierarchy = hierarchy
    };
    if (def.Annotations != null)
      AnnotationHelpers.ApplyAnnotations<Microsoft.AnalysisServices.Tabular.PerspectiveHierarchy>(perspectiveHierarchy, def.Annotations, (Func<Microsoft.AnalysisServices.Tabular.PerspectiveHierarchy, ICollection<Microsoft.AnalysisServices.Tabular.Annotation>>) (ph => (ICollection<Microsoft.AnalysisServices.Tabular.Annotation>) ph.Annotations));
    perspectiveTable1.PerspectiveHierarchies.Add(perspectiveHierarchy);
    TransactionOperations.RecordOperation(info, $"Added hierarchy '{def.HierarchyName}' to perspective table '{def.TableName}' in perspective '{perspectiveName}'");
    ConnectionOperations.SaveChangesWithRollback(info, "add hierarchy to perspective table");
    PerspectiveOperationResult perspectiveTable2 = new PerspectiveOperationResult { Success = true };
    perspectiveTable2.PerspectiveName = perspectiveName;
    perspectiveTable2.Message = $"Hierarchy '{def.HierarchyName}' added to perspective table '{def.TableName}' successfully";
    return perspectiveTable2;
  }

  public static PerspectiveOperationResult RemoveHierarchyFromPerspectiveTable(
    string? connectionName,
    string perspectiveName,
    string tableName,
    string hierarchyName)
  {
    if (string.IsNullOrWhiteSpace(perspectiveName))
      throw new McpException("Perspective name is required");
    if (string.IsNullOrWhiteSpace(tableName))
      throw new McpException("Table name is required");
    if (string.IsNullOrWhiteSpace(hierarchyName))
      throw new McpException("Hierarchy name is required");
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.PerspectiveTable perspectiveTable = (info.Database.Model.Perspectives.Find(perspectiveName) ?? throw new McpException($"Perspective '{perspectiveName}' not found")).PerspectiveTables.Find(tableName);
    if (perspectiveTable == null)
      throw new McpException($"Table '{tableName}' not found in perspective '{perspectiveName}'");
    perspectiveTable.PerspectiveHierarchies.Remove(perspectiveTable.PerspectiveHierarchies.Find(hierarchyName) ?? throw new McpException($"Hierarchy '{hierarchyName}' not found in perspective table '{tableName}'"));
    TransactionOperations.RecordOperation(info, $"Removed hierarchy '{hierarchyName}' from perspective table '{tableName}' in perspective '{perspectiveName}'");
    ConnectionOperations.SaveChangesWithRollback(info, "remove hierarchy from perspective table");
    PerspectiveOperationResult perspectiveOperationResult = new PerspectiveOperationResult { Success = true };
    perspectiveOperationResult.PerspectiveName = perspectiveName;
    perspectiveOperationResult.Message = $"Hierarchy '{hierarchyName}' removed from perspective table '{tableName}' successfully";
    return perspectiveOperationResult;
  }

  public static string ExportTMDL(
    string? connectionName,
    string perspectiveName,
    ExportTmdl? options)
  {
    if (string.IsNullOrWhiteSpace(perspectiveName))
      throw new ArgumentException("Perspective name cannot be null or empty", nameof (perspectiveName));
    Microsoft.AnalysisServices.Tabular.Perspective @object = ConnectionOperations.Get(connectionName).Database.Model.Perspectives.Find(perspectiveName);
    if (@object == null)
      throw new ArgumentException($"Perspective '{perspectiveName}' not found");
    if (options?.SerializationOptions == null)
      return TmdlSerializer.SerializeObject((MetadataObject) @object);
    MetadataSerializationOptions serializationOptions = options.SerializationOptions.ToMetadataSerializationOptions();
    return TmdlSerializer.SerializeObject((MetadataObject) @object, serializationOptions);
  }
}
