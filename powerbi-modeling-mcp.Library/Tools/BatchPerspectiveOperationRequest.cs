// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using PowerBIModelingMCP.Library.Common.DataStructures;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

#nullable enable
namespace PowerBIModelingMCP.Library.Tools;

public class BatchPerspectiveOperationRequest
{
  [Description("Connection name (optional, uses last used connection if not provided)")]
  public string? ConnectionName { get; set; }

  [Required]
  [Description("The batch operation to perform: Help, BatchAddTables, BatchUpdateTables, BatchRemoveTables, BatchGetTables, BatchAddColumns, BatchRemoveColumns, BatchGetColumns, BatchAddMeasures, BatchRemoveMeasures, BatchGetMeasures, BatchAddHierarchies, BatchRemoveHierarchies, BatchGetHierarchies")]
  public required string Operation { get; set; }

  [Description("Batch add perspective tables request")]
  public BatchAddPerspectiveTablesRequest? BatchAddPerspectiveTablesRequest { get; set; }

  [Description("Batch update perspective tables request")]
  public BatchUpdatePerspectiveTablesRequest? BatchUpdatePerspectiveTablesRequest { get; set; }

  [Description("Batch remove perspective tables request")]
  public BatchRemovePerspectiveTablesRequest? BatchRemovePerspectiveTablesRequest { get; set; }

  [Description("Batch get perspective tables request")]
  public BatchGetPerspectiveTablesRequest? BatchGetPerspectiveTablesRequest { get; set; }

  [Description("Batch add perspective columns request")]
  public BatchAddPerspectiveColumnsRequest? BatchAddPerspectiveColumnsRequest { get; set; }

  [Description("Batch remove perspective columns request")]
  public BatchRemovePerspectiveColumnsRequest? BatchRemovePerspectiveColumnsRequest { get; set; }

  [Description("Batch get perspective columns request")]
  public BatchGetPerspectiveColumnsRequest? BatchGetPerspectiveColumnsRequest { get; set; }

  [Description("Batch add perspective measures request")]
  public BatchAddPerspectiveMeasuresRequest? BatchAddPerspectiveMeasuresRequest { get; set; }

  [Description("Batch remove perspective measures request")]
  public BatchRemovePerspectiveMeasuresRequest? BatchRemovePerspectiveMeasuresRequest { get; set; }

  [Description("Batch get perspective measures request")]
  public BatchGetPerspectiveMeasuresRequest? BatchGetPerspectiveMeasuresRequest { get; set; }

  [Description("Batch add perspective hierarchies request")]
  public BatchAddPerspectiveHierarchiesRequest? BatchAddPerspectiveHierarchiesRequest { get; set; }

  [Description("Batch remove perspective hierarchies request")]
  public BatchRemovePerspectiveHierarchiesRequest? BatchRemovePerspectiveHierarchiesRequest { get; set; }

  [Description("Batch get perspective hierarchies request")]
  public BatchGetPerspectiveHierarchiesRequest? BatchGetPerspectiveHierarchiesRequest { get; set; }
}
