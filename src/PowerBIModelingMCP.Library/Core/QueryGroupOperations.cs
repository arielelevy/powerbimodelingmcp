// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Core.QueryGroupOperations
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using Microsoft.AnalysisServices.Tabular;
using Microsoft.AnalysisServices.Tabular.Serialization;
using ModelContextProtocol;
using PowerBIModelingMCP.Library.Common.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable
namespace PowerBIModelingMCP.Library.Core;

public static class QueryGroupOperations
{
  public static Microsoft.AnalysisServices.Tabular.QueryGroup FindOrCreateQueryGroup(
    Microsoft.AnalysisServices.Tabular.Database db,
    string queryGroupName,
    out bool wasCreated)
  {
    Microsoft.AnalysisServices.Tabular.QueryGroup createQueryGroup = db.Model.QueryGroups.Find(queryGroupName);
    if (createQueryGroup != null)
    {
      wasCreated = false;
      return createQueryGroup;
    }
    Microsoft.AnalysisServices.Tabular.QueryGroup metadataObject = new Microsoft.AnalysisServices.Tabular.QueryGroup()
    {
      Folder = queryGroupName
    };
    db.Model.QueryGroups.Add(metadataObject);
    wasCreated = true;
    return metadataObject;
  }

  public static void ValidateQueryGroupDefinition(QueryGroupBase def, bool isCreate)
  {
    if (def == null)
      throw new McpException("QueryGroup definition cannot be null");
    if (isCreate && string.IsNullOrWhiteSpace(def.Folder))
      throw new McpException("Folder is required for create operations");
    if (!string.IsNullOrWhiteSpace(def.Folder))
    {
      char[] invalidChars = new char[7]
      {
        '<',
        '>',
        ':',
        '"',
        '|',
        '?',
        '*'
      };
      if (Enumerable.Any<char>((IEnumerable<char>) def.Folder, (c => Enumerable.Contains<char>((IEnumerable<char>) invalidChars, c))))
        throw new McpException("Folder contains invalid characters");
    }
    AnnotationHelpers.ValidateAnnotations(def.Annotations);
  }

  public static List<QueryGroupList> ListQueryGroups(string? connectionName)
  {
    return Enumerable.ToList<QueryGroupList>(Enumerable.Select<Microsoft.AnalysisServices.Tabular.QueryGroup, QueryGroupList>((IEnumerable<Microsoft.AnalysisServices.Tabular.QueryGroup>) ConnectionOperations.Get(connectionName).Database.Model.QueryGroups, (qg =>
    {
      return new QueryGroupList()
      {
        Name = qg.Name,
        Description = !string.IsNullOrEmpty(qg.Description) ? qg.Description : (string) null,
        Folder = !string.IsNullOrEmpty(qg.Folder) ? qg.Folder : (string) null
      };
    })));
  }

  public static QueryGroupGet GetQueryGroup(string? connectionName, string queryGroupName)
  {
    if (string.IsNullOrWhiteSpace(queryGroupName))
      throw new McpException("queryGroupName is required");
    Microsoft.AnalysisServices.Tabular.QueryGroup queryGroup1 = ConnectionOperations.Get(connectionName).Database.Model.QueryGroups.Find(queryGroupName) ?? throw new McpException($"QueryGroup '{queryGroupName}' not found");
    QueryGroupGet queryGroupGet = new QueryGroupGet { Name = queryGroup1.Name };
    queryGroupGet.Description = queryGroup1.Description;
    queryGroupGet.Folder = queryGroup1.Folder;
    queryGroupGet.Annotations = new List<KeyValuePair<string, string>>();
    QueryGroupGet queryGroup2 = queryGroupGet;
    if (queryGroup1.Annotations != null)
    {
      foreach (Microsoft.AnalysisServices.Tabular.Annotation annotation in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.Annotation, Microsoft.AnalysisServices.Tabular.QueryGroup>) queryGroup1.Annotations)
        queryGroup2.Annotations.Add(new KeyValuePair<string, string>(annotation.Name, annotation.Value));
    }
    return queryGroup2;
  }

  public static string ExportTMDL(string? connectionName, string queryGroupName, ExportTmdl? options)
  {
    if (string.IsNullOrWhiteSpace(queryGroupName))
      throw new McpException("queryGroupName is required");
    Microsoft.AnalysisServices.Tabular.QueryGroup @object = ConnectionOperations.Get(connectionName).Database.Model.QueryGroups.Find(queryGroupName) ?? throw new McpException($"QueryGroup '{queryGroupName}' not found");
    if (options?.SerializationOptions == null)
      return TmdlSerializer.SerializeObject((MetadataObject) @object);
    MetadataSerializationOptions serializationOptions = options.SerializationOptions.ToMetadataSerializationOptions();
    return TmdlSerializer.SerializeObject((MetadataObject) @object, serializationOptions);
  }

  public static QueryGroupOperationResult CreateQueryGroup(
    string? connectionName,
    QueryGroupCreate def)
  {
    QueryGroupOperations.ValidateQueryGroupDefinition((QueryGroupBase) def, true);
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Database database = info.Database;
    Microsoft.AnalysisServices.Tabular.QueryGroup queryGroup = new Microsoft.AnalysisServices.Tabular.QueryGroup()
    {
      Folder = def.Folder
    };
    if (!string.IsNullOrWhiteSpace(def.Description))
      queryGroup.Description = def.Description;
    if (def.Annotations != null)
    {
      foreach (KeyValuePair<string, string> annotation in def.Annotations)
      {
        QueryGroupAnnotationCollection annotations = queryGroup.Annotations;
        Microsoft.AnalysisServices.Tabular.Annotation metadataObject = new Microsoft.AnalysisServices.Tabular.Annotation();
        metadataObject.Name = annotation.Key;
        metadataObject.Value = annotation.Value;
        annotations.Add(metadataObject);
      }
    }
    database.Model.QueryGroups.Add(queryGroup);
    TransactionOperations.RecordOperation(info, "Created query group in model " + database.Model.Name);
    ConnectionOperations.SaveChangesWithRollback(info, "create query group");
    return QueryGroupOperations.CreateQueryGroupOperationResult(queryGroup);
  }

  public static QueryGroupOperationResult UpdateQueryGroup(
    string? connectionName,
    QueryGroupUpdate update)
  {
    QueryGroupOperations.ValidateQueryGroupDefinition((QueryGroupBase) update, false);
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Database database = info.Database;
    Microsoft.AnalysisServices.Tabular.QueryGroup queryGroup = database.Model.QueryGroups.Find(update.Name) ?? throw new McpException($"QueryGroup '{update.Name}' not found");
    bool flag = false;
    if (update.Description != null)
    {
      string description = string.IsNullOrEmpty(update.Description) ? (string) null : update.Description;
      if ((queryGroup.Description != description))
      {
        queryGroup.Description = description;
        flag = true;
      }
    }
    if (update.Folder != null)
    {
      string folder = string.IsNullOrEmpty(update.Folder) ? (string) null : update.Folder;
      if ((queryGroup.Folder != folder))
      {
        queryGroup.Folder = folder;
        flag = true;
      }
    }
    if (update.Annotations != null)
    {
      queryGroup.Annotations.Clear();
      foreach (KeyValuePair<string, string> annotation in update.Annotations)
      {
        QueryGroupAnnotationCollection annotations = queryGroup.Annotations;
        Microsoft.AnalysisServices.Tabular.Annotation metadataObject = new Microsoft.AnalysisServices.Tabular.Annotation();
        metadataObject.Name = annotation.Key;
        metadataObject.Value = annotation.Value;
        annotations.Add(metadataObject);
      }
      flag = true;
    }
    if (!flag)
      return QueryGroupOperations.CreateQueryGroupOperationResult(queryGroup, false);
    TransactionOperations.RecordOperation(info, $"Updated query group '{update.Name}' in model {database.Model.Name}");
    ConnectionOperations.SaveChangesWithRollback(info, "update query group");
    return QueryGroupOperations.CreateQueryGroupOperationResult(queryGroup);
  }

  public static void DeleteQueryGroup(string? connectionName, string queryGroupName)
  {
    if (string.IsNullOrWhiteSpace(queryGroupName))
      throw new McpException("queryGroupName is required");
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Database database = info.Database;
    Microsoft.AnalysisServices.Tabular.QueryGroup queryGroup = database.Model.QueryGroups.Find(queryGroupName) ?? throw new McpException($"QueryGroup '{queryGroupName}' not found");
    List<string> stringList = QueryGroupOperations.CheckQueryGroupDependencies(database, queryGroup);
    if (stringList.Count > 0)
      throw new McpException($"Cannot delete query group '{queryGroupName}' because it has dependencies: {string.Join(", ", (IEnumerable<string>) stringList)}");
    database.Model.QueryGroups.Remove(queryGroup);
    TransactionOperations.RecordOperation(info, $"Deleted query group '{queryGroupName}' from model {database.Model.Name}");
    ConnectionOperations.SaveChangesWithRollback(info, "delete query group");
  }

  private static List<string> CheckQueryGroupDependencies(Microsoft.AnalysisServices.Tabular.Database db, Microsoft.AnalysisServices.Tabular.QueryGroup queryGroup)
  {
    List<string> stringList = new List<string>();
    foreach (Microsoft.AnalysisServices.Tabular.Table table in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.Table, Microsoft.AnalysisServices.Tabular.Model>) db.Model.Tables)
    {
      foreach (Microsoft.AnalysisServices.Tabular.Partition partition in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.Partition, Microsoft.AnalysisServices.Tabular.Table>) table.Partitions)
      {
        if (partition.QueryGroup == queryGroup)
          stringList.Add($"Partition: {table.Name}.{partition.Name}");
      }
    }
    foreach (Microsoft.AnalysisServices.Tabular.NamedExpression expression in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.NamedExpression, Microsoft.AnalysisServices.Tabular.Model>) db.Model.Expressions)
    {
      if (expression.QueryGroup == queryGroup)
        stringList.Add("NamedExpression: " + expression.Name);
    }
    return stringList;
  }

  private static QueryGroupOperationResult CreateQueryGroupOperationResult(
    Microsoft.AnalysisServices.Tabular.QueryGroup queryGroup,
    bool hasChanges = true)
  {
    return new QueryGroupOperationResult()
    {
      QueryGroupName = queryGroup.Name,
      HasChanges = hasChanges
    };
  }
}
