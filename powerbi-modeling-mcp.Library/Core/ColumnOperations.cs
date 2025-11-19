// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Core.ColumnOperations
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using Microsoft.AnalysisServices.Tabular;
using Microsoft.AnalysisServices.Tabular.Serialization;
using ModelContextProtocol;
using PowerBIModelingMCP.Library.Common;
using PowerBIModelingMCP.Library.Common.DataStructures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

#nullable enable
namespace PowerBIModelingMCP.Library.Core;

public static class ColumnOperations
{
  private static void ValidateBase(ColumnBase def, bool isCreate)
  {
    if (def == null)
      throw new McpException("Column definition cannot be null");
    if (string.IsNullOrWhiteSpace(def.TableName))
      throw new McpException("TableName is required");
    if (string.IsNullOrWhiteSpace(def.Name))
      throw new McpException("Name is required");
    if (isCreate && string.IsNullOrWhiteSpace(def.Expression) && string.IsNullOrWhiteSpace(def.SourceColumn))
      throw new McpException("Either Expression or SourceColumn must be provided for creation");
    if (!string.IsNullOrWhiteSpace(def.Expression) && !string.IsNullOrWhiteSpace(def.SourceColumn))
      throw new McpException("Cannot specify both Expression and SourceColumn");
    if (!string.IsNullOrWhiteSpace(def.DataType) && !Enum.IsDefined(typeof (DataType), (object) def.DataType))
    {
      string[] names = Enum.GetNames(typeof (DataType));
      throw new McpException($"Invalid DataType '{def.DataType}'. Valid values are: {string.Join(", ", names)}");
    }
    if (def.ExtendedProperties != null)
    {
      List<string> stringList = ExtendedPropertyHelpers.Validate(def.ExtendedProperties);
      if (stringList.Count > 0)
        throw new McpException("ExtendedProperties validation failed: " + string.Join(", ", (IEnumerable<string>) stringList));
    }
    AnnotationHelpers.ValidateAnnotations(def.Annotations);
    if (def.AlternateOf != null)
      ColumnOperations.ValidateAlternateOfStructure(def.AlternateOf);
    if (def.GroupByColumns == null || def.GroupByColumns.Count <= 0)
      return;
    ColumnOperations.ValidateGroupByColumnsStructure(def.GroupByColumns, def.Name);
  }

  private static void ValidateAlternateOfStructure(AlternateOfDefinition alternateOf)
  {
    if (string.IsNullOrWhiteSpace(alternateOf.BaseTable))
      throw new McpException("AlternateOf BaseTable is required. Specify the name of the table that contains the source data for this aggregation.");
    if (string.IsNullOrWhiteSpace(alternateOf.BaseColumn))
    {
      if (!string.IsNullOrWhiteSpace(alternateOf.Summarization) && !alternateOf.Summarization.Equals("Count", StringComparison.OrdinalIgnoreCase))
        throw new McpException($"When creating a table reference (BaseColumn not specified), Summarization must be 'Count'. Current value: '{alternateOf.Summarization}'. For other summarization types (Sum, Min, Max, GroupBy), specify a BaseColumn to reference a specific column.");
    }
    else if (!string.IsNullOrWhiteSpace(alternateOf.Summarization))
    {
      string[] strArray = new string[5]
      {
        "GroupBy",
        "Sum",
        "Count",
        "Min",
        "Max"
      };
      if (!Enumerable.Contains<string>((IEnumerable<string>) strArray, alternateOf.Summarization, (IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase))
        throw new McpException($"Invalid Summarization '{alternateOf.Summarization}' for column reference. Valid values: {string.Join(", ", strArray)}. " + "Use 'GroupBy' for dimensional data, 'Sum'/'Min'/'Max' for numeric aggregations, or 'Count' for counting rows.");
    }
    List<string> stringList = new List<string>();
    foreach (KeyValuePair<string, string> annotation in alternateOf.Annotations)
    {
      if (string.IsNullOrWhiteSpace(annotation.Key))
        throw new McpException("AlternateOf annotation keys cannot be null or empty. Each annotation must have a valid key-value pair.");
      if (stringList.Contains(annotation.Key))
        throw new McpException($"Duplicate AlternateOf annotation key: '{annotation.Key}'. Each annotation key must be unique within the AlternateOf definition.");
      stringList.Add(annotation.Key);
    }
  }

  private static void ValidateAlternateOf(
    AlternateOfDefinition alternateOf,
    string currentTableName,
    Microsoft.AnalysisServices.Tabular.Database database)
  {
    ColumnOperations.ValidateAlternateOfStructure(alternateOf);
    if (alternateOf.BaseTable.Equals(currentTableName, StringComparison.OrdinalIgnoreCase))
      throw new McpException($"AlternateOf BaseTable cannot reference the same table ('{currentTableName}'). AlternateOf relationships must establish cross-table relationships. Specify a different table name for BaseTable.");
    Microsoft.AnalysisServices.Tabular.Table table = database.Model.Tables.Find(alternateOf.BaseTable);
    if (table == null)
    {
      List<string> list = Enumerable.ToList<string>(Enumerable.Take<string>(Enumerable.Select<Microsoft.AnalysisServices.Tabular.Table, string>(Enumerable.Where<Microsoft.AnalysisServices.Tabular.Table>((IEnumerable<Microsoft.AnalysisServices.Tabular.Table>) database.Model.Tables, (t => !t.Name.Equals(currentTableName, StringComparison.OrdinalIgnoreCase))), (t => t.Name)), 5));
      string str = Enumerable.Any<string>((IEnumerable<string>) list) ? $" Available tables (excluding current table): {string.Join(", ", (IEnumerable<string>) list)}{(database.Model.Tables.Count > list.Count + 1 ? "..." : "")}" : " No other tables are available in the model.";
      throw new McpException($"AlternateOf BaseTable references non-existent table '{alternateOf.BaseTable}'.{str}");
    }
    int count = table.Partitions.Count;
    if (count == 0)
      throw new McpException($"AlternateOf BaseTable '{alternateOf.BaseTable}' has no partitions. AlternateOf relationships require the base table to have exactly one partition in DirectQuery mode.");
    if (count > 1)
      throw new McpException($"AlternateOf BaseTable '{alternateOf.BaseTable}' has {count} partitions. " + "AlternateOf relationships require the base table to have exactly one partition in DirectQuery mode.");
    Microsoft.AnalysisServices.Tabular.Partition partition = table.Partitions[0];
    if (partition.Mode != ModeType.DirectQuery)
    {
      string str = partition.Mode.ToString();
      throw new McpException($"AlternateOf BaseTable '{alternateOf.BaseTable}' partition is in '{str}' mode. " + "AlternateOf relationships require the base table to have exactly one partition in DirectQuery mode. This is a platform limitation for AlternateOf functionality.");
    }
    if (string.IsNullOrWhiteSpace(alternateOf.BaseColumn))
      return;
    Microsoft.AnalysisServices.Tabular.Column column = table.Columns.Find(alternateOf.BaseColumn);
    if (column == null)
    {
      List<string> list = Enumerable.ToList<string>(Enumerable.Take<string>(Enumerable.Select<Microsoft.AnalysisServices.Tabular.Column, string>(Enumerable.Where<Microsoft.AnalysisServices.Tabular.Column>((IEnumerable<Microsoft.AnalysisServices.Tabular.Column>) table.Columns, (c => !(c is RowNumberColumn))), (c => c.Name)), 5));
      string str1;
      if (!Enumerable.Any<string>((IEnumerable<string>) list))
        str1 = $" Table '{alternateOf.BaseTable}' has no accessible columns.";
      else
        str1 = $" Available columns in table '{alternateOf.BaseTable}': {string.Join(", ", (IEnumerable<string>) list)}{(table.Columns.Count > list.Count ? "..." : "")}";
      string str2 = str1;
      throw new McpException($"AlternateOf BaseColumn references non-existent column '{alternateOf.BaseColumn}' in table '{alternateOf.BaseTable}'.{str2}");
    }
    string str3 = column.DataType.ToString();
    if (string.IsNullOrWhiteSpace(alternateOf.Summarization))
      return;
    bool flag1 = Enumerable.Contains<string>((IEnumerable<string>) new string[3]
    {
      "Sum",
      "Min",
      "Max"
    }, alternateOf.Summarization, (IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    bool flag2 = Enumerable.Contains<string>((IEnumerable<string>) new string[4]
    {
      "Int64",
      "Double",
      "Decimal",
      "Currency"
    }, str3);
    if (flag1 && !flag2)
      throw new McpException($"Summarization '{alternateOf.Summarization}' is typically used with numeric columns, but column '{alternateOf.BaseColumn}' has data type '{str3}'. " + "Consider using 'GroupBy' for non-numeric columns or 'Count' for counting operations.");
  }

  private static void ValidateGroupByColumnsStructure(
    List<string> groupByColumns,
    string columnName)
  {
    if (Enumerable.Any<string>(Enumerable.ToList<string>(Enumerable.Where<string>((IEnumerable<string>) groupByColumns, (c => string.IsNullOrWhiteSpace(c))))))
      throw new McpException("GroupByColumns cannot contain null or empty column names. All referenced columns must have valid names.");
    List<string> list = Enumerable.ToList<string>(Enumerable.Select<IGrouping<string, string>, string>(Enumerable.Where<IGrouping<string, string>>(Enumerable.GroupBy<string, string>((IEnumerable<string>) groupByColumns, (c => c), (IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase), (Func<IGrouping<string, string>, bool>) (g => Enumerable.Count<string>((IEnumerable<string>) g) > 1)), (Func<IGrouping<string, string>, string>) (g => g.Key)));
    if (Enumerable.Any<string>((IEnumerable<string>) list))
      throw new McpException($"GroupByColumns contains duplicate column references: {string.Join(", ", (IEnumerable<string>) list)}. Each column can only be referenced once.");
    if (Enumerable.Any<string>((IEnumerable<string>) groupByColumns, (c => c.Equals(columnName, StringComparison.OrdinalIgnoreCase))))
      throw new McpException($"Column '{columnName}' cannot reference itself in GroupByColumns. GroupByColumns must reference other columns in the same table.");
  }

  private static void ValidateGroupByColumns(
    List<string> groupByColumns,
    string tableName,
    string columnName,
    Microsoft.AnalysisServices.Tabular.Database database)
  {
    ColumnOperations.ValidateGroupByColumnsStructure(groupByColumns, columnName);
    Microsoft.AnalysisServices.Tabular.Table table = database.Model.Tables.Find(tableName);
    if (table == null)
      throw new McpException($"Table '{tableName}' not found for GroupByColumns validation.");
    foreach (string groupByColumn in groupByColumns)
    {
      Microsoft.AnalysisServices.Tabular.Column column = table.Columns.Find(groupByColumn);
      if (column == null)
      {
        List<string> list = Enumerable.ToList<string>(Enumerable.Select<Microsoft.AnalysisServices.Tabular.Column, string>(Enumerable.Where<Microsoft.AnalysisServices.Tabular.Column>((IEnumerable<Microsoft.AnalysisServices.Tabular.Column>) table.Columns, (c => !(c is RowNumberColumn))), (c => c.Name)));
        string str1;
        if (!Enumerable.Any<string>((IEnumerable<string>) list))
          str1 = $" Table '{tableName}' has no accessible columns.";
        else
          str1 = $" Available columns in table '{tableName}': {string.Join(", ", (IEnumerable<string>) list)}{(table.Columns.Count > list.Count ? "..." : "")}";
        string str2 = str1;
        throw new McpException($"GroupByColumns references non-existent column '{groupByColumn}' in table '{tableName}'.{str2}");
      }
      if (column is RowNumberColumn)
        throw new McpException($"GroupByColumns cannot reference RowNumber column '{groupByColumn}'. RowNumber columns are system-generated and not suitable for grouping operations.");
    }
  }

  private static void ApplyGroupByColumns(Microsoft.AnalysisServices.Tabular.Column column, List<string> groupByColumns)
  {
    try
    {
      column.RelatedColumnDetails = (Microsoft.AnalysisServices.Tabular.RelatedColumnDetails) null;
      if (groupByColumns == null || groupByColumns.Count <= 0)
        return;
      Microsoft.AnalysisServices.Tabular.RelatedColumnDetails relatedColumnDetails = new Microsoft.AnalysisServices.Tabular.RelatedColumnDetails();
      foreach (string groupByColumn in groupByColumns)
        relatedColumnDetails.GroupByColumns.Add(new Microsoft.AnalysisServices.Tabular.GroupByColumn()
        {
          GroupingColumn = column.Table.Columns.Find(groupByColumn) ?? throw new McpException($"GroupByColumns references non-existent column '{groupByColumn}' in table '{column.Table.Name}'.")
        });
      column.RelatedColumnDetails = relatedColumnDetails;
    }
    catch (Exception ex)
    {
      throw new McpException($"Failed to apply GroupByColumns to column '{column.Name}': {ex.Message}", ex);
    }
  }

  private static List<string>? ExtractGroupByColumns(Microsoft.AnalysisServices.Tabular.Column column)
  {
    return column.RelatedColumnDetails?.GroupByColumns == null || !Enumerable.Any<Microsoft.AnalysisServices.Tabular.GroupByColumn>((IEnumerable<Microsoft.AnalysisServices.Tabular.GroupByColumn>) column.RelatedColumnDetails.GroupByColumns) ? (List<string>) null : Enumerable.ToList<string>(Enumerable.Select<Microsoft.AnalysisServices.Tabular.GroupByColumn, string>((IEnumerable<Microsoft.AnalysisServices.Tabular.GroupByColumn>) column.RelatedColumnDetails.GroupByColumns, (gbc => gbc.GroupingColumn.Name)));
  }

  private static bool CompareGroupByColumns(List<string>? current, List<string>? updated)
  {
    if (current == null && updated == null)
      return true;
    return current != null && updated != null && current.Count == updated.Count && Enumerable.SequenceEqual<string>((IEnumerable<string>) current, (IEnumerable<string>) updated, (IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
  }

  private static void ValidateKeyColumnConstraint(
    Microsoft.AnalysisServices.Tabular.Table table,
    string? columnName,
    bool isSettingAsKey)
  {
    if (!isSettingAsKey)
      return;
    List<Microsoft.AnalysisServices.Tabular.Column> list = Enumerable.ToList<Microsoft.AnalysisServices.Tabular.Column>(Enumerable.Where<Microsoft.AnalysisServices.Tabular.Column>((IEnumerable<Microsoft.AnalysisServices.Tabular.Column>) table.Columns, (c =>
    {
      if (!c.IsKey)
        return false;
      return columnName == null || !c.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase);
    })));
    if (Enumerable.Any<Microsoft.AnalysisServices.Tabular.Column>((IEnumerable<Microsoft.AnalysisServices.Tabular.Column>) list))
    {
      string str = string.Join(", ", Enumerable.Select<Microsoft.AnalysisServices.Tabular.Column, string>((IEnumerable<Microsoft.AnalysisServices.Tabular.Column>) list, (c => c.Name)));
      throw new McpException($"Cannot set column as key because table '{table.Name}' already has key column(s): {str}. Only one column per table can be designated as a key column.");
    }
  }

  private static VariationDefinition? ExtractVariation(Microsoft.AnalysisServices.Tabular.Column column, Microsoft.AnalysisServices.Tabular.Database database)
  {
    if (column.Variations == null || !Enumerable.Any<Microsoft.AnalysisServices.Tabular.Variation>((IEnumerable<Microsoft.AnalysisServices.Tabular.Variation>) column.Variations))
      return (VariationDefinition) null;
    if (column.Variations.Count > 1)
      throw new McpException($"Column '{column.Name}' in table '{column.Table.Name}' has {column.Variations.Count} variations. Only single variations are supported.");
    Microsoft.AnalysisServices.Tabular.Variation variation1 = column.Variations[0];
    ColumnOperations.ValidateVariation(variation1, database);
    VariationDefinition variation2 = new VariationDefinition();
    if (variation1.Relationship != null)
    {
      variation2.RelationshipName = variation1.Relationship.Name;
      Microsoft.AnalysisServices.Tabular.Table toTable = variation1.Relationship.ToTable;
      variation2.HiddenTableName = toTable.Name;
      if (Enumerable.Any<Microsoft.AnalysisServices.Tabular.Hierarchy>((IEnumerable<Microsoft.AnalysisServices.Tabular.Hierarchy>) toTable.Hierarchies))
      {
        Microsoft.AnalysisServices.Tabular.Hierarchy hierarchy = toTable.Hierarchies[0];
        variation2.HierarchyName = hierarchy.Name;
        variation2.HierarchyLevelNames = ColumnOperations.ExtractHierarchyLevelNames(toTable, hierarchy.Name);
      }
    }
    return variation2;
  }

  private static void ValidateVariation(Microsoft.AnalysisServices.Tabular.Variation variation, Microsoft.AnalysisServices.Tabular.Database database)
  {
    Microsoft.AnalysisServices.Tabular.Relationship relationship = variation.Relationship != null ? variation.Relationship : throw new McpException("Variation must have a Relationship property. Other variation types are not supported in the simplified scenario.");
    if (database.Model.Relationships.Find(relationship.Name) == null)
      throw new McpException($"Relationship '{relationship.Name}' referenced by variation does not exist in the model.");
    Microsoft.AnalysisServices.Tabular.Table toTable = relationship.ToTable;
    if (toTable == null)
      throw new McpException($"Relationship '{relationship.Name}' does not have a valid ToTable.");
    if (!toTable.IsHidden)
      throw new McpException($"Table '{toTable.Name}' referenced by variation relationship must be hidden.");
    if (toTable.Hierarchies == null || !Enumerable.Any<Microsoft.AnalysisServices.Tabular.Hierarchy>((IEnumerable<Microsoft.AnalysisServices.Tabular.Hierarchy>) toTable.Hierarchies))
      throw new McpException($"Hidden table '{toTable.Name}' must contain at least one hierarchy.");
  }

  private static List<string> ExtractHierarchyLevelNames(Microsoft.AnalysisServices.Tabular.Table table, string hierarchyName)
  {
    Microsoft.AnalysisServices.Tabular.Hierarchy hierarchy = table.Hierarchies.Find(hierarchyName);
    if (hierarchy == null)
      throw new McpException($"Hierarchy '{hierarchyName}' not found in table '{table.Name}'.");
    List<string> hierarchyLevelNames = new List<string>();
    foreach (Microsoft.AnalysisServices.Tabular.Level level in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.Level, Microsoft.AnalysisServices.Tabular.Hierarchy>) hierarchy.Levels)
      hierarchyLevelNames.Add(level.Name);
    return hierarchyLevelNames;
  }

  private static void ApplyAlternateOf(Microsoft.AnalysisServices.Tabular.Column column, AlternateOfDefinition alternateOf)
  {
    try
    {
      Microsoft.AnalysisServices.Tabular.AlternateOf alternateOf1 = new Microsoft.AnalysisServices.Tabular.AlternateOf();
      if (!string.IsNullOrWhiteSpace(alternateOf.BaseColumn))
      {
        Microsoft.AnalysisServices.Tabular.Table table = column.Table.Model.Tables.Find(alternateOf.BaseTable);
        if (table == null)
        {
          string str = string.Join(", ", Enumerable.Select<Microsoft.AnalysisServices.Tabular.Table, string>((IEnumerable<Microsoft.AnalysisServices.Tabular.Table>) column.Table.Model.Tables, (t => t.Name)));
          throw new McpException($"AlternateOf BaseTable '{alternateOf.BaseTable}' not found. Available tables: {str}");
        }
        Microsoft.AnalysisServices.Tabular.Column column1 = table.Columns.Find(alternateOf.BaseColumn);
        if (column1 == null)
        {
          string str = string.Join(", ", Enumerable.Select<Microsoft.AnalysisServices.Tabular.Column, string>((IEnumerable<Microsoft.AnalysisServices.Tabular.Column>) table.Columns, (c => c.Name)));
          throw new McpException($"AlternateOf BaseColumn '{alternateOf.BaseColumn}' not found in table '{alternateOf.BaseTable}'. Available columns: {str}");
        }
        alternateOf1.BaseColumn = column1;
      }
      else
      {
        Microsoft.AnalysisServices.Tabular.Table table = column.Table.Model.Tables.Find(alternateOf.BaseTable);
        if (table == null)
        {
          string str = string.Join(", ", Enumerable.Select<Microsoft.AnalysisServices.Tabular.Table, string>((IEnumerable<Microsoft.AnalysisServices.Tabular.Table>) column.Table.Model.Tables, (t => t.Name)));
          throw new McpException($"AlternateOf BaseTable '{alternateOf.BaseTable}' not found. Available tables: {str}");
        }
        alternateOf1.BaseTable = table;
      }
      string str1 = !string.IsNullOrWhiteSpace(alternateOf.BaseColumn) ? (string.IsNullOrWhiteSpace(alternateOf.Summarization) ? "GroupBy" : alternateOf.Summarization) : (string.IsNullOrWhiteSpace(alternateOf.Summarization) ? "Count" : alternateOf.Summarization);
      SummarizationType summarizationType;
      alternateOf1.Summarization = Enum.TryParse<SummarizationType>(str1, true, out summarizationType) ? summarizationType : throw new McpException($"Invalid AlternateOf Summarization '{str1}'. Valid values: GroupBy, Sum, Count, Min, Max");
      foreach (KeyValuePair<string, string> annotation in alternateOf.Annotations)
      {
        AlternateOfAnnotationCollection annotations = alternateOf1.Annotations;
        Microsoft.AnalysisServices.Tabular.Annotation metadataObject = new Microsoft.AnalysisServices.Tabular.Annotation();
        metadataObject.Name = annotation.Key;
        metadataObject.Value = annotation.Value;
        annotations.Add(metadataObject);
      }
      column.AlternateOf = alternateOf1;
    }
    catch (Exception ex)
    {
      throw new McpException($"Failed to apply AlternateOf to column '{column.Name}': {ex.Message}", ex);
    }
  }

  private static AlternateOfDefinition? ExtractAlternateOf(Microsoft.AnalysisServices.Tabular.Column column)
  {
    if (column.AlternateOf == null)
      return (AlternateOfDefinition) null;
    AlternateOfDefinition alternateOf = new AlternateOfDefinition();
    if (column.AlternateOf.BaseTable != null)
      alternateOf.BaseTable = column.AlternateOf.BaseTable.Name;
    if (column.AlternateOf.BaseColumn != null)
      alternateOf.BaseColumn = column.AlternateOf.BaseColumn.Name;
    alternateOf.Summarization = column.AlternateOf.Summarization.ToString();
    foreach (Microsoft.AnalysisServices.Tabular.Annotation annotation in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.Annotation, Microsoft.AnalysisServices.Tabular.AlternateOf>) column.AlternateOf.Annotations)
      alternateOf.Annotations.Add(new KeyValuePair<string, string>(annotation.Name, annotation.Value));
    return alternateOf;
  }

  private static bool CompareAlternateOf(
    AlternateOfDefinition? current,
    AlternateOfDefinition? updated)
  {
    if (current == null && updated == null)
      return true;
    return current != null && updated != null && (current.BaseTable == updated.BaseTable) && (current.BaseColumn == updated.BaseColumn) && (current.Summarization == updated.Summarization) && Enumerable.SequenceEqual<KeyValuePair<string, string>>((IEnumerable<KeyValuePair<string, string>>) current.Annotations, (IEnumerable<KeyValuePair<string, string>>) updated.Annotations);
  }

  public static List<TableColumnList> ListColumns(
    string? connectionName,
    string? tableName,
    int? maxResults,
    out int totalCount)
  {
    Microsoft.AnalysisServices.Tabular.Database database = ConnectionOperations.Get(connectionName).Database;
    IEnumerable<Microsoft.AnalysisServices.Tabular.Table> tables;
    if (!string.IsNullOrWhiteSpace(tableName))
      tables = (IEnumerable<Microsoft.AnalysisServices.Tabular.Table>) new Microsoft.AnalysisServices.Tabular.Table[1]
      {
        database.Model.Tables.Find(tableName) ?? throw new McpException($"Table '{tableName}' not found")
      };
    else
      tables = (IEnumerable<Microsoft.AnalysisServices.Tabular.Table>) database.Model.Tables;
    List<TableColumnList> list = Enumerable.ToList<TableColumnList>(Enumerable.Where<TableColumnList>(Enumerable.Select<Microsoft.AnalysisServices.Tabular.Table, TableColumnList>(tables, (t => new TableColumnList()
    {
      TableName = t.Name,
      Columns = Enumerable.ToList<ColumnList>(Enumerable.Select<Microsoft.AnalysisServices.Tabular.Column, ColumnList>(Enumerable.Where<Microsoft.AnalysisServices.Tabular.Column>((IEnumerable<Microsoft.AnalysisServices.Tabular.Column>) t.Columns, (c => !(c is RowNumberColumn))), (c =>
      {
        return new ColumnList()
        {
          Name = c.Name,
          Description = !string.IsNullOrEmpty(c.Description) ? c.Description : (string) null,
          DataType = c.DataType.ToString(),
          IsCalculated = !(c is Microsoft.AnalysisServices.Tabular.CalculatedColumn calculatedColumn2) || string.IsNullOrEmpty(calculatedColumn2.Expression) ? new bool?() : new bool?(true),
          DisplayFolder = !string.IsNullOrEmpty(c.DisplayFolder) ? c.DisplayFolder : (string) null
        };
      })))
    })), (g => Enumerable.Any<ColumnList>((IEnumerable<ColumnList>) g.Columns))));
    totalCount = Enumerable.Sum<TableColumnList>((IEnumerable<TableColumnList>) list, (g => g.Columns.Count));
    if (!maxResults.HasValue || maxResults.Value <= 0)
      return list;
    int num = maxResults.Value;
    List<TableColumnList> tableColumnListList = new List<TableColumnList>();
    foreach (TableColumnList tableColumnList in list)
    {
      if (num > 0)
      {
        if (tableColumnList.Columns.Count <= num)
        {
          tableColumnListList.Add(tableColumnList);
          num -= tableColumnList.Columns.Count;
        }
        else
        {
          tableColumnListList.Add(new TableColumnList()
          {
            TableName = tableColumnList.TableName,
            Columns = Enumerable.ToList<ColumnList>(Enumerable.Take<ColumnList>((IEnumerable<ColumnList>) tableColumnList.Columns, num))
          });
          num = 0;
        }
      }
      else
        break;
    }
    return tableColumnListList;
  }

  public static ColumnGet GetColumn(string? connectionName, string tableName, string columnName)
  {
    if (string.IsNullOrWhiteSpace(tableName))
      throw new McpException("tableName is required");
    if (string.IsNullOrWhiteSpace(columnName))
      throw new McpException("columnName is required");
    Microsoft.AnalysisServices.Tabular.Database database = ConnectionOperations.Get(connectionName).Database;
    Microsoft.AnalysisServices.Tabular.Column column1 = (database.Model.Tables.Find(tableName) ?? throw new McpException($"Table '{tableName}' not found")).Columns.Find(columnName) ?? throw new McpException($"Column '{columnName}' not found in table '{tableName}'");
    string str1;
    switch (column1)
    {
      case RowNumberColumn _:
        throw new McpException($"Column '{columnName}' is a RowNumber column and is not accessible");
      case Microsoft.AnalysisServices.Tabular.DataColumn _:
        str1 = "Data";
        break;
      case Microsoft.AnalysisServices.Tabular.CalculatedColumn _:
        str1 = "Calculated";
        break;
      case Microsoft.AnalysisServices.Tabular.CalculatedTableColumn _:
        str1 = "CalculatedTableColumn";
        break;
      default:
        str1 = "Unknown";
        break;
    }
    string str2 = str1;
    ColumnGet columnGet = new ColumnGet { TableName = tableName };
    columnGet.Name = column1.Name;
    columnGet.SourceColumn = column1 is Microsoft.AnalysisServices.Tabular.DataColumn dataColumn ? dataColumn.SourceColumn : (string) null;
    columnGet.Expression = column1 is Microsoft.AnalysisServices.Tabular.CalculatedColumn calculatedColumn ? calculatedColumn.Expression : (string) null;
    columnGet.DataType = column1.DataType.ToString();
    columnGet.DataCategory = column1.DataCategory;
    columnGet.FormatString = column1.FormatString;
    columnGet.SummarizeBy = column1.SummarizeBy.ToString();
    columnGet.DefaultLabel = new bool?(column1.IsDefaultLabel);
    columnGet.DefaultImage = new bool?(column1.IsDefaultImage);
    columnGet.IsHidden = new bool?(column1.IsHidden);
    columnGet.IsUnique = new bool?(column1.IsUnique);
    columnGet.IsKey = new bool?(column1.IsKey);
    columnGet.IsNullable = new bool?(column1.IsNullable);
    columnGet.DisplayFolder = column1.DisplayFolder;
    columnGet.SortByColumn = column1.SortByColumn?.Name;
    columnGet.SourceProviderType = column1.SourceProviderType;
    columnGet.Description = column1.Description;
    columnGet.IsAvailableInMDX = new bool?(column1.IsAvailableInMDX);
    columnGet.Alignment = column1.Alignment.ToString();
    columnGet.TableDetailPosition = new int?(column1.TableDetailPosition);
    columnGet.ColumnType = str2;
    columnGet.State = column1.State.ToString();
    columnGet.ErrorMessage = column1.ErrorMessage;
    columnGet.Annotations = new List<KeyValuePair<string, string>>();
    ColumnGet column2 = columnGet;
    if (column1.Annotations != null)
    {
      foreach (Microsoft.AnalysisServices.Tabular.Annotation annotation in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.Annotation, Microsoft.AnalysisServices.Tabular.Column>) column1.Annotations)
        column2.Annotations.Add(new KeyValuePair<string, string>(annotation.Name ?? string.Empty, annotation.Value ?? string.Empty));
    }
    column2.ExtendedProperties = ExtendedPropertyHelpers.ExtractFromColumn(column1);
    column2.AlternateOf = ColumnOperations.ExtractAlternateOf(column1);
    column2.GroupByColumns = ColumnOperations.ExtractGroupByColumns(column1);
    column2.Variation = ColumnOperations.ExtractVariation(column1, database);
    return column2;
  }

  public static string ExportTMDL(
    string? connectionName,
    string tableName,
    string columnName,
    ExportTmdl? options)
  {
    if (string.IsNullOrWhiteSpace(tableName))
      throw new McpException("tableName is required");
    if (string.IsNullOrWhiteSpace(columnName))
      throw new McpException("columnName is required");
    Microsoft.AnalysisServices.Tabular.Column @object = (ConnectionOperations.Get(connectionName).Database.Model.Tables.Find(tableName) ?? throw new McpException($"Table '{tableName}' not found")).Columns.Find(columnName) ?? throw new McpException($"Column '{columnName}' not found in table '{tableName}'");
    if (@object is RowNumberColumn)
      throw new McpException($"Column '{columnName}' is a RowNumber column and is not accessible");
    if (options?.SerializationOptions == null)
      return TmdlSerializer.SerializeObject((MetadataObject) @object);
    MetadataSerializationOptions serializationOptions = options.SerializationOptions.ToMetadataSerializationOptions();
    return TmdlSerializer.SerializeObject((MetadataObject) @object, serializationOptions);
  }

  public static ColumnOperations.ColumnOperationResult CreateColumn(
    string? connectionName,
    ColumnCreate def)
  {
    ColumnOperations.ValidateBase((ColumnBase) def, true);
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Table table1 = info.Database.Model.Tables.Find(def.TableName) ?? throw new McpException($"Table '{def.TableName}' not found");
    if (table1.Columns.Contains(def.Name))
      throw new McpException($"Column [{def.Name}] already exists in table '{def.TableName}'");
    Microsoft.AnalysisServices.Tabular.Column column1;
    if (!string.IsNullOrWhiteSpace(def.Expression))
    {
      Microsoft.AnalysisServices.Tabular.CalculatedColumn calculatedColumn = new Microsoft.AnalysisServices.Tabular.CalculatedColumn();
      calculatedColumn.Name = def.Name;
      calculatedColumn.Expression = def.Expression;
      column1 = (Microsoft.AnalysisServices.Tabular.Column) calculatedColumn;
    }
    else
    {
      column1 = (Microsoft.AnalysisServices.Tabular.Column) new Microsoft.AnalysisServices.Tabular.DataColumn();
      column1.Name = def.Name;
      ((Microsoft.AnalysisServices.Tabular.DataColumn) column1).SourceColumn = def.SourceColumn;
    }
    Microsoft.AnalysisServices.Tabular.Column column2 = column1;
    if (!string.IsNullOrWhiteSpace(def.DataType))
    {
      DataType dataType;
      if (!Enum.TryParse<DataType>(def.DataType, true, out dataType))
        throw new McpException($"Invalid DataType '{def.DataType}'. Valid values are: {string.Join(", ", Enum.GetNames(typeof (DataType)))}");
      column2.DataType = dataType;
    }
    column2.DataCategory = def.DataCategory;
    column2.FormatString = def.FormatString;
    if (!string.IsNullOrWhiteSpace(def.SummarizeBy))
    {
      AggregateFunction aggregateFunction;
      if (Enum.TryParse<AggregateFunction>(def.SummarizeBy, true, out aggregateFunction))
      {
        column2.SummarizeBy = aggregateFunction;
      }
      else
      {
        string[] names = Enum.GetNames(typeof (AggregateFunction));
        throw new McpException($"Invalid SummarizeBy '{def.SummarizeBy}'. Valid values are: {string.Join(", ", names)}");
      }
    }
    if (def.DefaultLabel.HasValue)
      column2.IsDefaultLabel = def.DefaultLabel.Value;
    if (def.DefaultImage.HasValue)
      column2.IsDefaultImage = def.DefaultImage.Value;
    if (def.IsHidden.HasValue)
      column2.IsHidden = def.IsHidden.Value;
    if (def.IsUnique.HasValue)
      column2.IsUnique = def.IsUnique.Value;
    bool? nullable;
    if (def.IsKey.HasValue)
    {
      Microsoft.AnalysisServices.Tabular.Table table2 = table1;
      nullable = def.IsKey;
      int num1 = nullable.Value ? 1 : 0;
      ColumnOperations.ValidateKeyColumnConstraint(table2, (string) null, num1 != 0);
      Microsoft.AnalysisServices.Tabular.Column column3 = column2;
      nullable = def.IsKey;
      int num2 = nullable.Value ? 1 : 0;
      column3.IsKey = num2 != 0;
    }
    nullable = def.IsNullable;
    if (nullable.HasValue)
    {
      Microsoft.AnalysisServices.Tabular.Column column4 = column2;
      nullable = def.IsNullable;
      int num = nullable.Value ? 1 : 0;
      column4.IsNullable = num != 0;
    }
    column2.DisplayFolder = def.DisplayFolder;
    if (!string.IsNullOrWhiteSpace(def.SortByColumn))
      column2.SortByColumn = table1.Columns.Find(def.SortByColumn) ?? throw new McpException($"SortByColumn '{def.SortByColumn}' not found in table '{def.TableName}'");
    column2.SourceProviderType = def.SourceProviderType;
    column2.Description = def.Description;
    nullable = def.IsAvailableInMDX;
    if (nullable.HasValue)
    {
      Microsoft.AnalysisServices.Tabular.Column column5 = column2;
      nullable = def.IsAvailableInMDX;
      int num = nullable.Value ? 1 : 0;
      column5.IsAvailableInMDX = num != 0;
    }
    if (!string.IsNullOrWhiteSpace(def.Alignment))
    {
      Alignment alignment;
      if (!Enum.TryParse<Alignment>(def.Alignment, true, out alignment))
        throw new McpException($"Invalid Alignment '{def.Alignment}'. Valid values are: Default, Left, Right, Center");
      column2.Alignment = alignment;
    }
    if (def.TableDetailPosition.HasValue)
      column2.TableDetailPosition = def.TableDetailPosition.Value;
    if (def.Annotations != null)
      AnnotationHelpers.ApplyAnnotations<Microsoft.AnalysisServices.Tabular.Column>(column2, def.Annotations, (Func<Microsoft.AnalysisServices.Tabular.Column, ICollection<Microsoft.AnalysisServices.Tabular.Annotation>>) (c => (ICollection<Microsoft.AnalysisServices.Tabular.Annotation>) c.Annotations));
    if (def.ExtendedProperties != null)
      ExtendedPropertyHelpers.ApplyToColumn(column2, def.ExtendedProperties);
    table1.Columns.Add(column2);
    if (def.AlternateOf != null)
    {
      ColumnOperations.ValidateAlternateOf(def.AlternateOf, def.TableName, info.Database);
      ColumnOperations.ApplyAlternateOf(column2, def.AlternateOf);
    }
    if (def.GroupByColumns != null && def.GroupByColumns.Count > 0)
    {
      ColumnOperations.ValidateGroupByColumns(def.GroupByColumns, def.TableName, def.Name, info.Database);
      ColumnOperations.ApplyGroupByColumns(column2, def.GroupByColumns);
    }
    TransactionOperations.RecordOperation(info, $"Created column [{def.Name}] in table '{def.TableName}'");
    ConnectionOperations.SaveChangesWithRollback(info, "create column");
    string str1;
    switch (column2)
    {
      case Microsoft.AnalysisServices.Tabular.DataColumn _:
        str1 = "Data";
        break;
      case Microsoft.AnalysisServices.Tabular.CalculatedColumn _:
        str1 = "Calculated";
        break;
      case Microsoft.AnalysisServices.Tabular.CalculatedTableColumn _:
        str1 = "CalculatedTableColumn";
        break;
      case RowNumberColumn _:
        str1 = "RowNumber";
        break;
      default:
        str1 = "Unknown";
        break;
    }
    string str2 = str1;
    return new ColumnOperations.ColumnOperationResult()
    {
      State = column2.State.ToString(),
      ErrorMessage = column2.ErrorMessage,
      ColumnName = column2.Name,
      TableName = table1.Name,
      ColumnType = str2
    };
  }

  public static ColumnOperations.ColumnOperationResult UpdateColumn(
    string? connectionName,
    ColumnUpdate update)
  {
    ColumnOperations.ValidateBase((ColumnBase) update, false);
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Database database = info.Database;
    Microsoft.AnalysisServices.Tabular.Table table1 = database.Model.Tables.Find(update.TableName) ?? throw new McpException($"Table '{update.TableName}' not found");
    Microsoft.AnalysisServices.Tabular.Column column1 = table1.Columns.Find(update.Name) ?? throw new McpException($"Column '{update.Name}' not found in table '{update.TableName}'");
    if (column1 is RowNumberColumn)
      throw new McpException($"Column '{update.Name}' is a RowNumber column and cannot be updated");
    bool flag = false;
    if (!string.IsNullOrWhiteSpace(update.Expression))
    {
      if (!(column1 is Microsoft.AnalysisServices.Tabular.CalculatedColumn calculatedColumn))
        throw new McpException("Cannot set Expression on a non-calculated column");
      if ((calculatedColumn.Expression != update.Expression))
      {
        calculatedColumn.Expression = update.Expression;
        flag = true;
      }
    }
    if (!string.IsNullOrWhiteSpace(update.SourceColumn))
    {
      if (!(column1 is Microsoft.AnalysisServices.Tabular.DataColumn dataColumn))
        throw new McpException("Cannot set SourceColumn on a calculated column");
      if ((dataColumn.SourceColumn != update.SourceColumn))
      {
        dataColumn.SourceColumn = update.SourceColumn;
        flag = true;
      }
    }
    if (!string.IsNullOrWhiteSpace(update.DataType))
    {
      DataType dataType;
      if (Enum.TryParse<DataType>(update.DataType, true, out dataType))
      {
        if (column1.DataType != dataType)
        {
          column1.DataType = dataType;
          flag = true;
        }
      }
      else
      {
        string[] names = Enum.GetNames(typeof (DataType));
        throw new McpException($"Invalid DataType '{update.DataType}'. Valid values are: {string.Join(", ", names)}");
      }
    }
    if (update.DataCategory != null)
    {
      string dataCategory = string.IsNullOrEmpty(update.DataCategory) ? (string) null : update.DataCategory;
      if ((dataCategory != column1.DataCategory))
      {
        column1.DataCategory = dataCategory;
        flag = true;
      }
    }
    if (update.FormatString != null)
    {
      string formatString = string.IsNullOrEmpty(update.FormatString) ? (string) null : update.FormatString;
      if ((formatString != column1.FormatString))
      {
        column1.FormatString = formatString;
        flag = true;
      }
    }
    if (!string.IsNullOrWhiteSpace(update.SummarizeBy))
    {
      AggregateFunction aggregateFunction;
      if (Enum.TryParse<AggregateFunction>(update.SummarizeBy, true, out aggregateFunction))
      {
        if (column1.SummarizeBy != aggregateFunction)
        {
          column1.SummarizeBy = aggregateFunction;
          flag = true;
        }
      }
      else
      {
        string[] names = Enum.GetNames(typeof (AggregateFunction));
        throw new McpException($"Invalid SummarizeBy '{update.SummarizeBy}'. Valid values are: {string.Join(", ", names)}");
      }
    }
    bool? nullable;
    if (update.DefaultLabel.HasValue)
    {
      int num1 = column1.IsDefaultLabel ? 1 : 0;
      nullable = update.DefaultLabel;
      int num2 = nullable.Value ? 1 : 0;
      if (num1 != num2)
      {
        Microsoft.AnalysisServices.Tabular.Column column2 = column1;
        nullable = update.DefaultLabel;
        int num3 = nullable.Value ? 1 : 0;
        column2.IsDefaultLabel = num3 != 0;
        flag = true;
      }
    }
    nullable = update.DefaultImage;
    if (nullable.HasValue)
    {
      int num4 = column1.IsDefaultImage ? 1 : 0;
      nullable = update.DefaultImage;
      int num5 = nullable.Value ? 1 : 0;
      if (num4 != num5)
      {
        Microsoft.AnalysisServices.Tabular.Column column3 = column1;
        nullable = update.DefaultImage;
        int num6 = nullable.Value ? 1 : 0;
        column3.IsDefaultImage = num6 != 0;
        flag = true;
      }
    }
    nullable = update.IsHidden;
    if (nullable.HasValue)
    {
      int num7 = column1.IsHidden ? 1 : 0;
      nullable = update.IsHidden;
      int num8 = nullable.Value ? 1 : 0;
      if (num7 != num8)
      {
        Microsoft.AnalysisServices.Tabular.Column column4 = column1;
        nullable = update.IsHidden;
        int num9 = nullable.Value ? 1 : 0;
        column4.IsHidden = num9 != 0;
        flag = true;
      }
    }
    nullable = update.IsUnique;
    if (nullable.HasValue)
    {
      int num10 = column1.IsUnique ? 1 : 0;
      nullable = update.IsUnique;
      int num11 = nullable.Value ? 1 : 0;
      if (num10 != num11)
      {
        Microsoft.AnalysisServices.Tabular.Column column5 = column1;
        nullable = update.IsUnique;
        int num12 = nullable.Value ? 1 : 0;
        column5.IsUnique = num12 != 0;
        flag = true;
      }
    }
    nullable = update.IsKey;
    if (nullable.HasValue)
    {
      int num13 = column1.IsKey ? 1 : 0;
      nullable = update.IsKey;
      int num14 = nullable.Value ? 1 : 0;
      if (num13 != num14)
      {
        Microsoft.AnalysisServices.Tabular.Table table2 = table1;
        string name = column1.Name;
        nullable = update.IsKey;
        int num15 = nullable.Value ? 1 : 0;
        ColumnOperations.ValidateKeyColumnConstraint(table2, name, num15 != 0);
        Microsoft.AnalysisServices.Tabular.Column column6 = column1;
        nullable = update.IsKey;
        int num16 = nullable.Value ? 1 : 0;
        column6.IsKey = num16 != 0;
        flag = true;
      }
    }
    nullable = update.IsNullable;
    if (nullable.HasValue)
    {
      int num17 = column1.IsNullable ? 1 : 0;
      nullable = update.IsNullable;
      int num18 = nullable.Value ? 1 : 0;
      if (num17 != num18)
      {
        Microsoft.AnalysisServices.Tabular.Column column7 = column1;
        nullable = update.IsNullable;
        int num19 = nullable.Value ? 1 : 0;
        column7.IsNullable = num19 != 0;
        flag = true;
      }
    }
    if (update.DisplayFolder != null)
    {
      string displayFolder = string.IsNullOrEmpty(update.DisplayFolder) ? (string) null : update.DisplayFolder;
      if ((displayFolder != column1.DisplayFolder))
      {
        column1.DisplayFolder = displayFolder;
        flag = true;
      }
    }
    if (update.SortByColumn != null)
    {
      if (string.IsNullOrEmpty(update.SortByColumn))
      {
        if (column1.SortByColumn != null)
        {
          column1.SortByColumn = (Microsoft.AnalysisServices.Tabular.Column) null;
          flag = true;
        }
      }
      else
      {
        Microsoft.AnalysisServices.Tabular.Column column8 = table1.Columns.Find(update.SortByColumn);
        if (column8 == null)
          throw new McpException($"SortByColumn '{update.SortByColumn}' not found in table '{update.TableName}'");
        if (column1.SortByColumn != column8)
        {
          column1.SortByColumn = column8;
          flag = true;
        }
      }
    }
    if (update.SourceProviderType != null)
    {
      string sourceProviderType = string.IsNullOrEmpty(update.SourceProviderType) ? (string) null : update.SourceProviderType;
      if ((sourceProviderType != column1.SourceProviderType))
      {
        column1.SourceProviderType = sourceProviderType;
        flag = true;
      }
    }
    if (update.Description != null)
    {
      string description = string.IsNullOrEmpty(update.Description) ? (string) null : update.Description;
      if ((description != column1.Description))
      {
        column1.Description = description;
        flag = true;
      }
    }
    nullable = update.IsAvailableInMDX;
    if (nullable.HasValue)
    {
      int num20 = column1.IsAvailableInMDX ? 1 : 0;
      nullable = update.IsAvailableInMDX;
      int num21 = nullable.Value ? 1 : 0;
      if (num20 != num21)
      {
        Microsoft.AnalysisServices.Tabular.Column column9 = column1;
        nullable = update.IsAvailableInMDX;
        int num22 = nullable.Value ? 1 : 0;
        column9.IsAvailableInMDX = num22 != 0;
        flag = true;
      }
    }
    if (!string.IsNullOrWhiteSpace(update.Alignment))
    {
      Alignment alignment;
      if (!Enum.TryParse<Alignment>(update.Alignment, true, out alignment))
        throw new McpException($"Invalid Alignment '{update.Alignment}'. Valid values are: Default, Left, Right, Center");
      if (column1.Alignment != alignment)
      {
        column1.Alignment = alignment;
        flag = true;
      }
    }
    if (update.TableDetailPosition.HasValue)
    {
      int tableDetailPosition1 = column1.TableDetailPosition;
      int? tableDetailPosition2 = update.TableDetailPosition;
      int num23 = tableDetailPosition2.Value;
      if (tableDetailPosition1 != num23)
      {
        Microsoft.AnalysisServices.Tabular.Column column10 = column1;
        tableDetailPosition2 = update.TableDetailPosition;
        int num24 = tableDetailPosition2.Value;
        column10.TableDetailPosition = num24;
        flag = true;
      }
    }
    if (update.Annotations != null && AnnotationHelpers.ReplaceAnnotations<Microsoft.AnalysisServices.Tabular.Column>(column1, update.Annotations, (Func<Microsoft.AnalysisServices.Tabular.Column, ICollection<Microsoft.AnalysisServices.Tabular.Annotation>>) (c => (ICollection<Microsoft.AnalysisServices.Tabular.Annotation>) c.Annotations)))
      flag = true;
    if (update.ExtendedProperties != null)
    {
      int num = column1.ExtendedProperties.Count > 0 ? 1 : 0;
      ExtendedPropertyHelpers.ReplaceExtendedProperties<Microsoft.AnalysisServices.Tabular.Column>(column1, update.ExtendedProperties, (Func<Microsoft.AnalysisServices.Tabular.Column, ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>>) (c => (ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>) c.ExtendedProperties));
      if (num != 0 || update.ExtendedProperties.Count > 0)
        flag = true;
    }
    if (update.AlternateOf != null)
    {
      if (!ColumnOperations.CompareAlternateOf(ColumnOperations.ExtractAlternateOf(column1), update.AlternateOf))
      {
        ColumnOperations.ValidateAlternateOf(update.AlternateOf, update.TableName, database);
        ColumnOperations.ApplyAlternateOf(column1, update.AlternateOf);
        flag = true;
      }
    }
    else if (update.AlternateOf == null && column1.AlternateOf != null)
    {
      column1.AlternateOf = (Microsoft.AnalysisServices.Tabular.AlternateOf) null;
      flag = true;
    }
    if (update.GroupByColumns != null)
    {
      if (!ColumnOperations.CompareGroupByColumns(ColumnOperations.ExtractGroupByColumns(column1), update.GroupByColumns))
      {
        if (update.GroupByColumns.Count > 0)
          ColumnOperations.ValidateGroupByColumns(update.GroupByColumns, update.TableName, update.Name, database);
        ColumnOperations.ApplyGroupByColumns(column1, update.GroupByColumns);
        flag = true;
      }
    }
    else if (update.GroupByColumns == null && ColumnOperations.ExtractGroupByColumns(column1) != null)
    {
      ColumnOperations.ApplyGroupByColumns(column1, new List<string>());
      flag = true;
    }
    if (!flag)
    {
      string str1;
      switch (column1)
      {
        case Microsoft.AnalysisServices.Tabular.DataColumn _:
          str1 = "Data";
          break;
        case Microsoft.AnalysisServices.Tabular.CalculatedColumn _:
          str1 = "Calculated";
          break;
        case Microsoft.AnalysisServices.Tabular.CalculatedTableColumn _:
          str1 = "CalculatedTableColumn";
          break;
        case RowNumberColumn _:
          str1 = "RowNumber";
          break;
        default:
          str1 = "Unknown";
          break;
      }
      string str2 = str1;
      return new ColumnOperations.ColumnOperationResult()
      {
        State = column1.State.ToString(),
        ErrorMessage = column1.ErrorMessage,
        ColumnName = column1.Name,
        TableName = table1.Name,
        ColumnType = str2,
        HasChanges = false
      };
    }
    TransactionOperations.RecordOperation(info, $"Updated column '{update.Name}' in table '{update.TableName}'");
    ConnectionOperations.SaveChangesWithRollback(info, "update column");
    string str3;
    switch (column1)
    {
      case Microsoft.AnalysisServices.Tabular.DataColumn _:
        str3 = "Data";
        break;
      case Microsoft.AnalysisServices.Tabular.CalculatedColumn _:
        str3 = "Calculated";
        break;
      case Microsoft.AnalysisServices.Tabular.CalculatedTableColumn _:
        str3 = "CalculatedTableColumn";
        break;
      case RowNumberColumn _:
        str3 = "RowNumber";
        break;
      default:
        str3 = "Unknown";
        break;
    }
    string str4 = str3;
    return new ColumnOperations.ColumnOperationResult()
    {
      State = column1.State.ToString(),
      ErrorMessage = column1.ErrorMessage,
      ColumnName = column1.Name,
      TableName = table1.Name,
      ColumnType = str4,
      HasChanges = true
    };
  }

  public static void RenameColumn(
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
    Microsoft.AnalysisServices.Tabular.Column column = table.Columns.Find(oldName) ?? throw new McpException($"Column '{oldName}' not found in table '{tableName}'");
    if (column is RowNumberColumn)
      throw new McpException($"Column '{oldName}' is a RowNumber column and cannot be renamed");
    if (table.Columns.Contains(newName) && !string.Equals(oldName, newName, StringComparison.OrdinalIgnoreCase))
      throw new McpException($"Column '{newName}' already exists in table '{tableName}'");
    column.RequestRename(newName);
    TransactionOperations.RecordOperation(info, $"Renamed column '{oldName}' to '{newName}' in table '{tableName}'");
    ConnectionOperations.SaveChangesWithRollback(info, "rename column", ConnectionOperations.CheckpointMode.AfterRequestRename);
  }

  public static List<string> DeleteColumn(
    string? connectionName,
    string tableName,
    string columnName,
    bool shouldCascadeDelete)
  {
    List<string> stringList1 = new List<string>();
    if (string.IsNullOrWhiteSpace(tableName))
      throw new McpException("tableName is required");
    if (string.IsNullOrWhiteSpace(columnName))
      throw new McpException("columnName is required");
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Database database = info.Database;
    Microsoft.AnalysisServices.Tabular.Table table = database.Model.Tables.Find(tableName) ?? throw new McpException($"Table '{tableName}' not found");
    Microsoft.AnalysisServices.Tabular.Column col = table.Columns.Find(columnName) ?? throw new McpException($"Column '{columnName}' not found in table '{tableName}'");
    if (col is RowNumberColumn)
      throw new McpException($"Column '{columnName}' is a RowNumber column and cannot be deleted");
    if (table.Partitions.Count == 0)
      throw new McpException($"Cannot delete column '{columnName}' from table '{tableName}' because the table has no partitions. \r\nDon't know how to handle column deletion from a table with no partitions. \r\nDeleting columns requires modifying the partition source to remove the columns, but don't know how to make this happen automatically.");
    List<string> stringList2 = StructuralDependencyHelper.CheckAndDeleteDependenciesIfRequired(database, (NamedMetadataObject) col, shouldCascadeDelete);
    if (!shouldCascadeDelete)
    {
      if (Enumerable.Any<string>((IEnumerable<string>) stringList2))
        throw new McpException($"Cannot delete column '{columnName}' because it is used by: {string.Join(", ", (IEnumerable<string>) stringList2)}");
      List<string> list = Enumerable.ToList<string>(Enumerable.Select<Microsoft.AnalysisServices.Tabular.Column, string>(Enumerable.Where<Microsoft.AnalysisServices.Tabular.Column>((IEnumerable<Microsoft.AnalysisServices.Tabular.Column>) table.Columns, (c => c.SortByColumn == col)), (c => c.Name)));
      if (Enumerable.Any<string>((IEnumerable<string>) list))
        throw new McpException($"Cannot delete column '{columnName}' because it is used as SortByColumn for: {string.Join(", ", (IEnumerable<string>) list)}");
    }
    Microsoft.AnalysisServices.Tabular.Partition partition = table.Partitions[0];
    string str1 = Enumerable.Any<string>((IEnumerable<string>) stringList2) ? $"Dependencies have been removed: {string.Join(", ", (IEnumerable<string>) stringList2)}.\n" : "";
    if (partition.Source is CalculatedPartitionSource)
      throw new McpException($"{str1}Cannot delete column '{columnName}' from calculated table '{tableName}'. \r\nCalculated table columns are derived from the DAX expression. \r\nModify the DAX expression directly to eliminate unwanted columns.");
    if (partition.Source is MPartitionSource source)
    {
      string str2 = (col is Microsoft.AnalysisServices.Tabular.DataColumn dataColumn ? dataColumn.SourceColumn : (string) null) ?? columnName;
      string str3 = source.Expression ?? "";
      table.Columns.Remove(col);
      string str4 = $"{str1}Column '{columnName}' has been deleted from table '{tableName}' with M partition. \r\nWARNING: The column may be re-added automatically by other authoring tools unless you also modify the M expression of the partition to remove the column and then refresh the table. \r\nAdd a Table.RemoveColumns step to exclude the source column '{str2}'.\r\n\r\nCurrent M Expression:\r\n{str3}\r\n\r\nExample RemoveColumns syntax:\r\nlet\r\n    Source = Sql.Databases(\"ServerName\"),\r\n    SampleDB = Source{{[Name=\"SampleDatabase\"]}}[Data],\r\n    SampleTable = SampleDB{{[Schema=\"dbo\",Item=\"SampleTable\"]}}[Data],\r\n    #\"Removed Columns\" = Table.RemoveColumns(SampleTable, {{\"ColumnA\", \"ColumnB\", \"{str2}\"}}, MissingField.Ignore)\r\nin\r\n    #\"Removed Columns\"\r\n\r\nUse the partition_operations tool to modify the partition expression and then refresh the table.";
      stringList1.Add(str4);
    }
    else
    {
      table.Columns.Remove(col);
      string str5 = $"{str1}Column '{columnName}' has been deleted from table '{tableName}'. \r\nWARNING: The column may be re-added automatically by other authoring tools unless you also modify the source query of the partition to exclude the column and then refresh the table.\r\nUse the partition_operations tool to modify the partition to exclude the column, then refresh the table.";
      stringList1.Add(str5);
    }
    TransactionOperations.RecordOperation(info, $"Deleted column '{columnName}' from table '{tableName}'");
    ConnectionOperations.SaveChangesWithRollback(info, "delete column");
    return stringList1;
  }

  public static ColumnOperations.CalculatedColumnValidationResult ValidateCalculatedColumnExpression(
    string? connectionName,
    string tableName,
    string expression)
  {
    if (string.IsNullOrWhiteSpace(tableName))
      throw new McpException("tableName is required");
    if (string.IsNullOrWhiteSpace(expression))
      throw new McpException("expression is required");
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Table table = info.Database.Model.Tables.Find(tableName) ?? throw new McpException($"Table '{tableName}' not found");
    HashSet<string> stringSet = new HashSet<string>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    foreach (Microsoft.AnalysisServices.Tabular.Column column in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.Column, Microsoft.AnalysisServices.Tabular.Table>) table.Columns)
      stringSet.Add(column.Name);
    int num = 1;
    string str;
    do
    {
      str = $"__TempColumn{num}";
      ++num;
    }
    while (stringSet.Contains(str));
    ColumnOperations.CalculatedColumnValidationResult validationResult = new ColumnOperations.CalculatedColumnValidationResult()
    {
      Expression = expression
    };
    Stopwatch stopwatch = Stopwatch.StartNew();
    Microsoft.AnalysisServices.Tabular.CalculatedColumn metadataObject = (Microsoft.AnalysisServices.Tabular.CalculatedColumn) null;
    bool flag = false;
    try
    {
      Microsoft.AnalysisServices.Tabular.CalculatedColumn calculatedColumn = new Microsoft.AnalysisServices.Tabular.CalculatedColumn();
      calculatedColumn.Name = str;
      calculatedColumn.Expression = expression;
      metadataObject = calculatedColumn;
      table.Columns.Add((Microsoft.AnalysisServices.Tabular.Column) metadataObject);
      flag = true;
      ConnectionOperations.SaveChangesIfNeeded(info);
      validationResult.ObjectState = metadataObject.State.ToString();
      if (metadataObject.State != ObjectState.Ready)
      {
        validationResult.IsValid = false;
        validationResult.ErrorMessage = metadataObject.ErrorMessage ?? "Unknown error during validation.";
      }
      else
      {
        validationResult.IsValid = true;
        validationResult.Message = "Expression is valid.";
      }
    }
    catch (Exception ex)
    {
      validationResult.IsValid = false;
      validationResult.ErrorMessage = ex.Message;
    }
    finally
    {
      stopwatch.Stop();
      validationResult.ValidationTimeMs = stopwatch.ElapsedMilliseconds;
      if (flag)
      {
        if (metadataObject != null)
        {
          try
          {
            table.Columns.Remove((Microsoft.AnalysisServices.Tabular.Column) metadataObject);
            ConnectionOperations.SaveChangesIfNeeded(info, ConnectionOperations.CheckpointMode.ForceEvenInTransaction);
          }
          catch (Exception ex)
          {
          }
        }
      }
    }
    return validationResult;
  }

  public class ColumnOperationResult
  {
    public string State { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }

    public string ColumnName { get; set; } = string.Empty;

    public string TableName { get; set; } = string.Empty;

    public string ColumnType { get; set; } = string.Empty;

    public bool HasChanges { get; set; }
  }

  public class CalculatedColumnValidationResult
  {
    public bool IsValid { get; set; }

    public string? ObjectState { get; set; }

    public string? ErrorMessage { get; set; }

    public string Expression { get; set; } = string.Empty;

    public string? Message { get; set; }

    public long ValidationTimeMs { get; set; }
  }
}
