// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using PowerBIModelingMCP.Library.Common.DataStructures;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

#nullable enable
namespace PowerBIModelingMCP.Library.Tools;

public class SecurityRoleOperationRequest
{
  [Description("The connection name to use for the operation (optional - uses last used connection if not provided)")]
  public string? ConnectionName { get; set; }

  [Required]
  [Description("The operation to perform: Help, Create, Update, Delete, Get, List, Rename, CreatePermission, UpdatePermission, DeletePermission, GetPermission, ListPermissions, GetEffectivePermissions, ExportTMDL (YAML-like format), ExportTMSL (JSON script format)")]
  public required string Operation { get; set; }

  [Description("Role name (required for most role operations)")]
  public string? RoleName { get; set; }

  [Description("New role name (required for Rename operation)")]
  public string? NewRoleName { get; set; }

  [Description("Table name (required for table permission operations)")]
  public string? TableName { get; set; }

  [Description("Role definition for Create operation")]
  public ModelRoleCreate? CreateRoleDefinition { get; set; }

  [Description("Role update definition for Update operation")]
  public ModelRoleUpdate? UpdateRoleDefinition { get; set; }

  [Description("Table permission definition for CreatePermission operation")]
  public TablePermissionCreate? CreateTablePermissionDefinition { get; set; }

  [Description("Table permission update definition for UpdatePermission operation")]
  public TablePermissionUpdate? UpdateTablePermissionDefinition { get; set; }

  [Description("TMDL (YAML-like) export parameters for ExportTMDL operation")]
  public RoleExportTmdl? TmdlExportOptions { get; set; }

  [Description("TMSL (JSON script) export parameters for ExportTMSL operation")]
  public RoleExportTmsl? TmslExportOptions { get; set; }
}
