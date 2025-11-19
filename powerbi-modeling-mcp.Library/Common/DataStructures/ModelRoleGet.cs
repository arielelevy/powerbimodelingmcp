// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System.Collections.Generic;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class ModelRoleGet : ModelRoleBase
{
  public List<Dictionary<string, string>>? TablePermissions { get; set; } = new List<Dictionary<string, string>>();
}
