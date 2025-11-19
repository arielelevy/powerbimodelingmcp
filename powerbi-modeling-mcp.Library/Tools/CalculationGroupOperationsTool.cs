// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
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
public class CalculationGroupOperationsTool
{
  private readonly ILogger<CalculationGroupOperationsTool> _logger;
  public static readonly ToolMetadata toolMetadata;

  public CalculationGroupOperationsTool(ILogger<CalculationGroupOperationsTool> logger)
  {
    this._logger = logger;
  }

  [McpServerTool(Name = "calculation_group_operations")]
  [Description("Perform operations on semantic model calculation groups and calculation items. Supported operations: Help, CreateGroup, UpdateGroup, DeleteGroup, GetGroup, ListGroups, RenameGroup, CreateItem, UpdateItem, DeleteItem, GetItem, ListItems, RenameItem, ReorderItems, ExportTMDL. Use the Operation parameter to specify which operation to perform.")]
  public CalculationGroupOperationResponse ExecuteCalculationGroupOperation(
    McpServer mcpServer,
    CalculationGroupOperationRequest request)
  {
    this._logger.LogDebug("Executing {ToolName}.{Operation}: CalculationGroup={CalculationGroupName}, CalculationItem={CalculationItemName}, Connection={ConnectionName}", (object) nameof (CalculationGroupOperationsTool), (object) request.Operation, (object) request.CalculationGroupName, (object) request.CalculationItemName, (object) (request.ConnectionName ?? "(last used)"));
    try
    {
      string[] strArray1 = new string[15]
      {
        "CREATEGROUP",
        "UPDATEGROUP",
        "DELETEGROUP",
        "GETGROUP",
        "LISTGROUPS",
        "RENAMEGROUP",
        "CREATEITEM",
        "UPDATEITEM",
        "DELETEITEM",
        "GETITEM",
        "LISTITEMS",
        "RENAMEITEM",
        "REORDERITEMS",
        "EXPORTTMDL",
        "HELP"
      };
      string[] strArray2 = new string[9]
      {
        "CREATEGROUP",
        "UPDATEGROUP",
        "DELETEGROUP",
        "RENAMEGROUP",
        "CREATEITEM",
        "UPDATEITEM",
        "DELETEITEM",
        "RENAMEITEM",
        "REORDERITEMS"
      };
      if (!Enumerable.Contains<string>((IEnumerable<string>) strArray1, request.Operation.ToUpperInvariant()))
      {
        this._logger.LogWarning("Invalid operation '{Operation}' requested for {ToolName}. Valid operations: {ValidOperations}", (object) request.Operation, (object) nameof (CalculationGroupOperationsTool), (object) string.Join(", ", strArray1));
        return CalculationGroupOperationResponse.Forbidden(request.Operation, $"Invalid operation: {request.Operation}. Supported operations: {string.Join(", ", strArray1)}");
      }
      if (!this.ValidateRequest(request.Operation, request))
        throw new McpException($"Invalid request for {request.Operation} operation.");
      if (Enumerable.Contains<string>((IEnumerable<string>) strArray2, request.Operation.ToUpperInvariant()))
      {
        WriteOperationResult writeOperationResult = WriteGuard.ExecuteWriteOperationWithGuards(mcpServer, request.ConnectionName, request.Operation);
        if (!writeOperationResult.Success)
        {
          this._logger.LogWarning("{ToolName}.{Operation} blocked by write guard: {Reason}", (object) nameof (CalculationGroupOperationsTool), (object) request.Operation, (object) writeOperationResult.Message);
          return CalculationGroupOperationResponse.Forbidden(request.Operation, writeOperationResult.Message);
        }
      }
      bool allowed = WriteGuard.IsWriteAllowed("").allowed;
      string upperInvariant = request.Operation.ToUpperInvariant();
      CalculationGroupOperationResponse operationResponse;
      if (upperInvariant != null)
      {
        switch (upperInvariant.Length)
        {
          case 4:
            if ((upperInvariant == "HELP"))
            {
              operationResponse = this.HandleHelpOperation(request, allowed ? strArray1 : Enumerable.ToArray<string>(Enumerable.Except<string>((IEnumerable<string>) strArray1, (IEnumerable<string>) strArray2)));
              goto label_43;
            }
            break;
          case 7:
            if ((upperInvariant == "GETITEM"))
            {
              operationResponse = this.HandleGetItemOperation(request);
              goto label_43;
            }
            break;
          case 8:
            if ((upperInvariant == "GETGROUP"))
            {
              operationResponse = this.HandleGetGroupOperation(request);
              goto label_43;
            }
            break;
          case 9:
            if ((upperInvariant == "LISTITEMS"))
            {
              operationResponse = this.HandleListItemsOperation(request);
              goto label_43;
            }
            break;
          case 10:
            switch (upperInvariant[0])
            {
              case 'C':
                if ((upperInvariant == "CREATEITEM"))
                {
                  operationResponse = this.HandleCreateItemOperation(request);
                  goto label_43;
                }
                break;
              case 'D':
                if ((upperInvariant == "DELETEITEM"))
                {
                  operationResponse = this.HandleDeleteItemOperation(request);
                  goto label_43;
                }
                break;
              case 'E':
                if ((upperInvariant == "EXPORTTMDL"))
                {
                  operationResponse = this.HandleExportTMDLOperation(request);
                  goto label_43;
                }
                break;
              case 'L':
                if ((upperInvariant == "LISTGROUPS"))
                {
                  operationResponse = this.HandleListGroupsOperation(request);
                  goto label_43;
                }
                break;
              case 'R':
                if ((upperInvariant == "RENAMEITEM"))
                {
                  operationResponse = this.HandleRenameItemOperation(request);
                  goto label_43;
                }
                break;
              case 'U':
                if ((upperInvariant == "UPDATEITEM"))
                {
                  operationResponse = this.HandleUpdateItemOperation(request);
                  goto label_43;
                }
                break;
            }
            break;
          case 11:
            switch (upperInvariant[0])
            {
              case 'C':
                if ((upperInvariant == "CREATEGROUP"))
                {
                  operationResponse = this.HandleCreateGroupOperation(request);
                  goto label_43;
                }
                break;
              case 'D':
                if ((upperInvariant == "DELETEGROUP"))
                {
                  operationResponse = this.HandleDeleteGroupOperation(request);
                  goto label_43;
                }
                break;
              case 'R':
                if ((upperInvariant == "RENAMEGROUP"))
                {
                  operationResponse = this.HandleRenameGroupOperation(request);
                  goto label_43;
                }
                break;
              case 'U':
                if ((upperInvariant == "UPDATEGROUP"))
                {
                  operationResponse = this.HandleUpdateGroupOperation(request);
                  goto label_43;
                }
                break;
            }
            break;
          case 12:
            if ((upperInvariant == "REORDERITEMS"))
            {
              operationResponse = this.HandleReorderItemsOperation(request);
              goto label_43;
            }
            break;
        }
      }
      operationResponse = CalculationGroupOperationResponse.Forbidden(request.Operation, $"Operation {request.Operation} not implemented");
label_43:
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Error executing {ToolName}.{Operation}: {ErrorMessage}", (object) nameof (CalculationGroupOperationsTool), (object) request.Operation, (object) ex.Message);
      return new CalculationGroupOperationResponse()
      {
        Success = false,
        Message = "Error executing calculation group operation: " + ex.Message,
        Operation = request.Operation,
        CalculationGroupName = request.CalculationGroupName,
        CalculationItemName = request.CalculationItemName
      };
    }
  }

  private CalculationGroupOperationResponse HandleCreateGroupOperation(
    CalculationGroupOperationRequest request)
  {
    try
    {
      CalculationGroupOperationResult calculationGroup = CalculationGroupOperations.CreateCalculationGroup(request.ConnectionName, request.CreateGroupDefinition);
      this._logger.LogInformation("{ToolName}.{Operation} completed: CalculationGroup={CalculationGroupName}, ItemCount={ItemCount}", (object) nameof (CalculationGroupOperationsTool), (object) "CreateGroup", (object) request.CreateGroupDefinition.Name, (object) calculationGroup.CalculationItemCount);
      CalculationGroupOperationResponse groupOperation = new CalculationGroupOperationResponse { Success = true };
      groupOperation.Message = $"Calculation group '{request.CreateGroupDefinition.Name}' created successfully with {calculationGroup.CalculationItemCount} calculation items";
      groupOperation.Operation = request.Operation;
      groupOperation.CalculationGroupName = request.CreateGroupDefinition.Name;
      groupOperation.OperationResult = (object) calculationGroup;
      return groupOperation;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      CalculationGroupOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new CalculationGroupOperationResponse()
      {
        Success = false,
        Message = "Failed to create calculation group: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private CalculationGroupOperationResponse HandleUpdateGroupOperation(
    CalculationGroupOperationRequest request)
  {
    try
    {
      CalculationGroupOperationResult groupOperationResult = CalculationGroupOperations.UpdateCalculationGroup(request.ConnectionName, request.UpdateGroupDefinition);
      this._logger.LogInformation("{ToolName}.{Operation} completed: CalculationGroup={CalculationGroupName}", (object) nameof (CalculationGroupOperationsTool), (object) "UpdateGroup", (object) request.UpdateGroupDefinition.Name);
      return new CalculationGroupOperationResponse()
      {
        Success = true,
        Message = $"Calculation group '{request.UpdateGroupDefinition.Name}' updated successfully",
        Operation = request.Operation,
        CalculationGroupName = request.UpdateGroupDefinition.Name,
        OperationResult = (object) groupOperationResult
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      CalculationGroupOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new CalculationGroupOperationResponse()
      {
        Success = false,
        Message = "Failed to update calculation group: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private CalculationGroupOperationResponse HandleDeleteGroupOperation(
    CalculationGroupOperationRequest request)
  {
    try
    {
      CalculationGroupOperations.DeleteCalculationGroup(request.ConnectionName, request.CalculationGroupName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: CalculationGroup={CalculationGroupName}", (object) nameof (CalculationGroupOperationsTool), (object) "DeleteGroup", (object) request.CalculationGroupName);
      return new CalculationGroupOperationResponse()
      {
        Success = true,
        Message = $"Calculation group '{request.CalculationGroupName}' deleted successfully",
        Operation = request.Operation,
        CalculationGroupName = request.CalculationGroupName
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      CalculationGroupOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new CalculationGroupOperationResponse()
      {
        Success = false,
        Message = "Failed to delete calculation group: " + ex.Message,
        Operation = request.Operation,
        CalculationGroupName = request.CalculationGroupName,
        Help = (object) operationMetadata
      };
    }
  }

  private CalculationGroupOperationResponse HandleGetGroupOperation(
    CalculationGroupOperationRequest request)
  {
    try
    {
      CalculationGroupGet calculationGroup = CalculationGroupOperations.GetCalculationGroup(request.ConnectionName, request.CalculationGroupName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: CalculationGroup={CalculationGroupName}, ItemCount={ItemCount}", (object) nameof (CalculationGroupOperationsTool), (object) "GetGroup", (object) request.CalculationGroupName, (object) calculationGroup.CalculationItems.Count);
      CalculationGroupOperationResponse groupOperation = new CalculationGroupOperationResponse { Success = true };
      groupOperation.Message = $"Calculation group '{request.CalculationGroupName}' retrieved successfully with {calculationGroup.CalculationItems.Count} calculation items";
      groupOperation.Operation = request.Operation;
      groupOperation.CalculationGroupName = request.CalculationGroupName;
      groupOperation.Data = (object) calculationGroup;
      return groupOperation;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      CalculationGroupOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new CalculationGroupOperationResponse()
      {
        Success = false,
        Message = "Failed to get calculation group: " + ex.Message,
        Operation = request.Operation,
        CalculationGroupName = request.CalculationGroupName,
        Help = (object) operationMetadata
      };
    }
  }

  private CalculationGroupOperationResponse HandleListGroupsOperation(
    CalculationGroupOperationRequest request)
  {
    try
    {
      List<CalculationGroupList> calculationGroupListList = CalculationGroupOperations.ListCalculationGroups(request.ConnectionName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Count={Count}", (object) nameof (CalculationGroupOperationsTool), (object) "ListGroups", (object) calculationGroupListList.Count);
      CalculationGroupOperationResponse operationResponse = new CalculationGroupOperationResponse { Success = true };
      operationResponse.Message = $"Found {calculationGroupListList.Count} calculation groups";
      operationResponse.Operation = request.Operation;
      operationResponse.Data = (object) calculationGroupListList;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      CalculationGroupOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new CalculationGroupOperationResponse()
      {
        Success = false,
        Message = "Failed to list calculation groups: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private CalculationGroupOperationResponse HandleRenameGroupOperation(
    CalculationGroupOperationRequest request)
  {
    try
    {
      CalculationGroupOperations.RenameCalculationGroup(request.ConnectionName, request.CalculationGroupName, request.NewCalculationGroupName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: From={OldName}, To={NewName}", (object) nameof (CalculationGroupOperationsTool), (object) "RenameGroup", (object) request.CalculationGroupName, (object) request.NewCalculationGroupName);
      CalculationGroupOperationResponse operationResponse = new CalculationGroupOperationResponse { Success = true };
      operationResponse.Message = $"Calculation group '{request.CalculationGroupName}' renamed to '{request.NewCalculationGroupName}' successfully";
      operationResponse.Operation = request.Operation;
      operationResponse.CalculationGroupName = request.NewCalculationGroupName;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      CalculationGroupOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new CalculationGroupOperationResponse()
      {
        Success = false,
        Message = "Failed to rename calculation group: " + ex.Message,
        Operation = request.Operation,
        CalculationGroupName = request.CalculationGroupName,
        Help = (object) operationMetadata
      };
    }
  }

  private CalculationGroupOperationResponse HandleCreateItemOperation(
    CalculationGroupOperationRequest request)
  {
    try
    {
      CalculationItemOperationResult calculationItem = CalculationGroupOperations.CreateCalculationItem(request.ConnectionName, request.CalculationGroupName, request.CreateItemDefinition);
      this._logger.LogInformation("{ToolName}.{Operation} completed: CalculationGroup={CalculationGroupName}, CalculationItem={CalculationItemName}", (object) nameof (CalculationGroupOperationsTool), (object) "CreateItem", (object) request.CalculationGroupName, (object) request.CreateItemDefinition.Name);
      CalculationGroupOperationResponse itemOperation = new CalculationGroupOperationResponse { Success = true };
      itemOperation.Message = $"Calculation item '{request.CreateItemDefinition.Name}' created successfully in calculation group '{request.CalculationGroupName}'";
      itemOperation.Operation = "CREATEITEM";
      itemOperation.CalculationGroupName = request.CalculationGroupName;
      itemOperation.CalculationItemName = request.CreateItemDefinition.Name;
      itemOperation.OperationResult = (object) calculationItem;
      return itemOperation;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      CalculationGroupOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new CalculationGroupOperationResponse()
      {
        Success = false,
        Message = "Failed to create calculation item: " + ex.Message,
        Operation = request.Operation,
        CalculationGroupName = request.CalculationGroupName,
        Help = (object) operationMetadata
      };
    }
  }

  private CalculationGroupOperationResponse HandleUpdateItemOperation(
    CalculationGroupOperationRequest request)
  {
    try
    {
      CalculationItemOperationResult itemOperationResult = CalculationGroupOperations.UpdateCalculationItem(request.ConnectionName, request.CalculationGroupName, request.CalculationItemName, request.UpdateItemDefinition);
      this._logger.LogInformation("{ToolName}.{Operation} completed: CalculationGroup={CalculationGroupName}, CalculationItem={CalculationItemName}", (object) nameof (CalculationGroupOperationsTool), (object) "UpdateItem", (object) request.CalculationGroupName, (object) request.CalculationItemName);
      CalculationGroupOperationResponse operationResponse = new CalculationGroupOperationResponse { Success = true };
      operationResponse.Message = $"Calculation item '{request.CalculationItemName}' updated successfully in calculation group '{request.CalculationGroupName}'";
      operationResponse.Operation = "UPDATEITEM";
      operationResponse.CalculationGroupName = request.CalculationGroupName;
      operationResponse.CalculationItemName = request.CalculationItemName;
      operationResponse.OperationResult = (object) itemOperationResult;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      CalculationGroupOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new CalculationGroupOperationResponse()
      {
        Success = false,
        Message = "Failed to update calculation item: " + ex.Message,
        Operation = request.Operation,
        CalculationGroupName = request.CalculationGroupName,
        CalculationItemName = request.CalculationItemName,
        Help = (object) operationMetadata
      };
    }
  }

  private CalculationGroupOperationResponse HandleDeleteItemOperation(
    CalculationGroupOperationRequest request)
  {
    try
    {
      CalculationGroupOperations.DeleteCalculationItem(request.ConnectionName, request.CalculationGroupName, request.CalculationItemName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: CalculationGroup={CalculationGroupName}, CalculationItem={CalculationItemName}", (object) nameof (CalculationGroupOperationsTool), (object) "DeleteItem", (object) request.CalculationGroupName, (object) request.CalculationItemName);
      CalculationGroupOperationResponse operationResponse = new CalculationGroupOperationResponse { Success = true };
      operationResponse.Message = $"Calculation item '{request.CalculationItemName}' deleted successfully from calculation group '{request.CalculationGroupName}'";
      operationResponse.Operation = request.Operation;
      operationResponse.CalculationGroupName = request.CalculationGroupName;
      operationResponse.CalculationItemName = request.CalculationItemName;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      CalculationGroupOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new CalculationGroupOperationResponse()
      {
        Success = false,
        Message = "Error deleting calculation item: " + ex.Message,
        Operation = request.Operation,
        CalculationGroupName = request.CalculationGroupName,
        CalculationItemName = request.CalculationItemName,
        Help = (object) operationMetadata
      };
    }
  }

  private CalculationGroupOperationResponse HandleGetItemOperation(
    CalculationGroupOperationRequest request)
  {
    try
    {
      CalculationItemGet calculationItem = CalculationGroupOperations.GetCalculationItem(request.ConnectionName, request.CalculationGroupName, request.CalculationItemName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: CalculationGroup={CalculationGroupName}, CalculationItem={CalculationItemName}", (object) nameof (CalculationGroupOperationsTool), (object) "GetItem", (object) request.CalculationGroupName, (object) request.CalculationItemName);
      CalculationGroupOperationResponse itemOperation = new CalculationGroupOperationResponse { Success = true };
      itemOperation.Message = $"Calculation item '{request.CalculationItemName}' retrieved successfully from calculation group '{request.CalculationGroupName}'";
      itemOperation.Operation = request.Operation;
      itemOperation.CalculationGroupName = request.CalculationGroupName;
      itemOperation.CalculationItemName = request.CalculationItemName;
      itemOperation.Data = (object) calculationItem;
      return itemOperation;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      CalculationGroupOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new CalculationGroupOperationResponse()
      {
        Success = false,
        Message = "Error getting calculation item: " + ex.Message,
        Operation = request.Operation,
        CalculationGroupName = request.CalculationGroupName,
        CalculationItemName = request.CalculationItemName,
        Help = (object) operationMetadata
      };
    }
  }

  private CalculationGroupOperationResponse HandleListItemsOperation(
    CalculationGroupOperationRequest request)
  {
    try
    {
      List<CalculationItemList> calculationItemListList = CalculationGroupOperations.ListCalculationItems(request.ConnectionName, request.CalculationGroupName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: CalculationGroup={CalculationGroupName}, Count={Count}", (object) nameof (CalculationGroupOperationsTool), (object) "ListItems", (object) request.CalculationGroupName, (object) calculationItemListList.Count);
      CalculationGroupOperationResponse operationResponse = new CalculationGroupOperationResponse { Success = true };
      operationResponse.Message = $"Found {calculationItemListList.Count} calculation items in calculation group '{request.CalculationGroupName}'";
      operationResponse.Operation = request.Operation;
      operationResponse.CalculationGroupName = request.CalculationGroupName;
      operationResponse.Data = (object) calculationItemListList;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      CalculationGroupOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new CalculationGroupOperationResponse()
      {
        Success = false,
        Message = "Error listing calculation items: " + ex.Message,
        Operation = request.Operation,
        CalculationGroupName = request.CalculationGroupName,
        Help = (object) operationMetadata
      };
    }
  }

  private CalculationGroupOperationResponse HandleRenameItemOperation(
    CalculationGroupOperationRequest request)
  {
    try
    {
      CalculationGroupOperations.RenameCalculationItem(request.ConnectionName, request.CalculationGroupName, request.CalculationItemName, request.NewCalculationItemName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: CalculationGroup={CalculationGroupName}, From={OldName}, To={NewName}", (object) nameof (CalculationGroupOperationsTool), (object) "RenameItem", (object) request.CalculationGroupName, (object) request.CalculationItemName, (object) request.NewCalculationItemName);
      CalculationGroupOperationResponse operationResponse = new CalculationGroupOperationResponse { Success = true };
      operationResponse.Message = $"Calculation item '{request.CalculationItemName}' renamed to '{request.NewCalculationItemName}' successfully in calculation group '{request.CalculationGroupName}'";
      operationResponse.Operation = request.Operation;
      operationResponse.CalculationGroupName = request.CalculationGroupName;
      operationResponse.CalculationItemName = request.NewCalculationItemName;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      CalculationGroupOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new CalculationGroupOperationResponse()
      {
        Success = false,
        Message = "Error renaming calculation item: " + ex.Message,
        Operation = request.Operation,
        CalculationGroupName = request.CalculationGroupName,
        CalculationItemName = request.CalculationItemName,
        Help = (object) operationMetadata
      };
    }
  }

  private CalculationGroupOperationResponse HandleReorderItemsOperation(
    CalculationGroupOperationRequest request)
  {
    try
    {
      CalculationGroupOperations.ReorderCalculationItems(request.ConnectionName, request.CalculationGroupName, request.ItemNamesInOrder);
      this._logger.LogInformation("{ToolName}.{Operation} completed: CalculationGroup={CalculationGroupName}, ItemCount={Count}", (object) nameof (CalculationGroupOperationsTool), (object) "ReorderItems", (object) request.CalculationGroupName, (object) request.ItemNamesInOrder.Count);
      return new CalculationGroupOperationResponse()
      {
        Success = true,
        Message = $"Calculation items reordered successfully in calculation group '{request.CalculationGroupName}'. New order: {string.Join(", ", (IEnumerable<string>) request.ItemNamesInOrder)}",
        Operation = request.Operation,
        CalculationGroupName = request.CalculationGroupName,
        Data = (object) request.ItemNamesInOrder
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      CalculationGroupOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new CalculationGroupOperationResponse()
      {
        Success = false,
        Message = "Error reordering calculation items: " + ex.Message,
        Operation = request.Operation,
        CalculationGroupName = request.CalculationGroupName,
        Help = (object) operationMetadata
      };
    }
  }

  private CalculationGroupOperationResponse HandleExportTMDLOperation(
    CalculationGroupOperationRequest request)
  {
    try
    {
      string str = CalculationGroupOperations.ExportTMDL(request.ConnectionName, request.CalculationGroupName, (ExportTmdl) request.TmdlExportOptions);
      this._logger.LogInformation("{ToolName}.{Operation} completed: CalculationGroup={CalculationGroupName}", (object) nameof (CalculationGroupOperationsTool), (object) "ExportTMDL", (object) request.CalculationGroupName);
      return new CalculationGroupOperationResponse()
      {
        Success = true,
        Message = $"TMDL for calculation group '{request.CalculationGroupName}' exported successfully",
        Operation = request.Operation,
        CalculationGroupName = request.CalculationGroupName,
        Data = (object) str
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      CalculationGroupOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new CalculationGroupOperationResponse()
      {
        Success = false,
        Message = $"Error getting TMDL for calculation group '{request.CalculationGroupName}': {ex.Message}",
        Operation = request.Operation,
        CalculationGroupName = request.CalculationGroupName,
        Help = (object) operationMetadata
      };
    }
  }

  private CalculationGroupOperationResponse HandleHelpOperation(
    CalculationGroupOperationRequest request,
    string[] operations)
  {
    this._logger.LogInformation("{ToolName}.{Operation} completed: Operations={OperationCount}", (object) nameof (CalculationGroupOperationsTool), (object) "Help", (object) operations.Length);
    return new CalculationGroupOperationResponse()
    {
      Success = true,
      Message = "Tool description retrieved successfully",
      Operation = request.Operation,
      Help = (object) new
      {
        ToolName = "calculation_group_operations",
        Description = "Perform operations on semantic model calculation groups and calculation items.",
        SupportedOperations = operations,
        Examples = Enumerable.Where<KeyValuePair<string, OperationMetadata>>((IEnumerable<KeyValuePair<string, OperationMetadata>>) CalculationGroupOperationsTool.toolMetadata.Operations, (Func<KeyValuePair<string, OperationMetadata>, bool>) (p => Enumerable.Contains<string>((IEnumerable<string>) operations, p.Key, (IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase))),
        Notes = new string[3]
        {
          "Use the Operation parameter to specify which operation to perform.",
          "For operations that require a CalculationGroupName or CalculationItemName, the request should include these properties.",
          "For operations that require a CreateGroupDefinition or CreateItemDefinition, the request should include these properties."
        }
      }
    };
  }

  private bool ValidateRequest(string operation, CalculationGroupOperationRequest request)
  {
    OperationMetadata operationMetadata;
    if (!CalculationGroupOperationsTool.toolMetadata.Operations.TryGetValue(operation, out operationMetadata))
      return true;
    JsonObject requestDict = JsonSerializer.SerializeToNode<CalculationGroupOperationRequest>(request) as JsonObject;
    List<string> list1 = Enumerable.ToList<string>(Enumerable.Where<string>((IEnumerable<string>) operationMetadata.RequiredParams, (p => requestDict != null && requestDict[p] == null)));
    List<string> list2 = Enumerable.ToList<string>(Enumerable.Where<string>((IEnumerable<string>) operationMetadata.ForbiddenParams, (p => requestDict != null && requestDict[p] != null)));
    if (Enumerable.Any<string>((IEnumerable<string>) list1))
      throw new McpException($"Missing required parameters needed for {operation} operation: {string.Join(", ", (IEnumerable<string>) list1)}");
    if (Enumerable.Any<string>((IEnumerable<string>) list2))
      throw new McpException($"Forbidden parameters not allowed for {operation} operation: {string.Join(", ", (IEnumerable<string>) list2)}");
    return true;
  }

  static CalculationGroupOperationsTool()
  {
    ToolMetadata toolMetadata1 = new ToolMetadata();
    ToolMetadata toolMetadata2 = toolMetadata1;
    Dictionary<string, OperationMetadata> dictionary1 = new Dictionary<string, OperationMetadata>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    Dictionary<string, OperationMetadata> dictionary2 = dictionary1;
    OperationMetadata operationMetadata1 = new OperationMetadata { RequiredParams = new string[1]
    {
      "CreateGroupDefinition"
    } };
    operationMetadata1.Description = "Create a new calculation group with optional initial calculation items.\r\nMandatory properties: CreateGroupDefinition (with Name).\r\nOptional: Description, IsHidden, Precedence, MultipleOrEmptySelectionExpression, NoSelectionExpression, CalculationItems, Annotations.\r\nNote: If MultipleOrEmptySelectionExpression or NoSelectionExpression are provided, their Expression property is mandatory.";
    OperationMetadata operationMetadata2 = operationMetadata1;
    List<string> stringList1 = new List<string>();
    stringList1.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"CreateGroup\",\r\n        \"CreateGroupDefinition\": { \r\n            \"Name\": \"Time Intelligence\",\r\n            \"Precedence\": 1,\r\n            \"NoSelectionExpression\": {\r\n                \"Expression\": \"SELECTEDMEASURE()\"\r\n            }\r\n        }\r\n    }\r\n}");
    stringList1.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"CreateGroup\",\r\n        \"CreateGroupDefinition\": { \r\n            \"Name\": \"Time Intelligence\",\r\n            \"Precedence\": 1,\r\n            \"MultipleOrEmptySelectionExpression\": {\r\n                \"Expression\": \"CALCULATE(SELECTEDMEASURE(), ALL('Date'))\"\r\n            }\r\n        }\r\n    }\r\n}");
    operationMetadata2.ExampleRequests = stringList1;
    OperationMetadata operationMetadata3 = operationMetadata1;
    dictionary2["CreateGroup"] = operationMetadata3;
    Dictionary<string, OperationMetadata> dictionary3 = dictionary1;
    OperationMetadata operationMetadata4 = new OperationMetadata { RequiredParams = new string[1]
    {
      "UpdateGroupDefinition"
    } };
    operationMetadata4.Description = "Update an existing calculation group. Names cannot be changed and must use the Rename operation instead.\r\nMandatory properties: UpdateGroupDefinition (with Name).\r\nOptional: Description, IsHidden, Precedence, MultipleOrEmptySelectionExpression, NoSelectionExpression, Annotations.";
    OperationMetadata operationMetadata5 = operationMetadata4;
    List<string> stringList2 = new List<string>();
    stringList2.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"UpdateGroup\",\r\n        \"UpdateGroupDefinition\": { \r\n            \"Name\": \"Time Intelligence\",\r\n            \"Precedence\": 1,\r\n            \"NoSelectionExpression\": {\r\n                \"Expression\": \"SELECTEDMEASURE()\"\r\n            },\r\n            \"CalculationItems\": [\r\n                {\r\n                    \"Name\": \"YTD\",\r\n                    \"Expression\": \"CALCULATE(SELECTEDMEASURE(), DATESYTD('Date'[Date]))\",\r\n                    \"Ordinal\": 1\r\n                }\r\n            ]\r\n        }\r\n    }\r\n}");
    operationMetadata5.ExampleRequests = stringList2;
    OperationMetadata operationMetadata6 = operationMetadata4;
    dictionary3["UpdateGroup"] = operationMetadata6;
    Dictionary<string, OperationMetadata> dictionary4 = dictionary1;
    OperationMetadata operationMetadata7 = new OperationMetadata { RequiredParams = new string[1]
    {
      "CalculationGroupName"
    } };
    operationMetadata7.Description = "Delete a calculation group and all its calculation items.\r\nMandatory properties: CalculationGroupName.";
    OperationMetadata operationMetadata8 = operationMetadata7;
    List<string> stringList3 = new List<string>();
    stringList3.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"DeleteGroup\",\r\n        \"CalculationGroupName\": \"ObsoleteGroup\"\r\n    }\r\n}");
    operationMetadata8.ExampleRequests = stringList3;
    OperationMetadata operationMetadata9 = operationMetadata7;
    dictionary4["DeleteGroup"] = operationMetadata9;
    Dictionary<string, OperationMetadata> dictionary5 = dictionary1;
    OperationMetadata operationMetadata10 = new OperationMetadata { RequiredParams = new string[1]
    {
      "CalculationGroupName"
    } };
    operationMetadata10.Description = "Get details of a calculation group including all its calculation items and metadata.\r\nMandatory properties: CalculationGroupName.";
    OperationMetadata operationMetadata11 = operationMetadata10;
    List<string> stringList4 = new List<string>();
    stringList4.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"GetGroup\",\r\n        \"CalculationGroupName\": \"Time Intelligence\"\r\n    }\r\n}");
    operationMetadata11.ExampleRequests = stringList4;
    OperationMetadata operationMetadata12 = operationMetadata10;
    dictionary5["GetGroup"] = operationMetadata12;
    Dictionary<string, OperationMetadata> dictionary6 = dictionary1;
    OperationMetadata operationMetadata13 = new OperationMetadata { Description = "List all calculation groups in the model with basic information.\r\nMandatory properties: None." };
    List<string> stringList5 = new List<string>();
    stringList5.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"ListGroups\"\r\n    }\r\n}");
    operationMetadata13.ExampleRequests = stringList5;
    dictionary6["ListGroups"] = operationMetadata13;
    Dictionary<string, OperationMetadata> dictionary7 = dictionary1;
    OperationMetadata operationMetadata14 = new OperationMetadata { RequiredParams = new string[2]
    {
      "CalculationGroupName",
      "NewCalculationGroupName"
    } };
    operationMetadata14.Description = "Rename a calculation group to a new name.\r\nMandatory properties: CalculationGroupName, NewCalculationGroupName.";
    OperationMetadata operationMetadata15 = operationMetadata14;
    List<string> stringList6 = new List<string>();
    stringList6.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"RenameGroup\",\r\n        \"CalculationGroupName\": \"OldGroup\",\r\n        \"NewCalculationGroupName\": \"NewGroup\"\r\n    }\r\n}");
    operationMetadata15.ExampleRequests = stringList6;
    OperationMetadata operationMetadata16 = operationMetadata14;
    dictionary7["RenameGroup"] = operationMetadata16;
    Dictionary<string, OperationMetadata> dictionary8 = dictionary1;
    OperationMetadata operationMetadata17 = new OperationMetadata { RequiredParams = new string[2]
    {
      "CalculationGroupName",
      "CreateItemDefinition"
    } };
    operationMetadata17.Description = "Create a calculation item in an existing calculation group.\r\nMandatory properties: CalculationGroupName, CreateItemDefinition (with Name, Expression).\r\nOptional: Description, Ordinal, FormatStringExpression, CalculationGroupName, Annotations.";
    OperationMetadata operationMetadata18 = operationMetadata17;
    List<string> stringList7 = new List<string>();
    stringList7.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"CreateItem\",\r\n        \"CalculationGroupName\": \"Time Intelligence\",\r\n        \"CreateItemDefinition\": { \r\n            \"Name\": \"YTD\", \r\n            \"Expression\": \"CALCULATE(SELECTEDMEASURE(), DATESYTD('Date'[Date]))\",\r\n            \"Ordinal\": 1,\r\n            \"CalculationGroupName\": \"Time Intelligence\" \r\n        }\r\n    }\r\n}");
    operationMetadata18.ExampleRequests = stringList7;
    OperationMetadata operationMetadata19 = operationMetadata17;
    dictionary8["CreateItem"] = operationMetadata19;
    Dictionary<string, OperationMetadata> dictionary9 = dictionary1;
    OperationMetadata operationMetadata20 = new OperationMetadata { RequiredParams = new string[3]
    {
      "CalculationGroupName",
      "CalculationItemName",
      "UpdateItemDefinition"
    } };
    operationMetadata20.Description = "Update a calculation item in a group. Names cannot be changed and must use the Rename operation instead.\r\nMandatory properties: CalculationGroupName, CalculationItemName, UpdateItemDefinition (with Name).\r\nOptional: Description, Expression, Ordinal, FormatStringExpression, CalculationGroupName, Annotations.\r\nNote: Expression cannot be empty string if provided.";
    OperationMetadata operationMetadata21 = operationMetadata20;
    List<string> stringList8 = new List<string>();
    stringList8.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"UpdateItem\",\r\n        \"CalculationGroupName\": \"Time Intelligence\",\r\n        \"CalculationItemName\": \"YTD\",\r\n        \"UpdateItemDefinition\": { \r\n            \"Name\": \"YTD\", \r\n            \"Expression\": \"CALCULATE(SELECTEDMEASURE(), DATESYTD('Date'[Date]))\",\r\n            \"Ordinal\": 1,\r\n            \"CalculationGroupName\": \"Time Intelligence\" \r\n        }\r\n    }\r\n}");
    operationMetadata21.ExampleRequests = stringList8;
    OperationMetadata operationMetadata22 = operationMetadata20;
    dictionary9["UpdateItem"] = operationMetadata22;
    Dictionary<string, OperationMetadata> dictionary10 = dictionary1;
    OperationMetadata operationMetadata23 = new OperationMetadata { RequiredParams = new string[2]
    {
      "CalculationGroupName",
      "CalculationItemName"
    } };
    operationMetadata23.Description = "Delete a calculation item from a calculation group.\r\nMandatory properties: CalculationGroupName, CalculationItemName.";
    OperationMetadata operationMetadata24 = operationMetadata23;
    List<string> stringList9 = new List<string>();
    stringList9.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"DeleteItem\",\r\n        \"CalculationGroupName\": \"Time Intelligence\",\r\n        \"CalculationItemName\": \"ObsoleteItem\"\r\n    }\r\n}");
    operationMetadata24.ExampleRequests = stringList9;
    OperationMetadata operationMetadata25 = operationMetadata23;
    dictionary10["DeleteItem"] = operationMetadata25;
    Dictionary<string, OperationMetadata> dictionary11 = dictionary1;
    OperationMetadata operationMetadata26 = new OperationMetadata { RequiredParams = new string[2]
    {
      "CalculationGroupName",
      "CalculationItemName"
    } };
    operationMetadata26.Description = "Get details of a specific calculation item from a calculation group.\r\nMandatory properties: CalculationGroupName, CalculationItemName.";
    OperationMetadata operationMetadata27 = operationMetadata26;
    List<string> stringList10 = new List<string>();
    stringList10.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"GetItem\",\r\n        \"CalculationGroupName\": \"Time Intelligence\",\r\n        \"CalculationItemName\": \"YTD\"\r\n    }\r\n}");
    operationMetadata27.ExampleRequests = stringList10;
    OperationMetadata operationMetadata28 = operationMetadata26;
    dictionary11["GetItem"] = operationMetadata28;
    Dictionary<string, OperationMetadata> dictionary12 = dictionary1;
    OperationMetadata operationMetadata29 = new OperationMetadata { RequiredParams = new string[1]
    {
      "CalculationGroupName"
    } };
    operationMetadata29.Description = "List all calculation items in a calculation group with basic information.\r\nMandatory properties: CalculationGroupName.";
    OperationMetadata operationMetadata30 = operationMetadata29;
    List<string> stringList11 = new List<string>();
    stringList11.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"ListItems\",\r\n        \"CalculationGroupName\": \"Time Intelligence\"\r\n    }\r\n}");
    operationMetadata30.ExampleRequests = stringList11;
    OperationMetadata operationMetadata31 = operationMetadata29;
    dictionary12["ListItems"] = operationMetadata31;
    Dictionary<string, OperationMetadata> dictionary13 = dictionary1;
    OperationMetadata operationMetadata32 = new OperationMetadata { RequiredParams = new string[3]
    {
      "CalculationGroupName",
      "CalculationItemName",
      "NewCalculationItemName"
    } };
    operationMetadata32.Description = "Rename a calculation item in a calculation group to a new name.\r\nMandatory properties: CalculationGroupName, CalculationItemName, NewCalculationItemName.";
    OperationMetadata operationMetadata33 = operationMetadata32;
    List<string> stringList12 = new List<string>();
    stringList12.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"RenameItem\",\r\n        \"CalculationGroupName\": \"Time Intelligence\",\r\n        \"CalculationItemName\": \"OldItem\",\r\n        \"NewCalculationItemName\": \"NewItem\"\r\n    }\r\n}");
    operationMetadata33.ExampleRequests = stringList12;
    OperationMetadata operationMetadata34 = operationMetadata32;
    dictionary13["RenameItem"] = operationMetadata34;
    Dictionary<string, OperationMetadata> dictionary14 = dictionary1;
    OperationMetadata operationMetadata35 = new OperationMetadata { RequiredParams = new string[2]
    {
      "CalculationGroupName",
      "ItemNamesInOrder"
    } };
    operationMetadata35.Description = "Reorder calculation items in a calculation group by specifying the desired order.\r\nMandatory properties: CalculationGroupName, ItemNamesInOrder.";
    OperationMetadata operationMetadata36 = operationMetadata35;
    List<string> stringList13 = new List<string>();
    stringList13.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"ReorderItems\",\r\n        \"CalculationGroupName\": \"Time Intelligence\",\r\n        \"ItemNamesInOrder\": [\"YTD\", \"QTD\", \"MTD\"]\r\n    }\r\n}");
    operationMetadata36.ExampleRequests = stringList13;
    OperationMetadata operationMetadata37 = operationMetadata35;
    dictionary14["ReorderItems"] = operationMetadata37;
    Dictionary<string, OperationMetadata> dictionary15 = dictionary1;
    OperationMetadata operationMetadata38 = new OperationMetadata { RequiredParams = new string[1]
    {
      "CalculationGroupName"
    } };
    operationMetadata38.Description = "Export a calculation group to TMDL format.\r\nMandatory properties: CalculationGroupName.\r\nOptional: TmdlExportOptions.";
    OperationMetadata operationMetadata39 = operationMetadata38;
    List<string> stringList14 = new List<string>();
    stringList14.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"ExportTMDL\",\r\n        \"CalculationGroupName\": \"Time Intelligence\"\r\n    }\r\n}");
    operationMetadata39.ExampleRequests = stringList14;
    OperationMetadata operationMetadata40 = operationMetadata38;
    dictionary15["ExportTMDL"] = operationMetadata40;
    Dictionary<string, OperationMetadata> dictionary16 = dictionary1;
    OperationMetadata operationMetadata41 = new OperationMetadata { Description = "Describe the tool and its operations with usage examples.\r\nMandatory properties: None." };
    List<string> stringList15 = new List<string>();
    stringList15.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Help\"\r\n    }\r\n}");
    operationMetadata41.ExampleRequests = stringList15;
    dictionary16["Help"] = operationMetadata41;
    Dictionary<string, OperationMetadata> dictionary17 = dictionary1;
    toolMetadata2.Operations = dictionary17;
    CalculationGroupOperationsTool.toolMetadata = toolMetadata1;
  }
}
