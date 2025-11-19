// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using ModelContextProtocol;
using System;

#nullable enable
namespace PowerBIModelingMCP.Library.Common;

public static class ValidationHelpers
{
  public static void ValidateObjectName(string? objectName, string parameterName)
  {
    if (string.IsNullOrWhiteSpace(objectName))
      throw new McpException(parameterName + " is required and cannot be empty");
  }

  public static void ValidateConnectionName(string? connectionName)
  {
    if (!string.IsNullOrEmpty(connectionName) && string.IsNullOrWhiteSpace(connectionName))
      throw new McpException("connectionName cannot be empty if provided");
  }

  public static string? ExtractServerNameFromConnectionString(string connectionString)
  {
    if (string.IsNullOrWhiteSpace(connectionString))
      return (string) null;
    foreach (string str1 in connectionString.Split(';', (StringSplitOptions) 0))
    {
      string str2 = str1.Trim();
      if (str2.StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase))
        return str2.Substring("Data Source=".Length).Trim();
      if (str2.StartsWith("Server=", StringComparison.OrdinalIgnoreCase))
        return str2.Substring("Server=".Length).Trim();
    }
    return (string) null;
  }

  public static string? ExtractDatabaseNameFromConnectionString(string connectionString)
  {
    if (string.IsNullOrWhiteSpace(connectionString))
      return (string) null;
    foreach (string str1 in connectionString.Split(';', (StringSplitOptions) 0))
    {
      string str2 = str1.Trim();
      if (str2.StartsWith("Initial Catalog=", StringComparison.OrdinalIgnoreCase))
        return str2.Substring("Initial Catalog=".Length).Trim();
      if (str2.StartsWith("Database=", StringComparison.OrdinalIgnoreCase))
        return str2.Substring("Database=".Length).Trim();
    }
    return (string) null;
  }
}
