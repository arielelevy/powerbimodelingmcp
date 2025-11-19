// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using PowerBIModelingMCP.Library.Common;
using PowerBIModelingMCP.Library.Common.DataStructures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

#nullable enable
namespace PowerBIModelingMCP.Library.Core;

public static class BatchFunctionOperations
{
  public static BatchOperationResponse BatchCreateFunctions(BatchCreateFunctionsRequest request)
  {
    Stopwatch stopwatch = Stopwatch.StartNew();
    List<string> warnings = new List<string>();
    BatchOperationResponse functions = new BatchOperationResponse()
    {
      Operation = "BatchCreate",
      Results = new List<ItemResult>(),
      Warnings = warnings
    };
    if (request.Items == null || !Enumerable.Any<FunctionCreate>((IEnumerable<FunctionCreate>) request.Items))
    {
      functions.Success = false;
      functions.Message = "No functions provided for batch creation";
      functions.Summary = new BatchSummary()
      {
        TotalItems = 0,
        SuccessCount = 0,
        FailureCount = 0,
        ExecutionTime = stopwatch.Elapsed
      };
      return functions;
    }
    int count = request.Items.Count;
    int num1 = 0;
    int num2 = 0;
    ConnectionInfo connectionInfo = ConnectionOperations.Get(request.ConnectionName);
    TransactionSetupResult transactionSetupResult = BatchTransactionHelper.HandleTransactionSetup(connectionInfo, request.Options.UseTransaction, request.ConnectionName, warnings);
    string transactionId = transactionSetupResult.TransactionId;
    bool ownsTransaction = transactionSetupResult.OwnsTransaction;
    try
    {
      for (int index = 0; index < request.Items.Count; ++index)
      {
        FunctionCreate def = request.Items[index];
        ItemResult itemResult = new ItemResult()
        {
          Index = index,
          ItemIdentifier = def.Name ?? "Unknown"
        };
        try
        {
          FunctionOperations.FunctionOperationResult function = FunctionOperations.CreateFunction(request.ConnectionName, def);
          itemResult.Success = (function.State == "Ready");
          itemResult.Message = (function.State == "Ready") ? $"Function '{def.Name}' created successfully" : function.ErrorMessage ?? $"Failed to create function '{def.Name}'";
          itemResult.Data = (object) function;
          if (itemResult.Success)
            ++num1;
          else
            ++num2;
        }
        catch (Exception ex)
        {
          itemResult.Success = false;
          itemResult.Message = $"Error creating function '{def.Name}': {ex.Message}";
          ++num2;
        }
        functions.Results.Add(itemResult);
        if (!itemResult.Success && !request.Options.ContinueOnError)
          break;
      }
      if (request.Options.UseTransaction && transactionId != null)
      {
        if (num2 == 0 || request.Options.ContinueOnError && num1 > 0)
        {
          if (ownsTransaction)
          {
            try
            {
              TransactionOperations.CommitTransaction(request.ConnectionName);
              functions.Message = $"Transaction committed. Created {num1} of {count} functions.";
            }
            catch (Exception ex)
            {
              functions.Message = "Failed to commit transaction: " + ex.Message;
              functions.Success = false;
            }
          }
          else
            functions.Message = $"Created {num1} of {count} functions in existing transaction. Transaction remains open for explicit commit.";
        }
        else
        {
          if (ownsTransaction)
          {
            try
            {
              TransactionOperations.RollbackTransaction(request.ConnectionName);
              functions.Message = $"Transaction rolled back due to failures. Created 0 of {count} functions.";
            }
            catch (Exception ex)
            {
              functions.Message = "Failed to rollback transaction: " + ex.Message;
            }
          }
          else
            functions.Message = "Batch create failed in existing transaction. Transaction remains open - you should rollback.";
          functions.Success = false;
        }
      }
      else if (num1 > 0)
        ConnectionOperations.SaveChangesIfNeeded(connectionInfo);
      functions.Success = num2 == 0;
      if (string.IsNullOrEmpty(functions.Message))
      {
        BatchOperationResponse operationResponse = functions;
        string str;
        if (!functions.Success)
          str = $"Batch operation completed with {num2} failures out of {count} items";
        else
          str = $"Successfully created {num1} functions";
        operationResponse.Message = str;
      }
    }
    catch (Exception ex)
    {
      if (transactionId != null & ownsTransaction)
      {
        try
        {
          TransactionOperations.RollbackTransaction(request.ConnectionName);
        }
        catch
        {
        }
      }
      functions.Success = false;
      functions.Message = "Batch create operation failed: " + ex.Message;
      num2 = count;
      num1 = 0;
    }
    stopwatch.Stop();
    functions.Summary = new BatchSummary()
    {
      TotalItems = count,
      SuccessCount = num1,
      FailureCount = num2,
      ExecutionTime = stopwatch.Elapsed
    };
    return functions;
  }

  public static BatchOperationResponse BatchUpdateFunctions(BatchUpdateFunctionsRequest request)
  {
    Stopwatch stopwatch = Stopwatch.StartNew();
    List<string> warnings = new List<string>();
    BatchOperationResponse operationResponse1 = new BatchOperationResponse()
    {
      Operation = "BatchUpdate",
      Results = new List<ItemResult>(),
      Warnings = warnings
    };
    if (request.Items == null || !Enumerable.Any<FunctionUpdate>((IEnumerable<FunctionUpdate>) request.Items))
    {
      operationResponse1.Success = false;
      operationResponse1.Message = "No functions provided for batch update";
      operationResponse1.Summary = new BatchSummary()
      {
        TotalItems = 0,
        SuccessCount = 0,
        FailureCount = 0,
        ExecutionTime = stopwatch.Elapsed
      };
      return operationResponse1;
    }
    int count = request.Items.Count;
    int num1 = 0;
    int num2 = 0;
    ConnectionInfo connectionInfo = ConnectionOperations.Get(request.ConnectionName);
    TransactionSetupResult transactionSetupResult = BatchTransactionHelper.HandleTransactionSetup(connectionInfo, request.Options.UseTransaction, request.ConnectionName, warnings);
    string transactionId = transactionSetupResult.TransactionId;
    bool ownsTransaction = transactionSetupResult.OwnsTransaction;
    try
    {
      for (int index = 0; index < request.Items.Count; ++index)
      {
        FunctionUpdate update = request.Items[index];
        ItemResult itemResult1 = new ItemResult()
        {
          Index = index,
          ItemIdentifier = update.Name ?? "Unknown"
        };
        try
        {
          FunctionOperations.FunctionOperationResult functionOperationResult = FunctionOperations.UpdateFunction(request.ConnectionName, update);
          itemResult1.Success = (functionOperationResult.State == "Ready");
          itemResult1.Data = (object) functionOperationResult;
          if (itemResult1.Success)
          {
            if (functionOperationResult.HasChanges)
            {
              itemResult1.Message = $"Function '{update.Name}' updated successfully";
            }
            else
            {
              itemResult1.Message = $"Function '{update.Name}' is already in the requested state";
              ItemResult itemResult2 = itemResult1;
              List<string> stringList = new List<string>();
              stringList.Add("No changes were detected. The function is already in the requested state.");
              itemResult2.Warnings = stringList;
            }
            ++num1;
          }
          else
          {
            itemResult1.Message = functionOperationResult.ErrorMessage ?? $"Failed to update function '{update.Name}'";
            ++num2;
          }
        }
        catch (Exception ex)
        {
          itemResult1.Success = false;
          itemResult1.Message = $"Error updating function '{update.Name}': {ex.Message}";
          ++num2;
        }
        operationResponse1.Results.Add(itemResult1);
        if (!itemResult1.Success && !request.Options.ContinueOnError)
          break;
      }
      List<ItemResult> list = Enumerable.ToList<ItemResult>(Enumerable.Where<ItemResult>((IEnumerable<ItemResult>) operationResponse1.Results, (r => r.Success && r.Data != null && r.Data.GetType().GetProperty("HasChanges")?.GetValue(r.Data) is bool flag && !flag)));
      if (Enumerable.Any<ItemResult>((IEnumerable<ItemResult>) list))
        operationResponse1.Warnings.Add($"{list.Count} of {count} functions had no changes detected - they are already in the requested state.");
      if (request.Options.UseTransaction && transactionId != null)
      {
        if (num2 == 0 || request.Options.ContinueOnError && num1 > 0)
        {
          if (ownsTransaction)
          {
            try
            {
              TransactionOperations.CommitTransaction(request.ConnectionName);
              int num3 = num1 - list.Count;
              BatchOperationResponse operationResponse2 = operationResponse1;
              string str;
              if (num3 <= 0)
                str = $"Transaction committed. All {count} functions were already in the requested state.";
              else
                str = $"Transaction committed. Updated {num3} of {count} functions.";
              operationResponse2.Message = str;
            }
            catch (Exception ex)
            {
              operationResponse1.Message = "Failed to commit transaction: " + ex.Message;
              operationResponse1.Success = false;
            }
          }
          else
          {
            int num4 = num1 - list.Count;
            BatchOperationResponse operationResponse3 = operationResponse1;
            string str;
            if (num4 <= 0)
              str = $"All {count} functions were already in the requested state. Transaction remains open for explicit commit.";
            else
              str = $"Updated {num4} of {count} functions in existing transaction. Transaction remains open for explicit commit.";
            operationResponse3.Message = str;
          }
        }
        else
        {
          if (ownsTransaction)
          {
            try
            {
              TransactionOperations.RollbackTransaction(request.ConnectionName);
              operationResponse1.Message = $"Transaction rolled back due to failures. Updated 0 of {count} functions.";
            }
            catch (Exception ex)
            {
              operationResponse1.Message = "Failed to rollback transaction: " + ex.Message;
            }
          }
          else
            operationResponse1.Message = "Batch update failed in existing transaction. Transaction remains open - you should rollback.";
          operationResponse1.Success = false;
        }
      }
      else if (num1 > 0)
        ConnectionOperations.SaveChangesIfNeeded(connectionInfo);
      operationResponse1.Success = num2 == 0;
      if (string.IsNullOrEmpty(operationResponse1.Message))
      {
        BatchOperationResponse operationResponse4 = operationResponse1;
        string str;
        if (!operationResponse1.Success)
          str = $"Batch operation completed with {num2} failures out of {count} items";
        else
          str = $"Successfully updated {num1} functions";
        operationResponse4.Message = str;
      }
    }
    catch (Exception ex)
    {
      if (transactionId != null & ownsTransaction)
      {
        try
        {
          TransactionOperations.RollbackTransaction(request.ConnectionName);
        }
        catch
        {
        }
      }
      operationResponse1.Success = false;
      operationResponse1.Message = "Batch update operation failed: " + ex.Message;
      num2 = count - num1;
    }
    stopwatch.Stop();
    operationResponse1.Summary = new BatchSummary()
    {
      TotalItems = count,
      SuccessCount = num1,
      FailureCount = num2,
      ExecutionTime = stopwatch.Elapsed
    };
    return operationResponse1;
  }

  public static BatchOperationResponse BatchDeleteFunctions(BatchDeleteFunctionsRequest request)
  {
    Stopwatch stopwatch = Stopwatch.StartNew();
    List<string> warnings = new List<string>();
    BatchOperationResponse operationResponse1 = new BatchOperationResponse()
    {
      Operation = "BatchDelete",
      Results = new List<ItemResult>(),
      Warnings = warnings
    };
    if (request.Items == null || !Enumerable.Any<string>((IEnumerable<string>) request.Items))
    {
      operationResponse1.Success = false;
      operationResponse1.Message = "No function names provided for batch deletion";
      operationResponse1.Summary = new BatchSummary()
      {
        TotalItems = 0,
        SuccessCount = 0,
        FailureCount = 0,
        ExecutionTime = stopwatch.Elapsed
      };
      return operationResponse1;
    }
    int count = request.Items.Count;
    int num1 = 0;
    int num2 = 0;
    ConnectionInfo connectionInfo = ConnectionOperations.Get(request.ConnectionName);
    TransactionSetupResult transactionSetupResult = BatchTransactionHelper.HandleTransactionSetup(connectionInfo, request.Options.UseTransaction, request.ConnectionName, warnings);
    string transactionId = transactionSetupResult.TransactionId;
    bool ownsTransaction = transactionSetupResult.OwnsTransaction;
    try
    {
      for (int index = 0; index < request.Items.Count; ++index)
      {
        string functionName = request.Items[index];
        ItemResult itemResult = new ItemResult()
        {
          Index = index,
          ItemIdentifier = functionName
        };
        try
        {
          FunctionOperations.DeleteFunction(request.ConnectionName, functionName);
          itemResult.Success = true;
          itemResult.Message = $"Function '{functionName}' deleted successfully";
          ++num1;
        }
        catch (Exception ex)
        {
          itemResult.Success = false;
          itemResult.Message = $"Error deleting function '{functionName}': {ex.Message}";
          ++num2;
        }
        operationResponse1.Results.Add(itemResult);
        if (!itemResult.Success && !request.Options.ContinueOnError)
          break;
      }
      if (request.Options.UseTransaction && transactionId != null)
      {
        if (num2 == 0 || request.Options.ContinueOnError && num1 > 0)
        {
          if (ownsTransaction)
          {
            try
            {
              TransactionOperations.CommitTransaction(request.ConnectionName);
              operationResponse1.Message = $"Transaction committed. Deleted {num1} of {count} functions.";
            }
            catch (Exception ex)
            {
              operationResponse1.Message = "Failed to commit transaction: " + ex.Message;
              operationResponse1.Success = false;
            }
          }
          else
            operationResponse1.Message = $"Deleted {num1} of {count} functions in existing transaction. Transaction remains open for explicit commit.";
        }
        else
        {
          if (ownsTransaction)
          {
            try
            {
              TransactionOperations.RollbackTransaction(request.ConnectionName);
              operationResponse1.Message = $"Transaction rolled back due to failures. Deleted 0 of {count} functions.";
            }
            catch (Exception ex)
            {
              operationResponse1.Message = "Failed to rollback transaction: " + ex.Message;
            }
          }
          else
            operationResponse1.Message = "Batch delete failed in existing transaction. Transaction remains open - you should rollback.";
          operationResponse1.Success = false;
        }
      }
      else if (num1 > 0)
        ConnectionOperations.SaveChangesIfNeeded(connectionInfo);
      operationResponse1.Success = num2 == 0;
      if (string.IsNullOrEmpty(operationResponse1.Message))
      {
        BatchOperationResponse operationResponse2 = operationResponse1;
        string str;
        if (!operationResponse1.Success)
          str = $"Batch operation completed with {num2} failures out of {count} items";
        else
          str = $"Successfully deleted {num1} functions";
        operationResponse2.Message = str;
      }
    }
    catch (Exception ex)
    {
      if (transactionId != null & ownsTransaction)
      {
        try
        {
          TransactionOperations.RollbackTransaction(request.ConnectionName);
        }
        catch
        {
        }
      }
      operationResponse1.Success = false;
      operationResponse1.Message = "Batch delete operation failed: " + ex.Message;
      num2 = count - num1;
    }
    stopwatch.Stop();
    operationResponse1.Summary = new BatchSummary()
    {
      TotalItems = count,
      SuccessCount = num1,
      FailureCount = num2,
      ExecutionTime = stopwatch.Elapsed
    };
    return operationResponse1;
  }

  public static BatchOperationResponse BatchGetFunctions(BatchGetFunctionsRequest request)
  {
    Stopwatch stopwatch = Stopwatch.StartNew();
    BatchOperationResponse functions = new BatchOperationResponse()
    {
      Operation = "BatchGet",
      Results = new List<ItemResult>()
    };
    if (request.Items == null || !Enumerable.Any<string>((IEnumerable<string>) request.Items))
    {
      functions.Success = false;
      functions.Message = "No function names provided for batch retrieval";
      functions.Summary = new BatchSummary()
      {
        TotalItems = 0,
        SuccessCount = 0,
        FailureCount = 0,
        ExecutionTime = stopwatch.Elapsed
      };
      return functions;
    }
    int count = request.Items.Count;
    int num1 = 0;
    int num2 = 0;
    try
    {
      for (int index = 0; index < request.Items.Count; ++index)
      {
        string functionName = request.Items[index];
        ItemResult itemResult = new ItemResult()
        {
          Index = index,
          ItemIdentifier = functionName
        };
        try
        {
          FunctionGet function = FunctionOperations.GetFunction(request.ConnectionName, functionName);
          itemResult.Success = true;
          itemResult.Message = $"Function '{functionName}' retrieved successfully";
          itemResult.Data = (object) function;
          ++num1;
        }
        catch (Exception ex)
        {
          itemResult.Success = false;
          itemResult.Message = $"Error retrieving function '{functionName}': {ex.Message}";
          ++num2;
        }
        functions.Results.Add(itemResult);
        if (!itemResult.Success && !request.Options.ContinueOnError)
          break;
      }
      functions.Success = num2 == 0;
      BatchOperationResponse operationResponse = functions;
      string str;
      if (!functions.Success)
        str = $"Batch operation completed with {num2} failures out of {count} items";
      else
        str = $"Successfully retrieved {num1} functions";
      operationResponse.Message = str;
    }
    catch (Exception ex)
    {
      functions.Success = false;
      functions.Message = "Batch get operation failed: " + ex.Message;
      num2 = count;
      num1 = 0;
    }
    stopwatch.Stop();
    functions.Summary = new BatchSummary()
    {
      TotalItems = count,
      SuccessCount = num1,
      FailureCount = num2,
      ExecutionTime = stopwatch.Elapsed
    };
    return functions;
  }

  public static BatchOperationResponse BatchRenameFunctions(BatchRenameFunctionsRequest request)
  {
    Stopwatch stopwatch = Stopwatch.StartNew();
    List<string> warnings = new List<string>();
    BatchOperationResponse operationResponse1 = new BatchOperationResponse()
    {
      Operation = "BatchRename",
      Results = new List<ItemResult>(),
      Warnings = warnings
    };
    if (request.Items == null || !Enumerable.Any<FunctionRename>((IEnumerable<FunctionRename>) request.Items))
    {
      operationResponse1.Success = false;
      operationResponse1.Message = "No function rename definitions provided";
      operationResponse1.Summary = new BatchSummary()
      {
        TotalItems = 0,
        SuccessCount = 0,
        FailureCount = 0,
        ExecutionTime = stopwatch.Elapsed
      };
      return operationResponse1;
    }
    int count = request.Items.Count;
    int num1 = 0;
    int num2 = 0;
    ConnectionInfo connectionInfo = ConnectionOperations.Get(request.ConnectionName);
    TransactionSetupResult transactionSetupResult = BatchTransactionHelper.HandleTransactionSetup(connectionInfo, request.Options.UseTransaction, request.ConnectionName, warnings);
    string transactionId = transactionSetupResult.TransactionId;
    bool ownsTransaction = transactionSetupResult.OwnsTransaction;
    try
    {
      for (int index = 0; index < request.Items.Count; ++index)
      {
        FunctionRename functionRename = request.Items[index];
        ItemResult itemResult = new ItemResult()
        {
          Index = index,
          ItemIdentifier = $"{functionRename.CurrentName} -> {functionRename.NewName}"
        };
        try
        {
          FunctionOperations.RenameFunction(request.ConnectionName, functionRename.CurrentName, functionRename.NewName);
          itemResult.Success = true;
          itemResult.Message = $"Function renamed from '{functionRename.CurrentName}' to '{functionRename.NewName}' successfully";
          ++num1;
        }
        catch (Exception ex)
        {
          itemResult.Success = false;
          itemResult.Message = $"Error renaming function from '{functionRename.CurrentName}' to '{functionRename.NewName}': {ex.Message}";
          ++num2;
        }
        operationResponse1.Results.Add(itemResult);
        if (!itemResult.Success && !request.Options.ContinueOnError)
          break;
      }
      if (request.Options.UseTransaction && transactionId != null)
      {
        if (num2 == 0 || request.Options.ContinueOnError && num1 > 0)
        {
          if (ownsTransaction)
          {
            try
            {
              TransactionOperations.CommitTransaction(request.ConnectionName);
              operationResponse1.Message = $"Transaction committed. Renamed {num1} of {count} functions.";
            }
            catch (Exception ex)
            {
              operationResponse1.Message = "Failed to commit transaction: " + ex.Message;
              operationResponse1.Success = false;
            }
          }
          else
            operationResponse1.Message = $"Renamed {num1} of {count} functions in existing transaction. Transaction remains open for explicit commit.";
        }
        else
        {
          if (ownsTransaction)
          {
            try
            {
              TransactionOperations.RollbackTransaction(request.ConnectionName);
              operationResponse1.Message = $"Transaction rolled back due to failures. Renamed 0 of {count} functions.";
            }
            catch (Exception ex)
            {
              operationResponse1.Message = "Failed to rollback transaction: " + ex.Message;
            }
          }
          else
            operationResponse1.Message = "Batch rename failed in existing transaction. Transaction remains open - you should rollback.";
          operationResponse1.Success = false;
        }
      }
      else if (num1 > 0)
        ConnectionOperations.SaveChangesIfNeeded(connectionInfo);
      operationResponse1.Success = num2 == 0;
      if (string.IsNullOrEmpty(operationResponse1.Message))
      {
        BatchOperationResponse operationResponse2 = operationResponse1;
        string str;
        if (!operationResponse1.Success)
          str = $"Batch operation completed with {num2} failures out of {count} items";
        else
          str = $"Successfully renamed {num1} functions";
        operationResponse2.Message = str;
      }
    }
    catch (Exception ex)
    {
      if (transactionId != null & ownsTransaction)
      {
        try
        {
          TransactionOperations.RollbackTransaction(request.ConnectionName);
        }
        catch
        {
        }
      }
      operationResponse1.Success = false;
      operationResponse1.Message = "Batch rename operation failed: " + ex.Message;
      num2 = count - num1;
    }
    stopwatch.Stop();
    operationResponse1.Summary = new BatchSummary()
    {
      TotalItems = count,
      SuccessCount = num1,
      FailureCount = num2,
      ExecutionTime = stopwatch.Elapsed
    };
    return operationResponse1;
  }
}
