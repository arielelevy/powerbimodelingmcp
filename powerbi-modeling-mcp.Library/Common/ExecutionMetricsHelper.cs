// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using PowerBIModelingMCP.Library.Common.DataStructures;
using PowerBIModelingMCP.Library.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;

#nullable enable
namespace PowerBIModelingMCP.Library.Common;

public static class ExecutionMetricsHelper
{
  public static List<CapturedTraceEvent> FilterEventsByRequestId(
    List<CapturedTraceEvent> allEvents,
    string requestId)
  {
    return string.IsNullOrWhiteSpace(requestId) ? new List<CapturedTraceEvent>() : Enumerable.ToList<CapturedTraceEvent>(Enumerable.Where<CapturedTraceEvent>((IEnumerable<CapturedTraceEvent>) allEvents, (e => string.Equals(e.RequestId, requestId, StringComparison.OrdinalIgnoreCase))));
  }

  public static string? ExtractRequestIdFromQueryBegin(List<CapturedTraceEvent> events)
  {
    return Enumerable.FirstOrDefault<CapturedTraceEvent>(Enumerable.OrderByDescending<CapturedTraceEvent, DateTime?>(Enumerable.Where<CapturedTraceEvent>((IEnumerable<CapturedTraceEvent>) events, (e => string.Equals(e.EventClassName, "QueryBegin", StringComparison.OrdinalIgnoreCase))), (e => e.CurrentTime ?? e.StartTime)))?.RequestId;
  }

  public static CalculatedExecutionMetrics CalculateMetrics(List<CapturedTraceEvent> queryEvents)
  {
    CalculatedExecutionMetrics metrics = new CalculatedExecutionMetrics()
    {
      Success = true
    };
    if (queryEvents.Count == 0)
    {
      metrics.Success = false;
      metrics.ErrorMessage = "No trace events found for query";
      return metrics;
    }
    metrics.ActivityId = Enumerable.FirstOrDefault<CapturedTraceEvent>((IEnumerable<CapturedTraceEvent>) queryEvents)?.ActivityId;
    ExecutionMetricsHelper.FixEventTimings(queryEvents);
    CapturedTraceEvent capturedTraceEvent1 = Enumerable.FirstOrDefault<CapturedTraceEvent>((IEnumerable<CapturedTraceEvent>) queryEvents, (e => string.Equals(e.EventClassName, "QueryBegin", StringComparison.OrdinalIgnoreCase)));
    CapturedTraceEvent capturedTraceEvent2 = Enumerable.FirstOrDefault<CapturedTraceEvent>((IEnumerable<CapturedTraceEvent>) queryEvents, (e => string.Equals(e.EventClassName, "QueryEnd", StringComparison.OrdinalIgnoreCase)));
    long? nullable1;
    DateTime? nullable2;
    if (capturedTraceEvent2 != null)
    {
      CalculatedExecutionMetrics executionMetrics1 = metrics;
      nullable1 = capturedTraceEvent2.Duration;
      long valueOrDefault1 = nullable1.GetValueOrDefault();
      executionMetrics1.TotalDuration = valueOrDefault1;
      CalculatedExecutionMetrics executionMetrics2 = metrics;
      nullable1 = capturedTraceEvent2.CpuTime;
      long valueOrDefault2 = nullable1.GetValueOrDefault();
      executionMetrics2.TotalCpuTime = valueOrDefault2;
      CalculatedExecutionMetrics executionMetrics3 = metrics;
      nullable2 = capturedTraceEvent2.EndTime;
      DateTime? nullable3 = nullable2 ?? capturedTraceEvent2.CurrentTime;
      executionMetrics3.QueryEndDateTime = nullable3;
      metrics.QueryText = capturedTraceEvent2.TextData;
    }
    if (capturedTraceEvent1 != null)
    {
      CalculatedExecutionMetrics executionMetrics = metrics;
      nullable2 = capturedTraceEvent1.StartTime;
      DateTime? nullable4 = nullable2 ?? capturedTraceEvent1.CurrentTime;
      executionMetrics.QueryStartDateTime = nullable4;
      if (metrics.QueryText == null)
        metrics.QueryText = capturedTraceEvent1.TextData;
    }
    CapturedTraceEvent maxEvent1 = (CapturedTraceEvent) null;
    CapturedTraceEvent maxEvent2 = (CapturedTraceEvent) null;
    int num1 = 0;
    long num2 = 0;
    long num3 = 0;
    foreach (CapturedTraceEvent queryEvent in queryEvents)
    {
      if (string.Equals(queryEvent.EventClassName, "VertiPaqSEQueryBegin", StringComparison.OrdinalIgnoreCase))
      {
        if (string.Equals(queryEvent.EventSubclassName, "BatchVertiPaqScan", StringComparison.OrdinalIgnoreCase))
        {
          ++num1;
          num2 = 0L;
          num3 = 0L;
        }
      }
      else if (string.Equals(queryEvent.EventClassName, "VertiPaqSEQueryEnd", StringComparison.OrdinalIgnoreCase))
      {
        if (string.Equals(queryEvent.EventSubclassName, "BatchVertiPaqScan", StringComparison.OrdinalIgnoreCase))
        {
          --num1;
          CapturedTraceEvent capturedTraceEvent3 = queryEvent;
          nullable1 = queryEvent.Duration;
          long? nullable5 = new long?(Math.Max(nullable1.GetValueOrDefault() - num2, 0L));
          capturedTraceEvent3.Duration = nullable5;
          CapturedTraceEvent capturedTraceEvent4 = queryEvent;
          nullable1 = queryEvent.Duration;
          long valueOrDefault3 = nullable1.GetValueOrDefault();
          capturedTraceEvent4.NetParallelDuration = valueOrDefault3;
          CapturedTraceEvent capturedTraceEvent5 = queryEvent;
          nullable1 = queryEvent.CpuTime;
          long? nullable6 = new long?(Math.Max(nullable1.GetValueOrDefault() - num3, 0L));
          capturedTraceEvent5.CpuTime = nullable6;
          CalculatedExecutionMetrics executionMetrics4 = metrics;
          long storageEngineDuration = executionMetrics4.StorageEngineDuration;
          nullable1 = queryEvent.Duration;
          long valueOrDefault4 = nullable1.GetValueOrDefault();
          executionMetrics4.StorageEngineDuration = storageEngineDuration + valueOrDefault4;
          CalculatedExecutionMetrics executionMetrics5 = metrics;
          long parallelDuration = executionMetrics5.StorageEngineNetParallelDuration;
          nullable1 = queryEvent.Duration;
          long valueOrDefault5 = nullable1.GetValueOrDefault();
          executionMetrics5.StorageEngineNetParallelDuration = parallelDuration + valueOrDefault5;
          CalculatedExecutionMetrics executionMetrics6 = metrics;
          long storageEngineCpuTime = executionMetrics6.StorageEngineCpuTime;
          nullable1 = queryEvent.CpuTime;
          long valueOrDefault6 = nullable1.GetValueOrDefault();
          executionMetrics6.StorageEngineCpuTime = storageEngineCpuTime + valueOrDefault6;
          ++metrics.StorageEngineQueryCount;
        }
        else if (string.Equals(queryEvent.EventSubclassName, "VertiPaqScan", StringComparison.OrdinalIgnoreCase))
        {
          if (num1 > 0)
          {
            queryEvent.InternalBatchEvent = true;
            num2 += queryEvent.NetParallelDuration;
            long num4 = num3;
            nullable1 = queryEvent.CpuTime;
            long valueOrDefault = nullable1.GetValueOrDefault();
            num3 = num4 + valueOrDefault;
          }
          else
          {
            ExecutionMetricsHelper.UpdateForParallelOperations(ref maxEvent1, queryEvent);
            CalculatedExecutionMetrics executionMetrics = metrics;
            long storageEngineDuration = executionMetrics.StorageEngineDuration;
            nullable1 = queryEvent.Duration;
            long valueOrDefault = nullable1.GetValueOrDefault();
            executionMetrics.StorageEngineDuration = storageEngineDuration + valueOrDefault;
          }
          metrics.StorageEngineNetParallelDuration += queryEvent.NetParallelDuration;
          CalculatedExecutionMetrics executionMetrics7 = metrics;
          long storageEngineCpuTime = executionMetrics7.StorageEngineCpuTime;
          nullable1 = queryEvent.CpuTime;
          long valueOrDefault7 = nullable1.GetValueOrDefault();
          executionMetrics7.StorageEngineCpuTime = storageEngineCpuTime + valueOrDefault7;
          ++metrics.StorageEngineQueryCount;
        }
      }
      else if (string.Equals(queryEvent.EventClassName, "DirectQueryEnd", StringComparison.OrdinalIgnoreCase))
      {
        ExecutionMetricsHelper.UpdateForParallelOperations(ref maxEvent2, queryEvent);
        CalculatedExecutionMetrics executionMetrics8 = metrics;
        long directQueryDuration = executionMetrics8.TotalDirectQueryDuration;
        nullable1 = queryEvent.Duration;
        long valueOrDefault8 = nullable1.GetValueOrDefault();
        executionMetrics8.TotalDirectQueryDuration = directQueryDuration + valueOrDefault8;
        CalculatedExecutionMetrics executionMetrics9 = metrics;
        long storageEngineDuration = executionMetrics9.StorageEngineDuration;
        nullable1 = queryEvent.Duration;
        long valueOrDefault9 = nullable1.GetValueOrDefault();
        executionMetrics9.StorageEngineDuration = storageEngineDuration + valueOrDefault9;
        metrics.StorageEngineNetParallelDuration += queryEvent.NetParallelDuration;
        CalculatedExecutionMetrics executionMetrics10 = metrics;
        long storageEngineCpuTime = executionMetrics10.StorageEngineCpuTime;
        nullable1 = queryEvent.CpuTime;
        long valueOrDefault10 = nullable1.GetValueOrDefault();
        executionMetrics10.StorageEngineCpuTime = storageEngineCpuTime + valueOrDefault10;
        ++metrics.StorageEngineQueryCount;
        ++metrics.TotalDirectQueryCount;
      }
      else if (string.Equals(queryEvent.EventClassName, "VertiPaqSEQueryCacheMatch", StringComparison.OrdinalIgnoreCase))
        ++metrics.VertipaqCacheMatches;
    }
    metrics.FormulaEngineDuration = ExecutionMetricsHelper.CalculateFormulaEngineDuration(queryEvents);
    metrics.TotalDuration = Math.Max(metrics.FormulaEngineDuration, metrics.TotalDuration);
    if ((double) (metrics.StorageEngineNetParallelDuration + metrics.FormulaEngineDuration) < (double) metrics.TotalDuration)
    {
      metrics.StorageEngineDuration = metrics.StorageEngineNetParallelDuration;
      metrics.FormulaEngineDuration = metrics.TotalDuration - metrics.StorageEngineDuration;
    }
    else
      metrics.StorageEngineDuration = metrics.TotalDuration - metrics.FormulaEngineDuration;
    metrics.TotalCpuFactor = metrics.TotalDuration == 0L ? 0.0 : (double) metrics.TotalCpuTime / (double) metrics.TotalDuration;
    metrics.StorageEngineCpuFactor = metrics.StorageEngineDuration == 0L ? 0.0 : (double) metrics.StorageEngineCpuTime / (double) metrics.StorageEngineDuration;
    metrics.StorageEngineDurationPercentage = metrics.TotalDuration == 0L ? 0.0 : (double) metrics.StorageEngineNetParallelDuration / (double) metrics.TotalDuration * 100.0;
    metrics.FormulaEngineDurationPercentage = metrics.TotalDuration == 0L ? 0.0 : (double) metrics.FormulaEngineDuration / (double) metrics.TotalDuration * 100.0;
    metrics.VertipaqCacheMatchesPercentage = metrics.StorageEngineQueryCount == 0 ? 0.0 : (double) metrics.VertipaqCacheMatches / (double) metrics.StorageEngineQueryCount * 100.0;
    return metrics;
  }

  private static void FixEventTimings(List<CapturedTraceEvent> events)
  {
    foreach (CapturedTraceEvent capturedTraceEvent1 in events)
    {
      if (string.Equals(capturedTraceEvent1.EventClassName, "VertiPaqSEQueryEnd", StringComparison.OrdinalIgnoreCase) || string.Equals(capturedTraceEvent1.EventClassName, "DirectQueryEnd", StringComparison.OrdinalIgnoreCase) || string.Equals(capturedTraceEvent1.EventClassName, "QueryEnd", StringComparison.OrdinalIgnoreCase))
      {
        DateTime? nullable1 = capturedTraceEvent1.StartTime;
        DateTime? nullable2 = nullable1 ?? capturedTraceEvent1.CurrentTime;
        nullable1 = capturedTraceEvent1.EndTime;
        DateTime? nullable3 = nullable1 ?? capturedTraceEvent1.CurrentTime;
        long valueOrDefault = capturedTraceEvent1.Duration.GetValueOrDefault();
        nullable1 = nullable3;
        DateTime? nullable4 = nullable2;
        if ((nullable1.HasValue == nullable4.HasValue ? (nullable1.HasValue ? ((nullable1.GetValueOrDefault() == nullable4.GetValueOrDefault()) ? 1 : 0) : 1) : 0) != 0 && valueOrDefault > 0L)
        {
          CapturedTraceEvent capturedTraceEvent2 = capturedTraceEvent1;
          DateTime? nullable5;
          if (!nullable2.HasValue)
          {
            nullable4 = new DateTime?();
            nullable5 = nullable4;
          }
          else
            nullable5 = new DateTime?(nullable2.GetValueOrDefault().AddMilliseconds((double) valueOrDefault));
          capturedTraceEvent2.EndTime = nullable5;
        }
        else
        {
          nullable4 = nullable3;
          nullable1 = nullable2;
          if ((nullable4.HasValue & nullable1.HasValue ? ((nullable4.GetValueOrDefault() >= nullable1.GetValueOrDefault()) ? 1 : 0) : 0) != 0 && nullable3.HasValue && nullable2.HasValue)
          {
            long totalMilliseconds = (long) (nullable3.Value - nullable2.Value).TotalMilliseconds;
            if (totalMilliseconds > valueOrDefault)
              capturedTraceEvent1.Duration = new long?(totalMilliseconds);
          }
        }
        capturedTraceEvent1.NetParallelDuration = capturedTraceEvent1.Duration.GetValueOrDefault();
      }
    }
  }

  private static void UpdateForParallelOperations(
    ref CapturedTraceEvent? maxEvent,
    CapturedTraceEvent traceEvent)
  {
    if (maxEvent == null)
    {
      maxEvent = traceEvent;
    }
    else
    {
      DateTime? nullable1 = maxEvent.EndTime ?? maxEvent.CurrentTime;
      DateTime? nullable2 = traceEvent.StartTime ?? traceEvent.CurrentTime;
      DateTime? nullable3 = traceEvent.EndTime ?? traceEvent.CurrentTime;
      if (!nullable1.HasValue || !nullable2.HasValue || !nullable3.HasValue)
        return;
      TimeSpan timeSpan = (nullable1.Value - nullable2.Value);
      if (timeSpan.TotalMilliseconds > 0.0)
      {
        if ((nullable1.Value > nullable3.Value))
        {
          traceEvent.NetParallelDuration = 0L;
        }
        else
        {
          CapturedTraceEvent capturedTraceEvent = traceEvent;
          timeSpan = (nullable3.Value - nullable1.Value);
          long totalMilliseconds = (long) timeSpan.TotalMilliseconds;
          capturedTraceEvent.NetParallelDuration = totalMilliseconds;
          maxEvent = traceEvent;
        }
      }
      else
        maxEvent = traceEvent;
    }
  }

  private static long CalculateFormulaEngineDuration(List<CapturedTraceEvent> events)
  {
    int num = 0;
    double formulaEngineDuration = 0.0;
    DateTime? nullable1 = new DateTime?();
    foreach (CapturedTraceEvent capturedTraceEvent in events)
    {
      DateTime? nullable2 = capturedTraceEvent.StartTime ?? capturedTraceEvent.CurrentTime;
      DateTime? nullable3 = capturedTraceEvent.EndTime ?? capturedTraceEvent.CurrentTime;
      if (string.Equals(capturedTraceEvent.EventClassName, "QueryBegin", StringComparison.OrdinalIgnoreCase))
        nullable1 = nullable2;
      else if (string.Equals(capturedTraceEvent.EventClassName, "QueryEnd", StringComparison.OrdinalIgnoreCase))
      {
        if (num == 0 && nullable1.HasValue && nullable3.HasValue)
        {
          double totalMilliseconds = (nullable3.Value - nullable1.Value).TotalMilliseconds;
          formulaEngineDuration += totalMilliseconds;
        }
      }
      else if (string.Equals(capturedTraceEvent.EventClassName, "VertiPaqSEQueryBegin", StringComparison.OrdinalIgnoreCase) || string.Equals(capturedTraceEvent.EventClassName, "DirectQueryBegin", StringComparison.OrdinalIgnoreCase))
      {
        if (num == 0 && nullable1.HasValue && nullable2.HasValue)
        {
          double totalMilliseconds = (nullable2.Value - nullable1.Value).TotalMilliseconds;
          formulaEngineDuration += totalMilliseconds;
        }
        ++num;
      }
      else if (string.Equals(capturedTraceEvent.EventClassName, "VertiPaqSEQueryEnd", StringComparison.OrdinalIgnoreCase) || string.Equals(capturedTraceEvent.EventClassName, "DirectQueryEnd", StringComparison.OrdinalIgnoreCase))
      {
        --num;
        if (num == 0)
          nullable1 = nullable3;
      }
    }
    return (long) formulaEngineDuration;
  }

  public static (bool Success, string ErrorMessage) WaitForQueryMetricsEvents(
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
        return (false, "Error while waiting for query metrics events: " + ex.Message);
      }
    }
    return (false, $"Timeout after {timeoutSeconds} seconds waiting for QueryEnd or Error event");
  }

  public static CalculatedExecutionMetrics ExtractQueryMetrics(List<CapturedTraceEvent> allEvents)
  {
    string idFromQueryBegin = ExecutionMetricsHelper.ExtractRequestIdFromQueryBegin(allEvents);
    if (string.IsNullOrWhiteSpace(idFromQueryBegin))
      return new CalculatedExecutionMetrics()
      {
        Success = false,
        ErrorMessage = "No QueryBegin event found in trace events"
      };
    List<CapturedTraceEvent> queryEvents = ExecutionMetricsHelper.FilterEventsByRequestId(allEvents, idFromQueryBegin);
    if (queryEvents.Count != 0)
      return ExecutionMetricsHelper.CalculateMetrics(queryEvents);
    return new CalculatedExecutionMetrics()
    {
      Success = false,
      ErrorMessage = "No trace events found with RequestId: " + idFromQueryBegin
    };
  }

  public static ReportedExecutionMetrics ExtractServerReportedMetrics(
    List<CapturedTraceEvent> capturedEvents)
  {
    if (capturedEvents == null || capturedEvents.Count == 0)
      return new ReportedExecutionMetrics()
      {
        Success = false,
        ErrorMessage = "No captured events provided"
      };
    List<CapturedTraceEvent> list = Enumerable.ToList<CapturedTraceEvent>(Enumerable.Where<CapturedTraceEvent>((IEnumerable<CapturedTraceEvent>) capturedEvents, (e => string.Equals(e.EventClassName, "ExecutionMetrics", StringComparison.OrdinalIgnoreCase))));
    if (list.Count == 0)
      return new ReportedExecutionMetrics()
      {
        Success = false,
        ErrorMessage = "No ExecutionMetrics events found in captured events"
      };
    List<(CapturedTraceEvent, ReportedExecutionMetrics, JsonElement)> valueTupleList = new List<(CapturedTraceEvent, ReportedExecutionMetrics, JsonElement)>();
    foreach (CapturedTraceEvent capturedTraceEvent in list)
    {
      string textData = capturedTraceEvent.TextData;
      if (!string.IsNullOrWhiteSpace(textData))
      {
        try
        {
          using (JsonDocument jsonDocument = JsonDocument.Parse(textData))
          {
            JsonElement rootElement = jsonDocument.RootElement;
            string str1 = ExecutionMetricsHelper.GetString(rootElement, "commandType");
            string str2 = ExecutionMetricsHelper.GetString(rootElement, "queryDialect");
            if (string.Equals(str1, "Statement", StringComparison.OrdinalIgnoreCase))
            {
              if ((str2 == "3"))
              {
                ReportedExecutionMetrics executionMetrics = new ReportedExecutionMetrics()
                {
                  Success = true,
                  ActivityId = capturedTraceEvent.ActivityId,
                  RequestId = capturedTraceEvent.RequestId,
                  TimeStart = ExecutionMetricsHelper.GetDateTime(rootElement, "timeStart"),
                  TimeEnd = ExecutionMetricsHelper.GetDateTime(rootElement, "timeEnd"),
                  DurationMs = ExecutionMetricsHelper.GetInt64(rootElement, "durationMs"),
                  DatasourceConnectionThrottleTimeMs = ExecutionMetricsHelper.GetInt64(rootElement, "datasourceConnectionThrottleTimeMs"),
                  DirectQueryConnectionTimeMs = ExecutionMetricsHelper.GetInt64(rootElement, "directQueryConnectionTimeMs"),
                  DirectQueryIterationTimeMs = ExecutionMetricsHelper.GetInt64(rootElement, "directQueryIterationTimeMs"),
                  DirectQueryTotalTimeMs = ExecutionMetricsHelper.GetInt64(rootElement, "directQueryTotalTimeMs"),
                  ExternalQueryExecutionTimeMs = ExecutionMetricsHelper.GetInt64(rootElement, "externalQueryExecutionTimeMs"),
                  VertipaqJobCpuTimeMs = ExecutionMetricsHelper.GetInt64(rootElement, "vertipaqJobCpuTimeMs"),
                  MEngineCpuTimeMs = ExecutionMetricsHelper.GetInt64(rootElement, "mEngineCpuTimeMs"),
                  QueryProcessingCpuTimeMs = ExecutionMetricsHelper.GetInt64(rootElement, "queryProcessingCpuTimeMs"),
                  TotalCpuTimeMs = ExecutionMetricsHelper.GetInt64(rootElement, "totalCpuTimeMs"),
                  ExecutionDelayMs = ExecutionMetricsHelper.GetInt64(rootElement, "executionDelayMs"),
                  CapacityThrottlingMs = ExecutionMetricsHelper.GetInt64(rootElement, "capacityThrottlingMs"),
                  ApproximatePeakMemConsumptionKB = ExecutionMetricsHelper.GetInt64(rootElement, "approximatePeakMemConsumptionKB"),
                  MEnginePeakMemoryKB = ExecutionMetricsHelper.GetInt64(rootElement, "mEnginePeakMemoryKB"),
                  ExternalQueryTimeoutMs = ExecutionMetricsHelper.GetInt64(rootElement, "externalQueryTimeoutMs"),
                  DirectQueryTimeoutMs = ExecutionMetricsHelper.GetInt64(rootElement, "directQueryTimeoutMs"),
                  TabularConnectionTimeoutMs = ExecutionMetricsHelper.GetInt64(rootElement, "tabularConnectionTimeoutMs"),
                  CommandType = ExecutionMetricsHelper.GetString(rootElement, "commandType"),
                  DiscoverType = ExecutionMetricsHelper.GetString(rootElement, "discoverType"),
                  QueryDialect = ExecutionMetricsHelper.GetString(rootElement, "queryDialect"),
                  ErrorCount = ExecutionMetricsHelper.GetInt32(rootElement, "errorCount"),
                  RefreshParallelism = ExecutionMetricsHelper.GetInt32(rootElement, "refreshParallelism"),
                  VertipaqTotalRows = ExecutionMetricsHelper.GetInt64(rootElement, "vertipaqTotalRows"),
                  QueryResultRows = ExecutionMetricsHelper.GetInt64(rootElement, "queryResultRows"),
                  DirectQueryRequestCount = ExecutionMetricsHelper.GetInt32(rootElement, "directQueryRequestCount"),
                  DirectQueryTotalRows = ExecutionMetricsHelper.GetInt64(rootElement, "directQueryTotalRows"),
                  QsoReplicaVersion = ExecutionMetricsHelper.GetString(rootElement, "qsoReplicaVersion"),
                  IntendedUsage = ExecutionMetricsHelper.GetInt32(rootElement, "intendedUsage"),
                  DirectLakeFallbackNotFramed = ExecutionMetricsHelper.GetBool(rootElement, "directLakeFallbackNotFramed"),
                  DirectLakeFallbackView = ExecutionMetricsHelper.GetBool(rootElement, "directLakeFallbackView"),
                  DirectLakeFallbackTooManyFiles = ExecutionMetricsHelper.GetBool(rootElement, "directLakeFallbackTooManyFiles"),
                  DirectLakeFallbackTooManyRowgroups = ExecutionMetricsHelper.GetBool(rootElement, "directLakeFallbackTooManyRowgroups"),
                  DirectLakeFallbackTooManyRows = ExecutionMetricsHelper.GetBool(rootElement, "directLakeFallbackTooManyRows"),
                  DirectLakeFallbackFramingRls = ExecutionMetricsHelper.GetBool(rootElement, "directLakeFallbackFramingRls"),
                  DirectLakeFallbackQueryOls = ExecutionMetricsHelper.GetBool(rootElement, "directLakeFallbackQueryOls"),
                  DirectLakeFallbackQueryRls = ExecutionMetricsHelper.GetBool(rootElement, "directLakeFallbackQueryRls")
                };
                valueTupleList.Add((capturedTraceEvent, executionMetrics, rootElement));
              }
            }
          }
        }
        catch (JsonException ex)
        {
        }
      }
    }
    if (valueTupleList.Count == 0)
    {
      ReportedExecutionMetrics serverReportedMetrics = new ReportedExecutionMetrics { Success = false };
      serverReportedMetrics.ErrorMessage = $"No ExecutionMetrics events found matching criteria (commandType='Statement', queryDialect=3). Found {list.Count} ExecutionMetrics events total.";
      return serverReportedMetrics;
    }
    if (valueTupleList.Count <= 1)
      return valueTupleList[0].Item2;
    ReportedExecutionMetrics serverReportedMetrics1 = new ReportedExecutionMetrics { Success = false };
    serverReportedMetrics1.ErrorMessage = $"Found {valueTupleList.Count} ExecutionMetrics events matching criteria. Expected exactly one.";
    return serverReportedMetrics1;
  }

  private static long? GetInt64(JsonElement root, string propertyName)
  {
    JsonElement jsonElement;
    return root.TryGetProperty(propertyName, out jsonElement) && jsonElement.ValueKind == JsonValueKind.Number ? new long?(jsonElement.GetInt64()) : new long?();
  }

  private static int? GetInt32(JsonElement root, string propertyName)
  {
    JsonElement jsonElement;
    return root.TryGetProperty(propertyName, out jsonElement) && jsonElement.ValueKind == JsonValueKind.Number ? new int?(jsonElement.GetInt32()) : new int?();
  }

  private static string? GetString(JsonElement root, string propertyName)
  {
    JsonElement jsonElement;
    if (root.TryGetProperty(propertyName, out jsonElement))
    {
      if (jsonElement.ValueKind == JsonValueKind.String)
        return jsonElement.GetString();
      if (jsonElement.ValueKind == JsonValueKind.Number)
        return jsonElement.GetInt64().ToString();
    }
    return (string) null;
  }

  private static DateTime? GetDateTime(JsonElement root, string propertyName)
  {
    JsonElement jsonElement;
    if (root.TryGetProperty(propertyName, out jsonElement) && jsonElement.ValueKind == JsonValueKind.String)
    {
      string str = jsonElement.GetString();
      DateTime dateTime;
      if (!string.IsNullOrEmpty(str) && DateTime.TryParse(str, out dateTime))
        return new DateTime?(dateTime);
    }
    return new DateTime?();
  }

  private static bool? GetBool(JsonElement root, string propertyName)
  {
    JsonElement jsonElement;
    if (root.TryGetProperty(propertyName, out jsonElement))
    {
      if (jsonElement.ValueKind == JsonValueKind.True || jsonElement.ValueKind == JsonValueKind.False)
        return new bool?(jsonElement.GetBoolean());
      if (jsonElement.ValueKind == JsonValueKind.Number)
        return new bool?(jsonElement.GetInt64() != 0L);
    }
    return new bool?();
  }
}
