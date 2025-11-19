// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Core.SecurityRoleOperations
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

#nullable enable
namespace PowerBIModelingMCP.Library.Core;

public static class SecurityRoleOperations
{
  public static List<ModelRoleList> ListModelRoles(string? connectionName)
  {
    Microsoft.AnalysisServices.Tabular.Database database = ConnectionOperations.Get(connectionName).Database;
    List<ModelRoleList> modelRoleListList1 = new List<ModelRoleList>();
    foreach (Microsoft.AnalysisServices.Tabular.ModelRole role in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.ModelRole, Microsoft.AnalysisServices.Tabular.Model>) database.Model.Roles)
    {
      List<ModelRoleList> modelRoleListList2 = modelRoleListList1;
      ModelRoleList modelRoleList = new ModelRoleList { Name = role.Name };
      modelRoleList.Description = !string.IsNullOrEmpty(role.Description) ? role.Description : (string) null;
      modelRoleList.ModelPermission = role.ModelPermission.ToString();
      modelRoleList.TableNames = role.TablePermissions.Count > 0 ? Enumerable.ToList<string>(Enumerable.Cast<string>((IEnumerable) Enumerable.Where<string>(Enumerable.Select<Microsoft.AnalysisServices.Tabular.TablePermission, string>((IEnumerable<Microsoft.AnalysisServices.Tabular.TablePermission>) role.TablePermissions, (tp => tp.Table?.Name)), (name => !string.IsNullOrEmpty(name))))) : (List<string>) null;
      modelRoleListList2.Add(modelRoleList);
    }
    return modelRoleListList1;
  }

  public static ModelRoleGet GetModelRole(string? connectionName, string roleName)
  {
    if (string.IsNullOrWhiteSpace(roleName))
      throw new McpException("roleName is required");
    Microsoft.AnalysisServices.Tabular.ModelRole modelRole1 = ConnectionOperations.Get(connectionName).Database.Model.Roles.Find(roleName) ?? throw new McpException($"Model role '{roleName}' not found");
    ModelRoleGet modelRoleGet = new ModelRoleGet { Name = modelRole1.Name };
    modelRoleGet.Description = modelRole1.Description;
    modelRoleGet.ModelPermission = modelRole1.ModelPermission.ToString();
    modelRoleGet.TablePermissions = new List<Dictionary<string, string>>();
    modelRoleGet.Annotations = new List<KeyValuePair<string, string>>();
    modelRoleGet.ExtendedProperties = new List<PowerBIModelingMCP.Library.Common.DataStructures.ExtendedProperty>();
    ModelRoleGet modelRole2 = modelRoleGet;
    foreach (Microsoft.AnalysisServices.Tabular.TablePermission tablePermission in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.TablePermission, Microsoft.AnalysisServices.Tabular.ModelRole>) modelRole1.TablePermissions)
      modelRole2.TablePermissions.Add(new Dictionary<string, string>()
      {
        ["TableName"] = tablePermission.Table?.Name ?? "",
        ["FilterExpression"] = tablePermission.FilterExpression ?? "",
        ["MetadataPermission"] = tablePermission.MetadataPermission.ToString()
      });
    foreach (Microsoft.AnalysisServices.Tabular.Annotation annotation in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.Annotation, Microsoft.AnalysisServices.Tabular.ModelRole>) modelRole1.Annotations)
      modelRole2.Annotations.Add(new KeyValuePair<string, string>(annotation.Name, annotation.Value));
    modelRole2.ExtendedProperties = ExtendedPropertyHelpers.ExtractFromModelRole(modelRole1);
    return modelRole2;
  }

  public static TmdlExportResult ExportTMDL(
    string? connectionName,
    string roleName,
    RoleExportTmdl? options = null)
  {
    if (string.IsNullOrWhiteSpace(roleName))
      throw new McpException("roleName is required");
    try
    {
      Microsoft.AnalysisServices.Tabular.ModelRole @object = ConnectionOperations.Get(connectionName).Database.Model.Roles.Find(roleName) ?? throw new McpException($"Model role '{roleName}' not found");
      string str1;
      if (options?.SerializationOptions != null)
      {
        MetadataSerializationOptions serializationOptions = options.SerializationOptions.ToMetadataSerializationOptions();
        str1 = TmdlSerializer.SerializeObject((MetadataObject) @object, serializationOptions);
      }
      else
        str1 = TmdlSerializer.SerializeObject((MetadataObject) @object);
      if (options == null)
        return TmdlExportResult.CreateSuccess(roleName, "Role", str1, str1, false, (string) null, new List<string>());
      (string str2, bool flag, string str3, List<string> stringList) = ExportContentProcessor.ProcessExportContent(str1, (ExportOptionsBase) options);
      return TmdlExportResult.CreateSuccess(roleName, "Role", str1, str2, flag, str3, stringList, (ExportTmdl) options);
    }
    catch (Exception ex)
    {
      return TmdlExportResult.CreateFailure(roleName, "Role", ex.Message);
    }
  }

  public static TmslExportResult ExportTMSL(
    string? connectionName,
    string roleName,
    RoleExportTmsl tmslOptions)
  {
    if (string.IsNullOrWhiteSpace(roleName))
      throw new McpException("roleName is required");
    if (tmslOptions == null)
      throw new McpException("tmslOptions is required");
    if (string.IsNullOrWhiteSpace(tmslOptions.TmslOperationType))
      throw new McpException("TmslOperationType is required in tmslOptions");
    try
    {
      Microsoft.AnalysisServices.Tabular.ModelRole metadataObject = ConnectionOperations.Get(connectionName).Database.Model.Roles.Find(roleName);
      if (metadataObject == null)
        throw new McpException($"Model role '{roleName}' not found");
      TmslOperationType operationType;
      if (!Enum.TryParse<TmslOperationType>(tmslOptions.TmslOperationType, true, out operationType))
      {
        Enum.GetNames(typeof (TmslOperationType));
        throw new McpException($"Invalid TmslOperationType '{tmslOptions.TmslOperationType}'. Valid values for roles: Create, CreateOrReplace, Alter, Delete (Refresh not supported)");
      }
      if (operationType == TmslOperationType.Refresh)
        throw new McpException("Refresh operations are not supported for roles. Valid operations: Create, CreateOrReplace, Alter, Delete");
      TmslOperationRequest options = new TmslOperationRequest()
      {
        OperationType = operationType,
        IncludeRestricted = tmslOptions.IncludeRestricted.GetValueOrDefault()
      };
      TmslExportResult tmslExportResult = TmslExportResult.FromLegacyResult(TmslScriptingService.GenerateScript<Microsoft.AnalysisServices.Tabular.ModelRole>(metadataObject, operationType, options));
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
      return TmslExportResult.CreateFailure(roleName, "Role", ex.Message);
    }
  }

  public static void CreateModelRole(string? connectionName, ModelRoleCreate def)
  {
    SecurityRoleOperations.ValidateModelRoleDefinition((ModelRoleBase) def, true);
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Database database = info.Database;
    if (database.Model.Roles.Contains(def.Name))
      throw new McpException($"Model role '{def.Name}' already exists");
    Microsoft.AnalysisServices.Tabular.ModelRole modelRole1 = new Microsoft.AnalysisServices.Tabular.ModelRole();
    modelRole1.Name = def.Name;
    Microsoft.AnalysisServices.Tabular.ModelRole modelRole2 = modelRole1;
    if (!string.IsNullOrWhiteSpace(def.Description))
      modelRole2.Description = def.Description;
    if (!string.IsNullOrWhiteSpace(def.ModelPermission))
    {
      ModelPermission modelPermission;
      if (Enum.TryParse<ModelPermission>(def.ModelPermission, true, out modelPermission))
      {
        modelRole2.ModelPermission = modelPermission;
      }
      else
      {
        string[] names = Enum.GetNames(typeof (ModelPermission));
        throw new McpException($"Invalid ModelPermission '{def.ModelPermission}'. Valid values are: {string.Join(", ", names)}");
      }
    }
    else
      modelRole2.ModelPermission = ModelPermission.Read;
    if (def.Annotations != null)
    {
      foreach (KeyValuePair<string, string> annotation in def.Annotations)
      {
        ModelRoleAnnotationCollection annotations = modelRole2.Annotations;
        Microsoft.AnalysisServices.Tabular.Annotation metadataObject = new Microsoft.AnalysisServices.Tabular.Annotation();
        metadataObject.Name = annotation.Key;
        metadataObject.Value = annotation.Value;
        annotations.Add(metadataObject);
      }
    }
    if (def.ExtendedProperties != null)
      ExtendedPropertyHelpers.ApplyToModelRole(modelRole2, def.ExtendedProperties);
    database.Model.Roles.Add(modelRole2);
    TransactionOperations.RecordOperation(info, $"Created model role '{def.Name}'");
    ConnectionOperations.SaveChangesWithRollback(info, "create model role");
  }

  public static OperationResult UpdateModelRole(string? connectionName, ModelRoleUpdate update)
  {
    SecurityRoleOperations.ValidateModelRoleDefinition((ModelRoleBase) update, false);
    if (string.IsNullOrWhiteSpace(update.Name))
      throw new McpException("Name is required to identify the role to update");
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.ModelRole modelRole = info.Database.Model.Roles.Find(update.Name) ?? throw new McpException($"Role '{update.Name}' not found");
    bool flag = false;
    if (update.Description != null)
    {
      string description = string.IsNullOrEmpty(update.Description) ? (string) null : update.Description;
      if ((modelRole.Description != description))
      {
        modelRole.Description = description;
        flag = true;
      }
    }
    if (!string.IsNullOrWhiteSpace(update.ModelPermission))
    {
      ModelPermission modelPermission;
      if (Enum.TryParse<ModelPermission>(update.ModelPermission, true, out modelPermission))
      {
        if (modelRole.ModelPermission != modelPermission)
        {
          modelRole.ModelPermission = modelPermission;
          flag = true;
        }
      }
      else
      {
        string[] names = Enum.GetNames(typeof (ModelPermission));
        throw new McpException($"Invalid ModelPermission '{update.ModelPermission}'. Valid values are: {string.Join(", ", names)}");
      }
    }
    if (update.Annotations != null && AnnotationHelpers.ReplaceAnnotations<Microsoft.AnalysisServices.Tabular.ModelRole>(modelRole, update.Annotations, (Func<Microsoft.AnalysisServices.Tabular.ModelRole, ICollection<Microsoft.AnalysisServices.Tabular.Annotation>>) (obj => (ICollection<Microsoft.AnalysisServices.Tabular.Annotation>) obj.Annotations)))
      flag = true;
    if (update.ExtendedProperties != null)
    {
      int num = modelRole.ExtendedProperties.Count > 0 ? 1 : 0;
      ExtendedPropertyHelpers.ReplaceExtendedProperties<Microsoft.AnalysisServices.Tabular.ModelRole>(modelRole, update.ExtendedProperties, (Func<Microsoft.AnalysisServices.Tabular.ModelRole, ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>>) (obj => (ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>) obj.ExtendedProperties));
      if (num != 0 || update.ExtendedProperties.Count > 0)
        flag = true;
    }
    if (!flag)
      return OperationResult.CreateSuccess($"Role '{update.Name}' is already in the requested state", update.Name, new ObjectType?(ObjectType.SecurityRole), new Operation?(Operation.Update), false);
    TransactionOperations.RecordOperation(info, $"Updated role '{update.Name}'");
    ConnectionOperations.SaveChangesWithRollback(info, "update role");
    return OperationResult.CreateSuccess($"Role '{update.Name}' updated successfully", update.Name, new ObjectType?(ObjectType.SecurityRole), new Operation?(Operation.Update));
  }

  public static void DeleteModelRole(string? connectionName, string roleName)
  {
    if (string.IsNullOrWhiteSpace(roleName))
      throw new McpException("roleName is required");
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Database database = info.Database;
    database.Model.Roles.Remove(database.Model.Roles.Find(roleName) ?? throw new McpException($"Role '{roleName}' not found"));
    TransactionOperations.RecordOperation(info, $"Deleted role '{roleName}'");
    ConnectionOperations.SaveChangesWithRollback(info, "delete role");
  }

  public static void RenameModelRole(string? connectionName, string oldName, string newName)
  {
    if (string.IsNullOrWhiteSpace(oldName))
      throw new McpException("oldName is required");
    if (string.IsNullOrWhiteSpace(newName))
      throw new McpException("newName is required");
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Database database = info.Database;
    Microsoft.AnalysisServices.Tabular.ModelRole modelRole = database.Model.Roles.Find(oldName) ?? throw new McpException($"Model role '{oldName}' not found");
    if (database.Model.Roles.Contains(newName) && !string.Equals(oldName, newName, StringComparison.OrdinalIgnoreCase))
      throw new McpException($"Model role '{newName}' already exists");
    modelRole.RequestRename(newName);
    TransactionOperations.RecordOperation(info, $"Renamed model role from '{oldName}' to '{newName}'");
    ConnectionOperations.SaveChangesWithRollback(info, "rename model role", ConnectionOperations.CheckpointMode.AfterRequestRename);
  }

  public static TablePermissionOperationResult CreateTablePermission(
    string? connectionName,
    TablePermissionCreate def)
  {
    SecurityRoleOperations.ValidateTablePermissionDefinition((TablePermissionBase) def, true);
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Database database = info.Database;
    Microsoft.AnalysisServices.Tabular.ModelRole modelRole = database.Model.Roles.Find(def.RoleName) ?? throw new McpException($"Role '{def.RoleName}' not found");
    Microsoft.AnalysisServices.Tabular.Table table = database.Model.Tables.Find(def.TableName) ?? throw new McpException($"Table '{def.TableName}' not found");
    if (Enumerable.FirstOrDefault<Microsoft.AnalysisServices.Tabular.TablePermission>((IEnumerable<Microsoft.AnalysisServices.Tabular.TablePermission>) modelRole.TablePermissions, (tp => tp.Table == table)) != null)
      throw new McpException($"Table permission already exists for table '{def.TableName}' in role '{def.RoleName}'");
    Microsoft.AnalysisServices.Tabular.TablePermission tablePermission = new Microsoft.AnalysisServices.Tabular.TablePermission()
    {
      Table = table
    };
    if (!string.IsNullOrWhiteSpace(def.FilterExpression))
      tablePermission.FilterExpression = def.FilterExpression;
    tablePermission.MetadataPermission = string.IsNullOrWhiteSpace(def.MetadataPermission) ? MetadataPermission.Default : Enum.Parse<MetadataPermission>(def.MetadataPermission, true);
    if (def.Annotations != null)
    {
      foreach (KeyValuePair<string, string> annotation in def.Annotations)
      {
        TablePermissionAnnotationCollection annotations = tablePermission.Annotations;
        Microsoft.AnalysisServices.Tabular.Annotation metadataObject = new Microsoft.AnalysisServices.Tabular.Annotation();
        metadataObject.Name = annotation.Key;
        metadataObject.Value = annotation.Value;
        annotations.Add(metadataObject);
      }
    }
    if (def.ExtendedProperties != null)
      ExtendedPropertyHelpers.ApplyToTablePermission(tablePermission, def.ExtendedProperties);
    modelRole.TablePermissions.Add(tablePermission);
    TransactionOperations.RecordOperation(info, $"Created table permission for '{def.TableName}' in role '{def.RoleName}'");
    ConnectionOperations.SaveChangesWithRollback(info, "create table permission");
    return new TablePermissionOperationResult()
    {
      State = tablePermission.State.ToString(),
      ErrorMessage = tablePermission.ErrorMessage,
      RoleName = def.RoleName,
      TableName = def.TableName
    };
  }

  public static TablePermissionOperationResult UpdateTablePermission(
    string? connectionName,
    TablePermissionUpdate update)
  {
    SecurityRoleOperations.ValidateTablePermissionDefinition((TablePermissionBase) update, false);
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Database database = info.Database;
    Microsoft.AnalysisServices.Tabular.ModelRole modelRole = database.Model.Roles.Find(update.RoleName);
    if (modelRole == null)
      throw new McpException($"Role '{update.RoleName}' not found");
    Microsoft.AnalysisServices.Tabular.Table table = database.Model.Tables.Find(update.TableName) ?? throw new McpException($"Table '{update.TableName}' not found");
    Microsoft.AnalysisServices.Tabular.TablePermission tablePermission = Enumerable.FirstOrDefault<Microsoft.AnalysisServices.Tabular.TablePermission>((IEnumerable<Microsoft.AnalysisServices.Tabular.TablePermission>) modelRole.TablePermissions, (tp => tp.Table == table)) ?? throw new McpException($"Table permission not found for table '{update.TableName}' in role '{update.RoleName}'");
    bool flag = false;
    if (update.FilterExpression != null)
    {
      string filterExpression = string.IsNullOrEmpty(update.FilterExpression) ? (string) null : update.FilterExpression;
      if ((tablePermission.FilterExpression != filterExpression))
      {
        tablePermission.FilterExpression = filterExpression;
        flag = true;
      }
    }
    if (!string.IsNullOrWhiteSpace(update.MetadataPermission))
    {
      MetadataPermission metadataPermission;
      if (Enum.TryParse<MetadataPermission>(update.MetadataPermission, true, out metadataPermission))
      {
        if (tablePermission.MetadataPermission != metadataPermission)
        {
          tablePermission.MetadataPermission = metadataPermission;
          flag = true;
        }
      }
      else
      {
        string[] names = Enum.GetNames(typeof (MetadataPermission));
        throw new McpException($"Invalid MetadataPermission '{update.MetadataPermission}'. Valid values are: {string.Join(", ", names)}");
      }
    }
    if (update.Annotations != null && AnnotationHelpers.ReplaceAnnotations<Microsoft.AnalysisServices.Tabular.TablePermission>(tablePermission, update.Annotations, (Func<Microsoft.AnalysisServices.Tabular.TablePermission, ICollection<Microsoft.AnalysisServices.Tabular.Annotation>>) (obj => (ICollection<Microsoft.AnalysisServices.Tabular.Annotation>) obj.Annotations)))
      flag = true;
    if (update.ExtendedProperties != null)
    {
      int num = tablePermission.ExtendedProperties.Count > 0 ? 1 : 0;
      ExtendedPropertyHelpers.ReplaceExtendedProperties<Microsoft.AnalysisServices.Tabular.TablePermission>(tablePermission, update.ExtendedProperties, (Func<Microsoft.AnalysisServices.Tabular.TablePermission, ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>>) (obj => (ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>) obj.ExtendedProperties));
      if (num != 0 || update.ExtendedProperties.Count > 0)
        flag = true;
    }
    if (!flag)
      return new TablePermissionOperationResult()
      {
        State = tablePermission.State.ToString(),
        ErrorMessage = tablePermission.ErrorMessage,
        RoleName = update.RoleName,
        TableName = update.TableName,
        HasChanges = false
      };
    TransactionOperations.RecordOperation(info, $"Updated table permission for '{update.TableName}' in role '{update.RoleName}'");
    ConnectionOperations.SaveChangesWithRollback(info, "update table permission");
    return new TablePermissionOperationResult()
    {
      State = tablePermission.State.ToString(),
      ErrorMessage = tablePermission.ErrorMessage,
      RoleName = update.RoleName,
      TableName = update.TableName,
      HasChanges = true
    };
  }

  public static void DeleteTablePermission(
    string? connectionName,
    string roleName,
    string tableName)
  {
    if (string.IsNullOrWhiteSpace(roleName))
      throw new McpException("roleName is required");
    if (string.IsNullOrWhiteSpace(tableName))
      throw new McpException("tableName is required");
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Database database = info.Database;
    Microsoft.AnalysisServices.Tabular.ModelRole modelRole = database.Model.Roles.Find(roleName);
    if (modelRole == null)
      throw new McpException($"Role '{roleName}' not found");
    Microsoft.AnalysisServices.Tabular.Table table = database.Model.Tables.Find(tableName) ?? throw new McpException($"Table '{tableName}' not found");
    modelRole.TablePermissions.Remove(Enumerable.FirstOrDefault<Microsoft.AnalysisServices.Tabular.TablePermission>((IEnumerable<Microsoft.AnalysisServices.Tabular.TablePermission>) modelRole.TablePermissions, (tp => tp.Table == table)) ?? throw new McpException($"Table permission not found for table '{tableName}' in role '{roleName}'"));
    TransactionOperations.RecordOperation(info, $"Deleted table permission for '{tableName}' in role '{roleName}'");
    ConnectionOperations.SaveChangesWithRollback(info, "delete table permission");
  }

  public static List<Dictionary<string, string>> GetTablePermissions(
    string? connectionName,
    string roleName)
  {
    if (string.IsNullOrWhiteSpace(roleName))
      throw new McpException("roleName is required");
    Microsoft.AnalysisServices.Tabular.ModelRole modelRole = ConnectionOperations.Get(connectionName).Database.Model.Roles.Find(roleName);
    if (modelRole == null)
      throw new McpException($"Role '{roleName}' not found");
    List<Dictionary<string, string>> tablePermissions = new List<Dictionary<string, string>>();
    foreach (Microsoft.AnalysisServices.Tabular.TablePermission tablePermission in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.TablePermission, Microsoft.AnalysisServices.Tabular.ModelRole>) modelRole.TablePermissions)
      tablePermissions.Add(new Dictionary<string, string>()
      {
        ["TableName"] = tablePermission.Table?.Name ?? "",
        ["FilterExpression"] = tablePermission.FilterExpression ?? "",
        ["MetadataPermission"] = tablePermission.MetadataPermission.ToString()
      });
    return tablePermissions;
  }

  public static TablePermissionGet GetTablePermission(
    string? connectionName,
    string roleName,
    string tableName)
  {
    if (string.IsNullOrWhiteSpace(roleName))
      throw new McpException("roleName is required");
    if (string.IsNullOrWhiteSpace(tableName))
      throw new McpException("tableName is required");
    Microsoft.AnalysisServices.Tabular.Database database = ConnectionOperations.Get(connectionName).Database;
    Microsoft.AnalysisServices.Tabular.ModelRole modelRole = database.Model.Roles.Find(roleName);
    if (modelRole == null)
      throw new McpException($"Role '{roleName}' not found");
    Microsoft.AnalysisServices.Tabular.Table table = database.Model.Tables.Find(tableName) ?? throw new McpException($"Table '{tableName}' not found");
    Microsoft.AnalysisServices.Tabular.TablePermission tablePermission1 = Enumerable.FirstOrDefault<Microsoft.AnalysisServices.Tabular.TablePermission>((IEnumerable<Microsoft.AnalysisServices.Tabular.TablePermission>) modelRole.TablePermissions, (tp => tp.Table == table)) ?? throw new McpException($"Table permission not found for table '{tableName}' in role '{roleName}'");
    TablePermissionGet tablePermissionGet = new TablePermissionGet { RoleName = roleName };
    tablePermissionGet.TableName = tableName;
    tablePermissionGet.FilterExpression = tablePermission1.FilterExpression;
    tablePermissionGet.MetadataPermission = tablePermission1.MetadataPermission.ToString();
    tablePermissionGet.State = tablePermission1.State.ToString();
    tablePermissionGet.ErrorMessage = tablePermission1.ErrorMessage;
    tablePermissionGet.ModifiedTime = new DateTime?(tablePermission1.ModifiedTime);
    tablePermissionGet.Annotations = new List<KeyValuePair<string, string>>();
    tablePermissionGet.ExtendedProperties = new List<PowerBIModelingMCP.Library.Common.DataStructures.ExtendedProperty>();
    TablePermissionGet tablePermission2 = tablePermissionGet;
    foreach (Microsoft.AnalysisServices.Tabular.Annotation annotation in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.Annotation, Microsoft.AnalysisServices.Tabular.TablePermission>) tablePermission1.Annotations)
      tablePermission2.Annotations.Add(new KeyValuePair<string, string>(annotation.Name, annotation.Value));
    tablePermission2.ExtendedProperties = ExtendedPropertyHelpers.ExtractFromTablePermission(tablePermission1);
    return tablePermission2;
  }

  public static List<Dictionary<string, object>> GetEffectivePermissions(string? connectionName)
  {
    Microsoft.AnalysisServices.Tabular.Database database = ConnectionOperations.Get(connectionName).Database;
    List<Dictionary<string, object>> effectivePermissions = new List<Dictionary<string, object>>();
    foreach (Microsoft.AnalysisServices.Tabular.ModelRole role in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.ModelRole, Microsoft.AnalysisServices.Tabular.Model>) database.Model.Roles)
    {
      List<string> stringList1 = new List<string>();
      List<string> stringList2 = new List<string>();
      Dictionary<string, object> dictionary = new Dictionary<string, object>()
      {
        ["RoleName"] = (object) role.Name,
        ["ModelPermission"] = (object) role.ModelPermission.ToString(),
        ["MemberCount"] = (object) role.Members.Count,
        ["TablesWithRLS"] = (object) stringList1,
        ["TablesWithoutRLS"] = (object) stringList2
      };
      foreach (Microsoft.AnalysisServices.Tabular.Table table1 in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.Table, Microsoft.AnalysisServices.Tabular.Model>) database.Model.Tables)
      {
        Microsoft.AnalysisServices.Tabular.Table table = table1;
        Microsoft.AnalysisServices.Tabular.TablePermission tablePermission = Enumerable.FirstOrDefault<Microsoft.AnalysisServices.Tabular.TablePermission>((IEnumerable<Microsoft.AnalysisServices.Tabular.TablePermission>) role.TablePermissions, (tp => tp.Table == table));
        if (tablePermission != null && !string.IsNullOrWhiteSpace(tablePermission.FilterExpression))
          stringList1.Add(table.Name);
        else
          stringList2.Add(table.Name);
      }
      effectivePermissions.Add(dictionary);
    }
    return effectivePermissions;
  }

  public static void ValidateModelRoleDefinition(ModelRoleBase def, bool isCreate)
  {
    if (def == null)
      throw new McpException("ModelRole definition cannot be null");
    if (isCreate && string.IsNullOrWhiteSpace(def.Name))
      throw new McpException("Name is required");
    if (!string.IsNullOrWhiteSpace(def.ModelPermission) && !Enum.IsDefined(typeof (ModelPermission), (object) def.ModelPermission))
    {
      string[] names = Enum.GetNames(typeof (ModelPermission));
      throw new McpException($"Invalid ModelPermission '{def.ModelPermission}'. Valid values are: {string.Join(", ", names)}");
    }
    if (def.ExtendedProperties != null)
    {
      List<string> stringList = ExtendedPropertyHelpers.Validate(def.ExtendedProperties);
      if (stringList.Count > 0)
        throw new McpException("ExtendedProperties validation failed: " + string.Join(", ", (IEnumerable<string>) stringList));
    }
    AnnotationHelpers.ValidateAnnotations(def.Annotations);
  }

  public static void ValidateTablePermissionDefinition(TablePermissionBase def, bool isCreate)
  {
    if (def == null)
      throw new McpException("TablePermission definition cannot be null");
    if (string.IsNullOrWhiteSpace(def.RoleName))
      throw new McpException("RoleName is required");
    if (string.IsNullOrWhiteSpace(def.TableName))
      throw new McpException("TableName is required");
    if (!string.IsNullOrWhiteSpace(def.MetadataPermission) && !Enum.IsDefined(typeof (MetadataPermission), (object) def.MetadataPermission))
    {
      string[] names = Enum.GetNames(typeof (MetadataPermission));
      throw new McpException($"Invalid MetadataPermission '{def.MetadataPermission}'. Valid values are: {string.Join(", ", names)}");
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
