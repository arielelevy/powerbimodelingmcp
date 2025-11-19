// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System;

#nullable enable
namespace PowerBIModelingMCP.Library.Common;

public class SyncStateInfo
{
  public bool CanSync { get; set; }

  public bool HasLocalChanges { get; set; }

  public bool IsOffline { get; set; }

  public bool IsInTransaction { get; set; }

  public DateTime? LastSynced { get; set; }

  public string Message { get; set; } = string.Empty;
}
