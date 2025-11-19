// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using Microsoft.AnalysisServices;
using ModelContextProtocol;
using PowerBIModelingMCP.Library.Common;
using PowerBIModelingMCP.Library.Common.DataStructures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Xml;

#nullable enable
namespace PowerBIModelingMCP.Library.Core;

public static class TraceOperations
{
  private static readonly List<string> DefaultEvents;
  private static readonly List<string> QueriesEvents;
  private static readonly List<string> CommandEvents;
  private static readonly List<string> DiscoverEvents;
  private static readonly List<string> ErrorsAndWarnings;
  private static readonly List<string> ProgressReports;
  private static readonly List<string> QueryProcessing;
  private static readonly List<string> ExecutionMetricsEvents;
  private static readonly List<string> JobGraphEvents;
  private static readonly HashSet<string> AllowedEvents;
  private static readonly Dictionary<string, TraceEventDefinition> EventDefinitions;

  static TraceOperations()
  {
    List<string> stringList1 = new List<string>();
    stringList1.Add("CommandBegin");
    stringList1.Add("CommandEnd");
    stringList1.Add("QueryBegin");
    stringList1.Add("QueryEnd");
    stringList1.Add("VertiPaqSEQueryBegin");
    stringList1.Add("VertiPaqSEQueryEnd");
    stringList1.Add("VertiPaqSEQueryCacheMatch");
    stringList1.Add("DirectQueryBegin");
    stringList1.Add("DirectQueryEnd");
    stringList1.Add("ExecutionMetrics");
    stringList1.Add("Error");
    TraceOperations.DefaultEvents = stringList1;
    List<string> stringList2 = new List<string>();
    stringList2.Add("QueryBegin");
    stringList2.Add("QueryEnd");
    TraceOperations.QueriesEvents = stringList2;
    List<string> stringList3 = new List<string>();
    stringList3.Add("CommandBegin");
    stringList3.Add("CommandEnd");
    TraceOperations.CommandEvents = stringList3;
    List<string> stringList4 = new List<string>();
    stringList4.Add("DiscoverBegin");
    stringList4.Add("DiscoverEnd");
    TraceOperations.DiscoverEvents = stringList4;
    List<string> stringList5 = new List<string>();
    stringList5.Add("Error");
    TraceOperations.ErrorsAndWarnings = stringList5;
    List<string> stringList6 = new List<string>();
    stringList6.Add("ProgressReportBegin");
    stringList6.Add("ProgressReportEnd");
    stringList6.Add("ProgressReportCurrent");
    stringList6.Add("ProgressReportError");
    TraceOperations.ProgressReports = stringList6;
    List<string> stringList7 = new List<string>();
    stringList7.Add("VertiPaqSEQueryBegin");
    stringList7.Add("VertiPaqSEQueryEnd");
    stringList7.Add("VertiPaqSEQueryCacheMatch");
    stringList7.Add("DirectQueryBegin");
    stringList7.Add("DirectQueryEnd");
    stringList7.Add("DAXQueryPlan");
    stringList7.Add("ResourceUsage");
    stringList7.Add("DAXEvaluationLog");
    stringList7.Add("AggregateTableRewriteQuery");
    TraceOperations.QueryProcessing = stringList7;
    List<string> stringList8 = new List<string>();
    stringList8.Add("ExecutionMetrics");
    TraceOperations.ExecutionMetricsEvents = stringList8;
    List<string> stringList9 = new List<string>();
    stringList9.Add("JobGraph");
    TraceOperations.JobGraphEvents = stringList9;
    TraceOperations.AllowedEvents = new HashSet<string>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    TraceOperations.EventDefinitions = new Dictionary<string, TraceEventDefinition>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    TraceOperations.AllowedEvents.UnionWith((IEnumerable<string>) TraceOperations.QueriesEvents);
    TraceOperations.AllowedEvents.UnionWith((IEnumerable<string>) TraceOperations.CommandEvents);
    TraceOperations.AllowedEvents.UnionWith((IEnumerable<string>) TraceOperations.DiscoverEvents);
    TraceOperations.AllowedEvents.UnionWith((IEnumerable<string>) TraceOperations.ErrorsAndWarnings);
    TraceOperations.AllowedEvents.UnionWith((IEnumerable<string>) TraceOperations.ProgressReports);
    TraceOperations.AllowedEvents.UnionWith((IEnumerable<string>) TraceOperations.QueryProcessing);
    TraceOperations.AllowedEvents.UnionWith((IEnumerable<string>) TraceOperations.ExecutionMetricsEvents);
    TraceOperations.AllowedEvents.UnionWith((IEnumerable<string>) TraceOperations.JobGraphEvents);
    TraceOperations.InitializeEventDefinitions();
  }

  public static TraceStartResult StartTrace(string? connectionName, TraceStartRequest request)
  {
    PowerBIModelingMCP.Library.Common.DataStructures.ConnectionInfo connectionInfo = ConnectionOperations.Get(connectionName);
    ConnectionValidator.ValidateForTrace(connectionInfo);
    if (connectionInfo.Trace != null)
    {
      TraceContext trace = connectionInfo.Trace;
      TraceStartResult traceStartResult = new TraceStartResult { TraceName = trace.TraceName };
      traceStartResult.Status = trace.IsPaused ? "paused" : "active";
      traceStartResult.StartTime = trace.StartTime;
      traceStartResult.SubscribedEvents = trace.SubscribedEvents;
      List<string> stringList = new List<string>();
      stringList.Add("Trace is already active on this connection. Stop the existing trace first to start a new one.");
      traceStartResult.Warnings = stringList;
      return traceStartResult;
    }
    List<string> eventsToSubscribe = TraceOperations.DetermineEventsToSubscribe(request.Events);
    string name = Guid.NewGuid().ToString();
    Microsoft.AnalysisServices.Tabular.Trace trace1 = connectionInfo.TabularServer.Traces.Add(name);
    TraceContext context = new TraceContext()
    {
      Trace = trace1,
      Server = connectionInfo.TabularServer,
      TraceName = name,
      SubscribedEvents = eventsToSubscribe,
      StartTime = DateTime.UtcNow,
      FilterCurrentSessionOnly = request.FilterCurrentSessionOnly
    };
    try
    {
      TraceOperations.AddEventsToTrace(context, eventsToSubscribe);
      if (request.FilterCurrentSessionOnly)
        TraceOperations.ApplySessionFilter(context, connectionInfo);
      TraceOperations.SetupTraceEventHandler(context);
      context.Trace.Update();
      context.Trace.Start();
      connectionInfo.Trace = context;
      return new TraceStartResult()
      {
        TraceName = context.TraceName,
        Status = "started",
        StartTime = context.StartTime,
        SubscribedEvents = eventsToSubscribe,
        FilterCurrentSessionOnly = context.FilterCurrentSessionOnly
      };
    }
    catch (Exception ex)
    {
      try
      {
        if (trace1 != null)
        {
          trace1.Stop();
          trace1.Drop();
        }
      }
      catch
      {
      }
      throw new McpException("Failed to start trace: " + ex.Message);
    }
  }

  public static TraceStopResult StopTrace(string? connectionName)
  {
    PowerBIModelingMCP.Library.Common.DataStructures.ConnectionInfo connection = ConnectionOperations.Get(connectionName);
    ConnectionValidator.ValidateForTrace(connection);
    TraceContext traceContext = connection.Trace != null ? connection.Trace : throw new McpException("No active trace on this connection");
    double totalSeconds = (DateTime.UtcNow - traceContext.StartTime).TotalSeconds;
    try
    {
      traceContext.Trace.Stop();
      traceContext.Trace.Drop();
      TraceStopResult traceStopResult = new TraceStopResult { TraceName = traceContext.TraceName };
      traceStopResult.Status = "stopped";
      traceStopResult.Duration = totalSeconds;
      traceStopResult.TotalEventsCaptured = traceContext.TotalEventsCaptured;
      traceStopResult.TotalEventsDiscarded = traceContext.TotalEventsDiscarded;
      connection.Trace = (TraceContext) null;
      return traceStopResult;
    }
    catch (Exception ex)
    {
      throw new McpException("Failed to stop trace: " + ex.Message);
    }
  }

  public static TraceGet GetTrace(string? connectionName)
  {
    PowerBIModelingMCP.Library.Common.DataStructures.ConnectionInfo connection = ConnectionOperations.Get(connectionName);
    ConnectionValidator.ValidateForTrace(connection);
    if (connection.Trace == null)
      return new TraceGet() { Status = "no active trace" };
    TraceContext trace = connection.Trace;
    double totalSeconds = (DateTime.UtcNow - trace.StartTime).TotalSeconds;
    return new TraceGet()
    {
      TraceName = trace.TraceName,
      Status = trace.IsPaused ? "paused" : "active",
      StartTime = new DateTime?(trace.StartTime),
      Duration = new double?(totalSeconds),
      EventsCaptured = new int?(trace.TotalEventsCaptured),
      EventsDiscarded = new int?(trace.TotalEventsDiscarded),
      SubscribedEvents = trace.SubscribedEvents,
      FilterCurrentSessionOnly = new bool?(trace.FilterCurrentSessionOnly)
    };
  }

  public static List<TraceGet> ListTraces()
  {
    List<TraceGet> traceGetList = new List<TraceGet>();
    foreach (ConnectionGet listConnection in ConnectionOperations.ListConnections())
    {
      try
      {
        PowerBIModelingMCP.Library.Common.DataStructures.ConnectionInfo connectionInfo = ConnectionOperations.Get(listConnection.ConnectionName);
        if (connectionInfo.Trace != null)
        {
          TraceContext trace = connectionInfo.Trace;
          double totalSeconds = (DateTime.UtcNow - trace.StartTime).TotalSeconds;
          traceGetList.Add(new TraceGet()
          {
            TraceName = trace.TraceName,
            Status = trace.IsPaused ? "paused" : "active",
            StartTime = new DateTime?(trace.StartTime),
            Duration = new double?(totalSeconds),
            EventsCaptured = new int?(trace.TotalEventsCaptured),
            EventsDiscarded = new int?(trace.TotalEventsDiscarded),
            SubscribedEvents = trace.SubscribedEvents,
            FilterCurrentSessionOnly = new bool?(trace.FilterCurrentSessionOnly)
          });
        }
      }
      catch
      {
      }
    }
    return traceGetList;
  }

  public static List<AvailableTraceEvent> ListAvailableEvents()
  {
    List<AvailableTraceEvent> availableTraceEventList = new List<AvailableTraceEvent>();
    foreach (string allowedEvent in TraceOperations.AllowedEvents)
    {
      TraceEventDefinition eventDefinition = TraceOperations.EventDefinitions[allowedEvent];
      availableTraceEventList.Add(new AvailableTraceEvent()
      {
        EventName = allowedEvent,
        Category = eventDefinition.Category,
        Description = eventDefinition.Description
      });
    }
    return Enumerable.ToList<AvailableTraceEvent>(Enumerable.ThenBy<AvailableTraceEvent, string>(Enumerable.OrderBy<AvailableTraceEvent, string>((IEnumerable<AvailableTraceEvent>) availableTraceEventList, (e => e.Category)), (e => e.EventName)));
  }

  public static TracePauseResult PauseTrace(string? connectionName)
  {
    PowerBIModelingMCP.Library.Common.DataStructures.ConnectionInfo connection = ConnectionOperations.Get(connectionName);
    ConnectionValidator.ValidateForTrace(connection);
    TraceContext traceContext = connection.Trace != null ? connection.Trace : throw new McpException("No active trace on this connection");
    if (traceContext.IsPaused)
    {
      TracePauseResult tracePauseResult = new TracePauseResult { TraceName = traceContext.TraceName };
      tracePauseResult.Status = "paused";
      tracePauseResult.StartTime = traceContext.StartTime;
      tracePauseResult.Duration = (DateTime.UtcNow - traceContext.StartTime).TotalSeconds;
      tracePauseResult.EventsCaptured = traceContext.TotalEventsCaptured;
      tracePauseResult.EventsDiscarded = traceContext.TotalEventsDiscarded;
      tracePauseResult.SubscribedEvents = traceContext.SubscribedEvents;
      List<string> stringList = new List<string>();
      stringList.Add("Trace is already paused");
      tracePauseResult.Warnings = stringList;
      tracePauseResult.FilterCurrentSessionOnly = traceContext.FilterCurrentSessionOnly;
      return tracePauseResult;
    }
    traceContext.IsPaused = true;
    double totalSeconds = (DateTime.UtcNow - traceContext.StartTime).TotalSeconds;
    return new TracePauseResult()
    {
      TraceName = traceContext.TraceName,
      Status = "paused",
      StartTime = traceContext.StartTime,
      Duration = totalSeconds,
      EventsCaptured = traceContext.TotalEventsCaptured,
      EventsDiscarded = traceContext.TotalEventsDiscarded,
      SubscribedEvents = traceContext.SubscribedEvents,
      FilterCurrentSessionOnly = traceContext.FilterCurrentSessionOnly
    };
  }

  public static TraceResumeResult ResumeTrace(string? connectionName)
  {
    PowerBIModelingMCP.Library.Common.DataStructures.ConnectionInfo connection = ConnectionOperations.Get(connectionName);
    ConnectionValidator.ValidateForTrace(connection);
    TraceContext traceContext = connection.Trace != null ? connection.Trace : throw new McpException("No active trace on this connection");
    if (!traceContext.IsPaused)
    {
      TraceResumeResult traceResumeResult = new TraceResumeResult { TraceName = traceContext.TraceName };
      traceResumeResult.Status = "active";
      traceResumeResult.StartTime = traceContext.StartTime;
      traceResumeResult.Duration = (DateTime.UtcNow - traceContext.StartTime).TotalSeconds;
      traceResumeResult.EventsCaptured = traceContext.TotalEventsCaptured;
      traceResumeResult.EventsDiscarded = traceContext.TotalEventsDiscarded;
      traceResumeResult.SubscribedEvents = traceContext.SubscribedEvents;
      List<string> stringList = new List<string>();
      stringList.Add("Trace is not paused");
      traceResumeResult.Warnings = stringList;
      traceResumeResult.FilterCurrentSessionOnly = traceContext.FilterCurrentSessionOnly;
      return traceResumeResult;
    }
    traceContext.IsPaused = false;
    double totalSeconds = (DateTime.UtcNow - traceContext.StartTime).TotalSeconds;
    return new TraceResumeResult()
    {
      TraceName = traceContext.TraceName,
      Status = "active",
      StartTime = traceContext.StartTime,
      Duration = totalSeconds,
      EventsCaptured = traceContext.TotalEventsCaptured,
      EventsDiscarded = traceContext.TotalEventsDiscarded,
      SubscribedEvents = traceContext.SubscribedEvents,
      FilterCurrentSessionOnly = traceContext.FilterCurrentSessionOnly
    };
  }

  public static TraceClearResult ClearTraceEvents(string? connectionName)
  {
    PowerBIModelingMCP.Library.Common.DataStructures.ConnectionInfo connection = ConnectionOperations.Get(connectionName);
    ConnectionValidator.ValidateForTrace(connection);
    TraceContext traceContext = connection.Trace != null ? connection.Trace : throw new McpException("No active trace on this connection");
    int count = traceContext.CapturedEvents.Count;
    traceContext.CapturedEvents.Clear();
    traceContext.TotalEventsCaptured = 0;
    traceContext.TotalEventsDiscarded = 0;
    return new TraceClearResult()
    {
      TraceName = traceContext.TraceName,
      EventsCleared = count,
      Status = traceContext.IsPaused ? "paused" : "active"
    };
  }

  internal static List<CapturedTraceEvent> GetCapturedEvents(string? connectionName)
  {
    PowerBIModelingMCP.Library.Common.DataStructures.ConnectionInfo connection = ConnectionOperations.Get(connectionName);
    ConnectionValidator.ValidateForTrace(connection);
    if (connection.Trace == null)
      throw new McpException("No active trace on this connection");
    return connection.Trace.CapturedEvents;
  }

  public static TraceEventFetch FetchTraceEvents(string? connectionName, bool clearAfterFetch = true)
  {
    PowerBIModelingMCP.Library.Common.DataStructures.ConnectionInfo connection = ConnectionOperations.Get(connectionName);
    ConnectionValidator.ValidateForTrace(connection);
    TraceContext traceContext = connection.Trace != null ? connection.Trace : throw new McpException("No active trace on this connection");
    List<CapturedTraceEvent> capturedTraceEventList = new List<CapturedTraceEvent>((IEnumerable<CapturedTraceEvent>) traceContext.CapturedEvents);
    TraceEventFetch traceEventFetch = new TraceEventFetch { TraceName = traceContext.TraceName };
    traceEventFetch.EventCount = capturedTraceEventList.Count;
    traceEventFetch.Cleared = clearAfterFetch;
    traceEventFetch.Events = capturedTraceEventList;
    if (!clearAfterFetch)
      return traceEventFetch;
    traceContext.CapturedEvents.Clear();
    traceContext.TotalEventsCaptured = 0;
    traceContext.TotalEventsDiscarded = 0;
    return traceEventFetch;
  }

  public static TraceEventJSONExport ExportTraceEventsToJSON(
    string? connectionName,
    string filePath,
    bool clearAfterFetch = false)
  {
    List<string> stringList = !string.IsNullOrWhiteSpace(filePath) ? ExportContentProcessor.ValidateFilePath(filePath) : throw new McpException("File path is required for ExportJSON operation");
    if (Enumerable.Any<string>((IEnumerable<string>) stringList))
      throw new McpException("Invalid file path: " + string.Join(", ", (IEnumerable<string>) stringList));
    PowerBIModelingMCP.Library.Common.DataStructures.ConnectionInfo connection = ConnectionOperations.Get(connectionName);
    ConnectionValidator.ValidateForTrace(connection);
    TraceContext traceContext = connection.Trace != null ? connection.Trace : throw new McpException("No active trace on this connection");
    List<CapturedTraceEvent> capturedTraceEventList = new List<CapturedTraceEvent>((IEnumerable<CapturedTraceEvent>) traceContext.CapturedEvents);
    string str1 = System.Text.Json.JsonSerializer.Serialize(new
    {
      TraceName = traceContext.TraceName,
      ExportTime = DateTime.UtcNow,
      TraceStartTime = traceContext.StartTime,
      EventCount = capturedTraceEventList.Count,
      SubscribedEvents = traceContext.SubscribedEvents,
      Events = capturedTraceEventList
    }, new JsonSerializerOptions()
    {
      WriteIndented = true,
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    });
    string str2;
    try
    {
      str2 = Path.IsPathRooted(filePath) ? filePath : Path.GetFullPath(filePath);
      string directoryName = Path.GetDirectoryName(str2);
      if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
        Directory.CreateDirectory(directoryName);
      File.WriteAllText(str2, str1);
    }
    catch (Exception ex)
    {
      throw new McpException("Failed to save trace events to file: " + ex.Message);
    }
    TraceEventJSONExport json = new TraceEventJSONExport { TraceName = traceContext.TraceName };
    json.EventCount = capturedTraceEventList.Count;
    json.Cleared = clearAfterFetch;
    json.FilePath = str2;
    if (!clearAfterFetch)
      return json;
    traceContext.CapturedEvents.Clear();
    traceContext.TotalEventsCaptured = 0;
    traceContext.TotalEventsDiscarded = 0;
    return json;
  }

  private static List<string> DetermineEventsToSubscribe(List<string>? requestedEvents)
  {
    List<string> stringList = new List<string>();
    if (requestedEvents == null || requestedEvents.Count == 0)
    {
      stringList.AddRange((IEnumerable<string>) TraceOperations.DefaultEvents);
    }
    else
    {
      List<string> list = Enumerable.ToList<string>(Enumerable.Except<string>((IEnumerable<string>) requestedEvents, (IEnumerable<string>) TraceOperations.AllowedEvents, (IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase));
      if (Enumerable.Any<string>((IEnumerable<string>) list))
        throw new McpException("Invalid event names: " + string.Join(", ", (IEnumerable<string>) list));
      stringList.AddRange((IEnumerable<string>) requestedEvents);
    }
    return Enumerable.ToList<string>(Enumerable.Distinct<string>((IEnumerable<string>) stringList, (IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase));
  }

  private static void ApplySessionFilter(TraceContext context, PowerBIModelingMCP.Library.Common.DataStructures.ConnectionInfo info)
  {
    string sessionId = info.AdomdConnection != null ? info.SessionId : throw new McpException("Cannot apply session filter: ADOMD connection not available");
    XmlNode xmlNode = !string.IsNullOrEmpty(sessionId) ? TraceOperations.CreateSessionIdFilter(sessionId, "MCP-PBIModeling") : throw new McpException("Cannot apply session filter: SessionID not available");
    context.Trace.Filter = xmlNode;
  }

  private static XmlNode CreateSessionIdFilter(string sessionId, string applicationName)
  {
    string str = string.Format("<Or xmlns=\"http://schemas.microsoft.com/analysisservices/2003/engine\">\r\n  <Equal><ColumnID>{0}</ColumnID><Value>{1}</Value></Equal>\r\n  <Equal><ColumnID>{2}</ColumnID><Value>{3}</Value></Equal>\r\n</Or>", new object[4]
    {
      (object) 39,
      (object) sessionId,
      (object) 37,
      (object) applicationName
    });
    XmlDocument xmlDocument = new XmlDocument();
    xmlDocument.LoadXml(str);
    return (XmlNode) xmlDocument.DocumentElement;
  }

  private static void AddEventsToTrace(TraceContext context, List<string> events)
  {
    foreach (string str in events)
    {
      TraceEventClass traceEventClass;
      if (Enum.TryParse<TraceEventClass>(str, true, out traceEventClass))
        TraceOperations.AddEventColumns(context.Trace.Events.Add(traceEventClass), traceEventClass);
    }
  }

  private static void AddEventColumns(Microsoft.AnalysisServices.Tabular.TraceEvent traceEvent, TraceEventClass eventClass)
  {
    string str = eventClass.ToString();
    TraceEventDefinition traceEventDefinition;
    if (!TraceOperations.EventDefinitions.TryGetValue(str, out traceEventDefinition))
    {
      TraceOperations.AddCommonColumns(traceEvent);
    }
    else
    {
      foreach (string column in traceEventDefinition.Columns)
      {
        TraceColumn traceColumn;
        if (TraceOperations.TryMapColumnNameToEnum(column, out traceColumn))
        {
          try
          {
            traceEvent.Columns.Add(traceColumn);
          }
          catch
          {
          }
        }
      }
    }
  }

  private static void AddCommonColumns(Microsoft.AnalysisServices.Tabular.TraceEvent traceEvent)
  {
    TraceColumn[] traceColumnArray = new TraceColumn[16 /*0x10*/]
    {
      TraceColumn.EventClass,
      TraceColumn.EventSubclass,
      TraceColumn.CurrentTime,
      TraceColumn.StartTime,
      TraceColumn.DatabaseName,
      TraceColumn.DatabaseFriendlyName,
      TraceColumn.Spid,
      TraceColumn.ApplicationName,
      TraceColumn.ActivityID,
      TraceColumn.RequestID,
      TraceColumn.SessionID,
      TraceColumn.TextData,
      TraceColumn.Duration,
      TraceColumn.CpuTime,
      TraceColumn.EndTime,
      TraceColumn.Error
    };
    foreach (TraceColumn traceColumn in traceColumnArray)
    {
      try
      {
        traceEvent.Columns.Add(traceColumn);
      }
      catch
      {
      }
    }
  }

  private static bool TryMapColumnNameToEnum(string columnName, out TraceColumn traceColumn)
  {
    return Enum.TryParse<TraceColumn>((columnName == "SPID") ? "Spid" : ((columnName == "CPUTime") ? "CpuTime" : columnName), true, out traceColumn);
  }

  private static void SetupTraceEventHandler(TraceContext context)
  {
    context.Trace.OnEvent += (Microsoft.AnalysisServices.Tabular.TraceEventHandler) ((sender, args) =>
    {
      if (context.IsPaused)
      {
        ++context.TotalEventsDiscarded;
      }
      else
      {
        context.CapturedEvents.Add(TraceOperations.MapTraceEventArgs(args));
        ++context.TotalEventsCaptured;
        if (context.IsActive)
          return;
        context.IsActive = true;
      }
    });
  }

  private static CapturedTraceEvent MapTraceEventArgs(Microsoft.AnalysisServices.Tabular.TraceEventArgs args)
  {
    string str = args.EventClass.ToString();
    HashSet<string> availableColumns = (HashSet<string>) null;
    TraceEventDefinition traceEventDefinition;
    if (TraceOperations.EventDefinitions.TryGetValue(str, out traceEventDefinition))
      availableColumns = traceEventDefinition.Columns;
    return new CapturedTraceEvent()
    {
      EventClassName = args.EventClass.ToString(),
      EventSubclassName = HasColumn("EventSubclass") ? args.EventSubclass.ToString() : (string) null,
      TextData = HasColumn("TextData") ? args.TextData : (string) null,
      DatabaseName = HasColumn("DatabaseName") || HasColumn("DatabaseFriendlyName") ? args.DatabaseName : (string) null,
      ActivityId = HasColumn("ActivityID") ? args.ActivityID : (string) null,
      RequestId = HasColumn("RequestID") ? args.RequestID : (string) null,
      SessionId = HasColumn("SessionID") ? args.SessionID : (string) null,
      ApplicationName = HasColumn("ApplicationName") ? args.ApplicationName : (string) null,
      CurrentTime = HasColumn("CurrentTime") ? new DateTime?(args.CurrentTime) : new DateTime?(),
      StartTime = HasColumn("StartTime") ? new DateTime?(args.StartTime) : new DateTime?(),
      Duration = HasColumn("Duration") ? new long?(args.Duration) : new long?(),
      CpuTime = HasColumn("CPUTime") || HasColumn("CpuTime") ? new long?(args.CpuTime) : new long?(),
      EndTime = HasColumn("EndTime") ? new DateTime?(args.EndTime) : new DateTime?(),
      NTUserName = HasColumn("NTUserName") || HasColumn("NTCanonicalUserName") || HasColumn("NTDomainName") ? args.NTUserName : (string) null,
      RequestProperties = HasColumn("RequestProperties") ? args.RequestProperties : (string) null,
      RequestParameters = HasColumn("RequestParameters") ? args.RequestParameters : (string) null,
      ObjectName = HasColumn("ObjectName") ? args.ObjectName : (string) null,
      ObjectPath = HasColumn("ObjectPath") ? args.ObjectPath : (string) null,
      ObjectReference = HasColumn("ObjectReference") ? args.ObjectReference : (string) null,
      Spid = HasColumn("SPID") || HasColumn("Spid") ? args.Spid : (string) null,
      IntegerData = HasColumn("IntegerData") ? new long?(args.IntegerData) : new long?(),
      ProgressTotal = HasColumn("ProgressTotal") ? new long?(args.ProgressTotal) : new long?(),
      ObjectId = HasColumn("ObjectID") ? args.ObjectID : (string) null,
      Error = HasColumn("Error") ? args.Error : (string) null
    };

    bool HasColumn(string columnName)
    {
      return availableColumns == null || availableColumns.Contains(columnName);
    }
  }

  private static void InitializeEventDefinitions()
  {
    TraceOperations.AddEventDef("QueryBegin", "Queries Events", "Query begin.", "ActivityID", "ApplicationContext", "ApplicationName", "ClientProcessID", "ConnectionID", "CurrentTime", "DatabaseFriendlyName", "DatabaseName", "EventClass", "EventSubclass", "Identity", "NTCanonicalUserName", "NTDomainName", "NTUserName", "RequestID", "RequestParameters", "RequestProperties", "SPID", "ServerName", "SessionID", "StartTime", "TextData", "UserObjectID");
    TraceOperations.AddEventDef("QueryEnd", "Queries Events", "Query end.", "ActivityID", "ApplicationContext", "ApplicationName", "CPUTime", "ClientProcessID", "ConnectionID", "CurrentTime", "DatabaseFriendlyName", "DatabaseName", "Duration", "EndTime", "Error", "ErrorType", "EventClass", "EventSubclass", "Identity", "NTCanonicalUserName", "NTDomainName", "NTUserName", "RequestID", "ServerName", "SessionID", "Severity", "SPID", "StartTime", "Success", "TextData", "UserObjectID");
    TraceOperations.AddEventDef("CommandBegin", "Command Events", "Command begin.", "ActivityID", "ApplicationContext", "ApplicationName", "ClientProcessID", "ConnectionID", "CurrentTime", "DatabaseFriendlyName", "DatabaseName", "EventClass", "EventSubclass", "Identity", "NTCanonicalUserName", "NTDomainName", "NTUserName", "RequestID", "RequestParameters", "RequestProperties", "SPID", "ServerName", "SessionID", "SessionType", "StartTime", "TextData", "UserObjectID");
    TraceOperations.AddEventDef("CommandEnd", "Command Events", "Command end.", "ActivityID", "ApplicationContext", "ApplicationName", "CPUTime", "ClientProcessID", "ConnectionID", "CurrentTime", "DatabaseFriendlyName", "DatabaseName", "Duration", "EndTime", "Error", "ErrorType", "EventClass", "EventSubclass", "Identity", "IntegerData", "NTCanonicalUserName", "NTDomainName", "NTUserName", "RequestID", "SPID", "ServerName", "SessionID", "SessionType", "Severity", "StartTime", "Success", "TextData", "UserObjectID");
    TraceOperations.AddEventDef("Error", "Errors and Warnings", "Server error.", "ActivityID", "ApplicationContext", "ApplicationName", "CalculationExpression", "ClientHostName", "ClientProcessID", "ConnectionID", "CurrentTime", "DatabaseFriendlyName", "DatabaseName", "Error", "ErrorType", "EventClass", "Identity", "NTDomainName", "NTUserName", "RequestID", "SPID", "ServerName", "SessionID", "SessionType", "Severity", "StartTime", "Success", "TextData", "UserObjectID");
    TraceOperations.AddEventDef("DiscoverBegin", "Discover Events", "Start of Discover Request.", "ActivityID", "ApplicationContext", "ApplicationName", "ClientProcessID", "ConnectionID", "CurrentTime", "DatabaseFriendlyName", "DatabaseName", "EventClass", "EventSubclass", "Identity", "NTCanonicalUserName", "NTDomainName", "NTUserName", "RequestID", "RequestProperties", "SPID", "ServerName", "SessionID", "StartTime", "TextData", "UserObjectID");
    TraceOperations.AddEventDef("DiscoverEnd", "Discover Events", "End of Discover Request.", "ActivityID", "ApplicationContext", "ApplicationName", "CPUTime", "ClientProcessID", "ConnectionID", "CurrentTime", "DatabaseFriendlyName", "DatabaseName", "Duration", "EndTime", "Error", "ErrorType", "EventClass", "EventSubclass", "Identity", "IntegerData", "NTCanonicalUserName", "NTDomainName", "NTUserName", "RequestID", "RequestProperties", "SPID", "ServerName", "SessionID", "Severity", "StartTime", "Success", "TextData", "UserObjectID");
    TraceOperations.AddEventDef("ProgressReportBegin", "Progress Reports", "Progress report begin.", "ActivityID", "ApplicationContext", "ConnectionID", "CurrentTime", "DatabaseFriendlyName", "DatabaseName", "EventClass", "EventSubclass", "Identity", "JobID", "NTCanonicalUserName", "NTDomainName", "NTUserName", "ObjectID", "ObjectName", "ObjectPath", "ObjectReference", "ObjectType", "RequestID", "SPID", "ServerName", "SessionID", "SessionType", "StartTime", "TextData", "UserObjectID");
    TraceOperations.AddEventDef("ProgressReportEnd", "Progress Reports", "Progress report end.", "ActivityID", "ApplicationContext", "CPUTime", "ConnectionID", "CurrentTime", "DatabaseFriendlyName", "DatabaseName", "Duration", "EndTime", "Error", "ErrorType", "EventClass", "EventSubclass", "Identity", "IntegerData", "JobID", "NTCanonicalUserName", "NTDomainName", "NTUserName", "ObjectID", "ObjectName", "ObjectPath", "ObjectReference", "ObjectType", "ProgressTotal", "RequestID", "SPID", "ServerName", "SessionID", "SessionType", "Severity", "StartTime", "Success", "TextData", "UserObjectID");
    TraceOperations.AddEventDef("ProgressReportCurrent", "Progress Reports", "Progress report current.", "ActivityID", "ApplicationContext", "ConnectionID", "CurrentTime", "DatabaseFriendlyName", "DatabaseName", "EventClass", "EventSubclass", "Identity", "IntegerData", "JobID", "NTDomainName", "NTUserName", "ObjectID", "ObjectName", "ObjectPath", "ObjectReference", "ObjectType", "ProgressTotal", "RequestID", "SPID", "ServerName", "SessionID", "SessionType", "StartTime", "TextData", "UserObjectID");
    TraceOperations.AddEventDef("ProgressReportError", "Progress Reports", "Progress report error.", "ActivityID", "ApplicationContext", "ConnectionID", "CurrentTime", "DatabaseFriendlyName", "DatabaseName", "Duration", "EndTime", "Error", "ErrorType", "EventClass", "EventSubclass", "Identity", "IntegerData", "JobID", "NTDomainName", "NTUserName", "ObjectID", "ObjectName", "ObjectPath", "ObjectReference", "ObjectType", "ProgressTotal", "RequestID", "SPID", "ServerName", "SessionID", "SessionType", "Severity", "StartTime", "TextData", "UserObjectID");
    TraceOperations.AddEventDef("VertiPaqSEQueryBegin", "Query Processing", "VertiPaq SE Query", "ActivityID", "ApplicationContext", "ConnectionID", "CurrentTime", "DatabaseFriendlyName", "DatabaseName", "EventClass", "EventSubclass", "Identity", "JobID", "NTCanonicalUserName", "NTDomainName", "NTUserName", "ObjectID", "ObjectName", "ObjectPath", "ObjectReference", "ObjectType", "RequestID", "SPID", "ServerName", "SessionID", "SessionType", "StartTime", "TextData", "UserObjectID");
    TraceOperations.AddEventDef("VertiPaqSEQueryEnd", "Query Processing", "VertiPaq SE Query", "ActivityID", "ApplicationContext", "CPUTime", "ConnectionID", "CurrentTime", "DatabaseFriendlyName", "DatabaseName", "Duration", "EndTime", "Error", "ErrorType", "EventClass", "EventSubclass", "Identity", "IntegerData", "JobID", "NTCanonicalUserName", "NTDomainName", "NTUserName", "ObjectID", "ObjectName", "ObjectPath", "ObjectReference", "ObjectType", "ProgressTotal", "RequestID", "SPID", "ServerName", "SessionID", "SessionType", "Severity", "StartTime", "Success", "TextData", "UserObjectID");
    TraceOperations.AddEventDef("VertiPaqSEQueryCacheMatch", "Query Processing", "VertiPaq SE Query Cache Use", "ActivityID", "ApplicationContext", "ConnectionID", "CurrentTime", "DatabaseFriendlyName", "DatabaseName", "EventClass", "EventSubclass", "Identity", "JobID", "NTCanonicalUserName", "NTDomainName", "NTUserName", "ObjectID", "ObjectName", "ObjectPath", "ObjectReference", "ObjectType", "RequestID", "SPID", "ServerName", "SessionID", "SessionType", "TextData", "UserObjectID");
    TraceOperations.AddEventDef("DirectQueryBegin", "Query Processing", "DirectQuery Begin.", "ActivityID", "ApplicationContext", "ApplicationName", "CPUTime", "ClientProcessID", "ConnectionID", "CurrentTime", "DatabaseFriendlyName", "DatabaseName", "Error", "ErrorType", "EventClass", "Identity", "JobID", "NTUserName", "ObjectID", "ObjectName", "ObjectPath", "ObjectType", "RequestID", "SPID", "ServerName", "SessionID", "SessionType", "Severity", "StartTime", "Success", "TextData");
    TraceOperations.AddEventDef("DirectQueryEnd", "Query Processing", "DirectQuery End.", "ActivityID", "ApplicationContext", "ApplicationName", "CPUTime", "ClientProcessID", "ConnectionID", "CurrentTime", "DatabaseFriendlyName", "DatabaseName", "Duration", "EndTime", "Error", "ErrorType", "EventClass", "Identity", "JobID", "NTUserName", "ObjectID", "ObjectName", "ObjectPath", "ObjectType", "RequestID", "SPID", "ServerName", "SessionID", "SessionType", "Severity", "StartTime", "Success", "TextData");
    TraceOperations.AddEventDef("DAXQueryPlan", "Query Processing", "DAX logical/physical plan tree for VertiPaq and DirectQuery modes.", "ActivityID", "ApplicationContext", "ApplicationName", "CPUTime", "ClientHostName", "ClientProcessID", "ConnectionID", "CurrentTime", "DatabaseFriendlyName", "DatabaseName", "Duration", "EndTime", "EventClass", "EventSubclass", "Identity", "IntegerData", "NTCanonicalUserName", "RequestID", "SPID", "ServerName", "SessionID", "StartTime", "TextData");
    TraceOperations.AddEventDef("ResourceUsage", "Query Processing", "Reports reads, writes, cpu usage after end of commands and queries.", "ActivityID", "ApplicationContext", "ApplicationName", "ClientProcessID", "ConnectionID", "CurrentTime", "DatabaseFriendlyName", "DatabaseName", "EventClass", "Identity", "NTCanonicalUserName", "NTDomainName", "NTUserName", "RequestID", "SPID", "ServerName", "SessionID", "TextData", "UserObjectID");
    TraceOperations.AddEventDef("CalculationEvaluation", "Query Processing", "Information about the evaluation of calculations. This event will have a negative impact on performance when turned on.", "ActivityID", "ApplicationContext", "ApplicationName", "CPUTime", "ClientHostName", "ClientProcessID", "ConnectionID", "CurrentTime", "DatabaseFriendlyName", "DatabaseName", "Duration", "EndTime", "EventClass", "EventSubclass", "Identity", "IntegerData", "NTCanonicalUserName", "RequestID", "SPID", "ServerName", "SessionID", "StartTime", "TextData");
    TraceOperations.AddEventDef("DAXEvaluationLog", "Query Processing", "Output of EvaluateAndLog function.", "ActivityID", "ApplicationContext", "ApplicationName", "CPUTime", "ClientHostName", "ClientProcessID", "ConnectionID", "CurrentTime", "DatabaseFriendlyName", "DatabaseName", "Duration", "EndTime", "EventClass", "Identity", "IntegerData", "Label", "NTCanonicalUserName", "RequestID", "SPID", "ServerName", "SessionID", "StartTime", "TextData");
    TraceOperations.AddEventDef("AggregateTableRewriteQuery", "Query Processing", "A query was rewritten according to available aggregate tables.", "ActivityID", "ApplicationContext", "ApplicationName", "ClientHostName", "ClientProcessID", "ConnectionID", "CurrentTime", "DatabaseFriendlyName", "DatabaseName", "Duration", "EndTime", "EventClass", "EventSubclass", "Identity", "IntegerData", "NTCanonicalUserName", "NTDomainName", "NTUserName", "RequestID", "SPID", "ServerName", "SessionID", "StartTime", "Success", "TextData", "UserObjectID");
    TraceOperations.AddEventDef("ExecutionMetrics", "Execution Metrics Events", "Customer facing execution metrics.", "ActivityID", "ApplicationContext", "ApplicationName", "DatabaseFriendlyName", "DatabaseName", "EventClass", "Identity", "RequestID", "SPID", "ServerName", "TextData");
    TraceOperations.AddEventDef("JobGraph", "Job Graph Events", "Job graph related events", "ActivityID", "ApplicationName", "ClientProcessID", "ConnectionID", "CurrentTime", "DatabaseFriendlyName", "DatabaseName", "EventClass", "EventSubclass", "Identity", "IntegerData", "NTDomainName", "NTUserName", "RequestID", "SPID", "ServerName", "SessionID", "Success", "TextData");
  }

  private static void AddEventDef(
    string name,
    string category,
    string description,
    params string[] columns)
  {
    TraceOperations.EventDefinitions[name] = new TraceEventDefinition()
    {
      Name = name,
      Category = category,
      Description = description,
      Columns = new HashSet<string>((IEnumerable<string>) columns, (IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase)
    };
  }
}
