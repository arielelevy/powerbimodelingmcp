// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System;
using System.Collections.Generic;
using System.ComponentModel;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public abstract class ExportResultBase
{
  [Description("Indicates whether the export operation was successful")]
  public bool Success { get; set; }

  [Description("Error message if the operation failed")]
  public string? ErrorMessage { get; set; }

  [Description("Name of the object that was exported")]
  public string ObjectName { get; set; } = string.Empty;

  [Description("Type of the object that was exported")]
  public string ObjectType { get; set; } = string.Empty;

  [Description("The exported content (may be truncated)")]
  public string Content { get; set; } = string.Empty;

  [Description("Total length of the generated content")]
  public int ContentLength { get; set; }

  [Description("Whether the content was truncated")]
  public bool IsTruncated { get; set; }

  [Description("File path where the content was saved")]
  public string? SavedFilePath { get; set; }

  [Description("Timestamp when the export was performed")]
  public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

  [Description("Warnings encountered during export")]
  public List<string> Warnings { get; set; } = new List<string>();

  [Description("Additional metadata about the export operation")]
  public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

  protected static T CreateSuccess<T>(
    string objectName,
    string objectType,
    string content,
    string processedContent,
    bool isTruncated,
    string? savedFilePath,
    List<string> warnings)
    where T : ExportResultBase, new()
  {
    T success = new T { Success = true };
    success.ObjectName = objectName;
    success.ObjectType = objectType;
    success.Content = processedContent;
    success.ContentLength = content.Length;
    success.IsTruncated = isTruncated;
    success.SavedFilePath = savedFilePath;
    success.Warnings = warnings;
    success.GeneratedAt = DateTime.UtcNow;
    return success;
  }

  protected static T CreateFailure<T>(string objectName, string objectType, string errorMessage) where T : ExportResultBase, new()
  {
    T failure = new T { Success = false };
    failure.ObjectName = objectName;
    failure.ObjectType = objectType;
    failure.ErrorMessage = errorMessage;
    failure.GeneratedAt = DateTime.UtcNow;
    return failure;
  }
}
