// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class CultureCreate : CultureBase
{
  public CultureCreate()
  {
  }

  public CultureCreate(string name) => this.Name = name;
}
