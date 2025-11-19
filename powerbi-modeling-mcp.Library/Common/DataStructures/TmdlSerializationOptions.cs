// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using Microsoft.AnalysisServices.Tabular.Serialization;
using ModelContextProtocol;
using System;
using System.ComponentModel;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class TmdlSerializationOptions
{
  [Description("Whether to include children of the root object (default: true)")]
  public bool IncludeChildren { get; set; } = true;

  [Description("Whether to include inferred data types (default: false)")]
  public bool IncludeInferredDataTypes { get; set; }

  [Description("Whether to include restricted information (default: false)")]
  public bool IncludeRestrictedInformation { get; set; }

  public MetadataSerializationOptions ToMetadataSerializationOptions()
  {
    try
    {
      MetadataSerializationOptionsBuilder serializationOptionsBuilder = new MetadataSerializationOptionsBuilder(MetadataSerializationStyle.Tmdl);
      if (!this.IncludeChildren)
        serializationOptionsBuilder.WithoutChildrenMetadata();
      if (this.IncludeInferredDataTypes)
        serializationOptionsBuilder.WithInferredDataTypes();
      if (this.IncludeRestrictedInformation)
        serializationOptionsBuilder.WithRestrictedInformation();
      return serializationOptionsBuilder.GetOptions();
    }
    catch (McpException ex)
    {
      throw;
    }
    catch (Exception ex)
    {
      throw new McpException($"Failed to convert TMDL serialization options: {ex.Message}. Please check your SerializationOptions configuration.");
    }
  }
}
