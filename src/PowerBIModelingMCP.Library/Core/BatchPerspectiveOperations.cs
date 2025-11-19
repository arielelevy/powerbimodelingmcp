// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Core.BatchPerspectiveOperations
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using PowerBIModelingMCP.Library.Common;
using PowerBIModelingMCP.Library.Common.DataStructures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

#nullable enable
namespace PowerBIModelingMCP.Library.Core;

public static class BatchPerspectiveOperations
{
  public static BatchOperationResponse BatchAddPerspectiveTables(
    BatchAddPerspectiveTablesRequest request)
  {
    Stopwatch stopwatch = Stopwatch.StartNew();
    List<string> warnings = new List<string>();
    BatchOperationResponse operationResponse = new BatchOperationResponse()
    {
      Operation = "BatchAddTables",
      Results = new List<ItemResult>(),
      Warnings = warnings
    };
    if (request.Items == null || !Enumerable.Any<PerspectiveTableCreate>((IEnumerable<PerspectiveTableCreate>) request.Items))
    {
      operationResponse.Success = false;
      operationResponse.Message = "No perspective tables provided for batch addition";
      operationResponse.Summary = new BatchSummary()
      {
        TotalItems = 0,
        SuccessCount = 0,
        FailureCount = 0,
        ExecutionTime = stopwatch.Elapsed
      };
      return operationResponse;
    }
    if (string.IsNullOrWhiteSpace(request.PerspectiveName))
    {
      operationResponse.Success = false;
      operationResponse.Message = "Perspective name is required";
      operationResponse.Summary = new BatchSummary()
      {
        TotalItems = request.Items.Count,
        SuccessCount = 0,
        FailureCount = request.Items.Count,
        ExecutionTime = stopwatch.Elapsed
      };
      return operationResponse;
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
        PerspectiveTableCreate def = request.Items[index];
        ItemResult itemResult1 = new ItemResult { Index = index };
        string str1 = def.TableName;
        if (str1 == null)
          str1 = $"Item_{index}";
        itemResult1.ItemIdentifier = str1;
        ItemResult itemResult2 = itemResult1;
        try
        {
          PerspectiveOperationResult perspective = PerspectiveOperations.AddTableToPerspective(request.ConnectionName, request.PerspectiveName, def);
          itemResult2.Success = perspective.Success;
          ItemResult itemResult3 = itemResult2;
          string str2;
          if (!itemResult2.Success)
            str2 = $"Failed to add table '{def.TableName}' to perspective '{request.PerspectiveName}': {perspective.Message}";
          else
            str2 = $"Successfully added table '{def.TableName}' to perspective '{request.PerspectiveName}'";
          itemResult3.Message = str2;
          if (itemResult2.Success)
          {
            ++num1;
            if (transactionId != null)
              TransactionOperations.RecordOperation(connectionInfo, $"Added table '{def.TableName}' to perspective '{request.PerspectiveName}'");
          }
          else
            ++num2;
        }
        catch (Exception ex)
        {
          itemResult2.Success = false;
          itemResult2.Message = $"Error adding table '{def.TableName}' to perspective '{request.PerspectiveName}': {ex.Message}";
          ++num2;
        }
        operationResponse.Results.Add(itemResult2);
        if (!itemResult2.Success && !request.Options.ContinueOnError)
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
              operationResponse.Message = $"Transaction committed. Added {num1} of {count} tables to perspective.";
            }
            catch (Exception ex)
            {
              operationResponse.Message = "Failed to commit transaction: " + ex.Message;
              operationResponse.Success = false;
            }
          }
          else
            operationResponse.Message = $"Added {num1} of {count} tables to perspective in existing transaction. Transaction remains open for explicit commit.";
        }
        else
        {
          if (ownsTransaction)
          {
            try
            {
              TransactionOperations.RollbackTransaction(request.ConnectionName);
              operationResponse.Message = "Transaction rolled back due to errors. No tables were added to perspective.";
            }
            catch (Exception ex)
            {
              operationResponse.Message = "Failed to rollback transaction: " + ex.Message;
            }
          }
          else
            operationResponse.Message = "Batch add failed in existing transaction. Transaction remains open - you should rollback.";
          operationResponse.Success = false;
        }
      }
      else if (num1 > 0)
        operationResponse.Message = $"Added {num1} of {count} tables to perspective.";
      if (string.IsNullOrEmpty(operationResponse.Message))
        operationResponse.Message = num2 > 0 ? "Batch add operation completed with errors." : "Batch add operation completed successfully.";
      operationResponse.Success = num2 == 0 || request.Options.ContinueOnError && num1 > 0;
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
      operationResponse.Success = false;
      operationResponse.Message = "Batch add operation failed: " + ex.Message;
      num2 = count - num1;
    }
    stopwatch.Stop();
    operationResponse.Summary = new BatchSummary()
    {
      TotalItems = count,
      SuccessCount = num1,
      FailureCount = num2,
      ExecutionTime = stopwatch.Elapsed
    };
    return operationResponse;
  }

  public static BatchOperationResponse BatchUpdatePerspectiveTables(
    BatchUpdatePerspectiveTablesRequest request)
  {
    Stopwatch stopwatch = Stopwatch.StartNew();
    List<string> warnings = new List<string>();
    BatchOperationResponse operationResponse1 = new BatchOperationResponse()
    {
      Operation = "BatchUpdateTables",
      Results = new List<ItemResult>(),
      Warnings = warnings
    };
    if (request.Items == null || !Enumerable.Any<PerspectiveTableUpdate>((IEnumerable<PerspectiveTableUpdate>) request.Items))
    {
      operationResponse1.Success = false;
      operationResponse1.Message = "No perspective tables provided for batch update";
      operationResponse1.Summary = new BatchSummary()
      {
        TotalItems = 0,
        SuccessCount = 0,
        FailureCount = 0,
        ExecutionTime = stopwatch.Elapsed
      };
      return operationResponse1;
    }
    if (string.IsNullOrWhiteSpace(request.PerspectiveName))
    {
      operationResponse1.Success = false;
      operationResponse1.Message = "Perspective name is required";
      operationResponse1.Summary = new BatchSummary()
      {
        TotalItems = request.Items.Count,
        SuccessCount = 0,
        FailureCount = request.Items.Count,
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
        PerspectiveTableUpdate update = request.Items[index];
        ItemResult itemResult1 = new ItemResult { Index = index };
        string str = update.TableName;
        if (str == null)
          str = $"Item_{index}";
        itemResult1.ItemIdentifier = str;
        ItemResult itemResult2 = itemResult1;
        try
        {
          PerspectiveOperationResult perspectiveOperationResult = PerspectiveOperations.UpdatePerspectiveTable(request.ConnectionName, request.PerspectiveName, update.TableName, update);
          itemResult2.Success = perspectiveOperationResult.Success;
          itemResult2.Data = (object) perspectiveOperationResult;
          if (itemResult2.Success)
          {
            if (perspectiveOperationResult.HasChanges)
            {
              itemResult2.Message = $"Successfully updated table '{update.TableName}' in perspective '{request.PerspectiveName}'";
            }
            else
            {
              itemResult2.Message = $"Table '{update.TableName}' in perspective '{request.PerspectiveName}' is already in the requested state";
              ItemResult itemResult3 = itemResult2;
              List<string> stringList = new List<string>();
              stringList.Add("No changes were detected. The perspective table is already in the requested state.");
              itemResult3.Warnings = stringList;
            }
            ++num1;
            if (transactionId != null)
              TransactionOperations.RecordOperation(connectionInfo, $"Updated table '{update.TableName}' in perspective '{request.PerspectiveName}'");
          }
          else
          {
            itemResult2.Message = $"Failed to update table '{update.TableName}' in perspective '{request.PerspectiveName}': {perspectiveOperationResult.Message}";
            ++num2;
          }
        }
        catch (Exception ex)
        {
          itemResult2.Success = false;
          itemResult2.Message = $"Error updating table '{update.TableName}' in perspective '{request.PerspectiveName}': {ex.Message}";
          ++num2;
        }
        operationResponse1.Results.Add(itemResult2);
        if (!itemResult2.Success && !request.Options.ContinueOnError)
          break;
      }
      List<ItemResult> list = Enumerable.ToList<ItemResult>(Enumerable.Where<ItemResult>((IEnumerable<ItemResult>) operationResponse1.Results, (r => r.Success && r.Data != null && r.Data.GetType().GetProperty("HasChanges")?.GetValue(r.Data) is bool flag && !flag)));
      if (Enumerable.Any<ItemResult>((IEnumerable<ItemResult>) list))
        operationResponse1.Warnings.Add($"{list.Count} of {count} perspective tables had no changes detected - they are already in the requested state.");
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
                str = $"Transaction committed. All {count} perspective tables were already in the requested state.";
              else
                str = $"Transaction committed. Updated {num3} of {count} perspective tables.";
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
            operationResponse1.Warnings.Add("Transaction remains open for explicit commit by caller.");
            BatchOperationResponse operationResponse3 = operationResponse1;
            string str;
            if (num4 <= 0)
              str = $"All {count} perspective tables were already in the requested state.";
            else
              str = $"Updated {num4} of {count} perspective tables in existing transaction.";
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
              operationResponse1.Message = "Transaction rolled back due to errors. No perspective tables were updated.";
            }
            catch (Exception ex)
            {
              operationResponse1.Message = "Failed to rollback transaction: " + ex.Message;
            }
          }
          else
          {
            operationResponse1.Warnings.Add("Transaction remains open. Caller must handle rollback due to operation errors.");
            operationResponse1.Message = "Operation encountered errors. No perspective tables were updated.";
          }
          operationResponse1.Success = false;
        }
      }
      else if (num1 > 0)
        operationResponse1.Message = $"Updated {num1} of {count} perspective tables.";
      if (string.IsNullOrEmpty(operationResponse1.Message))
        operationResponse1.Message = num2 > 0 ? "Batch update operation completed with errors." : "Batch update operation completed successfully.";
      operationResponse1.Success = num2 == 0 || request.Options.ContinueOnError && num1 > 0;
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

  public static BatchOperationResponse BatchRemovePerspectiveTables(
    BatchRemovePerspectiveTablesRequest request)
  {
    Stopwatch stopwatch = Stopwatch.StartNew();
    List<string> warnings = new List<string>();
    BatchOperationResponse operationResponse = new BatchOperationResponse()
    {
      Operation = "BatchRemoveTables",
      Results = new List<ItemResult>(),
      Warnings = warnings
    };
    if (request.Items == null || !Enumerable.Any<string>((IEnumerable<string>) request.Items))
    {
      operationResponse.Success = false;
      operationResponse.Message = "No perspective tables provided for batch removal";
      operationResponse.Summary = new BatchSummary()
      {
        TotalItems = 0,
        SuccessCount = 0,
        FailureCount = 0,
        ExecutionTime = stopwatch.Elapsed
      };
      return operationResponse;
    }
    if (string.IsNullOrWhiteSpace(request.PerspectiveName))
    {
      operationResponse.Success = false;
      operationResponse.Message = "Perspective name is required";
      operationResponse.Summary = new BatchSummary()
      {
        TotalItems = request.Items.Count,
        SuccessCount = 0,
        FailureCount = request.Items.Count,
        ExecutionTime = stopwatch.Elapsed
      };
      return operationResponse;
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
        string tableName = request.Items[index];
        ItemResult itemResult1 = new ItemResult()
        {
          Index = index,
          ItemIdentifier = tableName
        };
        try
        {
          PerspectiveOperationResult perspectiveOperationResult = PerspectiveOperations.RemoveTableFromPerspective(request.ConnectionName, request.PerspectiveName, tableName);
          itemResult1.Success = perspectiveOperationResult.Success;
          ItemResult itemResult2 = itemResult1;
          string str;
          if (!itemResult1.Success)
            str = $"Failed to remove table '{tableName}' from perspective '{request.PerspectiveName}': {perspectiveOperationResult.Message}";
          else
            str = $"Successfully removed table '{tableName}' from perspective '{request.PerspectiveName}'";
          itemResult2.Message = str;
          if (itemResult1.Success)
          {
            ++num1;
            if (transactionId != null)
              TransactionOperations.RecordOperation(connectionInfo, $"Removed table '{tableName}' from perspective '{request.PerspectiveName}'");
          }
          else
            ++num2;
        }
        catch (Exception ex)
        {
          itemResult1.Success = false;
          itemResult1.Message = $"Error removing table '{tableName}' from perspective '{request.PerspectiveName}': {ex.Message}";
          ++num2;
        }
        operationResponse.Results.Add(itemResult1);
        if (!itemResult1.Success && !request.Options.ContinueOnError)
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
              operationResponse.Message = $"Transaction committed. Removed {num1} of {count} tables from perspective.";
            }
            catch (Exception ex)
            {
              operationResponse.Message = "Failed to commit transaction: " + ex.Message;
              operationResponse.Success = false;
            }
          }
          else
          {
            operationResponse.Warnings.Add("Transaction remains open for explicit commit by caller.");
            operationResponse.Message = $"Removed {num1} of {count} tables from perspective in existing transaction.";
          }
        }
        else
        {
          if (ownsTransaction)
          {
            try
            {
              TransactionOperations.RollbackTransaction(request.ConnectionName);
              operationResponse.Message = "Transaction rolled back due to errors. No tables were removed from perspective.";
            }
            catch (Exception ex)
            {
              operationResponse.Message = "Failed to rollback transaction: " + ex.Message;
            }
          }
          else
          {
            operationResponse.Warnings.Add("Transaction remains open. Caller must handle rollback due to operation errors.");
            operationResponse.Message = "Operation encountered errors. No tables were removed from perspective.";
          }
          operationResponse.Success = false;
        }
      }
      else if (num1 > 0)
        operationResponse.Message = $"Removed {num1} of {count} tables from perspective.";
      if (string.IsNullOrEmpty(operationResponse.Message))
        operationResponse.Message = num2 > 0 ? "Batch remove operation completed with errors." : "Batch remove operation completed successfully.";
      operationResponse.Success = num2 == 0 || request.Options.ContinueOnError && num1 > 0;
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
      operationResponse.Success = false;
      operationResponse.Message = "Batch remove operation failed: " + ex.Message;
      num2 = count - num1;
    }
    stopwatch.Stop();
    operationResponse.Summary = new BatchSummary()
    {
      TotalItems = count,
      SuccessCount = num1,
      FailureCount = num2,
      ExecutionTime = stopwatch.Elapsed
    };
    return operationResponse;
  }

  public static BatchOperationResponse BatchGetPerspectiveTables(
    BatchGetPerspectiveTablesRequest request)
  {
    Stopwatch stopwatch = Stopwatch.StartNew();
    BatchOperationResponse perspectiveTables = new BatchOperationResponse()
    {
      Operation = "BatchGetTables",
      Results = new List<ItemResult>()
    };
    if (request.Items == null || !Enumerable.Any<string>((IEnumerable<string>) request.Items))
    {
      perspectiveTables.Success = false;
      perspectiveTables.Message = "No perspective tables specified for batch retrieval";
      perspectiveTables.Summary = new BatchSummary()
      {
        TotalItems = 0,
        SuccessCount = 0,
        FailureCount = 0,
        ExecutionTime = stopwatch.Elapsed
      };
      return perspectiveTables;
    }
    if (string.IsNullOrWhiteSpace(request.PerspectiveName))
    {
      perspectiveTables.Success = false;
      perspectiveTables.Message = "Perspective name is required";
      perspectiveTables.Summary = new BatchSummary()
      {
        TotalItems = request.Items.Count,
        SuccessCount = 0,
        FailureCount = request.Items.Count,
        ExecutionTime = stopwatch.Elapsed
      };
      return perspectiveTables;
    }
    int count = request.Items.Count;
    int num1 = 0;
    int num2 = 0;
    try
    {
      for (int index = 0; index < request.Items.Count; ++index)
      {
        string tableName = request.Items[index];
        ItemResult itemResult = new ItemResult()
        {
          Index = index,
          ItemIdentifier = tableName
        };
        try
        {
          PerspectiveTableGet perspectiveTable = PerspectiveOperations.GetPerspectiveTable(request.ConnectionName, request.PerspectiveName, tableName);
          itemResult.Success = true;
          itemResult.Message = $"Successfully retrieved table '{tableName}' from perspective '{request.PerspectiveName}'";
          itemResult.Data = (object) new Dictionary<string, object>()
          {
            ["TableName"] = (object) perspectiveTable.TableName,
            ["Name"] = (object) perspectiveTable.Name,
            ["IncludeAll"] = (object) perspectiveTable.IncludeAll,
            ["ModifiedTime"] = (object) perspectiveTable.ModifiedTime
          };
          ++num1;
        }
        catch (Exception ex)
        {
          itemResult.Success = false;
          itemResult.Message = $"Error retrieving table '{tableName}' from perspective '{request.PerspectiveName}': {ex.Message}";
          ++num2;
        }
        perspectiveTables.Results.Add(itemResult);
      }
      if (num1 > 0)
        perspectiveTables.Message = $"Retrieved {num1} of {count} perspective tables.";
      if (string.IsNullOrEmpty(perspectiveTables.Message))
        perspectiveTables.Message = num2 > 0 ? "Batch get operation completed with errors." : "Batch get operation completed successfully.";
      perspectiveTables.Success = num2 == 0;
    }
    catch (Exception ex)
    {
      perspectiveTables.Success = false;
      perspectiveTables.Message = "Batch get operation failed: " + ex.Message;
      num2 = count - num1;
    }
    stopwatch.Stop();
    perspectiveTables.Summary = new BatchSummary()
    {
      TotalItems = count,
      SuccessCount = num1,
      FailureCount = num2,
      ExecutionTime = stopwatch.Elapsed
    };
    return perspectiveTables;
  }

  public static BatchOperationResponse BatchAddPerspectiveColumns(
    BatchAddPerspectiveColumnsRequest request)
  {
    Stopwatch stopwatch = Stopwatch.StartNew();
    List<string> warnings = new List<string>();
    BatchOperationResponse operationResponse = new BatchOperationResponse()
    {
      Operation = "BatchAddColumns",
      Results = new List<ItemResult>(),
      Warnings = warnings
    };
    if (request.Items == null || !Enumerable.Any<PerspectiveColumnCreate>((IEnumerable<PerspectiveColumnCreate>) request.Items))
    {
      operationResponse.Success = false;
      operationResponse.Message = "No perspective columns provided for batch addition";
      operationResponse.Summary = new BatchSummary()
      {
        TotalItems = 0,
        SuccessCount = 0,
        FailureCount = 0,
        ExecutionTime = stopwatch.Elapsed
      };
      return operationResponse;
    }
    if (string.IsNullOrWhiteSpace(request.PerspectiveName))
    {
      operationResponse.Success = false;
      operationResponse.Message = "Perspective name is required";
      operationResponse.Summary = new BatchSummary()
      {
        TotalItems = request.Items.Count,
        SuccessCount = 0,
        FailureCount = request.Items.Count,
        ExecutionTime = stopwatch.Elapsed
      };
      return operationResponse;
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
        PerspectiveColumnCreate def = request.Items[index];
        ItemResult itemResult1 = new ItemResult()
        {
          Index = index,
          ItemIdentifier = $"{def.TableName}.{def.ColumnName}"
        };
        try
        {
          PerspectiveOperationResult perspectiveTable = PerspectiveOperations.AddColumnToPerspectiveTable(request.ConnectionName, request.PerspectiveName, def);
          itemResult1.Success = perspectiveTable.Success;
          ItemResult itemResult2 = itemResult1;
          string str;
          if (!itemResult1.Success)
            str = $"Failed to add column '{def.TableName}.{def.ColumnName}' to perspective '{request.PerspectiveName}': {perspectiveTable.Message}";
          else
            str = $"Successfully added column '{def.TableName}.{def.ColumnName}' to perspective '{request.PerspectiveName}'";
          itemResult2.Message = str;
          if (itemResult1.Success)
          {
            ++num1;
            if (transactionId != null)
              TransactionOperations.RecordOperation(connectionInfo, $"Added column '{def.TableName}.{def.ColumnName}' to perspective '{request.PerspectiveName}'");
          }
          else
            ++num2;
        }
        catch (Exception ex)
        {
          itemResult1.Success = false;
          itemResult1.Message = $"Error adding column '{def.TableName}.{def.ColumnName}' to perspective '{request.PerspectiveName}': {ex.Message}";
          ++num2;
        }
        operationResponse.Results.Add(itemResult1);
        if (!itemResult1.Success && !request.Options.ContinueOnError)
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
              operationResponse.Message = $"Transaction committed. Added {num1} of {count} columns to perspective.";
            }
            catch (Exception ex)
            {
              operationResponse.Message = "Failed to commit transaction: " + ex.Message;
              operationResponse.Success = false;
            }
          }
          else
          {
            operationResponse.Warnings.Add("Transaction remains open for explicit commit by caller.");
            operationResponse.Message = $"Added {num1} of {count} columns to perspective in existing transaction.";
          }
        }
        else
        {
          if (ownsTransaction)
          {
            try
            {
              TransactionOperations.RollbackTransaction(request.ConnectionName);
              operationResponse.Message = "Transaction rolled back due to errors. No columns were added to perspective.";
            }
            catch (Exception ex)
            {
              operationResponse.Message = "Failed to rollback transaction: " + ex.Message;
            }
          }
          else
          {
            operationResponse.Warnings.Add("Transaction remains open. Caller must handle rollback due to operation errors.");
            operationResponse.Message = "Operation encountered errors. No columns were added to perspective.";
          }
          operationResponse.Success = false;
        }
      }
      else if (num1 > 0)
        operationResponse.Message = $"Added {num1} of {count} columns to perspective.";
      if (string.IsNullOrEmpty(operationResponse.Message))
        operationResponse.Message = num2 > 0 ? "Batch add operation completed with errors." : "Batch add operation completed successfully.";
      operationResponse.Success = num2 == 0 || request.Options.ContinueOnError && num1 > 0;
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
      operationResponse.Success = false;
      operationResponse.Message = "Batch add operation failed: " + ex.Message;
      num2 = count - num1;
    }
    stopwatch.Stop();
    operationResponse.Summary = new BatchSummary()
    {
      TotalItems = count,
      SuccessCount = num1,
      FailureCount = num2,
      ExecutionTime = stopwatch.Elapsed
    };
    return operationResponse;
  }

  public static BatchOperationResponse BatchRemovePerspectiveColumns(
    BatchRemovePerspectiveColumnsRequest request)
  {
    Stopwatch stopwatch = Stopwatch.StartNew();
    List<string> warnings = new List<string>();
    BatchOperationResponse operationResponse = new BatchOperationResponse()
    {
      Operation = "BatchRemoveColumns",
      Results = new List<ItemResult>(),
      Warnings = warnings
    };
    if (request.Items == null || !Enumerable.Any<PerspectiveColumnBase>((IEnumerable<PerspectiveColumnBase>) request.Items))
    {
      operationResponse.Success = false;
      operationResponse.Message = "No perspective columns provided for batch removal";
      operationResponse.Summary = new BatchSummary()
      {
        TotalItems = 0,
        SuccessCount = 0,
        FailureCount = 0,
        ExecutionTime = stopwatch.Elapsed
      };
      return operationResponse;
    }
    if (string.IsNullOrWhiteSpace(request.PerspectiveName))
    {
      operationResponse.Success = false;
      operationResponse.Message = "Perspective name is required";
      operationResponse.Summary = new BatchSummary()
      {
        TotalItems = request.Items.Count,
        SuccessCount = 0,
        FailureCount = request.Items.Count,
        ExecutionTime = stopwatch.Elapsed
      };
      return operationResponse;
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
        PerspectiveColumnBase perspectiveColumnBase = request.Items[index];
        ItemResult itemResult1 = new ItemResult()
        {
          Index = index,
          ItemIdentifier = $"{perspectiveColumnBase.TableName}.{perspectiveColumnBase.ColumnName}"
        };
        try
        {
          PerspectiveOperationResult perspectiveOperationResult = PerspectiveOperations.RemoveColumnFromPerspectiveTable(request.ConnectionName, request.PerspectiveName, perspectiveColumnBase.TableName, perspectiveColumnBase.ColumnName);
          itemResult1.Success = perspectiveOperationResult.Success;
          ItemResult itemResult2 = itemResult1;
          string str;
          if (!itemResult1.Success)
            str = $"Failed to remove column '{perspectiveColumnBase.TableName}.{perspectiveColumnBase.ColumnName}' from perspective '{request.PerspectiveName}': {perspectiveOperationResult.Message}";
          else
            str = $"Successfully removed column '{perspectiveColumnBase.TableName}.{perspectiveColumnBase.ColumnName}' from perspective '{request.PerspectiveName}'";
          itemResult2.Message = str;
          if (itemResult1.Success)
          {
            ++num1;
            if (transactionId != null)
              TransactionOperations.RecordOperation(connectionInfo, $"Removed column '{perspectiveColumnBase.TableName}.{perspectiveColumnBase.ColumnName}' from perspective '{request.PerspectiveName}'");
          }
          else
            ++num2;
        }
        catch (Exception ex)
        {
          itemResult1.Success = false;
          itemResult1.Message = $"Error removing column '{perspectiveColumnBase.TableName}.{perspectiveColumnBase.ColumnName}' from perspective '{request.PerspectiveName}': {ex.Message}";
          ++num2;
        }
        operationResponse.Results.Add(itemResult1);
        if (!itemResult1.Success && !request.Options.ContinueOnError)
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
              operationResponse.Message = $"Transaction committed. Removed {num1} of {count} columns from perspective.";
            }
            catch (Exception ex)
            {
              operationResponse.Message = "Failed to commit transaction: " + ex.Message;
              operationResponse.Success = false;
            }
          }
          else
          {
            operationResponse.Warnings.Add("Transaction remains open for explicit commit by caller.");
            operationResponse.Message = $"Removed {num1} of {count} columns from perspective in existing transaction.";
          }
        }
        else
        {
          if (ownsTransaction)
          {
            try
            {
              TransactionOperations.RollbackTransaction(request.ConnectionName);
              operationResponse.Message = "Transaction rolled back due to errors. No columns were removed from perspective.";
            }
            catch (Exception ex)
            {
              operationResponse.Message = "Failed to rollback transaction: " + ex.Message;
            }
          }
          else
          {
            operationResponse.Warnings.Add("Transaction remains open. Caller must handle rollback due to operation errors.");
            operationResponse.Message = "Operation encountered errors. No columns were removed from perspective.";
          }
          operationResponse.Success = false;
        }
      }
      else if (num1 > 0)
        operationResponse.Message = $"Removed {num1} of {count} columns from perspective.";
      if (string.IsNullOrEmpty(operationResponse.Message))
        operationResponse.Message = num2 > 0 ? "Batch remove operation completed with errors." : "Batch remove operation completed successfully.";
      operationResponse.Success = num2 == 0 || request.Options.ContinueOnError && num1 > 0;
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
      operationResponse.Success = false;
      operationResponse.Message = "Batch remove operation failed: " + ex.Message;
      num2 = count - num1;
    }
    stopwatch.Stop();
    operationResponse.Summary = new BatchSummary()
    {
      TotalItems = count,
      SuccessCount = num1,
      FailureCount = num2,
      ExecutionTime = stopwatch.Elapsed
    };
    return operationResponse;
  }

  public static BatchOperationResponse BatchGetPerspectiveColumns(
    BatchGetPerspectiveColumnsRequest request)
  {
    Stopwatch stopwatch = Stopwatch.StartNew();
    BatchOperationResponse perspectiveColumns = new BatchOperationResponse()
    {
      Operation = "BatchGetColumns",
      Results = new List<ItemResult>()
    };
    if (request.Items == null || !Enumerable.Any<PerspectiveColumnBase>((IEnumerable<PerspectiveColumnBase>) request.Items))
    {
      perspectiveColumns.Success = false;
      perspectiveColumns.Message = "No perspective columns specified for batch retrieval";
      perspectiveColumns.Summary = new BatchSummary()
      {
        TotalItems = 0,
        SuccessCount = 0,
        FailureCount = 0,
        ExecutionTime = stopwatch.Elapsed
      };
      return perspectiveColumns;
    }
    if (string.IsNullOrWhiteSpace(request.PerspectiveName))
    {
      perspectiveColumns.Success = false;
      perspectiveColumns.Message = "Perspective name is required";
      perspectiveColumns.Summary = new BatchSummary()
      {
        TotalItems = request.Items.Count,
        SuccessCount = 0,
        FailureCount = request.Items.Count,
        ExecutionTime = stopwatch.Elapsed
      };
      return perspectiveColumns;
    }
    int count = request.Items.Count;
    int num1 = 0;
    int num2 = 0;
    try
    {
      for (int index = 0; index < request.Items.Count; ++index)
      {
        PerspectiveColumnBase perspectiveColumnBase = request.Items[index];
        ItemResult itemResult = new ItemResult()
        {
          Index = index,
          ItemIdentifier = $"{perspectiveColumnBase.TableName}.{perspectiveColumnBase.ColumnName}"
        };
        try
        {
          PerspectiveColumnGet perspectiveColumn = PerspectiveOperations.GetPerspectiveColumn(request.ConnectionName, request.PerspectiveName, perspectiveColumnBase.TableName, perspectiveColumnBase.ColumnName);
          itemResult.Success = true;
          itemResult.Message = $"Successfully retrieved column '{perspectiveColumnBase.TableName}.{perspectiveColumnBase.ColumnName}' from perspective '{request.PerspectiveName}'";
          itemResult.Data = (object) perspectiveColumn;
          ++num1;
        }
        catch (Exception ex)
        {
          itemResult.Success = false;
          itemResult.Message = $"Error retrieving column '{perspectiveColumnBase.TableName}.{perspectiveColumnBase.ColumnName}' from perspective '{request.PerspectiveName}': {ex.Message}";
          ++num2;
        }
        perspectiveColumns.Results.Add(itemResult);
      }
      if (string.IsNullOrEmpty(perspectiveColumns.Message))
        perspectiveColumns.Message = num2 > 0 ? "Batch get operation completed with errors." : "Batch get operation completed successfully.";
      perspectiveColumns.Success = num2 == 0;
    }
    catch (Exception ex)
    {
      perspectiveColumns.Success = false;
      perspectiveColumns.Message = "Batch get operation failed: " + ex.Message;
      num2 = count - num1;
    }
    stopwatch.Stop();
    perspectiveColumns.Summary = new BatchSummary()
    {
      TotalItems = count,
      SuccessCount = num1,
      FailureCount = num2,
      ExecutionTime = stopwatch.Elapsed
    };
    return perspectiveColumns;
  }

  public static BatchOperationResponse BatchAddPerspectiveMeasures(
    BatchAddPerspectiveMeasuresRequest request)
  {
    Stopwatch stopwatch = Stopwatch.StartNew();
    List<string> warnings = new List<string>();
    BatchOperationResponse operationResponse = new BatchOperationResponse()
    {
      Operation = "BatchAddMeasures",
      Results = new List<ItemResult>(),
      Warnings = warnings
    };
    if (request.Items == null || !Enumerable.Any<PerspectiveMeasureCreate>((IEnumerable<PerspectiveMeasureCreate>) request.Items))
    {
      operationResponse.Success = false;
      operationResponse.Message = "No perspective measures provided for batch addition";
      operationResponse.Summary = new BatchSummary()
      {
        TotalItems = 0,
        SuccessCount = 0,
        FailureCount = 0,
        ExecutionTime = stopwatch.Elapsed
      };
      return operationResponse;
    }
    if (string.IsNullOrWhiteSpace(request.PerspectiveName))
    {
      operationResponse.Success = false;
      operationResponse.Message = "Perspective name is required";
      operationResponse.Summary = new BatchSummary()
      {
        TotalItems = request.Items.Count,
        SuccessCount = 0,
        FailureCount = request.Items.Count,
        ExecutionTime = stopwatch.Elapsed
      };
      return operationResponse;
    }
    int count = request.Items.Count;
    int num1 = 0;
    int num2 = 0;
    TransactionSetupResult transactionSetupResult = BatchTransactionHelper.HandleTransactionSetup(ConnectionOperations.Get(request.ConnectionName), request.Options.UseTransaction, request.ConnectionName, warnings);
    string transactionId = transactionSetupResult.TransactionId;
    bool ownsTransaction = transactionSetupResult.OwnsTransaction;
    try
    {
      for (int index = 0; index < request.Items.Count; ++index)
      {
        PerspectiveMeasureCreate def = request.Items[index];
        ItemResult itemResult1 = new ItemResult()
        {
          Index = index,
          ItemIdentifier = $"{def.TableName}.{def.MeasureName}"
        };
        try
        {
          PerspectiveOperationResult perspectiveTable = PerspectiveOperations.AddMeasureToPerspectiveTable(request.ConnectionName, request.PerspectiveName, def);
          itemResult1.Success = perspectiveTable.Success;
          ItemResult itemResult2 = itemResult1;
          string str = perspectiveTable.Message;
          if (str == null)
            str = $"Successfully added measure '{def.TableName}.{def.MeasureName}' to perspective '{request.PerspectiveName}'";
          itemResult2.Message = str;
          if (itemResult1.Success)
          {
            if (transactionId != null)
              itemResult1.Message += " (in transaction)";
            ++num1;
          }
          else
            ++num2;
        }
        catch (Exception ex)
        {
          itemResult1.Success = false;
          itemResult1.Message = $"Error adding measure '{def.TableName}.{def.MeasureName}' to perspective '{request.PerspectiveName}': {ex.Message}";
          ++num2;
        }
        operationResponse.Results.Add(itemResult1);
        if (!itemResult1.Success && !request.Options.ContinueOnError)
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
              operationResponse.Message = $"Transaction committed. Added {num1} of {count} measures to perspective.";
            }
            catch (Exception ex)
            {
              operationResponse.Message = "Failed to commit transaction: " + ex.Message;
              operationResponse.Success = false;
            }
          }
          else
          {
            operationResponse.Warnings.Add("Transaction remains open for explicit commit by caller.");
            operationResponse.Message = $"Added {num1} of {count} measures to perspective in existing transaction.";
          }
        }
        else
        {
          if (ownsTransaction)
          {
            try
            {
              TransactionOperations.RollbackTransaction(request.ConnectionName);
              operationResponse.Message = "Transaction rolled back due to errors. No measures were added to perspective.";
            }
            catch (Exception ex)
            {
              operationResponse.Message = "Failed to rollback transaction: " + ex.Message;
            }
          }
          else
          {
            operationResponse.Warnings.Add("Transaction remains open. Caller must handle rollback due to operation errors.");
            operationResponse.Message = "Operation encountered errors. No measures were added to perspective.";
          }
          operationResponse.Success = false;
        }
      }
      else if (num1 > 0)
        operationResponse.Message = $"Added {num1} of {count} measures to perspective.";
      if (string.IsNullOrEmpty(operationResponse.Message))
        operationResponse.Message = num2 > 0 ? "Batch add operation completed with errors." : "Batch add operation completed successfully.";
      operationResponse.Success = num2 == 0 || request.Options.ContinueOnError && num1 > 0;
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
      operationResponse.Success = false;
      operationResponse.Message = "Batch add operation failed: " + ex.Message;
      num2 = count - num1;
    }
    stopwatch.Stop();
    operationResponse.Summary = new BatchSummary()
    {
      TotalItems = count,
      SuccessCount = num1,
      FailureCount = num2,
      ExecutionTime = stopwatch.Elapsed
    };
    return operationResponse;
  }

  public static BatchOperationResponse BatchRemovePerspectiveMeasures(
    BatchRemovePerspectiveMeasuresRequest request)
  {
    Stopwatch stopwatch = Stopwatch.StartNew();
    List<string> warnings = new List<string>();
    BatchOperationResponse operationResponse = new BatchOperationResponse()
    {
      Operation = "BatchRemoveMeasures",
      Results = new List<ItemResult>(),
      Warnings = warnings
    };
    if (request.Items == null || !Enumerable.Any<PerspectiveMeasureBase>((IEnumerable<PerspectiveMeasureBase>) request.Items))
    {
      operationResponse.Success = false;
      operationResponse.Message = "No perspective measures provided for batch removal";
      operationResponse.Summary = new BatchSummary()
      {
        TotalItems = 0,
        SuccessCount = 0,
        FailureCount = 0,
        ExecutionTime = stopwatch.Elapsed
      };
      return operationResponse;
    }
    if (string.IsNullOrWhiteSpace(request.PerspectiveName))
    {
      operationResponse.Success = false;
      operationResponse.Message = "Perspective name is required";
      operationResponse.Summary = new BatchSummary()
      {
        TotalItems = request.Items.Count,
        SuccessCount = 0,
        FailureCount = request.Items.Count,
        ExecutionTime = stopwatch.Elapsed
      };
      return operationResponse;
    }
    int count = request.Items.Count;
    int num1 = 0;
    int num2 = 0;
    TransactionSetupResult transactionSetupResult = BatchTransactionHelper.HandleTransactionSetup(ConnectionOperations.Get(request.ConnectionName), request.Options.UseTransaction, request.ConnectionName, warnings);
    string transactionId = transactionSetupResult.TransactionId;
    bool ownsTransaction = transactionSetupResult.OwnsTransaction;
    try
    {
      for (int index = 0; index < request.Items.Count; ++index)
      {
        PerspectiveMeasureBase perspectiveMeasureBase = request.Items[index];
        ItemResult itemResult1 = new ItemResult()
        {
          Index = index,
          ItemIdentifier = $"{perspectiveMeasureBase.TableName}.{perspectiveMeasureBase.MeasureName}"
        };
        try
        {
          PerspectiveOperationResult perspectiveOperationResult = PerspectiveOperations.RemoveMeasureFromPerspectiveTable(request.ConnectionName, request.PerspectiveName, perspectiveMeasureBase.TableName, perspectiveMeasureBase.MeasureName);
          itemResult1.Success = perspectiveOperationResult.Success;
          ItemResult itemResult2 = itemResult1;
          string str = perspectiveOperationResult.Message;
          if (str == null)
            str = $"Successfully removed measure '{perspectiveMeasureBase.TableName}.{perspectiveMeasureBase.MeasureName}' from perspective '{request.PerspectiveName}'";
          itemResult2.Message = str;
          if (itemResult1.Success)
          {
            if (transactionId != null)
              itemResult1.Message += " (in transaction)";
            ++num1;
          }
          else
            ++num2;
        }
        catch (Exception ex)
        {
          itemResult1.Success = false;
          itemResult1.Message = $"Error removing measure '{perspectiveMeasureBase.TableName}.{perspectiveMeasureBase.MeasureName}' from perspective '{request.PerspectiveName}': {ex.Message}";
          ++num2;
        }
        operationResponse.Results.Add(itemResult1);
        if (!itemResult1.Success && !request.Options.ContinueOnError)
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
              operationResponse.Message = $"Transaction committed. Removed {num1} of {count} measures from perspective.";
            }
            catch (Exception ex)
            {
              operationResponse.Message = "Failed to commit transaction: " + ex.Message;
              operationResponse.Success = false;
            }
          }
          else
          {
            operationResponse.Warnings.Add("Transaction remains open for explicit commit by caller.");
            operationResponse.Message = $"Removed {num1} of {count} measures from perspective in existing transaction.";
          }
        }
        else
        {
          if (ownsTransaction)
          {
            try
            {
              TransactionOperations.RollbackTransaction(request.ConnectionName);
              operationResponse.Message = "Transaction rolled back due to errors. No measures were removed from perspective.";
            }
            catch (Exception ex)
            {
              operationResponse.Message = "Failed to rollback transaction: " + ex.Message;
            }
          }
          else
          {
            operationResponse.Warnings.Add("Transaction remains open. Caller must handle rollback due to operation errors.");
            operationResponse.Message = "Operation encountered errors. No measures were removed from perspective.";
          }
          operationResponse.Success = false;
        }
      }
      else if (num1 > 0)
        operationResponse.Message = $"Removed {num1} of {count} measures from perspective.";
      if (string.IsNullOrEmpty(operationResponse.Message))
        operationResponse.Message = num2 > 0 ? "Batch remove operation completed with errors." : "Batch remove operation completed successfully.";
      operationResponse.Success = num2 == 0 || request.Options.ContinueOnError && num1 > 0;
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
      operationResponse.Success = false;
      operationResponse.Message = "Batch remove operation failed: " + ex.Message;
      num2 = count - num1;
    }
    stopwatch.Stop();
    operationResponse.Summary = new BatchSummary()
    {
      TotalItems = count,
      SuccessCount = num1,
      FailureCount = num2,
      ExecutionTime = stopwatch.Elapsed
    };
    return operationResponse;
  }

  public static BatchOperationResponse BatchGetPerspectiveMeasures(
    BatchGetPerspectiveMeasuresRequest request)
  {
    Stopwatch stopwatch = Stopwatch.StartNew();
    BatchOperationResponse perspectiveMeasures = new BatchOperationResponse()
    {
      Operation = "BatchGetMeasures",
      Results = new List<ItemResult>()
    };
    if (request.Items == null || !Enumerable.Any<PerspectiveMeasureBase>((IEnumerable<PerspectiveMeasureBase>) request.Items))
    {
      perspectiveMeasures.Success = false;
      perspectiveMeasures.Message = "No perspective measures specified for batch retrieval";
      perspectiveMeasures.Summary = new BatchSummary()
      {
        TotalItems = 0,
        SuccessCount = 0,
        FailureCount = 0,
        ExecutionTime = stopwatch.Elapsed
      };
      return perspectiveMeasures;
    }
    if (string.IsNullOrWhiteSpace(request.PerspectiveName))
    {
      perspectiveMeasures.Success = false;
      perspectiveMeasures.Message = "Perspective name is required";
      perspectiveMeasures.Summary = new BatchSummary()
      {
        TotalItems = request.Items.Count,
        SuccessCount = 0,
        FailureCount = request.Items.Count,
        ExecutionTime = stopwatch.Elapsed
      };
      return perspectiveMeasures;
    }
    int count = request.Items.Count;
    int num1 = 0;
    int num2 = 0;
    try
    {
      for (int index = 0; index < request.Items.Count; ++index)
      {
        PerspectiveMeasureBase perspectiveMeasureBase = request.Items[index];
        ItemResult itemResult = new ItemResult()
        {
          Index = index,
          ItemIdentifier = $"{perspectiveMeasureBase.TableName}.{perspectiveMeasureBase.MeasureName}"
        };
        try
        {
          PerspectiveMeasureGet perspectiveMeasure = PerspectiveOperations.GetPerspectiveMeasure(request.ConnectionName, request.PerspectiveName, perspectiveMeasureBase.TableName, perspectiveMeasureBase.MeasureName);
          itemResult.Success = true;
          itemResult.Message = $"Successfully retrieved measure '{perspectiveMeasureBase.TableName}.{perspectiveMeasureBase.MeasureName}' from perspective '{request.PerspectiveName}'";
          itemResult.Data = (object) perspectiveMeasure;
          ++num1;
        }
        catch (Exception ex)
        {
          itemResult.Success = false;
          itemResult.Message = $"Error retrieving measure '{perspectiveMeasureBase.TableName}.{perspectiveMeasureBase.MeasureName}' from perspective '{request.PerspectiveName}': {ex.Message}";
          ++num2;
        }
        perspectiveMeasures.Results.Add(itemResult);
      }
      if (num1 > 0)
        perspectiveMeasures.Message = $"Retrieved {num1} of {count} perspective measures.";
      if (string.IsNullOrEmpty(perspectiveMeasures.Message))
        perspectiveMeasures.Message = num2 > 0 ? "Batch get operation completed with errors." : "Batch get operation completed successfully.";
      perspectiveMeasures.Success = num2 == 0;
    }
    catch (Exception ex)
    {
      perspectiveMeasures.Success = false;
      perspectiveMeasures.Message = "Batch get operation failed: " + ex.Message;
      num2 = count - num1;
    }
    stopwatch.Stop();
    perspectiveMeasures.Summary = new BatchSummary()
    {
      TotalItems = count,
      SuccessCount = num1,
      FailureCount = num2,
      ExecutionTime = stopwatch.Elapsed
    };
    return perspectiveMeasures;
  }

  public static BatchOperationResponse BatchAddPerspectiveHierarchies(
    BatchAddPerspectiveHierarchiesRequest request)
  {
    Stopwatch stopwatch = Stopwatch.StartNew();
    List<string> warnings = new List<string>();
    BatchOperationResponse operationResponse = new BatchOperationResponse()
    {
      Operation = "BatchAddHierarchies",
      Results = new List<ItemResult>(),
      Warnings = warnings
    };
    if (request.Items == null || !Enumerable.Any<PerspectiveHierarchyCreate>((IEnumerable<PerspectiveHierarchyCreate>) request.Items))
    {
      operationResponse.Success = false;
      operationResponse.Message = "No perspective hierarchies provided for batch addition";
      operationResponse.Summary = new BatchSummary()
      {
        TotalItems = 0,
        SuccessCount = 0,
        FailureCount = 0,
        ExecutionTime = stopwatch.Elapsed
      };
      return operationResponse;
    }
    if (string.IsNullOrWhiteSpace(request.PerspectiveName))
    {
      operationResponse.Success = false;
      operationResponse.Message = "Perspective name is required";
      operationResponse.Summary = new BatchSummary()
      {
        TotalItems = request.Items.Count,
        SuccessCount = 0,
        FailureCount = request.Items.Count,
        ExecutionTime = stopwatch.Elapsed
      };
      return operationResponse;
    }
    int count = request.Items.Count;
    int num1 = 0;
    int num2 = 0;
    TransactionSetupResult transactionSetupResult = BatchTransactionHelper.HandleTransactionSetup(ConnectionOperations.Get(request.ConnectionName), request.Options.UseTransaction, request.ConnectionName, warnings);
    string transactionId = transactionSetupResult.TransactionId;
    bool ownsTransaction = transactionSetupResult.OwnsTransaction;
    try
    {
      for (int index = 0; index < request.Items.Count; ++index)
      {
        PerspectiveHierarchyCreate def = request.Items[index];
        ItemResult itemResult = new ItemResult()
        {
          Index = index,
          ItemIdentifier = $"{def.TableName}.{def.HierarchyName}"
        };
        try
        {
          PerspectiveOperationResult perspectiveTable = PerspectiveOperations.AddHierarchyToPerspectiveTable(request.ConnectionName, request.PerspectiveName, def);
          itemResult.Success = true;
          itemResult.Message = $"Successfully added hierarchy '{def.TableName}.{def.HierarchyName}' to perspective '{request.PerspectiveName}'";
          itemResult.Data = (object) perspectiveTable;
          ++num1;
        }
        catch (Exception ex)
        {
          itemResult.Success = false;
          itemResult.Message = $"Error adding hierarchy '{def.TableName}.{def.HierarchyName}' to perspective '{request.PerspectiveName}': {ex.Message}";
          ++num2;
          if (!request.Options.ContinueOnError)
          {
            operationResponse.Results.Add(itemResult);
            break;
          }
        }
        operationResponse.Results.Add(itemResult);
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
              operationResponse.Message = $"Transaction committed. Added {num1} of {count} hierarchies to perspective.";
            }
            catch (Exception ex)
            {
              operationResponse.Success = false;
              operationResponse.Message = "Failed to commit transaction: " + ex.Message;
            }
          }
          else
          {
            operationResponse.Warnings.Add("Transaction remains open for explicit commit by caller.");
            operationResponse.Message = $"Added {num1} of {count} hierarchies to perspective in existing transaction.";
          }
        }
        else
        {
          if (ownsTransaction)
          {
            try
            {
              TransactionOperations.RollbackTransaction(request.ConnectionName);
              operationResponse.Message = "Transaction rolled back due to errors. No hierarchies were added to perspective.";
            }
            catch (Exception ex)
            {
              operationResponse.Message = "Failed to rollback transaction: " + ex.Message;
            }
          }
          else
          {
            operationResponse.Warnings.Add("Transaction remains open. Caller must handle rollback due to operation errors.");
            operationResponse.Message = "Operation encountered errors. No hierarchies were added to perspective.";
          }
          operationResponse.Success = false;
        }
      }
      else if (num1 > 0)
        operationResponse.Message = $"Added {num1} of {count} hierarchies to perspective.";
      if (string.IsNullOrEmpty(operationResponse.Message))
        operationResponse.Message = num2 > 0 ? "Batch add operation completed with errors." : "Batch add operation completed successfully.";
      operationResponse.Success = num2 == 0 || request.Options.ContinueOnError && num1 > 0;
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
      operationResponse.Success = false;
      operationResponse.Message = "Batch add operation failed: " + ex.Message;
      num2 = count - num1;
    }
    stopwatch.Stop();
    operationResponse.Summary = new BatchSummary()
    {
      TotalItems = count,
      SuccessCount = num1,
      FailureCount = num2,
      ExecutionTime = stopwatch.Elapsed
    };
    return operationResponse;
  }

  public static BatchOperationResponse BatchRemovePerspectiveHierarchies(
    BatchRemovePerspectiveHierarchiesRequest request)
  {
    Stopwatch stopwatch = Stopwatch.StartNew();
    List<string> warnings = new List<string>();
    BatchOperationResponse operationResponse = new BatchOperationResponse()
    {
      Operation = "BatchRemoveHierarchies",
      Results = new List<ItemResult>(),
      Warnings = warnings
    };
    if (request.Items == null || !Enumerable.Any<PerspectiveHierarchyBase>((IEnumerable<PerspectiveHierarchyBase>) request.Items))
    {
      operationResponse.Success = false;
      operationResponse.Message = "No perspective hierarchies provided for batch removal";
      operationResponse.Summary = new BatchSummary()
      {
        TotalItems = 0,
        SuccessCount = 0,
        FailureCount = 0,
        ExecutionTime = stopwatch.Elapsed
      };
      return operationResponse;
    }
    if (string.IsNullOrWhiteSpace(request.PerspectiveName))
    {
      operationResponse.Success = false;
      operationResponse.Message = "Perspective name is required";
      operationResponse.Summary = new BatchSummary()
      {
        TotalItems = request.Items.Count,
        SuccessCount = 0,
        FailureCount = request.Items.Count,
        ExecutionTime = stopwatch.Elapsed
      };
      return operationResponse;
    }
    int count = request.Items.Count;
    int num1 = 0;
    int num2 = 0;
    TransactionSetupResult transactionSetupResult = BatchTransactionHelper.HandleTransactionSetup(ConnectionOperations.Get(request.ConnectionName), request.Options.UseTransaction, request.ConnectionName, warnings);
    string transactionId = transactionSetupResult.TransactionId;
    bool ownsTransaction = transactionSetupResult.OwnsTransaction;
    try
    {
      for (int index = 0; index < request.Items.Count; ++index)
      {
        PerspectiveHierarchyBase perspectiveHierarchyBase = request.Items[index];
        ItemResult itemResult = new ItemResult()
        {
          Index = index,
          ItemIdentifier = $"{perspectiveHierarchyBase.TableName}.{perspectiveHierarchyBase.HierarchyName}"
        };
        try
        {
          PerspectiveOperationResult perspectiveOperationResult = PerspectiveOperations.RemoveHierarchyFromPerspectiveTable(request.ConnectionName, request.PerspectiveName, perspectiveHierarchyBase.TableName, perspectiveHierarchyBase.HierarchyName);
          itemResult.Success = true;
          itemResult.Message = $"Successfully removed hierarchy '{perspectiveHierarchyBase.TableName}.{perspectiveHierarchyBase.HierarchyName}' from perspective '{request.PerspectiveName}'";
          itemResult.Data = (object) perspectiveOperationResult;
          ++num1;
        }
        catch (Exception ex)
        {
          itemResult.Success = false;
          itemResult.Message = $"Error removing hierarchy '{perspectiveHierarchyBase.TableName}.{perspectiveHierarchyBase.HierarchyName}' from perspective '{request.PerspectiveName}': {ex.Message}";
          ++num2;
          if (!request.Options.ContinueOnError)
          {
            operationResponse.Results.Add(itemResult);
            break;
          }
        }
        operationResponse.Results.Add(itemResult);
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
              operationResponse.Message = $"Transaction committed. Removed {num1} of {count} hierarchies from perspective.";
            }
            catch (Exception ex)
            {
              operationResponse.Success = false;
              operationResponse.Message = "Failed to commit transaction: " + ex.Message;
            }
          }
          else
          {
            operationResponse.Warnings.Add("Transaction remains open for explicit commit by caller.");
            operationResponse.Message = $"Removed {num1} of {count} hierarchies from perspective in existing transaction.";
          }
        }
        else
        {
          if (ownsTransaction)
          {
            try
            {
              TransactionOperations.RollbackTransaction(request.ConnectionName);
              operationResponse.Message = "Transaction rolled back due to errors. No hierarchies were removed from perspective.";
            }
            catch (Exception ex)
            {
              operationResponse.Message = "Failed to rollback transaction: " + ex.Message;
            }
          }
          else
          {
            operationResponse.Warnings.Add("Transaction remains open. Caller must handle rollback due to operation errors.");
            operationResponse.Message = "Operation encountered errors. No hierarchies were removed from perspective.";
          }
          operationResponse.Success = false;
        }
      }
      else if (num1 > 0)
        operationResponse.Message = $"Removed {num1} of {count} hierarchies from perspective.";
      if (string.IsNullOrEmpty(operationResponse.Message))
        operationResponse.Message = num2 > 0 ? "Batch remove operation completed with errors." : "Batch remove operation completed successfully.";
      operationResponse.Success = num2 == 0 || request.Options.ContinueOnError && num1 > 0;
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
      operationResponse.Success = false;
      operationResponse.Message = "Batch remove operation failed: " + ex.Message;
      num2 = count - num1;
    }
    stopwatch.Stop();
    operationResponse.Summary = new BatchSummary()
    {
      TotalItems = count,
      SuccessCount = num1,
      FailureCount = num2,
      ExecutionTime = stopwatch.Elapsed
    };
    return operationResponse;
  }

  public static BatchOperationResponse BatchGetPerspectiveHierarchies(
    BatchGetPerspectiveHierarchiesRequest request)
  {
    Stopwatch stopwatch = Stopwatch.StartNew();
    BatchOperationResponse perspectiveHierarchies = new BatchOperationResponse()
    {
      Operation = "BatchGetHierarchies",
      Results = new List<ItemResult>()
    };
    if (request.Items == null || !Enumerable.Any<PerspectiveHierarchyBase>((IEnumerable<PerspectiveHierarchyBase>) request.Items))
    {
      perspectiveHierarchies.Success = false;
      perspectiveHierarchies.Message = "No perspective hierarchies specified for batch retrieval";
      perspectiveHierarchies.Summary = new BatchSummary()
      {
        TotalItems = 0,
        SuccessCount = 0,
        FailureCount = 0,
        ExecutionTime = stopwatch.Elapsed
      };
      return perspectiveHierarchies;
    }
    if (string.IsNullOrWhiteSpace(request.PerspectiveName))
    {
      perspectiveHierarchies.Success = false;
      perspectiveHierarchies.Message = "Perspective name is required";
      perspectiveHierarchies.Summary = new BatchSummary()
      {
        TotalItems = request.Items.Count,
        SuccessCount = 0,
        FailureCount = request.Items.Count,
        ExecutionTime = stopwatch.Elapsed
      };
      return perspectiveHierarchies;
    }
    int count = request.Items.Count;
    int num1 = 0;
    int num2 = 0;
    try
    {
      for (int index = 0; index < request.Items.Count; ++index)
      {
        PerspectiveHierarchyBase perspectiveHierarchyBase = request.Items[index];
        ItemResult itemResult = new ItemResult()
        {
          Index = index,
          ItemIdentifier = $"{perspectiveHierarchyBase.TableName}.{perspectiveHierarchyBase.HierarchyName}"
        };
        try
        {
          PerspectiveHierarchyGet perspectiveHierarchy = PerspectiveOperations.GetPerspectiveHierarchy(request.ConnectionName, request.PerspectiveName, perspectiveHierarchyBase.TableName, perspectiveHierarchyBase.HierarchyName);
          itemResult.Success = true;
          itemResult.Message = $"Successfully retrieved hierarchy '{perspectiveHierarchyBase.TableName}.{perspectiveHierarchyBase.HierarchyName}' from perspective '{request.PerspectiveName}'";
          itemResult.Data = (object) perspectiveHierarchy;
          ++num1;
        }
        catch (Exception ex)
        {
          itemResult.Success = false;
          itemResult.Message = $"Error retrieving hierarchy '{perspectiveHierarchyBase.TableName}.{perspectiveHierarchyBase.HierarchyName}' from perspective '{request.PerspectiveName}': {ex.Message}";
          ++num2;
        }
        perspectiveHierarchies.Results.Add(itemResult);
      }
      if (num1 > 0)
        perspectiveHierarchies.Message = $"Retrieved {num1} of {count} perspective hierarchies.";
      if (string.IsNullOrEmpty(perspectiveHierarchies.Message))
        perspectiveHierarchies.Message = num2 > 0 ? "Batch get operation completed with errors." : "Batch get operation completed successfully.";
      perspectiveHierarchies.Success = num2 == 0;
    }
    catch (Exception ex)
    {
      perspectiveHierarchies.Success = false;
      perspectiveHierarchies.Message = "Batch get operation failed: " + ex.Message;
      num2 = count - num1;
    }
    stopwatch.Stop();
    perspectiveHierarchies.Summary = new BatchSummary()
    {
      TotalItems = count,
      SuccessCount = num1,
      FailureCount = num2,
      ExecutionTime = stopwatch.Elapsed
    };
    return perspectiveHierarchies;
  }
}
