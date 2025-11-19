// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System;
using System.Collections.Generic;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class TmdlSerializeResult
{
  public bool Success { get; set; }

  public string FolderPath { get; set; } = string.Empty;

  public string DatabaseName { get; set; } = string.Empty;

  public List<string> FilesCreated { get; set; } = new List<string>();

  public int FileCount { get; set; }

  public DateTime SerializedAt { get; set; }

  public string? Message { get; set; }
}
