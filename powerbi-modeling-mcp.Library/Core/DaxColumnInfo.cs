// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
#nullable enable
namespace PowerBIModelingMCP.Library.Core;

public class DaxColumnInfo
{
  public string Name { get; set; } = string.Empty;

  public string DataType { get; set; } = string.Empty;

  public bool IsNullable { get; set; }

  public int Ordinal { get; set; }
}
