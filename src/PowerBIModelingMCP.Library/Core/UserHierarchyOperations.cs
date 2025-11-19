// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Core.UserHierarchyOperations
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using Microsoft.AnalysisServices.Tabular;
using Microsoft.AnalysisServices.Tabular.Serialization;
using ModelContextProtocol;
using PowerBIModelingMCP.Library.Common;
using PowerBIModelingMCP.Library.Common.DataStructures;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

#nullable enable
namespace PowerBIModelingMCP.Library.Core;

public static class UserHierarchyOperations
{
  public static List<HierarchyList> ListHierarchies(string? connectionName, string tableName)
  {
    if (string.IsNullOrWhiteSpace(tableName))
      throw new McpException("tableName is required");
    return Enumerable.ToList<HierarchyList>(Enumerable.Select<Microsoft.AnalysisServices.Tabular.Hierarchy, HierarchyList>((IEnumerable<Microsoft.AnalysisServices.Tabular.Hierarchy>) (ConnectionOperations.Get(connectionName).Database.Model.Tables.Find(tableName) ?? throw new McpException($"Table '{tableName}' not found")).Hierarchies, (h =>
    {
      return new HierarchyList()
      {
        Name = h.Name,
        Description = !string.IsNullOrEmpty(h.Description) ? h.Description : (string) null,
        Levels = Enumerable.ToList<LevelList>(Enumerable.Select<Microsoft.AnalysisServices.Tabular.Level, LevelList>(Enumerable.OrderBy<Microsoft.AnalysisServices.Tabular.Level, int>((IEnumerable<Microsoft.AnalysisServices.Tabular.Level>) h.Levels, (l => l.Ordinal)), (l =>
        {
          return new LevelList()
          {
            Name = l.Name,
            Description = !string.IsNullOrEmpty(l.Description) ? l.Description : (string) null
          };
        }))),
        DisplayFolder = !string.IsNullOrEmpty(h.DisplayFolder) ? h.DisplayFolder : (string) null
      };
    })));
  }

  public static HierarchyGet GetHierarchy(
    string? connectionName,
    string tableName,
    string hierarchyName)
  {
    if (string.IsNullOrWhiteSpace(tableName))
      throw new McpException("tableName is required");
    if (string.IsNullOrWhiteSpace(hierarchyName))
      throw new McpException("hierarchyName is required");
    Microsoft.AnalysisServices.Tabular.Hierarchy hierarchy1 = (ConnectionOperations.Get(connectionName).Database.Model.Tables.Find(tableName) ?? throw new McpException($"Table '{tableName}' not found")).Hierarchies.Find(hierarchyName) ?? throw new McpException($"Hierarchy '{hierarchyName}' not found in table '{tableName}'");
    HierarchyGet hierarchyGet = new HierarchyGet { TableName = tableName };
    hierarchyGet.Name = hierarchy1.Name;
    hierarchyGet.Description = hierarchy1.Description;
    hierarchyGet.IsHidden = new bool?(hierarchy1.IsHidden);
    hierarchyGet.DisplayFolder = hierarchy1.DisplayFolder;
    hierarchyGet.HideMembers = hierarchy1.HideMembers.ToString();
    hierarchyGet.LineageTag = hierarchy1.LineageTag;
    hierarchyGet.SourceLineageTag = hierarchy1.SourceLineageTag;
    hierarchyGet.State = new ObjectState?(hierarchy1.State);
    hierarchyGet.ModifiedTime = new DateTime?(hierarchy1.ModifiedTime);
    hierarchyGet.StructureModifiedTime = new DateTime?(hierarchy1.StructureModifiedTime);
    hierarchyGet.RefreshedTime = new DateTime?(hierarchy1.RefreshedTime);
    HierarchyGet hierarchy2 = hierarchyGet;
    foreach (Microsoft.AnalysisServices.Tabular.Level level in Enumerable.OrderBy<Microsoft.AnalysisServices.Tabular.Level, int>((IEnumerable<Microsoft.AnalysisServices.Tabular.Level>) hierarchy1.Levels, (l => l.Ordinal)))
    {
      LevelGet levelGet1 = new LevelGet { Name = level.Name };
      levelGet1.Description = level.Description;
      levelGet1.Ordinal = new int?(level.Ordinal);
      levelGet1.ColumnName = level.Column?.Name;
      levelGet1.LineageTag = level.LineageTag;
      levelGet1.SourceLineageTag = level.SourceLineageTag;
      levelGet1.ModifiedTime = new DateTime?(level.ModifiedTime);
      LevelGet levelGet2 = levelGet1;
      if (levelGet2.Annotations == null)
        levelGet2.Annotations = new List<KeyValuePair<string, string>>();
      foreach (Microsoft.AnalysisServices.Tabular.Annotation annotation in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.Annotation, Microsoft.AnalysisServices.Tabular.Level>) level.Annotations)
        levelGet2.Annotations.Add(new KeyValuePair<string, string>(annotation.Name ?? string.Empty, annotation.Value ?? string.Empty));
      levelGet2.ExtendedProperties = ExtendedPropertyHelpers.ExtractFromLevel(level);
      hierarchy2.Levels.Add(levelGet2);
    }
    if (hierarchy2.Annotations == null)
      hierarchy2.Annotations = new List<KeyValuePair<string, string>>();
    foreach (Microsoft.AnalysisServices.Tabular.Annotation annotation in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.Annotation, Microsoft.AnalysisServices.Tabular.Hierarchy>) hierarchy1.Annotations)
      hierarchy2.Annotations.Add(new KeyValuePair<string, string>(annotation.Name ?? string.Empty, annotation.Value ?? string.Empty));
    hierarchy2.ExtendedProperties = ExtendedPropertyHelpers.ExtractFromHierarchy(hierarchy1);
    return hierarchy2;
  }

  public static HierarchyOperationResult CreateHierarchy(string? connectionName, HierarchyCreate def)
  {
    UserHierarchyOperations.ValidateHierarchyDefinition((HierarchyBase) def, true);
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Table table = info.Database.Model.Tables.Find(def.TableName) ?? throw new McpException($"Table '{def.TableName}' not found");
    if (table.Hierarchies.Contains(def.Name))
      throw new McpException($"Hierarchy '{def.Name}' already exists in table '{def.TableName}'");
    foreach (LevelCreate level in def.Levels)
    {
      if (table.Columns.Find(level.ColumnName) == null)
        throw new McpException($"Column '{level.ColumnName}' not found in table '{def.TableName}' for level '{level.Name}'");
    }
    Microsoft.AnalysisServices.Tabular.Hierarchy hierarchy1 = new Microsoft.AnalysisServices.Tabular.Hierarchy();
    hierarchy1.Name = def.Name;
    Microsoft.AnalysisServices.Tabular.Hierarchy hierarchy2 = hierarchy1;
    if (!string.IsNullOrWhiteSpace(def.Description))
      hierarchy2.Description = def.Description;
    if (def.IsHidden.HasValue)
      hierarchy2.IsHidden = def.IsHidden.Value;
    if (!string.IsNullOrWhiteSpace(def.DisplayFolder))
      hierarchy2.DisplayFolder = def.DisplayFolder;
    if (!string.IsNullOrWhiteSpace(def.HideMembers))
    {
      HierarchyHideMembersType hierarchyHideMembersType;
      if (Enum.TryParse<HierarchyHideMembersType>(def.HideMembers, true, out hierarchyHideMembersType))
      {
        hierarchy2.HideMembers = hierarchyHideMembersType;
      }
      else
      {
        string[] names = Enum.GetNames(typeof (HierarchyHideMembersType));
        throw new McpException($"Invalid HideMembers '{def.HideMembers}'. Valid values are: {string.Join(", ", names)}");
      }
    }
    if (!string.IsNullOrWhiteSpace(def.LineageTag))
      hierarchy2.LineageTag = def.LineageTag;
    if (!string.IsNullOrWhiteSpace(def.SourceLineageTag))
      hierarchy2.SourceLineageTag = def.SourceLineageTag;
    if (def.Annotations != null)
    {
      foreach (KeyValuePair<string, string> annotation in def.Annotations)
      {
        if (!string.IsNullOrWhiteSpace(annotation.Key))
        {
          HierarchyAnnotationCollection annotations = hierarchy2.Annotations;
          Microsoft.AnalysisServices.Tabular.Annotation metadataObject = new Microsoft.AnalysisServices.Tabular.Annotation();
          metadataObject.Name = annotation.Key;
          metadataObject.Value = annotation.Value;
          annotations.Add(metadataObject);
        }
      }
    }
    if (def.ExtendedProperties != null)
      ExtendedPropertyHelpers.ApplyToHierarchy(hierarchy2, def.ExtendedProperties);
    List<LevelCreate> list = Enumerable.ToList<LevelCreate>((IEnumerable<LevelCreate>) def.Levels);
    if (Enumerable.All<LevelCreate>((IEnumerable<LevelCreate>) list, (l => !l.Ordinal.HasValue)))
    {
      for (int index = 0; index < list.Count; ++index)
        list[index].Ordinal = new int?(index);
    }
    foreach (LevelCreate levelCreate in Enumerable.OrderBy<LevelCreate, int?>((IEnumerable<LevelCreate>) list, (l => l.Ordinal)))
    {
      Microsoft.AnalysisServices.Tabular.Column column = table.Columns.Find(levelCreate.ColumnName);
      Microsoft.AnalysisServices.Tabular.Level level1 = new Microsoft.AnalysisServices.Tabular.Level();
      level1.Name = levelCreate.Name ?? levelCreate.ColumnName;
      level1.Ordinal = levelCreate.Ordinal.Value;
      level1.Column = column;
      Microsoft.AnalysisServices.Tabular.Level level2 = level1;
      if (!string.IsNullOrWhiteSpace(levelCreate.Description))
        level2.Description = levelCreate.Description;
      if (!string.IsNullOrWhiteSpace(levelCreate.LineageTag))
        level2.LineageTag = levelCreate.LineageTag;
      if (!string.IsNullOrWhiteSpace(levelCreate.SourceLineageTag))
        level2.SourceLineageTag = levelCreate.SourceLineageTag;
      if (levelCreate.Annotations != null)
      {
        foreach (KeyValuePair<string, string> annotation in levelCreate.Annotations)
        {
          if (!string.IsNullOrWhiteSpace(annotation.Key))
          {
            LevelAnnotationCollection annotations = level2.Annotations;
            Microsoft.AnalysisServices.Tabular.Annotation metadataObject = new Microsoft.AnalysisServices.Tabular.Annotation();
            metadataObject.Name = annotation.Key;
            metadataObject.Value = annotation.Value;
            annotations.Add(metadataObject);
          }
        }
      }
      if (levelCreate.ExtendedProperties != null)
        ExtendedPropertyHelpers.ApplyToLevel(level2, levelCreate.ExtendedProperties);
      hierarchy2.Levels.Add(level2);
    }
    table.Hierarchies.Add(hierarchy2);
    TransactionOperations.RecordOperation(info, $"Created hierarchy '{def.Name}' with {def.Levels.Count} levels in table '{def.TableName}'");
    ConnectionOperations.SaveChangesWithRollback(info, "create hierarchy");
    return new HierarchyOperationResult()
    {
      State = hierarchy2.State.ToString(),
      HierarchyName = hierarchy2.Name ?? string.Empty,
      TableName = table.Name,
      LevelCount = hierarchy2.Levels.Count,
      LevelNames = Enumerable.ToList<string>(Enumerable.Select<Microsoft.AnalysisServices.Tabular.Level, string>(Enumerable.OrderBy<Microsoft.AnalysisServices.Tabular.Level, int>((IEnumerable<Microsoft.AnalysisServices.Tabular.Level>) hierarchy2.Levels, (l => l.Ordinal)), (l => l.Name)))
    };
  }

  public static HierarchyOperationResult UpdateHierarchy(
    string? connectionName,
    HierarchyUpdate update)
  {
    UserHierarchyOperations.ValidateHierarchyDefinition((HierarchyBase) update, false);
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Table table = info.Database.Model.Tables.Find(update.TableName) ?? throw new McpException($"Table '{update.TableName}' not found");
    Microsoft.AnalysisServices.Tabular.Hierarchy hierarchy1 = table.Hierarchies.Find(update.Name) ?? throw new McpException($"Hierarchy '{update.Name}' not found in table '{update.TableName}'");
    bool flag = false;
    if (update.Description != null)
    {
      string description = string.IsNullOrEmpty(update.Description) ? (string) null : update.Description;
      if ((description != hierarchy1.Description))
      {
        hierarchy1.Description = description;
        flag = true;
      }
    }
    if (update.DisplayFolder != null)
    {
      string displayFolder = string.IsNullOrEmpty(update.DisplayFolder) ? (string) null : update.DisplayFolder;
      if ((displayFolder != hierarchy1.DisplayFolder))
      {
        hierarchy1.DisplayFolder = displayFolder;
        flag = true;
      }
    }
    if (update.LineageTag != null)
    {
      string lineageTag = string.IsNullOrEmpty(update.LineageTag) ? (string) null : update.LineageTag;
      if ((lineageTag != hierarchy1.LineageTag))
      {
        hierarchy1.LineageTag = lineageTag;
        flag = true;
      }
    }
    if (update.SourceLineageTag != null)
    {
      string sourceLineageTag = string.IsNullOrEmpty(update.SourceLineageTag) ? (string) null : update.SourceLineageTag;
      if ((sourceLineageTag != hierarchy1.SourceLineageTag))
      {
        hierarchy1.SourceLineageTag = sourceLineageTag;
        flag = true;
      }
    }
    if (update.IsHidden.HasValue)
    {
      int num1 = hierarchy1.IsHidden ? 1 : 0;
      bool? isHidden = update.IsHidden;
      int num2 = isHidden.Value ? 1 : 0;
      if (num1 != num2)
      {
        Microsoft.AnalysisServices.Tabular.Hierarchy hierarchy2 = hierarchy1;
        isHidden = update.IsHidden;
        int num3 = isHidden.Value ? 1 : 0;
        hierarchy2.IsHidden = num3 != 0;
        flag = true;
      }
    }
    if (!string.IsNullOrWhiteSpace(update.HideMembers))
    {
      HierarchyHideMembersType hierarchyHideMembersType;
      if (Enum.TryParse<HierarchyHideMembersType>(update.HideMembers, true, out hierarchyHideMembersType))
      {
        if (hierarchy1.HideMembers != hierarchyHideMembersType)
        {
          hierarchy1.HideMembers = hierarchyHideMembersType;
          flag = true;
        }
      }
      else
      {
        string[] names = Enum.GetNames(typeof (HierarchyHideMembersType));
        throw new McpException($"Invalid HideMembers '{update.HideMembers}'. Valid values are: {string.Join(", ", names)}");
      }
    }
    if (update.Annotations != null && AnnotationHelpers.ReplaceAnnotations<Microsoft.AnalysisServices.Tabular.Hierarchy>(hierarchy1, update.Annotations, (Func<Microsoft.AnalysisServices.Tabular.Hierarchy, ICollection<Microsoft.AnalysisServices.Tabular.Annotation>>) (obj => (ICollection<Microsoft.AnalysisServices.Tabular.Annotation>) obj.Annotations)))
      flag = true;
    if (update.ExtendedProperties != null)
    {
      int num = hierarchy1.ExtendedProperties.Count > 0 ? 1 : 0;
      ExtendedPropertyHelpers.ReplaceExtendedProperties<Microsoft.AnalysisServices.Tabular.Hierarchy>(hierarchy1, update.ExtendedProperties, (Func<Microsoft.AnalysisServices.Tabular.Hierarchy, ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>>) (obj => (ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>) obj.ExtendedProperties));
      if (num != 0 || update.ExtendedProperties.Count > 0)
        flag = true;
    }
    if (!flag)
      return new HierarchyOperationResult()
      {
        State = hierarchy1.State.ToString(),
        HierarchyName = hierarchy1.Name ?? string.Empty,
        TableName = table.Name,
        LevelCount = hierarchy1.Levels.Count,
        LevelNames = Enumerable.ToList<string>(Enumerable.Select<Microsoft.AnalysisServices.Tabular.Level, string>(Enumerable.OrderBy<Microsoft.AnalysisServices.Tabular.Level, int>((IEnumerable<Microsoft.AnalysisServices.Tabular.Level>) hierarchy1.Levels, (l => l.Ordinal)), (l => l.Name))),
        HasChanges = false
      };
    TransactionOperations.RecordOperation(info, $"Updated hierarchy '{update.Name}' in table '{update.TableName}'");
    ConnectionOperations.SaveChangesWithRollback(info, "update hierarchy");
    return new HierarchyOperationResult()
    {
      State = hierarchy1.State.ToString(),
      HierarchyName = hierarchy1.Name ?? string.Empty,
      TableName = table.Name,
      LevelCount = hierarchy1.Levels.Count,
      LevelNames = Enumerable.ToList<string>(Enumerable.Select<Microsoft.AnalysisServices.Tabular.Level, string>(Enumerable.OrderBy<Microsoft.AnalysisServices.Tabular.Level, int>((IEnumerable<Microsoft.AnalysisServices.Tabular.Level>) hierarchy1.Levels, (l => l.Ordinal)), (l => l.Name))),
      HasChanges = true
    };
  }

  public static void RenameHierarchy(
    string? connectionName,
    string tableName,
    string oldName,
    string newName)
  {
    if (string.IsNullOrWhiteSpace(tableName))
      throw new McpException("tableName is required");
    if (string.IsNullOrWhiteSpace(oldName))
      throw new McpException("oldName is required");
    if (string.IsNullOrWhiteSpace(newName))
      throw new McpException("newName is required");
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Table table = info.Database.Model.Tables.Find(tableName);
    if (table == null)
      throw new McpException($"Table '{tableName}' not found");
    Microsoft.AnalysisServices.Tabular.Hierarchy hierarchy = table.Hierarchies.Find(oldName) ?? throw new McpException($"Hierarchy '{oldName}' not found in table '{tableName}'");
    if (table.Hierarchies.Contains(newName) && !string.Equals(oldName, newName, StringComparison.OrdinalIgnoreCase))
      throw new McpException($"Hierarchy '{newName}' already exists in table '{tableName}'");
    hierarchy.RequestRename(newName);
    TransactionOperations.RecordOperation(info, $"Renamed hierarchy '{oldName}' to '{newName}' in table '{tableName}'");
    ConnectionOperations.SaveChangesWithRollback(info, "rename hierarchy", ConnectionOperations.CheckpointMode.AfterRequestRename);
  }

  public static void DeleteHierarchy(
    string? connectionName,
    string tableName,
    string hierarchyName,
    bool shouldCascadeDelete)
  {
    if (string.IsNullOrWhiteSpace(tableName))
      throw new McpException("tableName is required");
    if (string.IsNullOrWhiteSpace(hierarchyName))
      throw new McpException("hierarchyName is required");
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Database database = info.Database;
    Microsoft.AnalysisServices.Tabular.Table table = database.Model.Tables.Find(tableName) ?? throw new McpException($"Table '{tableName}' not found");
    Microsoft.AnalysisServices.Tabular.Hierarchy metadataObject = table.Hierarchies.Find(hierarchyName) ?? throw new McpException($"Hierarchy '{hierarchyName}' not found in table '{tableName}'");
    List<string> stringList = StructuralDependencyHelper.CheckAndDeleteDependenciesIfRequired(database, (NamedMetadataObject) metadataObject, shouldCascadeDelete);
    if (!shouldCascadeDelete && Enumerable.Any<string>((IEnumerable<string>) stringList))
      throw new McpException($"Cannot delete hierarchy '{hierarchyName}' because it has dependencies: {string.Join(", ", (IEnumerable<string>) stringList)}");
    table.Hierarchies.Remove(metadataObject);
    TransactionOperations.RecordOperation(info, $"Deleted hierarchy '{hierarchyName}' from table '{tableName}'");
    ConnectionOperations.SaveChangesWithRollback(info, "delete hierarchy");
  }

  public static List<string> GetHierarchyColumns(
    string? connectionName,
    string tableName,
    string hierarchyName)
  {
    if (string.IsNullOrWhiteSpace(tableName))
      throw new McpException("tableName is required");
    if (string.IsNullOrWhiteSpace(hierarchyName))
      throw new McpException("hierarchyName is required");
    return Enumerable.ToList<string>(Enumerable.Where<string>(Enumerable.Select<Microsoft.AnalysisServices.Tabular.Level, string>(Enumerable.OrderBy<Microsoft.AnalysisServices.Tabular.Level, int>((IEnumerable<Microsoft.AnalysisServices.Tabular.Level>) ((ConnectionOperations.Get(connectionName).Database.Model.Tables.Find(tableName) ?? throw new McpException($"Table '{tableName}' not found")).Hierarchies.Find(hierarchyName) ?? throw new McpException($"Hierarchy '{hierarchyName}' not found in table '{tableName}'")).Levels, (l => l.Ordinal)), (l => l.Column?.Name)), (c => c != null)));
  }

  public static void AddLevel(
    string? connectionName,
    string tableName,
    string hierarchyName,
    LevelCreate levelDef)
  {
    if (string.IsNullOrWhiteSpace(tableName))
      throw new McpException("tableName is required");
    if (string.IsNullOrWhiteSpace(hierarchyName))
      throw new McpException("hierarchyName is required");
    if (levelDef == null)
      throw new McpException("levelDef is required");
    if (string.IsNullOrWhiteSpace(levelDef.Name))
      throw new McpException("Level name is required");
    if (string.IsNullOrWhiteSpace(levelDef.ColumnName))
      throw new McpException("Level column is required");
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Table table = info.Database.Model.Tables.Find(tableName);
    if (table == null)
      throw new McpException($"Table '{tableName}' not found");
    Microsoft.AnalysisServices.Tabular.Hierarchy hierarchy = table.Hierarchies.Find(hierarchyName) ?? throw new McpException($"Hierarchy '{hierarchyName}' not found in table '{tableName}'");
    Microsoft.AnalysisServices.Tabular.Column column = table.Columns.Find(levelDef.ColumnName) ?? throw new McpException($"Column '{levelDef.ColumnName}' not found in table '{tableName}'");
    if (hierarchy.Levels.Contains(levelDef.Name))
      throw new McpException($"Level '{levelDef.Name}' already exists in hierarchy '{hierarchyName}'");
    TransactionOperations.RecordOperation(info, $"Added level '{levelDef.Name}' to hierarchy '{hierarchyName}' in table '{tableName}'");
    Microsoft.AnalysisServices.Tabular.Level level1 = new Microsoft.AnalysisServices.Tabular.Level();
    level1.Name = levelDef.Name;
    level1.Column = column;
    Microsoft.AnalysisServices.Tabular.Level level = level1;
    if (!string.IsNullOrWhiteSpace(levelDef.Description))
      level.Description = levelDef.Description;
    if (!string.IsNullOrWhiteSpace(levelDef.LineageTag))
      level.LineageTag = levelDef.LineageTag;
    if (!string.IsNullOrWhiteSpace(levelDef.SourceLineageTag))
      level.SourceLineageTag = levelDef.SourceLineageTag;
    hierarchy.Levels.Add(level);
    if (levelDef.Ordinal.HasValue)
    {
      int? ordinal = levelDef.Ordinal;
      if (ordinal.Value >= 0)
      {
        ordinal = levelDef.Ordinal;
        if (ordinal.Value <= hierarchy.Levels.Count - 1)
        {
          Microsoft.AnalysisServices.Tabular.Level level2 = level;
          ordinal = levelDef.Ordinal;
          int num = ordinal.Value;
          level2.Ordinal = num;
          using (IEnumerator<Microsoft.AnalysisServices.Tabular.Level> enumerator = Enumerable.Where<Microsoft.AnalysisServices.Tabular.Level>((IEnumerable<Microsoft.AnalysisServices.Tabular.Level>) hierarchy.Levels, (l => l != level && l.Ordinal >= levelDef.Ordinal.Value)).GetEnumerator())
          {
            while (((IEnumerator) enumerator).MoveNext())
              ++enumerator.Current.Ordinal;
            goto label_35;
          }
        }
      }
      DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(40, 2);
      interpolatedStringHandler.AppendLiteral("Invalid ordinal ");
      ref DefaultInterpolatedStringHandler local = ref interpolatedStringHandler;
      ordinal = levelDef.Ordinal;
      int num1 = ordinal.Value;
      local.AppendFormatted<int>(num1);
      interpolatedStringHandler.AppendLiteral(". Must be between 0 and ");
      interpolatedStringHandler.AppendFormatted<int>(hierarchy.Levels.Count - 1);
      throw new McpException(interpolatedStringHandler.ToStringAndClear());
    }
label_35:
    if (levelDef.Annotations != null)
    {
      foreach (KeyValuePair<string, string> annotation in levelDef.Annotations)
      {
        if (!string.IsNullOrWhiteSpace(annotation.Key))
        {
          LevelAnnotationCollection annotations = level.Annotations;
          Microsoft.AnalysisServices.Tabular.Annotation metadataObject = new Microsoft.AnalysisServices.Tabular.Annotation();
          metadataObject.Name = annotation.Key;
          metadataObject.Value = annotation.Value;
          annotations.Add(metadataObject);
        }
      }
    }
    if (levelDef.ExtendedProperties != null)
      ExtendedPropertyHelpers.ApplyToLevel(level, levelDef.ExtendedProperties);
    ConnectionOperations.SaveChangesWithRollback(info, "add level");
  }

  public static void RenameLevel(
    string? connectionName,
    string tableName,
    string hierarchyName,
    string oldLevelName,
    string newLevelName)
  {
    if (string.IsNullOrWhiteSpace(tableName))
      throw new McpException("tableName is required");
    if (string.IsNullOrWhiteSpace(hierarchyName))
      throw new McpException("hierarchyName is required");
    if (string.IsNullOrWhiteSpace(oldLevelName))
      throw new McpException("oldLevelName is required");
    if (string.IsNullOrWhiteSpace(newLevelName))
      throw new McpException("newLevelName is required");
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Hierarchy hierarchy = (info.Database.Model.Tables.Find(tableName) ?? throw new McpException($"Table '{tableName}' not found")).Hierarchies.Find(hierarchyName);
    if (hierarchy == null)
      throw new McpException($"Hierarchy '{hierarchyName}' not found in table '{tableName}'");
    Microsoft.AnalysisServices.Tabular.Level level = Enumerable.FirstOrDefault<Microsoft.AnalysisServices.Tabular.Level>((IEnumerable<Microsoft.AnalysisServices.Tabular.Level>) hierarchy.Levels, (l => (l.Name == oldLevelName))) ?? throw new McpException($"Level '{oldLevelName}' not found in hierarchy '{hierarchyName}'");
    if (Enumerable.Any<Microsoft.AnalysisServices.Tabular.Level>((IEnumerable<Microsoft.AnalysisServices.Tabular.Level>) hierarchy.Levels, (l => (l.Name == newLevelName))) && !string.Equals(oldLevelName, newLevelName, StringComparison.OrdinalIgnoreCase))
      throw new McpException($"Level '{newLevelName}' already exists in hierarchy '{hierarchyName}'");
    level.RequestRename(newLevelName);
    TransactionOperations.RecordOperation(info, $"Renamed level '{oldLevelName}' to '{newLevelName}' in hierarchy '{hierarchyName}' in table '{tableName}'");
    ConnectionOperations.SaveChangesWithRollback(info, "rename level", ConnectionOperations.CheckpointMode.AfterRequestRename);
  }

  public static void UpdateLevel(
    string? connectionName,
    string tableName,
    string hierarchyName,
    string levelName,
    LevelUpdate update)
  {
    if (string.IsNullOrWhiteSpace(tableName))
      throw new McpException("tableName is required");
    if (string.IsNullOrWhiteSpace(hierarchyName))
      throw new McpException("hierarchyName is required");
    if (string.IsNullOrWhiteSpace(levelName))
      throw new McpException("levelName is required");
    if (update == null)
      throw new McpException("Level update definition cannot be null");
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Table table = info.Database.Model.Tables.Find(tableName) ?? throw new McpException($"Table '{tableName}' not found");
    Microsoft.AnalysisServices.Tabular.Hierarchy hierarchy = table.Hierarchies.Find(hierarchyName) ?? throw new McpException($"Hierarchy '{hierarchyName}' not found in table '{tableName}'");
    Microsoft.AnalysisServices.Tabular.Level level = Enumerable.FirstOrDefault<Microsoft.AnalysisServices.Tabular.Level>((IEnumerable<Microsoft.AnalysisServices.Tabular.Level>) hierarchy.Levels, (l => (l.Name == levelName))) ?? throw new McpException($"Level '{levelName}' not found in hierarchy '{hierarchyName}'");
    bool flag = false;
    if (!string.IsNullOrWhiteSpace(update.Name) && (level.Name != update.Name))
      throw new McpException($"Level name cannot be changed through UpdateLevel. Use RenameLevel to rename level '{levelName}' to '{update.Name}'");
    if (update.Description != null)
    {
      string description = string.IsNullOrEmpty(update.Description) ? (string) null : update.Description;
      if ((description != level.Description))
      {
        level.Description = description;
        flag = true;
      }
    }
    if (update.LineageTag != null)
    {
      string lineageTag = string.IsNullOrEmpty(update.LineageTag) ? (string) null : update.LineageTag;
      if ((lineageTag != level.LineageTag))
      {
        level.LineageTag = lineageTag;
        flag = true;
      }
    }
    if (update.SourceLineageTag != null)
    {
      string sourceLineageTag = string.IsNullOrEmpty(update.SourceLineageTag) ? (string) null : update.SourceLineageTag;
      if ((sourceLineageTag != level.SourceLineageTag))
      {
        level.SourceLineageTag = sourceLineageTag;
        flag = true;
      }
    }
    int? ordinal1 = update.Ordinal;
    if (ordinal1.HasValue)
    {
      int ordinal2 = level.Ordinal;
      ordinal1 = update.Ordinal;
      int num1 = ordinal1.Value;
      if (ordinal2 != num1)
      {
        if (Enumerable.Any<Microsoft.AnalysisServices.Tabular.Level>((IEnumerable<Microsoft.AnalysisServices.Tabular.Level>) hierarchy.Levels, (l => l != level && l.Ordinal == update.Ordinal.Value)))
        {
          DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(50, 2);
          interpolatedStringHandler.AppendLiteral("Level with ordinal ");
          ref DefaultInterpolatedStringHandler local = ref interpolatedStringHandler;
          ordinal1 = update.Ordinal;
          int num2 = ordinal1.Value;
          local.AppendFormatted<int>(num2);
          interpolatedStringHandler.AppendLiteral(" already exists in hierarchy '");
          interpolatedStringHandler.AppendFormatted(hierarchyName);
          interpolatedStringHandler.AppendLiteral("'");
          throw new McpException(interpolatedStringHandler.ToStringAndClear());
        }
        Microsoft.AnalysisServices.Tabular.Level level1 = level;
        ordinal1 = update.Ordinal;
        int num3 = ordinal1.Value;
        level1.Ordinal = num3;
        flag = true;
      }
    }
    if (update.ColumnName != null)
    {
      if (string.IsNullOrEmpty(update.ColumnName))
      {
        if (level.Column != null)
        {
          level.Column = (Microsoft.AnalysisServices.Tabular.Column) null;
          flag = true;
        }
      }
      else
      {
        Microsoft.AnalysisServices.Tabular.Column column = table.Columns.Find(update.ColumnName) ?? throw new McpException($"Column '{update.ColumnName}' not found in table '{tableName}'");
        if (level.Column != column)
        {
          level.Column = column;
          flag = true;
        }
      }
    }
    if (update.Annotations != null && AnnotationHelpers.ReplaceAnnotations<Microsoft.AnalysisServices.Tabular.Level>(level, update.Annotations, (Func<Microsoft.AnalysisServices.Tabular.Level, ICollection<Microsoft.AnalysisServices.Tabular.Annotation>>) (obj => (ICollection<Microsoft.AnalysisServices.Tabular.Annotation>) obj.Annotations)))
      flag = true;
    if (update.ExtendedProperties != null)
    {
      int num = level.ExtendedProperties.Count > 0 ? 1 : 0;
      ExtendedPropertyHelpers.ReplaceExtendedProperties<Microsoft.AnalysisServices.Tabular.Level>(level, update.ExtendedProperties, (Func<Microsoft.AnalysisServices.Tabular.Level, ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>>) (obj => (ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>) obj.ExtendedProperties));
      if (num != 0 || update.ExtendedProperties.Count > 0)
        flag = true;
    }
    if (!flag)
      return;
    TransactionOperations.RecordOperation(info, $"Updated level '{levelName}' in hierarchy '{hierarchyName}' in table '{tableName}'");
    ConnectionOperations.SaveChangesWithRollback(info, "update level");
  }

  public static void RemoveLevel(
    string? connectionName,
    string tableName,
    string hierarchyName,
    string levelName,
    bool shouldCascadeDelete)
  {
    if (string.IsNullOrWhiteSpace(tableName))
      throw new McpException("tableName is required");
    if (string.IsNullOrWhiteSpace(hierarchyName))
      throw new McpException("hierarchyName is required");
    if (string.IsNullOrWhiteSpace(levelName))
      throw new McpException("levelName is required");
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Database database = info.Database;
    Microsoft.AnalysisServices.Tabular.Hierarchy hierarchy = (database.Model.Tables.Find(tableName) ?? throw new McpException($"Table '{tableName}' not found")).Hierarchies.Find(hierarchyName) ?? throw new McpException($"Hierarchy '{hierarchyName}' not found in table '{tableName}'");
    Microsoft.AnalysisServices.Tabular.Level metadataObject = Enumerable.FirstOrDefault<Microsoft.AnalysisServices.Tabular.Level>((IEnumerable<Microsoft.AnalysisServices.Tabular.Level>) hierarchy.Levels, (l => (l.Name == levelName))) ?? throw new McpException($"Level '{levelName}' not found in hierarchy '{hierarchyName}'");
    if (hierarchy.Levels.Count == 1)
      throw new McpException("Cannot remove the last level from a hierarchy. Delete the hierarchy instead.");
    List<string> stringList = StructuralDependencyHelper.CheckAndDeleteDependenciesIfRequired(database, (NamedMetadataObject) metadataObject, shouldCascadeDelete);
    if (!shouldCascadeDelete && Enumerable.Any<string>((IEnumerable<string>) stringList))
      throw new McpException($"Cannot remove level {levelName} because it is used by: {string.Join(", ", (IEnumerable<string>) stringList)}");
    hierarchy.Levels.Remove(metadataObject);
    UserHierarchyOperations.ReorderLevels(connectionName, tableName, hierarchyName, Enumerable.ToList<string>(Enumerable.Select<Microsoft.AnalysisServices.Tabular.Level, string>(Enumerable.OrderBy<Microsoft.AnalysisServices.Tabular.Level, int>((IEnumerable<Microsoft.AnalysisServices.Tabular.Level>) hierarchy.Levels, (l => l.Ordinal)), (l => l.Name))));
    TransactionOperations.RecordOperation(info, $"Removed level '{levelName}' from hierarchy '{hierarchyName}' in table '{tableName}'");
    ConnectionOperations.SaveChangesWithRollback(info, "remove level");
  }

  public static void ReorderLevels(
    string? connectionName,
    string tableName,
    string hierarchyName,
    List<string> levelNamesInOrder)
  {
    if (string.IsNullOrWhiteSpace(tableName))
      throw new McpException("tableName is required");
    if (string.IsNullOrWhiteSpace(hierarchyName))
      throw new McpException("hierarchyName is required");
    if (levelNamesInOrder == null || levelNamesInOrder.Count == 0)
      throw new McpException("levelNamesInOrder cannot be null or empty");
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Hierarchy hierarchy = (info.Database.Model.Tables.Find(tableName) ?? throw new McpException($"Table '{tableName}' not found")).Hierarchies.Find(hierarchyName) ?? throw new McpException($"Hierarchy '{hierarchyName}' not found in table '{tableName}'");
    if (levelNamesInOrder.Count != Enumerable.Count<string>(Enumerable.Distinct<string>((IEnumerable<string>) levelNamesInOrder)))
      throw new McpException("Duplicate level names found in levelNamesInOrder");
    List<string> list = Enumerable.ToList<string>(Enumerable.Select<Microsoft.AnalysisServices.Tabular.Level, string>((IEnumerable<Microsoft.AnalysisServices.Tabular.Level>) hierarchy.Levels, (l => l.Name)));
    if (levelNamesInOrder.Count != list.Count)
      throw new McpException($"Number of levels provided ({levelNamesInOrder.Count}) does not match the number of levels in the hierarchy ({list.Count})");
    foreach (string str in levelNamesInOrder)
    {
      if (!list.Contains(str))
        throw new McpException($"Level '{str}' not found in hierarchy '{hierarchyName}'");
    }
    for (int i = 0; i < levelNamesInOrder.Count; i++)
    {
      Microsoft.AnalysisServices.Tabular.Level level = Enumerable.FirstOrDefault<Microsoft.AnalysisServices.Tabular.Level>((IEnumerable<Microsoft.AnalysisServices.Tabular.Level>) hierarchy.Levels, (l => (l.Name == levelNamesInOrder[i])));
      if (level != null)
        level.Ordinal = i;
    }
    TransactionOperations.RecordOperation(info, $"Reordered levels in hierarchy '{hierarchyName}' in table '{tableName}'");
    ConnectionOperations.SaveChangesWithRollback(info, "reorder levels");
  }

  public static string ExportTMDL(
    string? connectionName,
    string tableName,
    string hierarchyName,
    ExportTmdl? options)
  {
    if (string.IsNullOrWhiteSpace(tableName))
      throw new McpException("tableName is required");
    if (string.IsNullOrWhiteSpace(hierarchyName))
      throw new McpException("hierarchyName is required");
    Microsoft.AnalysisServices.Tabular.Hierarchy @object = (ConnectionOperations.Get(connectionName).Database.Model.Tables.Find(tableName) ?? throw new ArgumentException($"Table '{tableName}' not found")).Hierarchies.Find(hierarchyName);
    if (@object == null)
      throw new ArgumentException($"Hierarchy '{hierarchyName}' not found in table '{tableName}'");
    if (options?.SerializationOptions == null)
      return TmdlSerializer.SerializeObject((MetadataObject) @object);
    MetadataSerializationOptions serializationOptions = options.SerializationOptions.ToMetadataSerializationOptions();
    return TmdlSerializer.SerializeObject((MetadataObject) @object, serializationOptions);
  }

  public static void ValidateHierarchyDefinition(HierarchyBase def, bool isCreate)
  {
    if (def == null)
      throw new McpException("Hierarchy definition cannot be null");
    if (string.IsNullOrWhiteSpace(def.TableName))
      throw new McpException("TableName is required");
    if (string.IsNullOrWhiteSpace(def.Name))
      throw new McpException("Name is required");
    if (isCreate && def is HierarchyCreate hierarchyCreate)
    {
      if (hierarchyCreate.Levels == null || hierarchyCreate.Levels.Count == 0)
        throw new McpException("At least one level must be specified when creating a hierarchy");
      int num1 = Enumerable.Any<LevelCreate>((IEnumerable<LevelCreate>) hierarchyCreate.Levels, (l => l.Ordinal.HasValue)) ? 1 : 0;
      bool flag = Enumerable.All<LevelCreate>((IEnumerable<LevelCreate>) hierarchyCreate.Levels, (l => l.Ordinal.HasValue));
      if (num1 != 0 && !flag)
        throw new McpException("Either all levels must have ordinals specified or none. Mixed ordinals are not allowed.");
      foreach (LevelCreate level in hierarchyCreate.Levels)
      {
        if (string.IsNullOrWhiteSpace(level.Name))
          throw new McpException("Level name is required");
        if (string.IsNullOrWhiteSpace(level.ColumnName))
          throw new McpException($"ColumnName is required for level '{level.Name}'");
        if (level.ExtendedProperties != null)
        {
          List<string> stringList = ExtendedPropertyHelpers.Validate(level.ExtendedProperties);
          if (stringList.Count > 0)
            throw new McpException($"Level '{level.Name}' ExtendedProperties validation failed: {string.Join(", ", (IEnumerable<string>) stringList)}");
        }
        AnnotationHelpers.ValidateAnnotations(level.Annotations, $"Level '{level.Name}'");
      }
      if (flag)
      {
        HashSet<int> intSet1 = new HashSet<int>();
        foreach (LevelCreate level in hierarchyCreate.Levels)
        {
          int? ordinal = level.Ordinal;
          if (ordinal.Value < 0)
            throw new McpException($"Ordinal must be non-negative for level '{level.Name}'");
          HashSet<int> intSet2 = intSet1;
          ordinal = level.Ordinal;
          int num2 = ordinal.Value;
          if (!intSet2.Add(num2))
          {
            DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(24, 1);
            interpolatedStringHandler.AppendLiteral("Duplicate ordinal ");
            ref DefaultInterpolatedStringHandler local = ref interpolatedStringHandler;
            ordinal = level.Ordinal;
            int num3 = ordinal.Value;
            local.AppendFormatted<int>(num3);
            interpolatedStringHandler.AppendLiteral(" found");
            throw new McpException(interpolatedStringHandler.ToStringAndClear());
          }
        }
        List<int> list = Enumerable.ToList<int>(Enumerable.OrderBy<int, int>((IEnumerable<int>) intSet1, (o => o)));
        int num4 = list[0];
        switch (num4)
        {
          case 0:
          case 1:
            for (int index = 0; index < list.Count; ++index)
            {
              if (list[index] != num4 + index)
                throw new McpException($"Level ordinals must be continuous. Missing ordinal {num4 + index}");
            }
            break;
          default:
            throw new McpException("Level ordinals must start from 0 or 1");
        }
      }
    }
    if (!string.IsNullOrWhiteSpace(def.HideMembers) && !Enum.IsDefined(typeof (HierarchyHideMembersType), (object) def.HideMembers))
    {
      string[] names = Enum.GetNames(typeof (HierarchyHideMembersType));
      throw new McpException($"Invalid HideMembers '{def.HideMembers}'. Valid values are: {string.Join(", ", names)}");
    }
    if (def.ExtendedProperties != null)
    {
      List<string> stringList = ExtendedPropertyHelpers.Validate(def.ExtendedProperties);
      if (stringList.Count > 0)
        throw new McpException("ExtendedProperties validation failed: " + string.Join(", ", (IEnumerable<string>) stringList));
    }
    AnnotationHelpers.ValidateAnnotations(def.Annotations);
  }
}
