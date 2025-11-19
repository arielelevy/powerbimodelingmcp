// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Tools.BatchMeasureOperationsTool
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
public class BatchMeasureOperationsTool
{
  private readonly ILogger<BatchMeasureOperationsTool> _logger;
  public static readonly ToolMetadata toolMetadata;

  public BatchMeasureOperationsTool(ILogger<BatchMeasureOperationsTool> logger)
  {
    this._logger = logger;
  }

  [McpServerTool(Name = "batch_measure_operations")]
  [Description("Perform batch operations on semantic model measures. Supported operations: Help, BatchCreate, BatchUpdate, BatchDelete, BatchGet, BatchRename, BatchMove. Use the Operation parameter to specify which batch operation to perform. Each operation includes options for error handling (ContinueOnError) and transactional behavior (UseTransaction).")]
  public BatchOperationResponse ExecuteBatchMeasureOperation(
    McpServer mcpServer,
    BatchMeasureOperationRequest request)
  {
    this._logger.LogDebug("Executing {ToolName}.{Operation}: ItemCount={ItemCount}, Connection={ConnectionName}", (object) nameof (BatchMeasureOperationsTool), (object) request.Operation, (object) (request.BatchCreateRequest?.Items?.Count ?? request.BatchUpdateRequest?.Items?.Count ?? request.BatchDeleteRequest?.Items?.Count ?? request.BatchGetRequest?.Items?.Count ?? request.BatchRenameRequest?.Items?.Count ?? request.BatchMoveRequest?.Items?.Count ?? 0), (object) (request.ConnectionName ?? "(last used)"));
    try
    {
      string[] strArray1 = new string[7]
      {
        "HELP",
        "BATCHCREATE",
        "BATCHUPDATE",
        "BATCHDELETE",
        "BATCHGET",
        "BATCHRENAME",
        "BATCHMOVE"
      };
      string[] strArray2 = new string[5]
      {
        "BATCHCREATE",
        "BATCHUPDATE",
        "BATCHDELETE",
        "BATCHRENAME",
        "BATCHMOVE"
      };
      string upperInvariant = request.Operation.ToUpperInvariant();
      if (!Enumerable.Contains<string>((IEnumerable<string>) strArray1, upperInvariant))
      {
        this._logger.LogWarning("Invalid operation '{Operation}' requested for {ToolName}. Valid operations: {ValidOperations}", (object) request.Operation, (object) nameof (BatchMeasureOperationsTool), (object) string.Join(", ", strArray1));
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
          this._logger.LogWarning("{ToolName}.{Operation} blocked by write guard: {Reason}", (object) nameof (BatchMeasureOperationsTool), (object) request.Operation, (object) writeOperationResult.Message);
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
              goto label_26;
            }
            break;
          case 8:
            if ((upperInvariant == "BATCHGET"))
            {
              operationResponse = this.HandleBatchGetOperation(request);
              goto label_26;
            }
            break;
          case 9:
            if ((upperInvariant == "BATCHMOVE"))
            {
              operationResponse = this.HandleBatchMoveOperation(request);
              goto label_26;
            }
            break;
          case 11:
            switch (upperInvariant[5])
            {
              case 'C':
                if ((upperInvariant == "BATCHCREATE"))
                {
                  operationResponse = this.HandleBatchCreateOperation(request);
                  goto label_26;
                }
                break;
              case 'D':
                if ((upperInvariant == "BATCHDELETE"))
                {
                  operationResponse = this.HandleBatchDeleteOperation(request);
                  goto label_26;
                }
                break;
              case 'R':
                if ((upperInvariant == "BATCHRENAME"))
                {
                  operationResponse = this.HandleBatchRenameOperation(request);
                  goto label_26;
                }
                break;
              case 'U':
                if ((upperInvariant == "BATCHUPDATE"))
                {
                  operationResponse = this.HandleBatchUpdateOperation(request);
                  goto label_26;
                }
                break;
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
label_26:
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Error executing {ToolName}.{Operation}: {ErrorMessage}", (object) nameof (BatchMeasureOperationsTool), (object) request.Operation, (object) ex.Message);
      return new BatchOperationResponse()
      {
        Success = false,
        Message = "Error executing batch measure operation: " + ex.Message,
        Operation = request.Operation
      };
    }
  }

  private BatchOperationResponse HandleBatchCreateOperation(BatchMeasureOperationRequest request)
  {
    try
    {
      if (string.IsNullOrEmpty(request.BatchCreateRequest.ConnectionName) && !string.IsNullOrEmpty(request.ConnectionName))
        request.BatchCreateRequest.ConnectionName = request.ConnectionName;
      BatchOperationResponse measures = BatchMeasureOperations.BatchCreateMeasures(request.BatchCreateRequest);
      int num1 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) measures.Results, (r => r.Success));
      int num2 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) measures.Results, (r => !r.Success));
      if (measures.Success)
        this._logger.LogInformation("{ToolName}.{Operation} completed: Total={Total}, Succeeded={Succeeded}, Failed={Failed}", (object) nameof (BatchMeasureOperationsTool), (object) request.Operation, (object) measures.Results.Count, (object) num1, (object) num2);
      else
        this._logger.LogWarning("{ToolName}.{Operation} completed with errors: Total={Total}, Succeeded={Succeeded}, Failed={Failed}, Message={Message}", (object) nameof (BatchMeasureOperationsTool), (object) request.Operation, (object) measures.Results.Count, (object) num1, (object) num2, (object) measures.Message);
      if (measures.Warnings != null && Enumerable.Any<string>((IEnumerable<string>) measures.Warnings))
      {
        foreach (string warning in measures.Warnings)
          this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (BatchMeasureOperationsTool), (object) request.Operation, (object) warning);
      }
      return measures;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      return new BatchOperationResponse()
      {
        Success = false,
        Message = "Error in BatchCreate operation: " + ex.Message,
        Operation = request.Operation
      };
    }
  }

  private BatchOperationResponse HandleBatchUpdateOperation(BatchMeasureOperationRequest request)
  {
    try
    {
      if (string.IsNullOrEmpty(request.BatchUpdateRequest.ConnectionName) && !string.IsNullOrEmpty(request.ConnectionName))
        request.BatchUpdateRequest.ConnectionName = request.ConnectionName;
      BatchOperationResponse operationResponse = BatchMeasureOperations.BatchUpdateMeasures(request.BatchUpdateRequest);
      int num1 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) operationResponse.Results, (r => r.Success));
      int num2 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) operationResponse.Results, (r => !r.Success));
      if (operationResponse.Success)
        this._logger.LogInformation("{ToolName}.{Operation} completed: Total={Total}, Succeeded={Succeeded}, Failed={Failed}", (object) nameof (BatchMeasureOperationsTool), (object) request.Operation, (object) operationResponse.Results.Count, (object) num1, (object) num2);
      else
        this._logger.LogWarning("{ToolName}.{Operation} completed with errors: Total={Total}, Succeeded={Succeeded}, Failed={Failed}, Message={Message}", (object) nameof (BatchMeasureOperationsTool), (object) request.Operation, (object) operationResponse.Results.Count, (object) num1, (object) num2, (object) operationResponse.Message);
      if (operationResponse.Warnings != null && Enumerable.Any<string>((IEnumerable<string>) operationResponse.Warnings))
      {
        foreach (string warning in operationResponse.Warnings)
          this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (BatchMeasureOperationsTool), (object) request.Operation, (object) warning);
      }
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      return new BatchOperationResponse()
      {
        Success = false,
        Message = "Error in BatchUpdate operation: " + ex.Message,
        Operation = request.Operation
      };
    }
  }

  private BatchOperationResponse HandleBatchDeleteOperation(BatchMeasureOperationRequest request)
  {
    try
    {
      if (string.IsNullOrEmpty(request.BatchDeleteRequest.ConnectionName) && !string.IsNullOrEmpty(request.ConnectionName))
        request.BatchDeleteRequest.ConnectionName = request.ConnectionName;
      BatchOperationResponse operationResponse = BatchMeasureOperations.BatchDeleteMeasures(request.BatchDeleteRequest);
      int num1 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) operationResponse.Results, (r => r.Success));
      int num2 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) operationResponse.Results, (r => !r.Success));
      if (operationResponse.Success)
        this._logger.LogInformation("{ToolName}.{Operation} completed: Total={Total}, Succeeded={Succeeded}, Failed={Failed}, Cascade={Cascade}", (object) nameof (BatchMeasureOperationsTool), (object) request.Operation, (object) operationResponse.Results.Count, (object) num1, (object) num2, (object) request.BatchDeleteRequest.ShouldCascadeDelete);
      else
        this._logger.LogWarning("{ToolName}.{Operation} completed with errors: Total={Total}, Succeeded={Succeeded}, Failed={Failed}, Cascade={Cascade}, Message={Message}", (object) nameof (BatchMeasureOperationsTool), (object) request.Operation, (object) operationResponse.Results.Count, (object) num1, (object) num2, (object) request.BatchDeleteRequest.ShouldCascadeDelete, (object) operationResponse.Message);
      if (operationResponse.Warnings != null && Enumerable.Any<string>((IEnumerable<string>) operationResponse.Warnings))
      {
        foreach (string warning in operationResponse.Warnings)
          this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (BatchMeasureOperationsTool), (object) request.Operation, (object) warning);
      }
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      return new BatchOperationResponse()
      {
        Success = false,
        Message = "Error in BatchDelete operation: " + ex.Message,
        Operation = request.Operation
      };
    }
  }

  private BatchOperationResponse HandleBatchGetOperation(BatchMeasureOperationRequest request)
  {
    try
    {
      if (string.IsNullOrEmpty(request.BatchGetRequest.ConnectionName) && !string.IsNullOrEmpty(request.ConnectionName))
        request.BatchGetRequest.ConnectionName = request.ConnectionName;
      BatchOperationResponse measures = BatchMeasureOperations.BatchGetMeasures(request.BatchGetRequest);
      int num1 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) measures.Results, (r => r.Success));
      int num2 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) measures.Results, (r => !r.Success));
      if (measures.Success)
        this._logger.LogInformation("{ToolName}.{Operation} completed: Total={Total}, Succeeded={Succeeded}, Failed={Failed}", (object) nameof (BatchMeasureOperationsTool), (object) request.Operation, (object) measures.Results.Count, (object) num1, (object) num2);
      else
        this._logger.LogWarning("{ToolName}.{Operation} completed with errors: Total={Total}, Succeeded={Succeeded}, Failed={Failed}, Message={Message}", (object) nameof (BatchMeasureOperationsTool), (object) request.Operation, (object) measures.Results.Count, (object) num1, (object) num2, (object) measures.Message);
      if (measures.Warnings != null && Enumerable.Any<string>((IEnumerable<string>) measures.Warnings))
      {
        foreach (string warning in measures.Warnings)
          this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (BatchMeasureOperationsTool), (object) request.Operation, (object) warning);
      }
      return measures;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      return new BatchOperationResponse()
      {
        Success = false,
        Message = "Error in BatchGet operation: " + ex.Message,
        Operation = request.Operation
      };
    }
  }

  private BatchOperationResponse HandleBatchRenameOperation(BatchMeasureOperationRequest request)
  {
    try
    {
      if (string.IsNullOrEmpty(request.BatchRenameRequest.ConnectionName) && !string.IsNullOrEmpty(request.ConnectionName))
        request.BatchRenameRequest.ConnectionName = request.ConnectionName;
      BatchOperationResponse operationResponse = BatchMeasureOperations.BatchRenameMeasures(request.BatchRenameRequest);
      int num1 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) operationResponse.Results, (r => r.Success));
      int num2 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) operationResponse.Results, (r => !r.Success));
      if (operationResponse.Success)
        this._logger.LogInformation("{ToolName}.{Operation} completed: Total={Total}, Succeeded={Succeeded}, Failed={Failed}", (object) nameof (BatchMeasureOperationsTool), (object) request.Operation, (object) operationResponse.Results.Count, (object) num1, (object) num2);
      else
        this._logger.LogWarning("{ToolName}.{Operation} completed with errors: Total={Total}, Succeeded={Succeeded}, Failed={Failed}, Message={Message}", (object) nameof (BatchMeasureOperationsTool), (object) request.Operation, (object) operationResponse.Results.Count, (object) num1, (object) num2, (object) operationResponse.Message);
      if (operationResponse.Warnings != null && Enumerable.Any<string>((IEnumerable<string>) operationResponse.Warnings))
      {
        foreach (string warning in operationResponse.Warnings)
          this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (BatchMeasureOperationsTool), (object) request.Operation, (object) warning);
      }
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      return new BatchOperationResponse()
      {
        Success = false,
        Message = "Error in BatchRename operation: " + ex.Message,
        Operation = request.Operation
      };
    }
  }

  private BatchOperationResponse HandleBatchMoveOperation(BatchMeasureOperationRequest request)
  {
    try
    {
      if (string.IsNullOrEmpty(request.BatchMoveRequest.ConnectionName) && !string.IsNullOrEmpty(request.ConnectionName))
        request.BatchMoveRequest.ConnectionName = request.ConnectionName;
      BatchOperationResponse operationResponse = BatchMeasureOperations.BatchMoveMeasures(request.BatchMoveRequest);
      int num1 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) operationResponse.Results, (r => r.Success));
      int num2 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) operationResponse.Results, (r => !r.Success));
      if (operationResponse.Success)
        this._logger.LogInformation("{ToolName}.{Operation} completed: Total={Total}, Succeeded={Succeeded}, Failed={Failed}", (object) nameof (BatchMeasureOperationsTool), (object) request.Operation, (object) operationResponse.Results.Count, (object) num1, (object) num2);
      else
        this._logger.LogWarning("{ToolName}.{Operation} completed with errors: Total={Total}, Succeeded={Succeeded}, Failed={Failed}, Message={Message}", (object) nameof (BatchMeasureOperationsTool), (object) request.Operation, (object) operationResponse.Results.Count, (object) num1, (object) num2, (object) operationResponse.Message);
      if (operationResponse.Warnings != null && Enumerable.Any<string>((IEnumerable<string>) operationResponse.Warnings))
      {
        foreach (string warning in operationResponse.Warnings)
          this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (BatchMeasureOperationsTool), (object) request.Operation, (object) warning);
      }
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      return new BatchOperationResponse()
      {
        Success = false,
        Message = "Error in BatchMove operation: " + ex.Message,
        Operation = request.Operation
      };
    }
  }

  private BatchOperationResponse HandleHelpOperation(
    BatchMeasureOperationRequest request,
    string[] operations)
  {
    try
    {
      List<object> objectList1 = new List<object>();
      foreach (string operation in operations)
      {
        if (BatchMeasureOperationsTool.toolMetadata.Operations.ContainsKey(operation))
          objectList1.Add((object) new
          {
            Operation = operation,
            Description = BatchMeasureOperationsTool.toolMetadata.Operations[operation]
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
      itemResult.Message = "Batch measure operations tool metadata";
      List<string> list = Enumerable.ToList<string>((IEnumerable<string>) operations);
      List<object> objectList2 = objectList1;
      var data = new
      {
        ContinueOnError = "Boolean: Whether to continue processing remaining items when an error occurs (default: false)",
        UseTransaction = "Boolean: Whether to wrap the batch operation in a transaction for atomicity (default: true)"
      };
      List<string> stringList = new List<string>();
      stringList.Add("All batch operations support transaction management for ACID compliance");
      stringList.Add("ContinueOnError allows processing to continue even if individual items fail");
      stringList.Add("UseTransaction ensures all operations succeed or all fail together");
      stringList.Add("BatchMove operation is specific to measures and allows moving measures between tables");
      itemResult.Data = (object) new
      {
        Tool = "batch_measure_operations",
        Description = "Perform batch operations on semantic model measures",
        SupportedOperations = list,
        OperationDetails = objectList2,
        BatchOptions = data,
        Notes = stringList
      };
      results.Add(itemResult);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Operations={OperationCount}", (object) nameof (BatchMeasureOperationsTool), (object) request.Operation, (object) operations.Length);
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

  private bool ValidateRequest(string operation, BatchMeasureOperationRequest request)
  {
    OperationMetadata operationMetadata;
    if (!BatchMeasureOperationsTool.toolMetadata.Operations.TryGetValue(operation, out operationMetadata))
      return true;
    JsonObject requestDict = JsonSerializer.SerializeToNode<BatchMeasureOperationRequest>(request) as JsonObject;
    List<string> list1 = Enumerable.ToList<string>(Enumerable.Where<string>((IEnumerable<string>) operationMetadata.RequiredParams, (p => requestDict != null && requestDict[p] == null)));
    List<string> list2 = Enumerable.ToList<string>(Enumerable.Where<string>((IEnumerable<string>) operationMetadata.ForbiddenParams, (p => requestDict != null && requestDict[p] != null)));
    if (Enumerable.Any<string>((IEnumerable<string>) list1))
      throw new McpException($"Missing required parameters needed for {operation} operation: {string.Join(", ", (IEnumerable<string>) list1)}");
    if (Enumerable.Any<string>((IEnumerable<string>) list2))
      throw new McpException($"Forbidden parameters not allowed for {operation} operation: {string.Join(", ", (IEnumerable<string>) list2)}");
    return true;
  }

  static BatchMeasureOperationsTool()
  {
    ToolMetadata toolMetadata1 = new ToolMetadata();
    ToolMetadata toolMetadata2 = toolMetadata1;
    Dictionary<string, OperationMetadata> dictionary1 = new Dictionary<string, OperationMetadata>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    Dictionary<string, OperationMetadata> dictionary2 = dictionary1;
    OperationMetadata operationMetadata1 = new OperationMetadata { RequiredParams = new string[1]
    {
      "BatchCreateRequest"
    } };
    operationMetadata1.Description = "Create multiple measures in a single batch operation with transactional support.\r\nMandatory properties: BatchCreateRequest (with Items containing measures with Name, Expression, TableName).\r\nOptional: Description, FormatString, IsHidden, IsSimpleMeasure, DisplayFolder, DataType, DataCategory, LineageTag, SourceLineageTag, KPI, DetailRowsExpression, FormatStringExpression, Annotations, ExtendedProperties, Options (with ContinueOnError, UseTransaction).";
    operationMetadata1.CommonMistakes = new string[1]
    {
      "Forgetting to supply the host table of the measure to be created in each item's TableName property"
    };
    OperationMetadata operationMetadata2 = operationMetadata1;
    List<string> stringList1 = new List<string>();
    stringList1.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"BatchCreate\",\r\n        \"BatchCreateRequest\": {\r\n            \"Items\": [\r\n                { \r\n                    \"Name\": \"TotalSales\", \r\n                    \"TableName\": \"Sales\", \r\n                    \"Expression\": \"SUM([SalesAmount])\" \r\n                },\r\n                { \r\n                    \"Name\": \"AvgSales\", \r\n                    \"TableName\": \"Sales\", \r\n                    \"Expression\": \"AVERAGE([SalesAmount])\" \r\n                }\r\n            ],\r\n            \"Options\": {\r\n                \"ContinueOnError\": false,\r\n                \"UseTransaction\": true\r\n            }\r\n        }\r\n    }\r\n}");
    operationMetadata2.ExampleRequests = stringList1;
    OperationMetadata operationMetadata3 = operationMetadata1;
    dictionary2["BatchCreate"] = operationMetadata3;
    Dictionary<string, OperationMetadata> dictionary3 = dictionary1;
    OperationMetadata operationMetadata4 = new OperationMetadata { RequiredParams = new string[1]
    {
      "BatchUpdateRequest"
    } };
    operationMetadata4.Description = "Update multiple existing measures in a single batch operation with transactional support.\r\nMandatory properties: BatchUpdateRequest (with Items containing measures with Name).\r\nOptional: Expression, Description, FormatString, IsHidden, IsSimpleMeasure, DisplayFolder, DataType, DataCategory, LineageTag, SourceLineageTag, KPI, DetailRowsExpression, FormatStringExpression, Annotations, ExtendedProperties, Options (with ContinueOnError, UseTransaction).\r\nNote: Cannot change table assignment via Update - use BatchMove operation instead.";
    OperationMetadata operationMetadata5 = operationMetadata4;
    List<string> stringList2 = new List<string>();
    stringList2.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"BatchUpdate\",\r\n        \"BatchUpdateRequest\": {\r\n            \"Items\": [\r\n                { \r\n                    \"Name\": \"TotalSales\", \r\n                    \"Description\": \"Updated description\",\r\n                    \"FormatString\": \"Currency\"\r\n                },\r\n                { \r\n                    \"Name\": \"AvgSales\", \r\n                    \"IsHidden\": true\r\n                }\r\n            ],\r\n            \"Options\": {\r\n                \"ContinueOnError\": true,\r\n                \"UseTransaction\": false\r\n            }\r\n        }\r\n    }\r\n}");
    operationMetadata5.ExampleRequests = stringList2;
    OperationMetadata operationMetadata6 = operationMetadata4;
    dictionary3["BatchUpdate"] = operationMetadata6;
    Dictionary<string, OperationMetadata> dictionary4 = dictionary1;
    OperationMetadata operationMetadata7 = new OperationMetadata { RequiredParams = new string[1]
    {
      "BatchDeleteRequest"
    } };
    operationMetadata7.Description = "Delete multiple measures in a single batch operation with transactional support.\r\nMandatory properties: BatchDeleteRequest (with Items containing measure names, ShouldCascadeDelete).\r\nOptional: Options (with ContinueOnError, UseTransaction).\r\nNote: When ShouldCascadeDelete is true, dependent objects will be automatically deleted.";
    OperationMetadata operationMetadata8 = operationMetadata7;
    List<string> stringList3 = new List<string>();
    stringList3.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"BatchDelete\",\r\n        \"BatchDeleteRequest\": {\r\n            \"Items\": [\"ObsoleteMeasure1\", \"ObsoleteMeasure2\"],\r\n            \"ShouldCascadeDelete\": true,\r\n            \"Options\": {\r\n                \"ContinueOnError\": false,\r\n                \"UseTransaction\": true\r\n            }\r\n        }\r\n    }\r\n}");
    operationMetadata8.ExampleRequests = stringList3;
    OperationMetadata operationMetadata9 = operationMetadata7;
    dictionary4["BatchDelete"] = operationMetadata9;
    Dictionary<string, OperationMetadata> dictionary5 = dictionary1;
    OperationMetadata operationMetadata10 = new OperationMetadata { RequiredParams = new string[1]
    {
      "BatchGetRequest"
    } };
    operationMetadata10.Description = "Retrieve details of multiple measures in a single batch operation.\r\nMandatory properties: BatchGetRequest (with Items containing measure names).\r\nOptional: Options (with ContinueOnError).\r\nReturns detailed measure information including properties, expressions, and metadata.";
    OperationMetadata operationMetadata11 = operationMetadata10;
    List<string> stringList4 = new List<string>();
    stringList4.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"BatchGet\",\r\n        \"BatchGetRequest\": {\r\n            \"Items\": [\"TotalSales\", \"AvgSales\", \"TotalProfit\"],\r\n            \"Options\": {\r\n                \"ContinueOnError\": true\r\n            }\r\n        }\r\n    }\r\n}");
    operationMetadata11.ExampleRequests = stringList4;
    OperationMetadata operationMetadata12 = operationMetadata10;
    dictionary5["BatchGet"] = operationMetadata12;
    Dictionary<string, OperationMetadata> dictionary6 = dictionary1;
    OperationMetadata operationMetadata13 = new OperationMetadata { RequiredParams = new string[1]
    {
      "BatchRenameRequest"
    } };
    operationMetadata13.Description = "Rename multiple measures in a single batch operation with automatic reference updates and transactional support.\r\nMandatory properties: BatchRenameRequest (with Items containing rename definitions with CurrentName, NewName).\r\nOptional: TableName (in rename definitions), Options (with ContinueOnError, UseTransaction).\r\nAll DAX expressions and other references will be automatically updated.";
    OperationMetadata operationMetadata14 = operationMetadata13;
    List<string> stringList5 = new List<string>();
    stringList5.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"BatchRename\",\r\n        \"BatchRenameRequest\": {\r\n            \"Items\": [\r\n                { \r\n                    \"CurrentName\": \"OldMeasure1\", \r\n                    \"NewName\": \"NewMeasure1\"\r\n                },\r\n                { \r\n                    \"CurrentName\": \"OldMeasure2\", \r\n                    \"NewName\": \"NewMeasure2\"\r\n                }\r\n            ],\r\n            \"Options\": {\r\n                \"ContinueOnError\": false,\r\n                \"UseTransaction\": true\r\n            }\r\n        }\r\n    }\r\n}");
    operationMetadata14.ExampleRequests = stringList5;
    OperationMetadata operationMetadata15 = operationMetadata13;
    dictionary6["BatchRename"] = operationMetadata15;
    Dictionary<string, OperationMetadata> dictionary7 = dictionary1;
    OperationMetadata operationMetadata16 = new OperationMetadata { RequiredParams = new string[1]
    {
      "BatchMoveRequest"
    } };
    operationMetadata16.Description = "Move multiple measures to different tables in a single batch operation with transactional support.\r\nMandatory properties: BatchMoveRequest (with Items containing move definitions with Name, DestinationTableName).\r\nOptional: CurrentTableName (in move definitions), Options (with ContinueOnError, UseTransaction).\r\nThis operation transfers measures between tables while maintaining their properties and references.";
    OperationMetadata operationMetadata17 = operationMetadata16;
    List<string> stringList6 = new List<string>();
    stringList6.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"BatchMove\",\r\n        \"BatchMoveRequest\": {\r\n            \"Items\": [\r\n                {\r\n                    \"MeasureName\": \"MyMeasure1\",\r\n                    \"DestinationTableName\": \"NewTable1\"\r\n                },\r\n                {\r\n                    \"MeasureName\": \"MyMeasure2\",\r\n                    \"DestinationTableName\": \"NewTable2\"\r\n                }\r\n            ],\r\n            \"Options\": {\r\n                \"ContinueOnError\": true,\r\n                \"UseTransaction\": false\r\n            }\r\n        }\r\n    }\r\n}");
    operationMetadata17.ExampleRequests = stringList6;
    OperationMetadata operationMetadata18 = operationMetadata16;
    dictionary7["BatchMove"] = operationMetadata18;
    Dictionary<string, OperationMetadata> dictionary8 = dictionary1;
    OperationMetadata operationMetadata19 = new OperationMetadata { Description = "Display comprehensive help information about the batch measure operations tool and all available operations.\r\nMandatory properties: None.\r\nOptional: None.\r\nReturns detailed documentation for each batch operation with examples and batch-specific options." };
    List<string> stringList7 = new List<string>();
    stringList7.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Help\"\r\n    }\r\n}");
    operationMetadata19.ExampleRequests = stringList7;
    dictionary8["Help"] = operationMetadata19;
    Dictionary<string, OperationMetadata> dictionary9 = dictionary1;
    toolMetadata2.Operations = dictionary9;
    BatchMeasureOperationsTool.toolMetadata = toolMetadata1;
  }
}
