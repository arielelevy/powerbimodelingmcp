// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System;
using System.ComponentModel;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class TmslOperationResult
{
  [Description("Indicates whether the TMSL script generation was successful")]
  public bool Success { get; set; }

  [Description("The generated TMSL script content")]
  public string TmslScript { get; set; } = string.Empty;

  [Description("The operation type that was performed")]
  public TmslOperationType OperationType { get; set; }

  [Description("Name of the object the operation was performed on")]
  public string ObjectName { get; set; } = string.Empty;

  [Description("Type of the object the operation was performed on")]
  public string ObjectType { get; set; } = string.Empty;

  [Description("Error message if the operation failed")]
  public string? ErrorMessage { get; set; }

  [Description("Timestamp when the script was generated")]
  public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}
