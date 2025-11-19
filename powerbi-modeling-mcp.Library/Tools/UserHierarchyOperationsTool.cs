// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Tools.UserHierarchyOperationsTool
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using Microsoft.Extensions.Logging;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using PowerBIModelingMCP.Library.Common;
using PowerBIModelingMCP.Library.Common.DataStructures;
using PowerBIModelingMCP.Library.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

#nullable enable
namespace PowerBIModelingMCP.Library.Tools;

[McpServerToolType]
public class UserHierarchyOperationsTool
{
  private readonly ILogger<UserHierarchyOperationsTool> _logger;
  public static readonly ToolMetadata toolMetadata;

  public UserHierarchyOperationsTool(ILogger<UserHierarchyOperationsTool> logger)
  {
    this._logger = logger;
  }

  [McpServerTool(Name = "user_hierarchy_operations")]
  [Description("Perform operations on semantic model user hierarchies. Supported operations: Help, List, Get, Create, Update, Delete, Rename, GetColumns, AddLevel, RemoveLevel, UpdateLevel, RenameLevel, ReorderLevels, ExportTMDL. Use the Operation parameter to specify which operation to perform. TableName is required for all operations.")]
  public UserHierarchyOperationResponse ExecuteUserHierarchyOperation(
    McpServer mcpServer,
    UserHierarchyOperationRequest request)
  {
    this._logger.LogDebug("Executing {ToolName}.{Operation}: Table={TableName}, Hierarchy={HierarchyName}, Connection={ConnectionName}", (object) nameof (UserHierarchyOperationsTool), (object) request.Operation, (object) request.TableName, (object) request.HierarchyName, (object) (request.ConnectionName ?? "(last used)"));
    try
    {
      string[] strArray1 = new string[14]
      {
        "LIST",
        "GET",
        "CREATE",
        "UPDATE",
        "DELETE",
        "RENAME",
        "GETCOLUMNS",
        "ADDLEVEL",
        "REMOVELEVEL",
        "UPDATELEVEL",
        "RENAMELEVEL",
        "REORDERLEVELS",
        "EXPORTTMDL",
        "HELP"
      };
      string[] strArray2 = new string[9]
      {
        "CREATE",
        "UPDATE",
        "DELETE",
        "RENAME",
        "ADDLEVEL",
        "REMOVELEVEL",
        "UPDATELEVEL",
        "RENAMELEVEL",
        "REORDERLEVELS"
      };
      if (!Enumerable.Contains<string>((IEnumerable<string>) strArray1, request.Operation.ToUpperInvariant()))
      {
        this._logger.LogWarning("Invalid operation '{Operation}' requested for {ToolName}. Valid operations: {ValidOperations}", (object) request.Operation, (object) nameof (UserHierarchyOperationsTool), (object) string.Join(", ", strArray1));
        return UserHierarchyOperationResponse.Forbidden(request.Operation, $"Invalid operation: {request.Operation}. Supported operations: {string.Join(", ", strArray1)}");
      }
      if (!this.ValidateRequest(request.Operation, request))
        throw new McpException($"Invalid request for {request.Operation} operation.");
      if (Enumerable.Contains<string>((IEnumerable<string>) strArray2, request.Operation.ToUpperInvariant()))
      {
        WriteOperationResult writeOperationResult = WriteGuard.ExecuteWriteOperationWithGuards(mcpServer, request.ConnectionName, request.Operation);
        if (!writeOperationResult.Success)
        {
          this._logger.LogWarning("{ToolName}.{Operation} blocked by write guard: {Reason}", (object) nameof (UserHierarchyOperationsTool), (object) request.Operation, (object) writeOperationResult.Message);
          return UserHierarchyOperationResponse.Forbidden(request.Operation, writeOperationResult.Message);
        }
      }
      bool allowed = WriteGuard.IsWriteAllowed("").allowed;
      string upperInvariant = request.Operation.ToUpperInvariant();
      UserHierarchyOperationResponse operationResponse;
      if (upperInvariant != null)
      {
        switch (upperInvariant.Length)
        {
          case 3:
            if ((upperInvariant == "GET"))
            {
              operationResponse = this.HandleGetOperation(request);
              goto label_43;
            }
            break;
          case 4:
            switch (upperInvariant[0])
            {
              case 'H':
                if ((upperInvariant == "HELP"))
                {
                  operationResponse = this.HandleHelpOperation(request, allowed ? strArray1 : Enumerable.ToArray<string>(Enumerable.Except<string>((IEnumerable<string>) strArray1, (IEnumerable<string>) strArray2)));
                  goto label_43;
                }
                break;
              case 'L':
                if ((upperInvariant == "LIST"))
                {
                  operationResponse = this.HandleListOperation(request);
                  goto label_43;
                }
                break;
            }
            break;
          case 6:
            switch (upperInvariant[0])
            {
              case 'C':
                if ((upperInvariant == "CREATE"))
                {
                  operationResponse = this.HandleCreateOperation(request);
                  goto label_43;
                }
                break;
              case 'D':
                if ((upperInvariant == "DELETE"))
                {
                  operationResponse = this.HandleDeleteOperation(request);
                  goto label_43;
                }
                break;
              case 'R':
                if ((upperInvariant == "RENAME"))
                {
                  operationResponse = this.HandleRenameOperation(request);
                  goto label_43;
                }
                break;
              case 'U':
                if ((upperInvariant == "UPDATE"))
                {
                  operationResponse = this.HandleUpdateOperation(request);
                  goto label_43;
                }
                break;
            }
            break;
          case 8:
            if ((upperInvariant == "ADDLEVEL"))
            {
              operationResponse = this.HandleAddLevelOperation(request);
              goto label_43;
            }
            break;
          case 10:
            switch (upperInvariant[0])
            {
              case 'E':
                if ((upperInvariant == "EXPORTTMDL"))
                {
                  operationResponse = this.HandleExportTMDLOperation(request);
                  goto label_43;
                }
                break;
              case 'G':
                if ((upperInvariant == "GETCOLUMNS"))
                {
                  operationResponse = this.HandleGetColumnsOperation(request);
                  goto label_43;
                }
                break;
            }
            break;
          case 11:
            switch (upperInvariant[2])
            {
              case 'D':
                if ((upperInvariant == "UPDATELEVEL"))
                {
                  operationResponse = this.HandleUpdateLevelOperation(request);
                  goto label_43;
                }
                break;
              case 'M':
                if ((upperInvariant == "REMOVELEVEL"))
                {
                  operationResponse = this.HandleRemoveLevelOperation(request);
                  goto label_43;
                }
                break;
              case 'N':
                if ((upperInvariant == "RENAMELEVEL"))
                {
                  operationResponse = this.HandleRenameLevelOperation(request);
                  goto label_43;
                }
                break;
            }
            break;
          case 13:
            if ((upperInvariant == "REORDERLEVELS"))
            {
              operationResponse = this.HandleReorderLevelsOperation(request);
              goto label_43;
            }
            break;
        }
      }
      operationResponse = UserHierarchyOperationResponse.Forbidden(request.Operation, $"Operation {request.Operation} not implemented");
label_43:
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Error executing {ToolName}.{Operation}: {ErrorMessage}", (object) nameof (UserHierarchyOperationsTool), (object) request.Operation, (object) ex.Message);
      return new UserHierarchyOperationResponse()
      {
        Success = false,
        Message = "Error executing user hierarchy operation: " + ex.Message,
        Operation = request.Operation,
        TableName = request.TableName,
        HierarchyName = request.HierarchyName
      };
    }
  }

  private UserHierarchyOperationResponse HandleListOperation(UserHierarchyOperationRequest request)
  {
    try
    {
      List<HierarchyList> hierarchyListList = UserHierarchyOperations.ListHierarchies(request.ConnectionName, request.TableName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Table={TableName}, Count={Count}", (object) nameof (UserHierarchyOperationsTool), (object) "List", (object) request.TableName, (object) hierarchyListList.Count);
      UserHierarchyOperationResponse operationResponse = new UserHierarchyOperationResponse { Success = true };
      operationResponse.Message = $"Found {hierarchyListList.Count} hierarchies in table '{request.TableName}'";
      operationResponse.Operation = "LIST";
      operationResponse.TableName = request.TableName;
      operationResponse.Data = (object) hierarchyListList;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      UserHierarchyOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new UserHierarchyOperationResponse()
      {
        Success = false,
        Message = "Failed to list hierarchies: " + ex.Message,
        Operation = "LIST",
        TableName = request.TableName,
        Help = (object) operationMetadata
      };
    }
  }

  private UserHierarchyOperationResponse HandleGetOperation(UserHierarchyOperationRequest request)
  {
    try
    {
      HierarchyGet hierarchy = UserHierarchyOperations.GetHierarchy(request.ConnectionName, request.TableName, request.HierarchyName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Table={TableName}, Hierarchy={HierarchyName}", (object) nameof (UserHierarchyOperationsTool), (object) "Get", (object) request.TableName, (object) request.HierarchyName);
      UserHierarchyOperationResponse operation = new UserHierarchyOperationResponse { Success = true };
      operation.Message = $"Retrieved hierarchy '{request.HierarchyName}' from table '{request.TableName}' successfully";
      operation.Operation = "GET";
      operation.TableName = request.TableName;
      operation.HierarchyName = request.HierarchyName;
      operation.Data = (object) hierarchy;
      return operation;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      UserHierarchyOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new UserHierarchyOperationResponse()
      {
        Success = false,
        Message = "Failed to get hierarchy: " + ex.Message,
        Operation = "GET",
        TableName = request.TableName,
        HierarchyName = request.HierarchyName,
        Help = (object) operationMetadata
      };
    }
  }

  private UserHierarchyOperationResponse HandleCreateOperation(UserHierarchyOperationRequest request)
  {
    try
    {
      if (string.IsNullOrEmpty(request.CreateDefinition.TableName))
        request.CreateDefinition.TableName = request.TableName;
      else if ((request.CreateDefinition.TableName != request.TableName))
        throw new McpException($"Table name mismatch: Request specifies '{request.TableName}' but CreateDefinition specifies '{request.CreateDefinition.TableName}'");
      HierarchyOperationResult hierarchy = UserHierarchyOperations.CreateHierarchy(request.ConnectionName, request.CreateDefinition);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Table={TableName}, Hierarchy={HierarchyName}, LevelCount={LevelCount}", (object) nameof (UserHierarchyOperationsTool), (object) "Create", (object) request.TableName, (object) request.CreateDefinition.Name, (object) hierarchy.LevelCount);
      UserHierarchyOperationResponse operation = new UserHierarchyOperationResponse { Success = true };
      operation.Message = $"Hierarchy '{request.CreateDefinition.Name}' created successfully in table '{request.TableName}' with {hierarchy.LevelCount} levels";
      operation.Operation = "CREATE";
      operation.TableName = request.TableName;
      operation.HierarchyName = request.CreateDefinition.Name;
      operation.Data = (object) hierarchy;
      return operation;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      UserHierarchyOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new UserHierarchyOperationResponse()
      {
        Success = false,
        Message = "Failed to create hierarchy: " + ex.Message,
        Operation = "CREATE",
        TableName = request.TableName,
        Help = (object) operationMetadata
      };
    }
  }

  private UserHierarchyOperationResponse HandleUpdateOperation(UserHierarchyOperationRequest request)
  {
    try
    {
      if (string.IsNullOrEmpty(request.UpdateDefinition.TableName))
        request.UpdateDefinition.TableName = request.TableName;
      else if ((request.UpdateDefinition.TableName != request.TableName))
        throw new McpException($"Table name mismatch: Request specifies '{request.TableName}' but UpdateDefinition specifies '{request.UpdateDefinition.TableName}'");
      HierarchyOperationResult hierarchyOperationResult = UserHierarchyOperations.UpdateHierarchy(request.ConnectionName, request.UpdateDefinition);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Table={TableName}, Hierarchy={HierarchyName}", (object) nameof (UserHierarchyOperationsTool), (object) "Update", (object) request.TableName, (object) request.UpdateDefinition.Name);
      UserHierarchyOperationResponse operationResponse = new UserHierarchyOperationResponse { Success = true };
      operationResponse.Message = $"Hierarchy '{request.UpdateDefinition.Name}' updated successfully in table '{request.TableName}'";
      operationResponse.Operation = "UPDATE";
      operationResponse.TableName = request.TableName;
      operationResponse.HierarchyName = request.UpdateDefinition.Name;
      operationResponse.Data = (object) hierarchyOperationResult;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      UserHierarchyOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new UserHierarchyOperationResponse()
      {
        Success = false,
        Message = "Failed to update hierarchy: " + ex.Message,
        Operation = "UPDATE",
        TableName = request.TableName,
        Help = (object) operationMetadata
      };
    }
  }

  private UserHierarchyOperationResponse HandleDeleteOperation(UserHierarchyOperationRequest request)
  {
    try
    {
      UserHierarchyOperations.DeleteHierarchy(request.ConnectionName, request.TableName, request.HierarchyName, request.ShouldCascadeDelete.Value);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Table={TableName}, Hierarchy={HierarchyName}, Cascade={Cascade}", (object) nameof (UserHierarchyOperationsTool), (object) "Delete", (object) request.TableName, (object) request.HierarchyName, (object) request.ShouldCascadeDelete);
      UserHierarchyOperationResponse operationResponse = new UserHierarchyOperationResponse { Success = true };
      operationResponse.Message = $"Hierarchy '{request.HierarchyName}' deleted successfully from table '{request.TableName}'";
      operationResponse.Operation = "DELETE";
      operationResponse.TableName = request.TableName;
      operationResponse.HierarchyName = request.HierarchyName;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      UserHierarchyOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new UserHierarchyOperationResponse()
      {
        Success = false,
        Message = "Failed to delete hierarchy: " + ex.Message,
        Operation = "DELETE",
        TableName = request.TableName,
        HierarchyName = request.HierarchyName,
        Help = (object) operationMetadata
      };
    }
  }

  private UserHierarchyOperationResponse HandleRenameOperation(UserHierarchyOperationRequest request)
  {
    try
    {
      UserHierarchyOperations.RenameHierarchy(request.ConnectionName, request.TableName, request.HierarchyName, request.NewName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Table={TableName}, From={OldName}, To={NewName}", (object) nameof (UserHierarchyOperationsTool), (object) "Rename", (object) request.TableName, (object) request.HierarchyName, (object) request.NewName);
      UserHierarchyOperationResponse operationResponse = new UserHierarchyOperationResponse { Success = true };
      operationResponse.Message = $"Hierarchy '{request.HierarchyName}' renamed to '{request.NewName}' successfully in table '{request.TableName}'";
      operationResponse.Operation = "RENAME";
      operationResponse.TableName = request.TableName;
      operationResponse.HierarchyName = request.NewName;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      UserHierarchyOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new UserHierarchyOperationResponse()
      {
        Success = false,
        Message = "Failed to rename hierarchy: " + ex.Message,
        Operation = "RENAME",
        TableName = request.TableName,
        HierarchyName = request.HierarchyName,
        Help = (object) operationMetadata
      };
    }
  }

  private UserHierarchyOperationResponse HandleGetColumnsOperation(
    UserHierarchyOperationRequest request)
  {
    try
    {
      List<string> hierarchyColumns = UserHierarchyOperations.GetHierarchyColumns(request.ConnectionName, request.TableName, request.HierarchyName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Table={TableName}, Hierarchy={HierarchyName}, Count={Count}", (object) nameof (UserHierarchyOperationsTool), (object) "GetColumns", (object) request.TableName, (object) request.HierarchyName, (object) hierarchyColumns.Count);
      UserHierarchyOperationResponse columnsOperation = new UserHierarchyOperationResponse { Success = true };
      columnsOperation.Message = $"Retrieved {hierarchyColumns.Count} columns for hierarchy '{request.HierarchyName}' in table '{request.TableName}'";
      columnsOperation.Operation = "GETCOLUMNS";
      columnsOperation.TableName = request.TableName;
      columnsOperation.HierarchyName = request.HierarchyName;
      columnsOperation.Data = (object) hierarchyColumns;
      return columnsOperation;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      UserHierarchyOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new UserHierarchyOperationResponse()
      {
        Success = false,
        Message = "Failed to get hierarchy columns: " + ex.Message,
        Operation = "GETCOLUMNS",
        TableName = request.TableName,
        HierarchyName = request.HierarchyName,
        Help = (object) operationMetadata
      };
    }
  }

  private UserHierarchyOperationResponse HandleAddLevelOperation(
    UserHierarchyOperationRequest request)
  {
    try
    {
      UserHierarchyOperations.AddLevel(request.ConnectionName, request.TableName, request.HierarchyName, request.LevelCreateDefinition);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Table={TableName}, Hierarchy={HierarchyName}, Level={LevelName}", (object) nameof (UserHierarchyOperationsTool), (object) "AddLevel", (object) request.TableName, (object) request.HierarchyName, (object) (request.LevelCreateDefinition.Name ?? request.LevelCreateDefinition.ColumnName));
      UserHierarchyOperationResponse operationResponse = new UserHierarchyOperationResponse { Success = true };
      operationResponse.Message = $"Level '{request.LevelCreateDefinition.Name ?? request.LevelCreateDefinition.ColumnName}' added successfully to hierarchy '{request.HierarchyName}' in table '{request.TableName}'";
      operationResponse.Operation = "ADDLEVEL";
      operationResponse.TableName = request.TableName;
      operationResponse.HierarchyName = request.HierarchyName;
      operationResponse.LevelName = request.LevelCreateDefinition.Name ?? request.LevelCreateDefinition.ColumnName;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      UserHierarchyOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new UserHierarchyOperationResponse()
      {
        Success = false,
        Message = "Failed to add level: " + ex.Message,
        Operation = "ADDLEVEL",
        TableName = request.TableName,
        HierarchyName = request.HierarchyName,
        Help = (object) operationMetadata
      };
    }
  }

  private UserHierarchyOperationResponse HandleRemoveLevelOperation(
    UserHierarchyOperationRequest request)
  {
    try
    {
      UserHierarchyOperations.RemoveLevel(request.ConnectionName, request.TableName, request.HierarchyName, request.LevelName, request.ShouldCascadeDelete.Value);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Table={TableName}, Hierarchy={HierarchyName}, Level={LevelName}, Cascade={Cascade}", (object) nameof (UserHierarchyOperationsTool), (object) "RemoveLevel", (object) request.TableName, (object) request.HierarchyName, (object) request.LevelName, (object) request.ShouldCascadeDelete);
      UserHierarchyOperationResponse operationResponse = new UserHierarchyOperationResponse { Success = true };
      operationResponse.Message = $"Level '{request.LevelName}' removed successfully from hierarchy '{request.HierarchyName}' in table '{request.TableName}'";
      operationResponse.Operation = "REMOVELEVEL";
      operationResponse.TableName = request.TableName;
      operationResponse.HierarchyName = request.HierarchyName;
      operationResponse.LevelName = request.LevelName;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      UserHierarchyOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new UserHierarchyOperationResponse()
      {
        Success = false,
        Message = "Failed to remove level: " + ex.Message,
        Operation = "REMOVELEVEL",
        TableName = request.TableName,
        HierarchyName = request.HierarchyName,
        LevelName = request.LevelName,
        Help = (object) operationMetadata
      };
    }
  }

  private UserHierarchyOperationResponse HandleUpdateLevelOperation(
    UserHierarchyOperationRequest request)
  {
    try
    {
      UserHierarchyOperations.UpdateLevel(request.ConnectionName, request.TableName, request.HierarchyName, request.LevelName, request.LevelUpdateDefinition);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Table={TableName}, Hierarchy={HierarchyName}, Level={LevelName}", (object) nameof (UserHierarchyOperationsTool), (object) "UpdateLevel", (object) request.TableName, (object) request.HierarchyName, (object) request.LevelName);
      UserHierarchyOperationResponse operationResponse = new UserHierarchyOperationResponse { Success = true };
      operationResponse.Message = $"Level '{request.LevelName}' updated successfully in hierarchy '{request.HierarchyName}' in table '{request.TableName}'";
      operationResponse.Operation = "UPDATELEVEL";
      operationResponse.TableName = request.TableName;
      operationResponse.HierarchyName = request.HierarchyName;
      operationResponse.LevelName = request.LevelName;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      UserHierarchyOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new UserHierarchyOperationResponse()
      {
        Success = false,
        Message = "Failed to update level: " + ex.Message,
        Operation = "UPDATELEVEL",
        TableName = request.TableName,
        HierarchyName = request.HierarchyName,
        LevelName = request.LevelName,
        Help = (object) operationMetadata
      };
    }
  }

  private UserHierarchyOperationResponse HandleRenameLevelOperation(
    UserHierarchyOperationRequest request)
  {
    try
    {
      UserHierarchyOperations.RenameLevel(request.ConnectionName, request.TableName, request.HierarchyName, request.LevelName, request.NewLevelName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Table={TableName}, Hierarchy={HierarchyName}, From={OldName}, To={NewName}", (object) nameof (UserHierarchyOperationsTool), (object) "RenameLevel", (object) request.TableName, (object) request.HierarchyName, (object) request.LevelName, (object) request.NewLevelName);
      UserHierarchyOperationResponse operationResponse = new UserHierarchyOperationResponse { Success = true };
      operationResponse.Message = $"Level '{request.LevelName}' renamed to '{request.NewLevelName}' successfully in hierarchy '{request.HierarchyName}' in table '{request.TableName}'";
      operationResponse.Operation = "RENAMELEVEL";
      operationResponse.TableName = request.TableName;
      operationResponse.HierarchyName = request.HierarchyName;
      operationResponse.LevelName = request.NewLevelName;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      UserHierarchyOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new UserHierarchyOperationResponse()
      {
        Success = false,
        Message = "Failed to rename level: " + ex.Message,
        Operation = "RENAMELEVEL",
        TableName = request.TableName,
        HierarchyName = request.HierarchyName,
        LevelName = request.LevelName,
        Help = (object) operationMetadata
      };
    }
  }

  private UserHierarchyOperationResponse HandleReorderLevelsOperation(
    UserHierarchyOperationRequest request)
  {
    try
    {
      UserHierarchyOperations.ReorderLevels(request.ConnectionName, request.TableName, request.HierarchyName, request.LevelNamesInOrder);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Table={TableName}, Hierarchy={HierarchyName}, LevelCount={LevelCount}", (object) nameof (UserHierarchyOperationsTool), (object) "ReorderLevels", (object) request.TableName, (object) request.HierarchyName, (object) request.LevelNamesInOrder.Count);
      UserHierarchyOperationResponse operationResponse = new UserHierarchyOperationResponse { Success = true };
      operationResponse.Message = $"Levels reordered successfully in hierarchy '{request.HierarchyName}' in table '{request.TableName}'. New order: {string.Join(", ", (IEnumerable<string>) request.LevelNamesInOrder)}";
      operationResponse.Operation = "REORDERLEVELS";
      operationResponse.TableName = request.TableName;
      operationResponse.HierarchyName = request.HierarchyName;
      operationResponse.Data = (object) request.LevelNamesInOrder;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      UserHierarchyOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new UserHierarchyOperationResponse()
      {
        Success = false,
        Message = "Failed to reorder levels: " + ex.Message,
        Operation = "REORDERLEVELS",
        TableName = request.TableName,
        HierarchyName = request.HierarchyName,
        Help = (object) operationMetadata
      };
    }
  }

  private UserHierarchyOperationResponse HandleExportTMDLOperation(
    UserHierarchyOperationRequest request)
  {
    try
    {
      string str = UserHierarchyOperations.ExportTMDL(request.ConnectionName, request.TableName, request.HierarchyName, (ExportTmdl) request.TmdlExportOptions);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Table={TableName}, Hierarchy={HierarchyName}", (object) nameof (UserHierarchyOperationsTool), (object) "ExportTMDL", (object) request.TableName, (object) request.HierarchyName);
      UserHierarchyOperationResponse operationResponse = new UserHierarchyOperationResponse { Success = true };
      operationResponse.Message = $"TMDL for hierarchy '{request.HierarchyName}' in table '{request.TableName}' retrieved successfully";
      operationResponse.Operation = request.Operation;
      operationResponse.TableName = request.TableName;
      operationResponse.HierarchyName = request.HierarchyName;
      operationResponse.Data = (object) str;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      UserHierarchyOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new UserHierarchyOperationResponse()
      {
        Success = false,
        Message = "Error getting hierarchy TMDL: " + ex.Message,
        Operation = request.Operation,
        TableName = request.TableName,
        HierarchyName = request.HierarchyName,
        Help = (object) operationMetadata
      };
    }
  }

  private UserHierarchyOperationResponse HandleHelpOperation(
    UserHierarchyOperationRequest request,
    string[] operations)
  {
    this._logger.LogInformation("{ToolName}.{Operation} completed: Operations={OperationCount}", (object) nameof (UserHierarchyOperationsTool), (object) "Help", (object) operations.Length);
    return new UserHierarchyOperationResponse()
    {
      Success = true,
      Message = "Help information for user hierarchy operations",
      Operation = request.Operation,
      Help = (object) new
      {
        ToolName = "user_hierarchy_operations",
        Description = "Perform operations on semantic model user hierarchies.",
        SupportedOperations = operations,
        Examples = Enumerable.Where<KeyValuePair<string, OperationMetadata>>((IEnumerable<KeyValuePair<string, OperationMetadata>>) UserHierarchyOperationsTool.toolMetadata.Operations, (Func<KeyValuePair<string, OperationMetadata>, bool>) (p => Enumerable.Contains<string>((IEnumerable<string>) operations, p.Key, (IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase))),
        Notes = new string[5]
        {
          "Use the Operation parameter to specify which operation to perform.",
          "TableName is required for all operations.",
          "For CREATE and UPDATE operations, the TableName in the definition must match the request.",
          "For ADDLEVEL and REMOVELEVEL operations, the LevelName must match the Name or ColumnName in the LevelCreateDefinition.",
          "For RENAMELEVEL and REORDERLEVELS operations, the LevelName must match an existing level in the hierarchy."
        }
      }
    };
  }

  private bool ValidateRequest(string operation, UserHierarchyOperationRequest request)
  {
    OperationMetadata operationMetadata;
    if (!UserHierarchyOperationsTool.toolMetadata.Operations.TryGetValue(operation, out operationMetadata))
      return true;
    JsonObject requestDict = JsonSerializer.SerializeToNode<UserHierarchyOperationRequest>(request) as JsonObject;
    List<string> list1 = Enumerable.ToList<string>(Enumerable.Where<string>((IEnumerable<string>) operationMetadata.RequiredParams, (p => requestDict != null && requestDict[p] == null)));
    List<string> list2 = Enumerable.ToList<string>(Enumerable.Where<string>((IEnumerable<string>) operationMetadata.ForbiddenParams, (p => requestDict != null && requestDict[p] != null)));
    if (Enumerable.Any<string>((IEnumerable<string>) list1))
      throw new McpException($"Missing required parameters needed for {operation} operation: {string.Join(", ", (IEnumerable<string>) list1)}");
    if (Enumerable.Any<string>((IEnumerable<string>) list2))
      throw new McpException($"Forbidden parameters not allowed for {operation} operation: {string.Join(", ", (IEnumerable<string>) list2)}");
    return true;
  }

  static UserHierarchyOperationsTool()
  {
    ToolMetadata toolMetadata1 = new ToolMetadata();
    ToolMetadata toolMetadata2 = toolMetadata1;
    Dictionary<string, OperationMetadata> dictionary1 = new Dictionary<string, OperationMetadata>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    Dictionary<string, OperationMetadata> dictionary2 = dictionary1;
    OperationMetadata operationMetadata1 = new OperationMetadata { RequiredParams = new string[1]
    {
      "TableName"
    } };
    operationMetadata1.Description = "List all user hierarchies in a table with their levels and metadata.\r\nMandatory properties: TableName.\r\nOptional: None.";
    OperationMetadata operationMetadata2 = operationMetadata1;
    List<string> stringList1 = new List<string>();
    stringList1.Add("{\r\n    \"request\": { \r\n    \"Operation\": \"List\",\r\n    \"ConnectionName\": \"MyConnection\",\r\n    \"TableName\": \"Sales\" \r\n    }\r\n}");
    operationMetadata2.ExampleRequests = stringList1;
    OperationMetadata operationMetadata3 = operationMetadata1;
    dictionary2["List"] = operationMetadata3;
    Dictionary<string, OperationMetadata> dictionary3 = dictionary1;
    OperationMetadata operationMetadata4 = new OperationMetadata { RequiredParams = new string[2]
    {
      "TableName",
      "HierarchyName"
    } };
    operationMetadata4.Description = "Get detailed information of a specific user hierarchy including all levels, properties, annotations, and extended properties.\r\nMandatory properties: TableName, HierarchyName.\r\nOptional: None.";
    OperationMetadata operationMetadata5 = operationMetadata4;
    List<string> stringList2 = new List<string>();
    stringList2.Add("{\r\n    \"request\": { \r\n        \"Operation\": \"Get\",\r\n        \"ConnectionName\": \"MyConnection\",\r\n        \"TableName\": \"Sales\",\r\n        \"HierarchyName\": \"Geography\"\r\n    }\r\n}");
    operationMetadata5.ExampleRequests = stringList2;
    OperationMetadata operationMetadata6 = operationMetadata4;
    dictionary3["Get"] = operationMetadata6;
    Dictionary<string, OperationMetadata> dictionary4 = dictionary1;
    OperationMetadata operationMetadata7 = new OperationMetadata { RequiredParams = new string[1]
    {
      "CreateDefinition"
    } };
    operationMetadata7.Description = "Create a new user hierarchy in a table with specified levels and properties.\r\nMandatory properties: CreateDefinition (with TableName, Name, Levels).\r\nEach level in Levels requires: Name, ColumnName.\r\nOptional: CreateDefinition properties (Description, IsHidden, DisplayFolder, HideMembers, LineageTag, SourceLineageTag, Annotations, ExtendedProperties).\r\nOptional level properties: Description, Ordinal, LineageTag, SourceLineageTag, Annotations, ExtendedProperties.\r\nNote: Either all levels must have Ordinal specified or none (mixed ordinals not allowed).";
    OperationMetadata operationMetadata8 = operationMetadata7;
    List<string> stringList3 = new List<string>();
    stringList3.Add("{\r\n    \"request\": { \r\n        \"Operation\": \"Create\",\r\n        \"ConnectionName\": \"MyConnection\",\r\n        \"CreateDefinition\": { \r\n            \"TableName\": \"Sales\", \r\n            \"Name\": \"Geography\", \r\n            \"Levels\": [\r\n                {\r\n                    \"Name\": \"Country\", \r\n                    \"ColumnName\": \"Country\",\r\n                    \"Description\": \"Country level\",\r\n                    \"Ordinal\": 1\r\n                }\r\n            ] \r\n        }\r\n    }\r\n}");
    operationMetadata8.ExampleRequests = stringList3;
    OperationMetadata operationMetadata9 = operationMetadata7;
    dictionary4["Create"] = operationMetadata9;
    Dictionary<string, OperationMetadata> dictionary5 = dictionary1;
    OperationMetadata operationMetadata10 = new OperationMetadata { RequiredParams = new string[1]
    {
      "UpdateDefinition"
    } };
    operationMetadata10.Description = "Update properties of an existing user hierarchy. Names cannot be changed through this operation - use Rename operation instead.\r\nMandatory properties: UpdateDefinition (with TableName, Name).\r\nOptional: Description, IsHidden, DisplayFolder, HideMembers, LineageTag, SourceLineageTag, Annotations, ExtendedProperties.\r\nNote: To change hierarchy name, use the Rename operation.";
    OperationMetadata operationMetadata11 = operationMetadata10;
    List<string> stringList4 = new List<string>();
    stringList4.Add("{\r\n    \"request\": { \r\n        \"Operation\": \"Update\",\r\n        \"ConnectionName\": \"MyConnection\",\r\n        \"UpdateDefinition\": { \r\n            \"TableName\": \"Sales\", \r\n            \"Name\": \"Geography\",\r\n            \"HideMembers\": \"HideBlankMembers\",\r\n            \"Levels\": [\r\n                {\r\n                    \"Name\": \"Country\", \r\n                    \"ColumnName\": \"Country\",\r\n                    \"Description\": \"Country level\",\r\n                    \"Ordinal\": 1\r\n                }\r\n            ] \r\n        }\r\n    }\r\n}");
    operationMetadata11.ExampleRequests = stringList4;
    OperationMetadata operationMetadata12 = operationMetadata10;
    dictionary5["Update"] = operationMetadata12;
    Dictionary<string, OperationMetadata> dictionary6 = dictionary1;
    OperationMetadata operationMetadata13 = new OperationMetadata { RequiredParams = new string[3]
    {
      "TableName",
      "HierarchyName",
      "ShouldCascadeDelete"
    } };
    operationMetadata13.Description = "Delete a user hierarchy from a table. Can optionally cascade delete dependent objects.\r\nMandatory properties: TableName, HierarchyName, ShouldCascadeDelete.\r\nOptional: None.";
    OperationMetadata operationMetadata14 = operationMetadata13;
    List<string> stringList5 = new List<string>();
    stringList5.Add("{\r\n    \"request\": { \r\n        \"Operation\": \"Delete\",\r\n        \"ConnectionName\": \"MyConnection\",\r\n        \"TableName\": \"Sales\",\r\n        \"HierarchyName\": \"Geography\",\r\n        \"ShouldCascadeDelete\": true\r\n    }\r\n}");
    operationMetadata14.ExampleRequests = stringList5;
    OperationMetadata operationMetadata15 = operationMetadata13;
    dictionary6["Delete"] = operationMetadata15;
    Dictionary<string, OperationMetadata> dictionary7 = dictionary1;
    OperationMetadata operationMetadata16 = new OperationMetadata { RequiredParams = new string[3]
    {
      "TableName",
      "HierarchyName",
      "NewName"
    } };
    operationMetadata16.Description = "Rename a user hierarchy to a new name.\r\nMandatory properties: TableName, HierarchyName, NewName.\r\nOptional: None.";
    OperationMetadata operationMetadata17 = operationMetadata16;
    List<string> stringList6 = new List<string>();
    stringList6.Add("{\r\n    \"request\": { \r\n        \"Operation\": \"Rename\",\r\n        \"ConnectionName\": \"MyConnection\",\r\n        \"TableName\": \"Sales\",\r\n        \"HierarchyName\": \"Geography\",\r\n        \"NewName\": \"Location\"\r\n    }\r\n}");
    operationMetadata17.ExampleRequests = stringList6;
    OperationMetadata operationMetadata18 = operationMetadata16;
    dictionary7["Rename"] = operationMetadata18;
    Dictionary<string, OperationMetadata> dictionary8 = dictionary1;
    OperationMetadata operationMetadata19 = new OperationMetadata { RequiredParams = new string[2]
    {
      "TableName",
      "HierarchyName"
    } };
    operationMetadata19.Description = "Get all columns referenced by levels in a user hierarchy.\r\nMandatory properties: TableName, HierarchyName.\r\nOptional: None.";
    OperationMetadata operationMetadata20 = operationMetadata19;
    List<string> stringList7 = new List<string>();
    stringList7.Add("{\r\n    \"request\": { \r\n        \"Operation\": \"GetColumns\",\r\n        \"ConnectionName\": \"MyConnection\",\r\n        \"TableName\": \"Sales\",\r\n        \"HierarchyName\": \"Geography\"\r\n    }\r\n}");
    operationMetadata20.ExampleRequests = stringList7;
    OperationMetadata operationMetadata21 = operationMetadata19;
    dictionary8["GetColumns"] = operationMetadata21;
    Dictionary<string, OperationMetadata> dictionary9 = dictionary1;
    OperationMetadata operationMetadata22 = new OperationMetadata { RequiredParams = new string[3]
    {
      "TableName",
      "HierarchyName",
      "LevelCreateDefinition"
    } };
    operationMetadata22.Description = "Add a new level to an existing user hierarchy.\r\nMandatory properties: TableName, HierarchyName, LevelCreateDefinition (with Name, ColumnName).\r\nOptional: LevelCreateDefinition properties (Description, Ordinal, LineageTag, SourceLineageTag, Annotations, ExtendedProperties).";
    OperationMetadata operationMetadata23 = operationMetadata22;
    List<string> stringList8 = new List<string>();
    stringList8.Add("{\r\n    \"request\": { \r\n        \"Operation\": \"AddLevel\",\r\n        \"ConnectionName\": \"MyConnection\",\r\n        \"TableName\": \"Sales\",\r\n        \"HierarchyName\": \"Geography\",\r\n        \"LevelCreateDefinition\": { \r\n            \"Name\": \"Country\", \r\n            \"ColumnName\": \"Country\",\r\n            \"Description\": \"Country level\",\r\n            \"Ordinal\": 1\r\n        }\r\n    }\r\n}");
    operationMetadata23.ExampleRequests = stringList8;
    OperationMetadata operationMetadata24 = operationMetadata22;
    dictionary9["AddLevel"] = operationMetadata24;
    Dictionary<string, OperationMetadata> dictionary10 = dictionary1;
    OperationMetadata operationMetadata25 = new OperationMetadata { RequiredParams = new string[4]
    {
      "TableName",
      "HierarchyName",
      "LevelName",
      "ShouldCascadeDelete"
    } };
    operationMetadata25.Description = "Remove a level from a user hierarchy. Cannot remove the last level (delete hierarchy instead).\r\nMandatory properties: TableName, HierarchyName, LevelName, ShouldCascadeDelete.\r\nOptional: None.";
    OperationMetadata operationMetadata26 = operationMetadata25;
    List<string> stringList9 = new List<string>();
    stringList9.Add("{\r\n    \"request\": { \r\n        \"Operation\": \"RemoveLevel\",\r\n        \"ConnectionName\": \"MyConnection\",\r\n        \"TableName\": \"Sales\",\r\n        \"HierarchyName\": \"Geography\",\r\n        \"LevelName\": \"Country\",\r\n        \"ShouldCascadeDelete\": true\r\n    }\r\n}");
    operationMetadata26.ExampleRequests = stringList9;
    OperationMetadata operationMetadata27 = operationMetadata25;
    dictionary10["RemoveLevel"] = operationMetadata27;
    Dictionary<string, OperationMetadata> dictionary11 = dictionary1;
    OperationMetadata operationMetadata28 = new OperationMetadata { RequiredParams = new string[4]
    {
      "TableName",
      "HierarchyName",
      "LevelName",
      "LevelUpdateDefinition"
    } };
    operationMetadata28.Description = "Update properties of an existing level in a user hierarchy. Level names cannot be changed through this operation - use RenameLevel operation instead.\r\nMandatory properties: TableName, HierarchyName, LevelName, LevelUpdateDefinition.\r\nOptional: LevelUpdateDefinition properties (Description, Ordinal, ColumnName, LineageTag, SourceLineageTag, Annotations, ExtendedProperties).\r\nNote: To change level name, use the RenameLevel operation.";
    OperationMetadata operationMetadata29 = operationMetadata28;
    List<string> stringList10 = new List<string>();
    stringList10.Add("{\r\n    \"request\": { \r\n        \"Operation\": \"UpdateLevel\",\r\n        \"ConnectionName\": \"MyConnection\",\r\n        \"TableName\": \"Sales\",\r\n        \"HierarchyName\": \"Geography\",\r\n        \"LevelName\": \"Country\",\r\n        \"LevelUpdateDefinition\": { \r\n            \"Name\": \"Country\", \r\n            \"ColumnName\": \"Country\",\r\n            \"Ordinal\": 1 \r\n        }\r\n    }\r\n}");
    operationMetadata29.ExampleRequests = stringList10;
    OperationMetadata operationMetadata30 = operationMetadata28;
    dictionary11["UpdateLevel"] = operationMetadata30;
    Dictionary<string, OperationMetadata> dictionary12 = dictionary1;
    OperationMetadata operationMetadata31 = new OperationMetadata { RequiredParams = new string[4]
    {
      "TableName",
      "HierarchyName",
      "LevelName",
      "NewLevelName"
    } };
    operationMetadata31.Description = "Rename a level in a user hierarchy to a new name.\r\nMandatory properties: TableName, HierarchyName, LevelName, NewLevelName.\r\nOptional: None.";
    OperationMetadata operationMetadata32 = operationMetadata31;
    List<string> stringList11 = new List<string>();
    stringList11.Add("{\r\n    \"request\": { \r\n        \"Operation\": \"RenameLevel\",\r\n        \"ConnectionName\": \"MyConnection\",\r\n        \"TableName\": \"Sales\",\r\n        \"HierarchyName\": \"Geography\",\r\n        \"LevelName\": \"Country\",\r\n        \"NewLevelName\": \"Nation\"\r\n    }\r\n}");
    operationMetadata32.ExampleRequests = stringList11;
    OperationMetadata operationMetadata33 = operationMetadata31;
    dictionary12["RenameLevel"] = operationMetadata33;
    Dictionary<string, OperationMetadata> dictionary13 = dictionary1;
    OperationMetadata operationMetadata34 = new OperationMetadata { RequiredParams = new string[3]
    {
      "TableName",
      "HierarchyName",
      "LevelNamesInOrder"
    } };
    operationMetadata34.Description = "Reorder levels in a user hierarchy by specifying the complete ordered list of level names.\r\nMandatory properties: TableName, HierarchyName, LevelNamesInOrder.\r\nOptional: None.";
    OperationMetadata operationMetadata35 = operationMetadata34;
    List<string> stringList12 = new List<string>();
    stringList12.Add("{\r\n    \"request\": { \r\n        \"Operation\": \"ReorderLevels\",\r\n        \"ConnectionName\": \"MyConnection\",\r\n        \"TableName\": \"Sales\",\r\n        \"HierarchyName\": \"Geography\",\r\n        \"LevelNamesInOrder\": [\"Country\", \"State\", \"City\"]\r\n    }\r\n}");
    operationMetadata35.ExampleRequests = stringList12;
    OperationMetadata operationMetadata36 = operationMetadata34;
    dictionary13["ReorderLevels"] = operationMetadata36;
    Dictionary<string, OperationMetadata> dictionary14 = dictionary1;
    OperationMetadata operationMetadata37 = new OperationMetadata { RequiredParams = new string[2]
    {
      "TableName",
      "HierarchyName"
    } };
    operationMetadata37.Description = "Export a user hierarchy definition to TMDL format.\r\nMandatory properties: TableName, HierarchyName.\r\nOptional: TmdlExportOptions.";
    OperationMetadata operationMetadata38 = operationMetadata37;
    List<string> stringList13 = new List<string>();
    stringList13.Add("{\r\n    \"request\": { \r\n        \"Operation\": \"ExportTMDL\",\r\n        \"ConnectionName\": \"MyConnection\",\r\n        \"TableName\": \"Sales\",\r\n        \"HierarchyName\": \"Geography\"\r\n    }\r\n}");
    operationMetadata38.ExampleRequests = stringList13;
    OperationMetadata operationMetadata39 = operationMetadata37;
    dictionary14["ExportTMDL"] = operationMetadata39;
    Dictionary<string, OperationMetadata> dictionary15 = dictionary1;
    OperationMetadata operationMetadata40 = new OperationMetadata { Description = "Describe the user hierarchy operations tool and list all available operations with their requirements.\r\nMandatory properties: None.\r\nOptional: None." };
    List<string> stringList14 = new List<string>();
    stringList14.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Help\"\r\n    }\r\n}");
    operationMetadata40.ExampleRequests = stringList14;
    dictionary15["Help"] = operationMetadata40;
    Dictionary<string, OperationMetadata> dictionary16 = dictionary1;
    toolMetadata2.Operations = dictionary16;
    UserHierarchyOperationsTool.toolMetadata = toolMetadata1;
  }
}
