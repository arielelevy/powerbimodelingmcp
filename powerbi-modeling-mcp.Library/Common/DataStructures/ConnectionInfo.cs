// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using Microsoft.AnalysisServices.AdomdClient;
using Microsoft.AnalysisServices.Tabular;
using PowerBIModelingMCP.Library.Core;
using System;
using System.Data;
using System.Globalization;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class ConnectionInfo
{
  private int? _cachedSpid;

  public required string ConnectionName { get; set; }

  public Server? TabularServer { get; set; }

  public required Database Database { get; set; }

  public string? ServerConnectionString { get; set; }

  public bool IsCloudConnection { get; set; }

  public string? AuthenticationContext { get; set; }

  public bool IsOffline { get; set; }

  public string? TmdlFolderPath { get; set; }

  public AdomdConnection? AdomdConnection { get; set; }

  public DateTime? ConnectedAt { get; set; }

  public DateTime? LastUsedAt { get; set; }

  public DateTime? LastSynced { get; set; }

  public TransactionContext? Transaction { get; set; }

  public TraceContext? Trace { get; set; }

  public bool IsLLMCreated { get; set; }

  public string? SessionId
  {
    get
    {
      if (this.IsOffline)
        return (string) null;
      if (this.AdomdConnection == null)
        return (string) null;
      try
      {
        if (this.AdomdConnection.State == (ConnectionState)1)
          return this.AdomdConnection.SessionID;
      }
      catch
      {
      }
      return (string) null;
    }
  }

  public int? Spid
  {
    get
    {
      if (this.IsOffline || this.AdomdConnection == null)
        return new int?();
      if (this._cachedSpid.HasValue)
        return this._cachedSpid;
      try
      {
        if (this.AdomdConnection.State == (ConnectionState)1)
        {
          this._cachedSpid = this.RetrieveSpid();
          return this._cachedSpid;
        }
      }
      catch
      {
      }
      return new int?();
    }
  }

  private int? RetrieveSpid()
  {
    try
    {
      DataSet schemaDataSet = this.AdomdConnection.GetSchemaDataSet("DISCOVER_SESSIONS", (AdomdRestrictionCollection) null);
      if (schemaDataSet == null || ((InternalDataCollectionBase) schemaDataSet.Tables).Count == 0)
        return new int?();
      string sessionId = this.AdomdConnection.SessionID;
      foreach (DataRow row in (InternalDataCollectionBase) schemaDataSet.Tables[0].Rows)
      {
        if (string.Equals(row["SESSION_ID"].ToString(), sessionId, StringComparison.OrdinalIgnoreCase))
          return new int?(int.Parse(row["SESSION_SPID"].ToString(), (IFormatProvider) CultureInfo.InvariantCulture));
      }
    }
    catch
    {
    }
    return new int?();
  }
}
