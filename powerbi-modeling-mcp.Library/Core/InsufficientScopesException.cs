// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Core.InsufficientScopesException
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

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
