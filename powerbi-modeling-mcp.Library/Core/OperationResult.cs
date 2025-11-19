// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System;
using System.Collections.Generic;

#nullable enable
namespace PowerBIModelingMCP.Library.Core;

public class OperationResult
{
  public bool Success { get; set; }

  public string? Message { get; set; }

  public string? ObjectName { get; set; }

  public PowerBIModelingMCP.Library.Core.ObjectType? ObjectType { get; set; }

  public PowerBIModelingMCP.Library.Core.Operation? Operation { get; set; }

  public Dictionary<string, object>? AdditionalData { get; set; }

  public Exception? Exception { get; set; }

  public bool HasChanges { get; set; }

  public static OperationResult CreateSuccess(
    string message,
    string? objectName = null,
    PowerBIModelingMCP.Library.Core.ObjectType? objectType = null,
    PowerBIModelingMCP.Library.Core.Operation? operation = null,
    bool hasChanges = true)
  {
    return new OperationResult()
    {
      Success = true,
      Message = message,
      ObjectName = objectName,
      ObjectType = objectType,
      Operation = operation,
      HasChanges = hasChanges
    };
  }

  public static OperationResult CreateFailure(string message, Exception? exception = null)
  {
    return new OperationResult()
    {
      Success = false,
      Message = message,
      Exception = exception,
      HasChanges = false
    };
  }
}
