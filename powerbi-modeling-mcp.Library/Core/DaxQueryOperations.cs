// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using Microsoft.AnalysisServices.AdomdClient;
using ModelContextProtocol;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Reflection;

#nullable enable
namespace PowerBIModelingMCP.Library.Core;

public static class DaxQueryOperations
{
  private const int DEFAULT_TIMEOUT_SECONDS = 200;
  private const int DEFAULT_VALIDATION_TIMEOUT_SECONDS = 10;
  private const int DEFAULT_MAX_ROWS = 2147483647 /*0x7FFFFFFF*/;
  private const int ABSOLUTE_MAX_ROWS = 2147483647 /*0x7FFFFFFF*/;

  private static string CreateConnectionString(string serverName, string databaseName)
  {
    string str = $"Data Source={serverName};Initial Catalog={databaseName};";
    return $"{str}{(str.EndsWith(";") ? "" : ";")}Application Name=MCP-PBIModeling";
  }

  private static void ValidateCommonParameters(string query, int? timeoutSeconds, int? maxRows = null)
  {
    if (string.IsNullOrWhiteSpace(query))
      throw new McpException("Query cannot be null or empty");
    if (timeoutSeconds.HasValue && timeoutSeconds.Value <= 0)
      throw new McpException("TimeoutSeconds must be greater than 0");
    if (maxRows.HasValue && maxRows.Value <= 0)
      throw new McpException("MaxRows must be greater than 0");
  }

  private static string GetDataTypeString(Type type)
  {
    if (object.Equals(type, typeof (string)))
      return "String";
    if (object.Equals(type, typeof (int)) || object.Equals(type, typeof (int?)))
      return "Int32";
    if (object.Equals(type, typeof (long)) || object.Equals(type, typeof (long?)))
      return "Int64";
    if (object.Equals(type, typeof (double)) || object.Equals(type, typeof (double?)))
      return "Double";
    if (object.Equals(type, typeof (Decimal)) || object.Equals(type, typeof (Decimal?)))
      return "Decimal";
    if (object.Equals(type, typeof (DateTime)) || object.Equals(type, typeof (DateTime?)))
      return "DateTime";
    if (object.Equals(type, typeof (bool)) || object.Equals(type, typeof (bool?)))
      return "Boolean";
    return object.Equals(type, typeof (byte[])) ? "Binary" : ((MemberInfo) type).Name;
  }

  public static DaxQueryResult ExecuteDaxQuery(string? connectionName, DaxQueryExecute queryDef)
  {
    if (queryDef == null)
      throw new McpException("Query definition cannot be null");
    int num1 = queryDef.TimeoutSeconds ?? 200;
    int num2 = queryDef.MaxRows ?? int.MaxValue;
    DaxQueryOperations.ValidateCommonParameters(queryDef.Query, new int?(num1), new int?(num2));
    PowerBIModelingMCP.Library.Common.DataStructures.ConnectionInfo connectionInfo = ConnectionOperations.Get(connectionName);
    ConnectionValidator.ValidateForDaxQueries(connectionInfo);
    DaxQueryResult daxQueryResult = new DaxQueryResult();
    Stopwatch stopwatch = Stopwatch.StartNew();
    try
    {
      AdomdConnection adomdConnection = connectionInfo.AdomdConnection;
      if (adomdConnection.State != (ConnectionState)1)
        throw new McpException("Connection is not open. Please reconnect.");
      using (AdomdCommand adomdCommand = new AdomdCommand(queryDef.Query, adomdConnection))
      {
        adomdCommand.CommandTimeout = num1;
        using (AdomdDataReader adomdDataReader = adomdCommand.ExecuteReader())
        {
          int fieldCount = adomdDataReader.FieldCount;
          for (int ordinal = 0; ordinal < fieldCount; ++ordinal)
            daxQueryResult.Columns.Add(new DaxColumnInfo()
            {
              Name = adomdDataReader.GetName(ordinal),
              DataType = DaxQueryOperations.GetDataTypeString(adomdDataReader.GetFieldType(ordinal)),
              IsNullable = true,
              Ordinal = ordinal
            });
          int num3;
          for (num3 = 0; adomdDataReader.Read() && (num2 == int.MaxValue || num3 < num2); ++num3)
          {
            if (queryDef.ReturnRows)
            {
              Dictionary<string, object> dictionary = new Dictionary<string, object>();
              for (int ordinal = 0; ordinal < fieldCount; ++ordinal)
              {
                object obj = adomdDataReader.IsDBNull(ordinal) ? (object) null : adomdDataReader.GetValue(ordinal);
                dictionary[adomdDataReader.GetName(ordinal)] = obj;
              }
              daxQueryResult.Rows.Add(dictionary);
            }
          }
          daxQueryResult.RowCount = num3;
          daxQueryResult.Success = true;
          TransactionOperations.RecordOperation(connectionInfo, $"Executed DAX query returning {num3} rows");
        }
      }
    }
    catch (Exception ex)
    {
      daxQueryResult.Success = false;
      daxQueryResult.ErrorMessage = ex.Message;
    }
    finally
    {
      stopwatch.Stop();
      daxQueryResult.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
    }
    return daxQueryResult;
  }

  public static DaxValidationResult ValidateDaxQuery(
    string? connectionName,
    DaxQueryValidate queryDef)
  {
    if (queryDef == null)
      throw new McpException("Query definition cannot be null");
    int num1 = queryDef.TimeoutSeconds ?? 10;
    DaxQueryOperations.ValidateCommonParameters(queryDef.Query, new int?(num1));
    PowerBIModelingMCP.Library.Common.DataStructures.ConnectionInfo connectionInfo = ConnectionOperations.Get(connectionName);
    ConnectionValidator.ValidateForDaxQueries(connectionInfo);
    DaxValidationResult validationResult1 = new DaxValidationResult();
    Stopwatch stopwatch = Stopwatch.StartNew();
    try
    {
      AdomdConnection adomdConnection = connectionInfo.AdomdConnection;
      if (adomdConnection.State != (ConnectionState)1)
        throw new McpException("Connection is not open. Please reconnect.");
      using (AdomdCommand adomdCommand = new AdomdCommand(queryDef.Query, adomdConnection))
      {
        adomdCommand.CommandTimeout = num1;
        adomdCommand.Properties.Add(new AdomdProperty("ExecutionMode", (object) "Prepare"));
        using (AdomdDataReader adomdDataReader = adomdCommand.ExecuteReader((CommandBehavior) 2))
        {
          DataTable schemaTable = adomdDataReader.GetSchemaTable();
          if (schemaTable != null)
          {
            int num2 = 0;
            foreach (DataRow row in (InternalDataCollectionBase) schemaTable.Rows)
            {
              List<DaxColumnInfo> expectedColumns = validationResult1.ExpectedColumns;
              DaxColumnInfo daxColumnInfo = new DaxColumnInfo();
              string str = row["ColumnName"]?.ToString();
              if (str == null)
                str = $"Column{num2}";
              daxColumnInfo.Name = str;
              daxColumnInfo.DataType = DaxQueryOperations.GetDataTypeString((Type) (row["DataType"] ?? (object) typeof (object)));
              daxColumnInfo.IsNullable = (bool) (row["AllowDBNull"] ?? (object) true);
              daxColumnInfo.Ordinal = num2++;
              expectedColumns.Add(daxColumnInfo);
            }
          }
          validationResult1.IsValid = true;
          TransactionOperations.RecordOperation(connectionInfo, "Validated DAX query successfully");
        }
      }
    }
    catch (Exception ex)
    {
      validationResult1.IsValid = false;
      validationResult1.ErrorMessage = ex.Message;
      if (ex is AdomdException adomdException)
      {
        validationResult1.DetailedError = "ADOMD Error: " + adomdException.Message;
        if (adomdException.InnerException != null)
        {
          DaxValidationResult validationResult2 = validationResult1;
          validationResult2.DetailedError = $"{validationResult2.DetailedError} Inner: {adomdException.InnerException.Message}";
        }
      }
    }
    finally
    {
      stopwatch.Stop();
      validationResult1.ValidationTimeMs = stopwatch.ElapsedMilliseconds;
    }
    return validationResult1;
  }

  public static NL2DAXPromptTemplateResult GetNL2DAXPromptTemplate()
  {
    NL2DAXPromptTemplateResult daxPromptTemplate = new NL2DAXPromptTemplateResult();
    try
    {
      string str = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "NL2DAXPromptTemplate.txt");
      if (!File.Exists(str))
      {
        daxPromptTemplate.Success = false;
        daxPromptTemplate.ErrorMessage = "NL2DAX prompt template file not found at: " + str;
        return daxPromptTemplate;
      }
      daxPromptTemplate.TemplateContent = File.ReadAllText(str);
      daxPromptTemplate.Success = true;
    }
    catch (Exception ex)
    {
      daxPromptTemplate.Success = false;
      daxPromptTemplate.ErrorMessage = "Error reading NL2DAX prompt template: " + ex.Message;
    }
    return daxPromptTemplate;
  }

  public static ClearCacheResult ClearCache(string? connectionName)
  {
    ClearCacheResult clearCacheResult = new ClearCacheResult();
    try
    {
      PowerBIModelingMCP.Library.Common.DataStructures.ConnectionInfo connectionInfo = ConnectionOperations.Get(connectionName);
      ConnectionValidator.ValidateForDaxQueries(connectionInfo);
      string id = connectionInfo.Database.ID;
      string name = connectionInfo.Database.Name;
      string str = $"<Batch xmlns=\"http://schemas.microsoft.com/analysisservices/2003/engine\">\r\n\t<ClearCache>\r\n\t\t<Object>\r\n\t\t\t<DatabaseID>{id}</DatabaseID>\r\n\t\t</Object>\r\n\t</ClearCache>\r\n</Batch>";
      AdomdConnection adomdConnection = connectionInfo.AdomdConnection;
      AdomdCommand adomdCommand = adomdConnection.State == (ConnectionState)1 ? adomdConnection.CreateCommand() : throw new McpException("Connection is not open. Please reconnect.");
      try
      {
        adomdCommand.CommandType = (CommandType) 1;
        adomdCommand.CommandText = str;
        int num = adomdCommand.ExecuteNonQuery();
        clearCacheResult.Success = true;
        clearCacheResult.DatabaseName = name;
        clearCacheResult.ConnectionName = connectionInfo.ConnectionName;
        clearCacheResult.RowsAffected = num;
        TransactionOperations.RecordOperation(connectionInfo, $"Cleared cache for database '{name}'");
      }
      finally
      {
        adomdCommand.Dispose();
      }
    }
    catch (Exception ex)
    {
      clearCacheResult.Success = false;
      clearCacheResult.ErrorMessage = ex.Message;
    }
    return clearCacheResult;
  }
}
