// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using Microsoft.AnalysisServices.Tabular;
using Microsoft.AnalysisServices.Tabular.Serialization;
using ModelContextProtocol;
using PowerBIModelingMCP.Library.Common;
using PowerBIModelingMCP.Library.Common.DataStructures;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

#nullable enable
namespace PowerBIModelingMCP.Library.Core;

public static class TableOperations
{
  public static void ValidateTableDefinition(TableBase def, bool isCreate)
  {
    if (def == null)
      throw new McpException("Table definition cannot be null");
    if (string.IsNullOrWhiteSpace(def.Name))
      throw new McpException("Name is required");
    if (isCreate && def is TableCreate tableCreate)
    {
      int num1 = Enumerable.Count<bool>((IEnumerable<bool>) new bool[4]
      {
        !string.IsNullOrWhiteSpace(tableCreate.DaxExpression),
        !string.IsNullOrWhiteSpace(tableCreate.MExpression),
        !string.IsNullOrWhiteSpace(tableCreate.SqlQuery),
        !string.IsNullOrWhiteSpace(tableCreate.EntityName)
      }, (x => x));
      if (num1 == 0)
        throw new McpException("One of DaxExpression, MExpression, EntityName, or SqlQuery must be provided");
      if (num1 > 1)
        throw new McpException("Only one of DaxExpression, MExpression, EntityName, or SqlQuery can be provided");
      if (!string.IsNullOrWhiteSpace(tableCreate.SqlQuery) && string.IsNullOrWhiteSpace(tableCreate.DataSourceName))
        throw new McpException("DataSourceName is required when SqlQuery is provided");
      if (!string.IsNullOrWhiteSpace(tableCreate.EntityName))
      {
        if (string.IsNullOrWhiteSpace(tableCreate.DataSourceName) && string.IsNullOrWhiteSpace(tableCreate.ExpressionSourceName))
          throw new McpException("Either ExpressionSourceName or DataSourceName is required when EntityName is provided");
        if (!string.IsNullOrWhiteSpace(tableCreate.DataSourceName) && !string.IsNullOrWhiteSpace(tableCreate.ExpressionSourceName))
          throw new McpException("Only one of ExpressionSourceName or DataSourceName can be provided when EntityName is specified");
      }
      int num2 = !string.IsNullOrWhiteSpace(tableCreate.DaxExpression) ? 1 : 0;
      if (num2 != 0 && tableCreate.Columns != null && tableCreate.Columns.Count > 0)
        throw new McpException("Columns cannot be specified for calculated tables. The columns are derived from the DAX expression.");
      if (num2 == 0 && tableCreate.Columns != null && tableCreate.Columns.Count > 0)
      {
        HashSet<string> stringSet = new HashSet<string>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
        List<string> stringList = new List<string>();
        foreach (ColumnCreate column in tableCreate.Columns)
        {
          if (string.IsNullOrWhiteSpace(column.Name))
            throw new McpException("All columns must have a valid name");
          if (!stringSet.Add(column.Name))
            stringList.Add(column.Name);
        }
        if (stringList.Count > 0)
          throw new McpException($"Duplicate column names found: {string.Join(", ", Enumerable.Distinct<string>((IEnumerable<string>) stringList))}. Each column name must be unique within the table.");
        List<ColumnCreate> list = Enumerable.ToList<ColumnCreate>(Enumerable.Where<ColumnCreate>((IEnumerable<ColumnCreate>) tableCreate.Columns, (c => c.IsKey.HasValue && c.IsKey.Value)));
        if (list.Count > 1)
          throw new McpException("Only one column per table can be designated as a key column. Found multiple key columns: " + string.Join(", ", Enumerable.Select<ColumnCreate, string>((IEnumerable<ColumnCreate>) list, (c => c.Name))));
      }
    }
    if (def.ExtendedProperties != null)
    {
      List<string> stringList = ExtendedPropertyHelpers.Validate(def.ExtendedProperties);
      if (stringList.Count > 0)
        throw new McpException("ExtendedProperties validation failed: " + string.Join(", ", (IEnumerable<string>) stringList));
    }
    Guid guid;
    if (!string.IsNullOrWhiteSpace(def.LineageTag) && !Guid.TryParse(def.LineageTag, out guid))
      throw new McpException("LineageTag must be a valid GUID format. Current value: " + def.LineageTag);
    if (!string.IsNullOrWhiteSpace(def.SourceLineageTag) && !Guid.TryParse(def.SourceLineageTag, out guid))
      throw new McpException("SourceLineageTag must be a valid GUID format. Current value: " + def.SourceLineageTag);
    AnnotationHelpers.ValidateAnnotations(def.Annotations);
  }

  public static List<TableList> ListTables(string? connectionName)
  {
    return Enumerable.ToList<TableList>(Enumerable.Select<Microsoft.AnalysisServices.Tabular.Table, TableList>((IEnumerable<Microsoft.AnalysisServices.Tabular.Table>) ConnectionOperations.Get(connectionName).Database.Model.Tables, (t =>
    {
      return new TableList()
      {
        Name = t.Name,
        Description = !string.IsNullOrEmpty(t.Description) ? t.Description : (string) null,
        ColumnCount = new int?(Enumerable.Count<Microsoft.AnalysisServices.Tabular.Column>((IEnumerable<Microsoft.AnalysisServices.Tabular.Column>) t.Columns, (c => !(c is RowNumberColumn)))),
        MeasureCount = t.Measures.Count > 0 ? new int?(t.Measures.Count) : new int?(),
        HierarchyCount = t.Hierarchies.Count > 0 ? new int?(t.Hierarchies.Count) : new int?(),
        PartitionCount = t.Partitions.Count > 0 ? new int?(t.Partitions.Count) : new int?(),
        CalendarCount = t.Calendars.Count > 0 ? new int?(t.Calendars.Count) : new int?()
      };
    })));
  }

  public static TableGet GetTable(string? connectionName, string tableName)
  {
    if (string.IsNullOrWhiteSpace(tableName))
      throw new McpException("tableName is required");
    Microsoft.AnalysisServices.Tabular.Table table1 = ConnectionOperations.Get(connectionName).Database.Model.Tables.Find(tableName) ?? throw new McpException($"Table '{tableName}' not found");
    TableGet tableGet = new TableGet { Name = table1.Name };
    tableGet.DataCategory = table1.DataCategory;
    tableGet.Description = table1.Description;
    tableGet.IsHidden = new bool?(table1.IsHidden);
    tableGet.ShowAsVariationsOnly = new bool?(table1.ShowAsVariationsOnly);
    tableGet.IsPrivate = new bool?(table1.IsPrivate);
    tableGet.AlternateSourcePrecedence = new int?(table1.AlternateSourcePrecedence);
    tableGet.ExcludeFromModelRefresh = new bool?(table1.ExcludeFromModelRefresh);
    tableGet.LineageTag = table1.LineageTag;
    tableGet.SourceLineageTag = table1.SourceLineageTag;
    tableGet.SystemManaged = new bool?(table1.SystemManaged);
    tableGet.Mode = Enumerable.FirstOrDefault<Microsoft.AnalysisServices.Tabular.Partition>((IEnumerable<Microsoft.AnalysisServices.Tabular.Partition>) table1.Partitions)?.Mode;
    tableGet.Columns = Enumerable.ToList<string>(Enumerable.Select<Microsoft.AnalysisServices.Tabular.Column, string>((IEnumerable<Microsoft.AnalysisServices.Tabular.Column>) table1.Columns, (c => c.Name)));
    tableGet.Measures = Enumerable.ToList<string>(Enumerable.Select<Microsoft.AnalysisServices.Tabular.Measure, string>((IEnumerable<Microsoft.AnalysisServices.Tabular.Measure>) table1.Measures, (m => m.Name)));
    tableGet.Hierarchies = Enumerable.ToList<string>(Enumerable.Select<Microsoft.AnalysisServices.Tabular.Hierarchy, string>((IEnumerable<Microsoft.AnalysisServices.Tabular.Hierarchy>) table1.Hierarchies, (h => h.Name)));
    tableGet.Annotations = new List<KeyValuePair<string, string>>();
    TableGet table2 = tableGet;
    List<PartitionGet> partitionGetList = PartitionOperations.ListPartitions(connectionName, tableName);
    table2.PartitionDetails = partitionGetList;
    if (table1.Annotations != null)
    {
      foreach (Microsoft.AnalysisServices.Tabular.Annotation annotation in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.Annotation, Microsoft.AnalysisServices.Tabular.Table>) table1.Annotations)
        table2.Annotations.Add(new KeyValuePair<string, string>(annotation.Name ?? string.Empty, annotation.Value ?? string.Empty));
    }
    table2.ExtendedProperties = ExtendedPropertyHelpers.ExtractFromTable(table1);
    return table2;
  }

  public static TableOperations.TableOperationResult CreateTable(
    string? connectionName,
    TableCreate def)
  {
    TableOperations.ValidateTableDefinition((TableBase) def, true);
    bool flag = !string.IsNullOrWhiteSpace(def.DaxExpression);
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Database database = info.Database;
    if (database.Model.Tables.Contains(def.Name))
      throw new McpException($"Table '{def.Name}' already exists");
    Microsoft.AnalysisServices.Tabular.Table table1 = new Microsoft.AnalysisServices.Tabular.Table();
    table1.Name = def.Name;
    Microsoft.AnalysisServices.Tabular.Table table2 = table1;
    TableOperations.ApplyTableProperties(table2, (TableBase) def);
    Microsoft.AnalysisServices.Tabular.Partition partition = TableOperations.CreatePartition(def, database);
    table2.Partitions.Add(partition);
    if (def.Annotations != null)
    {
      foreach (KeyValuePair<string, string> annotation in def.Annotations)
      {
        TableAnnotationCollection annotations = table2.Annotations;
        Microsoft.AnalysisServices.Tabular.Annotation metadataObject = new Microsoft.AnalysisServices.Tabular.Annotation();
        metadataObject.Name = annotation.Key;
        metadataObject.Value = annotation.Value;
        annotations.Add(metadataObject);
      }
    }
    if (def.ExtendedProperties != null)
      ExtendedPropertyHelpers.ApplyToTable(table2, def.ExtendedProperties);
    database.Model.Tables.Add(table2);
    if (!flag && def.Columns != null)
    {
      foreach (ColumnCreate column in def.Columns)
      {
        column.TableName = def.Name;
        ColumnOperations.CreateColumn(connectionName, column);
      }
    }
    TransactionOperations.RecordOperation(info, $"Created table '{def.Name}' in model {database.Model.Name}");
    ConnectionOperations.SaveChangesWithRollback(info, "create table");
    return TableOperations.CreateTableOperationResult(table2);
  }

  public static TableOperations.TableOperationResult UpdateTable(
    string? connectionName,
    TableUpdate update)
  {
    TableOperations.ValidateTableDefinition((TableBase) update, false);
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Database database = info.Database;
    Microsoft.AnalysisServices.Tabular.Table table = database.Model.Tables.Find(update.Name);
    bool flag = table != null ? TableOperations.ApplyTableUpdates(table, update) : throw new McpException($"Table '{update.Name}' not found");
    TableOperations.TableOperationResult tableOperationResult = TableOperations.CreateTableOperationResult(table);
    tableOperationResult.HasChanges = flag;
    if (!flag)
      return tableOperationResult;
    TransactionOperations.RecordOperation(info, $"Updated table '{update.Name}' in model {database.Model.Name}");
    ConnectionOperations.SaveChangesWithRollback(info, "update table");
    return tableOperationResult;
  }

  public static void RenameTable(string? connectionName, string oldName, string newName)
  {
    if (string.IsNullOrWhiteSpace(oldName))
      throw new McpException("oldName is required");
    if (string.IsNullOrWhiteSpace(newName))
      throw new McpException("newName is required");
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Database database = info.Database;
    Microsoft.AnalysisServices.Tabular.Table table = database.Model.Tables.Find(oldName);
    if (table == null)
      throw new McpException($"Table '{oldName}' not found");
    if (database.Model.Tables.Contains(newName) && !string.Equals(oldName, newName, StringComparison.OrdinalIgnoreCase))
      throw new McpException($"Table '{newName}' already exists");
    table.RequestRename(newName);
    TransactionOperations.RecordOperation(info, $"Renamed table '{oldName}' to '{newName}' in model {database.Model.Name}");
    ConnectionOperations.SaveChangesWithRollback(info, "rename table", ConnectionOperations.CheckpointMode.AfterRequestRename);
  }

  public static void DeleteTable(string? connectionName, string tableName, bool shouldCascadeDelete)
  {
    if (string.IsNullOrWhiteSpace(tableName))
      throw new McpException("tableName is required");
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Database database = info.Database;
    Microsoft.AnalysisServices.Tabular.Table table = database.Model.Tables.Find(tableName) ?? throw new McpException($"Table '{tableName}' not found");
    List<string> stringList = TableOperations.CheckTableDependencies(database, table);
    stringList.AddRange((IEnumerable<string>) StructuralDependencyHelper.CheckAndDeleteDependenciesIfRequired(database, (NamedMetadataObject) table, shouldCascadeDelete));
    if (!shouldCascadeDelete && Enumerable.Any<string>((IEnumerable<string>) stringList))
      throw new McpException($"Cannot delete table '{tableName}' because it has dependencies: {string.Join(", ", (IEnumerable<string>) stringList)}");
    database.Model.Tables.Remove(table);
    TransactionOperations.RecordOperation(info, $"Deleted table '{tableName}' from model {database.Model.Name}");
    ConnectionOperations.SaveChangesWithRollback(info, "delete table");
  }

  public static void RefreshTable(string? connectionName, string tableName, string? refreshType = "Automatic")
  {
    if (string.IsNullOrWhiteSpace(tableName))
      throw new McpException("tableName is required");
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Database database = info.Database;
    Microsoft.AnalysisServices.Tabular.Table table = database.Model.Tables.Find(tableName);
    if (table == null)
      throw new McpException($"Table '{tableName}' not found");
    RefreshType type;
    if (string.IsNullOrWhiteSpace(refreshType))
      type = RefreshType.Automatic;
    else if (!Enum.TryParse<RefreshType>(refreshType, true, out type))
    {
      string[] names = Enum.GetNames(typeof (RefreshType));
      throw new McpException($"Invalid refresh type '{refreshType}'. Valid values are: {string.Join(", ", names)}");
    }
    table.RequestRefresh(type);
    TransactionOperations.RecordOperation(info, $"Refreshed table '{tableName}' in model {database.Model.Name} with refresh type '{type}'");
    ConnectionOperations.SaveChangesWithRollback(info, "refresh table");
  }

  public static Dictionary<string, object> GetTableSchema(string? connectionName, string tableName)
  {
    if (string.IsNullOrWhiteSpace(tableName))
      throw new McpException("tableName is required");
    Microsoft.AnalysisServices.Tabular.Database database = ConnectionOperations.Get(connectionName).Database;
    Microsoft.AnalysisServices.Tabular.Table table = database.Model.Tables.Find(tableName) ?? throw new McpException($"Table '{tableName}' not found");
    return new Dictionary<string, object>()
    {
      ["TableName"] = (object) table.Name,
      ["Columns"] = (object) Enumerable.ToList(Enumerable.Select((IEnumerable<Microsoft.AnalysisServices.Tabular.Column>) table.Columns, c => new
      {
        Name = c.Name,
        DataType = c.DataType.ToString(),
        IsHidden = c.IsHidden,
        IsKey = c.IsKey,
        IsUnique = c.IsUnique,
        IsNullable = c.IsNullable,
        Description = c.Description,
        FormatString = c.FormatString,
        DataCategory = c.DataCategory,
        SummarizeBy = c.SummarizeBy.ToString(),
        DisplayFolder = c.DisplayFolder,
        SortByColumn = c.SortByColumn?.Name,
        Expression = c is Microsoft.AnalysisServices.Tabular.CalculatedColumn calculatedColumn ? calculatedColumn.Expression : (string) null,
        SourceColumn = c is Microsoft.AnalysisServices.Tabular.DataColumn dataColumn ? dataColumn.SourceColumn : (string) null
      })),
      ["Measures"] = (object) Enumerable.ToList(Enumerable.Select((IEnumerable<Microsoft.AnalysisServices.Tabular.Measure>) table.Measures, m => new
      {
        Name = m.Name,
        Expression = m.Expression,
        DataType = m.DataType.ToString(),
        FormatString = m.FormatString,
        Description = m.Description,
        IsHidden = m.IsHidden,
        DisplayFolder = m.DisplayFolder
      })),
      ["Hierarchies"] = (object) Enumerable.ToList(Enumerable.Select((IEnumerable<Microsoft.AnalysisServices.Tabular.Hierarchy>) table.Hierarchies, h => new
      {
        Name = h.Name,
        Description = h.Description,
        IsHidden = h.IsHidden,
        DisplayFolder = h.DisplayFolder,
        Levels = Enumerable.ToList(Enumerable.Select((IEnumerable<Microsoft.AnalysisServices.Tabular.Level>) h.Levels, l => new
        {
          Name = l.Name,
          Ordinal = l.Ordinal,
          Column = l.Column?.Name
        }))
      })),
      ["Relationships"] = (object) Enumerable.ToList(Enumerable.Select(Enumerable.Where<Microsoft.AnalysisServices.Tabular.SingleColumnRelationship>(Enumerable.OfType<Microsoft.AnalysisServices.Tabular.SingleColumnRelationship>((IEnumerable) database.Model.Relationships), (r => (r.FromTable.Name == table.Name) || (r.ToTable.Name == table.Name))), r => new
      {
        Name = r.Name,
        FromTable = r.FromTable.Name,
        FromColumn = r.FromColumn?.Name,
        ToTable = r.ToTable.Name,
        ToColumn = r.ToColumn?.Name,
        IsActive = r.IsActive,
        CrossFilteringBehavior = r.CrossFilteringBehavior.ToString(),
        JoinOnDateBehavior = r.JoinOnDateBehavior.ToString(),
        RelyOnReferentialIntegrity = r.RelyOnReferentialIntegrity
      }))
    };
  }

  public static TmdlExportResult ExportTMDL(
    string? connectionName,
    string tableName,
    TableExportTmdl? options = null)
  {
    if (string.IsNullOrWhiteSpace(tableName))
      throw new McpException("tableName is required");
    Microsoft.AnalysisServices.Tabular.Table @object = ConnectionOperations.Get(connectionName).Database.Model.Tables.Find(tableName);
    if (@object == null)
      throw new ArgumentException($"Table '{tableName}' not found");
    try
    {
      string str1;
      if (options?.SerializationOptions != null)
      {
        MetadataSerializationOptions serializationOptions = options.SerializationOptions.ToMetadataSerializationOptions();
        str1 = TmdlSerializer.SerializeObject((MetadataObject) @object, serializationOptions);
      }
      else
        str1 = TmdlSerializer.SerializeObject((MetadataObject) @object);
      if (options == null)
        return TmdlExportResult.CreateSuccess(tableName, "Table", str1, str1, false, (string) null, new List<string>());
      (string str2, bool flag, string str3, List<string> stringList) = ExportContentProcessor.ProcessExportContent(str1, (ExportOptionsBase) options);
      return TmdlExportResult.CreateSuccess(tableName, "Table", str1, str2, flag, str3, stringList, (ExportTmdl) options);
    }
    catch (Exception ex)
    {
      return TmdlExportResult.CreateFailure(tableName, "Table", ex.Message);
    }
  }

  public static TmslExportResult ExportTMSL(
    string? connectionName,
    string tableName,
    TableExportTmsl tmslOptions)
  {
    if (string.IsNullOrWhiteSpace(tableName))
      throw new McpException("tableName is required");
    if (tmslOptions == null)
      throw new McpException("tmslOptions is required");
    if (string.IsNullOrWhiteSpace(tmslOptions.TmslOperationType))
      throw new McpException("TmslOperationType is required in tmslOptions");
    try
    {
      Microsoft.AnalysisServices.Tabular.Table metadataObject = ConnectionOperations.Get(connectionName).Database.Model.Tables.Find(tableName);
      if (metadataObject == null)
        throw new ArgumentException($"Table '{tableName}' not found");
      TmslOperationType operationType;
      if (!Enum.TryParse<TmslOperationType>(tmslOptions.TmslOperationType, true, out operationType))
      {
        string[] names = Enum.GetNames(typeof (TmslOperationType));
        throw new McpException($"Invalid TmslOperationType '{tmslOptions.TmslOperationType}'. Valid values: {string.Join(", ", names)}");
      }
      TmslOperationRequest options = new TmslOperationRequest()
      {
        OperationType = operationType,
        IncludeRestricted = tmslOptions.IncludeRestricted.GetValueOrDefault()
      };
      if (!string.IsNullOrWhiteSpace(tmslOptions.RefreshType))
      {
        RefreshType refreshType;
        if (Enum.TryParse<RefreshType>(tmslOptions.RefreshType, true, out refreshType))
        {
          options.RefreshType = new RefreshType?(refreshType);
        }
        else
        {
          string[] names = Enum.GetNames(typeof (RefreshType));
          throw new McpException($"Invalid RefreshType '{tmslOptions.RefreshType}'. Valid values: {string.Join(", ", names)}");
        }
      }
      TmslExportResult tmslExportResult = TmslExportResult.FromLegacyResult(TmslScriptingService.GenerateScript<Microsoft.AnalysisServices.Tabular.Table>(metadataObject, operationType, options));
      (string Content, bool IsTruncated, string SavedFilePath, List<string> Warnings) = ExportContentProcessor.ProcessExportContent(tmslExportResult.Content, (ExportOptionsBase) tmslOptions);
      tmslExportResult.Content = Content;
      tmslExportResult.IsTruncated = IsTruncated;
      tmslExportResult.SavedFilePath = SavedFilePath;
      tmslExportResult.Warnings.AddRange((IEnumerable<string>) Warnings);
      tmslExportResult.AppliedOptions = (ExportTmsl) tmslOptions;
      return tmslExportResult;
    }
    catch (Exception ex)
    {
      return TmslExportResult.CreateFailure(tableName, "Table", ex.Message);
    }
  }

  private static void ApplyTableProperties(Microsoft.AnalysisServices.Tabular.Table table, TableBase def)
  {
    if (!string.IsNullOrWhiteSpace(def.DataCategory))
      table.DataCategory = def.DataCategory;
    if (!string.IsNullOrWhiteSpace(def.Description))
      table.Description = def.Description;
    bool? nullable = def.IsHidden;
    if (nullable.HasValue)
    {
      Microsoft.AnalysisServices.Tabular.Table table1 = table;
      nullable = def.IsHidden;
      int num = nullable.Value ? 1 : 0;
      table1.IsHidden = num != 0;
    }
    nullable = def.ShowAsVariationsOnly;
    if (nullable.HasValue)
    {
      Microsoft.AnalysisServices.Tabular.Table table2 = table;
      nullable = def.ShowAsVariationsOnly;
      int num = nullable.Value ? 1 : 0;
      table2.ShowAsVariationsOnly = num != 0;
    }
    nullable = def.IsPrivate;
    if (nullable.HasValue)
    {
      Microsoft.AnalysisServices.Tabular.Table table3 = table;
      nullable = def.IsPrivate;
      int num = nullable.Value ? 1 : 0;
      table3.IsPrivate = num != 0;
    }
    int? sourcePrecedence = def.AlternateSourcePrecedence;
    if (sourcePrecedence.HasValue)
    {
      Microsoft.AnalysisServices.Tabular.Table table4 = table;
      sourcePrecedence = def.AlternateSourcePrecedence;
      int num = sourcePrecedence.Value;
      table4.AlternateSourcePrecedence = num;
    }
    nullable = def.ExcludeFromModelRefresh;
    if (nullable.HasValue)
    {
      Microsoft.AnalysisServices.Tabular.Table table5 = table;
      nullable = def.ExcludeFromModelRefresh;
      int num = nullable.Value ? 1 : 0;
      table5.ExcludeFromModelRefresh = num != 0;
    }
    if (!string.IsNullOrWhiteSpace(def.LineageTag))
      table.LineageTag = def.LineageTag;
    if (!string.IsNullOrWhiteSpace(def.SourceLineageTag))
      table.SourceLineageTag = def.SourceLineageTag;
    nullable = def.SystemManaged;
    if (!nullable.HasValue)
      return;
    Microsoft.AnalysisServices.Tabular.Table table6 = table;
    nullable = def.SystemManaged;
    int num1 = nullable.Value ? 1 : 0;
    table6.SystemManaged = num1 != 0;
  }

  private static Microsoft.AnalysisServices.Tabular.Partition CreatePartition(
    TableCreate def,
    Microsoft.AnalysisServices.Tabular.Database db)
  {
    Microsoft.AnalysisServices.Tabular.Partition partition1;
    if (!string.IsNullOrWhiteSpace(def.DaxExpression))
    {
      Microsoft.AnalysisServices.Tabular.Partition partition2 = new Microsoft.AnalysisServices.Tabular.Partition();
      partition2.Name = def.PartitionName ?? def.Name;
      partition2.Source = (Microsoft.AnalysisServices.Tabular.PartitionSource) new CalculatedPartitionSource()
      {
        Expression = def.DaxExpression
      };
      partition1 = partition2;
    }
    else if (!string.IsNullOrWhiteSpace(def.MExpression))
    {
      Microsoft.AnalysisServices.Tabular.Partition partition3 = new Microsoft.AnalysisServices.Tabular.Partition();
      partition3.Name = def.PartitionName ?? def.Name;
      partition3.Source = (Microsoft.AnalysisServices.Tabular.PartitionSource) new MPartitionSource()
      {
        Expression = def.MExpression
      };
      partition1 = partition3;
    }
    else if (!string.IsNullOrWhiteSpace(def.SqlQuery))
    {
      WriteGuard.AssertPowerBICompatible("CREATE Table with SqlQuery", "PowerBI does not support tables with SQL queries that reference data sources");
      Microsoft.AnalysisServices.Tabular.DataSource dataSource = db.Model.DataSources.Find(def.DataSourceName) ?? throw new McpException($"Data source '{def.DataSourceName}' not found");
      Microsoft.AnalysisServices.Tabular.Partition partition4 = new Microsoft.AnalysisServices.Tabular.Partition();
      partition4.Name = def.PartitionName ?? def.Name;
      partition4.Source = (Microsoft.AnalysisServices.Tabular.PartitionSource) new QueryPartitionSource()
      {
        Query = def.SqlQuery,
        DataSource = dataSource
      };
      partition1 = partition4;
    }
    else
    {
      EntityPartitionSource entityPartitionSource = !string.IsNullOrWhiteSpace(def.EntityName) ? new EntityPartitionSource()
      {
        EntityName = def.EntityName
      } : throw new McpException("No valid expression provided");
      if (!string.IsNullOrWhiteSpace(def.SchemaName))
        entityPartitionSource.SchemaName = def.SchemaName;
      if (!string.IsNullOrWhiteSpace(def.ExpressionSourceName))
      {
        Microsoft.AnalysisServices.Tabular.NamedExpression namedExpression = db.Model.Expressions.Find(def.ExpressionSourceName) ?? throw new McpException($"Expression source '{def.ExpressionSourceName}' not found");
        entityPartitionSource.ExpressionSource = namedExpression;
      }
      else if (!string.IsNullOrWhiteSpace(def.DataSourceName))
      {
        WriteGuard.AssertPowerBICompatible("CREATE Table with EntityName", "PowerBI does not support tables with EntityPartitionSource referencing DataSourceName");
        Microsoft.AnalysisServices.Tabular.DataSource dataSource = db.Model.DataSources.Find(def.DataSourceName) ?? throw new McpException($"Data source '{def.DataSourceName}' not found");
        entityPartitionSource.DataSource = dataSource;
      }
      Microsoft.AnalysisServices.Tabular.Partition partition5 = new Microsoft.AnalysisServices.Tabular.Partition();
      partition5.Name = def.PartitionName ?? def.Name;
      partition5.Source = (Microsoft.AnalysisServices.Tabular.PartitionSource) entityPartitionSource;
      partition1 = partition5;
    }
    if (!string.IsNullOrWhiteSpace(def.Mode))
    {
      ModeType modeType;
      if (Enum.TryParse<ModeType>(def.Mode, true, out modeType))
      {
        partition1.Mode = modeType;
      }
      else
      {
        string[] names = Enum.GetNames(typeof (ModeType));
        throw new McpException($"Invalid mode '{def.Mode}'. Valid values are: {string.Join(", ", names)}");
      }
    }
    else
      partition1.Mode = ModeType.Import;
    return partition1;
  }

  private static bool ApplyTableUpdates(Microsoft.AnalysisServices.Tabular.Table table, TableUpdate update)
  {
    bool flag = false;
    if (update.DataCategory != null)
    {
      string dataCategory = string.IsNullOrEmpty(update.DataCategory) ? (string) null : update.DataCategory;
      if ((dataCategory != table.DataCategory))
      {
        table.DataCategory = dataCategory;
        flag = true;
      }
    }
    if (update.Description != null)
    {
      string description = string.IsNullOrEmpty(update.Description) ? (string) null : update.Description;
      if ((description != table.Description))
      {
        table.Description = description;
        flag = true;
      }
    }
    if (update.LineageTag != null)
    {
      string lineageTag = string.IsNullOrEmpty(update.LineageTag) ? (string) null : update.LineageTag;
      if ((lineageTag != table.LineageTag))
      {
        table.LineageTag = lineageTag;
        flag = true;
      }
    }
    if (update.SourceLineageTag != null)
    {
      string sourceLineageTag = string.IsNullOrEmpty(update.SourceLineageTag) ? (string) null : update.SourceLineageTag;
      if ((sourceLineageTag != table.SourceLineageTag))
      {
        table.SourceLineageTag = sourceLineageTag;
        flag = true;
      }
    }
    bool? nullable;
    if (update.IsHidden.HasValue)
    {
      int num1 = table.IsHidden ? 1 : 0;
      nullable = update.IsHidden;
      int num2 = nullable.Value ? 1 : 0;
      if (num1 != num2)
      {
        Microsoft.AnalysisServices.Tabular.Table table1 = table;
        nullable = update.IsHidden;
        int num3 = nullable.Value ? 1 : 0;
        table1.IsHidden = num3 != 0;
        flag = true;
      }
    }
    nullable = update.ShowAsVariationsOnly;
    if (nullable.HasValue)
    {
      int num4 = table.ShowAsVariationsOnly ? 1 : 0;
      nullable = update.ShowAsVariationsOnly;
      int num5 = nullable.Value ? 1 : 0;
      if (num4 != num5)
      {
        Microsoft.AnalysisServices.Tabular.Table table2 = table;
        nullable = update.ShowAsVariationsOnly;
        int num6 = nullable.Value ? 1 : 0;
        table2.ShowAsVariationsOnly = num6 != 0;
        flag = true;
      }
    }
    nullable = update.IsPrivate;
    if (nullable.HasValue)
    {
      int num7 = table.IsPrivate ? 1 : 0;
      nullable = update.IsPrivate;
      int num8 = nullable.Value ? 1 : 0;
      if (num7 != num8)
      {
        Microsoft.AnalysisServices.Tabular.Table table3 = table;
        nullable = update.IsPrivate;
        int num9 = nullable.Value ? 1 : 0;
        table3.IsPrivate = num9 != 0;
        flag = true;
      }
    }
    nullable = update.ExcludeFromModelRefresh;
    if (nullable.HasValue)
    {
      int num10 = table.ExcludeFromModelRefresh ? 1 : 0;
      nullable = update.ExcludeFromModelRefresh;
      int num11 = nullable.Value ? 1 : 0;
      if (num10 != num11)
      {
        Microsoft.AnalysisServices.Tabular.Table table4 = table;
        nullable = update.ExcludeFromModelRefresh;
        int num12 = nullable.Value ? 1 : 0;
        table4.ExcludeFromModelRefresh = num12 != 0;
        flag = true;
      }
    }
    nullable = update.SystemManaged;
    if (nullable.HasValue)
    {
      int num13 = table.SystemManaged ? 1 : 0;
      nullable = update.SystemManaged;
      int num14 = nullable.Value ? 1 : 0;
      if (num13 != num14)
      {
        Microsoft.AnalysisServices.Tabular.Table table5 = table;
        nullable = update.SystemManaged;
        int num15 = nullable.Value ? 1 : 0;
        table5.SystemManaged = num15 != 0;
        flag = true;
      }
    }
    if (update.AlternateSourcePrecedence.HasValue)
    {
      int sourcePrecedence1 = table.AlternateSourcePrecedence;
      int? sourcePrecedence2 = update.AlternateSourcePrecedence;
      int num16 = sourcePrecedence2.Value;
      if (sourcePrecedence1 != num16)
      {
        Microsoft.AnalysisServices.Tabular.Table table6 = table;
        sourcePrecedence2 = update.AlternateSourcePrecedence;
        int num17 = sourcePrecedence2.Value;
        table6.AlternateSourcePrecedence = num17;
        flag = true;
      }
    }
    if (update.Annotations != null && AnnotationHelpers.ReplaceAnnotations<Microsoft.AnalysisServices.Tabular.Table>(table, update.Annotations, (Func<Microsoft.AnalysisServices.Tabular.Table, ICollection<Microsoft.AnalysisServices.Tabular.Annotation>>) (obj => (ICollection<Microsoft.AnalysisServices.Tabular.Annotation>) obj.Annotations)))
      flag = true;
    if (update.ExtendedProperties != null)
    {
      int num = table.ExtendedProperties.Count > 0 ? 1 : 0;
      ExtendedPropertyHelpers.ReplaceExtendedProperties<Microsoft.AnalysisServices.Tabular.Table>(table, update.ExtendedProperties, (Func<Microsoft.AnalysisServices.Tabular.Table, ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>>) (obj => (ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>) obj.ExtendedProperties));
      if (num != 0 || update.ExtendedProperties.Count > 0)
        flag = true;
    }
    return flag;
  }

  private static List<string> CheckTableDependencies(Microsoft.AnalysisServices.Tabular.Database db, Microsoft.AnalysisServices.Tabular.Table table)
  {
    List<string> stringList = new List<string>();
    foreach (Microsoft.AnalysisServices.Tabular.Table table1 in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.Table, Microsoft.AnalysisServices.Tabular.Model>) db.Model.Tables)
    {
      if (table1 != table)
      {
        foreach (Microsoft.AnalysisServices.Tabular.Measure measure in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.Measure, Microsoft.AnalysisServices.Tabular.Table>) table1.Measures)
        {
          if (!string.IsNullOrWhiteSpace(measure.Expression) && measure.Expression.Contains($"'{table.Name}'") || measure.Expression.Contains($"[{table.Name}]"))
            stringList.Add($"Measure: {table1.Name}[{measure.Name}]");
        }
      }
    }
    return stringList;
  }

  private static TableOperations.TableOperationResult CreateTableOperationResult(Microsoft.AnalysisServices.Tabular.Table table)
  {
    TableOperations.TableOperationResult tableOperationResult = new TableOperations.TableOperationResult()
    {
      TableName = table.Name ?? string.Empty
    };
    foreach (Microsoft.AnalysisServices.Tabular.Partition partition in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.Partition, Microsoft.AnalysisServices.Tabular.Table>) table.Partitions)
      tableOperationResult.Partitions.Add(new PartitionOperationResult()
      {
        State = partition.State.ToString(),
        ErrorMessage = partition.ErrorMessage,
        PartitionName = partition.Name,
        TableName = table.Name ?? string.Empty
      });
    return tableOperationResult;
  }

  public class TableOperationResult
  {
    public string TableName { get; set; } = string.Empty;

    public List<PartitionOperationResult> Partitions { get; set; } = new List<PartitionOperationResult>();

    public bool HasChanges { get; set; }
  }
}
