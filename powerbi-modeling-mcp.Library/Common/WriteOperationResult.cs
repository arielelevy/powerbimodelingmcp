// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System.Collections.Generic;

#nullable enable
namespace PowerBIModelingMCP.Library.Common;

public class WriteOperationResult
{
  public bool Success { get; set; }

  public string? Message { get; set; }

  public List<string>? Warnings { get; set; }

  public bool UserDeclinedConfirmation { get; set; }

  public bool UserDeclinedDiscardLocalChanges { get; set; }
}
