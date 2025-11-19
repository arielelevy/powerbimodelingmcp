// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System.Collections.Generic;
using System.ComponentModel;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class TmdlExportResult : ExportResultBase
{
  [Description("TMDL-specific export metadata")]
  public Dictionary<string, object> TmdlMetadata { get; set; } = new Dictionary<string, object>();

  [Description("TMDL formatting options that were applied")]
  public ExportTmdl? AppliedOptions { get; set; }

  public static TmdlExportResult CreateSuccess(
    string objectName,
    string objectType,
    string content,
    string processedContent,
    bool isTruncated,
    string? savedFilePath,
    List<string> warnings,
    ExportTmdl? appliedOptions = null)
  {
    TmdlExportResult success = ExportResultBase.CreateSuccess<TmdlExportResult>(objectName, objectType, content, processedContent, isTruncated, savedFilePath, warnings);
    success.AppliedOptions = appliedOptions;
    success.TmdlMetadata["ExportType"] = (object) "TMDL";
    success.TmdlMetadata["ContentType"] = (object) "YAML-like";
    if (appliedOptions?.SerializationOptions != null)
    {
      success.TmdlMetadata["IncludeChildren"] = (object) appliedOptions.SerializationOptions.IncludeChildren;
      success.TmdlMetadata["IncludeInferredDataTypes"] = (object) appliedOptions.SerializationOptions.IncludeInferredDataTypes;
      success.TmdlMetadata["IncludeRestrictedInformation"] = (object) appliedOptions.SerializationOptions.IncludeRestrictedInformation;
    }
    return success;
  }

  public static TmdlExportResult CreateFailure(
    string objectName,
    string objectType,
    string errorMessage)
  {
    TmdlExportResult failure = ExportResultBase.CreateFailure<TmdlExportResult>(objectName, objectType, errorMessage);
    failure.TmdlMetadata["ExportType"] = (object) "TMDL";
    failure.TmdlMetadata["ContentType"] = (object) "YAML-like";
    return failure;
  }
}
