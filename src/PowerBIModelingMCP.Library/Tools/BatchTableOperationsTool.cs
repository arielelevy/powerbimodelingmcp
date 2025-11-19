// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Tools.BatchTableOperationsTool
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
public class BatchTableOperationsTool
{
  private readonly ILogger<BatchTableOperationsTool> _logger;
  public static readonly ToolMetadata toolMetadata;

  public BatchTableOperationsTool(ILogger<BatchTableOperationsTool> logger)
  {
    this._logger = logger;
  }

  [McpServerTool(Name = "batch_table_operations")]
  [Description("Perform batch operations on semantic model tables. Supported operations: Help, BatchCreate, BatchUpdate, BatchDelete, BatchGet, BatchRename. Use the Operation parameter to specify which batch operation to perform. Each operation includes options for error handling (ContinueOnError) and transactional behavior (UseTransaction).")]
  public BatchOperationResponse ExecuteBatchTableOperation(
    McpServer mcpServer,
    BatchTableOperationRequest request)
  {
    int num = request.BatchCreateRequest?.Items?.Count ?? request.BatchUpdateRequest?.Items?.Count ?? request.BatchDeleteRequest?.Items?.Count ?? request.BatchGetRequest?.Items?.Count ?? request.BatchRenameRequest?.Items?.Count ?? 0;
    this._logger.LogDebug("Executing {ToolName}.{Operation}: ItemCount={ItemCount}, Connection={ConnectionName}", (object) nameof (BatchTableOperationsTool), (object) request.Operation, (object) num, (object) (request.ConnectionName ?? "(last used)"));
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
        this._logger.LogWarning("Invalid operation '{Operation}' requested for {ToolName}. Valid operations: {ValidOperations}", (object) request.Operation, (object) nameof (BatchTableOperationsTool), (object) string.Join(", ", strArray1));
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
          this._logger.LogWarning("{ToolName}.{Operation} blocked by write guard: {Reason}", (object) nameof (BatchTableOperationsTool), (object) request.Operation, (object) writeOperationResult.Message);
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
      this._logger.LogError(ex, "Error executing {ToolName}.{Operation}: {ErrorMessage}", (object) nameof (BatchTableOperationsTool), (object) request.Operation, (object) ex.Message);
      return new BatchOperationResponse()
      {
        Success = false,
        Message = "Error executing batch table operation: " + ex.Message,
        Operation = request.Operation
      };
    }
  }

  private BatchOperationResponse HandleHelpOperation(
    BatchTableOperationRequest request,
    string[] operations)
  {
    try
    {
      List<object> objectList1 = new List<object>();
      foreach (string operation in operations)
      {
        if (BatchTableOperationsTool.toolMetadata.Operations.ContainsKey(operation))
          objectList1.Add((object) new
          {
            Operation = operation,
            Description = BatchTableOperationsTool.toolMetadata.Operations[operation]
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
      itemResult.Message = "Batch table operations tool metadata";
      List<string> list = Enumerable.ToList<string>((IEnumerable<string>) operations);
      List<object> objectList2 = objectList1;
      var data = new
      {
        ContinueOnError = "Boolean: Whether to continue processing remaining items when an error occurs (default: false)",
        UseTransaction = "Boolean: Whether to wrap the batch operation in a transaction for atomicity (default: true)"
      };
      List<string> stringList = new List<string>();
      stringList.Add("Table identifiers for BatchDelete and BatchGet operations use table names directly");
      stringList.Add("BatchCreate and BatchUpdate operations use full table definition objects");
      stringList.Add("BatchRename operations use rename definition objects with CurrentName and NewName");
      stringList.Add("Transaction support ensures all operations succeed or all fail together when UseTransaction is true");
      stringList.Add("ContinueOnError allows processing to continue even if individual items fail");
      stringList.Add("For calculated tables, use DAX expressions; for imported tables, use SQL queries and data source information");
      stringList.Add("Table deletion with cascade will remove all dependent objects including relationships, measures, and hierarchies");
      itemResult.Data = (object) new
      {
        Tool = "batch_table_operations",
        Description = "Perform batch operations on semantic model tables",
        SupportedOperations = list,
        OperationDetails = objectList2,
        BatchOptions = data,
        Notes = stringList
      };
      results.Add(itemResult);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Operations={OperationCount}", (object) nameof (BatchTableOperationsTool), (object) request.Operation, (object) operations.Length);
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

  private BatchOperationResponse HandleBatchCreateOperation(BatchTableOperationRequest request)
  {
    try
    {
      if (string.IsNullOrEmpty(request.BatchCreateRequest.ConnectionName) && !string.IsNullOrEmpty(request.ConnectionName))
        request.BatchCreateRequest.ConnectionName = request.ConnectionName;
      BatchOperationResponse tables = BatchTableOperations.BatchCreateTables(request.BatchCreateRequest);
      int num1 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) tables.Results, (r => r.Success));
      int num2 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) tables.Results, (r => !r.Success));
      if (tables.Success)
        this._logger.LogInformation("{ToolName}.{Operation} completed: Total={Total}, Succeeded={Succeeded}, Failed={Failed}", (object) nameof (BatchTableOperationsTool), (object) request.Operation, (object) tables.Results.Count, (object) num1, (object) num2);
      else
        this._logger.LogWarning("{ToolName}.{Operation} completed with errors: Total={Total}, Succeeded={Succeeded}, Failed={Failed}, Message={Message}", (object) nameof (BatchTableOperationsTool), (object) request.Operation, (object) tables.Results.Count, (object) num1, (object) num2, (object) tables.Message);
      if (tables.Warnings != null && Enumerable.Any<string>((IEnumerable<string>) tables.Warnings))
      {
        foreach (string warning in tables.Warnings)
          this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (BatchTableOperationsTool), (object) request.Operation, (object) warning);
      }
      return tables;
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

  private BatchOperationResponse HandleBatchUpdateOperation(BatchTableOperationRequest request)
  {
    try
    {
      if (string.IsNullOrEmpty(request.BatchUpdateRequest.ConnectionName) && !string.IsNullOrEmpty(request.ConnectionName))
        request.BatchUpdateRequest.ConnectionName = request.ConnectionName;
      BatchOperationResponse operationResponse = BatchTableOperations.BatchUpdateTables(request.BatchUpdateRequest);
      int num1 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) operationResponse.Results, (r => r.Success));
      int num2 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) operationResponse.Results, (r => !r.Success));
      if (operationResponse.Success)
        this._logger.LogInformation("{ToolName}.{Operation} completed: Total={Total}, Succeeded={Succeeded}, Failed={Failed}", (object) nameof (BatchTableOperationsTool), (object) request.Operation, (object) operationResponse.Results.Count, (object) num1, (object) num2);
      else
        this._logger.LogWarning("{ToolName}.{Operation} completed with errors: Total={Total}, Succeeded={Succeeded}, Failed={Failed}, Message={Message}", (object) nameof (BatchTableOperationsTool), (object) request.Operation, (object) operationResponse.Results.Count, (object) num1, (object) num2, (object) operationResponse.Message);
      if (operationResponse.Warnings != null && Enumerable.Any<string>((IEnumerable<string>) operationResponse.Warnings))
      {
        foreach (string warning in operationResponse.Warnings)
          this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (BatchTableOperationsTool), (object) request.Operation, (object) warning);
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

  private BatchOperationResponse HandleBatchDeleteOperation(BatchTableOperationRequest request)
  {
    try
    {
      if (string.IsNullOrEmpty(request.BatchDeleteRequest.ConnectionName) && !string.IsNullOrEmpty(request.ConnectionName))
        request.BatchDeleteRequest.ConnectionName = request.ConnectionName;
      BatchOperationResponse operationResponse = BatchTableOperations.BatchDeleteTables(request.BatchDeleteRequest);
      int num1 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) operationResponse.Results, (r => r.Success));
      int num2 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) operationResponse.Results, (r => !r.Success));
      if (operationResponse.Success)
        this._logger.LogInformation("{ToolName}.{Operation} completed: Total={Total}, Succeeded={Succeeded}, Failed={Failed}", (object) nameof (BatchTableOperationsTool), (object) request.Operation, (object) operationResponse.Results.Count, (object) num1, (object) num2);
      else
        this._logger.LogWarning("{ToolName}.{Operation} completed with errors: Total={Total}, Succeeded={Succeeded}, Failed={Failed}, Message={Message}", (object) nameof (BatchTableOperationsTool), (object) request.Operation, (object) operationResponse.Results.Count, (object) num1, (object) num2, (object) operationResponse.Message);
      if (operationResponse.Warnings != null && Enumerable.Any<string>((IEnumerable<string>) operationResponse.Warnings))
      {
        foreach (string warning in operationResponse.Warnings)
          this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (BatchTableOperationsTool), (object) request.Operation, (object) warning);
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

  private BatchOperationResponse HandleBatchGetOperation(BatchTableOperationRequest request)
  {
    try
    {
      if (string.IsNullOrEmpty(request.BatchGetRequest.ConnectionName) && !string.IsNullOrEmpty(request.ConnectionName))
        request.BatchGetRequest.ConnectionName = request.ConnectionName;
      BatchOperationResponse tables = BatchTableOperations.BatchGetTables(request.BatchGetRequest);
      int num1 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) tables.Results, (r => r.Success));
      int num2 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) tables.Results, (r => !r.Success));
      if (tables.Success)
        this._logger.LogInformation("{ToolName}.{Operation} completed: Total={Total}, Succeeded={Succeeded}, Failed={Failed}", (object) nameof (BatchTableOperationsTool), (object) request.Operation, (object) tables.Results.Count, (object) num1, (object) num2);
      else
        this._logger.LogWarning("{ToolName}.{Operation} completed with errors: Total={Total}, Succeeded={Succeeded}, Failed={Failed}, Message={Message}", (object) nameof (BatchTableOperationsTool), (object) request.Operation, (object) tables.Results.Count, (object) num1, (object) num2, (object) tables.Message);
      if (tables.Warnings != null && Enumerable.Any<string>((IEnumerable<string>) tables.Warnings))
      {
        foreach (string warning in tables.Warnings)
          this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (BatchTableOperationsTool), (object) request.Operation, (object) warning);
      }
      return tables;
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

  private BatchOperationResponse HandleBatchRenameOperation(BatchTableOperationRequest request)
  {
    try
    {
      if (string.IsNullOrEmpty(request.BatchRenameRequest.ConnectionName) && !string.IsNullOrEmpty(request.ConnectionName))
        request.BatchRenameRequest.ConnectionName = request.ConnectionName;
      BatchOperationResponse operationResponse = BatchTableOperations.BatchRenameTables(request.BatchRenameRequest);
      int num1 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) operationResponse.Results, (r => r.Success));
      int num2 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) operationResponse.Results, (r => !r.Success));
      if (operationResponse.Success)
        this._logger.LogInformation("{ToolName}.{Operation} completed: Total={Total}, Succeeded={Succeeded}, Failed={Failed}", (object) nameof (BatchTableOperationsTool), (object) request.Operation, (object) operationResponse.Results.Count, (object) num1, (object) num2);
      else
        this._logger.LogWarning("{ToolName}.{Operation} completed with errors: Total={Total}, Succeeded={Succeeded}, Failed={Failed}, Message={Message}", (object) nameof (BatchTableOperationsTool), (object) request.Operation, (object) operationResponse.Results.Count, (object) num1, (object) num2, (object) operationResponse.Message);
      if (operationResponse.Warnings != null && Enumerable.Any<string>((IEnumerable<string>) operationResponse.Warnings))
      {
        foreach (string warning in operationResponse.Warnings)
          this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (BatchTableOperationsTool), (object) request.Operation, (object) warning);
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

  private bool ValidateRequest(string operation, BatchTableOperationRequest request)
  {
    OperationMetadata operationMetadata;
    if (!BatchTableOperationsTool.toolMetadata.Operations.TryGetValue(operation, out operationMetadata))
      return true;
    JsonObject requestDict = JsonSerializer.SerializeToNode<BatchTableOperationRequest>(request) as JsonObject;
    List<string> list1 = Enumerable.ToList<string>(Enumerable.Where<string>((IEnumerable<string>) operationMetadata.RequiredParams, (p => requestDict != null && requestDict[p] == null)));
    List<string> list2 = Enumerable.ToList<string>(Enumerable.Where<string>((IEnumerable<string>) operationMetadata.ForbiddenParams, (p => requestDict != null && requestDict[p] != null)));
    if (Enumerable.Any<string>((IEnumerable<string>) list1))
      throw new McpException($"Missing required parameters needed for {operation} operation: {string.Join(", ", (IEnumerable<string>) list1)}");
    if (Enumerable.Any<string>((IEnumerable<string>) list2))
      throw new McpException($"Forbidden parameters not allowed for {operation} operation: {string.Join(", ", (IEnumerable<string>) list2)}");
    return true;
  }

  static BatchTableOperationsTool()
  {
    ToolMetadata toolMetadata1 = new ToolMetadata();
    ToolMetadata toolMetadata2 = toolMetadata1;
    Dictionary<string, OperationMetadata> dictionary1 = new Dictionary<string, OperationMetadata>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    Dictionary<string, OperationMetadata> dictionary2 = dictionary1;
    OperationMetadata operationMetadata1 = new OperationMetadata { RequiredParams = new string[1]
    {
      "BatchCreateRequest"
    } };
    operationMetadata1.Description = "Create multiple tables in a single batch operation with transactional support.\r\nMandatory properties: BatchCreateRequest (with Items containing tables with Name, and one of DaxExpression or MExpression).\r\nOptional: Description, DataCategory, IsHidden, ShowAsVariationsOnly, IsPrivate, AlternateSourcePrecedence, ExcludeFromModelRefresh, LineageTag, SourceLineageTag, SystemManaged, PartitionName, Mode, Columns, Annotations, ExtendedProperties, DataSourceName (required when using SqlQuery), Options (with ContinueOnError, UseTransaction).\r\nNote: For DaxExpression tables, do not specify Columns as they are auto-derived from the DAX expression. For MExpression tables, Columns will be empty unless explicitly specified.";
    OperationMetadata operationMetadata2 = operationMetadata1;
    List<string> stringList1 = new List<string>();
    stringList1.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"BatchCreate\",\r\n        \"BatchCreateRequest\": {\r\n            \"Items\": [\r\n                { \r\n                    \"Name\": \"CalculatedTable1\", \r\n                    \"DaxExpression\": \"SUMMARIZE(Sales, Sales[Category])\",\r\n                    \"Description\": \"Summary of sales by category\"\r\n                },\r\n                { \r\n                    \"Name\": \"ImportTable1\", \r\n                    \"SqlQuery\": \"SELECT * FROM Products\",\r\n                    \"DataSourceName\": \"SqlDataSource\",\r\n                    \"Description\": \"Product dimension table\",\r\n                    \"Columns\": [\r\n                        {\r\n                            \"Name\": \"ProductID\",\r\n                            \"DataType\": \"Int64\",\r\n                            \"IsKey\": true,\r\n                            \"IsNullable\": false,\r\n                            \"Description\": \"Unique product identifier\"\r\n                        },\r\n                        {\r\n                            \"Name\": \"ProductName\",\r\n                            \"DataType\": \"String\",\r\n                            \"IsNullable\": false,\r\n                            \"Description\": \"Name of the product\",\r\n                            \"SummarizeBy\": \"None\"\r\n                        },\r\n                        {\r\n                            \"Name\": \"CategoryID\",\r\n                            \"DataType\": \"Int64\",\r\n                            \"IsNullable\": true,\r\n                            \"Description\": \"Product category identifier\"\r\n                        },\r\n                        {\r\n                            \"Name\": \"UnitPrice\",\r\n                            \"DataType\": \"Decimal\",\r\n                            \"FormatString\": \"$#,##0.00\",\r\n                            \"IsNullable\": true,\r\n                            \"SummarizeBy\": \"Average\",\r\n                            \"Description\": \"Unit price of the product\"\r\n                        },\r\n                        {\r\n                            \"Name\": \"UnitsInStock\",\r\n                            \"DataType\": \"Int64\",\r\n                            \"IsNullable\": true,\r\n                            \"SummarizeBy\": \"Sum\",\r\n                            \"Description\": \"Current stock quantity\"\r\n                        },\r\n                        {\r\n                            \"Name\": \"Discontinued\",\r\n                            \"DataType\": \"Boolean\",\r\n                            \"IsNullable\": false,\r\n                            \"Description\": \"Whether the product is discontinued\"\r\n                        }\r\n                    ]\r\n                }\r\n            ],\r\n            \"Options\": {\r\n                \"ContinueOnError\": false,\r\n                \"UseTransaction\": true\r\n            }\r\n        }\r\n    }\r\n}");
    operationMetadata2.ExampleRequests = stringList1;
    OperationMetadata operationMetadata3 = operationMetadata1;
    dictionary2["BatchCreate"] = operationMetadata3;
    Dictionary<string, OperationMetadata> dictionary3 = dictionary1;
    OperationMetadata operationMetadata4 = new OperationMetadata { RequiredParams = new string[1]
    {
      "BatchUpdateRequest"
    } };
    operationMetadata4.Description = "Update multiple existing tables in a single batch operation with transactional support.\r\nMandatory properties: BatchUpdateRequest (with Items containing tables with Name).\r\nOptional: Description, DataCategory, IsHidden, ShowAsVariationsOnly, IsPrivate, AlternateSourcePrecedence, ExcludeFromModelRefresh, LineageTag, SourceLineageTag, SystemManaged, Annotations, ExtendedProperties, Options (with ContinueOnError, UseTransaction).\r\nNote: Table names cannot be changed via Update - use BatchRename operation instead.";
    OperationMetadata operationMetadata5 = operationMetadata4;
    List<string> stringList2 = new List<string>();
    stringList2.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"BatchUpdate\",\r\n        \"BatchUpdateRequest\": {\r\n            \"Items\": [\r\n                { \r\n                    \"Name\": \"Sales\", \r\n                    \"Description\": \"Updated sales table description\",\r\n                    \"IsHidden\": false \r\n                },\r\n                { \r\n                    \"Name\": \"Products\", \r\n                    \"Description\": \"Updated products table description\",\r\n                    \"DataCategory\": \"ProductCategory\"\r\n                }\r\n            ],\r\n            \"Options\": {\r\n                \"ContinueOnError\": true,\r\n                \"UseTransaction\": false\r\n            }\r\n        }\r\n    }\r\n}");
    operationMetadata5.ExampleRequests = stringList2;
    OperationMetadata operationMetadata6 = operationMetadata4;
    dictionary3["BatchUpdate"] = operationMetadata6;
    Dictionary<string, OperationMetadata> dictionary4 = dictionary1;
    OperationMetadata operationMetadata7 = new OperationMetadata { RequiredParams = new string[1]
    {
      "BatchDeleteRequest"
    } };
    operationMetadata7.Description = "Delete multiple tables in a single batch operation with transactional support.\r\nMandatory properties: BatchDeleteRequest (with Items containing table names, ShouldCascadeDelete).\r\nOptional: Options (with ContinueOnError, UseTransaction).\r\nNote: When ShouldCascadeDelete is true, dependent objects (columns, relationships, measures, etc.) will be automatically deleted.";
    OperationMetadata operationMetadata8 = operationMetadata7;
    List<string> stringList3 = new List<string>();
    stringList3.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"BatchDelete\",\r\n        \"BatchDeleteRequest\": {\r\n            \"Items\": [\"TempTable1\", \"TempTable2\"],\r\n            \"ShouldCascadeDelete\": true,\r\n            \"Options\": {\r\n                \"ContinueOnError\": false,\r\n                \"UseTransaction\": true\r\n            }\r\n        }\r\n    }\r\n}");
    operationMetadata8.ExampleRequests = stringList3;
    OperationMetadata operationMetadata9 = operationMetadata7;
    dictionary4["BatchDelete"] = operationMetadata9;
    Dictionary<string, OperationMetadata> dictionary5 = dictionary1;
    OperationMetadata operationMetadata10 = new OperationMetadata { RequiredParams = new string[1]
    {
      "BatchGetRequest"
    } };
    operationMetadata10.Description = "Retrieve detailed information about multiple tables in a single batch operation.\r\nMandatory properties: BatchGetRequest (with Items containing table names).\r\nOptional: Options (with ContinueOnError).\r\nReturns comprehensive table information including properties, columns, measures, hierarchies, and partition details.";
    OperationMetadata operationMetadata11 = operationMetadata10;
    List<string> stringList4 = new List<string>();
    stringList4.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"BatchGet\",\r\n        \"BatchGetRequest\": {\r\n            \"Items\": [\"Sales\", \"Products\", \"Customers\"]\r\n        }\r\n    }\r\n}");
    operationMetadata11.ExampleRequests = stringList4;
    OperationMetadata operationMetadata12 = operationMetadata10;
    dictionary5["BatchGet"] = operationMetadata12;
    Dictionary<string, OperationMetadata> dictionary6 = dictionary1;
    OperationMetadata operationMetadata13 = new OperationMetadata { RequiredParams = new string[1]
    {
      "BatchRenameRequest"
    } };
    operationMetadata13.Description = "Rename multiple tables in a single batch operation with automatic reference updates and transactional support.\r\nMandatory properties: BatchRenameRequest (with Items containing rename definitions with CurrentName, NewName).\r\nOptional: Options (with ContinueOnError, UseTransaction).\r\nAll DAX expressions, relationships, and other references will be automatically updated.";
    OperationMetadata operationMetadata14 = operationMetadata13;
    List<string> stringList5 = new List<string>();
    stringList5.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"BatchRename\",\r\n        \"BatchRenameRequest\": {\r\n            \"Items\": [\r\n                { \r\n                    \"CurrentName\": \"OldTable1\", \r\n                    \"NewName\": \"NewTable1\" \r\n                },\r\n                { \r\n                    \"CurrentName\": \"OldTable2\", \r\n                    \"NewName\": \"NewTable2\" \r\n                }\r\n            ],\r\n            \"Options\": {\r\n                \"ContinueOnError\": false,\r\n                \"UseTransaction\": true\r\n            }\r\n        }\r\n    }\r\n}");
    operationMetadata14.ExampleRequests = stringList5;
    OperationMetadata operationMetadata15 = operationMetadata13;
    dictionary6["BatchRename"] = operationMetadata15;
    Dictionary<string, OperationMetadata> dictionary7 = dictionary1;
    OperationMetadata operationMetadata16 = new OperationMetadata { Description = "Display comprehensive help information about the batch table operations tool and all available operations.\r\nMandatory properties: None.\r\nOptional: None.\r\nReturns detailed documentation for each batch operation with examples and batch-specific options." };
    List<string> stringList6 = new List<string>();
    stringList6.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Help\"\r\n    }\r\n}");
    operationMetadata16.ExampleRequests = stringList6;
    dictionary7["Help"] = operationMetadata16;
    Dictionary<string, OperationMetadata> dictionary8 = dictionary1;
    toolMetadata2.Operations = dictionary8;
    BatchTableOperationsTool.toolMetadata = toolMetadata1;
  }
}
