// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using Microsoft.Extensions.Logging;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using PowerBIModelingMCP.Library.Common;
using PowerBIModelingMCP.Library.Common.DataStructures;
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
public class DaxQueryOperationsTool
{
  private readonly ILogger<DaxQueryOperationsTool> _logger;
  public static readonly ToolMetadata toolMetadata;

  public DaxQueryOperationsTool(ILogger<DaxQueryOperationsTool> logger) => this._logger = logger;

  [McpServerTool(Name = "dax_query_operations")]
  [Description("Perform DAX query operations on semantic models. Supported operations: Help, Execute, Validate, ClearCache. Use the Operation parameter to specify which operation to perform.")]
  public DaxQueryOperationResponse ExecuteDaxQueryOperation(
    McpServer mcpServer,
    DaxQueryOperationRequest request)
  {
    ILogger<DaxQueryOperationsTool> logger = this._logger;
    object[] objArray = new object[4]
    {
      (object) nameof (DaxQueryOperationsTool),
      (object) request.Operation,
      null,
      null
    };
    string query = request.Query;
    objArray[2] = (object) (query != null ? query.Length : 0);
    objArray[3] = (object) (request.ConnectionName ?? "(last used)");
    logger.LogDebug("Executing {ToolName}.{Operation}: QueryLength={QueryLength}, Connection={ConnectionName}", objArray);
    try
    {
      string[] strArray = new string[4]
      {
        "EXECUTE",
        "VALIDATE",
        "HELP",
        "CLEARCACHE"
      };
      if (!Enumerable.Contains<string>((IEnumerable<string>) strArray, request.Operation.ToUpperInvariant()))
      {
        this._logger.LogWarning("Invalid operation '{Operation}' requested for {ToolName}. Valid operations: {ValidOperations}", (object) request.Operation, (object) nameof (DaxQueryOperationsTool), (object) string.Join(", ", strArray));
        return new DaxQueryOperationResponse()
        {
          Success = false,
          Message = $"Invalid operation: {request.Operation}. Supported operations: {string.Join(", ", strArray)}",
          Operation = request.Operation
        };
      }
      if (!this.ValidateRequest(request.Operation, request))
        throw new McpException($"Invalid request for {request.Operation} operation.");
      if ((request.Operation.ToUpperInvariant() == "EXECUTE") && !ConfirmationService.ConfirmRequest(mcpServer, request.ConnectionName, ConfirmationType.DaxOperation))
        return new DaxQueryOperationResponse()
        {
          Success = false,
          Message = "The user requested a dax operation but declined when asked to confirm. Do not retry or initiate any dax operations on your own. Wait until the user explicitly confirms or requests a dax operation again.",
          Operation = request.Operation
        };
      string upperInvariant = request.Operation.ToUpperInvariant();
      DaxQueryOperationResponse operationResponse;
      if (!(upperInvariant == "EXECUTE"))
      {
        if (!(upperInvariant == "VALIDATE"))
        {
          if (!(upperInvariant == "HELP"))
          {
            if ((upperInvariant == "CLEARCACHE"))
              operationResponse = this.HandleClearCacheOperation(request);
            else
              operationResponse = new DaxQueryOperationResponse()
              {
                Success = false,
                Message = $"Operation {request.Operation} is not implemented",
                Operation = request.Operation
              };
          }
          else
            operationResponse = this.HandleHelpOperation(request);
        }
        else
          operationResponse = this.HandleValidateOperation(request);
      }
      else
        operationResponse = this.HandleExecuteOperation(request);
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Error executing {ToolName}.{Operation}: {ErrorMessage}", (object) nameof (DaxQueryOperationsTool), (object) request.Operation, (object) ex.Message);
      return new DaxQueryOperationResponse()
      {
        Success = false,
        Message = "Error executing DAX query operation: " + ex.Message,
        Operation = request.Operation
      };
    }
  }

  private DaxQueryOperationResponse HandleExecuteOperation(DaxQueryOperationRequest request)
  {
    bool flag1 = false;
    bool flag2 = false;
    try
    {
      if (request.GetExecutionMetrics)
      {
        List<string> stringList1 = new List<string>();
        stringList1.Add("QueryBegin");
        stringList1.Add("QueryEnd");
        stringList1.Add("VertiPaqSEQueryBegin");
        stringList1.Add("VertiPaqSEQueryEnd");
        stringList1.Add("VertiPaqSEQueryCacheMatch");
        stringList1.Add("DirectQueryBegin");
        stringList1.Add("DirectQueryEnd");
        stringList1.Add("ExecutionMetrics");
        stringList1.Add("Error");
        List<string> stringList2 = stringList1;
        TraceGet trace = TraceOperations.GetTrace(request.ConnectionName);
        if ((trace.Status == "no active trace"))
        {
          TraceStartRequest request1 = new TraceStartRequest()
          {
            Events = stringList2
          };
          TraceOperations.StartTrace(request.ConnectionName, request1);
          flag1 = true;
        }
        else if ((trace.Status == "paused"))
        {
          TraceOperations.ResumeTrace(request.ConnectionName);
          flag2 = true;
        }
        else if ((trace.Status == "active"))
        {
          List<string> list = Enumerable.ToList<string>(Enumerable.Except<string>((IEnumerable<string>) stringList2, (IEnumerable<string>) (trace.SubscribedEvents ?? new List<string>()), (IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase));
          if (Enumerable.Any<string>((IEnumerable<string>) list))
            return new DaxQueryOperationResponse()
            {
              Success = false,
              Message = $"Active trace is missing required events for query metrics: {string.Join(", ", (IEnumerable<string>) list)}. Please stop the current trace and let the operation start a new one, or ensure all required events are subscribed.",
              Operation = request.Operation
            };
        }
        TraceOperations.ClearTraceEvents(request.ConnectionName);
      }
      DaxQueryExecute queryDef = new DaxQueryExecute()
      {
        Query = request.Query,
        TimeoutSeconds = request.TimeoutSeconds,
        MaxRows = request.MaxRows,
        ReturnRows = !request.GetExecutionMetrics || !request.ExecutionMetricsOnly
      };
      DaxQueryResult daxQueryResult = DaxQueryOperations.ExecuteDaxQuery(request.ConnectionName, queryDef);
      DaxQueryOperationResponse operationResponse1 = new DaxQueryOperationResponse { Success = daxQueryResult.Success };
      string str;
      if (!daxQueryResult.Success)
        str = "DAX query execution failed: " + daxQueryResult.ErrorMessage;
      else
        str = $"DAX query executed successfully, returned {daxQueryResult.RowCount} rows in {daxQueryResult.ExecutionTimeMs}ms";
      operationResponse1.Message = str;
      operationResponse1.Operation = request.Operation;
      DaxQueryOperationResponse operationResponse2 = operationResponse1;
      operationResponse2.Data = (object) daxQueryResult;
      if (daxQueryResult.Success)
        this._logger.LogInformation("{ToolName}.{Operation} completed: RowCount={RowCount}, Duration={Duration}ms", (object) nameof (DaxQueryOperationsTool), (object) request.Operation, (object) daxQueryResult.RowCount, (object) daxQueryResult.ExecutionTimeMs);
      else
        this._logger.LogWarning("{ToolName}.{Operation} completed: Status=Failed, Error={ErrorMessage}", (object) nameof (DaxQueryOperationsTool), (object) request.Operation, (object) daxQueryResult.ErrorMessage);
      if (request.GetExecutionMetrics)
      {
        if (daxQueryResult.Success)
        {
          try
          {
            (bool Success, string ErrorMessage) tuple = ExecutionMetricsHelper.WaitForQueryMetricsEvents(request.ConnectionName);
            if (!tuple.Success)
            {
              DaxQueryOperationResponse operationResponse3 = operationResponse2;
              List<string> stringList = new List<string>();
              stringList.Add("Query executed successfully but failed to collect complete metrics: " + tuple.ErrorMessage);
              operationResponse3.Warnings = stringList;
            }
            else
            {
              List<CapturedTraceEvent> capturedEvents = TraceOperations.GetCapturedEvents(request.ConnectionName);
              CalculatedExecutionMetrics queryMetrics = ExecutionMetricsHelper.ExtractQueryMetrics(capturedEvents);
              ReportedExecutionMetrics serverReportedMetrics = ExecutionMetricsHelper.ExtractServerReportedMetrics(capturedEvents);
              operationResponse2.ExecutionMetrics = new QueryExecutionMetrics()
              {
                CalculatedExecutionMetrics = queryMetrics,
                ReportedExecutionMetrics = serverReportedMetrics
              };
              List<string> stringList = new List<string>();
              if (!queryMetrics.Success)
                stringList.Add("Calculated metrics extraction had issues: " + queryMetrics.ErrorMessage);
              if (!serverReportedMetrics.Success)
                stringList.Add("Server-reported metrics extraction had issues: " + serverReportedMetrics.ErrorMessage);
              if (Enumerable.Any<string>((IEnumerable<string>) stringList))
                operationResponse2.Warnings = stringList;
            }
          }
          catch (Exception ex)
          {
            DaxQueryOperationResponse operationResponse4 = operationResponse2;
            List<string> stringList = new List<string>();
            stringList.Add("Query executed successfully but failed to collect metrics: " + ex.Message);
            operationResponse4.Warnings = stringList;
          }
        }
      }
      return operationResponse2;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      DaxQueryOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new DaxQueryOperationResponse()
      {
        Success = false,
        Message = "Error executing DAX query: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
    finally
    {
      if (flag1 | flag2)
      {
        try
        {
          TraceOperations.PauseTrace(request.ConnectionName);
        }
        catch
        {
        }
      }
    }
  }

  private DaxQueryOperationResponse HandleValidateOperation(DaxQueryOperationRequest request)
  {
    try
    {
      DaxQueryValidate queryDef = new DaxQueryValidate()
      {
        Query = request.Query,
        TimeoutSeconds = request.TimeoutSeconds
      };
      DaxValidationResult validationResult = DaxQueryOperations.ValidateDaxQuery(request.ConnectionName, queryDef);
      if (validationResult.IsValid)
        this._logger.LogInformation("{ToolName}.{Operation} completed: ColumnCount={ColumnCount}, Duration={Duration}ms", (object) nameof (DaxQueryOperationsTool), (object) request.Operation, (object) validationResult.ExpectedColumns.Count, (object) validationResult.ValidationTimeMs);
      else
        this._logger.LogWarning("{ToolName}.{Operation} completed: Status=Invalid, Error={ErrorMessage}", (object) nameof (DaxQueryOperationsTool), (object) request.Operation, (object) validationResult.ErrorMessage);
      DaxQueryOperationResponse operationResponse = new DaxQueryOperationResponse { Success = validationResult.IsValid };
      string str;
      if (!validationResult.IsValid)
        str = "DAX query validation failed: " + validationResult.ErrorMessage;
      else
        str = $"DAX query validated successfully with {validationResult.ExpectedColumns.Count} columns in {validationResult.ValidationTimeMs}ms";
      operationResponse.Message = str;
      operationResponse.Operation = request.Operation;
      operationResponse.Data = (object) validationResult;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      DaxQueryOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new DaxQueryOperationResponse()
      {
        Success = false,
        Message = "Error validating DAX query: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private DaxQueryOperationResponse HandleHelpOperation(DaxQueryOperationRequest request)
  {
    this._logger.LogInformation("{ToolName}.{Operation} completed: Operations={OperationCount}", (object) nameof (DaxQueryOperationsTool), (object) request.Operation, (object) DaxQueryOperationsTool.toolMetadata.Operations.Keys.Count);
    return new DaxQueryOperationResponse()
    {
      Success = true,
      Message = "Tool description retrieved successfully",
      Operation = "Help",
      Help = (object) new
      {
        ToolName = "dax_query_operations",
        Description = "Execute and validate DAX queries against semantic models.",
        SupportedOperations = Enumerable.ToList<string>((IEnumerable<string>) DaxQueryOperationsTool.toolMetadata.Operations.Keys),
        Authentication = "Uses existing connection established via connection_operations tool.",
        Capabilities = new string[4]
        {
          "Execute DAX queries with configurable timeout and row limits",
          "Validate DAX query syntax without execution",
          "Capture detailed query execution metrics (storage engine, DirectQuery, cache metrics)",
          "Clear database cache to force data refresh from source"
        },
        Examples = DaxQueryOperationsTool.toolMetadata.Operations,
        Notes = new string[5]
        {
          "Query parameter is required for Execute and Validate operations.",
          "TimeoutSeconds is optional (defaults: 200s for execute, 10s for validate).",
          "MaxRows is optional for Execute operation to limit result size.",
          "GetExecutionMetrics is optional for Execute operation to capture execution metrics (requires trace support).",
          "ClearCache operation clears the cache for the entire database."
        }
      }
    };
  }

  private DaxQueryOperationResponse HandleClearCacheOperation(DaxQueryOperationRequest request)
  {
    try
    {
      ClearCacheResult clearCacheResult = DaxQueryOperations.ClearCache(request.ConnectionName);
      if (clearCacheResult.Success)
        this._logger.LogInformation("{ToolName}.{Operation} completed: Database={DatabaseName}", (object) nameof (DaxQueryOperationsTool), (object) request.Operation, (object) clearCacheResult.DatabaseName);
      else
        this._logger.LogWarning("{ToolName}.{Operation} completed: Status=Failed, Error={ErrorMessage}", (object) nameof (DaxQueryOperationsTool), (object) request.Operation, (object) clearCacheResult.ErrorMessage);
      return new DaxQueryOperationResponse()
      {
        Success = clearCacheResult.Success,
        Message = clearCacheResult.Success ? $"Cache cleared successfully for database '{clearCacheResult.DatabaseName}'" : "Failed to clear cache: " + clearCacheResult.ErrorMessage,
        Operation = request.Operation,
        Data = (object) clearCacheResult
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      DaxQueryOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new DaxQueryOperationResponse()
      {
        Success = false,
        Message = "Error clearing cache: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private bool ValidateRequest(string operation, DaxQueryOperationRequest request)
  {
    OperationMetadata operationMetadata;
    if (!DaxQueryOperationsTool.toolMetadata.Operations.TryGetValue(operation, out operationMetadata))
      return true;
    JsonObject requestDict = JsonSerializer.SerializeToNode<DaxQueryOperationRequest>(request) as JsonObject;
    List<string> list1 = Enumerable.ToList<string>(Enumerable.Where<string>((IEnumerable<string>) operationMetadata.RequiredParams, (p => requestDict != null && requestDict[p] == null)));
    List<string> list2 = Enumerable.ToList<string>(Enumerable.Where<string>((IEnumerable<string>) operationMetadata.ForbiddenParams, (p => requestDict != null && requestDict[p] != null)));
    if (Enumerable.Any<string>((IEnumerable<string>) list1))
      throw new McpException($"Missing required parameters needed for {operation} operation: {string.Join(", ", (IEnumerable<string>) list1)}");
    if (Enumerable.Any<string>((IEnumerable<string>) list2))
      throw new McpException($"Forbidden parameters not allowed for {operation} operation: {string.Join(", ", (IEnumerable<string>) list2)}");
    return true;
  }

  static DaxQueryOperationsTool()
  {
    ToolMetadata toolMetadata1 = new ToolMetadata();
    ToolMetadata toolMetadata2 = toolMetadata1;
    Dictionary<string, OperationMetadata> dictionary1 = new Dictionary<string, OperationMetadata>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    Dictionary<string, OperationMetadata> dictionary2 = dictionary1;
    OperationMetadata operationMetadata1 = new OperationMetadata { RequiredParams = new string[1]
    {
      "Query"
    } };
    operationMetadata1.Description = "Execute a DAX query against a semantic model and return the results. \r\nOptionally capture detailed execution metrics including storage engine and DirectQuery metrics.\r\nMandatory properties: Query. \r\nOptional: ConnectionName, TimeoutSeconds, MaxRows, GetExecutionMetrics, ExecutionMetricsOnly.";
    operationMetadata1.Tips = new string[3]
    {
      "When GetExecutionMetrics is true, a trace will be automatically started if not already active and paused after metrics collection",
      "If a trace is already active, it must have the required events subscribed for metrics collection",
      "ExecutionMetricsOnly only takes effect when GetExecutionMetrics is true. When true, row data is not returned but all rows are read for accurate execution time"
    };
    OperationMetadata operationMetadata2 = operationMetadata1;
    List<string> stringList1 = new List<string>();
    stringList1.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Execute\",\r\n        \"ConnectionName\": \"MyConnection\",\r\n        \"Query\": \"EVALUATE Sales\",\r\n        \"GetExecutionMetrics\": false\r\n    }\r\n}");
    stringList1.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Execute\",\r\n        \"ConnectionName\": \"MyConnection\",\r\n        \"Query\": \"EVALUATE Sales\",\r\n        \"GetExecutionMetrics\": true\r\n    }\r\n}");
    operationMetadata2.ExampleRequests = stringList1;
    OperationMetadata operationMetadata3 = operationMetadata1;
    dictionary2["Execute"] = operationMetadata3;
    Dictionary<string, OperationMetadata> dictionary3 = dictionary1;
    OperationMetadata operationMetadata4 = new OperationMetadata { RequiredParams = new string[1]
    {
      "Query"
    } };
    operationMetadata4.Description = "Validate a DAX query for syntax and semantic correctness without executing it. \r\nMandatory properties: Query. \r\nOptional: ConnectionName, TimeoutSeconds.";
    OperationMetadata operationMetadata5 = operationMetadata4;
    List<string> stringList2 = new List<string>();
    stringList2.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Validate\",\r\n        \"ConnectionName\": \"MyConnection\",\r\n        \"Query\": \"EVALUATE Sales\"\r\n    }\r\n}");
    operationMetadata5.ExampleRequests = stringList2;
    OperationMetadata operationMetadata6 = operationMetadata4;
    dictionary3["Validate"] = operationMetadata6;
    Dictionary<string, OperationMetadata> dictionary4 = dictionary1;
    OperationMetadata operationMetadata7 = new OperationMetadata { Description = "Describe the DAX query operations tool and its available operations. \r\nMandatory properties: None. \r\nOptional: ConnectionName." };
    List<string> stringList3 = new List<string>();
    stringList3.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Help\"\r\n    }\r\n}");
    operationMetadata7.ExampleRequests = stringList3;
    dictionary4["Help"] = operationMetadata7;
    Dictionary<string, OperationMetadata> dictionary5 = dictionary1;
    OperationMetadata operationMetadata8 = new OperationMetadata { Description = "Clears the cache for the specified database connection. This forces the semantic model to reload data from source.\r\nMandatory properties: None.\r\nOptional: ConnectionName (uses last used connection if not provided)." };
    List<string> stringList4 = new List<string>();
    stringList4.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"ClearCache\",\r\n        \"ConnectionName\": \"MyConnection\"\r\n    }\r\n}");
    stringList4.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"ClearCache\"\r\n    }\r\n}");
    operationMetadata8.ExampleRequests = stringList4;
    dictionary5["ClearCache"] = operationMetadata8;
    Dictionary<string, OperationMetadata> dictionary6 = dictionary1;
    toolMetadata2.Operations = dictionary6;
    DaxQueryOperationsTool.toolMetadata = toolMetadata1;
  }
}
