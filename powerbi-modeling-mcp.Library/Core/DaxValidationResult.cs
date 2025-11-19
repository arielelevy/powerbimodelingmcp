// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System.Collections.Generic;

#nullable enable
namespace PowerBIModelingMCP.Library.Core;

public class DaxValidationResult
{
  public bool IsValid { get; set; }

  public string? ErrorMessage { get; set; }

  public string? DetailedError { get; set; }

  public List<DaxColumnInfo> ExpectedColumns { get; set; } = new List<DaxColumnInfo>();

  public long ValidationTimeMs { get; set; }
}
