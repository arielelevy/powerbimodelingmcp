// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.BatchTransactionHelper
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using PowerBIModelingMCP.Library.Common.DataStructures;
using PowerBIModelingMCP.Library.Core;
using System;
using System.Collections.Generic;

#nullable enable
namespace PowerBIModelingMCP.Library.Common;

public static class BatchTransactionHelper
{
  public static TransactionSetupResult HandleTransactionSetup(
    ConnectionInfo connectionInfo,
    bool useTransaction,
    string? connectionName,
    List<string> warnings)
  {
    if (!useTransaction)
      return new TransactionSetupResult()
      {
        TransactionId = (string) null,
        OwnsTransaction = false
      };
    if (connectionInfo.IsOffline)
    {
      warnings.Add("Transaction mode is not available when in offline mode. Operations may succeed individually while others may fail.");
      return new TransactionSetupResult()
      {
        TransactionId = (string) null,
        OwnsTransaction = false
      };
    }
    if (connectionInfo.Transaction != null)
    {
      warnings.Add("Reusing existing transaction - batch operations will NOT commit or rollback. You must explicitly commit or rollback the transaction when ready.");
      return new TransactionSetupResult()
      {
        TransactionId = connectionInfo.Transaction.TransactionId,
        OwnsTransaction = false
      };
    }
    try
    {
      TransactionBeginResult transactionBeginResult = TransactionOperations.BeginTransaction(connectionName);
      return new TransactionSetupResult()
      {
        TransactionId = transactionBeginResult.TransactionId,
        OwnsTransaction = true
      };
    }
    catch (Exception ex)
    {
      warnings.Add($"Failed to begin transaction: {ex.Message}. Operations will proceed without transaction protection.");
      return new TransactionSetupResult()
      {
        TransactionId = (string) null,
        OwnsTransaction = false
      };
    }
  }
}
