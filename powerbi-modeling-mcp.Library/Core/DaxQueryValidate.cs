// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
#nullable enable
namespace PowerBIModelingMCP.Library.Core;

public class DaxQueryValidate
{
  public required string Query { get; set; }

  public int? TimeoutSeconds { get; set; }
}
