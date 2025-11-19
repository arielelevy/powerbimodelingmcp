// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Tools.BatchPerspectiveOperationsTool
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
public class BatchPerspectiveOperationsTool
{
  private readonly ILogger<BatchPerspectiveOperationsTool> _logger;
  public static readonly ToolMetadata toolMetadata;

  public BatchPerspectiveOperationsTool(ILogger<BatchPerspectiveOperationsTool> logger)
  {
    this._logger = logger;
  }

  [McpServerTool(Name = "batch_perspective_operations")]
  [Description("Perform batch operations on perspective members (tables, columns, measures, hierarchies). Supported operations: BatchAddTables/BatchUpdateTables/BatchRemoveTables/BatchGetTables, BatchAddColumns/BatchRemoveColumns/BatchGetColumns, BatchAddMeasures/BatchRemoveMeasures/BatchGetMeasures, BatchAddHierarchies/BatchRemoveHierarchies/BatchGetHierarchies, Help. Use the Operation parameter to specify which batch operation to perform.")]
  public BatchOperationResponse ExecuteBatchPerspectiveOperation(
    McpServer mcpServer,
    BatchPerspectiveOperationRequest request)
  {
    int num = request.BatchAddPerspectiveTablesRequest?.Items?.Count ?? request.BatchUpdatePerspectiveTablesRequest?.Items?.Count ?? request.BatchRemovePerspectiveTablesRequest?.Items?.Count ?? request.BatchGetPerspectiveTablesRequest?.Items?.Count ?? request.BatchAddPerspectiveColumnsRequest?.Items?.Count ?? request.BatchRemovePerspectiveColumnsRequest?.Items?.Count ?? request.BatchGetPerspectiveColumnsRequest?.Items?.Count ?? request.BatchAddPerspectiveMeasuresRequest?.Items?.Count ?? request.BatchRemovePerspectiveMeasuresRequest?.Items?.Count ?? request.BatchGetPerspectiveMeasuresRequest?.Items?.Count ?? request.BatchAddPerspectiveHierarchiesRequest?.Items?.Count ?? request.BatchRemovePerspectiveHierarchiesRequest?.Items?.Count ?? request.BatchGetPerspectiveHierarchiesRequest?.Items?.Count ?? 0;
    this._logger.LogDebug("Executing {ToolName}.{Operation}: ItemCount={ItemCount}, Connection={ConnectionName}", (object) nameof (BatchPerspectiveOperationsTool), (object) request.Operation, (object) num, (object) (request.ConnectionName ?? "(last used)"));
    try
    {
      string[] strArray1 = new string[14]
      {
        "HELP",
        "BATCHADDTABLES",
        "BATCHUPDATETABLES",
        "BATCHREMOVETABLES",
        "BATCHGETTABLES",
        "BATCHADDCOLUMNS",
        "BATCHREMOVECOLUMNS",
        "BATCHGETCOLUMNS",
        "BATCHADDMEASURES",
        "BATCHREMOVEMEASURES",
        "BATCHGETMEASURES",
        "BATCHADDHIERARCHIES",
        "BATCHREMOVEHIERARCHIES",
        "BATCHGETHIERARCHIES"
      };
      string[] strArray2 = new string[9]
      {
        "BATCHADDTABLES",
        "BATCHUPDATETABLES",
        "BATCHREMOVETABLES",
        "BATCHADDCOLUMNS",
        "BATCHREMOVECOLUMNS",
        "BATCHADDMEASURES",
        "BATCHREMOVEMEASURES",
        "BATCHADDHIERARCHIES",
        "BATCHREMOVEHIERARCHIES"
      };
      string upperInvariant = request.Operation.ToUpperInvariant();
      if (!Enumerable.Contains<string>((IEnumerable<string>) strArray1, upperInvariant))
      {
        this._logger.LogWarning("Invalid operation '{Operation}' requested for {ToolName}. Valid operations: {ValidOperations}", (object) request.Operation, (object) nameof (BatchPerspectiveOperationsTool), (object) string.Join(", ", strArray1));
        return new BatchOperationResponse()
        {
          Success = false,
          Message = $"Invalid operation: {request.Operation}. Supported operations: {string.Join(", ", strArray1)}",
          Operation = request.Operation
        };
      }
      if (!this.ValidateRequest(request.Operation, request))
        throw new McpException($"Invalid request for {request.Operation} operation.");
      bool allowed = WriteGuard.IsWriteAllowed("").allowed;
      if (Enumerable.Contains<string>((IEnumerable<string>) strArray2, upperInvariant))
      {
        WriteOperationResult writeOperationResult = WriteGuard.ExecuteWriteOperationWithGuards(mcpServer, request.ConnectionName, request.Operation);
        if (!writeOperationResult.Success)
        {
          this._logger.LogWarning("{ToolName}.{Operation} blocked by write guard: {Reason}", (object) nameof (BatchPerspectiveOperationsTool), (object) request.Operation, (object) writeOperationResult.Message);
          return new BatchOperationResponse()
          {
            Success = false,
            Message = writeOperationResult.Message,
            Operation = request.Operation
          };
        }
      }
      BatchOperationResponse operationResponse;
      if (upperInvariant != null)
      {
        switch (upperInvariant.Length)
        {
          case 4:
            if ((upperInvariant == "HELP"))
            {
              operationResponse = this.HandleHelpOperation(request, allowed ? strArray1 : Enumerable.ToArray<string>(Enumerable.Except<string>((IEnumerable<string>) strArray1, (IEnumerable<string>) strArray2)));
              goto label_44;
            }
            break;
          case 14:
            switch (upperInvariant[5])
            {
              case 'A':
                if ((upperInvariant == "BATCHADDTABLES"))
                {
                  operationResponse = this.HandleBatchAddTablesOperation(request);
                  goto label_44;
                }
                break;
              case 'G':
                if ((upperInvariant == "BATCHGETTABLES"))
                {
                  operationResponse = this.HandleBatchGetTablesOperation(request);
                  goto label_44;
                }
                break;
            }
            break;
          case 15:
            switch (upperInvariant[5])
            {
              case 'A':
                if ((upperInvariant == "BATCHADDCOLUMNS"))
                {
                  operationResponse = this.HandleBatchAddColumnsOperation(request);
                  goto label_44;
                }
                break;
              case 'G':
                if ((upperInvariant == "BATCHGETCOLUMNS"))
                {
                  operationResponse = this.HandleBatchGetColumnsOperation(request);
                  goto label_44;
                }
                break;
            }
            break;
          case 16 /*0x10*/:
            switch (upperInvariant[5])
            {
              case 'A':
                if ((upperInvariant == "BATCHADDMEASURES"))
                {
                  operationResponse = this.HandleBatchAddMeasuresOperation(request);
                  goto label_44;
                }
                break;
              case 'G':
                if ((upperInvariant == "BATCHGETMEASURES"))
                {
                  operationResponse = this.HandleBatchGetMeasuresOperation(request);
                  goto label_44;
                }
                break;
            }
            break;
          case 17:
            switch (upperInvariant[5])
            {
              case 'R':
                if ((upperInvariant == "BATCHREMOVETABLES"))
                {
                  operationResponse = this.HandleBatchRemoveTablesOperation(request);
                  goto label_44;
                }
                break;
              case 'U':
                if ((upperInvariant == "BATCHUPDATETABLES"))
                {
                  operationResponse = this.HandleBatchUpdateTablesOperation(request);
                  goto label_44;
                }
                break;
            }
            break;
          case 18:
            if ((upperInvariant == "BATCHREMOVECOLUMNS"))
            {
              operationResponse = this.HandleBatchRemoveColumnsOperation(request);
              goto label_44;
            }
            break;
          case 19:
            switch (upperInvariant[5])
            {
              case 'A':
                if ((upperInvariant == "BATCHADDHIERARCHIES"))
                {
                  operationResponse = this.HandleBatchAddHierarchiesOperation(request);
                  goto label_44;
                }
                break;
              case 'G':
                if ((upperInvariant == "BATCHGETHIERARCHIES"))
                {
                  operationResponse = this.HandleBatchGetHierarchiesOperation(request);
                  goto label_44;
                }
                break;
              case 'R':
                if ((upperInvariant == "BATCHREMOVEMEASURES"))
                {
                  operationResponse = this.HandleBatchRemoveMeasuresOperation(request);
                  goto label_44;
                }
                break;
            }
            break;
          case 22:
            if ((upperInvariant == "BATCHREMOVEHIERARCHIES"))
            {
              operationResponse = this.HandleBatchRemoveHierarchiesOperation(request);
              goto label_44;
            }
            break;
        }
      }
      operationResponse = new BatchOperationResponse()
      {
        Success = false,
        Message = $"Operation {request.Operation} is not implemented",
        Operation = request.Operation
      };
label_44:
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Error executing {ToolName}.{Operation}: {ErrorMessage}", (object) nameof (BatchPerspectiveOperationsTool), (object) request.Operation, (object) ex.Message);
      return new BatchOperationResponse()
      {
        Success = false,
        Message = "Error executing batch perspective operation: " + ex.Message,
        Operation = request.Operation
      };
    }
  }

  private BatchOperationResponse HandleHelpOperation(
    BatchPerspectiveOperationRequest request,
    string[] operations)
  {
    try
    {
      List<object> objectList1 = new List<object>();
      foreach (string operation in operations)
      {
        if (BatchPerspectiveOperationsTool.toolMetadata.Operations.ContainsKey(operation))
          objectList1.Add((object) new
          {
            Operation = operation,
            Description = BatchPerspectiveOperationsTool.toolMetadata.Operations[operation]
          });
      }
      BatchOperationResponse operationResponse = new BatchOperationResponse()
      {
        Success = true,
        Operation = request.Operation,
        Message = "Tool description and batch operation metadata"
      };
      List<ItemResult> results = operationResponse.Results;
      ItemResult itemResult = new ItemResult { Index = 0 };
      itemResult.Success = true;
      itemResult.Message = "Batch perspective operations tool metadata";
      List<string> list = Enumerable.ToList<string>((IEnumerable<string>) operations);
      List<object> objectList2 = objectList1;
      var data = new
      {
        ContinueOnError = "Boolean: Whether to continue processing remaining items when an error occurs (default: false)",
        UseTransaction = "Boolean: Whether to wrap the batch operation in a transaction for atomicity (default: true)"
      };
      List<string> stringList = new List<string>();
      stringList.Add("Each batch operation targets exactly one perspective, specified in the PerspectiveName property");
      stringList.Add("Structured identifiers are used for remove/get operations with TableName + specific name properties");
      stringList.Add("Transaction support ensures all operations succeed or all fail together when UseTransaction is true");
      stringList.Add("ContinueOnError allows processing to continue even if individual items fail");
      stringList.Add("Remove operations are destructive and require explicit confirmation");
      itemResult.Data = (object) new
      {
        Tool = "batch_perspective_operations",
        Description = "Perform batch operations on perspective members (tables, columns, measures, hierarchies)",
        SupportedOperations = list,
        OperationDetails = objectList2,
        BatchOptions = data,
        Notes = stringList
      };
      results.Add(itemResult);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Operations={OperationCount}", (object) nameof (BatchPerspectiveOperationsTool), (object) request.Operation, (object) operations.Length);
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      return new BatchOperationResponse()
      {
        Success = false,
        Message = "Error executing Help operation: " + ex.Message,
        Operation = request.Operation
      };
    }
  }

  private BatchOperationResponse HandleBatchAddTablesOperation(
    BatchPerspectiveOperationRequest request)
  {
    try
    {
      if (string.IsNullOrEmpty(request.BatchAddPerspectiveTablesRequest.ConnectionName) && !string.IsNullOrEmpty(request.ConnectionName))
        request.BatchAddPerspectiveTablesRequest.ConnectionName = request.ConnectionName;
      BatchOperationResponse operationResponse = BatchPerspectiveOperations.BatchAddPerspectiveTables(request.BatchAddPerspectiveTablesRequest);
      int num1 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) operationResponse.Results, (r => r.Success));
      int num2 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) operationResponse.Results, (r => !r.Success));
      if (operationResponse.Success)
        this._logger.LogInformation("{ToolName}.{Operation} completed: Total={Total}, Succeeded={Succeeded}, Failed={Failed}", (object) nameof (BatchPerspectiveOperationsTool), (object) request.Operation, (object) operationResponse.Results.Count, (object) num1, (object) num2);
      else
        this._logger.LogWarning("{ToolName}.{Operation} completed with errors: Total={Total}, Succeeded={Succeeded}, Failed={Failed}, Message={Message}", (object) nameof (BatchPerspectiveOperationsTool), (object) request.Operation, (object) operationResponse.Results.Count, (object) num1, (object) num2, (object) operationResponse.Message);
      if (operationResponse.Warnings != null && Enumerable.Any<string>((IEnumerable<string>) operationResponse.Warnings))
      {
        foreach (string warning in operationResponse.Warnings)
          this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (BatchPerspectiveOperationsTool), (object) request.Operation, (object) warning);
      }
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      return new BatchOperationResponse()
      {
        Success = false,
        Message = "Error in BatchAddTables operation: " + ex.Message,
        Operation = request.Operation
      };
    }
  }

  private BatchOperationResponse HandleBatchUpdateTablesOperation(
    BatchPerspectiveOperationRequest request)
  {
    try
    {
      if (string.IsNullOrEmpty(request.BatchUpdatePerspectiveTablesRequest.ConnectionName) && !string.IsNullOrEmpty(request.ConnectionName))
        request.BatchUpdatePerspectiveTablesRequest.ConnectionName = request.ConnectionName;
      BatchOperationResponse operationResponse = BatchPerspectiveOperations.BatchUpdatePerspectiveTables(request.BatchUpdatePerspectiveTablesRequest);
      int num1 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) operationResponse.Results, (r => r.Success));
      int num2 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) operationResponse.Results, (r => !r.Success));
      if (operationResponse.Success)
        this._logger.LogInformation("{ToolName}.{Operation} completed: Total={Total}, Succeeded={Succeeded}, Failed={Failed}", (object) nameof (BatchPerspectiveOperationsTool), (object) request.Operation, (object) operationResponse.Results.Count, (object) num1, (object) num2);
      else
        this._logger.LogWarning("{ToolName}.{Operation} completed with errors: Total={Total}, Succeeded={Succeeded}, Failed={Failed}, Message={Message}", (object) nameof (BatchPerspectiveOperationsTool), (object) request.Operation, (object) operationResponse.Results.Count, (object) num1, (object) num2, (object) operationResponse.Message);
      if (operationResponse.Warnings != null && Enumerable.Any<string>((IEnumerable<string>) operationResponse.Warnings))
      {
        foreach (string warning in operationResponse.Warnings)
          this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (BatchPerspectiveOperationsTool), (object) request.Operation, (object) warning);
      }
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      return new BatchOperationResponse()
      {
        Success = false,
        Message = "Error in BatchUpdateTables operation: " + ex.Message,
        Operation = request.Operation
      };
    }
  }

  private BatchOperationResponse HandleBatchRemoveTablesOperation(
    BatchPerspectiveOperationRequest request)
  {
    try
    {
      if (string.IsNullOrEmpty(request.BatchRemovePerspectiveTablesRequest.ConnectionName) && !string.IsNullOrEmpty(request.ConnectionName))
        request.BatchRemovePerspectiveTablesRequest.ConnectionName = request.ConnectionName;
      BatchOperationResponse operationResponse = BatchPerspectiveOperations.BatchRemovePerspectiveTables(request.BatchRemovePerspectiveTablesRequest);
      int num1 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) operationResponse.Results, (r => r.Success));
      int num2 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) operationResponse.Results, (r => !r.Success));
      if (operationResponse.Success)
        this._logger.LogInformation("{ToolName}.{Operation} completed: Total={Total}, Succeeded={Succeeded}, Failed={Failed}", (object) nameof (BatchPerspectiveOperationsTool), (object) request.Operation, (object) operationResponse.Results.Count, (object) num1, (object) num2);
      else
        this._logger.LogWarning("{ToolName}.{Operation} completed with errors: Total={Total}, Succeeded={Succeeded}, Failed={Failed}, Message={Message}", (object) nameof (BatchPerspectiveOperationsTool), (object) request.Operation, (object) operationResponse.Results.Count, (object) num1, (object) num2, (object) operationResponse.Message);
      if (operationResponse.Warnings != null && Enumerable.Any<string>((IEnumerable<string>) operationResponse.Warnings))
      {
        foreach (string warning in operationResponse.Warnings)
          this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (BatchPerspectiveOperationsTool), (object) request.Operation, (object) warning);
      }
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      return new BatchOperationResponse()
      {
        Success = false,
        Message = "Error in BatchRemoveTables operation: " + ex.Message,
        Operation = request.Operation
      };
    }
  }

  private BatchOperationResponse HandleBatchGetTablesOperation(
    BatchPerspectiveOperationRequest request)
  {
    try
    {
      if (string.IsNullOrEmpty(request.BatchGetPerspectiveTablesRequest.ConnectionName) && !string.IsNullOrEmpty(request.ConnectionName))
        request.BatchGetPerspectiveTablesRequest.ConnectionName = request.ConnectionName;
      BatchOperationResponse perspectiveTables = BatchPerspectiveOperations.BatchGetPerspectiveTables(request.BatchGetPerspectiveTablesRequest);
      int num1 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) perspectiveTables.Results, (r => r.Success));
      int num2 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) perspectiveTables.Results, (r => !r.Success));
      if (perspectiveTables.Success)
        this._logger.LogInformation("{ToolName}.{Operation} completed: Total={Total}, Succeeded={Succeeded}, Failed={Failed}", (object) nameof (BatchPerspectiveOperationsTool), (object) request.Operation, (object) perspectiveTables.Results.Count, (object) num1, (object) num2);
      else
        this._logger.LogWarning("{ToolName}.{Operation} completed with errors: Total={Total}, Succeeded={Succeeded}, Failed={Failed}, Message={Message}", (object) nameof (BatchPerspectiveOperationsTool), (object) request.Operation, (object) perspectiveTables.Results.Count, (object) num1, (object) num2, (object) perspectiveTables.Message);
      if (perspectiveTables.Warnings != null && Enumerable.Any<string>((IEnumerable<string>) perspectiveTables.Warnings))
      {
        foreach (string warning in perspectiveTables.Warnings)
          this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (BatchPerspectiveOperationsTool), (object) request.Operation, (object) warning);
      }
      return perspectiveTables;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      return new BatchOperationResponse()
      {
        Success = false,
        Message = "Error in BatchGetTables operation: " + ex.Message,
        Operation = request.Operation
      };
    }
  }

  private BatchOperationResponse HandleBatchAddColumnsOperation(
    BatchPerspectiveOperationRequest request)
  {
    try
    {
      if (string.IsNullOrEmpty(request.BatchAddPerspectiveColumnsRequest.ConnectionName) && !string.IsNullOrEmpty(request.ConnectionName))
        request.BatchAddPerspectiveColumnsRequest.ConnectionName = request.ConnectionName;
      BatchOperationResponse operationResponse = BatchPerspectiveOperations.BatchAddPerspectiveColumns(request.BatchAddPerspectiveColumnsRequest);
      int num1 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) operationResponse.Results, (r => r.Success));
      int num2 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) operationResponse.Results, (r => !r.Success));
      if (operationResponse.Success)
        this._logger.LogInformation("{ToolName}.{Operation} completed: Total={Total}, Succeeded={Succeeded}, Failed={Failed}", (object) nameof (BatchPerspectiveOperationsTool), (object) request.Operation, (object) operationResponse.Results.Count, (object) num1, (object) num2);
      else
        this._logger.LogWarning("{ToolName}.{Operation} completed with errors: Total={Total}, Succeeded={Succeeded}, Failed={Failed}, Message={Message}", (object) nameof (BatchPerspectiveOperationsTool), (object) request.Operation, (object) operationResponse.Results.Count, (object) num1, (object) num2, (object) operationResponse.Message);
      if (operationResponse.Warnings != null && Enumerable.Any<string>((IEnumerable<string>) operationResponse.Warnings))
      {
        foreach (string warning in operationResponse.Warnings)
          this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (BatchPerspectiveOperationsTool), (object) request.Operation, (object) warning);
      }
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      return new BatchOperationResponse()
      {
        Success = false,
        Message = "Error in BatchAddColumns operation: " + ex.Message,
        Operation = request.Operation
      };
    }
  }

  private BatchOperationResponse HandleBatchRemoveColumnsOperation(
    BatchPerspectiveOperationRequest request)
  {
    try
    {
      if (string.IsNullOrEmpty(request.BatchRemovePerspectiveColumnsRequest.ConnectionName) && !string.IsNullOrEmpty(request.ConnectionName))
        request.BatchRemovePerspectiveColumnsRequest.ConnectionName = request.ConnectionName;
      BatchOperationResponse operationResponse = BatchPerspectiveOperations.BatchRemovePerspectiveColumns(request.BatchRemovePerspectiveColumnsRequest);
      int num1 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) operationResponse.Results, (r => r.Success));
      int num2 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) operationResponse.Results, (r => !r.Success));
      if (operationResponse.Success)
        this._logger.LogInformation("{ToolName}.{Operation} completed: Total={Total}, Succeeded={Succeeded}, Failed={Failed}", (object) nameof (BatchPerspectiveOperationsTool), (object) request.Operation, (object) operationResponse.Results.Count, (object) num1, (object) num2);
      else
        this._logger.LogWarning("{ToolName}.{Operation} completed with errors: Total={Total}, Succeeded={Succeeded}, Failed={Failed}, Message={Message}", (object) nameof (BatchPerspectiveOperationsTool), (object) request.Operation, (object) operationResponse.Results.Count, (object) num1, (object) num2, (object) operationResponse.Message);
      if (operationResponse.Warnings != null && Enumerable.Any<string>((IEnumerable<string>) operationResponse.Warnings))
      {
        foreach (string warning in operationResponse.Warnings)
          this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (BatchPerspectiveOperationsTool), (object) request.Operation, (object) warning);
      }
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      return new BatchOperationResponse()
      {
        Success = false,
        Message = "Error in BatchRemoveColumns operation: " + ex.Message,
        Operation = request.Operation
      };
    }
  }

  private BatchOperationResponse HandleBatchGetColumnsOperation(
    BatchPerspectiveOperationRequest request)
  {
    try
    {
      if (string.IsNullOrEmpty(request.BatchGetPerspectiveColumnsRequest.ConnectionName) && !string.IsNullOrEmpty(request.ConnectionName))
        request.BatchGetPerspectiveColumnsRequest.ConnectionName = request.ConnectionName;
      BatchOperationResponse perspectiveColumns = BatchPerspectiveOperations.BatchGetPerspectiveColumns(request.BatchGetPerspectiveColumnsRequest);
      int num1 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) perspectiveColumns.Results, (r => r.Success));
      int num2 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) perspectiveColumns.Results, (r => !r.Success));
      if (perspectiveColumns.Success)
        this._logger.LogInformation("{ToolName}.{Operation} completed: Total={Total}, Succeeded={Succeeded}, Failed={Failed}", (object) nameof (BatchPerspectiveOperationsTool), (object) request.Operation, (object) perspectiveColumns.Results.Count, (object) num1, (object) num2);
      else
        this._logger.LogWarning("{ToolName}.{Operation} completed with errors: Total={Total}, Succeeded={Succeeded}, Failed={Failed}, Message={Message}", (object) nameof (BatchPerspectiveOperationsTool), (object) request.Operation, (object) perspectiveColumns.Results.Count, (object) num1, (object) num2, (object) perspectiveColumns.Message);
      if (perspectiveColumns.Warnings != null && Enumerable.Any<string>((IEnumerable<string>) perspectiveColumns.Warnings))
      {
        foreach (string warning in perspectiveColumns.Warnings)
          this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (BatchPerspectiveOperationsTool), (object) request.Operation, (object) warning);
      }
      return perspectiveColumns;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      return new BatchOperationResponse()
      {
        Success = false,
        Message = "Error in BatchGetColumns operation: " + ex.Message,
        Operation = request.Operation
      };
    }
  }

  private BatchOperationResponse HandleBatchAddMeasuresOperation(
    BatchPerspectiveOperationRequest request)
  {
    try
    {
      if (string.IsNullOrEmpty(request.BatchAddPerspectiveMeasuresRequest.ConnectionName) && !string.IsNullOrEmpty(request.ConnectionName))
        request.BatchAddPerspectiveMeasuresRequest.ConnectionName = request.ConnectionName;
      BatchOperationResponse operationResponse = BatchPerspectiveOperations.BatchAddPerspectiveMeasures(request.BatchAddPerspectiveMeasuresRequest);
      int num1 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) operationResponse.Results, (r => r.Success));
      int num2 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) operationResponse.Results, (r => !r.Success));
      if (operationResponse.Success)
        this._logger.LogInformation("{ToolName}.{Operation} completed: Total={Total}, Succeeded={Succeeded}, Failed={Failed}", (object) nameof (BatchPerspectiveOperationsTool), (object) request.Operation, (object) operationResponse.Results.Count, (object) num1, (object) num2);
      else
        this._logger.LogWarning("{ToolName}.{Operation} completed with errors: Total={Total}, Succeeded={Succeeded}, Failed={Failed}, Message={Message}", (object) nameof (BatchPerspectiveOperationsTool), (object) request.Operation, (object) operationResponse.Results.Count, (object) num1, (object) num2, (object) operationResponse.Message);
      if (operationResponse.Warnings != null && Enumerable.Any<string>((IEnumerable<string>) operationResponse.Warnings))
      {
        foreach (string warning in operationResponse.Warnings)
          this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (BatchPerspectiveOperationsTool), (object) request.Operation, (object) warning);
      }
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      return new BatchOperationResponse()
      {
        Success = false,
        Message = "Error in BatchAddMeasures operation: " + ex.Message,
        Operation = request.Operation
      };
    }
  }

  private BatchOperationResponse HandleBatchRemoveMeasuresOperation(
    BatchPerspectiveOperationRequest request)
  {
    try
    {
      if (string.IsNullOrEmpty(request.BatchRemovePerspectiveMeasuresRequest.ConnectionName) && !string.IsNullOrEmpty(request.ConnectionName))
        request.BatchRemovePerspectiveMeasuresRequest.ConnectionName = request.ConnectionName;
      BatchOperationResponse operationResponse = BatchPerspectiveOperations.BatchRemovePerspectiveMeasures(request.BatchRemovePerspectiveMeasuresRequest);
      int num1 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) operationResponse.Results, (r => r.Success));
      int num2 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) operationResponse.Results, (r => !r.Success));
      if (operationResponse.Success)
        this._logger.LogInformation("{ToolName}.{Operation} completed: Total={Total}, Succeeded={Succeeded}, Failed={Failed}", (object) nameof (BatchPerspectiveOperationsTool), (object) request.Operation, (object) operationResponse.Results.Count, (object) num1, (object) num2);
      else
        this._logger.LogWarning("{ToolName}.{Operation} completed with errors: Total={Total}, Succeeded={Succeeded}, Failed={Failed}, Message={Message}", (object) nameof (BatchPerspectiveOperationsTool), (object) request.Operation, (object) operationResponse.Results.Count, (object) num1, (object) num2, (object) operationResponse.Message);
      if (operationResponse.Warnings != null && Enumerable.Any<string>((IEnumerable<string>) operationResponse.Warnings))
      {
        foreach (string warning in operationResponse.Warnings)
          this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (BatchPerspectiveOperationsTool), (object) request.Operation, (object) warning);
      }
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      return new BatchOperationResponse()
      {
        Success = false,
        Message = "Error in BatchRemoveMeasures operation: " + ex.Message,
        Operation = request.Operation
      };
    }
  }

  private BatchOperationResponse HandleBatchGetMeasuresOperation(
    BatchPerspectiveOperationRequest request)
  {
    try
    {
      if (string.IsNullOrEmpty(request.BatchGetPerspectiveMeasuresRequest.ConnectionName) && !string.IsNullOrEmpty(request.ConnectionName))
        request.BatchGetPerspectiveMeasuresRequest.ConnectionName = request.ConnectionName;
      BatchOperationResponse perspectiveMeasures = BatchPerspectiveOperations.BatchGetPerspectiveMeasures(request.BatchGetPerspectiveMeasuresRequest);
      int num1 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) perspectiveMeasures.Results, (r => r.Success));
      int num2 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) perspectiveMeasures.Results, (r => !r.Success));
      if (perspectiveMeasures.Success)
        this._logger.LogInformation("{ToolName}.{Operation} completed: Total={Total}, Succeeded={Succeeded}, Failed={Failed}", (object) nameof (BatchPerspectiveOperationsTool), (object) request.Operation, (object) perspectiveMeasures.Results.Count, (object) num1, (object) num2);
      else
        this._logger.LogWarning("{ToolName}.{Operation} completed with errors: Total={Total}, Succeeded={Succeeded}, Failed={Failed}, Message={Message}", (object) nameof (BatchPerspectiveOperationsTool), (object) request.Operation, (object) perspectiveMeasures.Results.Count, (object) num1, (object) num2, (object) perspectiveMeasures.Message);
      if (perspectiveMeasures.Warnings != null && Enumerable.Any<string>((IEnumerable<string>) perspectiveMeasures.Warnings))
      {
        foreach (string warning in perspectiveMeasures.Warnings)
          this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (BatchPerspectiveOperationsTool), (object) request.Operation, (object) warning);
      }
      return perspectiveMeasures;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      return new BatchOperationResponse()
      {
        Success = false,
        Message = "Error in BatchGetMeasures operation: " + ex.Message,
        Operation = request.Operation
      };
    }
  }

  private BatchOperationResponse HandleBatchAddHierarchiesOperation(
    BatchPerspectiveOperationRequest request)
  {
    try
    {
      if (string.IsNullOrEmpty(request.BatchAddPerspectiveHierarchiesRequest.ConnectionName) && !string.IsNullOrEmpty(request.ConnectionName))
        request.BatchAddPerspectiveHierarchiesRequest.ConnectionName = request.ConnectionName;
      BatchOperationResponse operationResponse = BatchPerspectiveOperations.BatchAddPerspectiveHierarchies(request.BatchAddPerspectiveHierarchiesRequest);
      int num1 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) operationResponse.Results, (r => r.Success));
      int num2 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) operationResponse.Results, (r => !r.Success));
      if (operationResponse.Success)
        this._logger.LogInformation("{ToolName}.{Operation} completed: Total={Total}, Succeeded={Succeeded}, Failed={Failed}", (object) nameof (BatchPerspectiveOperationsTool), (object) request.Operation, (object) operationResponse.Results.Count, (object) num1, (object) num2);
      else
        this._logger.LogWarning("{ToolName}.{Operation} completed with errors: Total={Total}, Succeeded={Succeeded}, Failed={Failed}, Message={Message}", (object) nameof (BatchPerspectiveOperationsTool), (object) request.Operation, (object) operationResponse.Results.Count, (object) num1, (object) num2, (object) operationResponse.Message);
      if (operationResponse.Warnings != null && Enumerable.Any<string>((IEnumerable<string>) operationResponse.Warnings))
      {
        foreach (string warning in operationResponse.Warnings)
          this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (BatchPerspectiveOperationsTool), (object) request.Operation, (object) warning);
      }
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      return new BatchOperationResponse()
      {
        Success = false,
        Message = "Error in BatchAddHierarchies operation: " + ex.Message,
        Operation = request.Operation
      };
    }
  }

  private BatchOperationResponse HandleBatchRemoveHierarchiesOperation(
    BatchPerspectiveOperationRequest request)
  {
    try
    {
      if (string.IsNullOrEmpty(request.BatchRemovePerspectiveHierarchiesRequest.ConnectionName) && !string.IsNullOrEmpty(request.ConnectionName))
        request.BatchRemovePerspectiveHierarchiesRequest.ConnectionName = request.ConnectionName;
      BatchOperationResponse operationResponse = BatchPerspectiveOperations.BatchRemovePerspectiveHierarchies(request.BatchRemovePerspectiveHierarchiesRequest);
      int num1 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) operationResponse.Results, (r => r.Success));
      int num2 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) operationResponse.Results, (r => !r.Success));
      if (operationResponse.Success)
        this._logger.LogInformation("{ToolName}.{Operation} completed: Total={Total}, Succeeded={Succeeded}, Failed={Failed}", (object) nameof (BatchPerspectiveOperationsTool), (object) request.Operation, (object) operationResponse.Results.Count, (object) num1, (object) num2);
      else
        this._logger.LogWarning("{ToolName}.{Operation} completed with errors: Total={Total}, Succeeded={Succeeded}, Failed={Failed}, Message={Message}", (object) nameof (BatchPerspectiveOperationsTool), (object) request.Operation, (object) operationResponse.Results.Count, (object) num1, (object) num2, (object) operationResponse.Message);
      if (operationResponse.Warnings != null && Enumerable.Any<string>((IEnumerable<string>) operationResponse.Warnings))
      {
        foreach (string warning in operationResponse.Warnings)
          this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (BatchPerspectiveOperationsTool), (object) request.Operation, (object) warning);
      }
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      return new BatchOperationResponse()
      {
        Success = false,
        Message = "Error in BatchRemoveHierarchies operation: " + ex.Message,
        Operation = request.Operation
      };
    }
  }

  private BatchOperationResponse HandleBatchGetHierarchiesOperation(
    BatchPerspectiveOperationRequest request)
  {
    try
    {
      if (string.IsNullOrEmpty(request.BatchGetPerspectiveHierarchiesRequest.ConnectionName) && !string.IsNullOrEmpty(request.ConnectionName))
        request.BatchGetPerspectiveHierarchiesRequest.ConnectionName = request.ConnectionName;
      BatchOperationResponse perspectiveHierarchies = BatchPerspectiveOperations.BatchGetPerspectiveHierarchies(request.BatchGetPerspectiveHierarchiesRequest);
      int num1 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) perspectiveHierarchies.Results, (r => r.Success));
      int num2 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) perspectiveHierarchies.Results, (r => !r.Success));
      if (perspectiveHierarchies.Success)
        this._logger.LogInformation("{ToolName}.{Operation} completed: Total={Total}, Succeeded={Succeeded}, Failed={Failed}", (object) nameof (BatchPerspectiveOperationsTool), (object) request.Operation, (object) perspectiveHierarchies.Results.Count, (object) num1, (object) num2);
      else
        this._logger.LogWarning("{ToolName}.{Operation} completed with errors: Total={Total}, Succeeded={Succeeded}, Failed={Failed}, Message={Message}", (object) nameof (BatchPerspectiveOperationsTool), (object) request.Operation, (object) perspectiveHierarchies.Results.Count, (object) num1, (object) num2, (object) perspectiveHierarchies.Message);
      if (perspectiveHierarchies.Warnings != null && Enumerable.Any<string>((IEnumerable<string>) perspectiveHierarchies.Warnings))
      {
        foreach (string warning in perspectiveHierarchies.Warnings)
          this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (BatchPerspectiveOperationsTool), (object) request.Operation, (object) warning);
      }
      return perspectiveHierarchies;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      return new BatchOperationResponse()
      {
        Success = false,
        Message = "Error in BatchGetHierarchies operation: " + ex.Message,
        Operation = request.Operation
      };
    }
  }

  private bool ValidateRequest(string operation, BatchPerspectiveOperationRequest request)
  {
    OperationMetadata operationMetadata;
    if (!BatchPerspectiveOperationsTool.toolMetadata.Operations.TryGetValue(operation, out operationMetadata))
      return true;
    JsonObject requestDict = JsonSerializer.SerializeToNode<BatchPerspectiveOperationRequest>(request) as JsonObject;
    List<string> list1 = Enumerable.ToList<string>(Enumerable.Where<string>((IEnumerable<string>) operationMetadata.RequiredParams, (p => requestDict != null && requestDict[p] == null)));
    List<string> list2 = Enumerable.ToList<string>(Enumerable.Where<string>((IEnumerable<string>) operationMetadata.ForbiddenParams, (p => requestDict != null && requestDict[p] != null)));
    if (Enumerable.Any<string>((IEnumerable<string>) list1))
      throw new McpException($"Missing required parameters needed for {operation} operation: {string.Join(", ", (IEnumerable<string>) list1)}");
    if (Enumerable.Any<string>((IEnumerable<string>) list2))
      throw new McpException($"Forbidden parameters not allowed for {operation} operation: {string.Join(", ", (IEnumerable<string>) list2)}");
    return true;
  }

  static BatchPerspectiveOperationsTool()
  {
    ToolMetadata toolMetadata1 = new ToolMetadata();
    ToolMetadata toolMetadata2 = toolMetadata1;
    Dictionary<string, OperationMetadata> dictionary1 = new Dictionary<string, OperationMetadata>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    Dictionary<string, OperationMetadata> dictionary2 = dictionary1;
    OperationMetadata operationMetadata1 = new OperationMetadata { RequiredParams = new string[1]
    {
      "BatchAddPerspectiveTablesRequest"
    } };
    operationMetadata1.Description = "Add multiple tables to a perspective in a single batch operation with optional transaction support. \r\nMandatory properties: BatchAddPerspectiveTablesRequest (with PerspectiveName, Items where each item has TableName). \r\nOptional: IncludeAll for each item, Annotations, ExtendedProperties, Options.";
    OperationMetadata operationMetadata2 = operationMetadata1;
    List<string> stringList1 = new List<string>();
    stringList1.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"BatchAddTables\",\r\n        \"BatchAddPerspectiveTablesRequest\": {\r\n            \"PerspectiveName\": \"Sales\",\r\n            \"Items\": [\r\n                { \"TableName\": \"Orders\", \"IncludeAll\": true },\r\n                { \"TableName\": \"Customers\", \"IncludeAll\": false }\r\n            ],\r\n            \"Options\": { \"ContinueOnError\": true, \"UseTransaction\": true }\r\n        }\r\n    }\r\n}");
    operationMetadata2.ExampleRequests = stringList1;
    OperationMetadata operationMetadata3 = operationMetadata1;
    dictionary2["BatchAddTables"] = operationMetadata3;
    Dictionary<string, OperationMetadata> dictionary3 = dictionary1;
    OperationMetadata operationMetadata4 = new OperationMetadata { RequiredParams = new string[1]
    {
      "BatchUpdatePerspectiveTablesRequest"
    } };
    operationMetadata4.Description = "Update multiple perspective tables in a single batch operation with optional transaction support. \r\nMandatory properties: BatchUpdatePerspectiveTablesRequest (with PerspectiveName, Items where each item has TableName). \r\nOptional: IncludeAll for each item, Annotations, ExtendedProperties, Options.";
    OperationMetadata operationMetadata5 = operationMetadata4;
    List<string> stringList2 = new List<string>();
    stringList2.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"BatchUpdateTables\",\r\n        \"BatchUpdatePerspectiveTablesRequest\": {\r\n            \"PerspectiveName\": \"Sales\",\r\n            \"Items\": [\r\n                { \"TableName\": \"Orders\", \"IncludeAll\": false },\r\n                { \"TableName\": \"Customers\", \"IncludeAll\": true }\r\n            ],\r\n            \"Options\": { \"ContinueOnError\": false, \"UseTransaction\": true }\r\n        }\r\n    }\r\n}");
    operationMetadata5.ExampleRequests = stringList2;
    OperationMetadata operationMetadata6 = operationMetadata4;
    dictionary3["BatchUpdateTables"] = operationMetadata6;
    Dictionary<string, OperationMetadata> dictionary4 = dictionary1;
    OperationMetadata operationMetadata7 = new OperationMetadata { RequiredParams = new string[1]
    {
      "BatchRemovePerspectiveTablesRequest"
    } };
    operationMetadata7.Description = "Remove multiple tables from a perspective in a single batch operation with optional transaction support. \r\nMandatory properties: BatchRemovePerspectiveTablesRequest (with PerspectiveName, Items containing table names). \r\nOptional: Options.";
    OperationMetadata operationMetadata8 = operationMetadata7;
    List<string> stringList3 = new List<string>();
    stringList3.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"BatchRemoveTables\",\r\n        \"BatchRemovePerspectiveTablesRequest\": {\r\n            \"PerspectiveName\": \"Sales\",\r\n            \"Items\": [\"Orders\", \"OldTable\"],\r\n            \"Options\": { \"ContinueOnError\": false, \"UseTransaction\": true }\r\n        }\r\n    }\r\n}");
    operationMetadata8.ExampleRequests = stringList3;
    OperationMetadata operationMetadata9 = operationMetadata7;
    dictionary4["BatchRemoveTables"] = operationMetadata9;
    Dictionary<string, OperationMetadata> dictionary5 = dictionary1;
    OperationMetadata operationMetadata10 = new OperationMetadata { RequiredParams = new string[1]
    {
      "BatchGetPerspectiveTablesRequest"
    } };
    operationMetadata10.Description = "Get detailed information for multiple perspective tables in a single batch operation. \r\nMandatory properties: BatchGetPerspectiveTablesRequest (with PerspectiveName, Items containing table names). \r\nOptional: None.";
    OperationMetadata operationMetadata11 = operationMetadata10;
    List<string> stringList4 = new List<string>();
    stringList4.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"BatchGetTables\",\r\n        \"BatchGetPerspectiveTablesRequest\": {\r\n            \"PerspectiveName\": \"Sales\",\r\n            \"Items\": [\"Orders\", \"Customers\"]\r\n        }\r\n    }\r\n}");
    operationMetadata11.ExampleRequests = stringList4;
    OperationMetadata operationMetadata12 = operationMetadata10;
    dictionary5["BatchGetTables"] = operationMetadata12;
    Dictionary<string, OperationMetadata> dictionary6 = dictionary1;
    OperationMetadata operationMetadata13 = new OperationMetadata { RequiredParams = new string[1]
    {
      "BatchAddPerspectiveColumnsRequest"
    } };
    operationMetadata13.Description = "Add multiple columns to perspective tables in a single batch operation with optional transaction support. \r\nMandatory properties: BatchAddPerspectiveColumnsRequest (with PerspectiveName, Items where each item has TableName, ColumnName). \r\nOptional: Annotations, ExtendedProperties for each item, Options.";
    OperationMetadata operationMetadata14 = operationMetadata13;
    List<string> stringList5 = new List<string>();
    stringList5.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"BatchAddColumns\",\r\n        \"BatchAddPerspectiveColumnsRequest\": {\r\n            \"PerspectiveName\": \"Sales\",\r\n            \"Items\": [\r\n                { \"TableName\": \"Orders\", \"ColumnName\": \"OrderDate\" },\r\n                { \"TableName\": \"Orders\", \"ColumnName\": \"TotalAmount\" }\r\n            ],\r\n            \"Options\": { \"ContinueOnError\": true, \"UseTransaction\": false }\r\n        }\r\n    }\r\n}");
    operationMetadata14.ExampleRequests = stringList5;
    OperationMetadata operationMetadata15 = operationMetadata13;
    dictionary6["BatchAddColumns"] = operationMetadata15;
    Dictionary<string, OperationMetadata> dictionary7 = dictionary1;
    OperationMetadata operationMetadata16 = new OperationMetadata { RequiredParams = new string[1]
    {
      "BatchRemovePerspectiveColumnsRequest"
    } };
    operationMetadata16.Description = "Remove multiple columns from perspective tables in a single batch operation with optional transaction support. \r\nMandatory properties: BatchRemovePerspectiveColumnsRequest (with PerspectiveName, Items where each item has TableName, ColumnName). \r\nOptional: Options.";
    OperationMetadata operationMetadata17 = operationMetadata16;
    List<string> stringList6 = new List<string>();
    stringList6.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"BatchRemoveColumns\",\r\n        \"BatchRemovePerspectiveColumnsRequest\": {\r\n            \"PerspectiveName\": \"Sales\",\r\n            \"Items\": [\r\n                { \"TableName\": \"Orders\", \"ColumnName\": \"OrderDate\" },\r\n                { \"TableName\": \"Orders\", \"ColumnName\": \"TotalAmount\" }\r\n            ],\r\n            \"Options\": { \"ContinueOnError\": false, \"UseTransaction\": true }\r\n        }\r\n    }\r\n}");
    operationMetadata17.ExampleRequests = stringList6;
    OperationMetadata operationMetadata18 = operationMetadata16;
    dictionary7["BatchRemoveColumns"] = operationMetadata18;
    Dictionary<string, OperationMetadata> dictionary8 = dictionary1;
    OperationMetadata operationMetadata19 = new OperationMetadata { RequiredParams = new string[1]
    {
      "BatchGetPerspectiveColumnsRequest"
    } };
    operationMetadata19.Description = "Get detailed information for multiple perspective columns in a single batch operation. \r\nMandatory properties: BatchGetPerspectiveColumnsRequest (with PerspectiveName, Items where each item has TableName, ColumnName). \r\nOptional: None.";
    OperationMetadata operationMetadata20 = operationMetadata19;
    List<string> stringList7 = new List<string>();
    stringList7.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"BatchGetColumns\",\r\n        \"BatchGetPerspectiveColumnsRequest\": {\r\n            \"PerspectiveName\": \"Sales\",\r\n            \"Items\": [\r\n                { \"TableName\": \"Orders\", \"ColumnName\": \"OrderDate\" },\r\n                { \"TableName\": \"Orders\", \"ColumnName\": \"TotalAmount\" }\r\n            ]\r\n        }\r\n    }\r\n}");
    operationMetadata20.ExampleRequests = stringList7;
    OperationMetadata operationMetadata21 = operationMetadata19;
    dictionary8["BatchGetColumns"] = operationMetadata21;
    Dictionary<string, OperationMetadata> dictionary9 = dictionary1;
    OperationMetadata operationMetadata22 = new OperationMetadata { RequiredParams = new string[1]
    {
      "BatchAddPerspectiveMeasuresRequest"
    } };
    operationMetadata22.Description = "Add multiple measures to perspective tables in a single batch operation with optional transaction support. \r\nMandatory properties: BatchAddPerspectiveMeasuresRequest (with PerspectiveName, Items where each item has TableName, MeasureName). \r\nOptional: Annotations, ExtendedProperties for each item, Options.";
    OperationMetadata operationMetadata23 = operationMetadata22;
    List<string> stringList8 = new List<string>();
    stringList8.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"BatchAddMeasures\",\r\n        \"BatchAddPerspectiveMeasuresRequest\": {\r\n            \"PerspectiveName\": \"Sales\",\r\n            \"Items\": [\r\n                { \"TableName\": \"FactSales\", \"MeasureName\": \"TotalSales\" },\r\n                { \"TableName\": \"FactSales\", \"MeasureName\": \"AvgSales\" }\r\n            ],\r\n            \"Options\": { \"ContinueOnError\": true, \"UseTransaction\": true }\r\n        }\r\n    }\r\n}");
    operationMetadata23.ExampleRequests = stringList8;
    OperationMetadata operationMetadata24 = operationMetadata22;
    dictionary9["BatchAddMeasures"] = operationMetadata24;
    Dictionary<string, OperationMetadata> dictionary10 = dictionary1;
    OperationMetadata operationMetadata25 = new OperationMetadata { RequiredParams = new string[1]
    {
      "BatchRemovePerspectiveMeasuresRequest"
    } };
    operationMetadata25.Description = "Remove multiple measures from perspective tables in a single batch operation with optional transaction support. \r\nMandatory properties: BatchRemovePerspectiveMeasuresRequest (with PerspectiveName, Items where each item has TableName, MeasureName). \r\nOptional: Options.";
    OperationMetadata operationMetadata26 = operationMetadata25;
    List<string> stringList9 = new List<string>();
    stringList9.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"BatchRemoveMeasures\",\r\n        \"BatchRemovePerspectiveMeasuresRequest\": {\r\n            \"PerspectiveName\": \"Sales\",\r\n            \"Items\": [\r\n                { \"TableName\": \"FactSales\", \"MeasureName\": \"TotalSales\" },\r\n                { \"TableName\": \"FactSales\", \"MeasureName\": \"AvgSales\" }\r\n            ],\r\n            \"Options\": { \"ContinueOnError\": false, \"UseTransaction\": true }\r\n        }\r\n    }\r\n}");
    operationMetadata26.ExampleRequests = stringList9;
    OperationMetadata operationMetadata27 = operationMetadata25;
    dictionary10["BatchRemoveMeasures"] = operationMetadata27;
    Dictionary<string, OperationMetadata> dictionary11 = dictionary1;
    OperationMetadata operationMetadata28 = new OperationMetadata { RequiredParams = new string[1]
    {
      "BatchGetPerspectiveMeasuresRequest"
    } };
    operationMetadata28.Description = "Get detailed information for multiple perspective measures in a single batch operation. \r\nMandatory properties: BatchGetPerspectiveMeasuresRequest (with PerspectiveName, Items where each item has TableName, MeasureName). \r\nOptional: None.";
    OperationMetadata operationMetadata29 = operationMetadata28;
    List<string> stringList10 = new List<string>();
    stringList10.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"BatchGetMeasures\",\r\n        \"BatchGetPerspectiveMeasuresRequest\": {\r\n            \"PerspectiveName\": \"Sales\",\r\n            \"Items\": [\r\n                { \"TableName\": \"FactSales\", \"MeasureName\": \"TotalSales\" },\r\n                { \"TableName\": \"FactSales\", \"MeasureName\": \"AvgSales\" }\r\n            ]\r\n        }\r\n    }\r\n}");
    operationMetadata29.ExampleRequests = stringList10;
    OperationMetadata operationMetadata30 = operationMetadata28;
    dictionary11["BatchGetMeasures"] = operationMetadata30;
    Dictionary<string, OperationMetadata> dictionary12 = dictionary1;
    OperationMetadata operationMetadata31 = new OperationMetadata { RequiredParams = new string[1]
    {
      "BatchAddPerspectiveHierarchiesRequest"
    } };
    operationMetadata31.Description = "Add multiple hierarchies to perspective tables in a single batch operation with optional transaction support. \r\nMandatory properties: BatchAddPerspectiveHierarchiesRequest (with PerspectiveName, Items where each item has TableName, HierarchyName). \r\nOptional: Annotations, ExtendedProperties for each item, Options.";
    OperationMetadata operationMetadata32 = operationMetadata31;
    List<string> stringList11 = new List<string>();
    stringList11.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"BatchAddHierarchies\",\r\n        \"BatchAddPerspectiveHierarchiesRequest\": {\r\n            \"PerspectiveName\": \"Sales\",\r\n            \"Items\": [\r\n                { \"TableName\": \"DimDate\", \"HierarchyName\": \"Calendar\" },\r\n                { \"TableName\": \"DimGeography\", \"HierarchyName\": \"Geographic\" }\r\n            ],\r\n            \"Options\": { \"ContinueOnError\": true, \"UseTransaction\": true }\r\n        }\r\n    }\r\n}");
    operationMetadata32.ExampleRequests = stringList11;
    OperationMetadata operationMetadata33 = operationMetadata31;
    dictionary12["BatchAddHierarchies"] = operationMetadata33;
    Dictionary<string, OperationMetadata> dictionary13 = dictionary1;
    OperationMetadata operationMetadata34 = new OperationMetadata { RequiredParams = new string[1]
    {
      "BatchRemovePerspectiveHierarchiesRequest"
    } };
    operationMetadata34.Description = "Remove multiple hierarchies from perspective tables in a single batch operation with optional transaction support. \r\nMandatory properties: BatchRemovePerspectiveHierarchiesRequest (with PerspectiveName, Items where each item has TableName, HierarchyName). \r\nOptional: Options.";
    OperationMetadata operationMetadata35 = operationMetadata34;
    List<string> stringList12 = new List<string>();
    stringList12.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"BatchRemoveHierarchies\",\r\n        \"BatchRemovePerspectiveHierarchiesRequest\": {\r\n            \"PerspectiveName\": \"Sales\",\r\n            \"Items\": [\r\n                { \"TableName\": \"DimDate\", \"HierarchyName\": \"Calendar\" },\r\n                { \"TableName\": \"DimGeography\", \"HierarchyName\": \"Geographic\" }\r\n            ],\r\n            \"Options\": { \"ContinueOnError\": false, \"UseTransaction\": true }\r\n        }\r\n    }\r\n}");
    operationMetadata35.ExampleRequests = stringList12;
    OperationMetadata operationMetadata36 = operationMetadata34;
    dictionary13["BatchRemoveHierarchies"] = operationMetadata36;
    Dictionary<string, OperationMetadata> dictionary14 = dictionary1;
    OperationMetadata operationMetadata37 = new OperationMetadata { RequiredParams = new string[1]
    {
      "BatchGetPerspectiveHierarchiesRequest"
    } };
    operationMetadata37.Description = "Get detailed information for multiple perspective hierarchies in a single batch operation. \r\nMandatory properties: BatchGetPerspectiveHierarchiesRequest (with PerspectiveName, Items where each item has TableName, HierarchyName). \r\nOptional: None.";
    OperationMetadata operationMetadata38 = operationMetadata37;
    List<string> stringList13 = new List<string>();
    stringList13.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"BatchGetHierarchies\",\r\n        \"BatchGetPerspectiveHierarchiesRequest\": {\r\n            \"PerspectiveName\": \"Sales\",\r\n            \"Items\": [\r\n                { \"TableName\": \"DimDate\", \"HierarchyName\": \"Calendar\" },\r\n                { \"TableName\": \"DimGeography\", \"HierarchyName\": \"Geographic\" }\r\n            ]\r\n        }\r\n    }\r\n}");
    operationMetadata38.ExampleRequests = stringList13;
    OperationMetadata operationMetadata39 = operationMetadata37;
    dictionary14["BatchGetHierarchies"] = operationMetadata39;
    Dictionary<string, OperationMetadata> dictionary15 = dictionary1;
    OperationMetadata operationMetadata40 = new OperationMetadata { Description = "Describe the batch_perspective_operations tool and its available batch operations. \r\nMandatory properties: None. \r\nOptional: None." };
    List<string> stringList14 = new List<string>();
    stringList14.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Help\"\r\n    }\r\n}");
    operationMetadata40.ExampleRequests = stringList14;
    dictionary15["Help"] = operationMetadata40;
    Dictionary<string, OperationMetadata> dictionary16 = dictionary1;
    toolMetadata2.Operations = dictionary16;
    BatchPerspectiveOperationsTool.toolMetadata = toolMetadata1;
  }
}
