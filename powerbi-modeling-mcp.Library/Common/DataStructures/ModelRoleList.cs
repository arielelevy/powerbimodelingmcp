// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System.Collections.Generic;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class ModelRoleList : ObjectListBase
{
  [System.ComponentModel.Description("The model permission level (None, Read, ReadRefresh, Refresh, Administrator)")]
  public string? ModelPermission { get; set; }

  [System.ComponentModel.Description("List of table names that have permissions defined in this role")]
  public List<string>? TableNames { get; set; }
}
