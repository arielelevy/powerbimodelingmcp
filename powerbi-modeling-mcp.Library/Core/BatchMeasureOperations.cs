// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Core.BatchMeasureOperations
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

public static class BatchMeasureOperations
{
  public static BatchOperationResponse BatchCreateMeasures(BatchCreateMeasuresRequest request)
  {
    Stopwatch stopwatch = Stopwatch.StartNew();
    List<string> warnings = new List<string>();
    BatchOperationResponse measures = new BatchOperationResponse()
    {
      Operation = "BatchCreate",
      Results = new List<ItemResult>(),
      Warnings = warnings
    };
    if (request.Items == null || !Enumerable.Any<MeasureCreate>((IEnumerable<MeasureCreate>) request.Items))
    {
      measures.Success = false;
      measures.Message = "No measures provided for batch creation";
      measures.Summary = new BatchSummary()
      {
        TotalItems = 0,
        SuccessCount = 0,
        FailureCount = 0,
        ExecutionTime = stopwatch.Elapsed
      };
      return measures;
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
        MeasureCreate def = request.Items[index];
        ItemResult itemResult = new ItemResult()
        {
          Index = index,
          ItemIdentifier = def.Name
        };
        try
        {
          MeasureOperations.MeasureOperationResult measure = MeasureOperations.CreateMeasure(request.ConnectionName, def);
          itemResult.Success = (measure.State == "Ready");
          itemResult.Message = itemResult.Success ? $"Successfully created measure '{def.Name}'" : $"Failed to create measure '{def.Name}': {measure.ErrorMessage}";
          if (itemResult.Success)
          {
            ++num1;
            if (transactionId != null)
              TransactionOperations.RecordOperation(connectionInfo, $"Created measure '{def.Name}'");
          }
          else
            ++num2;
        }
        catch (Exception ex)
        {
          itemResult.Success = false;
          itemResult.Message = $"Error creating measure '{def.Name}': {ex.Message}";
          ++num2;
          if (!request.Options.ContinueOnError)
          {
            measures.Results.Add(itemResult);
            break;
          }
        }
        measures.Results.Add(itemResult);
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
              measures.Message = $"Successfully created {num1} measures (transaction committed)";
            }
            catch (Exception ex)
            {
              measures.Success = false;
              measures.Message = "Failed to commit transaction: " + ex.Message;
              num2 = count;
              num1 = 0;
            }
          }
          else
            measures.Message = $"Created {num1} measures in existing transaction. Transaction remains open for explicit commit.";
        }
        else if (ownsTransaction)
        {
          try
          {
            TransactionOperations.RollbackTransaction(request.ConnectionName);
            measures.Message = $"Transaction rolled back due to {num2} failures";
            num1 = 0;
            num2 = count;
          }
          catch (Exception ex)
          {
            measures.Message = "Failed to rollback transaction: " + ex.Message;
          }
        }
        else
          measures.Message = "Batch create failed in existing transaction. Transaction remains open - you should rollback.";
      }
      else if (num1 > 0)
        ConnectionOperations.SaveChangesIfNeeded(connectionInfo);
      measures.Success = num2 == 0;
      if (string.IsNullOrEmpty(measures.Message))
      {
        BatchOperationResponse operationResponse = measures;
        string str;
        if (!measures.Success)
          str = $"Batch operation completed with {num2} failures out of {count} items";
        else
          str = $"Successfully created {num1} measures";
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
      measures.Success = false;
      measures.Message = "Batch create operation failed: " + ex.Message;
      num2 = count;
      num1 = 0;
    }
    stopwatch.Stop();
    measures.Summary = new BatchSummary()
    {
      TotalItems = count,
      SuccessCount = num1,
      FailureCount = num2,
      ExecutionTime = stopwatch.Elapsed
    };
    return measures;
  }

  public static BatchOperationResponse BatchUpdateMeasures(BatchUpdateMeasuresRequest request)
  {
    Stopwatch stopwatch = Stopwatch.StartNew();
    List<string> warnings = new List<string>();
    BatchOperationResponse operationResponse1 = new BatchOperationResponse()
    {
      Operation = "BatchUpdate",
      Results = new List<ItemResult>(),
      Warnings = warnings
    };
    if (request.Items == null || !Enumerable.Any<MeasureUpdate>((IEnumerable<MeasureUpdate>) request.Items))
    {
      operationResponse1.Success = false;
      operationResponse1.Message = "No measures provided for batch update";
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
        MeasureUpdate update = request.Items[index];
        ItemResult itemResult1 = new ItemResult()
        {
          Index = index,
          ItemIdentifier = update.Name
        };
        try
        {
          MeasureOperations.MeasureOperationResult measureOperationResult = MeasureOperations.UpdateMeasure(request.ConnectionName, update);
          itemResult1.Success = (measureOperationResult.State == "Ready");
          itemResult1.Data = (object) measureOperationResult;
          if (itemResult1.Success)
          {
            if (measureOperationResult.HasChanges)
            {
              itemResult1.Message = $"Successfully updated measure '{update.Name}'";
            }
            else
            {
              itemResult1.Message = $"Measure '{update.Name}' is already in the requested state";
              ItemResult itemResult2 = itemResult1;
              List<string> stringList = new List<string>();
              stringList.Add("No changes were detected. The measure is already in the requested state.");
              itemResult2.Warnings = stringList;
            }
            ++num1;
            if (transactionId != null)
              TransactionOperations.RecordOperation(connectionInfo, $"Updated measure '{update.Name}'");
          }
          else
          {
            itemResult1.Message = $"Failed to update measure '{update.Name}': {measureOperationResult.ErrorMessage}";
            ++num2;
          }
        }
        catch (Exception ex)
        {
          itemResult1.Success = false;
          itemResult1.Message = $"Error updating measure '{update.Name}': {ex.Message}";
          ++num2;
          if (!request.Options.ContinueOnError)
          {
            operationResponse1.Results.Add(itemResult1);
            break;
          }
        }
        operationResponse1.Results.Add(itemResult1);
      }
      List<ItemResult> list = Enumerable.ToList<ItemResult>(Enumerable.Where<ItemResult>((IEnumerable<ItemResult>) operationResponse1.Results, (r => r.Success && r.Data != null && r.Data.GetType().GetProperty("HasChanges")?.GetValue(r.Data) is bool flag && !flag)));
      if (Enumerable.Any<ItemResult>((IEnumerable<ItemResult>) list))
        operationResponse1.Warnings.Add($"{list.Count} of {count} measures had no changes detected - they are already in the requested state.");
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
                str = $"Transaction committed. All {count} measures were already in the requested state.";
              else
                str = $"Successfully updated {num3} measures (transaction committed)";
              operationResponse2.Message = str;
            }
            catch (Exception ex)
            {
              operationResponse1.Success = false;
              operationResponse1.Message = "Failed to commit transaction: " + ex.Message;
              num2 = count;
              num1 = 0;
            }
          }
          else
          {
            int num4 = num1 - list.Count;
            BatchOperationResponse operationResponse3 = operationResponse1;
            string str;
            if (num4 <= 0)
              str = $"All {count} measures were already in the requested state. Transaction remains open for explicit commit.";
            else
              str = $"Updated {num4} measures in existing transaction. Transaction remains open for explicit commit.";
            operationResponse3.Message = str;
          }
        }
        else if (ownsTransaction)
        {
          try
          {
            TransactionOperations.RollbackTransaction(request.ConnectionName);
            operationResponse1.Message = $"Transaction rolled back due to {num2} failures";
            num1 = 0;
            num2 = count;
          }
          catch (Exception ex)
          {
            operationResponse1.Message = "Failed to rollback transaction: " + ex.Message;
          }
        }
        else
          operationResponse1.Message = "Batch update failed in existing transaction. Transaction remains open - you should rollback.";
      }
      else if (num1 > 0)
        ConnectionOperations.SaveChangesIfNeeded(connectionInfo);
      operationResponse1.Success = num2 == 0;
      if (string.IsNullOrEmpty(operationResponse1.Message))
      {
        if (operationResponse1.Success)
        {
          int num5 = num1 - list.Count;
          BatchOperationResponse operationResponse4 = operationResponse1;
          string str;
          if (num5 <= 0)
            str = $"All {count} measures are already in the requested state";
          else
            str = $"Successfully processed {count} measures: {num5} updated, {list.Count} already current";
          operationResponse4.Message = str;
        }
        else
          operationResponse1.Message = $"Batch operation completed with {num2} failures out of {count} items";
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
      num2 = count;
      num1 = 0;
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

  public static BatchOperationResponse BatchDeleteMeasures(BatchDeleteMeasuresRequest request)
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
      operationResponse1.Message = "No measure names provided for batch deletion";
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
        string measureName = request.Items[index];
        ItemResult itemResult = new ItemResult()
        {
          Index = index,
          ItemIdentifier = measureName
        };
        try
        {
          MeasureOperations.DeleteMeasure(request.ConnectionName, measureName, request.ShouldCascadeDelete);
          itemResult.Success = true;
          itemResult.Message = $"Successfully deleted measure '{measureName}'";
          ++num1;
          if (transactionId != null)
            TransactionOperations.RecordOperation(connectionInfo, $"Deleted measure '{measureName}'");
        }
        catch (Exception ex)
        {
          itemResult.Success = false;
          itemResult.Message = $"Error deleting measure '{measureName}': {ex.Message}";
          ++num2;
          if (!request.Options.ContinueOnError)
          {
            operationResponse1.Results.Add(itemResult);
            break;
          }
        }
        operationResponse1.Results.Add(itemResult);
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
              operationResponse1.Message = $"Successfully deleted {num1} measures (transaction committed)";
            }
            catch (Exception ex)
            {
              operationResponse1.Success = false;
              operationResponse1.Message = "Failed to commit transaction: " + ex.Message;
              num2 = count;
              num1 = 0;
            }
          }
          else
            operationResponse1.Message = $"Deleted {num1} measures in existing transaction. Transaction remains open for explicit commit.";
        }
        else if (ownsTransaction)
        {
          try
          {
            TransactionOperations.RollbackTransaction(request.ConnectionName);
            operationResponse1.Message = $"Transaction rolled back due to {num2} failures";
            num1 = 0;
            num2 = count;
          }
          catch (Exception ex)
          {
            operationResponse1.Message = "Failed to rollback transaction: " + ex.Message;
          }
        }
        else
          operationResponse1.Message = "Batch delete failed in existing transaction. Transaction remains open - you should rollback.";
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
          str = $"Successfully deleted {num1} measures";
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
      num2 = count;
      num1 = 0;
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

  public static BatchOperationResponse BatchGetMeasures(BatchGetMeasuresRequest request)
  {
    Stopwatch stopwatch = Stopwatch.StartNew();
    BatchOperationResponse measures = new BatchOperationResponse()
    {
      Operation = "BatchGet",
      Results = new List<ItemResult>()
    };
    if (request.Items == null || !Enumerable.Any<string>((IEnumerable<string>) request.Items))
    {
      measures.Success = false;
      measures.Message = "No measure names provided for batch retrieval";
      measures.Summary = new BatchSummary()
      {
        TotalItems = 0,
        SuccessCount = 0,
        FailureCount = 0,
        ExecutionTime = stopwatch.Elapsed
      };
      return measures;
    }
    int count = request.Items.Count;
    int num1 = 0;
    int num2 = 0;
    try
    {
      for (int index = 0; index < request.Items.Count; ++index)
      {
        string measureName = request.Items[index];
        ItemResult itemResult = new ItemResult()
        {
          Index = index,
          ItemIdentifier = measureName
        };
        try
        {
          MeasureGet measure = MeasureOperations.GetMeasure(request.ConnectionName, measureName);
          itemResult.Success = true;
          itemResult.Message = $"Successfully retrieved measure '{measureName}'";
          itemResult.Data = (object) measure;
          ++num1;
        }
        catch (Exception ex)
        {
          itemResult.Success = false;
          itemResult.Message = $"Error retrieving measure '{measureName}': {ex.Message}";
          ++num2;
          if (!request.Options.ContinueOnError)
          {
            measures.Results.Add(itemResult);
            break;
          }
        }
        measures.Results.Add(itemResult);
      }
      measures.Success = num2 == 0;
      BatchOperationResponse operationResponse = measures;
      string str;
      if (!measures.Success)
        str = $"Batch operation completed with {num2} failures out of {count} items";
      else
        str = $"Successfully retrieved {num1} measures";
      operationResponse.Message = str;
    }
    catch (Exception ex)
    {
      measures.Success = false;
      measures.Message = "Batch get operation failed: " + ex.Message;
    }
    stopwatch.Stop();
    measures.Summary = new BatchSummary()
    {
      TotalItems = count,
      SuccessCount = num1,
      FailureCount = num2,
      ExecutionTime = stopwatch.Elapsed
    };
    return measures;
  }

  public static BatchOperationResponse BatchRenameMeasures(BatchRenameMeasuresRequest request)
  {
    Stopwatch stopwatch = Stopwatch.StartNew();
    List<string> warnings = new List<string>();
    BatchOperationResponse operationResponse1 = new BatchOperationResponse()
    {
      Operation = "BatchRename",
      Results = new List<ItemResult>(),
      Warnings = warnings
    };
    if (request.Items == null || !Enumerable.Any<MeasureRename>((IEnumerable<MeasureRename>) request.Items))
    {
      operationResponse1.Success = false;
      operationResponse1.Message = "No measure rename definitions provided";
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
        MeasureRename measureRename = request.Items[index];
        ItemResult itemResult = new ItemResult()
        {
          Index = index,
          ItemIdentifier = $"{measureRename.CurrentName} -> {measureRename.NewName}"
        };
        try
        {
          MeasureOperations.RenameMeasure(request.ConnectionName, measureRename.CurrentName, measureRename.NewName);
          itemResult.Success = true;
          itemResult.Message = $"Successfully renamed measure '{measureRename.CurrentName}' to '{measureRename.NewName}'";
          ++num1;
          if (transactionId != null)
            TransactionOperations.RecordOperation(connectionInfo, $"Renamed measure '{measureRename.CurrentName}' to '{measureRename.NewName}'");
        }
        catch (Exception ex)
        {
          itemResult.Success = false;
          itemResult.Message = $"Error renaming measure '{measureRename.CurrentName}' to '{measureRename.NewName}': {ex.Message}";
          ++num2;
          if (!request.Options.ContinueOnError)
          {
            operationResponse1.Results.Add(itemResult);
            break;
          }
        }
        operationResponse1.Results.Add(itemResult);
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
              operationResponse1.Message = $"Successfully renamed {num1} measures (transaction committed)";
            }
            catch (Exception ex)
            {
              operationResponse1.Success = false;
              operationResponse1.Message = "Failed to commit transaction: " + ex.Message;
              num2 = count;
              num1 = 0;
            }
          }
          else
            operationResponse1.Message = $"Renamed {num1} measures in existing transaction. Transaction remains open for explicit commit.";
        }
        else if (ownsTransaction)
        {
          try
          {
            TransactionOperations.RollbackTransaction(request.ConnectionName);
            operationResponse1.Message = $"Transaction rolled back due to {num2} failures";
            num1 = 0;
            num2 = count;
          }
          catch (Exception ex)
          {
            operationResponse1.Message = "Failed to rollback transaction: " + ex.Message;
          }
        }
        else
          operationResponse1.Message = "Batch rename failed in existing transaction. Transaction remains open - you should rollback.";
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
          str = $"Successfully renamed {num1} measures";
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
      num2 = count;
      num1 = 0;
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

  public static BatchOperationResponse BatchMoveMeasures(BatchMoveMeasuresRequest request)
  {
    Stopwatch stopwatch = Stopwatch.StartNew();
    List<string> warnings = new List<string>();
    BatchOperationResponse operationResponse1 = new BatchOperationResponse()
    {
      Operation = "BatchMove",
      Results = new List<ItemResult>(),
      Warnings = warnings
    };
    if (request.Items == null || !Enumerable.Any<MeasureMove>((IEnumerable<MeasureMove>) request.Items))
    {
      operationResponse1.Success = false;
      operationResponse1.Message = "No measure move definitions provided";
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
        MeasureMove measureMove = request.Items[index];
        ItemResult itemResult = new ItemResult()
        {
          Index = index,
          ItemIdentifier = $"{measureMove.Name} -> {measureMove.DestinationTableName}"
        };
        try
        {
          MeasureOperations.MoveMeasure(request.ConnectionName, measureMove.DestinationTableName, measureMove.Name);
          itemResult.Success = true;
          itemResult.Message = $"Successfully moved measure '{measureMove.Name}' to '{measureMove.DestinationTableName}'";
          ++num1;
          if (transactionId != null)
            TransactionOperations.RecordOperation(connectionInfo, $"Moved measure '{measureMove.Name}' to '{measureMove.DestinationTableName}'");
        }
        catch (Exception ex)
        {
          itemResult.Success = false;
          itemResult.Message = $"Error moving measure '{measureMove.Name}' to '{measureMove.DestinationTableName}': {ex.Message}";
          ++num2;
          if (!request.Options.ContinueOnError)
          {
            operationResponse1.Results.Add(itemResult);
            break;
          }
        }
        operationResponse1.Results.Add(itemResult);
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
              operationResponse1.Message = $"Successfully moved {num1} measures (transaction committed)";
            }
            catch (Exception ex)
            {
              operationResponse1.Success = false;
              operationResponse1.Message = "Failed to commit transaction: " + ex.Message;
              num2 = count;
              num1 = 0;
            }
          }
          else
            operationResponse1.Message = $"Moved {num1} measures in existing transaction. Transaction remains open for explicit commit.";
        }
        else if (ownsTransaction)
        {
          try
          {
            TransactionOperations.RollbackTransaction(request.ConnectionName);
            operationResponse1.Message = $"Transaction rolled back due to {num2} failures";
            num1 = 0;
            num2 = count;
          }
          catch (Exception ex)
          {
            operationResponse1.Message = "Failed to rollback transaction: " + ex.Message;
          }
        }
        else
          operationResponse1.Message = "Batch move failed in existing transaction. Transaction remains open - you should rollback.";
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
          str = $"Successfully moved {num1} measures";
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
      operationResponse1.Message = "Batch move operation failed: " + ex.Message;
      num2 = count;
      num1 = 0;
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
