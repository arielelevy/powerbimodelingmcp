// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using Microsoft.AnalysisServices.Tabular;
using ModelContextProtocol;
using PowerBIModelingMCP.Library.Common;
using PowerBIModelingMCP.Library.Common.DataStructures;
using System;
using System.Collections.Generic;

#nullable enable
namespace PowerBIModelingMCP.Library.Core;

public static class TransactionOperations
{
  public static TransactionBeginResult BeginTransaction(string? connectionName)
  {
    ConnectionInfo connection = ConnectionOperations.Get(connectionName);
    ConnectionValidator.ValidateForTransactions(connection);
    if (connection.Transaction != null)
      throw new McpException($"Connection '{connection.ConnectionName}' already has an active transaction. Commit or rollback the existing transaction first.");
    try
    {
      connection.TabularServer.BeginTransaction();
    }
    catch (Exception ex)
    {
      throw new McpException("Failed to begin server transaction: " + ex.Message);
    }
    TransactionContext transactionContext = new TransactionContext()
    {
      Server = connection.TabularServer,
      Database = connection.Database
    };
    connection.Transaction = transactionContext;
    transactionContext.Operations.Add($"Server transaction started at {transactionContext.StartTime:yyyy-MM-dd HH:mm:ss} UTC");
    return new TransactionBeginResult()
    {
      TransactionId = transactionContext.TransactionId,
      Status = "active",
      StartTime = transactionContext.StartTime.ToString("yyyy-MM-dd HH:mm:ss"),
      TransactionType = "server-side"
    };
  }

  public static TransactionCommitResult CommitTransaction(string? connectionName)
  {
    ConnectionInfo connectionInfo = ConnectionOperations.Get(connectionName);
    TransactionContext transactionContext = connectionInfo.Transaction != null ? connectionInfo.Transaction : throw new McpException($"Connection '{connectionInfo.ConnectionName}' does not have an active transaction");
    try
    {
      ModelOperationResult modelOperationResult = transactionContext.Database.Model.SaveChanges();
      transactionContext.Server.CommitTransaction();
      transactionContext.Operations.Add($"Server transaction committed and database updated at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
      TransactionCommitResult transactionCommitResult = new TransactionCommitResult { TransactionId = transactionContext.TransactionId };
      transactionCommitResult.Status = "committed";
      transactionCommitResult.OperationCount = transactionContext.Operations.Count - 2;
      transactionCommitResult.Duration = (DateTime.UtcNow - transactionContext.StartTime).TotalSeconds;
      transactionCommitResult.Operations = transactionContext.Operations;
      transactionCommitResult.TransactionType = "server-side";
      transactionCommitResult.Impact = ModelOperationResult.Empty == modelOperationResult ? (string) null : ObjectImpactSerializer.SerializeToString(modelOperationResult.Impact);
      connectionInfo.Transaction = (TransactionContext) null;
      return transactionCommitResult;
    }
    catch (Exception ex)
    {
      transactionContext.Operations.Add("Server transaction commit failed: " + ex.Message);
      throw new McpException("Failed to commit server transaction: " + ex.Message);
    }
  }

  public static TransactionRollbackResult RollbackTransaction(string? connectionName)
  {
    ConnectionInfo connectionInfo = ConnectionOperations.Get(connectionName);
    TransactionContext transactionContext = connectionInfo.Transaction != null ? connectionInfo.Transaction : throw new McpException($"Connection '{connectionInfo.ConnectionName}' does not have an active transaction");
    try
    {
      transactionContext.Server.RollbackTransaction();
      transactionContext.Operations.Add($"Server transaction rolled back at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
      TransactionRollbackResult transactionRollbackResult = new TransactionRollbackResult { TransactionId = transactionContext.TransactionId };
      transactionRollbackResult.Status = "rolled back";
      transactionRollbackResult.OperationCount = transactionContext.Operations.Count - 2;
      transactionRollbackResult.Duration = (DateTime.UtcNow - transactionContext.StartTime).TotalSeconds;
      transactionRollbackResult.Operations = transactionContext.Operations;
      transactionRollbackResult.TransactionType = "server-side";
      connectionInfo.Transaction = (TransactionContext) null;
      return transactionRollbackResult;
    }
    catch (Exception ex)
    {
      transactionContext.Operations.Add("Server transaction rollback failed: " + ex.Message);
      throw new McpException("Failed to rollback server transaction: " + ex.Message);
    }
  }

  public static TransactionStatusResult GetTransactionStatus(string? connectionName)
  {
    ConnectionInfo connectionInfo = ConnectionOperations.Get(connectionName);
    if (connectionInfo.Transaction == null)
      return new TransactionStatusResult()
      {
        Status = "no active transaction"
      };
    TransactionContext transaction = connectionInfo.Transaction;
    return new TransactionStatusResult()
    {
      TransactionId = transaction.TransactionId,
      Status = "active",
      StartTime = transaction.StartTime.ToString("yyyy-MM-dd HH:mm:ss"),
      Duration = new double?((DateTime.UtcNow - transaction.StartTime).TotalSeconds),
      OperationCount = new int?(transaction.Operations.Count),
      Operations = transaction.Operations,
      TransactionType = "server-side"
    };
  }

  public static List<ActiveTransactionInfo> ListActiveTransactions()
  {
    List<ActiveTransactionInfo> activeTransactionInfoList = new List<ActiveTransactionInfo>();
    foreach (string listConnectionName in ConnectionOperations.ListConnectionNames())
    {
      try
      {
        ConnectionInfo connectionInfo = ConnectionOperations.Get(listConnectionName);
        if (connectionInfo.Transaction != null)
        {
          TransactionContext transaction = connectionInfo.Transaction;
          activeTransactionInfoList.Add(new ActiveTransactionInfo()
          {
            TransactionId = transaction.TransactionId,
            StartTime = transaction.StartTime.ToString("yyyy-MM-dd HH:mm:ss"),
            Duration = (DateTime.UtcNow - transaction.StartTime).TotalSeconds,
            OperationCount = transaction.Operations.Count,
            Database = transaction.Database.Name,
            Server = transaction.Server.Name,
            IsCurrent = true,
            TransactionType = "server-side"
          });
        }
      }
      catch
      {
      }
    }
    return activeTransactionInfoList;
  }

  public static bool IsInTransaction(string? connectionName = null)
  {
    try
    {
      return ConnectionOperations.Get(connectionName).Transaction != null;
    }
    catch
    {
      return false;
    }
  }

  public static void RecordOperation(ConnectionInfo info, string operation)
  {
    try
    {
      if (info.Transaction == null)
        return;
      info.Transaction.Operations.Add($"{DateTime.UtcNow:HH:mm:ss} - {operation}");
    }
    catch
    {
    }
  }

  public static void CleanupServerTransactions(Server server)
  {
    foreach (string listConnectionName in ConnectionOperations.ListConnectionNames())
    {
      try
      {
        ConnectionInfo connectionInfo = ConnectionOperations.Get(listConnectionName);
        if (connectionInfo.Transaction != null)
        {
          if (connectionInfo.Transaction.Server == server)
          {
            try
            {
              server.RollbackTransaction();
              connectionInfo.Transaction.Operations.Add($"Server transaction automatically rolled back during cleanup at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            }
            catch (Exception ex)
            {
              connectionInfo.Transaction.Operations.Add("Failed to rollback transaction during cleanup: " + ex.Message);
            }
            finally
            {
              connectionInfo.Transaction = (TransactionContext) null;
            }
          }
        }
      }
      catch
      {
      }
    }
  }
}
