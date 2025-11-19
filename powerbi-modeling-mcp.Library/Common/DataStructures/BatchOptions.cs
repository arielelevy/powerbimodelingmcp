// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System.ComponentModel;

#nullable disable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class BatchOptions
{
  [Description("Continue processing remaining items when an error occurs")]
  public bool ContinueOnError { get; set; }

  [Description("Wrap the batch operation in a transaction for atomicity")]
  public bool UseTransaction { get; set; } = true;
}
