// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using Microsoft.AnalysisServices.Tabular;
using System;
using System.Collections.Generic;

#nullable enable
namespace PowerBIModelingMCP.Library.Core;

public class TransactionContext
{
  public string TransactionId { get; set; } = Guid.NewGuid().ToString();

  public DateTime StartTime { get; set; } = DateTime.UtcNow;

  public List<string> Operations { get; set; } = new List<string>();

  public required Server Server { get; set; }

  public required Database Database { get; set; }
}
