// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Tools.TransactionOperationsTool
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using Microsoft.Extensions.Logging;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using PowerBIModelingMCP.Library.Common;
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
public class TransactionOperationsTool
{
  private readonly ILogger<TransactionOperationsTool> _logger;
  public static readonly ToolMetadata toolMetadata;

  public TransactionOperationsTool(ILogger<TransactionOperationsTool> logger)
  {
    this._logger = logger;
  }

  [McpServerTool(Name = "transaction_operations")]
  [Description("Perform operations on Analysis Services transactions. Supported operations: Help, Begin, Commit, Rollback, GetStatus, ListActive. Use the Operation parameter to specify which operation to perform.")]
  public TransactionOperationResponse ExecuteTransactionOperation(
    McpServer mcpServer,
    TransactionOperationRequest request)
  {
    this._logger.LogDebug("Executing {ToolName}.{Operation}: Connection={ConnectionName}", (object) nameof (TransactionOperationsTool), (object) request.Operation, (object) (request.ConnectionName ?? "(last used)"));
    try
    {
      string[] strArray1 = new string[6]
      {
        "BEGIN",
        "COMMIT",
        "ROLLBACK",
        "GETSTATUS",
        "LISTACTIVE",
        "HELP"
      };
      string[] strArray2 = new string[3]
      {
        "BEGIN",
        "COMMIT",
        "ROLLBACK"
      };
      if (!Enumerable.Contains<string>((IEnumerable<string>) strArray1, request.Operation.ToUpperInvariant()))
      {
        this._logger.LogWarning("Invalid operation '{Operation}' requested for {ToolName}. Valid operations: {ValidOperations}", (object) request.Operation, (object) nameof (TransactionOperationsTool), (object) string.Join(", ", strArray1));
        return TransactionOperationResponse.Forbidden(request.Operation, $"Invalid operation: {request.Operation}. Supported operations: {string.Join(", ", strArray1)}");
      }
      if (!this.ValidateRequest(request.Operation, request))
        throw new McpException($"Invalid request for {request.Operation} operation.");
      if (Enumerable.Contains<string>((IEnumerable<string>) strArray2, request.Operation.ToUpperInvariant()))
      {
        WriteOperationResult writeOperationResult = WriteGuard.ExecuteWriteOperationWithGuards(mcpServer, request.ConnectionName, request.Operation);
        if (!writeOperationResult.Success)
        {
          this._logger.LogWarning("{ToolName}.{Operation} blocked by write guard: {Reason}", (object) nameof (TransactionOperationsTool), (object) request.Operation, (object) writeOperationResult.Message);
          return new TransactionOperationResponse()
          {
            Success = false,
            Message = writeOperationResult.Message,
            Operation = request.Operation
          };
        }
      }
      bool allowed = WriteGuard.IsWriteAllowed("").allowed;
      string upperInvariant = request.Operation.ToUpperInvariant();
      return (upperInvariant == "BEGIN") ? this.HandleBeginOperation(request) : ((upperInvariant == "COMMIT") ? this.HandleCommitOperation(request) : ((upperInvariant == "ROLLBACK") ? this.HandleRollbackOperation(request) : ((upperInvariant == "GETSTATUS") ? this.HandleGetStatusOperation(request) : ((upperInvariant == "LISTACTIVE") ? this.HandleListActiveOperation(request) : ((upperInvariant == "HELP") ? this.HandleHelpOperation(request, allowed ? strArray1 : Enumerable.ToArray<string>(Enumerable.Except<string>((IEnumerable<string>) strArray1, (IEnumerable<string>) strArray2))) : TransactionOperationResponse.Forbidden(request.Operation, $"Operation {request.Operation} is not implemented"))))));
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Error executing {ToolName}.{Operation}: {ErrorMessage}", (object) nameof (TransactionOperationsTool), (object) request.Operation, (object) ex.Message);
      return new TransactionOperationResponse()
      {
        Success = false,
        Message = "Error executing transaction operation: " + ex.Message,
        Operation = request.Operation,
        TransactionId = request.TransactionId
      };
    }
  }

  private TransactionOperationResponse HandleBeginOperation(TransactionOperationRequest request)
  {
    try
    {
      TransactionBeginResult transactionBeginResult = TransactionOperations.BeginTransaction(request.ConnectionName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: ConnectionName={ConnectionName}, TransactionId={TransactionId}", (object) nameof (TransactionOperationsTool), (object) "Begin", (object) (request.ConnectionName ?? "(last used)"), (object) transactionBeginResult.TransactionId);
      return new TransactionOperationResponse()
      {
        Success = true,
        Message = $"Transaction '{transactionBeginResult.TransactionId}' started successfully",
        Operation = request.Operation,
        TransactionId = transactionBeginResult.TransactionId,
        Data = (object) transactionBeginResult
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      TransactionOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new TransactionOperationResponse()
      {
        Success = false,
        Message = "Error starting transaction: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private TransactionOperationResponse HandleCommitOperation(TransactionOperationRequest request)
  {
    try
    {
      TransactionCommitResult transactionCommitResult = TransactionOperations.CommitTransaction(request.ConnectionName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: ConnectionName={ConnectionName}, TransactionId={TransactionId}, OperationCount={OperationCount}", (object) nameof (TransactionOperationsTool), (object) "Commit", (object) (request.ConnectionName ?? "(last used)"), (object) transactionCommitResult.TransactionId, (object) transactionCommitResult.OperationCount);
      TransactionOperationResponse operationResponse = new TransactionOperationResponse { Success = true };
      operationResponse.Message = $"Transaction '{transactionCommitResult.TransactionId}' committed successfully with {transactionCommitResult.OperationCount} operations";
      operationResponse.Operation = request.Operation;
      operationResponse.TransactionId = transactionCommitResult.TransactionId;
      operationResponse.Data = (object) transactionCommitResult;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      TransactionOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new TransactionOperationResponse()
      {
        Success = false,
        Message = "Error committing transaction: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private TransactionOperationResponse HandleRollbackOperation(TransactionOperationRequest request)
  {
    try
    {
      TransactionRollbackResult transactionRollbackResult = TransactionOperations.RollbackTransaction(request.ConnectionName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: ConnectionName={ConnectionName}, TransactionId={TransactionId}", (object) nameof (TransactionOperationsTool), (object) "Rollback", (object) (request.ConnectionName ?? "(last used)"), (object) transactionRollbackResult.TransactionId);
      return new TransactionOperationResponse()
      {
        Success = true,
        Message = $"Transaction '{transactionRollbackResult.TransactionId}' rolled back successfully",
        Operation = request.Operation,
        TransactionId = transactionRollbackResult.TransactionId,
        Data = (object) transactionRollbackResult
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      TransactionOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new TransactionOperationResponse()
      {
        Success = false,
        Message = "Error rolling back transaction: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private TransactionOperationResponse HandleGetStatusOperation(TransactionOperationRequest request)
  {
    try
    {
      TransactionStatusResult transactionStatus = TransactionOperations.GetTransactionStatus(request.ConnectionName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: ConnectionName={ConnectionName}, TransactionId={TransactionId}, Status={Status}", (object) nameof (TransactionOperationsTool), (object) "GetStatus", (object) (request.ConnectionName ?? "(last used)"), (object) transactionStatus.TransactionId, (object) transactionStatus.Status);
      return new TransactionOperationResponse()
      {
        Success = true,
        Message = "Transaction status: " + transactionStatus.Status,
        Operation = request.Operation,
        TransactionId = transactionStatus.TransactionId,
        Data = (object) transactionStatus
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      TransactionOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new TransactionOperationResponse()
      {
        Success = false,
        Message = "Error getting transaction status: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private TransactionOperationResponse HandleListActiveOperation(TransactionOperationRequest request)
  {
    try
    {
      List<ActiveTransactionInfo> activeTransactionInfoList = TransactionOperations.ListActiveTransactions();
      this._logger.LogInformation("{ToolName}.{Operation} completed: Count={Count}", (object) nameof (TransactionOperationsTool), (object) "ListActive", (object) activeTransactionInfoList.Count);
      TransactionOperationResponse operationResponse = new TransactionOperationResponse { Success = true };
      operationResponse.Message = $"Found {activeTransactionInfoList.Count} active transactions";
      operationResponse.Operation = request.Operation;
      operationResponse.Data = (object) activeTransactionInfoList;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      TransactionOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new TransactionOperationResponse()
      {
        Success = false,
        Message = "Error listing active transactions: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private TransactionOperationResponse HandleHelpOperation(
    TransactionOperationRequest request,
    string[] operations)
  {
    this._logger.LogInformation("{ToolName}.{Operation} completed: Operations={OperationCount}", (object) nameof (TransactionOperationsTool), (object) "Help", (object) operations.Length);
    return new TransactionOperationResponse()
    {
      Success = true,
      Message = "Tool description retrieved successfully",
      Operation = request.Operation,
      Help = (object) new
      {
        ToolName = "transaction_operations",
        Description = "Perform operations on Analysis Services transactions.",
        SupportedOperations = operations,
        Examples = Enumerable.Where<KeyValuePair<string, OperationMetadata>>((IEnumerable<KeyValuePair<string, OperationMetadata>>) TransactionOperationsTool.toolMetadata.Operations, (Func<KeyValuePair<string, OperationMetadata>, bool>) (p => Enumerable.Contains<string>((IEnumerable<string>) operations, p.Key))),
        Notes = new string[3]
        {
          "Use the Operation parameter to specify which operation to perform.",
          "Use the ConnectionName parameter to specify the connection to use for the operation.",
          "Use the TransactionId parameter to specify the transaction to operate on."
        }
      }
    };
  }

  private bool ValidateRequest(string operation, TransactionOperationRequest request)
  {
    OperationMetadata operationMetadata;
    if (!TransactionOperationsTool.toolMetadata.Operations.TryGetValue(operation, out operationMetadata))
      return true;
    JsonObject requestDict = JsonSerializer.SerializeToNode<TransactionOperationRequest>(request) as JsonObject;
    List<string> list1 = Enumerable.ToList<string>(Enumerable.Where<string>((IEnumerable<string>) operationMetadata.RequiredParams, (p => requestDict != null && requestDict[p] == null)));
    List<string> list2 = Enumerable.ToList<string>(Enumerable.Where<string>((IEnumerable<string>) operationMetadata.ForbiddenParams, (p => requestDict != null && requestDict[p] != null)));
    if (Enumerable.Any<string>((IEnumerable<string>) list1))
      throw new McpException($"Missing required parameters needed for {operation} operation: {string.Join(", ", (IEnumerable<string>) list1)}");
    if (Enumerable.Any<string>((IEnumerable<string>) list2))
      throw new McpException($"Forbidden parameters not allowed for {operation} operation: {string.Join(", ", (IEnumerable<string>) list2)}");
    return true;
  }

  static TransactionOperationsTool()
  {
    ToolMetadata toolMetadata1 = new ToolMetadata();
    ToolMetadata toolMetadata2 = toolMetadata1;
    Dictionary<string, OperationMetadata> dictionary1 = new Dictionary<string, OperationMetadata>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    Dictionary<string, OperationMetadata> dictionary2 = dictionary1;
    OperationMetadata operationMetadata1 = new OperationMetadata { Description = "Begin a new transaction for a connection. Creates a server-side transaction that can be used to \r\ngroup multiple operations together for atomic commit or rollback. Throws an exception if a \r\ntransaction is already active on the connection." };
    operationMetadata1.Tips = new string[3]
    {
      "Offline connections do not support transactions",
      "Only one transaction can be active per connection at a time",
      "Always commit or rollback transactions when done to free resources"
    };
    OperationMetadata operationMetadata2 = operationMetadata1;
    List<string> stringList1 = new List<string>();
    stringList1.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Begin\",\r\n        \"ConnectionName\": \"MyConnection\"\r\n    }\r\n}");
    operationMetadata2.ExampleRequests = stringList1;
    OperationMetadata operationMetadata3 = operationMetadata1;
    dictionary2["Begin"] = operationMetadata3;
    Dictionary<string, OperationMetadata> dictionary3 = dictionary1;
    OperationMetadata operationMetadata4 = new OperationMetadata { Description = "Commit the active transaction for a connection. Saves all pending changes to the database and \r\nends the transaction. Throws an exception if no transaction is currently active." };
    List<string> stringList2 = new List<string>();
    stringList2.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Commit\",\r\n        \"ConnectionName\": \"MyConnection\"\r\n    }\r\n}");
    operationMetadata4.ExampleRequests = stringList2;
    dictionary3["Commit"] = operationMetadata4;
    Dictionary<string, OperationMetadata> dictionary4 = dictionary1;
    OperationMetadata operationMetadata5 = new OperationMetadata { Description = "Rollback the active transaction for a connection. Discards all pending changes and ends the \r\ntransaction without saving to the database. Throws an exception if no transaction is currently active." };
    List<string> stringList3 = new List<string>();
    stringList3.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Rollback\",\r\n        \"ConnectionName\": \"MyConnection\"\r\n    }\r\n}");
    operationMetadata5.ExampleRequests = stringList3;
    dictionary4["Rollback"] = operationMetadata5;
    Dictionary<string, OperationMetadata> dictionary5 = dictionary1;
    OperationMetadata operationMetadata6 = new OperationMetadata { Description = "Get the status of the active transaction for a connection. Returns transaction details including \r\nID, start time, duration, operation count, and server transaction status." };
    List<string> stringList4 = new List<string>();
    stringList4.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"GetStatus\",\r\n        \"ConnectionName\": \"MyConnection\"\r\n    }\r\n}");
    operationMetadata6.ExampleRequests = stringList4;
    dictionary5["GetStatus"] = operationMetadata6;
    Dictionary<string, OperationMetadata> dictionary6 = dictionary1;
    OperationMetadata operationMetadata7 = new OperationMetadata { Description = "List all active transactions across all connections. Returns a collection of transaction information \r\nobjects containing transaction ID, start time, duration, operation count, database name, server name, \r\nand transaction type for each active transaction." };
    List<string> stringList5 = new List<string>();
    stringList5.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"ListActive\"\r\n    }\r\n}");
    operationMetadata7.ExampleRequests = stringList5;
    dictionary6["ListActive"] = operationMetadata7;
    Dictionary<string, OperationMetadata> dictionary7 = dictionary1;
    OperationMetadata operationMetadata8 = new OperationMetadata { Description = "Describe the transaction operations tool and its available operations. Returns tool information, \r\nsupported operations, and usage examples." };
    List<string> stringList6 = new List<string>();
    stringList6.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Help\"\r\n    }\r\n}");
    operationMetadata8.ExampleRequests = stringList6;
    dictionary7["Help"] = operationMetadata8;
    Dictionary<string, OperationMetadata> dictionary8 = dictionary1;
    toolMetadata2.Operations = dictionary8;
    TransactionOperationsTool.toolMetadata = toolMetadata1;
  }
}
