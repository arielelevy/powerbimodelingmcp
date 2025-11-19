// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using Microsoft.AnalysisServices.Tabular;
using Microsoft.AnalysisServices.Tabular.Serialization;
using ModelContextProtocol;
using PowerBIModelingMCP.Library.Common.DataStructures;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

#nullable enable
namespace PowerBIModelingMCP.Library.Core;

public static class RelationshipOperations
{
  public static void ValidateRelationshipDefinition(RelationshipBase def, bool isCreate)
  {
    if (def == null)
      throw new McpException("Relationship definition cannot be null");
    if (isCreate)
    {
      if (string.IsNullOrWhiteSpace(def.FromTable))
        throw new McpException("FromTable is required");
      if (string.IsNullOrWhiteSpace(def.FromColumn))
        throw new McpException("FromColumn is required");
      if (string.IsNullOrWhiteSpace(def.ToTable))
        throw new McpException("ToTable is required");
      if (string.IsNullOrWhiteSpace(def.ToColumn))
        throw new McpException("ToColumn is required");
    }
    if (!string.IsNullOrWhiteSpace(def.Type) && !Enum.IsDefined(typeof (RelationshipType), (object) def.Type))
    {
      string[] names = Enum.GetNames(typeof (RelationshipType));
      throw new McpException($"Invalid Type '{def.Type}'. Valid values are: {string.Join(", ", names)}");
    }
    if (!string.IsNullOrWhiteSpace(def.CrossFilteringBehavior) && !Enum.IsDefined(typeof (CrossFilteringBehavior), (object) def.CrossFilteringBehavior))
    {
      string[] names = Enum.GetNames(typeof (CrossFilteringBehavior));
      throw new McpException($"Invalid CrossFilteringBehavior '{def.CrossFilteringBehavior}'. Valid values are: {string.Join(", ", names)}");
    }
    if (!string.IsNullOrWhiteSpace(def.SecurityFilteringBehavior) && !Enum.IsDefined(typeof (SecurityFilteringBehavior), (object) def.SecurityFilteringBehavior))
    {
      string[] names = Enum.GetNames(typeof (SecurityFilteringBehavior));
      throw new McpException($"Invalid SecurityFilteringBehavior '{def.SecurityFilteringBehavior}'. Valid values are: {string.Join(", ", names)}");
    }
    if (!string.IsNullOrWhiteSpace(def.JoinOnDateBehavior) && !Enum.IsDefined(typeof (DateTimeRelationshipBehavior), (object) def.JoinOnDateBehavior))
    {
      string[] names = Enum.GetNames(typeof (DateTimeRelationshipBehavior));
      throw new McpException($"Invalid JoinOnDateBehavior '{def.JoinOnDateBehavior}'. Valid values are: {string.Join(", ", names)}");
    }
    if (def.ExtendedProperties != null)
    {
      List<string> stringList = ExtendedPropertyHelpers.Validate(def.ExtendedProperties);
      if (stringList.Count > 0)
        throw new McpException("ExtendedProperties validation failed: " + string.Join(", ", (IEnumerable<string>) stringList));
    }
    AnnotationHelpers.ValidateAnnotations(def.Annotations);
  }

  public static List<RelationshipList> ListRelationships(string? connectionName)
  {
    Microsoft.AnalysisServices.Tabular.Database database = ConnectionOperations.Get(connectionName).Database;
    List<RelationshipList> relationshipListList1 = new List<RelationshipList>();
    foreach (Microsoft.AnalysisServices.Tabular.Relationship relationship in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.Relationship, Microsoft.AnalysisServices.Tabular.Model>) database.Model.Relationships)
    {
      if (relationship is Microsoft.AnalysisServices.Tabular.SingleColumnRelationship columnRelationship)
      {
        List<RelationshipList> relationshipListList2 = relationshipListList1;
        RelationshipList relationshipList = new RelationshipList { Name = columnRelationship.Name };
        relationshipList.FromTable = columnRelationship.FromTable?.Name;
        relationshipList.FromColumn = columnRelationship.FromColumn?.Name;
        relationshipList.ToTable = columnRelationship.ToTable?.Name;
        relationshipList.ToColumn = columnRelationship.ToColumn?.Name;
        relationshipList.IsActive = new bool?(columnRelationship.IsActive);
        relationshipList.CrossFilteringBehavior = columnRelationship.CrossFilteringBehavior.ToString();
        RelationshipEndCardinality relationshipEndCardinality = columnRelationship.FromCardinality;
        relationshipList.FromCardinality = relationshipEndCardinality.ToString();
        relationshipEndCardinality = columnRelationship.ToCardinality;
        relationshipList.ToCardinality = relationshipEndCardinality.ToString();
        relationshipListList2.Add(relationshipList);
      }
      else
      {
        List<RelationshipList> relationshipListList3 = relationshipListList1;
        RelationshipList relationshipList = new RelationshipList { Name = relationship.Name };
        relationshipList.FromTable = relationship.FromTable?.Name;
        relationshipList.FromColumn = "[Multiple Columns]";
        relationshipList.ToTable = relationship.ToTable?.Name;
        relationshipList.ToColumn = "[Multiple Columns]";
        relationshipList.IsActive = new bool?(relationship.IsActive);
        relationshipList.CrossFilteringBehavior = relationship.CrossFilteringBehavior.ToString();
        relationshipListList3.Add(relationshipList);
      }
    }
    return relationshipListList1;
  }

  public static RelationshipGet GetRelationship(string? connectionName, string relationshipName)
  {
    if (string.IsNullOrWhiteSpace(relationshipName))
      throw new McpException("relationshipName is required");
    Microsoft.AnalysisServices.Tabular.Relationship relationship1 = ConnectionOperations.Get(connectionName).Database.Model.Relationships.Find(relationshipName) ?? throw new McpException($"Relationship '{relationshipName}' not found");
    RelationshipGet relationshipGet1 = new RelationshipGet { Name = relationship1.Name };
    relationshipGet1.IsActive = new bool?(relationship1.IsActive);
    relationshipGet1.Type = relationship1.Type.ToString();
    relationshipGet1.CrossFilteringBehavior = relationship1.CrossFilteringBehavior.ToString();
    relationshipGet1.JoinOnDateBehavior = relationship1.JoinOnDateBehavior.ToString();
    relationshipGet1.RelyOnReferentialIntegrity = new bool?(relationship1.RelyOnReferentialIntegrity);
    relationshipGet1.FromTable = relationship1.FromTable?.Name;
    relationshipGet1.ToTable = relationship1.ToTable?.Name;
    relationshipGet1.SecurityFilteringBehavior = relationship1.SecurityFilteringBehavior.ToString();
    relationshipGet1.State = relationship1.State.ToString();
    relationshipGet1.Annotations = new List<KeyValuePair<string, string>>();
    relationshipGet1.ExtendedProperties = new List<PowerBIModelingMCP.Library.Common.DataStructures.ExtendedProperty>();
    RelationshipGet relationship2 = relationshipGet1;
    if (relationship1 is Microsoft.AnalysisServices.Tabular.SingleColumnRelationship columnRelationship)
    {
      relationship2.FromColumn = columnRelationship.FromColumn?.Name;
      relationship2.ToColumn = columnRelationship.ToColumn?.Name;
      RelationshipGet relationshipGet2 = relationship2;
      RelationshipEndCardinality relationshipEndCardinality = columnRelationship.FromCardinality;
      string str1 = relationshipEndCardinality.ToString();
      relationshipGet2.FromCardinality = str1;
      RelationshipGet relationshipGet3 = relationship2;
      relationshipEndCardinality = columnRelationship.ToCardinality;
      string str2 = relationshipEndCardinality.ToString();
      relationshipGet3.ToCardinality = str2;
    }
    else
    {
      relationship2.FromColumn = "[Multiple Columns]";
      relationship2.ToColumn = "[Multiple Columns]";
    }
    foreach (Microsoft.AnalysisServices.Tabular.Annotation annotation in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.Annotation, Microsoft.AnalysisServices.Tabular.Relationship>) relationship1.Annotations)
      relationship2.Annotations.Add(new KeyValuePair<string, string>(annotation.Name, annotation.Value));
    relationship2.ExtendedProperties = ExtendedPropertyHelpers.ExtractFromRelationship(relationship1);
    return relationship2;
  }

  public static string ExportTMDL(
    string? connectionName,
    string relationshipName,
    ExportTmdl? options)
  {
    if (string.IsNullOrWhiteSpace(relationshipName))
      throw new McpException("relationshipName is required");
    Microsoft.AnalysisServices.Tabular.Relationship @object = ConnectionOperations.Get(connectionName).Database.Model.Relationships.Find(relationshipName) ?? throw new McpException($"Relationship '{relationshipName}' not found");
    if (options?.SerializationOptions == null)
      return TmdlSerializer.SerializeObject((MetadataObject) @object);
    MetadataSerializationOptions serializationOptions = options.SerializationOptions.ToMetadataSerializationOptions();
    return TmdlSerializer.SerializeObject((MetadataObject) @object, serializationOptions);
  }

  public static RelationshipOperationResult CreateRelationship(
    string? connectionName,
    RelationshipCreate def)
  {
    RelationshipOperations.ValidateRelationshipDefinition((RelationshipBase) def, true);
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Database database = info.Database;
    Microsoft.AnalysisServices.Tabular.Table fromTable1 = database.Model.Tables.Find(def.FromTable) ?? throw new McpException($"From table '{def.FromTable}' not found");
    Microsoft.AnalysisServices.Tabular.Column fromColumn1 = fromTable1.Columns.Find(def.FromColumn) ?? throw new McpException($"From column '{def.FromColumn}' not found in table '{def.FromTable}'");
    Microsoft.AnalysisServices.Tabular.Table toTable1 = database.Model.Tables.Find(def.ToTable) ?? throw new McpException($"To table '{def.ToTable}' not found");
    Microsoft.AnalysisServices.Tabular.Column toColumn1 = toTable1.Columns.Find(def.ToColumn) ?? throw new McpException($"To column '{def.ToColumn}' not found in table '{def.ToTable}'");
    List<string> warnings = new List<string>();
    (Microsoft.AnalysisServices.Tabular.Table fromTable2, Microsoft.AnalysisServices.Tabular.Column fromColumn2, Microsoft.AnalysisServices.Tabular.Table toTable2, Microsoft.AnalysisServices.Tabular.Column toColumn2) = RelationshipOperations.ValidateAndFixCardinality(def, fromTable1, fromColumn1, toTable1, toColumn1, warnings);
    if (Enumerable.FirstOrDefault<Microsoft.AnalysisServices.Tabular.SingleColumnRelationship>(Enumerable.OfType<Microsoft.AnalysisServices.Tabular.SingleColumnRelationship>((IEnumerable) database.Model.Relationships), (r =>
    {
      if (r.FromTable == fromTable2 && r.FromColumn == fromColumn2 && r.ToTable == toTable2 && r.ToColumn == toColumn2)
        return true;
      return r.FromTable == toTable2 && r.FromColumn == toColumn2 && r.ToTable == fromTable2 && r.ToColumn == fromColumn2;
    })) != null)
      throw new McpException($"A relationship already exists between {fromTable2.Name}[{fromColumn2.Name}] and {toTable2.Name}[{toColumn2.Name}]");
    int num = string.IsNullOrEmpty(def.Name) ? 1 : 0;
    Microsoft.AnalysisServices.Tabular.SingleColumnRelationship columnRelationship1 = new Microsoft.AnalysisServices.Tabular.SingleColumnRelationship();
    string str = def.Name;
    if (str == null)
      str = $"{fromTable2.Name}_{fromColumn2.Name}_{toTable2.Name}_{toColumn2.Name}";
    columnRelationship1.Name = str;
    columnRelationship1.FromColumn = fromColumn2;
    columnRelationship1.ToColumn = toColumn2;
    Microsoft.AnalysisServices.Tabular.SingleColumnRelationship columnRelationship2 = columnRelationship1;
    if (num != 0)
      warnings.Insert(0, $"Relationship name was auto-generated as '{columnRelationship2.Name}' based on table and column names");
    RelationshipOperations.ApplyRelationshipProperties(columnRelationship2, (RelationshipBase) def, database);
    if (columnRelationship2.IsActive && Enumerable.Count<Microsoft.AnalysisServices.Tabular.Relationship>((IEnumerable<Microsoft.AnalysisServices.Tabular.Relationship>) database.Model.Relationships, (r =>
    {
      if (!r.IsActive)
        return false;
      if (r.FromTable == fromTable2 && r.ToTable == toTable2)
        return true;
      return r.FromTable == toTable2 && r.ToTable == fromTable2;
    })) > 0)
      throw new McpException($"An active relationship already exists between tables '{fromTable2.Name}' and '{toTable2.Name}'. " + "Only one active relationship is allowed between two tables. Set IsActive to false or deactivate the existing relationship first.");
    database.Model.Relationships.Add((Microsoft.AnalysisServices.Tabular.Relationship) columnRelationship2);
    TransactionOperations.RecordOperation(info, $"Created relationship '{columnRelationship2.Name}' from {fromTable2.Name}[{fromColumn2.Name}] to {toTable2.Name}[{toColumn2.Name}]");
    ConnectionOperations.SaveChangesWithRollback(info, "create relationship");
    return new RelationshipOperationResult()
    {
      State = columnRelationship2.State.ToString(),
      RelationshipName = columnRelationship2.Name,
      FromTable = fromTable2.Name,
      FromColumn = fromColumn2.Name,
      ToTable = toTable2.Name,
      ToColumn = toColumn2.Name,
      Warnings = warnings.Count > 0 ? warnings : (List<string>) null
    };
  }

  public static RelationshipOperationResult UpdateRelationship(
    string? connectionName,
    RelationshipUpdate update)
  {
    RelationshipOperations.ValidateRelationshipDefinition((RelationshipBase) update, false);
    if (string.IsNullOrWhiteSpace(update.Name))
      throw new McpException("Name is required to identify the relationship to update");
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Database database = info.Database;
    Microsoft.AnalysisServices.Tabular.Relationship relationship1 = database.Model.Relationships.Find(update.Name);
    if (relationship1 == null)
      throw new McpException($"Relationship '{update.Name}' not found");
    if (!(relationship1 is Microsoft.AnalysisServices.Tabular.SingleColumnRelationship relationship2))
      throw new McpException($"Relationship '{update.Name}' is not a SingleColumnRelationship");
    if (!RelationshipOperations.ApplyRelationshipUpdates(relationship2, update, database))
      return new RelationshipOperationResult()
      {
        State = relationship2.State.ToString(),
        RelationshipName = relationship2.Name,
        FromTable = relationship2.FromTable?.Name ?? "",
        FromColumn = relationship2.FromColumn?.Name ?? "",
        ToTable = relationship2.ToTable?.Name ?? "",
        ToColumn = relationship2.ToColumn?.Name ?? "",
        HasChanges = false
      };
    TransactionOperations.RecordOperation(info, $"Updated relationship '{update.Name}'");
    ConnectionOperations.SaveChangesWithRollback(info, "update relationship");
    return new RelationshipOperationResult()
    {
      State = relationship2.State.ToString(),
      RelationshipName = relationship2.Name,
      FromTable = relationship2.FromTable?.Name ?? "",
      FromColumn = relationship2.FromColumn?.Name ?? "",
      ToTable = relationship2.ToTable?.Name ?? "",
      ToColumn = relationship2.ToColumn?.Name ?? "",
      HasChanges = true
    };
  }

  public static void DeleteRelationship(string? connectionName, string relationshipName)
  {
    if (string.IsNullOrWhiteSpace(relationshipName))
      throw new McpException("relationshipName is required");
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Database database = info.Database;
    database.Model.Relationships.Remove(database.Model.Relationships.Find(relationshipName) ?? throw new McpException($"Relationship '{relationshipName}' not found"));
    TransactionOperations.RecordOperation(info, $"Deleted relationship '{relationshipName}'");
    ConnectionOperations.SaveChangesWithRollback(info, "delete relationship");
  }

  public static void RenameRelationship(string? connectionName, string oldName, string newName)
  {
    if (string.IsNullOrWhiteSpace(oldName))
      throw new McpException("oldName is required");
    if (string.IsNullOrWhiteSpace(newName))
      throw new McpException("newName is required");
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Database database = info.Database;
    Microsoft.AnalysisServices.Tabular.Relationship relationship = database.Model.Relationships.Find(oldName) ?? throw new McpException($"Relationship '{oldName}' not found");
    if (database.Model.Relationships.Contains(newName) && !string.Equals(oldName, newName, StringComparison.OrdinalIgnoreCase))
      throw new McpException($"Relationship '{newName}' already exists");
    relationship.RequestRename(newName);
    TransactionOperations.RecordOperation(info, $"Renamed relationship '{oldName}' to '{newName}'");
    ConnectionOperations.SaveChangesWithRollback(info, "rename relationship", ConnectionOperations.CheckpointMode.AfterRequestRename);
  }

  public static RelationshipOperationResult ActivateRelationship(
    string? connectionName,
    string relationshipName)
  {
    if (string.IsNullOrWhiteSpace(relationshipName))
      throw new McpException("relationshipName is required");
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Database database = info.Database;
    Microsoft.AnalysisServices.Tabular.Relationship rel = database.Model.Relationships.Find(relationshipName) ?? throw new McpException($"Relationship '{relationshipName}' not found");
    List<string> stringList = new List<string>();
    if (rel.IsActive)
    {
      string str = $"Relationship '{relationshipName}' is already active";
      stringList.Add(str);
      return new RelationshipOperationResult()
      {
        State = rel.State.ToString(),
        RelationshipName = relationshipName,
        FromTable = rel.FromTable?.Name ?? "",
        FromColumn = rel is Microsoft.AnalysisServices.Tabular.SingleColumnRelationship columnRelationship1 ? columnRelationship1.FromColumn?.Name ?? "" : "",
        ToTable = rel.ToTable?.Name ?? "",
        ToColumn = rel is Microsoft.AnalysisServices.Tabular.SingleColumnRelationship columnRelationship2 ? columnRelationship2.ToColumn?.Name ?? "" : "",
        HasChanges = false,
        Warnings = stringList
      };
    }
    if (Enumerable.Count<Microsoft.AnalysisServices.Tabular.Relationship>((IEnumerable<Microsoft.AnalysisServices.Tabular.Relationship>) database.Model.Relationships, (r =>
    {
      if (r == rel || !r.IsActive)
        return false;
      if (r.FromTable == rel.FromTable && r.ToTable == rel.ToTable)
        return true;
      return r.FromTable == rel.ToTable && r.ToTable == rel.FromTable;
    })) > 0)
      throw new McpException($"Cannot activate relationship '{relationshipName}'. An active relationship already exists between these tables. Deactivate the existing relationship first.");
    rel.IsActive = true;
    TransactionOperations.RecordOperation(info, $"Activated relationship '{relationshipName}'");
    ConnectionOperations.SaveChangesWithRollback(info, "activate relationship");
    return new RelationshipOperationResult()
    {
      State = rel.State.ToString(),
      RelationshipName = relationshipName,
      FromTable = rel.FromTable?.Name ?? "",
      FromColumn = rel is Microsoft.AnalysisServices.Tabular.SingleColumnRelationship columnRelationship3 ? columnRelationship3.FromColumn?.Name ?? "" : "",
      ToTable = rel.ToTable?.Name ?? "",
      ToColumn = rel is Microsoft.AnalysisServices.Tabular.SingleColumnRelationship columnRelationship4 ? columnRelationship4.ToColumn?.Name ?? "" : "",
      HasChanges = true,
      Warnings = stringList.Count > 0 ? stringList : (List<string>) null
    };
  }

  public static RelationshipOperationResult DeactivateRelationship(
    string? connectionName,
    string relationshipName)
  {
    if (string.IsNullOrWhiteSpace(relationshipName))
      throw new McpException("relationshipName is required");
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Relationship relationship = info.Database.Model.Relationships.Find(relationshipName) ?? throw new McpException($"Relationship '{relationshipName}' not found");
    List<string> stringList = new List<string>();
    if (!relationship.IsActive)
    {
      string str = $"Relationship '{relationshipName}' is already inactive";
      stringList.Add(str);
      return new RelationshipOperationResult()
      {
        State = relationship.State.ToString(),
        RelationshipName = relationshipName,
        FromTable = relationship.FromTable?.Name ?? "",
        FromColumn = relationship is Microsoft.AnalysisServices.Tabular.SingleColumnRelationship columnRelationship1 ? columnRelationship1.FromColumn?.Name ?? "" : "",
        ToTable = relationship.ToTable?.Name ?? "",
        ToColumn = relationship is Microsoft.AnalysisServices.Tabular.SingleColumnRelationship columnRelationship2 ? columnRelationship2.ToColumn?.Name ?? "" : "",
        HasChanges = false,
        Warnings = stringList
      };
    }
    relationship.IsActive = false;
    TransactionOperations.RecordOperation(info, $"Deactivated relationship '{relationshipName}'");
    ConnectionOperations.SaveChangesWithRollback(info, "deactivate relationship");
    return new RelationshipOperationResult()
    {
      State = relationship.State.ToString(),
      RelationshipName = relationshipName,
      FromTable = relationship.FromTable?.Name ?? "",
      FromColumn = relationship is Microsoft.AnalysisServices.Tabular.SingleColumnRelationship columnRelationship3 ? columnRelationship3.FromColumn?.Name ?? "" : "",
      ToTable = relationship.ToTable?.Name ?? "",
      ToColumn = relationship is Microsoft.AnalysisServices.Tabular.SingleColumnRelationship columnRelationship4 ? columnRelationship4.ToColumn?.Name ?? "" : "",
      HasChanges = true,
      Warnings = stringList.Count > 0 ? stringList : (List<string>) null
    };
  }

  public static List<string> FindRelationshipsForTable(string? connectionName, string tableName)
  {
    if (string.IsNullOrWhiteSpace(tableName))
      throw new McpException("tableName is required");
    Microsoft.AnalysisServices.Tabular.Database database = ConnectionOperations.Get(connectionName).Database;
    Microsoft.AnalysisServices.Tabular.Table table = database.Model.Tables.Find(tableName) ?? throw new McpException($"Table '{tableName}' not found");
    List<string> relationshipsForTable = new List<string>();
    foreach (Microsoft.AnalysisServices.Tabular.Relationship relationship in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.Relationship, Microsoft.AnalysisServices.Tabular.Model>) database.Model.Relationships)
    {
      if (relationship.FromTable == table || relationship.ToTable == table)
        relationshipsForTable.Add(relationship.Name);
    }
    return relationshipsForTable;
  }

  private static void ApplyRelationshipProperties(
    Microsoft.AnalysisServices.Tabular.SingleColumnRelationship relationship,
    RelationshipBase def,
    Microsoft.AnalysisServices.Tabular.Database db)
  {
    relationship.IsActive = def.IsActive ?? true;
    if (!string.IsNullOrWhiteSpace(def.CrossFilteringBehavior))
    {
      CrossFilteringBehavior filteringBehavior;
      if (Enum.TryParse<CrossFilteringBehavior>(def.CrossFilteringBehavior, true, out filteringBehavior))
        relationship.CrossFilteringBehavior = filteringBehavior;
    }
    else
      relationship.CrossFilteringBehavior = CrossFilteringBehavior.OneDirection;
    DateTimeRelationshipBehavior relationshipBehavior;
    if (!string.IsNullOrWhiteSpace(def.JoinOnDateBehavior) && Enum.TryParse<DateTimeRelationshipBehavior>(def.JoinOnDateBehavior, true, out relationshipBehavior))
      relationship.JoinOnDateBehavior = relationshipBehavior;
    relationship.RelyOnReferentialIntegrity = def.RelyOnReferentialIntegrity.GetValueOrDefault();
    RelationshipEndCardinality relationshipEndCardinality1;
    if (!string.IsNullOrWhiteSpace(def.FromCardinality) && Enum.TryParse<RelationshipEndCardinality>(def.FromCardinality, true, out relationshipEndCardinality1))
      relationship.FromCardinality = relationshipEndCardinality1;
    RelationshipEndCardinality relationshipEndCardinality2;
    if (!string.IsNullOrWhiteSpace(def.ToCardinality) && Enum.TryParse<RelationshipEndCardinality>(def.ToCardinality, true, out relationshipEndCardinality2))
      relationship.ToCardinality = relationshipEndCardinality2;
    if (!string.IsNullOrWhiteSpace(def.SecurityFilteringBehavior))
    {
      SecurityFilteringBehavior filteringBehavior;
      if (Enum.TryParse<SecurityFilteringBehavior>(def.SecurityFilteringBehavior, true, out filteringBehavior))
        relationship.SecurityFilteringBehavior = filteringBehavior;
    }
    else
      relationship.SecurityFilteringBehavior = SecurityFilteringBehavior.OneDirection;
    if (def.Annotations != null)
    {
      foreach (KeyValuePair<string, string> annotation in def.Annotations)
      {
        RelationshipAnnotationCollection annotations = relationship.Annotations;
        Microsoft.AnalysisServices.Tabular.Annotation metadataObject = new Microsoft.AnalysisServices.Tabular.Annotation();
        metadataObject.Name = annotation.Key;
        metadataObject.Value = annotation.Value;
        annotations.Add(metadataObject);
      }
    }
    if (def.ExtendedProperties == null)
      return;
    ExtendedPropertyHelpers.ApplyToRelationship((Microsoft.AnalysisServices.Tabular.Relationship) relationship, def.ExtendedProperties);
  }

  private static bool ApplyRelationshipUpdates(
    Microsoft.AnalysisServices.Tabular.SingleColumnRelationship relationship,
    RelationshipUpdate update,
    Microsoft.AnalysisServices.Tabular.Database db)
  {
    bool flag = false;
    bool? nullable;
    if (update.IsActive.HasValue)
    {
      int num1 = relationship.IsActive ? 1 : 0;
      nullable = update.IsActive;
      int num2 = nullable.Value ? 1 : 0;
      if (num1 != num2)
      {
        nullable = update.IsActive;
        if (nullable.Value && Enumerable.Count<Microsoft.AnalysisServices.Tabular.Relationship>((IEnumerable<Microsoft.AnalysisServices.Tabular.Relationship>) db.Model.Relationships, (r =>
        {
          if (r == relationship || !r.IsActive)
            return false;
          if (r.FromTable == relationship.FromTable && r.ToTable == relationship.ToTable)
            return true;
          return r.FromTable == relationship.ToTable && r.ToTable == relationship.FromTable;
        })) > 0)
          throw new McpException($"Cannot activate relationship '{update.Name}'. An active relationship already exists between these tables. Deactivate the existing relationship first.");
        Microsoft.AnalysisServices.Tabular.SingleColumnRelationship columnRelationship = relationship;
        nullable = update.IsActive;
        int num3 = nullable.Value ? 1 : 0;
        columnRelationship.IsActive = num3 != 0;
        flag = true;
      }
    }
    if (!string.IsNullOrWhiteSpace(update.Type))
    {
      RelationshipType relationshipType;
      if (Enum.TryParse<RelationshipType>(update.Type, true, out relationshipType))
      {
        if (relationship.Type != relationshipType)
          throw new McpException($"Cannot change the Type of an existing relationship from '{relationship.Type}' to '{relationshipType}'. Delete and recreate the relationship instead.");
      }
      else
      {
        string[] names = Enum.GetNames(typeof (RelationshipType));
        throw new McpException($"Invalid Type '{update.Type}'. Valid values are: {string.Join(", ", names)}");
      }
    }
    if (!string.IsNullOrWhiteSpace(update.CrossFilteringBehavior))
    {
      CrossFilteringBehavior filteringBehavior;
      if (Enum.TryParse<CrossFilteringBehavior>(update.CrossFilteringBehavior, true, out filteringBehavior))
      {
        if (relationship.CrossFilteringBehavior != filteringBehavior)
        {
          relationship.CrossFilteringBehavior = filteringBehavior;
          flag = true;
        }
      }
      else
      {
        string[] names = Enum.GetNames(typeof (CrossFilteringBehavior));
        throw new McpException($"Invalid CrossFilteringBehavior '{update.CrossFilteringBehavior}'. Valid values are: {string.Join(", ", names)}");
      }
    }
    if (!string.IsNullOrWhiteSpace(update.JoinOnDateBehavior))
    {
      DateTimeRelationshipBehavior relationshipBehavior;
      if (Enum.TryParse<DateTimeRelationshipBehavior>(update.JoinOnDateBehavior, true, out relationshipBehavior))
      {
        if (relationship.JoinOnDateBehavior != relationshipBehavior)
        {
          relationship.JoinOnDateBehavior = relationshipBehavior;
          flag = true;
        }
      }
      else
      {
        string[] names = Enum.GetNames(typeof (DateTimeRelationshipBehavior));
        throw new McpException($"Invalid JoinOnDateBehavior '{update.JoinOnDateBehavior}'. Valid values are: {string.Join(", ", names)}");
      }
    }
    nullable = update.RelyOnReferentialIntegrity;
    if (nullable.HasValue)
    {
      int num4 = relationship.RelyOnReferentialIntegrity ? 1 : 0;
      nullable = update.RelyOnReferentialIntegrity;
      int num5 = nullable.Value ? 1 : 0;
      if (num4 != num5)
      {
        Microsoft.AnalysisServices.Tabular.SingleColumnRelationship columnRelationship = relationship;
        nullable = update.RelyOnReferentialIntegrity;
        int num6 = nullable.Value ? 1 : 0;
        columnRelationship.RelyOnReferentialIntegrity = num6 != 0;
        flag = true;
      }
    }
    if (!string.IsNullOrWhiteSpace(update.FromCardinality))
    {
      RelationshipEndCardinality relationshipEndCardinality;
      if (Enum.TryParse<RelationshipEndCardinality>(update.FromCardinality, true, out relationshipEndCardinality))
      {
        if (relationship.FromCardinality != relationshipEndCardinality)
        {
          relationship.FromCardinality = relationshipEndCardinality;
          flag = true;
        }
      }
      else
      {
        string[] names = Enum.GetNames(typeof (RelationshipEndCardinality));
        throw new McpException($"Invalid FromCardinality '{update.FromCardinality}'. Valid values are: {string.Join(", ", names)}");
      }
    }
    if (!string.IsNullOrWhiteSpace(update.ToCardinality))
    {
      RelationshipEndCardinality relationshipEndCardinality;
      if (Enum.TryParse<RelationshipEndCardinality>(update.ToCardinality, true, out relationshipEndCardinality))
      {
        if (relationship.ToCardinality != relationshipEndCardinality)
        {
          relationship.ToCardinality = relationshipEndCardinality;
          flag = true;
        }
      }
      else
      {
        string[] names = Enum.GetNames(typeof (RelationshipEndCardinality));
        throw new McpException($"Invalid ToCardinality '{update.ToCardinality}'. Valid values are: {string.Join(", ", names)}");
      }
    }
    if (!string.IsNullOrWhiteSpace(update.SecurityFilteringBehavior))
    {
      SecurityFilteringBehavior filteringBehavior;
      if (Enum.TryParse<SecurityFilteringBehavior>(update.SecurityFilteringBehavior, true, out filteringBehavior))
      {
        if (relationship.SecurityFilteringBehavior != filteringBehavior)
        {
          relationship.SecurityFilteringBehavior = filteringBehavior;
          flag = true;
        }
      }
      else
      {
        string[] names = Enum.GetNames(typeof (SecurityFilteringBehavior));
        throw new McpException($"Invalid SecurityFilteringBehavior '{update.SecurityFilteringBehavior}'. Valid values are: {string.Join(", ", names)}");
      }
    }
    if (!string.IsNullOrWhiteSpace(update.FromTable) || !string.IsNullOrWhiteSpace(update.FromColumn) || !string.IsNullOrWhiteSpace(update.ToTable) || !string.IsNullOrWhiteSpace(update.ToColumn))
      throw new McpException("Cannot change the tables or columns of an existing relationship. Delete and recreate the relationship instead.");
    if (update.Annotations != null)
    {
      relationship.Annotations.Clear();
      foreach (KeyValuePair<string, string> annotation in update.Annotations)
      {
        RelationshipAnnotationCollection annotations = relationship.Annotations;
        Microsoft.AnalysisServices.Tabular.Annotation metadataObject = new Microsoft.AnalysisServices.Tabular.Annotation();
        metadataObject.Name = annotation.Key;
        metadataObject.Value = annotation.Value;
        annotations.Add(metadataObject);
      }
      flag = true;
    }
    if (update.ExtendedProperties != null)
    {
      ExtendedPropertyHelpers.ReplaceRelationshipProperties((Microsoft.AnalysisServices.Tabular.Relationship) relationship, update.ExtendedProperties);
      flag = true;
    }
    return flag;
  }

  private static (Microsoft.AnalysisServices.Tabular.Table fromTable, Microsoft.AnalysisServices.Tabular.Column fromColumn, Microsoft.AnalysisServices.Tabular.Table toTable, Microsoft.AnalysisServices.Tabular.Column toColumn) ValidateAndFixCardinality(
    RelationshipCreate def,
    Microsoft.AnalysisServices.Tabular.Table fromTable,
    Microsoft.AnalysisServices.Tabular.Column fromColumn,
    Microsoft.AnalysisServices.Tabular.Table toTable,
    Microsoft.AnalysisServices.Tabular.Column toColumn,
    List<string> warnings)
  {
    RelationshipEndCardinality relationshipEndCardinality1 = RelationshipEndCardinality.Many;
    RelationshipEndCardinality relationshipEndCardinality2 = RelationshipEndCardinality.One;
    if (!string.IsNullOrWhiteSpace(def.FromCardinality))
      Enum.TryParse<RelationshipEndCardinality>(def.FromCardinality, true, out relationshipEndCardinality1);
    if (!string.IsNullOrWhiteSpace(def.ToCardinality))
      Enum.TryParse<RelationshipEndCardinality>(def.ToCardinality, true, out relationshipEndCardinality2);
    if (relationshipEndCardinality1 == RelationshipEndCardinality.One && relationshipEndCardinality2 == RelationshipEndCardinality.Many)
    {
      Microsoft.AnalysisServices.Tabular.Table table = fromTable;
      Microsoft.AnalysisServices.Tabular.Column column = fromColumn;
      fromTable = toTable;
      fromColumn = toColumn;
      toTable = table;
      toColumn = column;
      string fromTable1 = def.FromTable;
      string fromColumn1 = def.FromColumn;
      def.FromTable = def.ToTable;
      def.FromColumn = def.ToColumn;
      def.ToTable = fromTable1;
      def.ToColumn = fromColumn1;
      def.FromCardinality = "Many";
      def.ToCardinality = "One";
      warnings.Add($"Relationship direction was corrected: The 'from' side (many side) is now {def.FromTable}[{def.FromColumn}] and the 'to' side (one side) is now {def.ToTable}[{def.ToColumn}]. In Power BI relationships, the 'from' end cardinality should typically be 'Many' unless creating a one-to-one relationship.");
    }
    return (fromTable, fromColumn, toTable, toColumn);
  }
}
