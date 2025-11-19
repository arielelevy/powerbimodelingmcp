// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.DataStructures.TablePermissionBase
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using System;
using System.Collections.Generic;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class TablePermissionBase
{
  public string? RoleName { get; set; }

  public string? TableName { get; set; }

  public string? FilterExpression { get; set; }

  public string? MetadataPermission { get; set; }

  public string? State { get; set; }

  public string? ErrorMessage { get; set; }

  public DateTime? ModifiedTime { get; set; }

  public List<KeyValuePair<string, string>>? Annotations { get; set; }

  public List<ExtendedProperty>? ExtendedProperties { get; set; }
}
