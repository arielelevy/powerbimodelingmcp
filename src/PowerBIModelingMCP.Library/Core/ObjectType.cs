// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Core.ObjectType
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

#nullable disable
namespace PowerBIModelingMCP.Library.Core;

public enum ObjectType
{
  Database,
  Model,
  Table,
  Column,
  Measure,
  Relationship,
  Partition,
  DataSource,
  CalculationGroup,
  CalculationItem,
  UserHierarchy,
  SecurityRole,
  RLSFilter,
  NamedExpression,
  Perspective,
  PerspectiveTable,
  PerspectiveColumn,
  PerspectiveMeasure,
  PerspectiveHierarchy,
  Culture,
  ObjectTranslation,
  QueryGroup,
  Unknown,
}
