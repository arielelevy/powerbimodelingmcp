// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using Microsoft.AnalysisServices.Tabular;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#nullable enable
namespace PowerBIModelingMCP.Library.Common;

public static class StructuralDependencyHelper
{
  public static List<string> CheckAndDeleteDependenciesIfRequired(
    Database db,
    NamedMetadataObject obj,
    bool cascadeDelete)
  {
    switch (obj)
    {
      case Table table:
        return StructuralDependencyHelper.GetTableStructuralDependencies(db, table, cascadeDelete);
      case Column col:
        return StructuralDependencyHelper.GetColumnStructuralDependencies(db, col.Table, col, cascadeDelete);
      case Measure measure:
        return StructuralDependencyHelper.GetMeasureStructuralDependencies(db, measure, cascadeDelete);
      case Hierarchy hierarchy:
        return StructuralDependencyHelper.GetHierarchyStructuralDependencies(db, hierarchy, cascadeDelete);
      case Level level:
        return StructuralDependencyHelper.GetLevelStructuralDependencies(db, level, cascadeDelete);
      default:
        throw new ArgumentException("Unsupported object type: " + ((MemberInfo) obj.GetType()).Name);
    }
  }

  private static List<string> GetTableStructuralDependencies(
    Database db,
    Table table,
    bool cascadeDelete)
  {
    List<string> structuralDependencies = new List<string>();
    structuralDependencies.AddRange((IEnumerable<string>) StructuralDependencyHelper.GetDependentVariations(db, table, cascadeDelete));
    structuralDependencies.AddRange((IEnumerable<string>) StructuralDependencyHelper.GetDependentRelationships(db, table, cascadeDelete));
    structuralDependencies.AddRange((IEnumerable<string>) StructuralDependencyHelper.GetDependentTablePermissions(db, table, cascadeDelete));
    structuralDependencies.AddRange((IEnumerable<string>) StructuralDependencyHelper.GetDependentRelatedAggregations(db, table, (IReadOnlyDictionary<NamedMetadataObject, IList<AlternateOf>>) null, cascadeDelete));
    structuralDependencies.AddRange((IEnumerable<string>) StructuralDependencyHelper.GetDependentPerspectives(db, table, cascadeDelete));
    structuralDependencies.AddRange((IEnumerable<string>) StructuralDependencyHelper.GetDependentObjectTranslations(db, table, cascadeDelete));
    structuralDependencies.AddRange((IEnumerable<string>) StructuralDependencyHelper.GetDependentDataSources(db, table, cascadeDelete));
    return structuralDependencies;
  }

  private static List<string> GetColumnStructuralDependencies(
    Database db,
    Table table,
    Column col,
    bool cascadeDelete)
  {
    List<string> structuralDependencies = new List<string>();
    foreach (Column col1 in Enumerable.Prepend<Column>(StructuralDependencyHelper.GetAllInferredColumns(db.Model).GetInferredColumns(col), col))
    {
      structuralDependencies.AddRange((IEnumerable<string>) StructuralDependencyHelper.GetDependentPerspectives(db, col1, cascadeDelete));
      structuralDependencies.AddRange((IEnumerable<string>) StructuralDependencyHelper.GetDependentObjectTranslations(db, col1, cascadeDelete));
      structuralDependencies.AddRange((IEnumerable<string>) StructuralDependencyHelper.GetDependentTablePermissions(db, table, col1, cascadeDelete));
      structuralDependencies.AddRange((IEnumerable<string>) StructuralDependencyHelper.GetDependentHierarchies(db, table, col1, cascadeDelete));
      structuralDependencies.AddRange((IEnumerable<string>) StructuralDependencyHelper.GetDependentVariations(db, table, col1, cascadeDelete));
      structuralDependencies.AddRange((IEnumerable<string>) StructuralDependencyHelper.GetDependentRelatedAggregations(db, col1, (IReadOnlyDictionary<NamedMetadataObject, IList<AlternateOf>>) null, cascadeDelete));
      structuralDependencies.AddRange((IEnumerable<string>) StructuralDependencyHelper.GetDependentRelationships(db, col1, cascadeDelete));
      structuralDependencies.AddRange((IEnumerable<string>) StructuralDependencyHelper.GetDependentGroupByColumns(db, table, col1, cascadeDelete));
    }
    return structuralDependencies;
  }

  private static List<string> GetMeasureStructuralDependencies(
    Database db,
    Measure measure,
    bool cascadeDelete)
  {
    List<string> structuralDependencies = new List<string>();
    structuralDependencies.AddRange((IEnumerable<string>) StructuralDependencyHelper.GetDependentPerspectives(db, measure, cascadeDelete));
    structuralDependencies.AddRange((IEnumerable<string>) StructuralDependencyHelper.GetDependentObjectTranslations(db, measure, cascadeDelete));
    return structuralDependencies;
  }

  private static List<string> GetHierarchyStructuralDependencies(
    Database db,
    Hierarchy hierarchy,
    bool cascadeDelete)
  {
    List<string> structuralDependencies = new List<string>();
    structuralDependencies.AddRange((IEnumerable<string>) StructuralDependencyHelper.GetDependentPerspectives(db, hierarchy, cascadeDelete));
    structuralDependencies.AddRange((IEnumerable<string>) StructuralDependencyHelper.GetDependentObjectTranslations(db, hierarchy, cascadeDelete));
    return structuralDependencies;
  }

  private static List<string> GetLevelStructuralDependencies(
    Database db,
    Level level,
    bool cascadeDelete)
  {
    List<string> structuralDependencies = new List<string>();
    structuralDependencies.AddRange((IEnumerable<string>) StructuralDependencyHelper.GetDependentObjectTranslations(db, level, cascadeDelete));
    return structuralDependencies;
  }

  private static IReadOnlyDictionary<Column, IReadOnlyList<CalculatedTableColumn>> GetAllInferredColumns(
    Model model)
  {
    Dictionary<Column, IReadOnlyList<CalculatedTableColumn>> allInferredColumns = new Dictionary<Column, IReadOnlyList<CalculatedTableColumn>>();
    Dictionary<Column, List<CalculatedTableColumn>> dictionary = Enumerable.ToDictionary<IGrouping<Column, CalculatedTableColumn>, Column, List<CalculatedTableColumn>>(Enumerable.GroupBy<CalculatedTableColumn, Column>(Enumerable.Where<CalculatedTableColumn>(Enumerable.OfType<CalculatedTableColumn>((IEnumerable) Enumerable.SelectMany<Table, Column>((IEnumerable<Table>) model.Tables, (Func<Table, IEnumerable<Column>>) (t => (IEnumerable<Column>) t.Columns))), (c => c.ColumnOrigin != null)), (c => c.ColumnOrigin)), (Func<IGrouping<Column, CalculatedTableColumn>, Column>) (g => g.Key), (Func<IGrouping<Column, CalculatedTableColumn>, List<CalculatedTableColumn>>) (g => Enumerable.ToList<CalculatedTableColumn>((IEnumerable<CalculatedTableColumn>) g)));
    IEnumerable<Column> columns = Enumerable.Where<Column>((IEnumerable<Column>) dictionary.Keys, (c => (c is CalculatedTableColumn calculatedTableColumn ? calculatedTableColumn.ColumnOrigin : (Column) null) == null));
    Func<CalculatedTableColumn, bool> includeSubtreePredicate = (c => true);
    foreach (Column sourceColumn in columns)
      allInferredColumns.Add(sourceColumn, (IReadOnlyList<CalculatedTableColumn>) Enumerable.ToList<CalculatedTableColumn>(sourceColumn.GetInferredColumns((IReadOnlyDictionary<Column, List<CalculatedTableColumn>>) dictionary, includeSubtreePredicate)));
    return (IReadOnlyDictionary<Column, IReadOnlyList<CalculatedTableColumn>>) allInferredColumns;
  }

  private static IEnumerable<Column> GetInferredColumns(
    this IReadOnlyDictionary<Column, IReadOnlyList<CalculatedTableColumn>> allInferredColumns,
    Column sourceColumn,
    bool includeSourceColumn = false)
  {
    IEnumerable<Column> inferredColumns = Enumerable.Empty<Column>();
    if (allInferredColumns.ContainsKey(sourceColumn))
      inferredColumns = Enumerable.Concat<Column>(inferredColumns, (IEnumerable<Column>) allInferredColumns[sourceColumn]);
    return inferredColumns;
  }

  private static IEnumerable<CalculatedTableColumn> GetInferredColumns(
    this Column sourceColumn,
    IReadOnlyDictionary<Column, List<CalculatedTableColumn>> inferredColumnsLookup,
    Func<CalculatedTableColumn, bool> includeSubtreePredicate)
  {
    List<CalculatedTableColumn> calculatedTableColumnList;
    if (!inferredColumnsLookup.TryGetValue(sourceColumn, out calculatedTableColumnList))
      calculatedTableColumnList = new List<CalculatedTableColumn>();
    List<CalculatedTableColumn> list = Enumerable.ToList<CalculatedTableColumn>(Enumerable.Where<CalculatedTableColumn>((IEnumerable<CalculatedTableColumn>) calculatedTableColumnList, includeSubtreePredicate));
    List<CalculatedTableColumn> inferredColumns = new List<CalculatedTableColumn>();
    foreach (CalculatedTableColumn sourceColumn1 in list)
    {
      inferredColumns.Add(sourceColumn1);
      inferredColumns.AddRange(sourceColumn1.GetInferredColumns(inferredColumnsLookup, includeSubtreePredicate));
    }
    return (IEnumerable<CalculatedTableColumn>) inferredColumns;
  }

  private static List<string> GetDependentGroupByColumns(
    Database db,
    Table table,
    Column col,
    bool cascadeDelete)
  {
    List<string> dependentGroupByColumns = new List<string>();
    foreach (Column column in (MetadataObjectCollection<Column, Table>) table.Columns)
    {
      RelatedColumnDetails relatedColumnDetails = column.RelatedColumnDetails;
      if (relatedColumnDetails != null)
      {
        GroupByColumn metadataObject = Enumerable.SingleOrDefault<GroupByColumn>((IEnumerable<GroupByColumn>) relatedColumnDetails.GroupByColumns, (g => g.GroupingColumn == col));
        if (metadataObject != null)
        {
          if (cascadeDelete)
            relatedColumnDetails.GroupByColumns.Remove(metadataObject);
          dependentGroupByColumns.Add("GroupByColumn: " + metadataObject.GroupingColumn.Name);
        }
      }
    }
    return dependentGroupByColumns;
  }

  private static List<string> GetDependentPerspectives(
    Database db,
    Table table,
    bool cascadeDelete)
  {
    List<string> dependentPerspectives = new List<string>();
    foreach (Perspective perspective in (MetadataObjectCollection<Perspective, Model>) db.Model.Perspectives)
    {
      if (perspective.PerspectiveTables.Contains(table.Name))
      {
        PerspectiveTable perspectiveTable = perspective.PerspectiveTables[table.Name];
        if (cascadeDelete)
          perspective.PerspectiveTables.Remove(table.Name);
        dependentPerspectives.Add($"Perspective: {perspective.Name}, PerspectiveTable: {perspectiveTable.Name}");
      }
    }
    return dependentPerspectives;
  }

  private static List<string> GetDependentPerspectives(Database db, Column col, bool cascadeDelete)
  {
    List<string> dependentPerspectives = new List<string>();
    foreach (Perspective perspective in (MetadataObjectCollection<Perspective, Model>) db.Model.Perspectives)
    {
      if (perspective.PerspectiveTables.Contains(col.Table.Name))
      {
        PerspectiveTable perspectiveTable = perspective.PerspectiveTables[col.Table.Name];
        if (perspectiveTable.PerspectiveColumns.Contains(col.Name))
        {
          PerspectiveColumn perspectiveColumn = perspectiveTable.PerspectiveColumns[col.Name];
          if (cascadeDelete)
            perspectiveTable.PerspectiveColumns.Remove(col.Name);
          dependentPerspectives.Add($"Perspective: {perspective.Name}, PerspectiveTable: {perspectiveTable.Name}, PerspectiveColumn: {perspectiveColumn.Name}");
        }
      }
    }
    return dependentPerspectives;
  }

  private static List<string> GetDependentPerspectives(
    Database db,
    Measure measure,
    bool cascadeDelete)
  {
    List<string> dependentPerspectives = new List<string>();
    foreach (Perspective perspective in (MetadataObjectCollection<Perspective, Model>) db.Model.Perspectives)
    {
      if (perspective.PerspectiveTables.Contains(measure.Table.Name))
      {
        PerspectiveTable perspectiveTable = perspective.PerspectiveTables[measure.Table.Name];
        if (perspectiveTable.PerspectiveMeasures.Contains(measure.Name))
        {
          PerspectiveMeasure perspectiveMeasure = perspectiveTable.PerspectiveMeasures[measure.Name];
          if (cascadeDelete)
            perspectiveTable.PerspectiveMeasures.Remove(measure.Name);
          dependentPerspectives.Add($"Perspective: {perspective.Name}, PerspectiveTable: {perspectiveTable.Name}, PerspectiveMeasure: {perspectiveMeasure.Name}");
        }
      }
    }
    return dependentPerspectives;
  }

  private static List<string> GetDependentPerspectives(
    Database db,
    Hierarchy hierarchy,
    bool cascadeDelete)
  {
    List<string> dependentPerspectives = new List<string>();
    foreach (Perspective perspective in (MetadataObjectCollection<Perspective, Model>) db.Model.Perspectives)
    {
      if (perspective.PerspectiveTables.Contains(hierarchy.Table.Name))
      {
        PerspectiveTable perspectiveTable = perspective.PerspectiveTables[hierarchy.Table.Name];
        if (perspectiveTable.PerspectiveHierarchies.Contains(hierarchy.Name))
        {
          PerspectiveHierarchy perspectiveHierarchy = perspectiveTable.PerspectiveHierarchies[hierarchy.Name];
          if (cascadeDelete)
            perspectiveTable.PerspectiveHierarchies.Remove(hierarchy.Name);
          dependentPerspectives.Add($"Perspective: {perspective.Name}, PerspectiveTable: {perspectiveTable.Name}, PerspectiveHierarchy: {perspectiveHierarchy.Name}");
        }
      }
    }
    return dependentPerspectives;
  }

  private static List<string> GetDependentRelationships(
    Database db,
    Table table,
    bool cascadeDelete)
  {
    List<Relationship> list = Enumerable.ToList<Relationship>(Enumerable.Where<Relationship>((IEnumerable<Relationship>) db.Model.Relationships, (r => r.FromTable == table || r.ToTable == table)));
    if (cascadeDelete)
    {
      foreach (SingleColumnRelationship metadataObject in list)
        db.Model.Relationships.Remove((Relationship) metadataObject);
    }
    return Enumerable.ToList<string>(Enumerable.Select<Relationship, string>((IEnumerable<Relationship>) list, (r => "Relationship: " + r.Name)));
  }

  private static List<string> GetDependentRelationships(
    Database db,
    Column col,
    bool cascadeDelete)
  {
    List<string> dependentRelationships = new List<string>();
    foreach (SingleColumnRelationship metadataObject in Enumerable.ToList<Relationship>(Enumerable.Where<Relationship>((IEnumerable<Relationship>) db.Model.Relationships, (r => ((SingleColumnRelationship) r).FromColumn == col || ((SingleColumnRelationship) r).ToColumn == col))))
    {
      if (cascadeDelete)
        db.Model.Relationships.Remove((Relationship) metadataObject);
      if (metadataObject.FromColumn == col)
        dependentRelationships.Add($"Relationship: {metadataObject.Name}, ToColumn: {metadataObject.ToColumn.Name}");
      else
        dependentRelationships.Add($"Relationship: {metadataObject.Name}, FromColumn: {metadataObject.FromColumn.Name}");
    }
    return dependentRelationships;
  }

  private static List<string> GetDependentObjectTranslations(
    Database db,
    Table table,
    bool cascadeDelete)
  {
    List<string> objectTranslations = new List<string>();
    foreach (TranslatedProperty translatedProp in Enum.GetValues(typeof (TranslatedProperty)))
    {
      foreach (Culture culture in (MetadataObjectCollection<Culture, Model>) db.Model.Cultures)
      {
        ObjectTranslation objectTranslation = culture.ObjectTranslations[(MetadataObject) table, translatedProp];
        if (objectTranslation != null)
        {
          if (cascadeDelete)
            culture.ObjectTranslations.Remove(objectTranslation);
          objectTranslations.Add($"Culture: {culture.Name}, Translation: {objectTranslation.Value}");
        }
      }
    }
    foreach (Column column in (MetadataObjectCollection<Column, Table>) table.Columns)
      objectTranslations.AddRange((IEnumerable<string>) StructuralDependencyHelper.GetDependentObjectTranslations(db, column, cascadeDelete));
    foreach (Measure measure in (MetadataObjectCollection<Measure, Table>) table.Measures)
      objectTranslations.AddRange((IEnumerable<string>) StructuralDependencyHelper.GetDependentObjectTranslations(db, measure, cascadeDelete));
    foreach (Hierarchy hierarchy in (MetadataObjectCollection<Hierarchy, Table>) table.Hierarchies)
      objectTranslations.AddRange((IEnumerable<string>) StructuralDependencyHelper.GetDependentObjectTranslations(db, hierarchy, cascadeDelete));
    return objectTranslations;
  }

  private static List<string> GetDependentObjectTranslations(
    Database db,
    Column col,
    bool cascadeDelete)
  {
    List<string> objectTranslations = new List<string>();
    foreach (TranslatedProperty translatedProp in Enum.GetValues(typeof (TranslatedProperty)))
    {
      foreach (Culture culture in (MetadataObjectCollection<Culture, Model>) db.Model.Cultures)
      {
        ObjectTranslation objectTranslation = culture.ObjectTranslations[(MetadataObject) col, translatedProp];
        if (objectTranslation != null)
        {
          if (cascadeDelete)
            culture.ObjectTranslations.Remove(objectTranslation);
          objectTranslations.Add($"Culture: {culture.Name}, Column Translation: {objectTranslation.Value}, TranslatedProperty: {translatedProp.ToString()}");
        }
      }
    }
    return objectTranslations;
  }

  private static List<string> GetDependentObjectTranslations(
    Database db,
    Measure measure,
    bool cascadeDelete)
  {
    List<string> objectTranslations = new List<string>();
    foreach (TranslatedProperty translatedProp in Enum.GetValues(typeof (TranslatedProperty)))
    {
      foreach (Culture culture in (MetadataObjectCollection<Culture, Model>) db.Model.Cultures)
      {
        ObjectTranslation objectTranslation = culture.ObjectTranslations[(MetadataObject) measure, translatedProp];
        if (objectTranslation != null)
        {
          if (cascadeDelete)
            culture.ObjectTranslations.Remove(objectTranslation);
          objectTranslations.Add($"Culture: {culture.Name}, Measure Translation: {objectTranslation.Value}, TranslatedProperty: {translatedProp.ToString()}");
        }
      }
    }
    return objectTranslations;
  }

  private static List<string> GetDependentObjectTranslations(
    Database db,
    Hierarchy hierarchy,
    bool cascadeDelete)
  {
    List<string> objectTranslations = new List<string>();
    foreach (TranslatedProperty translatedProp in Enum.GetValues(typeof (TranslatedProperty)))
    {
      foreach (Culture culture in (MetadataObjectCollection<Culture, Model>) db.Model.Cultures)
      {
        ObjectTranslation objectTranslation = culture.ObjectTranslations[(MetadataObject) hierarchy, translatedProp];
        if (objectTranslation != null)
        {
          if (cascadeDelete)
            culture.ObjectTranslations.Remove(objectTranslation);
          objectTranslations.Add($"Culture: {culture.Name}, Hierarchy Translation: {objectTranslation.Value}, TranslatedProperty: {translatedProp.ToString()}");
        }
      }
    }
    foreach (Level level in (MetadataObjectCollection<Level, Hierarchy>) hierarchy.Levels)
      objectTranslations.AddRange((IEnumerable<string>) StructuralDependencyHelper.GetDependentObjectTranslations(db, level, cascadeDelete));
    return objectTranslations;
  }

  private static List<string> GetDependentObjectTranslations(
    Database db,
    Level level,
    bool cascadeDelete)
  {
    List<string> objectTranslations = new List<string>();
    foreach (TranslatedProperty translatedProp in Enum.GetValues(typeof (TranslatedProperty)))
    {
      foreach (Culture culture in (MetadataObjectCollection<Culture, Model>) db.Model.Cultures)
      {
        ObjectTranslation objectTranslation = culture.ObjectTranslations[(MetadataObject) level, translatedProp];
        if (objectTranslation != null)
        {
          if (cascadeDelete)
            culture.ObjectTranslations.Remove(objectTranslation);
          objectTranslations.Add($"Culture: {culture.Name}, Level Translation: {objectTranslation.Value}, TranslatedProperty: {translatedProp.ToString()}");
        }
      }
    }
    return objectTranslations;
  }

  private static List<string> GetDependentTablePermissions(
    Database db,
    Table table,
    bool cascadeDelete)
  {
    TablePermission[] array = Enumerable.ToArray<TablePermission>(Enumerable.Where<TablePermission>(Enumerable.SelectMany<ModelRole, TablePermission>((IEnumerable<ModelRole>) db.Model.Roles, (Func<ModelRole, IEnumerable<TablePermission>>) (r => (IEnumerable<TablePermission>) r.TablePermissions)), (tp => tp.Table == table)));
    if (cascadeDelete)
    {
      foreach (TablePermission metadataObject in array)
        metadataObject.Role.TablePermissions.Remove(metadataObject);
    }
    return Enumerable.ToList<string>(Enumerable.Select<TablePermission, string>((IEnumerable<TablePermission>) array, (tp => "TablePermission Role: " + tp.Role.Name)));
  }

  private static List<string> GetDependentTablePermissions(
    Database db,
    Table table,
    Column col,
    bool cascadeDelete)
  {
    List<string> tablePermissions = new List<string>();
    foreach (TablePermission tablePermission in Enumerable.ToArray<TablePermission>(Enumerable.Where<TablePermission>(Enumerable.SelectMany<ModelRole, TablePermission>((IEnumerable<ModelRole>) db.Model.Roles, (Func<ModelRole, IEnumerable<TablePermission>>) (r => (IEnumerable<TablePermission>) r.TablePermissions)), (tp => tp.Table == table))))
    {
      ColumnPermission metadataObject = Enumerable.FirstOrDefault<ColumnPermission>((IEnumerable<ColumnPermission>) tablePermission.ColumnPermissions, (cp => cp.Column == col));
      if (metadataObject != null)
      {
        if (cascadeDelete)
          tablePermission.ColumnPermissions.Remove(metadataObject);
        tablePermissions.Add($"TablePermission Role: {tablePermission.Role.Name}, ColumnPermission: {metadataObject.Name}");
      }
    }
    return tablePermissions;
  }

  private static List<string> GetDependentHierarchies(
    Database db,
    Table table,
    Column col,
    bool cascadeDelete)
  {
    Level[] array = Enumerable.ToArray<Level>(Enumerable.Where<Level>(Enumerable.SelectMany<Table, Level>((IEnumerable<Table>) db.Model.Tables, (Func<Table, IEnumerable<Level>>) (t => Enumerable.SelectMany<Hierarchy, Level>((IEnumerable<Hierarchy>) t.Hierarchies, (Func<Hierarchy, IEnumerable<Level>>) (h => (IEnumerable<Level>) h.Levels)))), (l => l.Column == col)));
    if (cascadeDelete)
    {
      foreach (Level metadataObject in array)
        metadataObject.Hierarchy.Levels.Remove(metadataObject);
    }
    return Enumerable.ToList<string>(Enumerable.Select<Level, string>((IEnumerable<Level>) array, (l => $"Hierarchy: {l.Hierarchy.Name}, Level: {l.Name}")));
  }

  private static List<string> GetDependentVariations(Database db, Table table, bool cascadeDelete)
  {
    List<string> dependentVariations = new List<string>();
    if (table.ShowAsVariationsOnly)
    {
      foreach (SingleColumnRelationship columnRelationship in Enumerable.Where<SingleColumnRelationship>(Enumerable.OfType<SingleColumnRelationship>((IEnumerable) db.Model.Relationships), (r => Enumerable.Any<Variation>((IEnumerable<Variation>) r.FromColumn.Variations, (v => v.Relationship?.ToTable == table)))))
      {
        Column fromColumn = columnRelationship.FromColumn;
        foreach (Variation variation in (MetadataObjectCollection<Variation, Column>) fromColumn.Variations)
        {
          if (cascadeDelete)
            fromColumn.Variations.Remove(variation);
          dependentVariations.Add("Variation: " + variation.Name);
        }
      }
    }
    foreach (Variation variation in Enumerable.ToList<Variation>(Enumerable.Where<Variation>(Enumerable.SelectMany<Column, Variation>((IEnumerable<Column>) table.Columns, (Func<Column, IEnumerable<Variation>>) (c => (IEnumerable<Variation>) c.Variations)), (v => v.Relationship != null && v.Relationship.ToTable.ShowAsVariationsOnly))))
    {
      SingleColumnRelationship relationship = (SingleColumnRelationship) variation.Relationship;
      if (cascadeDelete)
        db.Model.Relationships.Remove((Relationship) relationship);
      variation.Relationship.ToTable.ShowAsVariationsOnly = false;
      dependentVariations.Add($"Variation: {variation.Name}, Relationship: {relationship.Name}, RelationshipToTable: {relationship.ToTable.Name}");
    }
    return dependentVariations;
  }

  private static List<string> GetDependentVariations(
    Database db,
    Table table,
    Column col,
    bool cascadeDelete)
  {
    if (cascadeDelete)
    {
      foreach (Variation variation in (MetadataObjectCollection<Variation, Column>) col.Variations)
        col.Variations.Remove(variation);
    }
    return Enumerable.ToList<string>(Enumerable.Select<Variation, string>((IEnumerable<Variation>) col.Variations, (v => "Variation: " + v.Name)));
  }

  private static List<string> GetDependentRelatedAggregations(
    Database db,
    Table table,
    IReadOnlyDictionary<NamedMetadataObject, IList<AlternateOf>>? relatedAggregationsMap,
    bool cascadeDelete)
  {
    List<string> relatedAggregations = new List<string>();
    if (relatedAggregationsMap == null)
      relatedAggregationsMap = StructuralDependencyHelper.GetRelatedAggregationsMap(db.Model);
    foreach (Column column in (MetadataObjectCollection<Column, Table>) table.Columns)
      relatedAggregations.AddRange((IEnumerable<string>) StructuralDependencyHelper.GetDependentRelatedAggregations(db, column, relatedAggregationsMap, cascadeDelete));
    IList<AlternateOf> alternateOfList;
    if (relatedAggregationsMap.TryGetValue((NamedMetadataObject) table, out alternateOfList))
    {
      foreach (AlternateOf alternateOf in (IEnumerable<AlternateOf>) alternateOfList)
      {
        if (cascadeDelete)
          alternateOf.Column.AlternateOf = (AlternateOf) null;
        relatedAggregations.Add("RelatedAggregation: " + alternateOf.Column.Name);
      }
    }
    return relatedAggregations;
  }

  private static List<string> GetDependentRelatedAggregations(
    Database db,
    Column col,
    IReadOnlyDictionary<NamedMetadataObject, IList<AlternateOf>>? relatedAggregationsMap,
    bool cascadeDelete)
  {
    List<string> relatedAggregations = new List<string>();
    if (relatedAggregationsMap == null)
      relatedAggregationsMap = StructuralDependencyHelper.GetRelatedAggregationsMap(db.Model);
    IList<AlternateOf> alternateOfList;
    if (relatedAggregationsMap.TryGetValue((NamedMetadataObject) col, out alternateOfList))
    {
      foreach (AlternateOf alternateOf in (IEnumerable<AlternateOf>) alternateOfList)
      {
        if (cascadeDelete)
          alternateOf.Column.AlternateOf = (AlternateOf) null;
        relatedAggregations.Add("RelatedAggregation: " + alternateOf.Column.Name);
      }
    }
    return relatedAggregations;
  }

  private static IReadOnlyDictionary<NamedMetadataObject, IList<AlternateOf>> GetRelatedAggregationsMap(
    Model model)
  {
    Dictionary<NamedMetadataObject, IList<AlternateOf>> relatedAggregationsMap = new Dictionary<NamedMetadataObject, IList<AlternateOf>>();
    foreach (Table table in (MetadataObjectCollection<Table, Model>) model.Tables)
    {
      foreach (Column column in (MetadataObjectCollection<Column, Table>) table.Columns)
      {
        if (column.AlternateOf?.BaseColumn != null)
        {
          IList<AlternateOf> alternateOfList1;
          if (relatedAggregationsMap.TryGetValue((NamedMetadataObject) column.AlternateOf.BaseColumn, out alternateOfList1))
          {
            ((ICollection<AlternateOf>) alternateOfList1).Add(column.AlternateOf);
          }
          else
          {
            Dictionary<NamedMetadataObject, IList<AlternateOf>> dictionary = relatedAggregationsMap;
            Column baseColumn = column.AlternateOf.BaseColumn;
            List<AlternateOf> alternateOfList2 = new List<AlternateOf>();
            alternateOfList2.Add(column.AlternateOf);
            dictionary.Add((NamedMetadataObject) baseColumn, (IList<AlternateOf>) alternateOfList2);
          }
        }
        if (column.AlternateOf?.BaseTable != null)
        {
          IList<AlternateOf> alternateOfList3;
          if (relatedAggregationsMap.TryGetValue((NamedMetadataObject) column.AlternateOf.BaseTable, out alternateOfList3))
          {
            ((ICollection<AlternateOf>) alternateOfList3).Add(column.AlternateOf);
          }
          else
          {
            Dictionary<NamedMetadataObject, IList<AlternateOf>> dictionary = relatedAggregationsMap;
            Table baseTable = column.AlternateOf.BaseTable;
            List<AlternateOf> alternateOfList4 = new List<AlternateOf>();
            alternateOfList4.Add(column.AlternateOf);
            dictionary.Add((NamedMetadataObject) baseTable, (IList<AlternateOf>) alternateOfList4);
          }
        }
      }
    }
    return (IReadOnlyDictionary<NamedMetadataObject, IList<AlternateOf>>) relatedAggregationsMap;
  }

  private static List<string> GetDependentDataSources(Database db, Table table, bool cascadeDelete)
  {
    List<string> dependentDataSources = new List<string>();
    foreach (Partition partition in (MetadataObjectCollection<Partition, Table>) table.Partitions)
    {
      DataSource dataSource = partition.Source is QueryPartitionSource source1 ? source1.DataSource : (partition.Source is EntityPartitionSource source2 ? source2.DataSource : (DataSource) null);
      if (dataSource != null && Enumerable.Count<Table>(Enumerable.Where<Table>((IEnumerable<Table>) db.Model.Tables, (t => Enumerable.Any<Partition>((IEnumerable<Partition>) t.Partitions, (p => (p.Source is QueryPartitionSource source3 ? source3.DataSource : (DataSource) null) == dataSource || (p.Source is EntityPartitionSource source4 ? source4.DataSource : (DataSource) null) == dataSource))))) == 1)
      {
        if (cascadeDelete)
          partition.Source = (PartitionSource) null;
        dependentDataSources.Add("DataSource: " + dataSource.Name);
      }
    }
    return dependentDataSources;
  }
}
