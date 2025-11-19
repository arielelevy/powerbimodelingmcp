// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System;

#nullable enable
namespace PowerBIModelingMCP.Library.Core;

public class InsufficientScopesException : Exception
{
  public InsufficientScopesException(string message)
    : base(message)
  {
  }

  public InsufficientScopesException(string message, Exception innerException)
    : base(message, innerException)
  {
  }
}
