// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Tools.CalculationGroupOperationRequest
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using PowerBIModelingMCP.Library.Common.DataStructures;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

#nullable enable
namespace PowerBIModelingMCP.Library.Tools;

public class CalculationGroupOperationRequest
{
  [Description("The connection name to use for the operation (optional - uses last used connection if not provided)")]
  public string? ConnectionName { get; set; }

  [Required]
  [Description("The operation to perform: Help, CreateGroup, UpdateGroup, DeleteGroup, GetGroup, ListGroups, RenameGroup, CreateItem, UpdateItem, DeleteItem, GetItem, ListItems, RenameItem, ReorderItems, ExportTMDL")]
  public required string Operation { get; set; }

  [Description("Calculation group name (required for most operations)")]
  public string? CalculationGroupName { get; set; }

  [Description("New calculation group name (required for RenameGroup operation)")]
  public string? NewCalculationGroupName { get; set; }

  [Description("Calculation item name (required for item operations)")]
  public string? CalculationItemName { get; set; }

  [Description("New calculation item name (required for RenameItem operation)")]
  public string? NewCalculationItemName { get; set; }

  [Description("Calculation group definition for CreateGroup operation")]
  public CalculationGroupCreate? CreateGroupDefinition { get; set; }

  [Description("Calculation group update definition for UpdateGroup operation")]
  public CalculationGroupUpdate? UpdateGroupDefinition { get; set; }

  [Description("Calculation item definition for CreateItem operation")]
  public CalculationItemCreate? CreateItemDefinition { get; set; }

  [Description("Calculation item update definition for UpdateItem operation")]
  public CalculationItemUpdate? UpdateItemDefinition { get; set; }

  [Description("Ordered list of calculation item names for ReorderItems operation")]
  public List<string>? ItemNamesInOrder { get; set; }

  [Description("TMDL (YAML-like) export parameters for ExportTMDL operation")]
  public CalculationGroupExportTmdl? TmdlExportOptions { get; set; }
}
