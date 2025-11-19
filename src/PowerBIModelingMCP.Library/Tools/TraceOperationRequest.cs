// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Tools.TraceOperationRequest
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

#nullable enable
namespace PowerBIModelingMCP.Library.Tools;

public class TraceOperationRequest
{
  [Required]
  [Description("The operation to perform: Help, Start, Stop, Pause, Resume, Clear, Get, List, Report, ExportJSON")]
  public required string Operation { get; set; }

  [Description("Connection name (optional - uses last used connection if not provided)")]
  public string? ConnectionName { get; set; }

  [Description("Events to capture (Start operation only, optional). If not specified, default events will be captured: CommandBegin, CommandEnd, QueryBegin, QueryEnd, VertiPaqSEQueryBegin, VertiPaqSEQueryEnd, VertiPaqSEQueryCacheMatch, DirectQueryBegin, DirectQueryEnd, ExecutionMetrics, Error.")]
  public List<string>? Events { get; set; }

  [Description("When true, filters trace events to current session only (default: true). When false, captures all events from all sessions.")]
  public bool? FilterCurrentSessionOnly { get; set; }

  [Description("Whether to clear events after fetching (Fetch or ExportJSON operation only, default: false for Fetch, false for ExportJSON)")]
  public bool? ClearAfterFetch { get; set; }

  [Description("File path to save JSON export (ExportJSON operation only, required)")]
  public string? FilePath { get; set; }

  [Description("Columns to include in fetch (Fetch operation only, optional). Default: EventClassName, EventSubclassName, StartTime. Available columns: EventClassName, EventSubclassName, TextData, DatabaseName, ActivityId, RequestId, SessionId, ApplicationName, CurrentTime, StartTime, Duration, CpuTime, EndTime, NTUserName, RequestProperties, RequestParameters, ObjectName, ObjectPath, ObjectReference, Spid, IntegerData, ProgressTotal, ObjectId, Error")]
  public List<string>? Columns { get; set; }
}
