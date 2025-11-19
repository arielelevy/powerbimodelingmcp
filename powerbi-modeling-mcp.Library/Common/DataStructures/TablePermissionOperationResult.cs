// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class TablePermissionOperationResult
{
  public string State { get; set; } = string.Empty;

  public string? ErrorMessage { get; set; }

  public string RoleName { get; set; } = string.Empty;

  public string TableName { get; set; } = string.Empty;

  public bool HasChanges { get; set; }
}
