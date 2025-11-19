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
public class BatchObjectTranslationOperationsTool
{
  private readonly ILogger<BatchObjectTranslationOperationsTool> _logger;
  public static readonly ToolMetadata toolMetadata;

  public BatchObjectTranslationOperationsTool(
    ILogger<BatchObjectTranslationOperationsTool> logger)
  {
    this._logger = logger;
  }

  [McpServerTool(Name = "batch_object_translation_operations")]
  [Description("Perform batch operations on semantic model object translations. Supported operations: Help, BatchCreate, BatchUpdate, BatchDelete, BatchGet. Use the Operation parameter to specify which batch operation to perform. Each operation includes options for error handling (ContinueOnError) and transactional behavior (UseTransaction). Object translations allow you to provide localized text for model objects like tables, measures, columns, hierarchies, and levels.")]
  public BatchOperationResponse ExecuteBatchObjectTranslationOperation(
    McpServer mcpServer,
    BatchObjectTranslationOperationRequest request)
  {
    this._logger.LogDebug("Executing {ToolName}.{Operation}: ItemCount={ItemCount}, Connection={ConnectionName}", (object) nameof (BatchObjectTranslationOperationsTool), (object) request.Operation, (object) (request.BatchCreateRequest?.Items?.Count ?? request.BatchUpdateRequest?.Items?.Count ?? request.BatchDeleteRequest?.Items?.Count ?? request.BatchGetRequest?.Items?.Count ?? 0), (object) (request.ConnectionName ?? "(last used)"));
    try
    {
      string[] strArray1 = new string[5]
      {
        "HELP",
        "BATCHCREATE",
        "BATCHUPDATE",
        "BATCHDELETE",
        "BATCHGET"
      };
      string[] strArray2 = new string[3]
      {
        "BATCHCREATE",
        "BATCHUPDATE",
        "BATCHDELETE"
      };
      string upperInvariant = request.Operation.ToUpperInvariant();
      if (!Enumerable.Contains<string>((IEnumerable<string>) strArray1, upperInvariant))
      {
        this._logger.LogWarning("Invalid operation '{Operation}' requested for {ToolName}. Valid operations: {ValidOperations}", (object) request.Operation, (object) nameof (BatchObjectTranslationOperationsTool), (object) string.Join(", ", strArray1));
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
          this._logger.LogWarning("{ToolName}.{Operation} blocked by write guard: {Reason}", (object) nameof (BatchObjectTranslationOperationsTool), (object) request.Operation, (object) writeOperationResult.Message);
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
              if ((upperInvariant == "BATCHGET"))
                operationResponse = this.HandleBatchGetOperation(request);
              else
                operationResponse = new BatchOperationResponse()
                {
                  Success = false,
                  Message = $"Operation {request.Operation} is not implemented",
                  Operation = request.Operation
                };
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
      this._logger.LogError(ex, "Error executing {ToolName}.{Operation}: {ErrorMessage}", (object) nameof (BatchObjectTranslationOperationsTool), (object) request.Operation, (object) ex.Message);
      return new BatchOperationResponse()
      {
        Success = false,
        Message = "Error executing batch object translation operation: " + ex.Message,
        Operation = request.Operation
      };
    }
  }

  private BatchOperationResponse HandleHelpOperation(
    BatchObjectTranslationOperationRequest request,
    string[] operations)
  {
    string str = "Batch Object Translation Operations Tool\r\n\r\n            This tool allows you to perform batch operations on semantic model object translations for internationalization/localization.\r\n\r\n            Supported Operations:\r\n            - Help: Show this help message\r\n            - BatchCreate: Create multiple object translations at once\r\n            - BatchUpdate: Update multiple existing object translations \r\n            - BatchDelete: Delete multiple object translations\r\n            - BatchGet: Retrieve multiple object translations\r\n\r\n            Object Types Supported:\r\n            - Model: Model-level translations\r\n            - Table: Table translations\r\n            - Measure: Measure translations (model-level or table-level)\r\n            - Column: Column translations\r\n            - Hierarchy: Hierarchy translations\r\n            - Level: Level translations within hierarchies\r\n            - KPI: KPI translations\r\n\r\n            Translatable Properties:\r\n            - Caption: Display name for the object\r\n            - Description: Description text for the object\r\n            - DisplayFolder: Display folder (for measures, columns, hierarchies)\r\n\r\n            Culture Names:\r\n            Use standard culture codes like \"en-US\", \"es-ES\", \"fr-FR\", etc.\r\n\r\n            Batch Options:\r\n            - ContinueOnError: Continue processing if individual items fail (default: false)\r\n            - UseTransaction: Wrap operations in a transaction for atomicity (default: true)\r\n\r\n            Example Usage:\r\n            Create Spanish translations for sales-related objects:\r\n            {\r\n                \"Operation\": \"BatchCreate\",\r\n                \"BatchCreateRequest\": {\r\n                    \"Items\": [\r\n                        { \"CultureName\": \"es-ES\", \"Property\": \"Caption\", \"ObjectType\": \"Table\", \"TableName\": \"Sales\", \"Value\": \"Ventas\" },\r\n                        { \"CultureName\": \"es-ES\", \"Property\": \"Caption\", \"ObjectType\": \"Measure\", \"MeasureName\": \"Total Sales\", \"Value\": \"Ventas Totales\" }\r\n                    ],\r\n                    \"Options\": { \"UseTransaction\": true }\r\n                }\r\n            }";
    this._logger.LogInformation("{ToolName}.{Operation} completed: Operations={OperationCount}", (object) nameof (BatchObjectTranslationOperationsTool), (object) request.Operation, (object) operations.Length);
    return new BatchOperationResponse()
    {
      Success = true,
      Message = str,
      Operation = request.Operation,
      Summary = new BatchSummary()
      {
        TotalItems = 0,
        SuccessCount = 0,
        FailureCount = 0
      }
    };
  }

  private BatchOperationResponse HandleBatchCreateOperation(
    BatchObjectTranslationOperationRequest request)
  {
    if (request.BatchCreateRequest == null)
      throw new McpException("BatchCreateRequest is required for BatchCreate operation");
    try
    {
      if (string.IsNullOrEmpty(request.BatchCreateRequest.ConnectionName) && !string.IsNullOrEmpty(request.ConnectionName))
        request.BatchCreateRequest.ConnectionName = request.ConnectionName;
      BatchOperationResponse objectTranslations = BatchObjectTranslationOperations.BatchCreateObjectTranslations(request.BatchCreateRequest);
      int num1 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) objectTranslations.Results, (r => r.Success));
      int num2 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) objectTranslations.Results, (r => !r.Success));
      if (objectTranslations.Success)
        this._logger.LogInformation("{ToolName}.{Operation} completed: Total={Total}, Succeeded={Succeeded}, Failed={Failed}", (object) nameof (BatchObjectTranslationOperationsTool), (object) request.Operation, (object) objectTranslations.Results.Count, (object) num1, (object) num2);
      else
        this._logger.LogWarning("{ToolName}.{Operation} completed with errors: Total={Total}, Succeeded={Succeeded}, Failed={Failed}, Message={Message}", (object) nameof (BatchObjectTranslationOperationsTool), (object) request.Operation, (object) objectTranslations.Results.Count, (object) num1, (object) num2, (object) objectTranslations.Message);
      if (objectTranslations.Warnings != null && Enumerable.Any<string>((IEnumerable<string>) objectTranslations.Warnings))
      {
        foreach (string warning in objectTranslations.Warnings)
          this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (BatchObjectTranslationOperationsTool), (object) request.Operation, (object) warning);
      }
      return objectTranslations;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      return new BatchOperationResponse()
      {
        Success = false,
        Message = "Error executing batch create operation: " + ex.Message,
        Operation = "BatchCreate"
      };
    }
  }

  private BatchOperationResponse HandleBatchUpdateOperation(
    BatchObjectTranslationOperationRequest request)
  {
    if (request.BatchUpdateRequest == null)
      throw new McpException("BatchUpdateRequest is required for BatchUpdate operation");
    try
    {
      if (string.IsNullOrEmpty(request.BatchUpdateRequest.ConnectionName) && !string.IsNullOrEmpty(request.ConnectionName))
        request.BatchUpdateRequest.ConnectionName = request.ConnectionName;
      BatchOperationResponse operationResponse = BatchObjectTranslationOperations.BatchUpdateObjectTranslations(request.BatchUpdateRequest);
      int num1 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) operationResponse.Results, (r => r.Success));
      int num2 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) operationResponse.Results, (r => !r.Success));
      if (operationResponse.Success)
        this._logger.LogInformation("{ToolName}.{Operation} completed: Total={Total}, Succeeded={Succeeded}, Failed={Failed}", (object) nameof (BatchObjectTranslationOperationsTool), (object) request.Operation, (object) operationResponse.Results.Count, (object) num1, (object) num2);
      else
        this._logger.LogWarning("{ToolName}.{Operation} completed with errors: Total={Total}, Succeeded={Succeeded}, Failed={Failed}, Message={Message}", (object) nameof (BatchObjectTranslationOperationsTool), (object) request.Operation, (object) operationResponse.Results.Count, (object) num1, (object) num2, (object) operationResponse.Message);
      if (operationResponse.Warnings != null && Enumerable.Any<string>((IEnumerable<string>) operationResponse.Warnings))
      {
        foreach (string warning in operationResponse.Warnings)
          this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (BatchObjectTranslationOperationsTool), (object) request.Operation, (object) warning);
      }
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      return new BatchOperationResponse()
      {
        Success = false,
        Message = "Error executing batch update operation: " + ex.Message,
        Operation = "BatchUpdate"
      };
    }
  }

  private BatchOperationResponse HandleBatchDeleteOperation(
    BatchObjectTranslationOperationRequest request)
  {
    if (request.BatchDeleteRequest == null)
      throw new McpException("BatchDeleteRequest is required for BatchDelete operation");
    try
    {
      if (string.IsNullOrEmpty(request.BatchDeleteRequest.ConnectionName) && !string.IsNullOrEmpty(request.ConnectionName))
        request.BatchDeleteRequest.ConnectionName = request.ConnectionName;
      BatchOperationResponse operationResponse = BatchObjectTranslationOperations.BatchDeleteObjectTranslations(request.BatchDeleteRequest);
      int num1 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) operationResponse.Results, (r => r.Success));
      int num2 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) operationResponse.Results, (r => !r.Success));
      if (operationResponse.Success)
        this._logger.LogInformation("{ToolName}.{Operation} completed: Total={Total}, Succeeded={Succeeded}, Failed={Failed}", (object) nameof (BatchObjectTranslationOperationsTool), (object) request.Operation, (object) operationResponse.Results.Count, (object) num1, (object) num2);
      else
        this._logger.LogWarning("{ToolName}.{Operation} completed with errors: Total={Total}, Succeeded={Succeeded}, Failed={Failed}, Message={Message}", (object) nameof (BatchObjectTranslationOperationsTool), (object) request.Operation, (object) operationResponse.Results.Count, (object) num1, (object) num2, (object) operationResponse.Message);
      if (operationResponse.Warnings != null && Enumerable.Any<string>((IEnumerable<string>) operationResponse.Warnings))
      {
        foreach (string warning in operationResponse.Warnings)
          this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (BatchObjectTranslationOperationsTool), (object) request.Operation, (object) warning);
      }
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      return new BatchOperationResponse()
      {
        Success = false,
        Message = "Error executing batch delete operation: " + ex.Message,
        Operation = "BatchDelete"
      };
    }
  }

  private BatchOperationResponse HandleBatchGetOperation(
    BatchObjectTranslationOperationRequest request)
  {
    if (request.BatchGetRequest == null)
      throw new McpException("BatchGetRequest is required for BatchGet operation");
    try
    {
      if (string.IsNullOrEmpty(request.BatchGetRequest.ConnectionName) && !string.IsNullOrEmpty(request.ConnectionName))
        request.BatchGetRequest.ConnectionName = request.ConnectionName;
      BatchOperationResponse objectTranslations = BatchObjectTranslationOperations.BatchGetObjectTranslations(request.BatchGetRequest);
      int num1 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) objectTranslations.Results, (r => r.Success));
      int num2 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) objectTranslations.Results, (r => !r.Success));
      if (objectTranslations.Success)
        this._logger.LogInformation("{ToolName}.{Operation} completed: Total={Total}, Succeeded={Succeeded}, Failed={Failed}", (object) nameof (BatchObjectTranslationOperationsTool), (object) request.Operation, (object) objectTranslations.Results.Count, (object) num1, (object) num2);
      else
        this._logger.LogWarning("{ToolName}.{Operation} completed with errors: Total={Total}, Succeeded={Succeeded}, Failed={Failed}, Message={Message}", (object) nameof (BatchObjectTranslationOperationsTool), (object) request.Operation, (object) objectTranslations.Results.Count, (object) num1, (object) num2, (object) objectTranslations.Message);
      if (objectTranslations.Warnings != null && Enumerable.Any<string>((IEnumerable<string>) objectTranslations.Warnings))
      {
        foreach (string warning in objectTranslations.Warnings)
          this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (BatchObjectTranslationOperationsTool), (object) request.Operation, (object) warning);
      }
      return objectTranslations;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      return new BatchOperationResponse()
      {
        Success = false,
        Message = "Error executing batch get operation: " + ex.Message,
        Operation = "BatchGet"
      };
    }
  }

  private bool ValidateRequest(string operation, BatchObjectTranslationOperationRequest request)
  {
    OperationMetadata operationMetadata;
    if (!BatchObjectTranslationOperationsTool.toolMetadata.Operations.TryGetValue(operation, out operationMetadata))
      return true;
    JsonObject requestDict = JsonSerializer.SerializeToNode<BatchObjectTranslationOperationRequest>(request) as JsonObject;
    List<string> list1 = Enumerable.ToList<string>(Enumerable.Where<string>((IEnumerable<string>) operationMetadata.RequiredParams, (p => requestDict != null && requestDict[p] == null)));
    List<string> list2 = Enumerable.ToList<string>(Enumerable.Where<string>((IEnumerable<string>) operationMetadata.ForbiddenParams, (p => requestDict != null && requestDict[p] != null)));
    if (Enumerable.Any<string>((IEnumerable<string>) list1))
      throw new McpException($"Missing required parameters needed for {operation} operation: {string.Join(", ", (IEnumerable<string>) list1)}");
    if (Enumerable.Any<string>((IEnumerable<string>) list2))
      throw new McpException($"Forbidden parameters not allowed for {operation} operation: {string.Join(", ", (IEnumerable<string>) list2)}");
    return true;
  }

  static BatchObjectTranslationOperationsTool()
  {
    ToolMetadata toolMetadata1 = new ToolMetadata();
    ToolMetadata toolMetadata2 = toolMetadata1;
    Dictionary<string, OperationMetadata> dictionary1 = new Dictionary<string, OperationMetadata>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    Dictionary<string, OperationMetadata> dictionary2 = dictionary1;
    OperationMetadata operationMetadata1 = new OperationMetadata { RequiredParams = new string[1]
    {
      "BatchCreateRequest"
    } };
    operationMetadata1.Description = "Create multiple object translations in a single operation.\r\nMandatory properties: BatchCreateRequest (with Items array containing ObjectTranslationCreate objects).\r\nEach item requires: CultureName, ObjectType, Property, Value, and object identification properties based on ObjectType.\r\nOptional: CreateCultureIfNotExists (default: true), Options (with ContinueOnError, UseTransaction).\r\nObject identification requirements by ObjectType:\r\n- Model: ModelName\r\n- Table: TableName  \r\n- Measure/KPI: MeasureName (TableName optional)\r\n- Column: TableName, ColumnName\r\n- Hierarchy: TableName, HierarchyName\r\n- Level: TableName, HierarchyName, LevelName";
    OperationMetadata operationMetadata2 = operationMetadata1;
    List<string> stringList1 = new List<string>();
    stringList1.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"BatchCreate\",\r\n        \"BatchCreateRequest\": {\r\n            \"Items\": [\r\n                { \r\n                    \"CultureName\": \"es-ES\", \r\n                    \"Property\": \"Caption\", \r\n                    \"ObjectType\": \"Table\",\r\n                    \"TableName\": \"Sales\",\r\n                    \"Value\": \"Ventas\",\r\n                    \"CreateCultureIfNotExists\": true\r\n                },\r\n                { \r\n                    \"CultureName\": \"es-ES\", \r\n                    \"Property\": \"Caption\", \r\n                    \"ObjectType\": \"Measure\",\r\n                    \"TableName\": \"Sales\",\r\n                    \"MeasureName\": \"Total Sales\",\r\n                    \"Value\": \"Ventas Totales\",\r\n                    \"CreateCultureIfNotExists\": true\r\n                },\r\n                { \r\n                    \"CultureName\": \"fr-FR\", \r\n                    \"Property\": \"Description\", \r\n                    \"ObjectType\": \"Column\",\r\n                    \"TableName\": \"Sales\",\r\n                    \"ColumnName\": \"Amount\",\r\n                    \"Value\": \"Montant de la vente\",\r\n                    \"CreateCultureIfNotExists\": true\r\n                }\r\n            ],\r\n            \"Options\": {\r\n                \"ContinueOnError\": false,\r\n                \"UseTransaction\": true\r\n            }\r\n        }\r\n    }\r\n}");
    operationMetadata2.ExampleRequests = stringList1;
    OperationMetadata operationMetadata3 = operationMetadata1;
    dictionary2["BatchCreate"] = operationMetadata3;
    Dictionary<string, OperationMetadata> dictionary3 = dictionary1;
    OperationMetadata operationMetadata4 = new OperationMetadata { RequiredParams = new string[1]
    {
      "BatchUpdateRequest"
    } };
    operationMetadata4.Description = "Update multiple object translations in a single operation.\r\nMandatory properties: BatchUpdateRequest (with Items array containing ObjectTranslationUpdate objects).\r\nEach item requires: CultureName, ObjectType, Property, Value, and object identification properties based on ObjectType.\r\nOptional: Options (with ContinueOnError, UseTransaction).\r\nObject identification requirements by ObjectType:\r\n- Model: ModelName\r\n- Table: TableName  \r\n- Measure/KPI: MeasureName (TableName optional)\r\n- Column: TableName, ColumnName\r\n- Hierarchy: TableName, HierarchyName\r\n- Level: TableName, HierarchyName, LevelName";
    OperationMetadata operationMetadata5 = operationMetadata4;
    List<string> stringList2 = new List<string>();
    stringList2.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"BatchUpdate\",\r\n        \"BatchUpdateRequest\": {\r\n            \"Items\": [\r\n                { \r\n                    \"CultureName\": \"es-ES\", \r\n                    \"Property\": \"Caption\", \r\n                    \"ObjectType\": \"Table\",\r\n                    \"TableName\": \"Sales\",\r\n                    \"Value\": \"Ventas Actualizadas\"\r\n                },\r\n                { \r\n                    \"CultureName\": \"fr-FR\", \r\n                    \"Property\": \"Description\", \r\n                    \"ObjectType\": \"Measure\",\r\n                    \"TableName\": \"Sales\",\r\n                    \"MeasureName\": \"Total Sales\",\r\n                    \"Value\": \"Total des ventes mis à jour\"\r\n                }\r\n            ],\r\n            \"Options\": {\r\n                \"ContinueOnError\": true,\r\n                \"UseTransaction\": false\r\n            }\r\n        }\r\n    }\r\n}");
    operationMetadata5.ExampleRequests = stringList2;
    OperationMetadata operationMetadata6 = operationMetadata4;
    dictionary3["BatchUpdate"] = operationMetadata6;
    Dictionary<string, OperationMetadata> dictionary4 = dictionary1;
    OperationMetadata operationMetadata7 = new OperationMetadata { RequiredParams = new string[1]
    {
      "BatchDeleteRequest"
    } };
    operationMetadata7.Description = "Delete multiple object translations in a single operation.\r\nMandatory properties: BatchDeleteRequest (with Items array containing ObjectTranslationDelete objects).\r\nEach item requires: CultureName, ObjectType, Property, and object identification properties based on ObjectType.\r\nOptional: Options (with ContinueOnError, UseTransaction).\r\nObject identification requirements by ObjectType:\r\n- Model: ModelName\r\n- Table: TableName  \r\n- Measure/KPI: MeasureName (TableName optional)\r\n- Column: TableName, ColumnName\r\n- Hierarchy: TableName, HierarchyName\r\n- Level: TableName, HierarchyName, LevelName";
    OperationMetadata operationMetadata8 = operationMetadata7;
    List<string> stringList3 = new List<string>();
    stringList3.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"BatchDelete\",\r\n        \"BatchDeleteRequest\": {\r\n            \"Items\": [\r\n                { \r\n                    \"CultureName\": \"es-ES\", \r\n                    \"Property\": \"Caption\", \r\n                    \"ObjectType\": \"Table\",\r\n                    \"TableName\": \"Sales\"\r\n                },\r\n                { \r\n                    \"CultureName\": \"fr-FR\", \r\n                    \"Property\": \"Description\", \r\n                    \"ObjectType\": \"Measure\",\r\n                    \"TableName\": \"Sales\",\r\n                    \"MeasureName\": \"Total Sales\"\r\n                }\r\n            ],\r\n            \"Options\": {\r\n                \"ContinueOnError\": false,\r\n                \"UseTransaction\": true\r\n            }\r\n        }\r\n    }\r\n}");
    operationMetadata8.ExampleRequests = stringList3;
    OperationMetadata operationMetadata9 = operationMetadata7;
    dictionary4["BatchDelete"] = operationMetadata9;
    Dictionary<string, OperationMetadata> dictionary5 = dictionary1;
    OperationMetadata operationMetadata10 = new OperationMetadata { RequiredParams = new string[1]
    {
      "BatchGetRequest"
    } };
    operationMetadata10.Description = "Retrieve multiple object translations in a single operation.\r\nMandatory properties: BatchGetRequest (with Items array containing ObjectTranslationBase objects).\r\nEach item requires: CultureName, ObjectType, Property, and object identification properties based on ObjectType.\r\nObject identification requirements by ObjectType:\r\n- Model: ModelName\r\n- Table: TableName  \r\n- Measure/KPI: MeasureName (TableName optional)\r\n- Column: TableName, ColumnName\r\n- Hierarchy: TableName, HierarchyName\r\n- Level: TableName, HierarchyName, LevelName";
    OperationMetadata operationMetadata11 = operationMetadata10;
    List<string> stringList4 = new List<string>();
    stringList4.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"BatchGet\",\r\n        \"BatchGetRequest\": {\r\n            \"Items\": [\r\n                { \r\n                    \"CultureName\": \"es-ES\", \r\n                    \"Property\": \"Caption\", \r\n                    \"ObjectType\": \"Table\",\r\n                    \"TableName\": \"Sales\"\r\n                },\r\n                { \r\n                    \"CultureName\": \"fr-FR\", \r\n                    \"Property\": \"Description\", \r\n                    \"ObjectType\": \"Measure\",\r\n                    \"TableName\": \"Sales\",\r\n                    \"MeasureName\": \"Total Sales\"\r\n                }\r\n            ]\r\n        }\r\n    }\r\n}");
    operationMetadata11.ExampleRequests = stringList4;
    OperationMetadata operationMetadata12 = operationMetadata10;
    dictionary5["BatchGet"] = operationMetadata12;
    Dictionary<string, OperationMetadata> dictionary6 = dictionary1;
    OperationMetadata operationMetadata13 = new OperationMetadata { Description = "Describe the tool and its batch operations.\r\nNo mandatory properties required." };
    List<string> stringList5 = new List<string>();
    stringList5.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Help\"\r\n    }\r\n}");
    operationMetadata13.ExampleRequests = stringList5;
    dictionary6["Help"] = operationMetadata13;
    Dictionary<string, OperationMetadata> dictionary7 = dictionary1;
    toolMetadata2.Operations = dictionary7;
    BatchObjectTranslationOperationsTool.toolMetadata = toolMetadata1;
  }
}
