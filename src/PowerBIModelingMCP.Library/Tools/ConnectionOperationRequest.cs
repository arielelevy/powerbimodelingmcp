// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Tools.ConnectionOperationRequest
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

#nullable enable
namespace PowerBIModelingMCP.Library.Tools;

public class ConnectionOperationRequest
{
  [Required]
  [Description("The operation to perform: Help, Connect, ConnectFabric, ConnectFolder, Disconnect, GetConnection, ListConnections, ListLocalInstances, RenameConnection, ClearLastUsed, GetLastUsed, SetLastUsed")]
  public required string Operation { get; set; }

  [Description("Connection name (required for Disconnect, RenameConnection, SetLastUsed operations) - Cannot be supplied for Connect or ConnectFabric operations as the name is always auto-generated")]
  public string? ConnectionName { get; set; }

  [Description("Connection string (required only for Connect operation) - Can connect to PowerBI Desktop, XML/A endpoint, or Analysis Services")]
  public string? ConnectionString { get; set; }

  [Description("Local server (Desktop) instance, XML/A endpoint, or on-prem SSAS server, e.g. localhost:2383, powerbi://api.powerbi.com/v1.0/myorg/workspace, or server.domain.com")]
  public string? DataSource { get; set; }

  [Description("Dataset name or database name")]
  public string? InitialCatalog { get; set; }

  [Description("Old connection name (required for RenameConnection operation)")]
  public string? OldConnectionName { get; set; }

  [Description("New connection name (required for RenameConnection operation)")]
  public string? NewConnectionName { get; set; }

  [Description("Fabric workspace name (required for ConnectFabric operation) - exact name match")]
  public string? WorkspaceName { get; set; }

  [Description("Fabric semantic model name (required for ConnectFabric operation) - exact name match")]
  public string? SemanticModelName { get; set; }

  [Description("Tenant name for B2B/guest user scenarios (optional for ConnectFabric operation) - defaults to 'myorg'")]
  public string? TenantName { get; set; }

  [Description("Whether to clear cached credentials before connecting (forces fresh authentication)")]
  public bool ClearCredential { get; set; } = true;

  [Description("Folder path for ConnectFolder operation - must contain database.tmdl either directly or in a 'definition' subfolder")]
  public string? FolderPath { get; set; }
}
