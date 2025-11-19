// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class TmdlDeserializeResult
{
  public bool Success { get; set; }

  public string ConnectionName { get; set; } = string.Empty;

  public string DatabaseName { get; set; } = string.Empty;

  public string FolderPath { get; set; } = string.Empty;

  public int TablesLoaded { get; set; }

  public int MeasuresLoaded { get; set; }

  public int RelationshipsLoaded { get; set; }

  public DateTime LoadedAt { get; set; }

  public string? Message { get; set; }
}
