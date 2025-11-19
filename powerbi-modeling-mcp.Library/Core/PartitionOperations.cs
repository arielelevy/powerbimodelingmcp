// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using Microsoft.AnalysisServices.Tabular;
using Microsoft.AnalysisServices.Tabular.Serialization;
using ModelContextProtocol;
using PowerBIModelingMCP.Library.Common;
using PowerBIModelingMCP.Library.Common.DataStructures;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

#nullable enable
namespace PowerBIModelingMCP.Library.Core;

public static class PartitionOperations
{
  public static List<PartitionGet> ListPartitions(string? connectionName, string? tableName = null)
  {
    Microsoft.AnalysisServices.Tabular.Database database = ConnectionOperations.Get(connectionName).Database;
    List<PartitionGet> partitionGetList = new List<PartitionGet>();
    Microsoft.AnalysisServices.Tabular.Table[] tableArray1;
    if (!string.IsNullOrWhiteSpace(tableName))
    {
      Microsoft.AnalysisServices.Tabular.Table[] tableArray2 = new Microsoft.AnalysisServices.Tabular.Table[1];
      tableArray2[0] = database.Model.Tables.Find(tableName) ?? throw new McpException($"Table '{tableName}' not found");
      tableArray1 = tableArray2;
    }
    else
      tableArray1 = Enumerable.ToArray<Microsoft.AnalysisServices.Tabular.Table>((IEnumerable<Microsoft.AnalysisServices.Tabular.Table>) database.Model.Tables);
    foreach (Microsoft.AnalysisServices.Tabular.Table table in tableArray1)
    {
      foreach (Microsoft.AnalysisServices.Tabular.Partition partition in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.Partition, Microsoft.AnalysisServices.Tabular.Table>) table.Partitions)
      {
        PartitionGet partitionGet1 = new PartitionGet { Name = partition.Name };
        partitionGet1.TableName = table.Name;
        partitionGet1.Description = partition.Description;
        partitionGet1.ModifiedTime = new DateTime?(partition.ModifiedTime);
        partitionGet1.State = partition.State.ToString();
        partitionGet1.DataView = partition.DataView.ToString();
        partitionGet1.Mode = partition.Mode.ToString();
        partitionGet1.ErrorMessage = partition.ErrorMessage;
        partitionGet1.SourceType = partition.SourceType.ToString();
        partitionGet1.QueryGroupName = partition.QueryGroup?.Name;
        PartitionGet partitionGet2 = partitionGet1;
        PartitionOperations.ExtractSourceInformation(partition, partitionGet2);
        foreach (Microsoft.AnalysisServices.Tabular.Annotation annotation in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.Annotation, Microsoft.AnalysisServices.Tabular.Partition>) partition.Annotations)
          partitionGet2.Annotations.Add(new KeyValuePair<string, string>(annotation.Name, annotation.Value));
        partitionGet2.ExtendedProperties = ExtendedPropertyHelpers.ExtractFromPartition(partition);
        partitionGetList.Add(partitionGet2);
      }
    }
    return partitionGetList;
  }

  public static PartitionGet GetPartition(
    string? connectionName,
    string tableName,
    string partitionName)
  {
    if (string.IsNullOrWhiteSpace(tableName))
      throw new McpException("tableName is required");
    if (string.IsNullOrWhiteSpace(partitionName))
      throw new McpException("partitionName is required");
    Microsoft.AnalysisServices.Tabular.Table table = ConnectionOperations.Get(connectionName).Database.Model.Tables.Find(tableName) ?? throw new McpException($"Table '{tableName}' not found");
    Microsoft.AnalysisServices.Tabular.Partition partition = table.Partitions.Find(partitionName) ?? throw new McpException($"Partition '{partitionName}' not found in table '{tableName}'");
    PartitionGet partitionGet1 = new PartitionGet { Name = partition.Name };
    partitionGet1.TableName = table.Name;
    partitionGet1.Description = partition.Description;
    partitionGet1.ModifiedTime = new DateTime?(partition.ModifiedTime);
    partitionGet1.State = partition.State.ToString();
    partitionGet1.DataView = partition.DataView.ToString();
    partitionGet1.Mode = partition.Mode.ToString();
    partitionGet1.ErrorMessage = partition.ErrorMessage;
    partitionGet1.SourceType = partition.SourceType.ToString();
    partitionGet1.QueryGroupName = partition.QueryGroup?.Name;
    PartitionGet partitionGet2 = partitionGet1;
    PartitionOperations.ExtractSourceInformation(partition, partitionGet2);
    foreach (Microsoft.AnalysisServices.Tabular.Annotation annotation in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.Annotation, Microsoft.AnalysisServices.Tabular.Partition>) partition.Annotations)
      partitionGet2.Annotations.Add(new KeyValuePair<string, string>(annotation.Name, annotation.Value));
    partitionGet2.ExtendedProperties = ExtendedPropertyHelpers.ExtractFromPartition(partition);
    return partitionGet2;
  }

  public static PartitionOperationResult CreatePartition(string? connectionName, PartitionCreate def)
  {
    if (def == null)
      throw new McpException("Partition definition cannot be null");
    if (string.IsNullOrWhiteSpace(def.Name))
      throw new McpException("Partition name is required");
    if (string.IsNullOrWhiteSpace(def.TableName))
      throw new McpException("Table name is required");
    if (string.IsNullOrWhiteSpace(def.SourceType))
      throw new McpException("SourceType is required");
    if (def.ExtendedProperties != null)
    {
      List<string> stringList = ExtendedPropertyHelpers.Validate(def.ExtendedProperties);
      if (stringList.Count > 0)
        throw new McpException("ExtendedProperties validation failed: " + string.Join(", ", (IEnumerable<string>) stringList));
    }
    AnnotationHelpers.ValidateAnnotations(def.Annotations);
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Database database = info.Database;
    Microsoft.AnalysisServices.Tabular.Table table = database.Model.Tables.Find(def.TableName) ?? throw new McpException($"Table '{def.TableName}' not found");
    if (table.Partitions.Contains(def.Name))
      throw new McpException($"Partition '{def.Name}' already exists in table '{def.TableName}'");
    Microsoft.AnalysisServices.Tabular.PartitionSource partitionSource = PartitionOperations.CreatePartitionSource(def, database);
    Microsoft.AnalysisServices.Tabular.Partition partition1 = new Microsoft.AnalysisServices.Tabular.Partition();
    partition1.Name = def.Name;
    partition1.Source = partitionSource;
    Microsoft.AnalysisServices.Tabular.Partition partition2 = partition1;
    if (!string.IsNullOrWhiteSpace(def.Description))
      partition2.Description = def.Description;
    if (!string.IsNullOrWhiteSpace(def.Mode))
    {
      ModeType modeType;
      if (Enum.TryParse<ModeType>(def.Mode, true, out modeType))
      {
        partition2.Mode = modeType;
      }
      else
      {
        string[] names = Enum.GetNames(typeof (ModeType));
        throw new McpException($"Invalid mode '{def.Mode}'. Valid values are: {string.Join(", ", names)}");
      }
    }
    else
      partition2.Mode = ModeType.Import;
    if (def.Annotations != null)
    {
      foreach (KeyValuePair<string, string> annotation in def.Annotations)
      {
        PartitionAnnotationCollection annotations = partition2.Annotations;
        Microsoft.AnalysisServices.Tabular.Annotation metadataObject = new Microsoft.AnalysisServices.Tabular.Annotation();
        metadataObject.Name = annotation.Key;
        metadataObject.Value = annotation.Value;
        annotations.Add(metadataObject);
      }
    }
    if (def.ExtendedProperties != null)
      ExtendedPropertyHelpers.ApplyToPartition(partition2, def.ExtendedProperties);
    List<string> stringList1 = (List<string>) null;
    if (!string.IsNullOrWhiteSpace(def.QueryGroupName))
    {
      bool wasCreated;
      Microsoft.AnalysisServices.Tabular.QueryGroup createQueryGroup = QueryGroupOperations.FindOrCreateQueryGroup(database, def.QueryGroupName, out wasCreated);
      if (wasCreated)
      {
        List<string> stringList2 = new List<string>();
        stringList2.Add($"Query group '{def.QueryGroupName}' was automatically created");
        stringList1 = stringList2;
      }
      partition2.QueryGroup = createQueryGroup;
    }
    table.Partitions.Add(partition2);
    TransactionOperations.RecordOperation(info, $"Created partition '{def.Name}' in table '{def.TableName}' in model {database.Model.Name}");
    ConnectionOperations.SaveChangesWithRollback(info, "create partition");
    return new PartitionOperationResult()
    {
      State = partition2.State.ToString(),
      ErrorMessage = partition2.ErrorMessage,
      PartitionName = partition2.Name,
      TableName = table.Name,
      Warnings = stringList1
    };
  }

  public static void DeletePartition(string? connectionName, string tableName, string partitionName)
  {
    if (string.IsNullOrWhiteSpace(tableName))
      throw new McpException("tableName is required");
    if (string.IsNullOrWhiteSpace(partitionName))
      throw new McpException("partitionName is required");
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Database database = info.Database;
    Microsoft.AnalysisServices.Tabular.Table table = database.Model.Tables.Find(tableName);
    if (table == null)
      throw new McpException($"Table '{tableName}' not found");
    if (table.Partitions.Count <= 1)
      throw new McpException("Cannot delete the last partition in a table");
    table.Partitions.Remove(table.Partitions.Find(partitionName) ?? throw new McpException($"Partition '{partitionName}' not found in table '{tableName}'"));
    TransactionOperations.RecordOperation(info, $"Deleted partition '{partitionName}' from table '{tableName}' in model {database.Model.Name}");
    ConnectionOperations.SaveChangesWithRollback(info, "delete partition");
  }

  public static void RefreshPartition(
    string? connectionName,
    string tableName,
    string partitionName,
    string? refreshType = "Automatic")
  {
    if (string.IsNullOrWhiteSpace(tableName))
      throw new McpException("tableName is required");
    if (string.IsNullOrWhiteSpace(partitionName))
      throw new McpException("partitionName is required");
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Database database = info.Database;
    Microsoft.AnalysisServices.Tabular.Partition partition = (database.Model.Tables.Find(tableName) ?? throw new McpException($"Table '{tableName}' not found")).Partitions.Find(partitionName);
    if (partition == null)
      throw new McpException($"Partition '{partitionName}' not found in table '{tableName}'");
    RefreshType type;
    if (!Enum.TryParse<RefreshType>(refreshType, true, out type))
    {
      string[] names = Enum.GetNames(typeof (RefreshType));
      throw new McpException($"Invalid refresh type '{refreshType}'. Valid values are: {string.Join(", ", names)}");
    }
    partition.RequestRefresh(type);
    TransactionOperations.RecordOperation(info, $"Refreshed partition '{partitionName}' in table '{tableName}' in model {database.Model.Name} with refresh type '{type}'");
    ConnectionOperations.SaveChangesWithRollback(info, "refresh partition");
  }

  public static void RenamePartition(
    string? connectionName,
    string tableName,
    string partitionName,
    string newName)
  {
    if (string.IsNullOrWhiteSpace(tableName))
      throw new McpException("tableName is required");
    if (string.IsNullOrWhiteSpace(partitionName))
      throw new McpException("partitionName is required");
    if (string.IsNullOrWhiteSpace(newName))
      throw new McpException("newName is required");
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Database database = info.Database;
    Microsoft.AnalysisServices.Tabular.Table table = database.Model.Tables.Find(tableName);
    if (table == null)
      throw new McpException($"Table '{tableName}' not found");
    Microsoft.AnalysisServices.Tabular.Partition partition = table.Partitions.Find(partitionName) ?? throw new McpException($"Partition '{partitionName}' not found in table '{tableName}'");
    if (table.Partitions.Contains(newName) && !string.Equals(partitionName, newName, StringComparison.OrdinalIgnoreCase))
      throw new McpException($"Partition '{newName}' already exists in table '{tableName}'");
    partition.RequestRename(newName);
    TransactionOperations.RecordOperation(info, $"Renamed partition '{partitionName}' to '{newName}' in table '{tableName}' in model {database.Model.Name}");
    ConnectionOperations.SaveChangesWithRollback(info, "rename partition", ConnectionOperations.CheckpointMode.AfterRequestRename);
  }

  public static PartitionOperationResult UpdatePartition(
    string? connectionName,
    PartitionUpdate update)
  {
    if (update == null)
      throw new McpException("Partition update definition cannot be null");
    if (string.IsNullOrWhiteSpace(update.Name))
      throw new McpException("Name is required to identify the partition to update");
    if (string.IsNullOrWhiteSpace(update.TableName))
      throw new McpException("TableName is required to identify the table containing the partition");
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Database database = info.Database;
    Microsoft.AnalysisServices.Tabular.Table table = database.Model.Tables.Find(update.TableName) ?? throw new McpException($"Table '{update.TableName}' not found");
    Microsoft.AnalysisServices.Tabular.Partition partition = table.Partitions.Find(update.Name) ?? throw new McpException($"Partition '{update.Name}' not found in table '{update.TableName}'");
    bool flag = false;
    if (update.Description != null)
    {
      string description = string.IsNullOrEmpty(update.Description) ? (string) null : update.Description;
      if ((partition.Description != description))
      {
        partition.Description = description;
        flag = true;
      }
    }
    if (!string.IsNullOrWhiteSpace(update.Mode))
    {
      ModeType modeType;
      if (Enum.TryParse<ModeType>(update.Mode, true, out modeType))
      {
        if (partition.Mode != modeType)
        {
          partition.Mode = modeType;
          flag = true;
        }
      }
      else
      {
        string[] names = Enum.GetNames(typeof (ModeType));
        throw new McpException($"Invalid mode '{update.Mode}'. Valid values are: {string.Join(", ", names)}");
      }
    }
    if (PartitionOperations.UpdatePartitionSource(partition, update, database))
      flag = true;
    if (update.Annotations != null && AnnotationHelpers.ReplaceAnnotations<Microsoft.AnalysisServices.Tabular.Partition>(partition, update.Annotations, (Func<Microsoft.AnalysisServices.Tabular.Partition, ICollection<Microsoft.AnalysisServices.Tabular.Annotation>>) (obj => (ICollection<Microsoft.AnalysisServices.Tabular.Annotation>) obj.Annotations)))
      flag = true;
    if (update.ExtendedProperties != null)
    {
      int num = partition.ExtendedProperties.Count > 0 ? 1 : 0;
      ExtendedPropertyHelpers.ReplacePartitionProperties(partition, update.ExtendedProperties);
      if (num != 0 || update.ExtendedProperties.Count > 0)
        flag = true;
    }
    List<string> stringList1 = (List<string>) null;
    if (update.QueryGroupName != null)
    {
      Microsoft.AnalysisServices.Tabular.QueryGroup queryGroup = (Microsoft.AnalysisServices.Tabular.QueryGroup) null;
      if (!string.IsNullOrEmpty(update.QueryGroupName))
      {
        bool wasCreated;
        queryGroup = QueryGroupOperations.FindOrCreateQueryGroup(database, update.QueryGroupName, out wasCreated);
        if (wasCreated)
        {
          List<string> stringList2 = new List<string>();
          stringList2.Add($"Query group '{update.QueryGroupName}' was automatically created");
          stringList1 = stringList2;
        }
      }
      if (partition.QueryGroup != queryGroup)
      {
        partition.QueryGroup = queryGroup;
        flag = true;
      }
    }
    if (!flag)
      return new PartitionOperationResult()
      {
        State = partition.State.ToString(),
        ErrorMessage = partition.ErrorMessage,
        PartitionName = partition.Name,
        TableName = table.Name,
        HasChanges = false,
        Warnings = stringList1
      };
    TransactionOperations.RecordOperation(info, $"Updated partition '{update.Name}' in table '{update.TableName}' in model {database.Model.Name}");
    ConnectionOperations.SaveChangesWithRollback(info, "update partition");
    return new PartitionOperationResult()
    {
      State = partition.State.ToString(),
      ErrorMessage = partition.ErrorMessage,
      PartitionName = partition.Name,
      TableName = table.Name,
      HasChanges = true,
      Warnings = stringList1
    };
  }

  private static void ExtractSourceInformation(Microsoft.AnalysisServices.Tabular.Partition partition, PartitionGet partitionGet)
  {
    Microsoft.AnalysisServices.Tabular.PartitionSource source = partition.Source;
    switch (source)
    {
      case CalculatedPartitionSource calculatedPartitionSource:
        partitionGet.Expression = calculatedPartitionSource.Expression;
        partitionGet.RetainDataTillForceCalculate = new bool?(calculatedPartitionSource.RetainDataTillForceCalculate);
        break;
      case MPartitionSource mpartitionSource:
        partitionGet.Expression = mpartitionSource.Expression;
        partitionGet.Attributes = mpartitionSource.Attributes;
        break;
      case QueryPartitionSource queryPartitionSource:
        partitionGet.Query = queryPartitionSource.Query;
        partitionGet.DataSourceName = queryPartitionSource.DataSource?.Name;
        break;
      case PolicyRangePartitionSource rangePartitionSource:
        partitionGet.StartDateTime = rangePartitionSource.Start.ToString("yyyy-MM-dd HH:mm:ss");
        partitionGet.EndDateTime = rangePartitionSource.End.ToString("yyyy-MM-dd HH:mm:ss");
        partitionGet.Granularity = rangePartitionSource.Granularity.ToString();
        partitionGet.RefreshBookmark = rangePartitionSource.RefreshBookmark;
        break;
      case EntityPartitionSource entityPartitionSource:
        partitionGet.DataSourceName = entityPartitionSource.DataSource?.Name;
        partitionGet.EntityName = entityPartitionSource.EntityName;
        partitionGet.SchemaName = entityPartitionSource.SchemaName;
        partitionGet.ExpressionSourceName = entityPartitionSource.ExpressionSource?.Name;
        break;
      case CalculationGroupSource _:
        break;
      default:
        InferredPartitionSource inferredPartitionSource = source as InferredPartitionSource;
        break;
    }
  }

  private static Microsoft.AnalysisServices.Tabular.PartitionSource CreatePartitionSource(
    PartitionCreate def,
    Microsoft.AnalysisServices.Tabular.Database db)
  {
    string str = def.SourceType?.ToLower() ?? string.Empty;
    if (!(str == "calculated"))
    {
      if (!(str == "m"))
      {
        if (!(str == "query"))
        {
          if (!(str == "policyrange"))
          {
            if (!(str == "entity"))
              throw new McpException($"Unsupported SourceType '{def.SourceType}'. Valid values are: Calculated, M, Query, PolicyRange, Entity");
            if (string.IsNullOrWhiteSpace(def.EntityName))
              throw new McpException("EntityName is required for EntityPartitionSource");
            if (string.IsNullOrWhiteSpace(def.DataSourceName) && string.IsNullOrWhiteSpace(def.ExpressionSourceName))
              throw new McpException("Either ExpressionSourceName or DataSourceName is required for EntityPartitionSource");
            if (!string.IsNullOrWhiteSpace(def.DataSourceName) && !string.IsNullOrWhiteSpace(def.ExpressionSourceName))
              throw new McpException("Only one of ExpressionSourceName or DataSourceName can be provided for EntityPartitionSource");
            EntityPartitionSource partitionSource = new EntityPartitionSource()
            {
              EntityName = def.EntityName
            };
            if (!string.IsNullOrWhiteSpace(def.SchemaName))
              partitionSource.SchemaName = def.SchemaName;
            if (!string.IsNullOrWhiteSpace(def.ExpressionSourceName))
            {
              Microsoft.AnalysisServices.Tabular.NamedExpression namedExpression = db.Model.Expressions.Find(def.ExpressionSourceName) ?? throw new McpException($"Expression source '{def.ExpressionSourceName}' not found");
              partitionSource.ExpressionSource = namedExpression;
            }
            else if (!string.IsNullOrWhiteSpace(def.DataSourceName))
            {
              WriteGuard.AssertPowerBICompatible("CREATE Partition with EntityPartitionSource", "PowerBI does not support partitions with EntityPartitionSource referencing DataSourceName");
              Microsoft.AnalysisServices.Tabular.DataSource dataSource = db.Model.DataSources.Find(def.DataSourceName) ?? throw new McpException($"Data source '{def.DataSourceName}' not found");
              partitionSource.DataSource = dataSource;
            }
            return (Microsoft.AnalysisServices.Tabular.PartitionSource) partitionSource;
          }
          if (string.IsNullOrWhiteSpace(def.StartDateTime) || string.IsNullOrWhiteSpace(def.EndDateTime))
            throw new McpException("StartDateTime and EndDateTime are required for PolicyRangePartitionSource");
          DateTime dateTime1;
          if (!DateTime.TryParseExact(def.StartDateTime, "yyyy-MM-dd HH:mm:ss", (IFormatProvider) null, (DateTimeStyles) 0, out dateTime1))
            throw new McpException("Invalid StartDateTime format. Expected 'yyyy-MM-dd HH:mm:ss'");
          DateTime dateTime2;
          if (!DateTime.TryParseExact(def.EndDateTime, "yyyy-MM-dd HH:mm:ss", (IFormatProvider) null, (DateTimeStyles) 0, out dateTime2))
            throw new McpException("Invalid EndDateTime format. Expected 'yyyy-MM-dd HH:mm:ss'");
          PolicyRangePartitionSource partitionSource1 = new PolicyRangePartitionSource()
          {
            Start = dateTime1,
            End = dateTime2
          };
          if (!string.IsNullOrWhiteSpace(def.Granularity))
          {
            RefreshGranularityType refreshGranularityType;
            if (Enum.TryParse<RefreshGranularityType>(def.Granularity, true, out refreshGranularityType))
            {
              partitionSource1.Granularity = refreshGranularityType;
            }
            else
            {
              string[] names = Enum.GetNames(typeof (RefreshGranularityType));
              throw new McpException($"Invalid granularity '{def.Granularity}'. Valid values are: {string.Join(", ", names)}");
            }
          }
          return (Microsoft.AnalysisServices.Tabular.PartitionSource) partitionSource1;
        }
        if (string.IsNullOrWhiteSpace(def.Query))
          throw new McpException("Query is required for QueryPartitionSource");
        if (string.IsNullOrWhiteSpace(def.DataSourceName))
          throw new McpException("DataSourceName is required for QueryPartitionSource");
        WriteGuard.AssertPowerBICompatible("CREATE Partition with QueryPartitionSource", "PowerBI does not support partitions with QueryPartitionSource that references a data source");
        Microsoft.AnalysisServices.Tabular.DataSource dataSource1 = db.Model.DataSources.Find(def.DataSourceName) ?? throw new McpException($"Data source '{def.DataSourceName}' not found");
        return (Microsoft.AnalysisServices.Tabular.PartitionSource) new QueryPartitionSource()
        {
          Query = def.Query,
          DataSource = dataSource1
        };
      }
      MPartitionSource partitionSource2 = !string.IsNullOrWhiteSpace(def.Expression) ? new MPartitionSource()
      {
        Expression = def.Expression
      } : throw new McpException("Expression is required for MPartitionSource");
      if (!string.IsNullOrWhiteSpace(def.Attributes))
        partitionSource2.Attributes = def.Attributes;
      return (Microsoft.AnalysisServices.Tabular.PartitionSource) partitionSource2;
    }
    CalculatedPartitionSource partitionSource3 = !string.IsNullOrWhiteSpace(def.Expression) ? new CalculatedPartitionSource()
    {
      Expression = def.Expression
    } : throw new McpException("Expression is required for CalculatedPartitionSource");
    if (def.RetainDataTillForceCalculate.HasValue)
      partitionSource3.RetainDataTillForceCalculate = def.RetainDataTillForceCalculate.Value;
    return (Microsoft.AnalysisServices.Tabular.PartitionSource) partitionSource3;
  }

  private static bool UpdatePartitionSource(
    Microsoft.AnalysisServices.Tabular.Partition partition,
    PartitionUpdate update,
    Microsoft.AnalysisServices.Tabular.Database db)
  {
    int num1 = Enumerable.Count<bool>((IEnumerable<bool>) new bool[5]
    {
      !string.IsNullOrWhiteSpace(update.Expression) && (update.SourceType?.ToLower() == "calculated"),
      !string.IsNullOrWhiteSpace(update.Expression) && (update.SourceType?.ToLower() == "m"),
      !string.IsNullOrWhiteSpace(update.Query) && !string.IsNullOrWhiteSpace(update.DataSourceName),
      !string.IsNullOrWhiteSpace(update.StartDateTime) && !string.IsNullOrWhiteSpace(update.EndDateTime),
      !string.IsNullOrWhiteSpace(update.EntityName) && !string.IsNullOrWhiteSpace(update.DataSourceName)
    }, (x => x));
    if (num1 > 1)
      throw new McpException("Only one complete source replacement can be provided at a time");
    if (!string.IsNullOrWhiteSpace(update.Query) && string.IsNullOrWhiteSpace(update.DataSourceName))
      throw new McpException("DataSourceName is required when Query is provided");
    bool flag = false;
    if (num1 == 1)
    {
      Microsoft.AnalysisServices.Tabular.PartitionSource newSource = (Microsoft.AnalysisServices.Tabular.PartitionSource) null;
      if (!string.IsNullOrWhiteSpace(update.Expression) && (update.SourceType?.ToLower() == "calculated"))
      {
        CalculatedPartitionSource calculatedPartitionSource1 = new CalculatedPartitionSource()
        {
          Expression = update.Expression
        };
        bool? tillForceCalculate = update.RetainDataTillForceCalculate;
        if (tillForceCalculate.HasValue)
        {
          CalculatedPartitionSource calculatedPartitionSource2 = calculatedPartitionSource1;
          tillForceCalculate = update.RetainDataTillForceCalculate;
          int num2 = tillForceCalculate.Value ? 1 : 0;
          calculatedPartitionSource2.RetainDataTillForceCalculate = num2 != 0;
        }
        newSource = (Microsoft.AnalysisServices.Tabular.PartitionSource) calculatedPartitionSource1;
      }
      else if (!string.IsNullOrWhiteSpace(update.Expression) && (update.SourceType?.ToLower() == "m"))
      {
        MPartitionSource mpartitionSource = new MPartitionSource()
        {
          Expression = update.Expression
        };
        if (!string.IsNullOrWhiteSpace(update.Attributes))
          mpartitionSource.Attributes = update.Attributes;
        newSource = (Microsoft.AnalysisServices.Tabular.PartitionSource) mpartitionSource;
      }
      else if (!string.IsNullOrWhiteSpace(update.Query))
      {
        WriteGuard.AssertPowerBICompatible("UPDATE Partition to QueryPartitionSource", "PowerBI does not support partitions with QueryPartitionSource that references a data source");
        Microsoft.AnalysisServices.Tabular.DataSource dataSource = db.Model.DataSources.Find(update.DataSourceName) ?? throw new McpException($"Data source '{update.DataSourceName}' not found");
        newSource = (Microsoft.AnalysisServices.Tabular.PartitionSource) new QueryPartitionSource()
        {
          Query = update.Query,
          DataSource = dataSource
        };
      }
      else if (!string.IsNullOrWhiteSpace(update.StartDateTime) && !string.IsNullOrWhiteSpace(update.EndDateTime))
      {
        DateTime dateTime1;
        if (!DateTime.TryParseExact(update.StartDateTime, "yyyy-MM-dd HH:mm:ss", (IFormatProvider) null, (DateTimeStyles) 0, out dateTime1))
          throw new McpException("Invalid StartDateTime format. Expected 'yyyy-MM-dd HH:mm:ss'");
        DateTime dateTime2;
        if (!DateTime.TryParseExact(update.EndDateTime, "yyyy-MM-dd HH:mm:ss", (IFormatProvider) null, (DateTimeStyles) 0, out dateTime2))
          throw new McpException("Invalid EndDateTime format. Expected 'yyyy-MM-dd HH:mm:ss'");
        PolicyRangePartitionSource rangePartitionSource = new PolicyRangePartitionSource()
        {
          Start = dateTime1,
          End = dateTime2
        };
        if (!string.IsNullOrWhiteSpace(update.Granularity))
        {
          RefreshGranularityType refreshGranularityType;
          if (!Enum.TryParse<RefreshGranularityType>(update.Granularity, true, out refreshGranularityType))
            throw new McpException($"Invalid granularity '{update.Granularity}'");
          rangePartitionSource.Granularity = refreshGranularityType;
        }
        newSource = (Microsoft.AnalysisServices.Tabular.PartitionSource) rangePartitionSource;
      }
      else if (!string.IsNullOrWhiteSpace(update.EntityName))
      {
        if (string.IsNullOrWhiteSpace(update.DataSourceName) && string.IsNullOrWhiteSpace(update.ExpressionSourceName))
          throw new McpException("Either ExpressionSourceName or DataSourceName is required when EntityName is provided");
        if (!string.IsNullOrWhiteSpace(update.DataSourceName) && !string.IsNullOrWhiteSpace(update.ExpressionSourceName))
          throw new McpException("Only one of ExpressionSourceName or DataSourceName can be provided when EntityName is specified");
        EntityPartitionSource entityPartitionSource = new EntityPartitionSource()
        {
          EntityName = update.EntityName
        };
        if (!string.IsNullOrWhiteSpace(update.SchemaName))
          entityPartitionSource.SchemaName = update.SchemaName;
        if (!string.IsNullOrWhiteSpace(update.ExpressionSourceName))
        {
          Microsoft.AnalysisServices.Tabular.NamedExpression namedExpression = db.Model.Expressions.Find(update.ExpressionSourceName) ?? throw new McpException($"Expression source '{update.ExpressionSourceName}' not found");
          entityPartitionSource.ExpressionSource = namedExpression;
        }
        else if (!string.IsNullOrWhiteSpace(update.DataSourceName))
        {
          WriteGuard.AssertPowerBICompatible("UPDATE Partition to EntityPartitionSource", "PowerBI does not support partitions with EntityPartitionSource referencing DataSourceName");
          Microsoft.AnalysisServices.Tabular.DataSource dataSource = db.Model.DataSources.Find(update.DataSourceName) ?? throw new McpException($"Data source '{update.DataSourceName}' not found");
          entityPartitionSource.DataSource = dataSource;
        }
        newSource = (Microsoft.AnalysisServices.Tabular.PartitionSource) entityPartitionSource;
      }
      if (PartitionOperations.ComparePartitionSources(partition.Source, newSource))
      {
        partition.Source = newSource;
        flag = true;
      }
    }
    else
      flag = PartitionOperations.UpdateExistingPartitionSource(partition, update, db);
    return flag;
  }

  private static bool ComparePartitionSources(
    Microsoft.AnalysisServices.Tabular.PartitionSource currentSource,
    Microsoft.AnalysisServices.Tabular.PartitionSource? newSource)
  {
    if (newSource == null)
      return false;
    switch (currentSource)
    {
      case CalculatedPartitionSource calculatedPartitionSource2:
        if (newSource is CalculatedPartitionSource calculatedPartitionSource1 && !(calculatedPartitionSource2.Expression != calculatedPartitionSource1.Expression))
          return calculatedPartitionSource2.RetainDataTillForceCalculate != calculatedPartitionSource1.RetainDataTillForceCalculate;
        break;
      case MPartitionSource mpartitionSource2:
        if (newSource is MPartitionSource mpartitionSource1 && !(mpartitionSource2.Expression != mpartitionSource1.Expression))
          return (mpartitionSource2.Attributes != mpartitionSource1.Attributes);
        break;
      case QueryPartitionSource queryPartitionSource2:
        if (newSource is QueryPartitionSource queryPartitionSource1 && !(queryPartitionSource2.Query != queryPartitionSource1.Query))
          return queryPartitionSource2.DataSource != queryPartitionSource1.DataSource;
        break;
      case PolicyRangePartitionSource rangePartitionSource2:
        if (newSource is PolicyRangePartitionSource rangePartitionSource1 && rangePartitionSource2.Start == rangePartitionSource1.Start && rangePartitionSource2.End == rangePartitionSource1.End && rangePartitionSource2.Granularity == rangePartitionSource1.Granularity)
          return (rangePartitionSource2.RefreshBookmark != rangePartitionSource1.RefreshBookmark);
        break;
      case EntityPartitionSource entityPartitionSource2:
        if (newSource is EntityPartitionSource entityPartitionSource1 && !(entityPartitionSource2.EntityName != entityPartitionSource1.EntityName) && !(entityPartitionSource2.SchemaName != entityPartitionSource1.SchemaName) && entityPartitionSource2.DataSource == entityPartitionSource1.DataSource)
          return entityPartitionSource2.ExpressionSource != entityPartitionSource1.ExpressionSource;
        break;
    }
    return true;
  }

  private static bool UpdateExistingPartitionSource(
    Microsoft.AnalysisServices.Tabular.Partition partition,
    PartitionUpdate update,
    Microsoft.AnalysisServices.Tabular.Database db)
  {
    bool flag = false;
    switch (partition.Source)
    {
      case CalculatedPartitionSource calculatedPartitionSource:
        if (!string.IsNullOrWhiteSpace(update.Expression) && (string.IsNullOrWhiteSpace(update.SourceType) || (update.SourceType.ToLower() == "calculated")) && (calculatedPartitionSource.Expression != update.Expression))
        {
          calculatedPartitionSource.Expression = update.Expression;
          flag = true;
        }
        if (update.RetainDataTillForceCalculate.HasValue && calculatedPartitionSource.RetainDataTillForceCalculate != update.RetainDataTillForceCalculate.Value)
        {
          calculatedPartitionSource.RetainDataTillForceCalculate = update.RetainDataTillForceCalculate.Value;
          flag = true;
          break;
        }
        break;
      case MPartitionSource mpartitionSource:
        if (!string.IsNullOrWhiteSpace(update.Expression) && (string.IsNullOrWhiteSpace(update.SourceType) || (update.SourceType.ToLower() == "m")) && (mpartitionSource.Expression != update.Expression))
        {
          mpartitionSource.Expression = update.Expression;
          flag = true;
        }
        if (!string.IsNullOrWhiteSpace(update.Attributes) && (mpartitionSource.Attributes != update.Attributes))
        {
          mpartitionSource.Attributes = update.Attributes;
          flag = true;
          break;
        }
        break;
      case QueryPartitionSource queryPartitionSource:
        if (!string.IsNullOrWhiteSpace(update.Query) && (queryPartitionSource.Query != update.Query))
        {
          queryPartitionSource.Query = update.Query;
          flag = true;
        }
        if (!string.IsNullOrWhiteSpace(update.DataSourceName))
        {
          Microsoft.AnalysisServices.Tabular.DataSource dataSource = db.Model.DataSources.Find(update.DataSourceName) ?? throw new McpException($"Data source '{update.DataSourceName}' not found");
          if (queryPartitionSource.DataSource != dataSource)
          {
            queryPartitionSource.DataSource = dataSource;
            flag = true;
            break;
          }
          break;
        }
        break;
      case PolicyRangePartitionSource rangePartitionSource:
        if (!string.IsNullOrWhiteSpace(update.Granularity))
        {
          RefreshGranularityType refreshGranularityType;
          if (Enum.TryParse<RefreshGranularityType>(update.Granularity, true, out refreshGranularityType))
          {
            if (rangePartitionSource.Granularity != refreshGranularityType)
            {
              rangePartitionSource.Granularity = refreshGranularityType;
              flag = true;
              break;
            }
            break;
          }
          string[] names = Enum.GetNames(typeof (RefreshGranularityType));
          throw new McpException($"Invalid granularity '{update.Granularity}'. Valid values are: {string.Join(", ", names)}");
        }
        break;
      case EntityPartitionSource entityPartitionSource:
        if (!string.IsNullOrWhiteSpace(update.EntityName) && (entityPartitionSource.EntityName != update.EntityName))
        {
          entityPartitionSource.EntityName = update.EntityName;
          flag = true;
        }
        if (update.SchemaName != null)
        {
          string schemaName = string.IsNullOrWhiteSpace(update.SchemaName) ? (string) null : update.SchemaName;
          if ((entityPartitionSource.SchemaName != schemaName))
          {
            entityPartitionSource.SchemaName = schemaName;
            flag = true;
          }
        }
        if (!string.IsNullOrWhiteSpace(update.DataSourceName))
        {
          Microsoft.AnalysisServices.Tabular.DataSource dataSource = db.Model.DataSources.Find(update.DataSourceName) ?? throw new McpException($"Data source '{update.DataSourceName}' not found");
          if (entityPartitionSource.DataSource != dataSource)
          {
            entityPartitionSource.DataSource = dataSource;
            flag = true;
          }
        }
        if (!string.IsNullOrWhiteSpace(update.ExpressionSourceName))
        {
          Microsoft.AnalysisServices.Tabular.NamedExpression namedExpression = db.Model.Expressions.Find(update.ExpressionSourceName) ?? throw new McpException($"Expression source '{update.ExpressionSourceName}' not found");
          if (entityPartitionSource.ExpressionSource != namedExpression)
          {
            entityPartitionSource.ExpressionSource = namedExpression;
            flag = true;
            break;
          }
          break;
        }
        break;
    }
    return flag;
  }

  public static TmdlExportResult ExportTMDL(
    string? connectionName,
    string tableName,
    string partitionName,
    PartitionExportTmdl? options = null)
  {
    if (string.IsNullOrWhiteSpace(tableName))
      throw new ArgumentException("Table name cannot be null or empty", nameof (tableName));
    if (string.IsNullOrWhiteSpace(partitionName))
      throw new ArgumentException("Partition name cannot be null or empty", nameof (partitionName));
    try
    {
      Microsoft.AnalysisServices.Tabular.Partition @object = (ConnectionOperations.Get(connectionName).Database.Model.Tables.Find(tableName) ?? throw new ArgumentException($"Table '{tableName}' not found")).Partitions.Find(partitionName);
      if (@object == null)
        throw new ArgumentException($"Partition '{partitionName}' not found in table '{tableName}'");
      string str1;
      if (options?.SerializationOptions != null)
      {
        MetadataSerializationOptions serializationOptions = options.SerializationOptions.ToMetadataSerializationOptions();
        str1 = TmdlSerializer.SerializeObject((MetadataObject) @object, serializationOptions);
      }
      else
        str1 = TmdlSerializer.SerializeObject((MetadataObject) @object);
      if (options == null)
        return TmdlExportResult.CreateSuccess($"{tableName}.{partitionName}", "Partition", str1, str1, false, (string) null, new List<string>());
      (string str2, bool flag, string str3, List<string> stringList) = ExportContentProcessor.ProcessExportContent(str1, (ExportOptionsBase) options);
      return TmdlExportResult.CreateSuccess($"{tableName}.{partitionName}", "Partition", str1, str2, flag, str3, stringList, (ExportTmdl) options);
    }
    catch (Exception ex)
    {
      return TmdlExportResult.CreateFailure($"{tableName}.{partitionName}", "Partition", ex.Message);
    }
  }

  public static TmslExportResult ExportTMSL(
    string? connectionName,
    string tableName,
    string partitionName,
    PartitionExportTmsl tmslOptions)
  {
    if (string.IsNullOrWhiteSpace(tableName))
      throw new ArgumentException("Table name cannot be null or empty", nameof (tableName));
    if (string.IsNullOrWhiteSpace(partitionName))
      throw new ArgumentException("Partition name cannot be null or empty", nameof (partitionName));
    if (tmslOptions == null)
      throw new ArgumentNullException(nameof (tmslOptions));
    if (string.IsNullOrWhiteSpace(tmslOptions.TmslOperationType))
      throw new McpException("TmslOperationType is required in tmslOptions");
    try
    {
      Microsoft.AnalysisServices.Tabular.Partition metadataObject = (ConnectionOperations.Get(connectionName).Database.Model.Tables.Find(tableName) ?? throw new ArgumentException($"Table '{tableName}' not found")).Partitions.Find(partitionName);
      if (metadataObject == null)
        throw new ArgumentException($"Partition '{partitionName}' not found in table '{tableName}'");
      TmslOperationType operationType;
      if (!Enum.TryParse<TmslOperationType>(tmslOptions.TmslOperationType, true, out operationType))
      {
        string[] names = Enum.GetNames<TmslOperationType>();
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
          string[] names = Enum.GetNames<RefreshType>();
          throw new McpException($"Invalid RefreshType '{tmslOptions.RefreshType}'. Valid values: {string.Join(", ", names)}");
        }
      }
      TmslExportResult tmslExportResult = TmslExportResult.FromLegacyResult(TmslScriptingService.GenerateScript<Microsoft.AnalysisServices.Tabular.Partition>(metadataObject, operationType, options));
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
      return TmslExportResult.CreateFailure($"{tableName}.{partitionName}", "Partition", ex.Message);
    }
  }
}
