// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Tools.TraceOperationsTool
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

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
public class TraceOperationsTool
{
  private readonly ILogger<TraceOperationsTool> _logger;
  public static readonly ToolMetadata toolMetadata;

  public TraceOperationsTool(ILogger<TraceOperationsTool> logger) => this._logger = logger;

  [McpServerTool(Name = "trace_operations")]
  [Description("Perform trace operations on semantic models to capture and analyze Analysis Services events. Supported operations: Help, Start, Stop, Pause, Resume, Clear, Get, List, Fetch, ExportJSON. Use the Operation parameter to specify which operation to perform.")]
  public TraceOperationResponse ExecuteTraceOperation(
    McpServer mcpServer,
    TraceOperationRequest request)
  {
    this._logger.LogDebug("Executing {ToolName}.{Operation}: Connection={ConnectionName}", (object) nameof (TraceOperationsTool), (object) request.Operation, (object) (request.ConnectionName ?? "(last used)"));
    try
    {
      string[] strArray = new string[10]
      {
        "START",
        "STOP",
        "PAUSE",
        "RESUME",
        "CLEAR",
        "GET",
        "LIST",
        "FETCH",
        "EXPORTJSON",
        "HELP"
      };
      if (!Enumerable.Contains<string>((IEnumerable<string>) strArray, request.Operation.ToUpperInvariant()))
      {
        this._logger.LogWarning("Invalid operation '{Operation}' requested for {ToolName}. Valid operations: {ValidOperations}", (object) request.Operation, (object) nameof (TraceOperationsTool), (object) string.Join(", ", strArray));
        return new TraceOperationResponse()
        {
          Success = false,
          Message = $"Invalid operation: {request.Operation}. Supported operations: {string.Join(", ", strArray)}",
          Operation = request.Operation
        };
      }
      string str = this.ValidateRequest(request.Operation, request) ? request.Operation.ToUpperInvariant() : throw new McpException($"Invalid request for {request.Operation} operation.");
      TraceOperationResponse operationResponse;
      if (str != null)
      {
        switch (str.Length)
        {
          case 3:
            if ((str == "GET"))
            {
              operationResponse = this.HandleGetOperation(request);
              goto label_30;
            }
            break;
          case 4:
            switch (str[0])
            {
              case 'H':
                if ((str == "HELP"))
                {
                  operationResponse = this.HandleHelpOperation(request);
                  goto label_30;
                }
                break;
              case 'L':
                if ((str == "LIST"))
                {
                  operationResponse = this.HandleListOperation(request);
                  goto label_30;
                }
                break;
              case 'S':
                if ((str == "STOP"))
                {
                  operationResponse = this.HandleStopOperation(request);
                  goto label_30;
                }
                break;
            }
            break;
          case 5:
            switch (str[0])
            {
              case 'C':
                if ((str == "CLEAR"))
                {
                  operationResponse = this.HandleClearOperation(request);
                  goto label_30;
                }
                break;
              case 'F':
                if ((str == "FETCH"))
                {
                  operationResponse = this.HandleFetchOperation(request);
                  goto label_30;
                }
                break;
              case 'P':
                if ((str == "PAUSE"))
                {
                  operationResponse = this.HandlePauseOperation(request);
                  goto label_30;
                }
                break;
              case 'S':
                if ((str == "START"))
                {
                  operationResponse = this.HandleStartOperation(request);
                  goto label_30;
                }
                break;
            }
            break;
          case 6:
            if ((str == "RESUME"))
            {
              operationResponse = this.HandleResumeOperation(request);
              goto label_30;
            }
            break;
          case 10:
            if ((str == "EXPORTJSON"))
            {
              operationResponse = this.HandleExportJSONOperation(request);
              goto label_30;
            }
            break;
        }
      }
      operationResponse = new TraceOperationResponse()
      {
        Success = false,
        Message = $"Operation {request.Operation} is not implemented",
        Operation = request.Operation
      };
label_30:
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Error executing {ToolName}.{Operation}: {ErrorMessage}", (object) nameof (TraceOperationsTool), (object) request.Operation, (object) ex.Message);
      return new TraceOperationResponse()
      {
        Success = false,
        Message = "Error executing trace operation: " + ex.Message,
        Operation = request.Operation
      };
    }
  }

  private TraceOperationResponse HandleStartOperation(TraceOperationRequest request)
  {
    try
    {
      TraceStartRequest request1 = new TraceStartRequest()
      {
        Events = request.Events,
        FilterCurrentSessionOnly = request.FilterCurrentSessionOnly ?? true
      };
      TraceStartResult traceStartResult = TraceOperations.StartTrace(request.ConnectionName, request1);
      if (traceStartResult.Warnings != null && Enumerable.Any<string>((IEnumerable<string>) traceStartResult.Warnings))
      {
        foreach (string warning in traceStartResult.Warnings)
          this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (TraceOperationsTool), (object) request.Operation, (object) warning);
      }
      this._logger.LogInformation("{ToolName}.{Operation} completed: Trace={TraceName}, EventTypes={EventTypeCount}", (object) nameof (TraceOperationsTool), (object) request.Operation, (object) traceStartResult.TraceName, (object) traceStartResult.SubscribedEvents.Count);
      TraceOperationResponse operationResponse = new TraceOperationResponse { Success = true };
      List<string> warnings = traceStartResult.Warnings;
      string str;
      if ((warnings != null ? (Enumerable.Any<string>((IEnumerable<string>) warnings) ? 1 : 0) : 0) == 0)
        str = $"Trace started: {traceStartResult.TraceName}, capturing {traceStartResult.SubscribedEvents.Count} event types";
      else
        str = "Trace already active: " + traceStartResult.TraceName;
      operationResponse.Message = str;
      operationResponse.Operation = request.Operation;
      operationResponse.Data = (object) traceStartResult;
      operationResponse.Warnings = traceStartResult.Warnings;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      TraceOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new TraceOperationResponse()
      {
        Success = false,
        Message = "Error starting trace: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private TraceOperationResponse HandleStopOperation(TraceOperationRequest request)
  {
    try
    {
      TraceStopResult traceStopResult = TraceOperations.StopTrace(request.ConnectionName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Trace={TraceName}, Duration={Duration}s, Captured={EventsCaptured}, Discarded={EventsDiscarded}", (object) nameof (TraceOperationsTool), (object) request.Operation, (object) traceStopResult.TraceName, (object) traceStopResult.Duration, (object) traceStopResult.TotalEventsCaptured, (object) traceStopResult.TotalEventsDiscarded);
      TraceOperationResponse operationResponse = new TraceOperationResponse { Success = true };
      operationResponse.Message = $"Trace stopped: {traceStopResult.TraceName}, duration: {traceStopResult.Duration:F2}s, captured: {traceStopResult.TotalEventsCaptured} events, discarded: {traceStopResult.TotalEventsDiscarded} events";
      operationResponse.Operation = request.Operation;
      operationResponse.Data = (object) traceStopResult;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      TraceOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new TraceOperationResponse()
      {
        Success = false,
        Message = "Error stopping trace: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private TraceOperationResponse HandlePauseOperation(TraceOperationRequest request)
  {
    try
    {
      TracePauseResult tracePauseResult = TraceOperations.PauseTrace(request.ConnectionName);
      if (tracePauseResult.Warnings != null && Enumerable.Any<string>((IEnumerable<string>) tracePauseResult.Warnings))
      {
        foreach (string warning in tracePauseResult.Warnings)
          this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (TraceOperationsTool), (object) request.Operation, (object) warning);
      }
      this._logger.LogInformation("{ToolName}.{Operation} completed: Trace={TraceName}, EventsCaptured={EventsCaptured}", (object) nameof (TraceOperationsTool), (object) request.Operation, (object) tracePauseResult.TraceName, (object) tracePauseResult.EventsCaptured);
      TraceOperationResponse operationResponse = new TraceOperationResponse { Success = true };
      List<string> warnings = tracePauseResult.Warnings;
      string str;
      if ((warnings != null ? (Enumerable.Any<string>((IEnumerable<string>) warnings) ? 1 : 0) : 0) == 0)
        str = $"Trace paused: {tracePauseResult.TraceName}, events captured so far: {tracePauseResult.EventsCaptured}";
      else
        str = "Trace already paused: " + tracePauseResult.TraceName;
      operationResponse.Message = str;
      operationResponse.Operation = request.Operation;
      operationResponse.Data = (object) tracePauseResult;
      operationResponse.Warnings = tracePauseResult.Warnings;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      TraceOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new TraceOperationResponse()
      {
        Success = false,
        Message = "Error pausing trace: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private TraceOperationResponse HandleResumeOperation(TraceOperationRequest request)
  {
    try
    {
      TraceResumeResult traceResumeResult = TraceOperations.ResumeTrace(request.ConnectionName);
      if (traceResumeResult.Warnings != null && Enumerable.Any<string>((IEnumerable<string>) traceResumeResult.Warnings))
      {
        foreach (string warning in traceResumeResult.Warnings)
          this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (TraceOperationsTool), (object) request.Operation, (object) warning);
      }
      this._logger.LogInformation("{ToolName}.{Operation} completed: Trace={TraceName}, EventsCaptured={EventsCaptured}", (object) nameof (TraceOperationsTool), (object) request.Operation, (object) traceResumeResult.TraceName, (object) traceResumeResult.EventsCaptured);
      TraceOperationResponse operationResponse = new TraceOperationResponse { Success = true };
      List<string> warnings = traceResumeResult.Warnings;
      string str;
      if ((warnings != null ? (Enumerable.Any<string>((IEnumerable<string>) warnings) ? 1 : 0) : 0) == 0)
        str = $"Trace resumed: {traceResumeResult.TraceName}, events captured so far: {traceResumeResult.EventsCaptured}";
      else
        str = "Trace already active: " + traceResumeResult.TraceName;
      operationResponse.Message = str;
      operationResponse.Operation = request.Operation;
      operationResponse.Data = (object) traceResumeResult;
      operationResponse.Warnings = traceResumeResult.Warnings;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      TraceOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new TraceOperationResponse()
      {
        Success = false,
        Message = "Error resuming trace: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private TraceOperationResponse HandleClearOperation(TraceOperationRequest request)
  {
    try
    {
      TraceClearResult traceClearResult = TraceOperations.ClearTraceEvents(request.ConnectionName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Trace={TraceName}, EventsCleared={EventsCleared}", (object) nameof (TraceOperationsTool), (object) request.Operation, (object) traceClearResult.TraceName, (object) traceClearResult.EventsCleared);
      TraceOperationResponse operationResponse = new TraceOperationResponse { Success = true };
      operationResponse.Message = $"Cleared {traceClearResult.EventsCleared} events from trace: {traceClearResult.TraceName}";
      operationResponse.Operation = request.Operation;
      operationResponse.Data = (object) traceClearResult;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      TraceOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new TraceOperationResponse()
      {
        Success = false,
        Message = "Error clearing trace events: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private TraceOperationResponse HandleGetOperation(TraceOperationRequest request)
  {
    try
    {
      TraceGet trace = TraceOperations.GetTrace(request.ConnectionName);
      string status = trace.Status;
      string str1;
      if (!(status == "no active trace"))
      {
        if (!(status == "active"))
        {
          if ((status == "paused"))
            str1 = $"Trace paused: {trace.TraceName}, captured: {trace.EventsCaptured} events, duration: {trace.Duration:F2}s";
          else
            str1 = "Trace status: " + trace.Status;
        }
        else
          str1 = $"Trace active: {trace.TraceName}, captured: {trace.EventsCaptured} events, duration: {trace.Duration:F2}s";
      }
      else
        str1 = "No active trace on connection";
      string str2 = str1;
      if (trace.Warnings != null && Enumerable.Any<string>((IEnumerable<string>) trace.Warnings))
      {
        foreach (string warning in trace.Warnings)
          this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (TraceOperationsTool), (object) request.Operation, (object) warning);
      }
      this._logger.LogInformation("{ToolName}.{Operation} completed: Status={Status}, Trace={TraceName}, EventsCaptured={EventsCaptured}", (object) nameof (TraceOperationsTool), (object) request.Operation, (object) trace.Status, (object) trace.TraceName, (object) trace.EventsCaptured);
      return new TraceOperationResponse()
      {
        Success = true,
        Message = str2,
        Operation = request.Operation,
        Data = (object) trace,
        Warnings = trace.Warnings
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      TraceOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new TraceOperationResponse()
      {
        Success = false,
        Message = "Error getting trace details: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private TraceOperationResponse HandleListOperation(TraceOperationRequest request)
  {
    try
    {
      List<TraceGet> traceGetList = TraceOperations.ListTraces();
      this._logger.LogInformation("{ToolName}.{Operation} completed: Count={Count}", (object) nameof (TraceOperationsTool), (object) request.Operation, (object) traceGetList.Count);
      TraceOperationResponse operationResponse = new TraceOperationResponse { Success = true };
      string str;
      if (traceGetList.Count != 0)
        str = $"Found {traceGetList.Count} active trace(s)";
      else
        str = "No active traces found";
      operationResponse.Message = str;
      operationResponse.Operation = request.Operation;
      operationResponse.Data = (object) new
      {
        Traces = traceGetList,
        Count = traceGetList.Count
      };
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      TraceOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new TraceOperationResponse()
      {
        Success = false,
        Message = "Error listing traces: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private TraceOperationResponse HandleFetchOperation(TraceOperationRequest request)
  {
    try
    {
      bool valueOrDefault = request.ClearAfterFetch.GetValueOrDefault();
      TraceEventFetch traceEventFetch = TraceOperations.FetchTraceEvents(request.ConnectionName, valueOrDefault);
      List<string> stringList1 = request.Columns;
      if (stringList1 == null)
      {
        List<string> stringList2 = new List<string>();
        stringList2.Add("EventClassName");
        stringList2.Add("EventSubclassName");
        stringList2.Add("StartTime");
        stringList1 = stringList2;
      }
      List<string> columnsToInclude = stringList1;
      HashSet<string> validColumns = TraceOperationsTool.GetValidColumnNames();
      List<string> list1 = Enumerable.ToList<string>(Enumerable.Where<string>((IEnumerable<string>) columnsToInclude, (c => !Enumerable.Contains<string>((IEnumerable<string>) validColumns, c, (IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase))));
      if (Enumerable.Any<string>((IEnumerable<string>) list1))
      {
        this._logger.LogWarning("{ToolName}.{Operation} validation failed: Invalid columns: {InvalidColumns}", (object) nameof (TraceOperationsTool), (object) request.Operation, (object) string.Join(", ", (IEnumerable<string>) list1));
        return new TraceOperationResponse()
        {
          Success = false,
          Message = $"Invalid column names: {string.Join(", ", (IEnumerable<string>) list1)}. Valid columns: {string.Join(", ", (IEnumerable<string>) validColumns)}",
          Operation = request.Operation
        };
      }
      List<Dictionary<string, object>> list2 = Enumerable.ToList<Dictionary<string, object>>(Enumerable.Select<CapturedTraceEvent, Dictionary<string, object>>((IEnumerable<CapturedTraceEvent>) traceEventFetch.Events, (Func<CapturedTraceEvent, Dictionary<string, object>>) (e => TraceOperationsTool.ExtractEventColumns(e, columnsToInclude))));
      string str1;
      if (!traceEventFetch.Cleared)
        str1 = $"Fetched {traceEventFetch.EventCount} events from trace: {traceEventFetch.TraceName} (events retained)";
      else
        str1 = $"Fetched {traceEventFetch.EventCount} events from trace: {traceEventFetch.TraceName} (events cleared)";
      string str2 = str1;
      this._logger.LogInformation("{ToolName}.{Operation} completed: Trace={TraceName}, EventCount={EventCount}, Cleared={Cleared}", (object) nameof (TraceOperationsTool), (object) request.Operation, (object) traceEventFetch.TraceName, (object) traceEventFetch.EventCount, (object) traceEventFetch.Cleared);
      return new TraceOperationResponse()
      {
        Success = true,
        Message = str2,
        Operation = request.Operation,
        Data = (object) new
        {
          TraceName = traceEventFetch.TraceName,
          EventCount = traceEventFetch.EventCount,
          Cleared = traceEventFetch.Cleared,
          Columns = columnsToInclude,
          Events = list2
        }
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      TraceOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new TraceOperationResponse()
      {
        Success = false,
        Message = "Error fetching trace events: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private TraceOperationResponse HandleExportJSONOperation(TraceOperationRequest request)
  {
    try
    {
      bool valueOrDefault = request.ClearAfterFetch.GetValueOrDefault();
      TraceEventJSONExport json = TraceOperations.ExportTraceEventsToJSON(request.ConnectionName, request.FilePath, valueOrDefault);
      string str1;
      if (!json.Cleared)
        str1 = $"Exported {json.EventCount} events from trace: {json.TraceName} to {json.FilePath} (events retained)";
      else
        str1 = $"Exported {json.EventCount} events from trace: {json.TraceName} to {json.FilePath} (events cleared)";
      string str2 = str1;
      this._logger.LogInformation("{ToolName}.{Operation} completed: Trace={TraceName}, EventCount={EventCount}, FilePath={FilePath}, Cleared={Cleared}", (object) nameof (TraceOperationsTool), (object) request.Operation, (object) json.TraceName, (object) json.EventCount, (object) json.FilePath, (object) json.Cleared);
      return new TraceOperationResponse()
      {
        Success = true,
        Message = str2,
        Operation = request.Operation,
        Data = (object) json
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      TraceOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new TraceOperationResponse()
      {
        Success = false,
        Message = "Error exporting trace events to JSON: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private TraceOperationResponse HandleHelpOperation(TraceOperationRequest request)
  {
    this._logger.LogInformation("{ToolName}.{Operation} completed: Operations={OperationCount}", (object) nameof (TraceOperationsTool), (object) request.Operation, (object) TraceOperationsTool.toolMetadata.Operations.Keys.Count);
    return new TraceOperationResponse()
    {
      Success = true,
      Message = "Tool description retrieved successfully",
      Operation = "Help",
      Help = (object) new
      {
        ToolName = "trace_operations",
        Description = "Capture and analyze Analysis Services trace events for query execution monitoring, debugging, and performance analysis.",
        SupportedOperations = Enumerable.ToList<string>((IEnumerable<string>) TraceOperationsTool.toolMetadata.Operations.Keys),
        Authentication = "Uses existing connection established via connection_operations tool.",
        Capabilities = new string[5]
        {
          "Start/stop/pause/resume trace on connections",
          "Capture Analysis Services events (query, storage engine, DirectQuery, etc.)",
          "Fetch captured event summaries (event names and start times)",
          "Export full event details to JSON file",
          "List opened traces"
        },
        SupportedEvents = new
        {
          Queries = new string[2]
          {
            "QueryBegin",
            "QueryEnd"
          },
          Command = new string[2]
          {
            "CommandBegin",
            "CommandEnd"
          },
          Discover = new string[2]
          {
            "DiscoverBegin",
            "DiscoverEnd"
          },
          ErrorsAndWarnings = new string[1]{ "Error" },
          ProgressReports = new string[4]
          {
            "ProgressReportBegin",
            "ProgressReportEnd",
            "ProgressReportCurrent",
            "ProgressReportError"
          },
          QueryProcessing = new string[9]
          {
            "VertiPaqSEQueryBegin",
            "VertiPaqSEQueryEnd",
            "VertiPaqSEQueryCacheMatch",
            "DirectQueryBegin",
            "DirectQueryEnd",
            "DAXQueryPlan",
            "ResourceUsage",
            "DAXEvaluationLog",
            "AggregateTableRewriteQuery"
          },
          ExecutionMetrics = new string[1]
          {
            "ExecutionMetrics"
          },
          JobGraph = new string[1]{ "JobGraph" }
        },
        DefaultEvents = new string[11]
        {
          "CommandBegin",
          "CommandEnd",
          "QueryBegin",
          "QueryEnd",
          "VertiPaqSEQueryBegin",
          "VertiPaqSEQueryEnd",
          "VertiPaqSEQueryCacheMatch",
          "DirectQueryBegin",
          "DirectQueryEnd",
          "ExecutionMetrics",
          "Error"
        },
        ValidFetchColumns = new string[24]
        {
          "EventClassName",
          "EventSubclassName",
          "TextData",
          "DatabaseName",
          "ActivityId",
          "RequestId",
          "SessionId",
          "ApplicationName",
          "CurrentTime",
          "StartTime",
          "Duration",
          "CpuTime",
          "EndTime",
          "NTUserName",
          "RequestProperties",
          "RequestParameters",
          "ObjectName",
          "ObjectPath",
          "ObjectReference",
          "Spid",
          "IntegerData",
          "ProgressTotal",
          "ObjectId",
          "Error"
        },
        Examples = TraceOperationsTool.toolMetadata.Operations,
        Notes = new string[6]
        {
          "Only one trace can be created per connection at a time.",
          "Traces are automatically filtered on the server side to the current session.",
          "If Events parameter is not specified when starting a trace, default events will be captured: CommandBegin, CommandEnd, QueryBegin, QueryEnd, VertiPaqSEQueryBegin, VertiPaqSEQueryEnd, VertiPaqSEQueryCacheMatch, DirectQueryBegin, DirectQueryEnd, ExecutionMetrics, Error.",
          "Paused traces discard new events but can be resumed.",
          "Trace operations are not supported on offline connections.",
          "All operations are read-only and do not modify the model."
        }
      }
    };
  }

  private static HashSet<string> GetValidColumnNames()
  {
    HashSet<string> validColumnNames = new HashSet<string>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    validColumnNames.Add("EventClassName");
    validColumnNames.Add("EventSubclassName");
    validColumnNames.Add("TextData");
    validColumnNames.Add("DatabaseName");
    validColumnNames.Add("ActivityId");
    validColumnNames.Add("RequestId");
    validColumnNames.Add("SessionId");
    validColumnNames.Add("ApplicationName");
    validColumnNames.Add("CurrentTime");
    validColumnNames.Add("StartTime");
    validColumnNames.Add("Duration");
    validColumnNames.Add("CpuTime");
    validColumnNames.Add("EndTime");
    validColumnNames.Add("NTUserName");
    validColumnNames.Add("RequestProperties");
    validColumnNames.Add("RequestParameters");
    validColumnNames.Add("ObjectName");
    validColumnNames.Add("ObjectPath");
    validColumnNames.Add("ObjectReference");
    validColumnNames.Add("Spid");
    validColumnNames.Add("IntegerData");
    validColumnNames.Add("ProgressTotal");
    validColumnNames.Add("ObjectId");
    validColumnNames.Add("Error");
    return validColumnNames;
  }

  private static Dictionary<string, object?> ExtractEventColumns(
    CapturedTraceEvent evt,
    List<string> columns)
  {
    Dictionary<string, object> eventColumns = new Dictionary<string, object>();
    foreach (string column in columns)
    {
      string lowerInvariant = column.ToLowerInvariant();
      object obj1;
      if (lowerInvariant != null)
      {
        switch (lowerInvariant.Length)
        {
          case 4:
            if ((lowerInvariant == "spid"))
            {
              obj1 = (object) evt.Spid;
              goto label_60;
            }
            break;
          case 5:
            if ((lowerInvariant == "error"))
            {
              obj1 = (object) evt.Error;
              goto label_60;
            }
            break;
          case 7:
            switch (lowerInvariant[0])
            {
              case 'c':
                if ((lowerInvariant == "cputime"))
                {
                  obj1 = (object) evt.CpuTime;
                  goto label_60;
                }
                break;
              case 'e':
                if ((lowerInvariant == "endtime"))
                {
                  obj1 = (object) evt.EndTime;
                  goto label_60;
                }
                break;
            }
            break;
          case 8:
            switch (lowerInvariant[0])
            {
              case 'd':
                if ((lowerInvariant == "duration"))
                {
                  obj1 = (object) evt.Duration;
                  goto label_60;
                }
                break;
              case 'o':
                if ((lowerInvariant == "objectid"))
                {
                  obj1 = (object) evt.ObjectId;
                  goto label_60;
                }
                break;
              case 't':
                if ((lowerInvariant == "textdata"))
                {
                  obj1 = (object) evt.TextData;
                  goto label_60;
                }
                break;
            }
            break;
          case 9:
            switch (lowerInvariant[2])
            {
              case 'a':
                if ((lowerInvariant == "starttime"))
                {
                  obj1 = (object) evt.StartTime;
                  goto label_60;
                }
                break;
              case 'q':
                if ((lowerInvariant == "requestid"))
                {
                  obj1 = (object) evt.RequestId;
                  goto label_60;
                }
                break;
              case 's':
                if ((lowerInvariant == "sessionid"))
                {
                  obj1 = (object) evt.SessionId;
                  goto label_60;
                }
                break;
            }
            break;
          case 10:
            switch (lowerInvariant[0])
            {
              case 'a':
                if ((lowerInvariant == "activityid"))
                {
                  obj1 = (object) evt.ActivityId;
                  goto label_60;
                }
                break;
              case 'n':
                if ((lowerInvariant == "ntusername"))
                {
                  obj1 = (object) evt.NTUserName;
                  goto label_60;
                }
                break;
              case 'o':
                if (!(lowerInvariant == "objectname"))
                {
                  if ((lowerInvariant == "objectpath"))
                  {
                    obj1 = (object) evt.ObjectPath;
                    goto label_60;
                  }
                  break;
                }
                obj1 = (object) evt.ObjectName;
                goto label_60;
            }
            break;
          case 11:
            switch (lowerInvariant[0])
            {
              case 'c':
                if ((lowerInvariant == "currenttime"))
                {
                  obj1 = (object) evt.CurrentTime;
                  goto label_60;
                }
                break;
              case 'i':
                if ((lowerInvariant == "integerdata"))
                {
                  obj1 = (object) evt.IntegerData;
                  goto label_60;
                }
                break;
            }
            break;
          case 12:
            if ((lowerInvariant == "databasename"))
            {
              obj1 = (object) evt.DatabaseName;
              goto label_60;
            }
            break;
          case 13:
            if ((lowerInvariant == "progresstotal"))
            {
              obj1 = (object) evt.ProgressTotal;
              goto label_60;
            }
            break;
          case 14:
            if ((lowerInvariant == "eventclassname"))
            {
              obj1 = (object) evt.EventClassName;
              goto label_60;
            }
            break;
          case 15:
            switch (lowerInvariant[0])
            {
              case 'a':
                if ((lowerInvariant == "applicationname"))
                {
                  obj1 = (object) evt.ApplicationName;
                  goto label_60;
                }
                break;
              case 'o':
                if ((lowerInvariant == "objectreference"))
                {
                  obj1 = (object) evt.ObjectReference;
                  goto label_60;
                }
                break;
            }
            break;
          case 17:
            switch (lowerInvariant[8])
            {
              case 'a':
                if ((lowerInvariant == "requestparameters"))
                {
                  obj1 = (object) evt.RequestParameters;
                  goto label_60;
                }
                break;
              case 'c':
                if ((lowerInvariant == "eventsubclassname"))
                {
                  obj1 = (object) evt.EventSubclassName;
                  goto label_60;
                }
                break;
              case 'r':
                if ((lowerInvariant == "requestproperties"))
                {
                  obj1 = (object) evt.RequestProperties;
                  goto label_60;
                }
                break;
            }
            break;
        }
      }
      obj1 = (object) null;
label_60:
      object obj2 = obj1;
      eventColumns[column] = obj2;
    }
    return eventColumns;
  }

  private bool ValidateRequest(string operation, TraceOperationRequest request)
  {
    OperationMetadata operationMetadata;
    if (!TraceOperationsTool.toolMetadata.Operations.TryGetValue(operation, out operationMetadata))
      return true;
    JsonObject requestDict = JsonSerializer.SerializeToNode<TraceOperationRequest>(request) as JsonObject;
    List<string> list1 = Enumerable.ToList<string>(Enumerable.Where<string>((IEnumerable<string>) operationMetadata.RequiredParams, (p => requestDict != null && requestDict[p] == null)));
    List<string> list2 = Enumerable.ToList<string>(Enumerable.Where<string>((IEnumerable<string>) operationMetadata.ForbiddenParams, (p => requestDict != null && requestDict[p] != null)));
    if (Enumerable.Any<string>((IEnumerable<string>) list1))
      throw new McpException($"Missing required parameters needed for {operation} operation: {string.Join(", ", (IEnumerable<string>) list1)}");
    if (Enumerable.Any<string>((IEnumerable<string>) list2))
      throw new McpException($"Forbidden parameters not allowed for {operation} operation: {string.Join(", ", (IEnumerable<string>) list2)}");
    return true;
  }

  static TraceOperationsTool()
  {
    ToolMetadata toolMetadata1 = new ToolMetadata();
    ToolMetadata toolMetadata2 = toolMetadata1;
    Dictionary<string, OperationMetadata> dictionary1 = new Dictionary<string, OperationMetadata>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    Dictionary<string, OperationMetadata> dictionary2 = dictionary1;
    OperationMetadata operationMetadata1 = new OperationMetadata { Description = "Start a trace on the specified connection.\r\nOptional properties: ConnectionName, Events, FilterCurrentSessionOnly.\r\nFilterCurrentSessionOnly (default: true):\r\n- true: Filter events to only the current session\r\n- false: Capture all events from all sessions\r\nIf Events is not specified, default events will be captured: CommandBegin, CommandEnd, QueryBegin, QueryEnd, VertiPaqSEQueryBegin, VertiPaqSEQueryEnd, VertiPaqSEQueryCacheMatch, DirectQueryBegin, DirectQueryEnd, ExecutionMetrics, Error.\r\nOnly one trace can be active per connection.\r\nCannot start trace on offline connections - requires active server connection.\r\nOptional: ConnectionName, Events, FilterCurrentSessionOnly." };
    operationMetadata1.OptionalParams = new string[3]
    {
      "ConnectionName",
      "Events",
      "FilterCurrentSessionOnly"
    };
    operationMetadata1.Tips = new string[8]
    {
      "If Events is not specified, default events will be captured",
      "Default events: CommandBegin, CommandEnd, QueryBegin, QueryEnd, VertiPaqSEQueryBegin, VertiPaqSEQueryEnd, VertiPaqSEQueryCacheMatch, DirectQueryBegin, DirectQueryEnd, ExecutionMetrics, Error",
      "Only one trace can be active per connection",
      "Cannot start trace on offline connections - requires active server connection",
      "FilterCurrentSessionOnly=true is recommended for most scenarios to reduce noise and improve performance",
      "FilterCurrentSessionOnly=false should be used when you need to monitor all server activity",
      "Session filtering uses SessionID, ApplicationName, SPID, and always captures ExecutionMetrics events",
      "No need to call this operation explicitly to collect DAX query execution metrics for performance analysis. Call Execute operation on the dax_query_operations tool and set GetExecutionMetrics parameter to true. That operation will implicitly start trace if necessary."
    };
    OperationMetadata operationMetadata2 = operationMetadata1;
    List<string> stringList1 = new List<string>();
    stringList1.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Start\",\r\n        \"ConnectionName\": \"MyConnection\"\r\n    }\r\n}");
    stringList1.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Start\",\r\n        \"ConnectionName\": \"MyConnection\",\r\n        \"Events\": [\"QueryBegin\", \"QueryEnd\", \"VertiPaqSEQueryEnd\"]\r\n    }\r\n}");
    operationMetadata2.ExampleRequests = stringList1;
    OperationMetadata operationMetadata3 = operationMetadata1;
    dictionary2["Start"] = operationMetadata3;
    Dictionary<string, OperationMetadata> dictionary3 = dictionary1;
    OperationMetadata operationMetadata4 = new OperationMetadata { Description = "Stop active trace on a connection.\r\nReturns summary of captured events.\r\nAll captured events are discarded.\r\nOptional: ConnectionName." };
    operationMetadata4.OptionalParams = new string[1]
    {
      "ConnectionName"
    };
    OperationMetadata operationMetadata5 = operationMetadata4;
    List<string> stringList2 = new List<string>();
    stringList2.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Stop\",\r\n        \"ConnectionName\": \"MyConnection\"\r\n    }\r\n}");
    operationMetadata5.ExampleRequests = stringList2;
    OperationMetadata operationMetadata6 = operationMetadata4;
    dictionary3["Stop"] = operationMetadata6;
    Dictionary<string, OperationMetadata> dictionary4 = dictionary1;
    OperationMetadata operationMetadata7 = new OperationMetadata { Description = "Pause event capture on active trace (events are discarded while paused).\r\nTrace continues running but events are not captured.\r\nOptional: ConnectionName." };
    operationMetadata7.OptionalParams = new string[1]
    {
      "ConnectionName"
    };
    OperationMetadata operationMetadata8 = operationMetadata7;
    List<string> stringList3 = new List<string>();
    stringList3.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Pause\",\r\n        \"ConnectionName\": \"MyConnection\"\r\n    }\r\n}");
    operationMetadata8.ExampleRequests = stringList3;
    OperationMetadata operationMetadata9 = operationMetadata7;
    dictionary4["Pause"] = operationMetadata9;
    Dictionary<string, OperationMetadata> dictionary5 = dictionary1;
    OperationMetadata operationMetadata10 = new OperationMetadata { Description = "Resume event capture on paused trace.\r\nEvents will be captured again after resuming.\r\nOptional: ConnectionName." };
    operationMetadata10.OptionalParams = new string[1]
    {
      "ConnectionName"
    };
    OperationMetadata operationMetadata11 = operationMetadata10;
    List<string> stringList4 = new List<string>();
    stringList4.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Resume\",\r\n        \"ConnectionName\": \"MyConnection\"\r\n    }\r\n}");
    operationMetadata11.ExampleRequests = stringList4;
    OperationMetadata operationMetadata12 = operationMetadata10;
    dictionary5["Resume"] = operationMetadata12;
    Dictionary<string, OperationMetadata> dictionary6 = dictionary1;
    OperationMetadata operationMetadata13 = new OperationMetadata { Description = "Clear captured events without stopping trace.\r\nTrace continues running and capturing new events.\r\nOptional: ConnectionName." };
    operationMetadata13.OptionalParams = new string[1]
    {
      "ConnectionName"
    };
    OperationMetadata operationMetadata14 = operationMetadata13;
    List<string> stringList5 = new List<string>();
    stringList5.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Clear\",\r\n        \"ConnectionName\": \"MyConnection\"\r\n    }\r\n}");
    operationMetadata14.ExampleRequests = stringList5;
    OperationMetadata operationMetadata15 = operationMetadata13;
    dictionary6["Clear"] = operationMetadata15;
    Dictionary<string, OperationMetadata> dictionary7 = dictionary1;
    OperationMetadata operationMetadata16 = new OperationMetadata { Description = "Get trace details for a connection.\r\nReturns trace name, status, duration, and event counts.\r\nDoes not include captured events in the list for performance.\r\nUse Fetch operation to retrieve captured events.\r\nOptional: ConnectionName." };
    operationMetadata16.OptionalParams = new string[1]
    {
      "ConnectionName"
    };
    OperationMetadata operationMetadata17 = operationMetadata16;
    List<string> stringList6 = new List<string>();
    stringList6.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Get\",\r\n        \"ConnectionName\": \"MyConnection\"\r\n    }\r\n}");
    operationMetadata17.ExampleRequests = stringList6;
    OperationMetadata operationMetadata18 = operationMetadata16;
    dictionary7["Get"] = operationMetadata18;
    Dictionary<string, OperationMetadata> dictionary8 = dictionary1;
    OperationMetadata operationMetadata19 = new OperationMetadata { Description = "List all traces across all connections.\r\nReturns trace information for all connections with traces.\r\nDoes not include captured events in the list for performance.\r\nUse Fetch operation to retrieve captured events." };
    operationMetadata19.Tips = new string[3]
    {
      "Returns trace information for all connections with traces",
      "Does not include captured events in the list for performance",
      "Use Get operation to retrieve full trace details including events"
    };
    OperationMetadata operationMetadata20 = operationMetadata19;
    List<string> stringList7 = new List<string>();
    stringList7.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"List\"\r\n    }\r\n}");
    operationMetadata20.ExampleRequests = stringList7;
    OperationMetadata operationMetadata21 = operationMetadata19;
    dictionary8["List"] = operationMetadata21;
    Dictionary<string, OperationMetadata> dictionary9 = dictionary1;
    OperationMetadata operationMetadata22 = new OperationMetadata { Description = "Fetch captured trace events with specified columns.\r\nBy default, includes only EventClassName, EventSubclassName, and StartTime.\r\nUse Columns parameter to specify which columns to include.\r\nBy default, retains events after fetching.\r\nSet ClearAfterFetch=true to clear events after fetching.\r\nOptional: ConnectionName, ClearAfterFetch, Columns." };
    operationMetadata22.OptionalParams = new string[3]
    {
      "ConnectionName",
      "ClearAfterFetch",
      "Columns"
    };
    operationMetadata22.Tips = new string[3]
    {
      "Default columns: EventClassName, EventSubclassName, StartTime",
      "Available columns: EventClassName, EventSubclassName, TextData, DatabaseName, ActivityId, RequestId, SessionId, ApplicationName, CurrentTime, StartTime, Duration, CpuTime, EndTime, NTUserName, RequestProperties, RequestParameters, ObjectName, ObjectPath, ObjectReference, Spid, IntegerData, ProgressTotal, ObjectId, Error",
      "Specify Columns as an array to customize the report output"
    };
    OperationMetadata operationMetadata23 = operationMetadata22;
    List<string> stringList8 = new List<string>();
    stringList8.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Fetch\",\r\n        \"ConnectionName\": \"MyConnection\"\r\n    }\r\n}");
    stringList8.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Fetch\",\r\n        \"ConnectionName\": \"MyConnection\",\r\n        \"ClearAfterFetch\": true\r\n    }\r\n}");
    stringList8.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Fetch\",\r\n        \"ConnectionName\": \"MyConnection\",\r\n        \"Columns\": [\"EventClassName\", \"Duration\", \"CpuTime\", \"TextData\"]\r\n    }\r\n}");
    operationMetadata23.ExampleRequests = stringList8;
    OperationMetadata operationMetadata24 = operationMetadata22;
    dictionary9["Fetch"] = operationMetadata24;
    Dictionary<string, OperationMetadata> dictionary10 = dictionary1;
    OperationMetadata operationMetadata25 = new OperationMetadata { Description = "Export captured trace events to a JSON file.\r\nBy default, retains events after export (ClearAfterFetch defaults to false).\r\nSet ClearAfterFetch=true to clear events after export.\r\nRequired: FilePath.\r\nOptional: ConnectionName, ClearAfterFetch." };
    operationMetadata25.RequiredParams = new string[1]
    {
      "FilePath"
    };
    operationMetadata25.OptionalParams = new string[2]
    {
      "ConnectionName",
      "ClearAfterFetch"
    };
    OperationMetadata operationMetadata26 = operationMetadata25;
    List<string> stringList9 = new List<string>();
    stringList9.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"ExportJSON\",\r\n        \"ConnectionName\": \"MyConnection\",\r\n        \"FilePath\": \"C:\\\\traces\\\\trace_events.json\"\r\n    }\r\n}");
    stringList9.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"ExportJSON\",\r\n        \"ConnectionName\": \"MyConnection\",\r\n        \"FilePath\": \"./trace_output.json\",\r\n        \"ClearAfterFetch\": true\r\n    }\r\n}");
    operationMetadata26.ExampleRequests = stringList9;
    OperationMetadata operationMetadata27 = operationMetadata25;
    dictionary10["ExportJSON"] = operationMetadata27;
    Dictionary<string, OperationMetadata> dictionary11 = dictionary1;
    OperationMetadata operationMetadata28 = new OperationMetadata { Description = "Describe the trace operations tool and its operations.\r\nProvides detailed information about all available operations." };
    List<string> stringList10 = new List<string>();
    stringList10.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Help\"\r\n    }\r\n}");
    operationMetadata28.ExampleRequests = stringList10;
    dictionary11["Help"] = operationMetadata28;
    Dictionary<string, OperationMetadata> dictionary12 = dictionary1;
    toolMetadata2.Operations = dictionary12;
    TraceOperationsTool.toolMetadata = toolMetadata1;
  }
}
