// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.DataStructures.CommonTypes
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using ModelContextProtocol;
using System;
using System.Collections.Generic;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public static class CommonTypes
{
  public static class ObjectTypes
  {
    public const string Table = "Table";
    public const string Column = "Column";
    public const string Measure = "Measure";
    public const string CalculationGroup = "CalculationGroup";
    public const string CalculationItem = "CalculationItem";
    public const string Relationship = "Relationship";
    public const string DataSource = "DataSource";
    public const string Partition = "Partition";
    public const string ModelRole = "ModelRole";
    public const string TablePermission = "TablePermission";
    public const string UserHierarchy = "UserHierarchy";
    public const string HierarchyLevel = "HierarchyLevel";
    public const string Culture = "Culture";
    public const string Database = "Database";
  }

  public static class Connection
  {
    public const string ApplicationName = "MCP-PBIModeling";
  }

  public static class Annotations
  {
    public const string PBI_PRO_TOOLING_VALUE = "MCP-PBIModeling";

    public static KeyValuePair<string, string> Create(string key, string value)
    {
      return new KeyValuePair<string, string>(key, value);
    }
  }

  public static class Validation
  {
    public static void ValidateName(string? name, string objectType)
    {
      if (string.IsNullOrWhiteSpace(name))
        throw new McpException(objectType + " name cannot be null or empty");
    }

    public static void ValidateRequired(string? value, string propertyName, string objectType)
    {
      if (string.IsNullOrWhiteSpace(value))
        throw new McpException($"{propertyName} is required for {objectType}");
    }

    public static void ValidateEnum<T>(string? value, string propertyName) where T : struct, Enum
    {
      T obj;
      if (!string.IsNullOrWhiteSpace(value) && !Enum.TryParse<T>(value, out obj))
      {
        string[] names = Enum.GetNames(typeof (T));
        throw new McpException($"Invalid {propertyName} '{value}'. Valid values are: {string.Join(", ", names)}");
      }
    }
  }
}
