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

public static class DataSourceOperations
{
  public static void ValidateDataSourceDefinition(DataSourceBase def, bool isCreate)
  {
    if (def == null)
      throw new McpException("DataSource definition cannot be null");
    if (isCreate)
    {
      if (string.IsNullOrWhiteSpace(def.Name))
        throw new McpException("Name is required");
      if (string.IsNullOrWhiteSpace(def.ConnectionString))
        throw new McpException("ConnectionString is required");
    }
    if (!string.IsNullOrWhiteSpace(def.ImpersonationMode) && !Enum.IsDefined(typeof (ImpersonationMode), (object) def.ImpersonationMode))
    {
      string[] names = Enum.GetNames(typeof (ImpersonationMode));
      throw new McpException($"Invalid ImpersonationMode '{def.ImpersonationMode}'. Valid values are: {string.Join(", ", names)}");
    }
    if (!string.IsNullOrWhiteSpace(def.Isolation) && !Enum.IsDefined(typeof (DatasourceIsolation), (object) def.Isolation))
    {
      string[] names = Enum.GetNames(typeof (DatasourceIsolation));
      throw new McpException($"Invalid Isolation '{def.Isolation}'. Valid values are: {string.Join(", ", names)}");
    }
  }

  public static List<DataSourceList> ListDataSources(string? connectionName)
  {
    Microsoft.AnalysisServices.Tabular.Database database = ConnectionOperations.Get(connectionName).Database;
    List<DataSourceList> dataSourceListList1 = new List<DataSourceList>();
    foreach (Microsoft.AnalysisServices.Tabular.DataSource dataSource in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.DataSource, Microsoft.AnalysisServices.Tabular.Model>) database.Model.DataSources)
    {
      List<DataSourceList> dataSourceListList2 = dataSourceListList1;
      DataSourceList dataSourceList = new DataSourceList { Name = dataSource.Name };
      dataSourceList.Description = !string.IsNullOrEmpty(dataSource.Description) ? dataSource.Description : (string) null;
      dataSourceList.Type = dataSource.Type.ToString();
      dataSourceListList2.Add(dataSourceList);
    }
    return dataSourceListList1;
  }

  public static DataSourceGet GetDataSource(string? connectionName, string dataSourceName)
  {
    if (string.IsNullOrWhiteSpace(dataSourceName))
      throw new McpException("dataSourceName is required");
    Microsoft.AnalysisServices.Tabular.DataSource dataSource1 = ConnectionOperations.Get(connectionName).Database.Model.DataSources.Find(dataSourceName) ?? throw new McpException($"Data source '{dataSourceName}' not found");
    DataSourceGet dataSourceGet = new DataSourceGet { Name = dataSource1.Name };
    dataSourceGet.Type = dataSource1.Type.ToString();
    dataSourceGet.Description = dataSource1.Description;
    dataSourceGet.MaxConnections = new int?(dataSource1.MaxConnections);
    dataSourceGet.Annotations = new List<KeyValuePair<string, string>>();
    dataSourceGet.ExtendedProperties = new List<PowerBIModelingMCP.Library.Common.DataStructures.ExtendedProperty>();
    DataSourceGet dataSource2 = dataSourceGet;
    switch (dataSource1)
    {
      case Microsoft.AnalysisServices.Tabular.ProviderDataSource providerDataSource:
        dataSource2.Provider = providerDataSource.Provider;
        dataSource2.ConnectionString = providerDataSource.ConnectionString;
        dataSource2.ImpersonationMode = providerDataSource.ImpersonationMode.ToString();
        dataSource2.Account = providerDataSource.Account;
        dataSource2.Isolation = providerDataSource.Isolation.ToString();
        break;
      case Microsoft.AnalysisServices.Tabular.StructuredDataSource _:
        dataSource2.Provider = "Structured/M";
        dataSource2.ConnectionString = "[StructuredDataSource - use ConnectionDetails]";
        break;
    }
    foreach (Microsoft.AnalysisServices.Tabular.Annotation annotation in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.Annotation, Microsoft.AnalysisServices.Tabular.DataSource>) dataSource1.Annotations)
      dataSource2.Annotations.Add(new KeyValuePair<string, string>(annotation.Name, annotation.Value));
    dataSource2.ExtendedProperties = ExtendedPropertyHelpers.ExtractFromDataSource(dataSource1);
    return dataSource2;
  }

  public static OperationResult CreateDataSource(string? connectionName, DataSourceCreate def)
  {
    DataSourceOperations.ValidateDataSourceDefinition((DataSourceBase) def, true);
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Database database = info.Database;
    if (database.Model.DataSources.Contains(def.Name))
      throw new McpException($"Data source '{def.Name}' already exists");
    Microsoft.AnalysisServices.Tabular.ProviderDataSource providerDataSource1 = new Microsoft.AnalysisServices.Tabular.ProviderDataSource();
    providerDataSource1.Name = def.Name;
    providerDataSource1.ConnectionString = def.ConnectionString;
    providerDataSource1.Provider = def.Provider ?? "System.Data.SqlClient";
    Microsoft.AnalysisServices.Tabular.ProviderDataSource providerDataSource2 = providerDataSource1;
    DataSourceOperations.ApplyDataSourceProperties(providerDataSource2, (DataSourceBase) def);
    database.Model.DataSources.Add((Microsoft.AnalysisServices.Tabular.DataSource) providerDataSource2);
    TransactionOperations.RecordOperation(info, $"Created data source '{def.Name}'");
    ConnectionOperations.SaveChangesWithRollback(info, "create data source");
    return new OperationResult()
    {
      Success = true,
      Message = $"Data source '{def.Name}' created successfully",
      ObjectName = def.Name,
      ObjectType = new ObjectType?(ObjectType.DataSource),
      Operation = new Operation?(Operation.Create)
    };
  }

  public static OperationResult UpdateDataSource(string? connectionName, DataSourceUpdate update)
  {
    DataSourceOperations.ValidateDataSourceDefinition((DataSourceBase) update, false);
    if (string.IsNullOrWhiteSpace(update.Name))
      throw new McpException("Name is required to identify the data source to update");
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.DataSource dataSource = info.Database.Model.DataSources.Find(update.Name);
    if (dataSource == null)
      throw new McpException($"Data source '{update.Name}' not found");
    if (!DataSourceOperations.ApplyDataSourceUpdates(dataSource, update))
      return new OperationResult()
      {
        Success = true,
        Message = $"Data source '{update.Name}' is already in the requested state",
        ObjectName = update.Name,
        ObjectType = new ObjectType?(ObjectType.DataSource),
        Operation = new Operation?(Operation.Update),
        HasChanges = false
      };
    TransactionOperations.RecordOperation(info, $"Updated data source '{update.Name}'");
    ConnectionOperations.SaveChangesWithRollback(info, "update data source");
    return new OperationResult()
    {
      Success = true,
      Message = $"Data source '{update.Name}' updated successfully",
      ObjectName = update.Name,
      ObjectType = new ObjectType?(ObjectType.DataSource),
      Operation = new Operation?(Operation.Update),
      HasChanges = true
    };
  }

  public static void DeleteDataSource(string? connectionName, string dataSourceName)
  {
    if (string.IsNullOrWhiteSpace(dataSourceName))
      throw new McpException("dataSourceName is required");
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Database database = info.Database;
    Microsoft.AnalysisServices.Tabular.DataSource ds = database.Model.DataSources.Find(dataSourceName) ?? throw new McpException($"Data source '{dataSourceName}' not found");
    if (Enumerable.Any<Microsoft.AnalysisServices.Tabular.Table>((IEnumerable<Microsoft.AnalysisServices.Tabular.Table>) database.Model.Tables, (t => Enumerable.Any<Microsoft.AnalysisServices.Tabular.Partition>((IEnumerable<Microsoft.AnalysisServices.Tabular.Partition>) t.Partitions, (p => p.Source is QueryPartitionSource source && source.DataSource == ds)))))
      throw new McpException($"Cannot delete data source '{dataSourceName}' as it is referenced by one or more table partitions");
    database.Model.DataSources.Remove(ds);
    TransactionOperations.RecordOperation(info, $"Deleted data source '{dataSourceName}'");
    ConnectionOperations.SaveChangesWithRollback(info, "delete data source");
  }

  public static void RenameDataSource(string? connectionName, string currentName, string newName)
  {
    if (string.IsNullOrWhiteSpace(currentName))
      throw new McpException("currentName is required");
    if (string.IsNullOrWhiteSpace(newName))
      throw new McpException("newName is required");
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Database database = info.Database;
    Microsoft.AnalysisServices.Tabular.DataSource dataSource = database.Model.DataSources.Find(currentName) ?? throw new McpException($"Data source '{currentName}' not found");
    if (database.Model.DataSources.Contains(newName) && !string.Equals(currentName, newName, StringComparison.OrdinalIgnoreCase))
      throw new McpException($"Data source '{newName}' already exists");
    dataSource.RequestRename(newName);
    TransactionOperations.RecordOperation(info, $"Renamed data source '{currentName}' to '{newName}'");
    ConnectionOperations.SaveChangesWithRollback(info, "rename data source", ConnectionOperations.CheckpointMode.AfterRequestRename);
  }

  public static OperationResult TestDataSource(string? connectionName, string dataSourceName)
  {
    if (string.IsNullOrWhiteSpace(dataSourceName))
      throw new McpException("dataSourceName is required");
    Microsoft.AnalysisServices.Tabular.DataSource dataSource = ConnectionOperations.Get(connectionName).Database.Model.DataSources.Find(dataSourceName) ?? throw new McpException($"Data source '{dataSourceName}' not found");
    try
    {
      bool flag;
      string str;
      switch (dataSource)
      {
        case Microsoft.AnalysisServices.Tabular.ProviderDataSource providerDataSource:
          flag = !string.IsNullOrEmpty(providerDataSource.ConnectionString);
          str = flag ? $"Data source '{dataSourceName}' connection configuration is valid" : $"Data source '{dataSourceName}' connection test failed - no connection string";
          break;
        case Microsoft.AnalysisServices.Tabular.StructuredDataSource structuredDataSource:
          flag = structuredDataSource.ConnectionDetails != null;
          str = flag ? $"Data source '{dataSourceName}' connection configuration is valid" : $"Data source '{dataSourceName}' connection test failed - no connection details";
          break;
        default:
          flag = false;
          str = $"Data source '{dataSourceName}' type '{dataSource.Type}' is not supported for testing";
          break;
      }
      return new OperationResult()
      {
        Success = flag,
        Message = str,
        ObjectName = dataSourceName,
        ObjectType = new ObjectType?(ObjectType.DataSource),
        Operation = new Operation?(Operation.Get)
      };
    }
    catch (Exception ex)
    {
      return new OperationResult()
      {
        Success = false,
        Message = $"Data source '{dataSourceName}' connection test failed: {ex.Message}",
        ObjectName = dataSourceName,
        ObjectType = new ObjectType?(ObjectType.DataSource),
        Operation = new Operation?(Operation.Get),
        Exception = ex
      };
    }
  }

  private static string GetProviderInfo(Microsoft.AnalysisServices.Tabular.DataSource ds)
  {
    switch (ds)
    {
      case Microsoft.AnalysisServices.Tabular.ProviderDataSource providerDataSource:
        return providerDataSource.Provider ?? "Unknown";
      case Microsoft.AnalysisServices.Tabular.StructuredDataSource _:
        return "Structured/M";
      default:
        return ds.Type.ToString();
    }
  }

  private static void ApplyDataSourceProperties(Microsoft.AnalysisServices.Tabular.ProviderDataSource dataSource, DataSourceBase def)
  {
    if (!string.IsNullOrWhiteSpace(def.Description))
      dataSource.Description = def.Description;
    int? nullable = def.MaxConnections;
    if (nullable.HasValue)
    {
      Microsoft.AnalysisServices.Tabular.ProviderDataSource providerDataSource = dataSource;
      nullable = def.MaxConnections;
      int num = nullable.Value;
      providerDataSource.MaxConnections = num;
    }
    nullable = def.Timeout;
    if (nullable.HasValue)
    {
      Microsoft.AnalysisServices.Tabular.ProviderDataSource providerDataSource = dataSource;
      nullable = def.Timeout;
      int num = nullable.Value;
      providerDataSource.Timeout = num;
    }
    ImpersonationMode impersonationMode;
    if (!string.IsNullOrWhiteSpace(def.ImpersonationMode) && Enum.TryParse<ImpersonationMode>(def.ImpersonationMode, true, out impersonationMode))
      dataSource.ImpersonationMode = impersonationMode;
    if (!string.IsNullOrWhiteSpace(def.Account))
      dataSource.Account = def.Account;
    if (!string.IsNullOrWhiteSpace(def.Password))
      dataSource.Password = def.Password;
    DatasourceIsolation datasourceIsolation;
    if (!string.IsNullOrWhiteSpace(def.Isolation) && Enum.TryParse<DatasourceIsolation>(def.Isolation, true, out datasourceIsolation))
      dataSource.Isolation = datasourceIsolation;
    if (def.Annotations != null)
    {
      foreach (KeyValuePair<string, string> annotation in def.Annotations)
      {
        DataSourceAnnotationCollection annotations = dataSource.Annotations;
        Microsoft.AnalysisServices.Tabular.Annotation metadataObject = new Microsoft.AnalysisServices.Tabular.Annotation();
        metadataObject.Name = annotation.Key;
        metadataObject.Value = annotation.Value;
        annotations.Add(metadataObject);
      }
    }
    if (def.ExtendedProperties == null)
      return;
    ExtendedPropertyHelpers.ApplyToDataSource((Microsoft.AnalysisServices.Tabular.DataSource) dataSource, def.ExtendedProperties);
  }

  private static bool ApplyDataSourceUpdates(Microsoft.AnalysisServices.Tabular.DataSource dataSource, DataSourceUpdate update)
  {
    bool flag = false;
    if (update.Description != null)
    {
      string description = string.IsNullOrEmpty(update.Description) ? (string) null : update.Description;
      if ((description != dataSource.Description))
      {
        dataSource.Description = description;
        flag = true;
      }
    }
    int? nullable;
    if (update.MaxConnections.HasValue)
    {
      int maxConnections = dataSource.MaxConnections;
      nullable = update.MaxConnections;
      int num1 = nullable.Value;
      if (maxConnections != num1)
      {
        Microsoft.AnalysisServices.Tabular.DataSource dataSource1 = dataSource;
        nullable = update.MaxConnections;
        int num2 = nullable.Value;
        dataSource1.MaxConnections = num2;
        flag = true;
      }
    }
    if (dataSource is Microsoft.AnalysisServices.Tabular.ProviderDataSource providerDataSource1)
    {
      if (update.ConnectionString != null)
      {
        string connectionString = string.IsNullOrEmpty(update.ConnectionString) ? (string) null : update.ConnectionString;
        if ((connectionString != providerDataSource1.ConnectionString))
        {
          providerDataSource1.ConnectionString = connectionString;
          flag = true;
        }
      }
      nullable = update.Timeout;
      if (nullable.HasValue)
      {
        int timeout = providerDataSource1.Timeout;
        nullable = update.Timeout;
        int num3 = nullable.Value;
        if (timeout != num3)
        {
          Microsoft.AnalysisServices.Tabular.ProviderDataSource providerDataSource = providerDataSource1;
          nullable = update.Timeout;
          int num4 = nullable.Value;
          providerDataSource.Timeout = num4;
          flag = true;
        }
      }
      if (update.Provider != null)
      {
        string provider = string.IsNullOrEmpty(update.Provider) ? (string) null : update.Provider;
        if ((provider != providerDataSource1.Provider))
        {
          providerDataSource1.Provider = provider;
          flag = true;
        }
      }
      if (!string.IsNullOrWhiteSpace(update.ImpersonationMode))
      {
        ImpersonationMode impersonationMode;
        if (Enum.TryParse<ImpersonationMode>(update.ImpersonationMode, true, out impersonationMode))
        {
          if (providerDataSource1.ImpersonationMode != impersonationMode)
          {
            providerDataSource1.ImpersonationMode = impersonationMode;
            flag = true;
          }
        }
        else
        {
          string[] names = Enum.GetNames(typeof (ImpersonationMode));
          throw new McpException($"Invalid ImpersonationMode '{update.ImpersonationMode}'. Valid values are: {string.Join(", ", names)}");
        }
      }
      if (update.Account != null)
      {
        string account = string.IsNullOrEmpty(update.Account) ? (string) null : update.Account;
        if ((account != providerDataSource1.Account))
        {
          providerDataSource1.Account = account;
          flag = true;
        }
      }
      if (update.Password != null)
      {
        string password = string.IsNullOrEmpty(update.Password) ? (string) null : update.Password;
        if ((password != providerDataSource1.Password))
        {
          providerDataSource1.Password = password;
          flag = true;
        }
      }
      if (!string.IsNullOrWhiteSpace(update.Isolation))
      {
        DatasourceIsolation datasourceIsolation;
        if (Enum.TryParse<DatasourceIsolation>(update.Isolation, true, out datasourceIsolation))
        {
          if (providerDataSource1.Isolation != datasourceIsolation)
          {
            providerDataSource1.Isolation = datasourceIsolation;
            flag = true;
          }
        }
        else
        {
          string[] names = Enum.GetNames(typeof (DatasourceIsolation));
          throw new McpException($"Invalid Isolation '{update.Isolation}'. Valid values are: {string.Join(", ", names)}");
        }
      }
    }
    else
    {
      nullable = update.ConnectionString == null ? update.Timeout : throw new McpException("ConnectionString can only be updated on ProviderDataSource, not on StructuredDataSource");
      if (nullable.HasValue)
        throw new McpException("Timeout can only be updated on ProviderDataSource, not on StructuredDataSource");
      if (update.Provider != null)
        throw new McpException("Provider can only be updated on ProviderDataSource, not on StructuredDataSource");
      if (update.ImpersonationMode != null)
        throw new McpException("ImpersonationMode can only be updated on ProviderDataSource, not on StructuredDataSource");
      if (update.Account != null)
        throw new McpException("Account can only be updated on ProviderDataSource, not on StructuredDataSource");
      if (update.Password != null)
        throw new McpException("Password can only be updated on ProviderDataSource, not on StructuredDataSource");
      if (update.Isolation != null)
        throw new McpException("Isolation can only be updated on ProviderDataSource, not on StructuredDataSource");
    }
    if (update.Annotations != null && AnnotationHelpers.ReplaceAnnotations<Microsoft.AnalysisServices.Tabular.DataSource>(dataSource, update.Annotations, (Func<Microsoft.AnalysisServices.Tabular.DataSource, ICollection<Microsoft.AnalysisServices.Tabular.Annotation>>) (ds => (ICollection<Microsoft.AnalysisServices.Tabular.Annotation>) ds.Annotations)))
      flag = true;
    if (update.ExtendedProperties != null)
    {
      int num = dataSource.ExtendedProperties.Count > 0 ? 1 : 0;
      ExtendedPropertyHelpers.ReplaceDataSourceProperties(dataSource, update.ExtendedProperties);
      if (num != 0 || update.ExtendedProperties.Count > 0)
        flag = true;
    }
    return flag;
  }

  public static string ExportTMDL(string? connectionName, string dataSourceName, ExportTmdl? options)
  {
    Microsoft.AnalysisServices.Tabular.DataSource @object = ConnectionOperations.Get(connectionName).Database.Model.DataSources.Find(dataSourceName);
    if (@object == null)
      throw new ArgumentException($"Data source '{dataSourceName}' not found");
    if (options?.SerializationOptions == null)
      return TmdlSerializer.SerializeObject((MetadataObject) @object);
    MetadataSerializationOptions serializationOptions = options.SerializationOptions.ToMetadataSerializationOptions();
    return TmdlSerializer.SerializeObject((MetadataObject) @object, serializationOptions);
  }
}
