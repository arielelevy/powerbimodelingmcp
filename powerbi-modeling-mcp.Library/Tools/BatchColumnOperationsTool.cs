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
public class BatchColumnOperationsTool
{
  private readonly ILogger<BatchColumnOperationsTool> _logger;
  public static readonly ToolMetadata toolMetadata;

  public BatchColumnOperationsTool(ILogger<BatchColumnOperationsTool> logger)
  {
    this._logger = logger;
  }

  [McpServerTool(Name = "batch_column_operations")]
  [Description("Perform batch operations on semantic model columns. Supported operations: Help, BatchCreate, BatchUpdate, BatchDelete, BatchGet, BatchRename. Use the Operation parameter to specify which batch operation to perform. Each operation includes options for error handling (ContinueOnError) and transactional behavior (UseTransaction).")]
  public BatchOperationResponse ExecuteBatchColumnOperation(
    McpServer mcpServer,
    BatchColumnOperationRequest request)
  {
    this._logger.LogDebug("Executing {ToolName}.{Operation}: ItemCount={ItemCount}, Connection={ConnectionName}", (object) nameof (BatchColumnOperationsTool), (object) request.Operation, (object) (request.BatchCreateRequest?.Items?.Count ?? request.BatchUpdateRequest?.Items?.Count ?? request.BatchDeleteRequest?.Items?.Count ?? request.BatchGetRequest?.Items?.Count ?? request.BatchRenameRequest?.Items?.Count ?? 0), (object) (request.ConnectionName ?? "(last used)"));
    try
    {
      string[] strArray1 = new string[6]
      {
        "HELP",
        "BATCHCREATE",
        "BATCHUPDATE",
        "BATCHDELETE",
        "BATCHGET",
        "BATCHRENAME"
      };
      string[] strArray2 = new string[4]
      {
        "BATCHCREATE",
        "BATCHUPDATE",
        "BATCHDELETE",
        "BATCHRENAME"
      };
      string upperInvariant = request.Operation.ToUpperInvariant();
      if (!Enumerable.Contains<string>((IEnumerable<string>) strArray1, upperInvariant))
      {
        this._logger.LogWarning("Invalid operation '{Operation}' requested for {ToolName}. Valid operations: {ValidOperations}", (object) request.Operation, (object) nameof (BatchColumnOperationsTool), (object) string.Join(", ", strArray1));
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
          this._logger.LogWarning("{ToolName}.{Operation} blocked by write guard: {Reason}", (object) nameof (BatchColumnOperationsTool), (object) request.Operation, (object) writeOperationResult.Message);
          return new BatchOperationResponse()
          {
            Success = false,
            Message = writeOperationResult.Message,
            Operation = request.Operation
          };
        }
      }
      BatchOperationResponse operationResponse;
      if (!(upperInvariant == "HELP"))
      {
        if (!(upperInvariant == "BATCHCREATE"))
        {
          if (!(upperInvariant == "BATCHUPDATE"))
          {
            if (!(upperInvariant == "BATCHDELETE"))
            {
              if (!(upperInvariant == "BATCHGET"))
              {
                if ((upperInvariant == "BATCHRENAME"))
                  operationResponse = this.HandleBatchRenameOperation(request);
                else
                  operationResponse = new BatchOperationResponse()
                  {
                    Success = false,
                    Message = $"Operation {request.Operation} is not implemented",
                    Operation = request.Operation
                  };
              }
              else
                operationResponse = this.HandleBatchGetOperation(request);
            }
            else
              operationResponse = this.HandleBatchDeleteOperation(request);
          }
          else
            operationResponse = this.HandleBatchUpdateOperation(request);
        }
        else
          operationResponse = this.HandleBatchCreateOperation(request);
      }
      else
        operationResponse = this.HandleHelpOperation(request, allowed ? strArray1 : Enumerable.ToArray<string>(Enumerable.Except<string>((IEnumerable<string>) strArray1, (IEnumerable<string>) strArray2)));
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Error executing {ToolName}.{Operation}: {ErrorMessage}", (object) nameof (BatchColumnOperationsTool), (object) request.Operation, (object) ex.Message);
      return new BatchOperationResponse()
      {
        Success = false,
        Message = "Error executing batch column operation: " + ex.Message,
        Operation = request.Operation
      };
    }
  }

  private BatchOperationResponse HandleHelpOperation(
    BatchColumnOperationRequest request,
    string[] operations)
  {
    try
    {
      List<object> objectList1 = new List<object>();
      foreach (string operation in operations)
      {
        if (BatchColumnOperationsTool.toolMetadata.Operations.ContainsKey(operation))
          objectList1.Add((object) new
          {
            Operation = operation,
            Description = BatchColumnOperationsTool.toolMetadata.Operations[operation]
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
      itemResult.Message = "Batch column operations tool metadata";
      List<string> list = Enumerable.ToList<string>((IEnumerable<string>) operations);
      List<object> objectList2 = objectList1;
      var data = new
      {
        ContinueOnError = "Boolean: Whether to continue processing remaining items when an error occurs (default: false)",
        UseTransaction = "Boolean: Whether to wrap the batch operation in a transaction for atomicity (default: true)"
      };
      List<string> stringList = new List<string>();
      stringList.Add("Column identifiers for BatchGet operations use ColumnIdentifier objects with TableName and Name properties");
      stringList.Add("BatchDelete operations use ColumnIdentifier objects with TableName and Name properties");
      stringList.Add("BatchCreate and BatchUpdate operations use full column definition objects");
      stringList.Add("BatchRename operations use rename definition objects with TableName, CurrentName, and NewName");
      stringList.Add("Transaction support ensures all operations succeed or all fail together when UseTransaction is true");
      stringList.Add("ContinueOnError allows processing to continue even if individual items fail");
      stringList.Add("Unlike measures, columns cannot be moved between tables");
      itemResult.Data = (object) new
      {
        Tool = "batch_column_operations",
        Description = "Perform batch operations on semantic model columns",
        SupportedOperations = list,
        OperationDetails = objectList2,
        BatchOptions = data,
        Notes = stringList
      };
      results.Add(itemResult);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Operations={OperationCount}", (object) nameof (BatchColumnOperationsTool), (object) request.Operation, (object) operations.Length);
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

  private BatchOperationResponse HandleBatchCreateOperation(BatchColumnOperationRequest request)
  {
    try
    {
      if (string.IsNullOrEmpty(request.BatchCreateRequest.ConnectionName) && !string.IsNullOrEmpty(request.ConnectionName))
        request.BatchCreateRequest.ConnectionName = request.ConnectionName;
      BatchOperationResponse columns = BatchColumnOperations.BatchCreateColumns(request.BatchCreateRequest);
      int num1 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) columns.Results, (r => r.Success));
      int num2 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) columns.Results, (r => !r.Success));
      if (columns.Success)
        this._logger.LogInformation("{ToolName}.{Operation} completed: Total={Total}, Succeeded={Succeeded}, Failed={Failed}", (object) nameof (BatchColumnOperationsTool), (object) request.Operation, (object) columns.Results.Count, (object) num1, (object) num2);
      else
        this._logger.LogWarning("{ToolName}.{Operation} completed with errors: Total={Total}, Succeeded={Succeeded}, Failed={Failed}, Message={Message}", (object) nameof (BatchColumnOperationsTool), (object) request.Operation, (object) columns.Results.Count, (object) num1, (object) num2, (object) columns.Message);
      if (columns.Warnings != null && Enumerable.Any<string>((IEnumerable<string>) columns.Warnings))
      {
        foreach (string warning in columns.Warnings)
          this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (BatchColumnOperationsTool), (object) request.Operation, (object) warning);
      }
      return columns;
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

  private BatchOperationResponse HandleBatchUpdateOperation(BatchColumnOperationRequest request)
  {
    try
    {
      if (string.IsNullOrEmpty(request.BatchUpdateRequest.ConnectionName) && !string.IsNullOrEmpty(request.ConnectionName))
        request.BatchUpdateRequest.ConnectionName = request.ConnectionName;
      BatchOperationResponse operationResponse = BatchColumnOperations.BatchUpdateColumns(request.BatchUpdateRequest);
      int num1 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) operationResponse.Results, (r => r.Success));
      int num2 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) operationResponse.Results, (r => !r.Success));
      if (operationResponse.Success)
        this._logger.LogInformation("{ToolName}.{Operation} completed: Total={Total}, Succeeded={Succeeded}, Failed={Failed}", (object) nameof (BatchColumnOperationsTool), (object) request.Operation, (object) operationResponse.Results.Count, (object) num1, (object) num2);
      else
        this._logger.LogWarning("{ToolName}.{Operation} completed with errors: Total={Total}, Succeeded={Succeeded}, Failed={Failed}, Message={Message}", (object) nameof (BatchColumnOperationsTool), (object) request.Operation, (object) operationResponse.Results.Count, (object) num1, (object) num2, (object) operationResponse.Message);
      if (operationResponse.Warnings != null && Enumerable.Any<string>((IEnumerable<string>) operationResponse.Warnings))
      {
        foreach (string warning in operationResponse.Warnings)
          this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (BatchColumnOperationsTool), (object) request.Operation, (object) warning);
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

  private BatchOperationResponse HandleBatchDeleteOperation(BatchColumnOperationRequest request)
  {
    try
    {
      if (string.IsNullOrEmpty(request.BatchDeleteRequest.ConnectionName) && !string.IsNullOrEmpty(request.ConnectionName))
        request.BatchDeleteRequest.ConnectionName = request.ConnectionName;
      BatchOperationResponse operationResponse = BatchColumnOperations.BatchDeleteColumns(request.BatchDeleteRequest);
      int num1 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) operationResponse.Results, (r => r.Success));
      int num2 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) operationResponse.Results, (r => !r.Success));
      if (operationResponse.Success)
        this._logger.LogInformation("{ToolName}.{Operation} completed: Total={Total}, Succeeded={Succeeded}, Failed={Failed}, Cascade={Cascade}", (object) nameof (BatchColumnOperationsTool), (object) request.Operation, (object) operationResponse.Results.Count, (object) num1, (object) num2, (object) request.BatchDeleteRequest.ShouldCascadeDelete);
      else
        this._logger.LogWarning("{ToolName}.{Operation} completed with errors: Total={Total}, Succeeded={Succeeded}, Failed={Failed}, Cascade={Cascade}, Message={Message}", (object) nameof (BatchColumnOperationsTool), (object) request.Operation, (object) operationResponse.Results.Count, (object) num1, (object) num2, (object) request.BatchDeleteRequest.ShouldCascadeDelete, (object) operationResponse.Message);
      if (operationResponse.Warnings != null && Enumerable.Any<string>((IEnumerable<string>) operationResponse.Warnings))
      {
        foreach (string warning in operationResponse.Warnings)
          this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (BatchColumnOperationsTool), (object) request.Operation, (object) warning);
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

  private BatchOperationResponse HandleBatchGetOperation(BatchColumnOperationRequest request)
  {
    try
    {
      if (string.IsNullOrEmpty(request.BatchGetRequest.ConnectionName) && !string.IsNullOrEmpty(request.ConnectionName))
        request.BatchGetRequest.ConnectionName = request.ConnectionName;
      BatchOperationResponse columns = BatchColumnOperations.BatchGetColumns(request.BatchGetRequest);
      int num1 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) columns.Results, (r => r.Success));
      int num2 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) columns.Results, (r => !r.Success));
      if (columns.Success)
        this._logger.LogInformation("{ToolName}.{Operation} completed: Total={Total}, Succeeded={Succeeded}, Failed={Failed}", (object) nameof (BatchColumnOperationsTool), (object) request.Operation, (object) columns.Results.Count, (object) num1, (object) num2);
      else
        this._logger.LogWarning("{ToolName}.{Operation} completed with errors: Total={Total}, Succeeded={Succeeded}, Failed={Failed}, Message={Message}", (object) nameof (BatchColumnOperationsTool), (object) request.Operation, (object) columns.Results.Count, (object) num1, (object) num2, (object) columns.Message);
      if (columns.Warnings != null && Enumerable.Any<string>((IEnumerable<string>) columns.Warnings))
      {
        foreach (string warning in columns.Warnings)
          this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (BatchColumnOperationsTool), (object) request.Operation, (object) warning);
      }
      return columns;
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

  private BatchOperationResponse HandleBatchRenameOperation(BatchColumnOperationRequest request)
  {
    try
    {
      if (string.IsNullOrEmpty(request.BatchRenameRequest.ConnectionName) && !string.IsNullOrEmpty(request.ConnectionName))
        request.BatchRenameRequest.ConnectionName = request.ConnectionName;
      BatchOperationResponse operationResponse = BatchColumnOperations.BatchRenameColumns(request.BatchRenameRequest);
      int num1 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) operationResponse.Results, (r => r.Success));
      int num2 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) operationResponse.Results, (r => !r.Success));
      if (operationResponse.Success)
        this._logger.LogInformation("{ToolName}.{Operation} completed: Total={Total}, Succeeded={Succeeded}, Failed={Failed}", (object) nameof (BatchColumnOperationsTool), (object) request.Operation, (object) operationResponse.Results.Count, (object) num1, (object) num2);
      else
        this._logger.LogWarning("{ToolName}.{Operation} completed with errors: Total={Total}, Succeeded={Succeeded}, Failed={Failed}, Message={Message}", (object) nameof (BatchColumnOperationsTool), (object) request.Operation, (object) operationResponse.Results.Count, (object) num1, (object) num2, (object) operationResponse.Message);
      if (operationResponse.Warnings != null && Enumerable.Any<string>((IEnumerable<string>) operationResponse.Warnings))
      {
        foreach (string warning in operationResponse.Warnings)
          this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (BatchColumnOperationsTool), (object) request.Operation, (object) warning);
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

  private bool ValidateRequest(string operation, BatchColumnOperationRequest request)
  {
    OperationMetadata operationMetadata;
    if (!BatchColumnOperationsTool.toolMetadata.Operations.TryGetValue(operation, out operationMetadata))
      return true;
    JsonObject requestDict = JsonSerializer.SerializeToNode<BatchColumnOperationRequest>(request) as JsonObject;
    List<string> list1 = Enumerable.ToList<string>(Enumerable.Where<string>((IEnumerable<string>) operationMetadata.RequiredParams, (p => requestDict != null && requestDict[p] == null)));
    List<string> list2 = Enumerable.ToList<string>(Enumerable.Where<string>((IEnumerable<string>) operationMetadata.ForbiddenParams, (p => requestDict != null && requestDict[p] != null)));
    if (Enumerable.Any<string>((IEnumerable<string>) list1))
      throw new McpException($"Missing required parameters needed for {operation} operation: {string.Join(", ", (IEnumerable<string>) list1)}");
    if (Enumerable.Any<string>((IEnumerable<string>) list2))
      throw new McpException($"Forbidden parameters not allowed for {operation} operation: {string.Join(", ", (IEnumerable<string>) list2)}");
    return true;
  }

  static BatchColumnOperationsTool()
  {
    ToolMetadata toolMetadata1 = new ToolMetadata();
    ToolMetadata toolMetadata2 = toolMetadata1;
    Dictionary<string, OperationMetadata> dictionary1 = new Dictionary<string, OperationMetadata>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    Dictionary<string, OperationMetadata> dictionary2 = dictionary1;
    OperationMetadata operationMetadata1 = new OperationMetadata { RequiredParams = new string[1]
    {
      "BatchCreateRequest"
    } };
    operationMetadata1.Description = "Create multiple columns in a single operation. \r\nMandatory properties: BatchCreateRequest (with Items containing columns with Name, TableName, and either Expression or SourceColumn). \r\nOptional: DataType, DataCategory, FormatString, SummarizeBy, DefaultLabel, DefaultImage, IsHidden, IsUnique, IsKey, IsNullable, DisplayFolder, SortByColumn, SourceProviderType, Description, IsAvailableInMDX, Alignment, TableDetailPosition, Annotations, ExtendedProperties, AlternateOf, GroupByColumns, Options (with ContinueOnError, UseTransaction).";
    OperationMetadata operationMetadata2 = operationMetadata1;
    List<string> stringList1 = new List<string>();
    stringList1.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"BatchCreate\",\r\n        \"BatchCreateRequest\": {\r\n            \"Items\": [\r\n                { \r\n                    \"Name\": \"FullName\", \r\n                    \"TableName\": \"Customer\", \r\n                    \"Expression\": \"[FirstName] & \\\" \\\" & [LastName]\" \r\n                },\r\n                { \r\n                    \"Name\": \"Age\", \r\n                    \"TableName\": \"Customer\", \r\n                    \"Expression\": \"DATEDIFF([BirthDate], TODAY(), YEAR)\" \r\n                }\r\n            ],\r\n            \"Options\": {\r\n                \"ContinueOnError\": false,\r\n                \"UseTransaction\": true\r\n            }\r\n        }\r\n    }\r\n}");
    operationMetadata2.ExampleRequests = stringList1;
    OperationMetadata operationMetadata3 = operationMetadata1;
    dictionary2["BatchCreate"] = operationMetadata3;
    Dictionary<string, OperationMetadata> dictionary3 = dictionary1;
    OperationMetadata operationMetadata4 = new OperationMetadata { RequiredParams = new string[1]
    {
      "BatchUpdateRequest"
    } };
    operationMetadata4.Description = "Update multiple columns in a single operation. Names cannot be changed and must use the BatchRename operation instead. \r\nMandatory properties: BatchUpdateRequest (with Items containing columns with Name, TableName). \r\nOptional: Expression, SourceColumn, DataType, DataCategory, FormatString, SummarizeBy, DefaultLabel, DefaultImage, IsHidden, IsUnique, IsKey, IsNullable, DisplayFolder, SortByColumn, SourceProviderType, Description, IsAvailableInMDX, Alignment, TableDetailPosition, Annotations, ExtendedProperties, AlternateOf, GroupByColumns, Options (with ContinueOnError, UseTransaction).";
    OperationMetadata operationMetadata5 = operationMetadata4;
    List<string> stringList2 = new List<string>();
    stringList2.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"BatchUpdate\",\r\n        \"BatchUpdateRequest\": {\r\n            \"Items\": [\r\n                { \r\n                    \"Name\": \"FullName\", \r\n                    \"TableName\": \"Customer\", \r\n                    \"Description\": \"Customer's full name\", \r\n                    \"IsHidden\": false \r\n                },\r\n                { \r\n                    \"Name\": \"Age\", \r\n                    \"TableName\": \"Customer\", \r\n                    \"Description\": \"Customer's age in years\",\r\n                    \"DataType\": \"Int64\"\r\n                }\r\n            ],\r\n            \"Options\": {\r\n                \"ContinueOnError\": true,\r\n                \"UseTransaction\": false\r\n            }\r\n        }\r\n    }\r\n}");
    operationMetadata5.ExampleRequests = stringList2;
    OperationMetadata operationMetadata6 = operationMetadata4;
    dictionary3["BatchUpdate"] = operationMetadata6;
    Dictionary<string, OperationMetadata> dictionary4 = dictionary1;
    OperationMetadata operationMetadata7 = new OperationMetadata { RequiredParams = new string[1]
    {
      "BatchDeleteRequest"
    } };
    operationMetadata7.Description = "Delete multiple columns in a single operation. \r\nMandatory properties: BatchDeleteRequest (with Items containing ColumnIdentifier objects with TableName and Name properties). \r\nOptional: ShouldCascadeDelete, Options (with ContinueOnError, UseTransaction).";
    OperationMetadata operationMetadata8 = operationMetadata7;
    List<string> stringList3 = new List<string>();
    stringList3.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"BatchDelete\",\r\n        \"BatchDeleteRequest\": {\r\n            \"Items\": [\r\n                {\r\n                    \"TableName\": \"Customer\",\r\n                    \"Name\": \"FullName\"\r\n                },\r\n                {\r\n                    \"TableName\": \"Customer\",\r\n                    \"Name\": \"Age\"\r\n                }\r\n            ],\r\n            \"ShouldCascadeDelete\": true,\r\n            \"Options\": {\r\n                \"ContinueOnError\": false,\r\n                \"UseTransaction\": true\r\n            }\r\n        }\r\n    }\r\n}");
    operationMetadata8.ExampleRequests = stringList3;
    OperationMetadata operationMetadata9 = operationMetadata7;
    dictionary4["BatchDelete"] = operationMetadata9;
    Dictionary<string, OperationMetadata> dictionary5 = dictionary1;
    OperationMetadata operationMetadata10 = new OperationMetadata { RequiredParams = new string[1]
    {
      "BatchGetRequest"
    } };
    operationMetadata10.Description = "Retrieve multiple columns in a single operation. \r\nMandatory properties: BatchGetRequest (with Items containing column identifiers with TableName and Name). \r\nOptional: None.";
    OperationMetadata operationMetadata11 = operationMetadata10;
    List<string> stringList4 = new List<string>();
    stringList4.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"BatchGet\",\r\n        \"BatchGetRequest\": {\r\n            \"Items\": [\r\n                { \"TableName\": \"Customer\", \"Name\": \"FirstName\" },\r\n                { \"TableName\": \"Customer\", \"Name\": \"LastName\" },\r\n                { \"TableName\": \"Customer\", \"Name\": \"Email\" }\r\n            ]\r\n        }\r\n    }\r\n}");
    operationMetadata11.ExampleRequests = stringList4;
    OperationMetadata operationMetadata12 = operationMetadata10;
    dictionary5["BatchGet"] = operationMetadata12;
    Dictionary<string, OperationMetadata> dictionary6 = dictionary1;
    OperationMetadata operationMetadata13 = new OperationMetadata { RequiredParams = new string[1]
    {
      "BatchRenameRequest"
    } };
    operationMetadata13.Description = "Rename multiple columns in a single operation. \r\nMandatory properties: BatchRenameRequest (with Items containing columns with TableName, CurrentName, NewName). \r\nOptional: Options (with ContinueOnError, UseTransaction).";
    OperationMetadata operationMetadata14 = operationMetadata13;
    List<string> stringList5 = new List<string>();
    stringList5.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"BatchRename\",\r\n        \"BatchRenameRequest\": {\r\n            \"Items\": [\r\n                { \r\n                    \"TableName\": \"Customer\",\r\n                    \"CurrentName\": \"FName\", \r\n                    \"NewName\": \"FirstName\" \r\n                },\r\n                { \r\n                    \"TableName\": \"Customer\",\r\n                    \"CurrentName\": \"LName\", \r\n                    \"NewName\": \"LastName\" \r\n                }\r\n            ],\r\n            \"Options\": {\r\n                \"ContinueOnError\": false,\r\n                \"UseTransaction\": true\r\n            }\r\n        }\r\n    }\r\n}");
    operationMetadata14.ExampleRequests = stringList5;
    OperationMetadata operationMetadata15 = operationMetadata13;
    dictionary6["BatchRename"] = operationMetadata15;
    Dictionary<string, OperationMetadata> dictionary7 = dictionary1;
    OperationMetadata operationMetadata16 = new OperationMetadata { Description = "Describe the tool and its batch operations. \r\nMandatory properties: None. \r\nOptional: None." };
    List<string> stringList6 = new List<string>();
    stringList6.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Help\"\r\n    }\r\n}");
    operationMetadata16.ExampleRequests = stringList6;
    dictionary7["Help"] = operationMetadata16;
    Dictionary<string, OperationMetadata> dictionary8 = dictionary1;
    toolMetadata2.Operations = dictionary8;
    BatchColumnOperationsTool.toolMetadata = toolMetadata1;
  }
}
