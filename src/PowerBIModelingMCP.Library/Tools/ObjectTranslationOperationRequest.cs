// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Tools.ObjectTranslationOperationRequest
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using PowerBIModelingMCP.Library.Common.DataStructures;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

#nullable enable
namespace PowerBIModelingMCP.Library.Tools;

public class ObjectTranslationOperationRequest
{
  [Description("The connection name to use for the operation (optional - uses last used connection if not provided)")]
  public string? ConnectionName { get; set; }

  [Required]
  [Description("The operation to perform: Help, Create, Update, Delete, Get, List")]
  public required string Operation { get; set; }

  [Description("Object translation definition for Create operation")]
  public ObjectTranslationCreate? CreateDefinition { get; set; }

  [Description("Object translation update definition for Update operation")]
  public ObjectTranslationUpdate? UpdateDefinition { get; set; }

  [Description("Object translation delete definition for Delete operation")]
  public ObjectTranslationDelete? DeleteDefinition { get; set; }

  [Description("Object translation get definition for Get operation")]
  public ObjectTranslationBase? GetDefinition { get; set; }

  [Description("Optional filters for List operation")]
  public ObjectTranslationListFilters? ListFilters { get; set; }
}
