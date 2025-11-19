// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Core.BatchTableOperations
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

public static class BatchTableOperations
{
  public static BatchOperationResponse BatchCreateTables(BatchCreateTablesRequest request)
  {
    Stopwatch stopwatch = Stopwatch.StartNew();
    List<string> warnings = new List<string>();
    BatchOperationResponse tables = new BatchOperationResponse()
    {
      Operation = "BatchCreate",
      Results = new List<ItemResult>(),
      Warnings = warnings
    };
    if (request.Items == null || !Enumerable.Any<TableCreate>((IEnumerable<TableCreate>) request.Items))
    {
      tables.Success = false;
      tables.Message = "No tables provided for batch creation";
      tables.Summary = new BatchSummary()
      {
        TotalItems = 0,
        SuccessCount = 0,
        FailureCount = 0,
        ExecutionTime = stopwatch.Elapsed
      };
      return tables;
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
        TableCreate def = request.Items[index];
        ItemResult itemResult = new ItemResult()
        {
          Index = index,
          ItemIdentifier = def.Name ?? "Unknown"
        };
        try
        {
          TableOperations.TableOperationResult table = TableOperations.CreateTable(request.ConnectionName, def);
          itemResult.Success = true;
          itemResult.Message = $"Successfully created table '{def.Name}'";
          itemResult.Data = (object) table;
          ++num1;
          if (transactionId != null)
            TransactionOperations.RecordOperation(connectionInfo, $"Created table '{def.Name}'");
        }
        catch (Exception ex)
        {
          itemResult.Success = false;
          itemResult.Message = $"Error creating table '{def.Name}': {ex.Message}";
          ++num2;
        }
        tables.Results.Add(itemResult);
        if (!itemResult.Success && !request.Options.ContinueOnError)
          break;
      }
      if (transactionId != null)
      {
        if (num2 == 0 || request.Options.ContinueOnError && num1 > 0)
        {
          if (ownsTransaction)
          {
            try
            {
              TransactionOperations.CommitTransaction(request.ConnectionName);
              tables.Message = $"Transaction committed. Created {num1} of {count} tables.";
            }
            catch (Exception ex)
            {
              tables.Message = "Failed to commit transaction: " + ex.Message;
              tables.Success = false;
            }
          }
          else
            tables.Message = $"Created {num1} of {count} tables in existing transaction. Transaction remains open for explicit commit.";
        }
        else
        {
          if (ownsTransaction)
          {
            try
            {
              TransactionOperations.RollbackTransaction(request.ConnectionName);
              tables.Message = "Transaction rolled back due to errors. No tables were created.";
            }
            catch (Exception ex)
            {
              tables.Message = "Failed to rollback transaction: " + ex.Message;
            }
          }
          else
            tables.Message = $"Batch operation failed in existing transaction. {num2} of {count} tables failed. Transaction remains open - you should rollback.";
          tables.Success = false;
        }
      }
      else if (num1 > 0)
        tables.Message = $"Created {num1} of {count} tables.";
      if (string.IsNullOrEmpty(tables.Message))
        tables.Message = num2 > 0 ? "Batch create operation completed with errors." : "Batch create operation completed successfully.";
      tables.Success = num2 == 0 || request.Options.ContinueOnError && num1 > 0;
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
      tables.Success = false;
      tables.Message = "Batch create operation failed: " + ex.Message;
      num2 = count - num1;
    }
    stopwatch.Stop();
    tables.Summary = new BatchSummary()
    {
      TotalItems = count,
      SuccessCount = num1,
      FailureCount = num2,
      ExecutionTime = stopwatch.Elapsed
    };
    return tables;
  }

  public static BatchOperationResponse BatchUpdateTables(BatchUpdateTablesRequest request)
  {
    Stopwatch stopwatch = Stopwatch.StartNew();
    List<string> warnings = new List<string>();
    BatchOperationResponse operationResponse1 = new BatchOperationResponse()
    {
      Operation = "BatchUpdate",
      Results = new List<ItemResult>(),
      Warnings = warnings
    };
    if (request.Items == null || !Enumerable.Any<TableUpdate>((IEnumerable<TableUpdate>) request.Items))
    {
      operationResponse1.Success = false;
      operationResponse1.Message = "No tables provided for batch update";
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
        TableUpdate update = request.Items[index];
        ItemResult itemResult1 = new ItemResult()
        {
          Index = index,
          ItemIdentifier = update.Name ?? "Unknown"
        };
        try
        {
          TableOperations.TableOperationResult tableOperationResult = TableOperations.UpdateTable(request.ConnectionName, update);
          itemResult1.Success = true;
          itemResult1.Data = (object) tableOperationResult;
          if (tableOperationResult.HasChanges)
          {
            itemResult1.Message = $"Successfully updated table '{update.Name}'";
          }
          else
          {
            itemResult1.Message = $"Table '{update.Name}' is already in the requested state";
            ItemResult itemResult2 = itemResult1;
            List<string> stringList = new List<string>();
            stringList.Add("No changes were detected. The table is already in the requested state.");
            itemResult2.Warnings = stringList;
          }
          ++num1;
          if (transactionId != null)
            TransactionOperations.RecordOperation(connectionInfo, $"Updated table '{update.Name}'");
        }
        catch (Exception ex)
        {
          itemResult1.Success = false;
          itemResult1.Message = $"Error updating table '{update.Name}': {ex.Message}";
          ++num2;
        }
        operationResponse1.Results.Add(itemResult1);
        if (!itemResult1.Success && !request.Options.ContinueOnError)
          break;
      }
      List<ItemResult> list = Enumerable.ToList<ItemResult>(Enumerable.Where<ItemResult>((IEnumerable<ItemResult>) operationResponse1.Results, (r => r.Success && r.Data != null && r.Data.GetType().GetProperty("HasChanges")?.GetValue(r.Data) is bool flag && !flag)));
      if (Enumerable.Any<ItemResult>((IEnumerable<ItemResult>) list))
        operationResponse1.Warnings.Add($"{list.Count} of {count} tables had no changes detected - they are already in the requested state.");
      if (transactionId != null)
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
                str = $"Transaction committed. All {count} tables were already in the requested state.";
              else
                str = $"Transaction committed. Updated {num3} of {count} tables.";
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
              str = $"All {count} tables were already in the requested state. Transaction remains open for explicit commit.";
            else
              str = $"Updated {num4} of {count} tables in existing transaction. Transaction remains open for explicit commit.";
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
              operationResponse1.Message = "Transaction rolled back due to errors. No tables were updated.";
            }
            catch (Exception ex)
            {
              operationResponse1.Message = "Failed to rollback transaction: " + ex.Message;
            }
          }
          else
            operationResponse1.Message = $"Batch operation failed in existing transaction. {num2} of {count} tables failed. Transaction remains open - you should rollback.";
          operationResponse1.Success = false;
        }
      }
      else if (num1 > 0)
      {
        int num5 = num1 - list.Count;
        BatchOperationResponse operationResponse4 = operationResponse1;
        string str;
        if (num5 <= 0)
          str = $"All {count} tables are already in the requested state";
        else
          str = $"Successfully processed {count} tables: {num5} updated, {list.Count} already current";
        operationResponse4.Message = str;
      }
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

  public static BatchOperationResponse BatchDeleteTables(BatchDeleteTablesRequest request)
  {
    Stopwatch stopwatch = Stopwatch.StartNew();
    List<string> warnings = new List<string>();
    BatchOperationResponse operationResponse = new BatchOperationResponse()
    {
      Operation = "BatchDelete",
      Results = new List<ItemResult>(),
      Warnings = warnings
    };
    if (request.Items == null || !Enumerable.Any<string>((IEnumerable<string>) request.Items))
    {
      operationResponse.Success = false;
      operationResponse.Message = "No tables provided for batch deletion";
      operationResponse.Summary = new BatchSummary()
      {
        TotalItems = 0,
        SuccessCount = 0,
        FailureCount = 0,
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
        ItemResult itemResult = new ItemResult()
        {
          Index = index,
          ItemIdentifier = tableName
        };
        try
        {
          TableOperations.DeleteTable(request.ConnectionName, tableName, request.ShouldCascadeDelete);
          itemResult.Success = true;
          itemResult.Message = $"Successfully deleted table '{tableName}'";
          ++num1;
          if (transactionId != null)
            TransactionOperations.RecordOperation(connectionInfo, $"Deleted table '{tableName}'");
        }
        catch (Exception ex)
        {
          itemResult.Success = false;
          itemResult.Message = $"Error deleting table '{tableName}': {ex.Message}";
          ++num2;
        }
        operationResponse.Results.Add(itemResult);
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
              operationResponse.Message = $"Transaction committed. Deleted {num1} of {count} tables.";
            }
            catch (Exception ex)
            {
              operationResponse.Message = "Failed to commit transaction: " + ex.Message;
              operationResponse.Success = false;
            }
          }
          else
            operationResponse.Message = $"Deleted {num1} of {count} tables in existing transaction. Transaction remains open for explicit commit.";
        }
        else
        {
          if (ownsTransaction)
          {
            try
            {
              TransactionOperations.RollbackTransaction(request.ConnectionName);
              operationResponse.Message = "Transaction rolled back due to errors. No tables were deleted.";
            }
            catch (Exception ex)
            {
              operationResponse.Message = "Failed to rollback transaction: " + ex.Message;
            }
          }
          else
            operationResponse.Message = "Batch delete failed in existing transaction. Transaction remains open - you should rollback.";
          operationResponse.Success = false;
        }
      }
      else if (num1 > 0)
        operationResponse.Message = $"Deleted {num1} of {count} tables.";
      if (string.IsNullOrEmpty(operationResponse.Message))
        operationResponse.Message = num2 > 0 ? "Batch delete operation completed with errors." : "Batch delete operation completed successfully.";
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
      operationResponse.Message = "Batch delete operation failed: " + ex.Message;
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

  public static BatchOperationResponse BatchGetTables(BatchGetTablesRequest request)
  {
    Stopwatch stopwatch = Stopwatch.StartNew();
    List<string> stringList = new List<string>();
    BatchOperationResponse tables = new BatchOperationResponse()
    {
      Operation = "BatchGet",
      Results = new List<ItemResult>(),
      Warnings = stringList
    };
    if (request.Items == null || !Enumerable.Any<string>((IEnumerable<string>) request.Items))
    {
      tables.Success = false;
      tables.Message = "No tables provided for batch retrieval";
      tables.Summary = new BatchSummary()
      {
        TotalItems = 0,
        SuccessCount = 0,
        FailureCount = 0,
        ExecutionTime = stopwatch.Elapsed
      };
      return tables;
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
          TableGet table = TableOperations.GetTable(request.ConnectionName, tableName);
          itemResult.Success = true;
          itemResult.Message = $"Successfully retrieved table '{tableName}'";
          itemResult.Data = (object) table;
          ++num1;
        }
        catch (Exception ex)
        {
          itemResult.Success = false;
          itemResult.Message = $"Error retrieving table '{tableName}': {ex.Message}";
          ++num2;
        }
        tables.Results.Add(itemResult);
      }
      tables.Message = $"Retrieved {num1} of {count} tables.";
      tables.Success = num2 == 0;
    }
    catch (Exception ex)
    {
      tables.Success = false;
      tables.Message = "Batch get operation failed: " + ex.Message;
      num2 = count - num1;
    }
    stopwatch.Stop();
    tables.Summary = new BatchSummary()
    {
      TotalItems = count,
      SuccessCount = num1,
      FailureCount = num2,
      ExecutionTime = stopwatch.Elapsed
    };
    return tables;
  }

  public static BatchOperationResponse BatchRenameTables(BatchRenameTablesRequest request)
  {
    Stopwatch stopwatch = Stopwatch.StartNew();
    List<string> warnings = new List<string>();
    BatchOperationResponse operationResponse = new BatchOperationResponse()
    {
      Operation = "BatchRename",
      Results = new List<ItemResult>(),
      Warnings = warnings
    };
    if (request.Items == null || !Enumerable.Any<TableRename>((IEnumerable<TableRename>) request.Items))
    {
      operationResponse.Success = false;
      operationResponse.Message = "No tables provided for batch rename";
      operationResponse.Summary = new BatchSummary()
      {
        TotalItems = 0,
        SuccessCount = 0,
        FailureCount = 0,
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
        TableRename tableRename = request.Items[index];
        ItemResult itemResult = new ItemResult()
        {
          Index = index,
          ItemIdentifier = $"{tableRename.CurrentName} -> {tableRename.NewName}"
        };
        try
        {
          TableOperations.RenameTable(request.ConnectionName, tableRename.CurrentName, tableRename.NewName);
          itemResult.Success = true;
          itemResult.Message = $"Successfully renamed table '{tableRename.CurrentName}' to '{tableRename.NewName}'";
          ++num1;
          if (transactionId != null)
            TransactionOperations.RecordOperation(connectionInfo, $"Renamed table '{tableRename.CurrentName}' to '{tableRename.NewName}'");
        }
        catch (Exception ex)
        {
          itemResult.Success = false;
          itemResult.Message = $"Error renaming table '{tableRename.CurrentName}': {ex.Message}";
          ++num2;
        }
        operationResponse.Results.Add(itemResult);
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
              operationResponse.Message = $"Transaction committed. Renamed {num1} of {count} tables.";
            }
            catch (Exception ex)
            {
              operationResponse.Message = "Failed to commit transaction: " + ex.Message;
              operationResponse.Success = false;
            }
          }
          else
            operationResponse.Message = $"Renamed {num1} of {count} tables in existing transaction. Transaction remains open for explicit commit.";
        }
        else
        {
          if (ownsTransaction)
          {
            try
            {
              TransactionOperations.RollbackTransaction(request.ConnectionName);
              operationResponse.Message = "Transaction rolled back due to errors. No tables were renamed.";
            }
            catch (Exception ex)
            {
              operationResponse.Message = "Failed to rollback transaction: " + ex.Message;
            }
          }
          else
            operationResponse.Message = "Batch rename failed in existing transaction. Transaction remains open - you should rollback.";
          operationResponse.Success = false;
        }
      }
      else if (num1 > 0)
        operationResponse.Message = $"Renamed {num1} of {count} tables.";
      if (string.IsNullOrEmpty(operationResponse.Message))
        operationResponse.Message = num2 > 0 ? "Batch rename operation completed with errors." : "Batch rename operation completed successfully.";
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
      operationResponse.Message = "Batch rename operation failed: " + ex.Message;
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
}
