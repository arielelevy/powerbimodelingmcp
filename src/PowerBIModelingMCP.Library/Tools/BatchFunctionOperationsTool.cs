// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Tools.BatchFunctionOperationsTool
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
public class BatchFunctionOperationsTool
{
  private readonly ILogger<BatchFunctionOperationsTool> _logger;
  public static readonly ToolMetadata toolMetadata;

  public BatchFunctionOperationsTool(ILogger<BatchFunctionOperationsTool> logger)
  {
    this._logger = logger;
  }

  [McpServerTool(Name = "batch_function_operations")]
  [Description("Perform batch operations on semantic model functions. Supported operations: Help, BatchCreate, BatchUpdate, BatchDelete, BatchGet, BatchRename. Use the Operation parameter to specify which batch operation to perform. Each operation includes options for error handling (ContinueOnError) and transactional behavior (UseTransaction).")]
  public BatchOperationResponse ExecuteBatchFunctionOperation(
    McpServer mcpServer,
    BatchFunctionOperationRequest request)
  {
    this._logger.LogDebug("Executing {ToolName}.{Operation}: ItemCount={ItemCount}, Connection={ConnectionName}", (object) nameof (BatchFunctionOperationsTool), (object) request.Operation, (object) (request.BatchCreateRequest?.Items?.Count ?? request.BatchUpdateRequest?.Items?.Count ?? request.BatchDeleteRequest?.Items?.Count ?? request.BatchGetRequest?.Items?.Count ?? request.BatchRenameRequest?.Items?.Count ?? 0), (object) (request.ConnectionName ?? "(last used)"));
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
        this._logger.LogWarning("Invalid operation '{Operation}' requested for {ToolName}. Valid operations: {ValidOperations}", (object) request.Operation, (object) nameof (BatchFunctionOperationsTool), (object) string.Join(", ", strArray1));
        return new BatchOperationResponse()
        {
          Success = false,
          Message = $"Invalid operation '{request.Operation}'. Valid operations are: {string.Join(", ", strArray1)}",
          Operation = request.Operation
        };
      }
      if (!this.ValidateRequest(request.Operation, request))
        return new BatchOperationResponse()
        {
          Success = false,
          Message = "Request validation failed",
          Operation = request.Operation
        };
      bool allowed = WriteGuard.IsWriteAllowed("").allowed;
      if (Enumerable.Contains<string>((IEnumerable<string>) strArray2, upperInvariant))
      {
        WriteOperationResult writeOperationResult = WriteGuard.ExecuteWriteOperationWithGuards(mcpServer, request.ConnectionName, request.Operation);
        if (!writeOperationResult.Success)
        {
          this._logger.LogWarning("{ToolName}.{Operation} blocked by write guard: {Reason}", (object) nameof (BatchFunctionOperationsTool), (object) request.Operation, (object) writeOperationResult.Message);
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
                    Message = $"Operation '{request.Operation}' is not implemented",
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
      this._logger.LogError(ex, "Error executing {ToolName}.{Operation}: {ErrorMessage}", (object) nameof (BatchFunctionOperationsTool), (object) request.Operation, (object) ex.Message);
      return new BatchOperationResponse()
      {
        Success = false,
        Message = "Error executing batch function operation: " + ex.Message,
        Operation = request.Operation
      };
    }
  }

  private BatchOperationResponse HandleBatchCreateOperation(BatchFunctionOperationRequest request)
  {
    try
    {
      if (string.IsNullOrEmpty(request.BatchCreateRequest.ConnectionName) && !string.IsNullOrEmpty(request.ConnectionName))
        request.BatchCreateRequest.ConnectionName = request.ConnectionName;
      BatchOperationResponse functions = BatchFunctionOperations.BatchCreateFunctions(request.BatchCreateRequest);
      int num1 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) functions.Results, (r => r.Success));
      int num2 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) functions.Results, (r => !r.Success));
      if (functions.Success)
        this._logger.LogInformation("{ToolName}.{Operation} completed: Total={Total}, Succeeded={Succeeded}, Failed={Failed}", (object) nameof (BatchFunctionOperationsTool), (object) request.Operation, (object) functions.Results.Count, (object) num1, (object) num2);
      else
        this._logger.LogWarning("{ToolName}.{Operation} completed with errors: Total={Total}, Succeeded={Succeeded}, Failed={Failed}, Message={Message}", (object) nameof (BatchFunctionOperationsTool), (object) request.Operation, (object) functions.Results.Count, (object) num1, (object) num2, (object) functions.Message);
      if (functions.Warnings != null && Enumerable.Any<string>((IEnumerable<string>) functions.Warnings))
      {
        foreach (string warning in functions.Warnings)
          this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (BatchFunctionOperationsTool), (object) request.Operation, (object) warning);
      }
      return functions;
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

  private BatchOperationResponse HandleBatchUpdateOperation(BatchFunctionOperationRequest request)
  {
    try
    {
      if (string.IsNullOrEmpty(request.BatchUpdateRequest.ConnectionName) && !string.IsNullOrEmpty(request.ConnectionName))
        request.BatchUpdateRequest.ConnectionName = request.ConnectionName;
      BatchOperationResponse operationResponse = BatchFunctionOperations.BatchUpdateFunctions(request.BatchUpdateRequest);
      int num1 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) operationResponse.Results, (r => r.Success));
      int num2 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) operationResponse.Results, (r => !r.Success));
      if (operationResponse.Success)
        this._logger.LogInformation("{ToolName}.{Operation} completed: Total={Total}, Succeeded={Succeeded}, Failed={Failed}", (object) nameof (BatchFunctionOperationsTool), (object) request.Operation, (object) operationResponse.Results.Count, (object) num1, (object) num2);
      else
        this._logger.LogWarning("{ToolName}.{Operation} completed with errors: Total={Total}, Succeeded={Succeeded}, Failed={Failed}, Message={Message}", (object) nameof (BatchFunctionOperationsTool), (object) request.Operation, (object) operationResponse.Results.Count, (object) num1, (object) num2, (object) operationResponse.Message);
      if (operationResponse.Warnings != null && Enumerable.Any<string>((IEnumerable<string>) operationResponse.Warnings))
      {
        foreach (string warning in operationResponse.Warnings)
          this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (BatchFunctionOperationsTool), (object) request.Operation, (object) warning);
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

  private BatchOperationResponse HandleBatchDeleteOperation(BatchFunctionOperationRequest request)
  {
    try
    {
      if (string.IsNullOrEmpty(request.BatchDeleteRequest.ConnectionName) && !string.IsNullOrEmpty(request.ConnectionName))
        request.BatchDeleteRequest.ConnectionName = request.ConnectionName;
      BatchOperationResponse operationResponse = BatchFunctionOperations.BatchDeleteFunctions(request.BatchDeleteRequest);
      int num1 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) operationResponse.Results, (r => r.Success));
      int num2 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) operationResponse.Results, (r => !r.Success));
      if (operationResponse.Success)
        this._logger.LogInformation("{ToolName}.{Operation} completed: Total={Total}, Succeeded={Succeeded}, Failed={Failed}", (object) nameof (BatchFunctionOperationsTool), (object) request.Operation, (object) operationResponse.Results.Count, (object) num1, (object) num2);
      else
        this._logger.LogWarning("{ToolName}.{Operation} completed with errors: Total={Total}, Succeeded={Succeeded}, Failed={Failed}, Message={Message}", (object) nameof (BatchFunctionOperationsTool), (object) request.Operation, (object) operationResponse.Results.Count, (object) num1, (object) num2, (object) operationResponse.Message);
      if (operationResponse.Warnings != null && Enumerable.Any<string>((IEnumerable<string>) operationResponse.Warnings))
      {
        foreach (string warning in operationResponse.Warnings)
          this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (BatchFunctionOperationsTool), (object) request.Operation, (object) warning);
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

  private BatchOperationResponse HandleBatchGetOperation(BatchFunctionOperationRequest request)
  {
    try
    {
      if (string.IsNullOrEmpty(request.BatchGetRequest.ConnectionName) && !string.IsNullOrEmpty(request.ConnectionName))
        request.BatchGetRequest.ConnectionName = request.ConnectionName;
      BatchOperationResponse functions = BatchFunctionOperations.BatchGetFunctions(request.BatchGetRequest);
      int num1 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) functions.Results, (r => r.Success));
      int num2 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) functions.Results, (r => !r.Success));
      if (functions.Success)
        this._logger.LogInformation("{ToolName}.{Operation} completed: Total={Total}, Succeeded={Succeeded}, Failed={Failed}", (object) nameof (BatchFunctionOperationsTool), (object) request.Operation, (object) functions.Results.Count, (object) num1, (object) num2);
      else
        this._logger.LogWarning("{ToolName}.{Operation} completed with errors: Total={Total}, Succeeded={Succeeded}, Failed={Failed}, Message={Message}", (object) nameof (BatchFunctionOperationsTool), (object) request.Operation, (object) functions.Results.Count, (object) num1, (object) num2, (object) functions.Message);
      if (functions.Warnings != null && Enumerable.Any<string>((IEnumerable<string>) functions.Warnings))
      {
        foreach (string warning in functions.Warnings)
          this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (BatchFunctionOperationsTool), (object) request.Operation, (object) warning);
      }
      return functions;
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

  private BatchOperationResponse HandleBatchRenameOperation(BatchFunctionOperationRequest request)
  {
    try
    {
      if (string.IsNullOrEmpty(request.BatchRenameRequest.ConnectionName) && !string.IsNullOrEmpty(request.ConnectionName))
        request.BatchRenameRequest.ConnectionName = request.ConnectionName;
      BatchOperationResponse operationResponse = BatchFunctionOperations.BatchRenameFunctions(request.BatchRenameRequest);
      int num1 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) operationResponse.Results, (r => r.Success));
      int num2 = Enumerable.Count<ItemResult>((IEnumerable<ItemResult>) operationResponse.Results, (r => !r.Success));
      if (operationResponse.Success)
        this._logger.LogInformation("{ToolName}.{Operation} completed: Total={Total}, Succeeded={Succeeded}, Failed={Failed}", (object) nameof (BatchFunctionOperationsTool), (object) request.Operation, (object) operationResponse.Results.Count, (object) num1, (object) num2);
      else
        this._logger.LogWarning("{ToolName}.{Operation} completed with errors: Total={Total}, Succeeded={Succeeded}, Failed={Failed}, Message={Message}", (object) nameof (BatchFunctionOperationsTool), (object) request.Operation, (object) operationResponse.Results.Count, (object) num1, (object) num2, (object) operationResponse.Message);
      if (operationResponse.Warnings != null && Enumerable.Any<string>((IEnumerable<string>) operationResponse.Warnings))
      {
        foreach (string warning in operationResponse.Warnings)
          this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (BatchFunctionOperationsTool), (object) request.Operation, (object) warning);
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

  private BatchOperationResponse HandleHelpOperation(
    BatchFunctionOperationRequest request,
    string[] operations)
  {
    try
    {
      string str = $"\r\n# Batch Function Operations Tool\r\n\r\nThis tool performs batch operations on semantic model functions efficiently.\r\n\r\n## Available Operations:\r\n\r\n{string.Join("\n", Enumerable.Select<string, string>(Enumerable.Where<string>((IEnumerable<string>) operations, (op => (op != "HELP"))), (op => $"- **{op}**: {BatchFunctionOperationsTool.toolMetadata.Operations[op].Description}")))}\r\n\r\n## Key Features:\r\n- **Transaction Support**: Use `UseTransaction: true` for ACID compliance\r\n- **Error Handling**: Use `ContinueOnError: true` to process remaining items after failures\r\n- **Detailed Results**: Each item's success/failure status with specific error messages\r\n- **Performance**: Optimized batch processing reduces individual operation overhead\r\n\r\n## Common Options:\r\n- `ContinueOnError`: Continue processing remaining items if one fails (default: false)\r\n- `UseTransaction`: Wrap all operations in a single transaction (default: false)\r\n\r\n## Example Usage:\r\nSee the individual operation examples in the tool metadata for detailed request formats.\r\n\r\n## Important Notes:\r\n- Functions are identified by name only (they are model-level objects)\r\n- BatchCreate requires unique function names\r\n- BatchUpdate only modifies provided properties (null/missing = no change)\r\n- BatchDelete operations are permanent and cannot be undone\r\n- Use BatchGet to retrieve function details for validation before modifications\r\n";
      this._logger.LogInformation("{ToolName}.{Operation} completed: Operations={OperationCount}", (object) nameof (BatchFunctionOperationsTool), (object) request.Operation, (object) operations.Length);
      return new BatchOperationResponse()
      {
        Success = true,
        Message = str,
        Operation = "Help"
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      return new BatchOperationResponse()
      {
        Success = false,
        Message = "Error generating help: " + ex.Message,
        Operation = "Help"
      };
    }
  }

  private bool ValidateRequest(string operation, BatchFunctionOperationRequest request)
  {
    OperationMetadata operationMetadata;
    if (!BatchFunctionOperationsTool.toolMetadata.Operations.TryGetValue(operation, out operationMetadata))
      return true;
    JsonObject requestDict = JsonSerializer.SerializeToNode<BatchFunctionOperationRequest>(request) as JsonObject;
    List<string> list1 = Enumerable.ToList<string>(Enumerable.Where<string>((IEnumerable<string>) operationMetadata.RequiredParams, (p => requestDict != null && requestDict[p] == null)));
    List<string> list2 = Enumerable.ToList<string>(Enumerable.Where<string>((IEnumerable<string>) operationMetadata.ForbiddenParams, (p => requestDict != null && requestDict[p] != null)));
    if (Enumerable.Any<string>((IEnumerable<string>) list1))
      throw new McpException($"Missing required parameters needed for {operation} operation: {string.Join(", ", (IEnumerable<string>) list1)}");
    if (Enumerable.Any<string>((IEnumerable<string>) list2))
      throw new McpException($"Forbidden parameters not allowed for {operation} operation: {string.Join(", ", (IEnumerable<string>) list2)}");
    return true;
  }

  static BatchFunctionOperationsTool()
  {
    ToolMetadata toolMetadata1 = new ToolMetadata();
    ToolMetadata toolMetadata2 = toolMetadata1;
    Dictionary<string, OperationMetadata> dictionary1 = new Dictionary<string, OperationMetadata>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    Dictionary<string, OperationMetadata> dictionary2 = dictionary1;
    OperationMetadata operationMetadata1 = new OperationMetadata { RequiredParams = new string[1]
    {
      "BatchCreateRequest"
    } };
    operationMetadata1.Description = "Creates multiple user-defined DAX functions in a single batch operation.\r\nMandatory properties: BatchCreateRequest (with Items list of FunctionCreate, each with Name, Expression).\r\nOptional: Items properties (Description, IsHidden, LineageTag, SourceLineageTag, Annotations, ExtendedProperties), Options (ContinueOnError, UseTransaction).";
    OperationMetadata operationMetadata2 = operationMetadata1;
    List<string> stringList1 = new List<string>();
    stringList1.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"BatchCreate\",\r\n        \"BatchCreateRequest\": {\r\n            \"Items\": [\r\n                { \r\n                    \"Name\": \"CircleArea\", \r\n                    \"Expression\": \"(radius : SCALAR NUMERIC) => PI() * radius * radius\",\r\n                    \"Description\": \"Calculates the area of a circle given its radius\"\r\n                },\r\n                { \r\n                    \"Name\": \"StringSplit\", \r\n                    \"Expression\": \"(s : STRING, delimiter : STRING) => VAR str = SUBSTITUTE(s, delimiter, \\\"|\\\") VAR len = PATHLENGTH(str) RETURN SELECTCOLUMNS(GENERATESERIES(1, len), \\\"Value\\\", PATHITEM(str, [Value], TEXT))\",\r\n                    \"Description\": \"Splits a string by a delimiter and returns a table\"\r\n                }\r\n            ],\r\n            \"Options\": {\r\n                \"ContinueOnError\": false,\r\n                \"UseTransaction\": true\r\n            }\r\n        }\r\n    }\r\n}");
    operationMetadata2.ExampleRequests = stringList1;
    OperationMetadata operationMetadata3 = operationMetadata1;
    dictionary2["BatchCreate"] = operationMetadata3;
    Dictionary<string, OperationMetadata> dictionary3 = dictionary1;
    OperationMetadata operationMetadata4 = new OperationMetadata { RequiredParams = new string[1]
    {
      "BatchUpdateRequest"
    } };
    operationMetadata4.Description = "Updates multiple existing user-defined DAX functions in a single batch operation. Names cannot be changed, use BatchRename operation instead.\r\nMandatory properties: BatchUpdateRequest (with Items list of FunctionUpdate, each with Name).\r\nOptional: Items properties (Expression, Description, IsHidden, LineageTag, SourceLineageTag, Annotations, ExtendedProperties), Options (ContinueOnError, UseTransaction).";
    OperationMetadata operationMetadata5 = operationMetadata4;
    List<string> stringList2 = new List<string>();
    stringList2.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"BatchUpdate\",\r\n        \"BatchUpdateRequest\": {\r\n            \"Items\": [\r\n                { \r\n                    \"Name\": \"CircleArea\", \r\n                    \"Description\": \"Updated: Calculates the area of a circle given its radius\",\r\n                    \"IsHidden\": false\r\n                },\r\n                { \r\n                    \"Name\": \"StringSplit\", \r\n                    \"IsHidden\": true\r\n                }\r\n            ],\r\n            \"Options\": {\r\n                \"ContinueOnError\": true,\r\n                \"UseTransaction\": false\r\n            }\r\n        }\r\n    }\r\n}");
    operationMetadata5.ExampleRequests = stringList2;
    OperationMetadata operationMetadata6 = operationMetadata4;
    dictionary3["BatchUpdate"] = operationMetadata6;
    Dictionary<string, OperationMetadata> dictionary4 = dictionary1;
    OperationMetadata operationMetadata7 = new OperationMetadata { RequiredParams = new string[1]
    {
      "BatchDeleteRequest"
    } };
    operationMetadata7.Description = "Deletes multiple user-defined DAX functions in a single batch operation.\r\nMandatory properties: BatchDeleteRequest (with Items list of function names).\r\nOptional: Options (ContinueOnError, UseTransaction).";
    OperationMetadata operationMetadata8 = operationMetadata7;
    List<string> stringList3 = new List<string>();
    stringList3.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"BatchDelete\",\r\n        \"BatchDeleteRequest\": {\r\n            \"Items\": [\"ObsoleteFunction1\", \"ObsoleteFunction2\"],\r\n            \"Options\": {\r\n                \"ContinueOnError\": false,\r\n                \"UseTransaction\": true\r\n            }\r\n        }\r\n    }\r\n}");
    operationMetadata8.ExampleRequests = stringList3;
    OperationMetadata operationMetadata9 = operationMetadata7;
    dictionary4["BatchDelete"] = operationMetadata9;
    Dictionary<string, OperationMetadata> dictionary5 = dictionary1;
    OperationMetadata operationMetadata10 = new OperationMetadata { RequiredParams = new string[1]
    {
      "BatchGetRequest"
    } };
    operationMetadata10.Description = "Retrieves detailed information for multiple user-defined DAX functions in a single batch operation.\r\nMandatory properties: BatchGetRequest (with Items list of function names).\r\nOptional: Options (ContinueOnError).";
    OperationMetadata operationMetadata11 = operationMetadata10;
    List<string> stringList4 = new List<string>();
    stringList4.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"BatchGet\",\r\n        \"BatchGetRequest\": {\r\n            \"Items\": [\"CircleArea\", \"StringSplit\", \"MyCustomFunction\"],\r\n            \"Options\": {\r\n                \"ContinueOnError\": true\r\n            }\r\n        }\r\n    }\r\n}");
    operationMetadata11.ExampleRequests = stringList4;
    OperationMetadata operationMetadata12 = operationMetadata10;
    dictionary5["BatchGet"] = operationMetadata12;
    Dictionary<string, OperationMetadata> dictionary6 = dictionary1;
    OperationMetadata operationMetadata13 = new OperationMetadata { RequiredParams = new string[1]
    {
      "BatchRenameRequest"
    } };
    operationMetadata13.Description = "Renames multiple user-defined DAX functions in a single batch operation.\r\nMandatory properties: BatchRenameRequest (with Items list of FunctionRename, each with CurrentName, NewName).\r\nOptional: Options (ContinueOnError, UseTransaction).";
    OperationMetadata operationMetadata14 = operationMetadata13;
    List<string> stringList5 = new List<string>();
    stringList5.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"BatchRename\",\r\n        \"BatchRenameRequest\": {\r\n            \"Items\": [\r\n                { \r\n                    \"CurrentName\": \"OldFunction1\", \r\n                    \"NewName\": \"NewFunction1\"\r\n                },\r\n                { \r\n                    \"CurrentName\": \"OldFunction2\", \r\n                    \"NewName\": \"NewFunction2\"\r\n                }\r\n            ],\r\n            \"Options\": {\r\n                \"ContinueOnError\": false,\r\n                \"UseTransaction\": true\r\n            }\r\n        }\r\n    }\r\n}");
    operationMetadata14.ExampleRequests = stringList5;
    OperationMetadata operationMetadata15 = operationMetadata13;
    dictionary6["BatchRename"] = operationMetadata15;
    Dictionary<string, OperationMetadata> dictionary7 = dictionary1;
    OperationMetadata operationMetadata16 = new OperationMetadata { Description = "Provides detailed information about the batch function operations tool and its capabilities.\r\nMandatory properties: None.\r\nOptional: None." };
    List<string> stringList6 = new List<string>();
    stringList6.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Help\"\r\n    }\r\n}");
    operationMetadata16.ExampleRequests = stringList6;
    dictionary7["Help"] = operationMetadata16;
    Dictionary<string, OperationMetadata> dictionary8 = dictionary1;
    toolMetadata2.Operations = dictionary8;
    BatchFunctionOperationsTool.toolMetadata = toolMetadata1;
  }
}
