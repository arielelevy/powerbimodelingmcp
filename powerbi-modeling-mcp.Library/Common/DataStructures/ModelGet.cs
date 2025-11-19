// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System;

#nullable disable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class ModelGet : ModelBase
{
  public DateTime? ModifiedTime { get; set; }

  public DateTime? StructureModifiedTime { get; set; }
}
