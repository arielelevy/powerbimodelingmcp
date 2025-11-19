// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Tools.SecurityRoleOperationsTool
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
public class SecurityRoleOperationsTool
{
  private readonly ILogger<SecurityRoleOperationsTool> _logger;
  public static readonly ToolMetadata toolMetadata;

  public SecurityRoleOperationsTool(ILogger<SecurityRoleOperationsTool> logger)
  {
    this._logger = logger;
  }

  [McpServerTool(Name = "security_role_operations")]
  [Description("Execute security role operations for semantic model roles and table permissions. Supports operations: Help, Create, Update, Delete, Get, List, Rename, CreatePermission, UpdatePermission, DeletePermission, GetPermission, ListPermissions, GetEffectivePermissions, ExportTMDL (YAML-like format), ExportTMSL (JSON script format).")]
  public SecurityRoleOperationResponse ExecuteSecurityRoleOperation(
    McpServer mcpServer,
    SecurityRoleOperationRequest request)
  {
    this._logger.LogDebug("Executing {ToolName}.{Operation}: RoleName={RoleName}, TableName={TableName}, Connection={ConnectionName}", (object) nameof (SecurityRoleOperationsTool), (object) request.Operation, (object) request.RoleName, (object) request.TableName, (object) (request.ConnectionName ?? "(last used)"));
    try
    {
      string[] strArray1 = new string[15]
      {
        "CREATE",
        "UPDATE",
        "DELETE",
        "GET",
        "LIST",
        "RENAME",
        "CREATEPERMISSION",
        "UPDATEPERMISSION",
        "DELETEPERMISSION",
        "GETPERMISSION",
        "LISTPERMISSIONS",
        "GETEFFECTIVEPERMISSIONS",
        "EXPORTTMDL",
        "EXPORTTMSL",
        "HELP"
      };
      string[] strArray2 = new string[7]
      {
        "CREATE",
        "UPDATE",
        "DELETE",
        "RENAME",
        "CREATEPERMISSION",
        "UPDATEPERMISSION",
        "DELETEPERMISSION"
      };
      string upperInvariant = request.Operation.ToUpperInvariant();
      if (!Enumerable.Contains<string>((IEnumerable<string>) strArray1, upperInvariant))
      {
        this._logger.LogWarning("Invalid operation '{Operation}' requested for {ToolName}. Valid operations: {ValidOperations}", (object) request.Operation, (object) nameof (SecurityRoleOperationsTool), (object) string.Join(", ", strArray1));
        return SecurityRoleOperationResponse.Forbidden(request.Operation, $"Invalid operation: {request.Operation}. Supported operations: {string.Join(", ", strArray1)}");
      }
      if (!this.ValidateRequest(request.Operation, request))
        throw new McpException($"Invalid request for {request.Operation} operation.");
      if (Enumerable.Contains<string>((IEnumerable<string>) strArray2, upperInvariant))
      {
        WriteOperationResult writeOperationResult = WriteGuard.ExecuteWriteOperationWithGuards(mcpServer, request.ConnectionName, request.Operation);
        if (!writeOperationResult.Success)
        {
          this._logger.LogWarning("{ToolName}.{Operation} blocked by write guard: {Reason}", (object) nameof (SecurityRoleOperationsTool), (object) request.Operation, (object) writeOperationResult.Message);
          return SecurityRoleOperationResponse.Forbidden(request.Operation, writeOperationResult.Message);
        }
      }
      bool allowed = WriteGuard.IsWriteAllowed("").allowed;
      SecurityRoleOperationResponse operationResponse;
      if (upperInvariant != null)
      {
        switch (upperInvariant.Length)
        {
          case 3:
            if ((upperInvariant == "GET"))
            {
              operationResponse = this.HandleGetRoleOperation(request);
              goto label_45;
            }
            break;
          case 4:
            switch (upperInvariant[0])
            {
              case 'H':
                if ((upperInvariant == "HELP"))
                {
                  operationResponse = this.HandleHelpOperation(request, allowed ? strArray1 : Enumerable.ToArray<string>(Enumerable.Except<string>((IEnumerable<string>) strArray1, (IEnumerable<string>) strArray2)));
                  goto label_45;
                }
                break;
              case 'L':
                if ((upperInvariant == "LIST"))
                {
                  operationResponse = this.HandleListRolesOperation(request);
                  goto label_45;
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
                  operationResponse = this.HandleCreateRoleOperation(request);
                  goto label_45;
                }
                break;
              case 'D':
                if ((upperInvariant == "DELETE"))
                {
                  operationResponse = this.HandleDeleteRoleOperation(request);
                  goto label_45;
                }
                break;
              case 'R':
                if ((upperInvariant == "RENAME"))
                {
                  operationResponse = this.HandleRenameRoleOperation(request);
                  goto label_45;
                }
                break;
              case 'U':
                if ((upperInvariant == "UPDATE"))
                {
                  operationResponse = this.HandleUpdateRoleOperation(request);
                  goto label_45;
                }
                break;
            }
            break;
          case 10:
            switch (upperInvariant[8])
            {
              case 'D':
                if ((upperInvariant == "EXPORTTMDL"))
                {
                  operationResponse = this.HandleExportTMDLOperation(request);
                  goto label_45;
                }
                break;
              case 'S':
                if ((upperInvariant == "EXPORTTMSL"))
                {
                  operationResponse = this.HandleExportTMSLOperation(request);
                  goto label_45;
                }
                break;
            }
            break;
          case 13:
            if ((upperInvariant == "GETPERMISSION"))
            {
              operationResponse = this.HandleGetTablePermissionOperation(request);
              goto label_45;
            }
            break;
          case 15:
            if ((upperInvariant == "LISTPERMISSIONS"))
            {
              operationResponse = this.HandleGetTablePermissionsOperation(request);
              goto label_45;
            }
            break;
          case 16 /*0x10*/:
            switch (upperInvariant[0])
            {
              case 'C':
                if ((upperInvariant == "CREATEPERMISSION"))
                {
                  operationResponse = this.HandleCreateTablePermissionOperation(request);
                  goto label_45;
                }
                break;
              case 'D':
                if ((upperInvariant == "DELETEPERMISSION"))
                {
                  operationResponse = this.HandleDeleteTablePermissionOperation(request);
                  goto label_45;
                }
                break;
              case 'U':
                if ((upperInvariant == "UPDATEPERMISSION"))
                {
                  operationResponse = this.HandleUpdateTablePermissionOperation(request);
                  goto label_45;
                }
                break;
            }
            break;
          case 23:
            if ((upperInvariant == "GETEFFECTIVEPERMISSIONS"))
            {
              operationResponse = this.HandleGetEffectivePermissionsOperation(request);
              goto label_45;
            }
            break;
        }
      }
      operationResponse = SecurityRoleOperationResponse.Forbidden(request.Operation, $"Operation {request.Operation} is not implemented");
label_45:
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Error executing {ToolName}.{Operation}: {ErrorMessage}", (object) nameof (SecurityRoleOperationsTool), (object) request.Operation, (object) ex.Message);
      return new SecurityRoleOperationResponse()
      {
        Success = false,
        Message = "Error executing security operation: " + ex.Message,
        Operation = request.Operation
      };
    }
  }

  private SecurityRoleOperationResponse HandleCreateRoleOperation(
    SecurityRoleOperationRequest request)
  {
    try
    {
      SecurityRoleOperations.CreateModelRole(request.ConnectionName, request.CreateRoleDefinition);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Role={RoleName}", (object) nameof (SecurityRoleOperationsTool), (object) request.Operation, (object) request.CreateRoleDefinition.Name);
      return new SecurityRoleOperationResponse()
      {
        Success = true,
        Message = $"Role '{request.CreateRoleDefinition.Name}' created successfully",
        Operation = request.Operation
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      SecurityRoleOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new SecurityRoleOperationResponse()
      {
        Success = false,
        Message = "Error creating role: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private SecurityRoleOperationResponse HandleUpdateRoleOperation(
    SecurityRoleOperationRequest request)
  {
    try
    {
      SecurityRoleOperations.UpdateModelRole(request.ConnectionName, request.UpdateRoleDefinition);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Role={RoleName}", (object) nameof (SecurityRoleOperationsTool), (object) request.Operation, (object) request.UpdateRoleDefinition.Name);
      return new SecurityRoleOperationResponse()
      {
        Success = true,
        Message = $"Role '{request.UpdateRoleDefinition.Name}' updated successfully",
        Operation = request.Operation
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      SecurityRoleOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new SecurityRoleOperationResponse()
      {
        Success = false,
        Message = "Error updating role: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private SecurityRoleOperationResponse HandleDeleteRoleOperation(
    SecurityRoleOperationRequest request)
  {
    try
    {
      SecurityRoleOperations.DeleteModelRole(request.ConnectionName, request.RoleName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Role={RoleName}", (object) nameof (SecurityRoleOperationsTool), (object) request.Operation, (object) request.RoleName);
      return new SecurityRoleOperationResponse()
      {
        Success = true,
        Message = $"Role '{request.RoleName}' deleted successfully",
        Operation = request.Operation
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      SecurityRoleOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new SecurityRoleOperationResponse()
      {
        Success = false,
        Message = "Error deleting role: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private SecurityRoleOperationResponse HandleGetRoleOperation(SecurityRoleOperationRequest request)
  {
    try
    {
      ModelRoleGet modelRole = SecurityRoleOperations.GetModelRole(request.ConnectionName, request.RoleName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Role={RoleName}", (object) nameof (SecurityRoleOperationsTool), (object) request.Operation, (object) request.RoleName);
      return new SecurityRoleOperationResponse()
      {
        Success = true,
        Message = $"Role '{request.RoleName}' retrieved successfully",
        Operation = request.Operation,
        Data = (object) modelRole
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      SecurityRoleOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new SecurityRoleOperationResponse()
      {
        Success = false,
        Message = "Error getting role: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private SecurityRoleOperationResponse HandleListRolesOperation(
    SecurityRoleOperationRequest request)
  {
    try
    {
      List<ModelRoleList> modelRoleListList = SecurityRoleOperations.ListModelRoles(request.ConnectionName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Count={Count}", (object) nameof (SecurityRoleOperationsTool), (object) request.Operation, (object) modelRoleListList.Count);
      SecurityRoleOperationResponse operationResponse = new SecurityRoleOperationResponse { Success = true };
      operationResponse.Message = $"Found {modelRoleListList.Count} roles";
      operationResponse.Operation = request.Operation;
      operationResponse.Data = (object) modelRoleListList;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      SecurityRoleOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new SecurityRoleOperationResponse()
      {
        Success = false,
        Message = "Error listing roles: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private SecurityRoleOperationResponse HandleRenameRoleOperation(
    SecurityRoleOperationRequest request)
  {
    try
    {
      SecurityRoleOperations.RenameModelRole(request.ConnectionName, request.RoleName, request.NewRoleName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: From={OldName}, To={NewName}", (object) nameof (SecurityRoleOperationsTool), (object) request.Operation, (object) request.RoleName, (object) request.NewRoleName);
      SecurityRoleOperationResponse operationResponse = new SecurityRoleOperationResponse { Success = true };
      operationResponse.Message = $"Role renamed from '{request.RoleName}' to '{request.NewRoleName}' successfully";
      operationResponse.Operation = request.Operation;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      SecurityRoleOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new SecurityRoleOperationResponse()
      {
        Success = false,
        Message = "Error renaming role: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private SecurityRoleOperationResponse HandleExportTMDLOperation(
    SecurityRoleOperationRequest request)
  {
    try
    {
      TmdlExportResult tmdlExportResult = SecurityRoleOperations.ExportTMDL(request.ConnectionName, request.RoleName, request.TmdlExportOptions);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Role={RoleName}", (object) nameof (SecurityRoleOperationsTool), (object) request.Operation, (object) request.RoleName);
      return new SecurityRoleOperationResponse()
      {
        Success = true,
        Message = "Role TMDL exported successfully",
        Operation = request.Operation,
        Data = (object) tmdlExportResult
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      SecurityRoleOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new SecurityRoleOperationResponse()
      {
        Success = false,
        Message = "Failed to export role TMDL: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private SecurityRoleOperationResponse HandleExportTMSLOperation(
    SecurityRoleOperationRequest request)
  {
    try
    {
      TmslExportResult tmslExportResult = SecurityRoleOperations.ExportTMSL(request.ConnectionName, request.RoleName, request.TmslExportOptions);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Role={RoleName}, OperationType={OperationType}", (object) nameof (SecurityRoleOperationsTool), (object) request.Operation, (object) request.RoleName, (object) tmslExportResult.OperationType);
      SecurityRoleOperationResponse operationResponse = new SecurityRoleOperationResponse { Success = true };
      operationResponse.Message = $"Role TMSL exported successfully. Operation: {tmslExportResult.OperationType}, Generated at: {tmslExportResult.GeneratedAt}";
      operationResponse.Operation = request.Operation;
      operationResponse.Data = (object) tmslExportResult;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      SecurityRoleOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new SecurityRoleOperationResponse()
      {
        Success = false,
        Message = "Failed to export role TMSL: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private SecurityRoleOperationResponse HandleCreateTablePermissionOperation(
    SecurityRoleOperationRequest request)
  {
    try
    {
      TablePermissionOperationResult tablePermission = SecurityRoleOperations.CreateTablePermission(request.ConnectionName, request.CreateTablePermissionDefinition);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Role={RoleName}, Table={TableName}", (object) nameof (SecurityRoleOperationsTool), (object) request.Operation, (object) request.CreateTablePermissionDefinition.RoleName, (object) request.CreateTablePermissionDefinition.TableName);
      SecurityRoleOperationResponse permissionOperation = new SecurityRoleOperationResponse { Success = true };
      permissionOperation.Message = $"Table permission created for table '{request.CreateTablePermissionDefinition.TableName}' in role '{request.CreateTablePermissionDefinition.RoleName}' successfully";
      permissionOperation.Operation = request.Operation;
      permissionOperation.Data = (object) tablePermission;
      return permissionOperation;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      SecurityRoleOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new SecurityRoleOperationResponse()
      {
        Success = false,
        Message = "Error creating table permission: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private SecurityRoleOperationResponse HandleUpdateTablePermissionOperation(
    SecurityRoleOperationRequest request)
  {
    try
    {
      TablePermissionOperationResult permissionOperationResult = SecurityRoleOperations.UpdateTablePermission(request.ConnectionName, request.UpdateTablePermissionDefinition);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Role={RoleName}, Table={TableName}", (object) nameof (SecurityRoleOperationsTool), (object) request.Operation, (object) request.UpdateTablePermissionDefinition.RoleName, (object) request.UpdateTablePermissionDefinition.TableName);
      SecurityRoleOperationResponse operationResponse = new SecurityRoleOperationResponse { Success = true };
      operationResponse.Message = $"Table permission updated for table '{request.UpdateTablePermissionDefinition.TableName}' in role '{request.UpdateTablePermissionDefinition.RoleName}' successfully";
      operationResponse.Operation = request.Operation;
      operationResponse.Data = (object) permissionOperationResult;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      SecurityRoleOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new SecurityRoleOperationResponse()
      {
        Success = false,
        Message = "Error updating table permission: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private SecurityRoleOperationResponse HandleDeleteTablePermissionOperation(
    SecurityRoleOperationRequest request)
  {
    try
    {
      SecurityRoleOperations.DeleteTablePermission(request.ConnectionName, request.RoleName, request.TableName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Role={RoleName}, Table={TableName}", (object) nameof (SecurityRoleOperationsTool), (object) request.Operation, (object) request.RoleName, (object) request.TableName);
      SecurityRoleOperationResponse operationResponse = new SecurityRoleOperationResponse { Success = true };
      operationResponse.Message = $"Table permission deleted for table '{request.TableName}' in role '{request.RoleName}' successfully";
      operationResponse.Operation = request.Operation;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      SecurityRoleOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new SecurityRoleOperationResponse()
      {
        Success = false,
        Message = "Error deleting table permission: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private SecurityRoleOperationResponse HandleGetTablePermissionOperation(
    SecurityRoleOperationRequest request)
  {
    try
    {
      TablePermissionGet tablePermission = SecurityRoleOperations.GetTablePermission(request.ConnectionName, request.RoleName, request.TableName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Role={RoleName}, Table={TableName}", (object) nameof (SecurityRoleOperationsTool), (object) request.Operation, (object) request.RoleName, (object) request.TableName);
      SecurityRoleOperationResponse permissionOperation = new SecurityRoleOperationResponse { Success = true };
      permissionOperation.Message = $"Table permission retrieved for table '{request.TableName}' in role '{request.RoleName}' successfully";
      permissionOperation.Operation = request.Operation;
      permissionOperation.Data = (object) tablePermission;
      return permissionOperation;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      SecurityRoleOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new SecurityRoleOperationResponse()
      {
        Success = false,
        Message = "Error getting table permission: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private SecurityRoleOperationResponse HandleGetTablePermissionsOperation(
    SecurityRoleOperationRequest request)
  {
    try
    {
      List<Dictionary<string, string>> tablePermissions = SecurityRoleOperations.GetTablePermissions(request.ConnectionName, request.RoleName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Role={RoleName}, Count={Count}", (object) nameof (SecurityRoleOperationsTool), (object) request.Operation, (object) request.RoleName, (object) tablePermissions.Count);
      SecurityRoleOperationResponse permissionsOperation = new SecurityRoleOperationResponse { Success = true };
      permissionsOperation.Message = $"Found {tablePermissions.Count} table permissions for role '{request.RoleName}'";
      permissionsOperation.Operation = request.Operation;
      permissionsOperation.Data = (object) tablePermissions;
      return permissionsOperation;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      SecurityRoleOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new SecurityRoleOperationResponse()
      {
        Success = false,
        Message = "Error getting table permissions: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private SecurityRoleOperationResponse HandleGetEffectivePermissionsOperation(
    SecurityRoleOperationRequest request)
  {
    try
    {
      List<Dictionary<string, object>> effectivePermissions = SecurityRoleOperations.GetEffectivePermissions(request.ConnectionName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Count={Count}", (object) nameof (SecurityRoleOperationsTool), (object) request.Operation, (object) effectivePermissions.Count);
      SecurityRoleOperationResponse permissionsOperation = new SecurityRoleOperationResponse { Success = true };
      permissionsOperation.Message = $"Found effective permissions for {effectivePermissions.Count} roles";
      permissionsOperation.Operation = request.Operation;
      permissionsOperation.Data = (object) effectivePermissions;
      return permissionsOperation;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      SecurityRoleOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new SecurityRoleOperationResponse()
      {
        Success = false,
        Message = "Error getting effective permissions: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private SecurityRoleOperationResponse HandleHelpOperation(
    SecurityRoleOperationRequest request,
    string[] operations)
  {
    this._logger.LogInformation("{ToolName}.{Operation} completed: Operations={OperationCount}", (object) nameof (SecurityRoleOperationsTool), (object) request.Operation, (object) operations.Length);
    return new SecurityRoleOperationResponse()
    {
      Success = true,
      Message = "Help information for security operations",
      Operation = request.Operation,
      Help = (object) new
      {
        ToolName = "security_operations",
        Description = "Execute security operations for semantic model roles and table permissions.",
        SupportedOperations = operations,
        Examples = Enumerable.Where<KeyValuePair<string, OperationMetadata>>((IEnumerable<KeyValuePair<string, OperationMetadata>>) SecurityRoleOperationsTool.toolMetadata.Operations, (Func<KeyValuePair<string, OperationMetadata>, bool>) (p => Enumerable.Contains<string>((IEnumerable<string>) operations, p.Key, (IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase))),
        Notes = new string[3]
        {
          "All operations require a 'ConnectionName' parameter to specify the model connection.",
          "Operations that modify roles or table permissions require write permissions.",
          "Operations that return role or table permission details require read permissions."
        }
      }
    };
  }

  private bool ValidateRequest(string operation, SecurityRoleOperationRequest request)
  {
    OperationMetadata operationMetadata;
    if (!SecurityRoleOperationsTool.toolMetadata.Operations.TryGetValue(operation, out operationMetadata))
      return true;
    JsonObject requestDict = JsonSerializer.SerializeToNode<SecurityRoleOperationRequest>(request) as JsonObject;
    List<string> list1 = Enumerable.ToList<string>(Enumerable.Where<string>((IEnumerable<string>) operationMetadata.RequiredParams, (p => requestDict != null && requestDict[p] == null)));
    List<string> list2 = Enumerable.ToList<string>(Enumerable.Where<string>((IEnumerable<string>) operationMetadata.ForbiddenParams, (p => requestDict != null && requestDict[p] != null)));
    if (Enumerable.Any<string>((IEnumerable<string>) list1))
      throw new McpException($"Missing required parameters needed for {operation} operation: {string.Join(", ", (IEnumerable<string>) list1)}");
    if (Enumerable.Any<string>((IEnumerable<string>) list2))
      throw new McpException($"Forbidden parameters not allowed for {operation} operation: {string.Join(", ", (IEnumerable<string>) list2)}");
    return true;
  }

  static SecurityRoleOperationsTool()
  {
    ToolMetadata toolMetadata1 = new ToolMetadata();
    ToolMetadata toolMetadata2 = toolMetadata1;
    Dictionary<string, OperationMetadata> dictionary1 = new Dictionary<string, OperationMetadata>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    Dictionary<string, OperationMetadata> dictionary2 = dictionary1;
    OperationMetadata operationMetadata1 = new OperationMetadata { RequiredParams = new string[1]
    {
      "CreateRoleDefinition"
    } };
    operationMetadata1.Description = "Create a new model role. \r\nMandatory properties: CreateRoleDefinition (with Name). \r\nOptional: Description, ModelPermission, Annotations, ExtendedProperties.";
    OperationMetadata operationMetadata2 = operationMetadata1;
    List<string> stringList1 = new List<string>();
    stringList1.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Create\",\r\n        \"ConnectionName\": \"MyConnection\",\r\n        \"CreateRoleDefinition\": { \r\n            \"Name\": \"SalesRole\",\r\n            \"ModelPermission\": \"Read\" \r\n        }\r\n    }\r\n}");
    operationMetadata2.ExampleRequests = stringList1;
    OperationMetadata operationMetadata3 = operationMetadata1;
    dictionary2["Create"] = operationMetadata3;
    Dictionary<string, OperationMetadata> dictionary3 = dictionary1;
    OperationMetadata operationMetadata4 = new OperationMetadata { RequiredParams = new string[1]
    {
      "UpdateRoleDefinition"
    } };
    operationMetadata4.Description = "Update an existing model role. Names cannot be changed - use Rename operation instead. \r\nMandatory properties: UpdateRoleDefinition (with Name). \r\nOptional: Description, ModelPermission, Annotations, ExtendedProperties.";
    OperationMetadata operationMetadata5 = operationMetadata4;
    List<string> stringList2 = new List<string>();
    stringList2.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Update\",\r\n        \"ConnectionName\": \"MyConnection\",\r\n        \"UpdateRoleDefinition\": { \r\n            \"Name\": \"SalesRole\",\r\n            \"ModelPermission\": \"Read\"\r\n        }\r\n    }\r\n}");
    operationMetadata5.ExampleRequests = stringList2;
    OperationMetadata operationMetadata6 = operationMetadata4;
    dictionary3["Update"] = operationMetadata6;
    Dictionary<string, OperationMetadata> dictionary4 = dictionary1;
    OperationMetadata operationMetadata7 = new OperationMetadata { RequiredParams = new string[1]
    {
      "RoleName"
    } };
    operationMetadata7.Description = "Delete a model role. \r\nMandatory properties: RoleName. \r\nOptional: None.";
    OperationMetadata operationMetadata8 = operationMetadata7;
    List<string> stringList3 = new List<string>();
    stringList3.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Delete\",\r\n        \"ConnectionName\": \"MyConnection\",\r\n        \"RoleName\": \"SalesRole\"\r\n    }\r\n}");
    operationMetadata8.ExampleRequests = stringList3;
    OperationMetadata operationMetadata9 = operationMetadata7;
    dictionary4["Delete"] = operationMetadata9;
    Dictionary<string, OperationMetadata> dictionary5 = dictionary1;
    OperationMetadata operationMetadata10 = new OperationMetadata { RequiredParams = new string[1]
    {
      "RoleName"
    } };
    operationMetadata10.Description = "Get details of a model role. \r\nMandatory properties: RoleName. \r\nOptional: None.";
    OperationMetadata operationMetadata11 = operationMetadata10;
    List<string> stringList4 = new List<string>();
    stringList4.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Get\",\r\n        \"ConnectionName\": \"MyConnection\",\r\n        \"RoleName\": \"SalesRole\"\r\n    }\r\n}");
    operationMetadata11.ExampleRequests = stringList4;
    OperationMetadata operationMetadata12 = operationMetadata10;
    dictionary5["Get"] = operationMetadata12;
    Dictionary<string, OperationMetadata> dictionary6 = dictionary1;
    OperationMetadata operationMetadata13 = new OperationMetadata { Description = "List all model roles in the model. \r\nMandatory properties: None. \r\nOptional: None." };
    List<string> stringList5 = new List<string>();
    stringList5.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"List\",\r\n        \"ConnectionName\": \"MyConnection\"\r\n    }\r\n}");
    operationMetadata13.ExampleRequests = stringList5;
    dictionary6["List"] = operationMetadata13;
    Dictionary<string, OperationMetadata> dictionary7 = dictionary1;
    OperationMetadata operationMetadata14 = new OperationMetadata { RequiredParams = new string[2]
    {
      "RoleName",
      "NewRoleName"
    } };
    operationMetadata14.Description = "Rename a model role. \r\nMandatory properties: RoleName, NewRoleName. \r\nOptional: None.";
    OperationMetadata operationMetadata15 = operationMetadata14;
    List<string> stringList6 = new List<string>();
    stringList6.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Rename\",\r\n        \"ConnectionName\": \"MyConnection\",\r\n        \"RoleName\": \"SalesRole\",\r\n        \"NewRoleName\": \"NewSalesRole\"\r\n    }\r\n}");
    operationMetadata15.ExampleRequests = stringList6;
    OperationMetadata operationMetadata16 = operationMetadata14;
    dictionary7["Rename"] = operationMetadata16;
    Dictionary<string, OperationMetadata> dictionary8 = dictionary1;
    OperationMetadata operationMetadata17 = new OperationMetadata { RequiredParams = new string[1]
    {
      "CreateTablePermissionDefinition"
    } };
    operationMetadata17.Description = "Create a table permission for row-level security. \r\nMandatory properties: CreateTablePermissionDefinition (with RoleName, TableName). \r\nOptional: FilterExpression, MetadataPermission, Annotations, ExtendedProperties.";
    OperationMetadata operationMetadata18 = operationMetadata17;
    List<string> stringList7 = new List<string>();
    stringList7.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"CreatePermission\",\r\n        \"ConnectionName\": \"MyConnection\",\r\n        \"CreateTablePermissionDefinition\": { \r\n            \"RoleName\": \"SalesRole\", \r\n            \"TableName\": \"Sales\",\r\n            \"FilterExpression\": \"[Email] = USERPRINCIPALNAME()\",\r\n            \"MetadataPermission\": \"Read\"\r\n        }\r\n    }\r\n}");
    operationMetadata18.ExampleRequests = stringList7;
    OperationMetadata operationMetadata19 = operationMetadata17;
    dictionary8["CreatePermission"] = operationMetadata19;
    Dictionary<string, OperationMetadata> dictionary9 = dictionary1;
    OperationMetadata operationMetadata20 = new OperationMetadata { RequiredParams = new string[1]
    {
      "UpdateTablePermissionDefinition"
    } };
    operationMetadata20.Description = "Update a table permission for row-level security. \r\nMandatory properties: UpdateTablePermissionDefinition (with RoleName, TableName). \r\nOptional: FilterExpression, MetadataPermission, Annotations, ExtendedProperties.";
    OperationMetadata operationMetadata21 = operationMetadata20;
    List<string> stringList8 = new List<string>();
    stringList8.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"UpdatePermission\",\r\n        \"ConnectionName\": \"MyConnection\",\r\n        \"UpdateTablePermissionDefinition\": { \r\n            \"RoleName\": \"SalesRole\", \r\n            \"TableName\": \"Sales\",\r\n            \"FilterExpression\": \"[Email] = USERPRINCIPALNAME()\",\r\n            \"MetadataPermission\": \"None\"\r\n        }\r\n    }\r\n}");
    operationMetadata21.ExampleRequests = stringList8;
    OperationMetadata operationMetadata22 = operationMetadata20;
    dictionary9["UpdatePermission"] = operationMetadata22;
    Dictionary<string, OperationMetadata> dictionary10 = dictionary1;
    OperationMetadata operationMetadata23 = new OperationMetadata { RequiredParams = new string[2]
    {
      "RoleName",
      "TableName"
    } };
    operationMetadata23.Description = "Delete a table permission. \r\nMandatory properties: RoleName, TableName. \r\nOptional: None.";
    OperationMetadata operationMetadata24 = operationMetadata23;
    List<string> stringList9 = new List<string>();
    stringList9.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"DeletePermission\",\r\n        \"ConnectionName\": \"MyConnection\",\r\n        \"RoleName\": \"SalesRole\",\r\n        \"TableName\": \"Sales\"\r\n    }\r\n}");
    operationMetadata24.ExampleRequests = stringList9;
    OperationMetadata operationMetadata25 = operationMetadata23;
    dictionary10["DeletePermission"] = operationMetadata25;
    Dictionary<string, OperationMetadata> dictionary11 = dictionary1;
    OperationMetadata operationMetadata26 = new OperationMetadata { RequiredParams = new string[2]
    {
      "RoleName",
      "TableName"
    } };
    operationMetadata26.Description = "Get a table permission. \r\nMandatory properties: RoleName, TableName. \r\nOptional: None.";
    OperationMetadata operationMetadata27 = operationMetadata26;
    List<string> stringList10 = new List<string>();
    stringList10.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"GetPermission\",\r\n        \"ConnectionName\": \"MyConnection\",\r\n        \"RoleName\": \"SalesRole\",\r\n        \"TableName\": \"Sales\"\r\n    }\r\n}");
    operationMetadata27.ExampleRequests = stringList10;
    OperationMetadata operationMetadata28 = operationMetadata26;
    dictionary11["GetPermission"] = operationMetadata28;
    Dictionary<string, OperationMetadata> dictionary12 = dictionary1;
    OperationMetadata operationMetadata29 = new OperationMetadata { RequiredParams = new string[1]
    {
      "RoleName"
    } };
    operationMetadata29.Description = "Get all table permissions for a role. \r\nMandatory properties: RoleName. \r\nOptional: None.";
    OperationMetadata operationMetadata30 = operationMetadata29;
    List<string> stringList11 = new List<string>();
    stringList11.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"ListPermissions\",\r\n        \"ConnectionName\": \"MyConnection\",\r\n        \"RoleName\": \"SalesRole\"\r\n    }\r\n}");
    operationMetadata30.ExampleRequests = stringList11;
    OperationMetadata operationMetadata31 = operationMetadata29;
    dictionary12["ListPermissions"] = operationMetadata31;
    Dictionary<string, OperationMetadata> dictionary13 = dictionary1;
    OperationMetadata operationMetadata32 = new OperationMetadata { Description = "Get effective permissions for all roles. \r\nMandatory properties: None. \r\nOptional: None." };
    List<string> stringList12 = new List<string>();
    stringList12.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"GetEffectivePermissions\",\r\n        \"ConnectionName\": \"MyConnection\"\r\n    }\r\n}");
    operationMetadata32.ExampleRequests = stringList12;
    dictionary13["GetEffectivePermissions"] = operationMetadata32;
    Dictionary<string, OperationMetadata> dictionary14 = dictionary1;
    OperationMetadata operationMetadata33 = new OperationMetadata { RequiredParams = new string[1]
    {
      "RoleName"
    } };
    operationMetadata33.Description = "Export role to TMDL (YAML-like syntax) format. TMDL is a human-readable, declarative format for semantic models. \r\nMandatory properties: RoleName. \r\nOptional: TmdlExportOptions.";
    OperationMetadata operationMetadata34 = operationMetadata33;
    List<string> stringList13 = new List<string>();
    stringList13.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"ExportTMDL\",\r\n        \"ConnectionName\": \"MyConnection\",\r\n        \"RoleName\": \"SalesRole\"\r\n    }\r\n}");
    operationMetadata34.ExampleRequests = stringList13;
    OperationMetadata operationMetadata35 = operationMetadata33;
    dictionary14["ExportTMDL"] = operationMetadata35;
    Dictionary<string, OperationMetadata> dictionary15 = dictionary1;
    OperationMetadata operationMetadata36 = new OperationMetadata { RequiredParams = new string[2]
    {
      "RoleName",
      "TmslExportOptions"
    } };
    operationMetadata36.Description = "Export role to TMSL (JSON syntax) script format with specified operation type. TMSL generates executable JSON scripts for role operations. Supports Create, CreateOrReplace, Alter, Delete (Refresh not supported for roles). \r\nMandatory properties: RoleName, TmslExportOptions (with TmslOperationType). \r\nOptional: IncludeRestricted.";
    OperationMetadata operationMetadata37 = operationMetadata36;
    List<string> stringList14 = new List<string>();
    stringList14.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"ExportTMSL\",\r\n        \"ConnectionName\": \"MyConnection\",\r\n        \"RoleName\": \"SalesRole\",\r\n        \"TmslExportOptions\": {\r\n            \"TmslOperationType\": \"CreateOrReplace\",\r\n            \"IncludeRestricted\": false\r\n        }\r\n    }\r\n}");
    stringList14.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"ExportTMSL\",\r\n        \"ConnectionName\": \"MyConnection\",\r\n        \"RoleName\": \"SalesRole\",\r\n        \"TmslExportOptions\": {\r\n            \"TmslOperationType\": \"Delete\"\r\n        }\r\n    }\r\n}");
    operationMetadata37.ExampleRequests = stringList14;
    OperationMetadata operationMetadata38 = operationMetadata36;
    dictionary15["ExportTMSL"] = operationMetadata38;
    Dictionary<string, OperationMetadata> dictionary16 = dictionary1;
    OperationMetadata operationMetadata39 = new OperationMetadata { Description = "Describe the tool and its operations. \r\nMandatory properties: None. \r\nOptional: None." };
    List<string> stringList15 = new List<string>();
    stringList15.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Help\"\r\n    }\r\n}");
    operationMetadata39.ExampleRequests = stringList15;
    dictionary16["Help"] = operationMetadata39;
    Dictionary<string, OperationMetadata> dictionary17 = dictionary1;
    toolMetadata2.Operations = dictionary17;
    SecurityRoleOperationsTool.toolMetadata = toolMetadata1;
  }
}
