// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.DataStructures.DeployToFabricRequest
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using System.ComponentModel;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class DeployToFabricRequest
{
  [Description("Direct connection string to target Fabric workspace XMLA endpoint")]
  public string? TargetConnectionString { get; set; }

  [Description("Target Fabric workspace name")]
  public string? TargetWorkspaceName { get; set; }

  [Description("Target tenant name (defaults to 'myorg')")]
  public string? TargetTenantName { get; set; }

  [Description("Optional new database name for the deployed model")]
  public string? NewDatabaseName { get; set; }

  [Description("Whether to include restricted properties in TMSL script")]
  public bool? IncludeRestricted { get; set; }

  [Description("Connection timeout in seconds for target connection")]
  public int? ConnectTimeoutSeconds { get; set; }
}
