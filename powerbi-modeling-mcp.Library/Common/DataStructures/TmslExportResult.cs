// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System;
using System.Collections.Generic;
using System.ComponentModel;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class TmslExportResult : ExportResultBase
{
  [Description("The operation type that was performed")]
  public TmslOperationType OperationType { get; set; }

  [Description("TMSL-specific export metadata")]
  public Dictionary<string, object> TmslMetadata { get; set; } = new Dictionary<string, object>();

  [Description("TMSL options that were applied")]
  public ExportTmsl? AppliedOptions { get; set; }

  [Description("The generated TMSL script content (legacy compatibility)")]
  public string TmslScript
  {
    get => this.Content;
    set => this.Content = value;
  }

  public static TmslExportResult CreateSuccess(
    string objectName,
    string objectType,
    string content,
    string processedContent,
    bool isTruncated,
    string? savedFilePath,
    List<string> warnings,
    TmslOperationType operationType,
    ExportTmsl? appliedOptions = null)
  {
    TmslExportResult success = ExportResultBase.CreateSuccess<TmslExportResult>(objectName, objectType, content, processedContent, isTruncated, savedFilePath, warnings);
    success.OperationType = operationType;
    success.AppliedOptions = appliedOptions;
    success.TmslMetadata["ExportType"] = (object) "TMSL";
    success.TmslMetadata["ContentType"] = (object) "JSON";
    success.TmslMetadata["OperationType"] = (object) operationType.ToString();
    if (appliedOptions != null)
    {
      success.TmslMetadata["FormatJson"] = (object) appliedOptions.FormatJson;
      if (!string.IsNullOrWhiteSpace(appliedOptions.TmslOperationType))
        success.TmslMetadata["RequestedOperationType"] = (object) appliedOptions.TmslOperationType;
      if (!string.IsNullOrWhiteSpace(appliedOptions.RefreshType))
        success.TmslMetadata["RefreshType"] = (object) appliedOptions.RefreshType;
      bool? includeRestricted = appliedOptions.IncludeRestricted;
      if (includeRestricted.HasValue)
      {
        success.TmslMetadata["IncludeRestricted"] = includeRestricted.Value;
      }
    }
    return success;
  }

  public static TmslExportResult CreateFailure(
    string objectName,
    string objectType,
    string errorMessage)
  {
    TmslExportResult failure = ExportResultBase.CreateFailure<TmslExportResult>(objectName, objectType, errorMessage);
    failure.TmslMetadata["ExportType"] = (object) "TMSL";
    failure.TmslMetadata["ContentType"] = (object) "JSON";
    return failure;
  }

  public static TmslExportResult FromLegacyResult(TmslOperationResult legacyResult)
  {
    TmslExportResult tmslExportResult = new TmslExportResult { Success = legacyResult.Success };
    tmslExportResult.ObjectName = legacyResult.ObjectName;
    tmslExportResult.ObjectType = legacyResult.ObjectType;
    tmslExportResult.Content = legacyResult.TmslScript;
    tmslExportResult.ContentLength = legacyResult.TmslScript.Length;
    tmslExportResult.ErrorMessage = legacyResult.ErrorMessage;
    tmslExportResult.GeneratedAt = legacyResult.GeneratedAt;
    tmslExportResult.OperationType = legacyResult.OperationType;
    tmslExportResult.TmslMetadata["ExportType"] = (object) "TMSL";
    tmslExportResult.TmslMetadata["ContentType"] = (object) "JSON";
    tmslExportResult.TmslMetadata["OperationType"] = (object) legacyResult.OperationType.ToString();
    tmslExportResult.TmslMetadata["MigratedFromLegacy"] = (object) true;
    return tmslExportResult;
  }
}
