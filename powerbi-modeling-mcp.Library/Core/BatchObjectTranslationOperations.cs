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

public static class BatchObjectTranslationOperations
{
  public static BatchOperationResponse BatchCreateObjectTranslations(
    BatchCreateObjectTranslationsRequest request)
  {
    Stopwatch stopwatch = Stopwatch.StartNew();
    List<string> warnings = new List<string>();
    BatchOperationResponse objectTranslations = new BatchOperationResponse()
    {
      Operation = "BatchCreate",
      Results = new List<ItemResult>(),
      Warnings = warnings
    };
    if (request.Items == null || !Enumerable.Any<ObjectTranslationCreate>((IEnumerable<ObjectTranslationCreate>) request.Items))
    {
      objectTranslations.Success = false;
      objectTranslations.Message = "No object translations provided for batch creation";
      objectTranslations.Summary = new BatchSummary()
      {
        TotalItems = 0,
        SuccessCount = 0,
        FailureCount = 0,
        ExecutionTime = stopwatch.Elapsed
      };
      return objectTranslations;
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
        ObjectTranslationCreate translationDef = request.Items[index];
        ItemResult itemResult1 = new ItemResult { Index = index };
        itemResult1.ItemIdentifier = $"{translationDef.CultureName}.{translationDef.ObjectType}.{translationDef.Property}";
        ItemResult itemResult2 = itemResult1;
        try
        {
          ObjectTranslationOperations.ObjectTranslationOperationResult objectTranslation = ObjectTranslationOperations.CreateObjectTranslation(request.ConnectionName, translationDef);
          itemResult2.Success = objectTranslation.Success;
          itemResult2.Message = objectTranslation.Message ?? objectTranslation.ErrorMessage ?? "Unknown result";
          itemResult2.Data = (object) objectTranslation;
          if (objectTranslation.Success)
          {
            ++num1;
          }
          else
          {
            ++num2;
            if (!request.Options.ContinueOnError)
            {
              objectTranslations.Success = false;
              objectTranslations.Results.Add(itemResult2);
              break;
            }
          }
        }
        catch (Exception ex)
        {
          itemResult2.Success = false;
          itemResult2.Message = ex.Message;
          ++num2;
          if (!request.Options.ContinueOnError)
          {
            objectTranslations.Success = false;
            objectTranslations.Results.Add(itemResult2);
            break;
          }
        }
        objectTranslations.Results.Add(itemResult2);
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
            }
            catch (Exception ex)
            {
              objectTranslations.Success = false;
              objectTranslations.Message = "Failed to commit transaction: " + ex.Message;
            }
          }
          else
            objectTranslations.Warnings.Add("Transaction remains open for explicit commit by caller.");
        }
        else
        {
          if (ownsTransaction)
          {
            try
            {
              TransactionOperations.RollbackTransaction(request.ConnectionName);
            }
            catch (Exception ex)
            {
              objectTranslations.Message = "Failed to rollback transaction: " + ex.Message;
            }
          }
          else
            objectTranslations.Warnings.Add("Transaction remains open. Caller must handle rollback due to operation errors.");
          objectTranslations.Success = false;
        }
      }
      else if (num1 > 0)
      {
        try
        {
          connectionInfo.Database.Model.SaveChanges();
        }
        catch (Exception ex)
        {
          objectTranslations.Success = false;
          objectTranslations.Message = "Failed to save changes: " + ex.Message;
        }
      }
      if (string.IsNullOrEmpty(objectTranslations.Message))
        objectTranslations.Message = $"Batch create completed. {num1} successful, {num2} failed out of {count} total.";
    }
    catch (Exception ex1)
    {
      if (transactionId != null & ownsTransaction)
      {
        try
        {
          TransactionOperations.RollbackTransaction(request.ConnectionName);
        }
        catch (Exception ex2)
        {
          objectTranslations.Message = $"Operation failed: {ex1.Message}. Rollback also failed: {ex2.Message}";
        }
      }
      objectTranslations.Success = false;
      if (string.IsNullOrEmpty(objectTranslations.Message))
        objectTranslations.Message = "Batch operation failed: " + ex1.Message;
    }
    stopwatch.Stop();
    objectTranslations.Summary = new BatchSummary()
    {
      TotalItems = count,
      SuccessCount = num1,
      FailureCount = num2,
      ExecutionTime = stopwatch.Elapsed
    };
    return objectTranslations;
  }

  public static BatchOperationResponse BatchUpdateObjectTranslations(
    BatchUpdateObjectTranslationsRequest request)
  {
    Stopwatch stopwatch = Stopwatch.StartNew();
    List<string> warnings = new List<string>();
    BatchOperationResponse operationResponse = new BatchOperationResponse()
    {
      Operation = "BatchUpdate",
      Results = new List<ItemResult>(),
      Warnings = warnings
    };
    if (request.Items == null || !Enumerable.Any<ObjectTranslationUpdate>((IEnumerable<ObjectTranslationUpdate>) request.Items))
    {
      operationResponse.Success = false;
      operationResponse.Message = "No object translations provided for batch update";
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
        ObjectTranslationUpdate translationDef = request.Items[index];
        ItemResult itemResult1 = new ItemResult { Index = index };
        itemResult1.ItemIdentifier = $"{translationDef.CultureName}.{translationDef.ObjectType}.{translationDef.Property}";
        ItemResult itemResult2 = itemResult1;
        try
        {
          ObjectTranslationOperations.ObjectTranslationOperationResult translationOperationResult = ObjectTranslationOperations.UpdateObjectTranslation(request.ConnectionName, translationDef);
          itemResult2.Success = translationOperationResult.Success;
          itemResult2.Data = (object) translationOperationResult;
          if (translationOperationResult.Success)
          {
            if (translationOperationResult.HasChanges)
            {
              itemResult2.Message = translationOperationResult.Message ?? "Successfully updated object translation";
            }
            else
            {
              itemResult2.Message = $"Object translation for {translationDef.CultureName}.{translationDef.ObjectType}.{translationDef.Property} is already in the requested state";
              ItemResult itemResult3 = itemResult2;
              List<string> stringList = new List<string>();
              stringList.Add("No changes were detected. The object translation is already in the requested state.");
              itemResult3.Warnings = stringList;
            }
            ++num1;
          }
          else
          {
            itemResult2.Message = translationOperationResult.Message ?? translationOperationResult.ErrorMessage ?? "Failed to update object translation";
            ++num2;
            if (!request.Options.ContinueOnError)
            {
              operationResponse.Success = false;
              operationResponse.Results.Add(itemResult2);
              break;
            }
          }
        }
        catch (Exception ex)
        {
          itemResult2.Success = false;
          itemResult2.Message = ex.Message;
          ++num2;
          if (!request.Options.ContinueOnError)
          {
            operationResponse.Success = false;
            operationResponse.Results.Add(itemResult2);
            break;
          }
        }
        operationResponse.Results.Add(itemResult2);
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
            }
            catch (Exception ex)
            {
              operationResponse.Success = false;
              operationResponse.Message = "Failed to commit transaction: " + ex.Message;
            }
          }
          else
            operationResponse.Warnings.Add("Transaction remains open for explicit commit by caller.");
        }
        else
        {
          if (ownsTransaction)
          {
            try
            {
              TransactionOperations.RollbackTransaction(request.ConnectionName);
            }
            catch (Exception ex)
            {
              operationResponse.Message = "Failed to rollback transaction: " + ex.Message;
            }
          }
          else
            operationResponse.Warnings.Add("Transaction remains open. Caller must handle rollback due to operation errors.");
          operationResponse.Success = false;
        }
      }
      else if (num1 > 0)
      {
        try
        {
          connectionInfo.Database.Model.SaveChanges();
        }
        catch (Exception ex)
        {
          operationResponse.Success = false;
          operationResponse.Message = "Failed to save changes: " + ex.Message;
        }
      }
      if (string.IsNullOrEmpty(operationResponse.Message))
        operationResponse.Message = $"Batch update completed. {num1} successful, {num2} failed out of {count} total.";
    }
    catch (Exception ex1)
    {
      if (transactionId != null & ownsTransaction)
      {
        try
        {
          TransactionOperations.RollbackTransaction(request.ConnectionName);
        }
        catch (Exception ex2)
        {
          operationResponse.Message = $"Operation failed: {ex1.Message}. Rollback also failed: {ex2.Message}";
        }
      }
      operationResponse.Success = false;
      if (string.IsNullOrEmpty(operationResponse.Message))
        operationResponse.Message = "Batch operation failed: " + ex1.Message;
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

  public static BatchOperationResponse BatchDeleteObjectTranslations(
    BatchDeleteObjectTranslationsRequest request)
  {
    Stopwatch stopwatch = Stopwatch.StartNew();
    List<string> warnings = new List<string>();
    BatchOperationResponse operationResponse = new BatchOperationResponse()
    {
      Operation = "BatchDelete",
      Results = new List<ItemResult>(),
      Warnings = warnings
    };
    if (request.Items == null || !Enumerable.Any<ObjectTranslationDelete>((IEnumerable<ObjectTranslationDelete>) request.Items))
    {
      operationResponse.Success = false;
      operationResponse.Message = "No object translation identifiers provided for batch deletion";
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
        ObjectTranslationDelete translationDef = request.Items[index];
        ItemResult itemResult1 = new ItemResult { Index = index };
        itemResult1.ItemIdentifier = $"{translationDef.CultureName}.{translationDef.ObjectType}.{translationDef.Property}";
        ItemResult itemResult2 = itemResult1;
        try
        {
          ObjectTranslationOperations.ObjectTranslationOperationResult translationOperationResult = ObjectTranslationOperations.DeleteObjectTranslation(request.ConnectionName, translationDef);
          itemResult2.Success = translationOperationResult.Success;
          itemResult2.Message = translationOperationResult.Message ?? translationOperationResult.ErrorMessage ?? "Unknown result";
          itemResult2.Data = (object) translationOperationResult;
          if (translationOperationResult.Success)
          {
            ++num1;
          }
          else
          {
            ++num2;
            if (!request.Options.ContinueOnError)
            {
              operationResponse.Success = false;
              operationResponse.Results.Add(itemResult2);
              break;
            }
          }
        }
        catch (Exception ex)
        {
          itemResult2.Success = false;
          itemResult2.Message = ex.Message;
          ++num2;
          if (!request.Options.ContinueOnError)
          {
            operationResponse.Success = false;
            operationResponse.Results.Add(itemResult2);
            break;
          }
        }
        operationResponse.Results.Add(itemResult2);
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
            }
            catch (Exception ex)
            {
              operationResponse.Success = false;
              operationResponse.Message = "Failed to commit transaction: " + ex.Message;
            }
          }
          else
            operationResponse.Warnings.Add("Transaction remains open for explicit commit by caller.");
        }
        else
        {
          if (ownsTransaction)
          {
            try
            {
              TransactionOperations.RollbackTransaction(request.ConnectionName);
            }
            catch (Exception ex)
            {
              operationResponse.Message = "Failed to rollback transaction: " + ex.Message;
            }
          }
          else
            operationResponse.Warnings.Add("Transaction remains open. Caller must handle rollback due to operation errors.");
          operationResponse.Success = false;
        }
      }
      else if (num1 > 0)
      {
        try
        {
          connectionInfo.Database.Model.SaveChanges();
        }
        catch (Exception ex)
        {
          operationResponse.Success = false;
          operationResponse.Message = "Failed to save changes: " + ex.Message;
        }
      }
      if (string.IsNullOrEmpty(operationResponse.Message))
        operationResponse.Message = $"Batch delete completed. {num1} successful, {num2} failed out of {count} total.";
    }
    catch (Exception ex1)
    {
      if (transactionId != null & ownsTransaction)
      {
        try
        {
          TransactionOperations.RollbackTransaction(request.ConnectionName);
        }
        catch (Exception ex2)
        {
          operationResponse.Message = $"Operation failed: {ex1.Message}. Rollback also failed: {ex2.Message}";
        }
      }
      operationResponse.Success = false;
      if (string.IsNullOrEmpty(operationResponse.Message))
        operationResponse.Message = "Batch operation failed: " + ex1.Message;
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

  public static BatchOperationResponse BatchGetObjectTranslations(
    BatchGetObjectTranslationsRequest request)
  {
    Stopwatch stopwatch = Stopwatch.StartNew();
    BatchOperationResponse objectTranslations = new BatchOperationResponse()
    {
      Operation = "BatchGet",
      Results = new List<ItemResult>()
    };
    if (request.Items == null || !Enumerable.Any<ObjectTranslationBase>((IEnumerable<ObjectTranslationBase>) request.Items))
    {
      objectTranslations.Success = false;
      objectTranslations.Message = "No object translation identifiers provided for batch retrieval";
      objectTranslations.Summary = new BatchSummary()
      {
        TotalItems = 0,
        SuccessCount = 0,
        FailureCount = 0,
        ExecutionTime = stopwatch.Elapsed
      };
      return objectTranslations;
    }
    int count = request.Items.Count;
    int num1 = 0;
    int num2 = 0;
    try
    {
      for (int index = 0; index < request.Items.Count; ++index)
      {
        ObjectTranslationBase translation = request.Items[index];
        ItemResult itemResult1 = new ItemResult { Index = index };
        itemResult1.ItemIdentifier = $"{translation.CultureName}.{translation.ObjectType}.{translation.Property}";
        ItemResult itemResult2 = itemResult1;
        try
        {
          ObjectTranslationGet objectTranslation = ObjectTranslationOperations.GetObjectTranslation(request.ConnectionName, translation);
          if (objectTranslation != null)
          {
            itemResult2.Success = true;
            itemResult2.Message = "Object translation retrieved successfully";
            itemResult2.Data = (object) objectTranslation;
            ++num1;
          }
          else
          {
            itemResult2.Success = false;
            itemResult2.Message = "Object translation not found";
            ++num2;
            if (!request.Options.ContinueOnError)
            {
              objectTranslations.Success = false;
              objectTranslations.Results.Add(itemResult2);
              break;
            }
          }
        }
        catch (Exception ex)
        {
          itemResult2.Success = false;
          itemResult2.Message = ex.Message;
          ++num2;
          if (!request.Options.ContinueOnError)
          {
            objectTranslations.Success = false;
            objectTranslations.Results.Add(itemResult2);
            break;
          }
        }
        objectTranslations.Results.Add(itemResult2);
      }
      if (string.IsNullOrEmpty(objectTranslations.Message))
        objectTranslations.Message = $"Batch get completed. {num1} successful, {num2} failed out of {count} total.";
    }
    catch (Exception ex)
    {
      objectTranslations.Success = false;
      objectTranslations.Message = "Batch operation failed: " + ex.Message;
    }
    stopwatch.Stop();
    objectTranslations.Summary = new BatchSummary()
    {
      TotalItems = count,
      SuccessCount = num1,
      FailureCount = num2,
      ExecutionTime = stopwatch.Elapsed
    };
    return objectTranslations;
  }
}
