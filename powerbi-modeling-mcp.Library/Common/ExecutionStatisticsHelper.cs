// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using PowerBIModelingMCP.Library.Common.DataStructures;
using PowerBIModelingMCP.Library.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

#nullable enable
namespace PowerBIModelingMCP.Library.Common;

public static class ExecutionStatisticsHelper
{
  public static List<CapturedTraceEvent> FilterEventsByRequestId(
    List<CapturedTraceEvent> allEvents,
    string requestId)
  {
    return string.IsNullOrWhiteSpace(requestId) ? new List<CapturedTraceEvent>() : Enumerable.ToList<CapturedTraceEvent>(Enumerable.Where<CapturedTraceEvent>((IEnumerable<CapturedTraceEvent>) allEvents, (e => string.Equals(e.RequestId, requestId, StringComparison.OrdinalIgnoreCase))));
  }

  private static List<CapturedTraceEvent> GetPrimaryVertipaqScans(
    List<CapturedTraceEvent> queryEvents)
  {
    List<CapturedTraceEvent> primaryVertipaqScans = new List<CapturedTraceEvent>();
    bool flag1 = false;
    foreach (CapturedTraceEvent queryEvent in queryEvents)
    {
      int num1 = string.Equals(queryEvent.EventClassName, "VertiPaqSEQueryBegin", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
      bool flag2 = string.Equals(queryEvent.EventClassName, "VertiPaqSEQueryEnd", StringComparison.OrdinalIgnoreCase);
      bool flag3 = string.Equals(queryEvent.EventSubclassName, "BatchVertiPaqScan", StringComparison.OrdinalIgnoreCase);
      bool flag4 = string.Equals(queryEvent.EventSubclassName, "VertiPaqScan", StringComparison.OrdinalIgnoreCase);
      int num2 = flag3 ? 1 : 0;
      if ((num1 & num2) != 0)
        flag1 = true;
      else if (flag2 & flag3)
      {
        flag1 = false;
        primaryVertipaqScans.Add(queryEvent);
      }
      else if (flag2 & flag4 && !flag1)
        primaryVertipaqScans.Add(queryEvent);
    }
    return primaryVertipaqScans;
  }

  public static string? ExtractRequestIdFromQueryBegin(List<CapturedTraceEvent> events)
  {
    return Enumerable.FirstOrDefault<CapturedTraceEvent>(Enumerable.OrderByDescending<CapturedTraceEvent, DateTime?>(Enumerable.Where<CapturedTraceEvent>((IEnumerable<CapturedTraceEvent>) events, (e => string.Equals(e.EventClassName, "QueryBegin", StringComparison.OrdinalIgnoreCase))), (e => e.CurrentTime ?? e.StartTime)))?.RequestId;
  }

  public static QueryExecutionStatistics CalculateStatistics(
    List<CapturedTraceEvent> queryEvents,
    bool includeDetailedEvents = false)
  {
    QueryExecutionStatistics statistics = new QueryExecutionStatistics()
    {
      Success = true,
      DetailedEvents = includeDetailedEvents ? queryEvents : (List<CapturedTraceEvent>) null
    };
    if (queryEvents.Count == 0)
    {
      statistics.Success = false;
      statistics.ErrorMessage = "No trace events found for query";
      return statistics;
    }
    statistics.ActivityId = Enumerable.FirstOrDefault<CapturedTraceEvent>((IEnumerable<CapturedTraceEvent>) queryEvents)?.ActivityId;
    CapturedTraceEvent capturedTraceEvent1 = Enumerable.FirstOrDefault<CapturedTraceEvent>((IEnumerable<CapturedTraceEvent>) queryEvents, (e => string.Equals(e.EventClassName, "QueryBegin", StringComparison.OrdinalIgnoreCase)));
    CapturedTraceEvent capturedTraceEvent2 = Enumerable.FirstOrDefault<CapturedTraceEvent>((IEnumerable<CapturedTraceEvent>) queryEvents, (e => string.Equals(e.EventClassName, "QueryEnd", StringComparison.OrdinalIgnoreCase)));
    if (capturedTraceEvent2 != null)
    {
      QueryExecutionStatistics executionStatistics1 = statistics;
      long? nullable = capturedTraceEvent2.Duration;
      long valueOrDefault1 = nullable.GetValueOrDefault();
      executionStatistics1.TotalDuration = valueOrDefault1;
      QueryExecutionStatistics executionStatistics2 = statistics;
      nullable = capturedTraceEvent2.CpuTime;
      long valueOrDefault2 = nullable.GetValueOrDefault();
      executionStatistics2.TotalCpuTime = valueOrDefault2;
      statistics.QueryEndDateTime = capturedTraceEvent2.EndTime ?? capturedTraceEvent2.CurrentTime;
      statistics.QueryText = capturedTraceEvent2.TextData;
    }
    if (capturedTraceEvent1 != null)
    {
      statistics.QueryStartDateTime = capturedTraceEvent1.StartTime ?? capturedTraceEvent1.CurrentTime;
      if (statistics.QueryText == null)
        statistics.QueryText = capturedTraceEvent1.TextData;
    }
    List<CapturedTraceEvent> primaryVertipaqScans = ExecutionStatisticsHelper.GetPrimaryVertipaqScans(queryEvents);
    statistics.TotalVertipaqQueryCount = primaryVertipaqScans.Count;
    statistics.TotalVertipaqDuration = Enumerable.Sum<CapturedTraceEvent>((IEnumerable<CapturedTraceEvent>) primaryVertipaqScans, (e => e.Duration.GetValueOrDefault()));
    statistics.TotalVertipaqCpuTime = Enumerable.Sum<CapturedTraceEvent>((IEnumerable<CapturedTraceEvent>) primaryVertipaqScans, (e => e.CpuTime.GetValueOrDefault()));
    List<CapturedTraceEvent> list1 = Enumerable.ToList<CapturedTraceEvent>(Enumerable.Where<CapturedTraceEvent>((IEnumerable<CapturedTraceEvent>) queryEvents, (e => string.Equals(e.EventClassName, "VertiPaqSEQueryCacheMatch", StringComparison.OrdinalIgnoreCase))));
    statistics.TotalVertipaqCacheMatches = list1.Count;
    List<CapturedTraceEvent> list2 = Enumerable.ToList<CapturedTraceEvent>(Enumerable.Where<CapturedTraceEvent>((IEnumerable<CapturedTraceEvent>) queryEvents, (e => string.Equals(e.EventClassName, "DirectQueryEnd", StringComparison.OrdinalIgnoreCase))));
    statistics.TotalDirectQueryCount = list2.Count;
    statistics.TotalDirectQueryDuration = Enumerable.Sum<CapturedTraceEvent>((IEnumerable<CapturedTraceEvent>) list2, (e => e.Duration.GetValueOrDefault()));
    return statistics;
  }

  public static (bool Success, string ErrorMessage) WaitForQueryStatisticsEvents(
    string? connectionName,
    int timeoutSeconds = 20)
  {
    int num1 = timeoutSeconds * 1000 / 100;
    int num2 = 0;
    while (num2 < num1)
    {
      try
      {
        List<CapturedTraceEvent> capturedEvents = TraceOperations.GetCapturedEvents(connectionName);
        for (int index = capturedEvents.Count - 1; index >= 0; --index)
        {
          string eventClassName = capturedEvents[index].EventClassName;
          if (string.Equals(eventClassName, "QueryEnd", StringComparison.OrdinalIgnoreCase) || string.Equals(eventClassName, "Error", StringComparison.OrdinalIgnoreCase))
            return (true, string.Empty);
        }
        Thread.Sleep(100);
        ++num2;
      }
      catch (Exception ex)
      {
        return (false, "Error while waiting for query statistics events: " + ex.Message);
      }
    }
    return (false, $"Timeout after {timeoutSeconds} seconds waiting for QueryEnd or Error event");
  }

  public static QueryExecutionStatistics ExtractQueryStatistics(
    List<CapturedTraceEvent> allEvents,
    bool includeDetailedEvents = false)
  {
    string idFromQueryBegin = ExecutionStatisticsHelper.ExtractRequestIdFromQueryBegin(allEvents);
    if (string.IsNullOrWhiteSpace(idFromQueryBegin))
      return new QueryExecutionStatistics()
      {
        Success = false,
        ErrorMessage = "No QueryBegin event found in trace events"
      };
    List<CapturedTraceEvent> queryEvents = ExecutionStatisticsHelper.FilterEventsByRequestId(allEvents, idFromQueryBegin);
    if (queryEvents.Count != 0)
      return ExecutionStatisticsHelper.CalculateStatistics(queryEvents, includeDetailedEvents);
    return new QueryExecutionStatistics()
    {
      Success = false,
      ErrorMessage = "No trace events found with RequestId: " + idFromQueryBegin
    };
  }
}
