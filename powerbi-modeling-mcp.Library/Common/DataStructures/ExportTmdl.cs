// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System.ComponentModel;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class ExportTmdl : ExportOptionsBase
{
  [Description("Optional TMDL serialization options for controlling metadata output")]
  public TmdlSerializationOptions? SerializationOptions { get; set; }
}
