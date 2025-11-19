// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class CultureGet : CultureBase
{
  public string? State { get; set; }

  public string? ErrorMessage { get; set; }

  public CultureGet()
  {
  }

  public CultureGet(string name) => this.Name = name;
}
