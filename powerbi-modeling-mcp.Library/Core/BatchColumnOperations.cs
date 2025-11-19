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

public static class BatchColumnOperations
{
  public static BatchOperationResponse BatchCreateColumns(BatchCreateColumnsRequest request)
  {
    Stopwatch stopwatch = Stopwatch.StartNew();
    List<string> warnings = new List<string>();
    BatchOperationResponse columns = new BatchOperationResponse()
    {
      Operation = "BatchCreate",
      Results = new List<ItemResult>(),
      Warnings = warnings
    };
    if (request.Items == null || !Enumerable.Any<ColumnCreate>((IEnumerable<ColumnCreate>) request.Items))
    {
      columns.Success = false;
      columns.Message = "No columns provided for batch creation";
      columns.Summary = new BatchSummary()
      {
        TotalItems = 0,
        SuccessCount = 0,
        FailureCount = 0,
        ExecutionTime = stopwatch.Elapsed
      };
      return columns;
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
        ColumnCreate def = request.Items[index];
        ItemResult itemResult1 = new ItemResult()
        {
          Index = index,
          ItemIdentifier = $"{def.TableName}.{def.Name}"
        };
        try
        {
          ColumnOperations.ColumnOperationResult column = ColumnOperations.CreateColumn(request.ConnectionName, def);
          itemResult1.Success = (column.State == "Ready");
          ItemResult itemResult2 = itemResult1;
          string str;
          if (!itemResult1.Success)
            str = $"Failed to create column '{def.Name}' in table '{def.TableName}': {column.ErrorMessage}";
          else
            str = $"Successfully created column '{def.Name}' in table '{def.TableName}'";
          itemResult2.Message = str;
          if (itemResult1.Success)
          {
            ++num1;
            if (transactionId != null)
              TransactionOperations.RecordOperation(connectionInfo, $"Created column '{def.TableName}.{def.Name}'");
          }
          else
            ++num2;
        }
        catch (Exception ex)
        {
          itemResult1.Success = false;
          itemResult1.Message = $"Error creating column '{def.Name}' in table '{def.TableName}': {ex.Message}";
          ++num2;
        }
        columns.Results.Add(itemResult1);
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
              columns.Message = $"Transaction committed. Created {num1} of {count} columns.";
            }
            catch (Exception ex)
            {
              columns.Message = "Failed to commit transaction: " + ex.Message;
              columns.Success = false;
            }
          }
          else
            columns.Message = $"Created {num1} of {count} columns in existing transaction. Transaction remains open for explicit commit.";
        }
        else
        {
          if (ownsTransaction)
          {
            try
            {
              TransactionOperations.RollbackTransaction(request.ConnectionName);
              columns.Message = "Transaction rolled back due to errors. No columns were created.";
            }
            catch (Exception ex)
            {
              columns.Message = "Failed to rollback transaction: " + ex.Message;
            }
          }
          else
            columns.Message = $"Batch operation failed in existing transaction. {num2} of {count} columns failed. Transaction remains open - you should rollback.";
          columns.Success = false;
        }
      }
      else if (num1 > 0)
        columns.Message = $"Created {num1} of {count} columns.";
      if (string.IsNullOrEmpty(columns.Message))
        columns.Message = num2 > 0 ? "Batch create operation completed with errors." : "Batch create operation completed successfully.";
      columns.Success = num2 == 0 || request.Options.ContinueOnError && num1 > 0;
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
      columns.Success = false;
      columns.Message = "Batch create operation failed: " + ex.Message;
      num2 = count - num1;
    }
    stopwatch.Stop();
    columns.Summary = new BatchSummary()
    {
      TotalItems = count,
      SuccessCount = num1,
      FailureCount = num2,
      ExecutionTime = stopwatch.Elapsed
    };
    return columns;
  }

  public static BatchOperationResponse BatchUpdateColumns(BatchUpdateColumnsRequest request)
  {
    Stopwatch stopwatch = Stopwatch.StartNew();
    List<string> warnings = new List<string>();
    BatchOperationResponse operationResponse1 = new BatchOperationResponse()
    {
      Operation = "BatchUpdate",
      Results = new List<ItemResult>(),
      Warnings = warnings
    };
    if (request.Items == null || !Enumerable.Any<ColumnUpdate>((IEnumerable<ColumnUpdate>) request.Items))
    {
      operationResponse1.Success = false;
      operationResponse1.Message = "No columns provided for batch update";
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
        ColumnUpdate update = request.Items[index];
        ItemResult itemResult1 = new ItemResult()
        {
          Index = index,
          ItemIdentifier = $"{update.TableName}.{update.Name}"
        };
        try
        {
          ColumnOperations.ColumnOperationResult columnOperationResult = ColumnOperations.UpdateColumn(request.ConnectionName, update);
          itemResult1.Success = (columnOperationResult.State == "Ready");
          itemResult1.Data = (object) columnOperationResult;
          if (itemResult1.Success)
          {
            if (columnOperationResult.HasChanges)
            {
              itemResult1.Message = $"Successfully updated column '{update.Name}' in table '{update.TableName}'";
            }
            else
            {
              itemResult1.Message = $"Column '{update.Name}' in table '{update.TableName}' is already in the requested state";
              ItemResult itemResult2 = itemResult1;
              List<string> stringList = new List<string>();
              stringList.Add("No changes were detected. The column is already in the requested state.");
              itemResult2.Warnings = stringList;
            }
            ++num1;
            if (transactionId != null)
              TransactionOperations.RecordOperation(connectionInfo, $"Updated column '{update.TableName}.{update.Name}'");
          }
          else
          {
            itemResult1.Message = $"Failed to update column '{update.Name}' in table '{update.TableName}': {columnOperationResult.ErrorMessage}";
            ++num2;
          }
        }
        catch (Exception ex)
        {
          itemResult1.Success = false;
          itemResult1.Message = $"Error updating column '{update.Name}' in table '{update.TableName}': {ex.Message}";
          ++num2;
        }
        operationResponse1.Results.Add(itemResult1);
        if (!itemResult1.Success && !request.Options.ContinueOnError)
          break;
      }
      List<ItemResult> list = Enumerable.ToList<ItemResult>(Enumerable.Where<ItemResult>((IEnumerable<ItemResult>) operationResponse1.Results, (r => r.Success && r.Data != null && r.Data.GetType().GetProperty("HasChanges")?.GetValue(r.Data) is bool flag && !flag)));
      if (Enumerable.Any<ItemResult>((IEnumerable<ItemResult>) list))
        operationResponse1.Warnings.Add($"{list.Count} of {count} columns had no changes detected - they are already in the requested state.");
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
                str = $"Transaction committed. All {count} columns were already in the requested state.";
              else
                str = $"Transaction committed. Updated {num3} of {count} columns.";
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
              str = $"All {count} columns were already in the requested state. Transaction remains open for explicit commit.";
            else
              str = $"Updated {num4} of {count} columns in existing transaction. Transaction remains open for explicit commit.";
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
              operationResponse1.Message = "Transaction rolled back due to errors. No columns were updated.";
            }
            catch (Exception ex)
            {
              operationResponse1.Message = "Failed to rollback transaction: " + ex.Message;
            }
          }
          else
            operationResponse1.Message = $"Batch operation failed in existing transaction. {num2} of {count} columns failed. Transaction remains open - you should rollback.";
          operationResponse1.Success = false;
        }
      }
      else if (num1 > 0)
        operationResponse1.Message = $"Updated {num1} of {count} columns.";
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

  public static BatchOperationResponse BatchDeleteColumns(BatchDeleteColumnsRequest request)
  {
    Stopwatch stopwatch = Stopwatch.StartNew();
    List<string> warnings = new List<string>();
    BatchOperationResponse operationResponse = new BatchOperationResponse()
    {
      Operation = "BatchDelete",
      Results = new List<ItemResult>(),
      Warnings = warnings
    };
    if (request.Items == null || !Enumerable.Any<ColumnIdentifier>((IEnumerable<ColumnIdentifier>) request.Items))
    {
      operationResponse.Success = false;
      operationResponse.Message = "No columns provided for batch deletion";
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
        ColumnIdentifier columnIdentifier = request.Items[index];
        ItemResult itemResult1 = new ItemResult()
        {
          Index = index,
          ItemIdentifier = $"{columnIdentifier.TableName}.{columnIdentifier.Name}"
        };
        try
        {
          string tableName = columnIdentifier.TableName;
          string name = columnIdentifier.Name;
          List<string> stringList = ColumnOperations.DeleteColumn(request.ConnectionName, tableName, name, request.ShouldCascadeDelete);
          itemResult1.Success = true;
          itemResult1.Message = $"Successfully deleted column '{name}' from table '{tableName}'";
          if (Enumerable.Any<string>((IEnumerable<string>) stringList))
          {
            ItemResult itemResult2 = itemResult1;
            itemResult2.Message = $"{itemResult2.Message}\n\nWarnings:\n{string.Join("\n\n", (IEnumerable<string>) stringList)}";
          }
          ++num1;
          if (transactionId != null)
            TransactionOperations.RecordOperation(connectionInfo, $"Deleted column '{columnIdentifier.TableName}.{columnIdentifier.Name}'");
        }
        catch (Exception ex)
        {
          itemResult1.Success = false;
          itemResult1.Message = $"Error deleting column '{columnIdentifier.TableName}.{columnIdentifier.Name}': {ex.Message}";
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
              operationResponse.Message = $"Transaction committed. Deleted {num1} of {count} columns.";
            }
            catch (Exception ex)
            {
              operationResponse.Message = "Failed to commit transaction: " + ex.Message;
              operationResponse.Success = false;
            }
          }
          else
            operationResponse.Message = $"Deleted {num1} of {count} columns in existing transaction. Transaction remains open for explicit commit.";
        }
        else
        {
          if (ownsTransaction)
          {
            try
            {
              TransactionOperations.RollbackTransaction(request.ConnectionName);
              operationResponse.Message = "Transaction rolled back due to errors. No columns were deleted.";
            }
            catch (Exception ex)
            {
              operationResponse.Message = "Failed to rollback transaction: " + ex.Message;
            }
          }
          else
            operationResponse.Message = $"Batch operation failed in existing transaction. {num2} of {count} columns failed. Transaction remains open - you should rollback.";
          operationResponse.Success = false;
        }
      }
      else if (num1 > 0)
        operationResponse.Message = $"Deleted {num1} of {count} columns.";
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

  public static BatchOperationResponse BatchGetColumns(BatchGetColumnsRequest request)
  {
    Stopwatch stopwatch = Stopwatch.StartNew();
    BatchOperationResponse columns = new BatchOperationResponse()
    {
      Operation = "BatchGet",
      Results = new List<ItemResult>()
    };
    if (request.Items == null || !Enumerable.Any<ColumnIdentifier>((IEnumerable<ColumnIdentifier>) request.Items))
    {
      columns.Success = false;
      columns.Message = "No columns provided for batch retrieval";
      columns.Summary = new BatchSummary()
      {
        TotalItems = 0,
        SuccessCount = 0,
        FailureCount = 0,
        ExecutionTime = stopwatch.Elapsed
      };
      return columns;
    }
    int count = request.Items.Count;
    int num1 = 0;
    int num2 = 0;
    try
    {
      for (int index = 0; index < request.Items.Count; ++index)
      {
        ColumnIdentifier columnIdentifier = request.Items[index];
        ItemResult itemResult = new ItemResult()
        {
          Index = index,
          ItemIdentifier = $"{columnIdentifier.TableName}.{columnIdentifier.Name}"
        };
        try
        {
          string tableName = columnIdentifier.TableName;
          string name = columnIdentifier.Name;
          ColumnGet column = ColumnOperations.GetColumn(request.ConnectionName, tableName, name);
          itemResult.Success = true;
          itemResult.Message = $"Successfully retrieved column '{name}' from table '{tableName}'";
          itemResult.Data = (object) column;
          ++num1;
        }
        catch (Exception ex)
        {
          itemResult.Success = false;
          itemResult.Message = $"Error retrieving column '{columnIdentifier.TableName}.{columnIdentifier.Name}': {ex.Message}";
          ++num2;
        }
        columns.Results.Add(itemResult);
      }
      columns.Message = $"Retrieved {num1} of {count} columns.";
      columns.Success = num2 == 0;
    }
    catch (Exception ex)
    {
      columns.Success = false;
      columns.Message = "Batch get operation failed: " + ex.Message;
      num2 = count - num1;
    }
    stopwatch.Stop();
    columns.Summary = new BatchSummary()
    {
      TotalItems = count,
      SuccessCount = num1,
      FailureCount = num2,
      ExecutionTime = stopwatch.Elapsed
    };
    return columns;
  }

  public static BatchOperationResponse BatchRenameColumns(BatchRenameColumnsRequest request)
  {
    Stopwatch stopwatch = Stopwatch.StartNew();
    List<string> warnings = new List<string>();
    BatchOperationResponse operationResponse = new BatchOperationResponse()
    {
      Operation = "BatchRename",
      Results = new List<ItemResult>(),
      Warnings = warnings
    };
    if (request.Items == null || !Enumerable.Any<ColumnRename>((IEnumerable<ColumnRename>) request.Items))
    {
      operationResponse.Success = false;
      operationResponse.Message = "No columns provided for batch rename";
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
        ColumnRename columnRename = request.Items[index];
        ItemResult itemResult = new ItemResult()
        {
          Index = index,
          ItemIdentifier = $"{columnRename.TableName}.{columnRename.CurrentName}"
        };
        try
        {
          ColumnOperations.RenameColumn(request.ConnectionName, columnRename.TableName, columnRename.CurrentName, columnRename.NewName);
          itemResult.Success = true;
          itemResult.Message = $"Successfully renamed column '{columnRename.CurrentName}' to '{columnRename.NewName}' in table '{columnRename.TableName}'";
          ++num1;
          if (transactionId != null)
            TransactionOperations.RecordOperation(connectionInfo, $"Renamed column '{columnRename.TableName}.{columnRename.CurrentName}' to '{columnRename.NewName}'");
        }
        catch (Exception ex)
        {
          itemResult.Success = false;
          itemResult.Message = $"Error renaming column '{columnRename.CurrentName}' in table '{columnRename.TableName}': {ex.Message}";
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
              operationResponse.Message = $"Transaction committed. Renamed {num1} of {count} columns.";
            }
            catch (Exception ex)
            {
              operationResponse.Message = "Failed to commit transaction: " + ex.Message;
              operationResponse.Success = false;
            }
          }
          else
            operationResponse.Message = $"Renamed {num1} of {count} columns in existing transaction. Transaction remains open for explicit commit.";
        }
        else
        {
          if (ownsTransaction)
          {
            try
            {
              TransactionOperations.RollbackTransaction(request.ConnectionName);
              operationResponse.Message = "Transaction rolled back due to errors. No columns were renamed.";
            }
            catch (Exception ex)
            {
              operationResponse.Message = "Failed to rollback transaction: " + ex.Message;
            }
          }
          else
            operationResponse.Message = $"Batch operation failed in existing transaction. {num2} of {count} columns failed. Transaction remains open - you should rollback.";
          operationResponse.Success = false;
        }
      }
      else if (num1 > 0)
        operationResponse.Message = $"Renamed {num1} of {count} columns.";
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
