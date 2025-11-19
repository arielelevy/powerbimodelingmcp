// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Tools.ConnectionOperationsTool
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using Microsoft.Extensions.Logging;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using PowerBIModelingMCP.Library.Common;
using PowerBIModelingMCP.Library.Common.DataStructures;
using PowerBIModelingMCP.Library.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

#nullable enable
namespace PowerBIModelingMCP.Library.Tools;

[McpServerToolType]
public class ConnectionOperationsTool
{
  private readonly ILogger<ConnectionOperationsTool> _logger;
  public static readonly ToolMetadata toolMetadata;

  public ConnectionOperationsTool(ILogger<ConnectionOperationsTool> logger)
  {
    this._logger = logger;
  }

  [McpServerTool(Name = "connection_operations")]
  [Description("Perform operations on Microsoft tabular semantic model data source connections (PowerBI Desktop, Analysis Services, Fabric). Supported operations: Help, Connect, ConnectFabric, Disconnect, GetConnection, ListConnections, ListLocalInstances, RenameConnection, ClearLastUsed, GetLastUsed, SetLastUsed. Use the Operation parameter to specify which operation to perform.")]
  public ConnectionOperationResponse ExecuteConnectionOperation(
    McpServer mcpServer,
    ConnectionOperationRequest request)
  {
    this._logger.LogDebug("Executing {ToolName}.{Operation}: ConnectionName={ConnectionName}", (object) nameof (ConnectionOperationsTool), (object) request.Operation, (object) (request.ConnectionName ?? "(none)"));
    try
    {
      string[] operations = new string[12]
      {
        "HELP",
        "CONNECT",
        "CONNECTFABRIC",
        "CONNECTFOLDER",
        "DISCONNECT",
        "GETCONNECTION",
        "LISTCONNECTIONS",
        "LISTLOCALINSTANCES",
        "RENAMECONNECTION",
        "CLEARLASTUSED",
        "GETLASTUSED",
        "SETLASTUSED"
      };
      if (!Enumerable.Contains<string>((IEnumerable<string>) operations, request.Operation.ToUpperInvariant()))
      {
        this._logger.LogWarning("Invalid operation '{Operation}' requested for {ToolName}. Valid operations: {ValidOperations}", (object) request.Operation, (object) nameof (ConnectionOperationsTool), (object) string.Join(", ", operations));
        return new ConnectionOperationResponse()
        {
          Success = false,
          Message = $"Invalid operation: {request.Operation}. Supported operations: {string.Join(", ", operations)}",
          Operation = request.Operation
        };
      }
      string str = this.ValidateRequest(request.Operation, request) ? request.Operation.ToUpperInvariant() : throw new McpException($"Invalid request for {request.Operation} operation.");
      ConnectionOperationResponse operationResponse;
      if (str != null)
      {
        switch (str.Length)
        {
          case 4:
            if ((str == "HELP"))
            {
              operationResponse = this.HandleHelpOperation(request, operations);
              goto label_34;
            }
            break;
          case 7:
            if ((str == "CONNECT"))
            {
              operationResponse = this.HandleConnectOperation(request);
              goto label_34;
            }
            break;
          case 10:
            if ((str == "DISCONNECT"))
            {
              operationResponse = this.HandleDisconnectOperation(request);
              goto label_34;
            }
            break;
          case 11:
            switch (str[0])
            {
              case 'G':
                if ((str == "GETLASTUSED"))
                {
                  operationResponse = this.HandleGetLastUsedOperation(request);
                  goto label_34;
                }
                break;
              case 'S':
                if ((str == "SETLASTUSED"))
                {
                  operationResponse = this.HandleSetLastUsedOperation(request);
                  goto label_34;
                }
                break;
            }
            break;
          case 13:
            switch (str[8])
            {
              case 'A':
                if ((str == "CONNECTFABRIC"))
                {
                  operationResponse = this.HandleConnectFabricOperation(request);
                  goto label_34;
                }
                break;
              case 'C':
                if ((str == "GETCONNECTION"))
                {
                  operationResponse = this.HandleGetConnectionOperation(request);
                  goto label_34;
                }
                break;
              case 'O':
                if ((str == "CONNECTFOLDER"))
                {
                  operationResponse = this.HandleConnectFolderOperation(request);
                  goto label_34;
                }
                break;
              case 'T':
                if ((str == "CLEARLASTUSED"))
                {
                  operationResponse = this.HandleClearLastUsedOperation(request);
                  goto label_34;
                }
                break;
            }
            break;
          case 15:
            if ((str == "LISTCONNECTIONS"))
            {
              operationResponse = this.HandleListConnectionsOperation(request);
              goto label_34;
            }
            break;
          case 16 /*0x10*/:
            if ((str == "RENAMECONNECTION"))
            {
              operationResponse = this.HandleRenameConnectionOperation(request);
              goto label_34;
            }
            break;
          case 18:
            if ((str == "LISTLOCALINSTANCES"))
            {
              operationResponse = this.HandleListLocalInstancesOperation(request);
              goto label_34;
            }
            break;
        }
      }
      operationResponse = new ConnectionOperationResponse()
      {
        Success = false,
        Message = $"Operation {request.Operation} is not implemented",
        Operation = request.Operation
      };
label_34:
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Error executing {ToolName}.{Operation}: {ErrorMessage}", (object) nameof (ConnectionOperationsTool), (object) request.Operation, (object) ex.Message);
      return new ConnectionOperationResponse()
      {
        Success = false,
        Message = "Error executing connection operation: " + ex.Message,
        Operation = request.Operation
      };
    }
  }

  private ConnectionOperationResponse HandleConnectOperation(ConnectionOperationRequest request)
  {
    try
    {
      string serverConnectionString = request.ConnectionString;
      if (string.IsNullOrWhiteSpace(serverConnectionString))
      {
        string dataSource = !string.IsNullOrWhiteSpace(request.DataSource) ? request.DataSource.Trim() : throw new McpException("Either ConnectionString or DataSource must be supplied.");
        if (dataSource.Contains("Desktop", StringComparison.OrdinalIgnoreCase) || !dataSource.Contains(":") && !dataSource.StartsWith("powerbi://", StringComparison.OrdinalIgnoreCase))
          throw new McpException("A valid connection string cannot be generated automatically. Use ListLocalInstances to discover the exact Data Source and Initial Catalog.");
        serverConnectionString = ConnectionOperations.BuildConnectionString(dataSource, request.InitialCatalog);
      }
      string result = ConnectionOperations.Connect(serverConnectionString, request.ClearCredential).GetAwaiter().GetResult();
      this._logger.LogInformation("{ToolName}.{Operation} completed: ConnectionName={ConnectionName}, DataSource={DataSource}", (object) nameof (ConnectionOperationsTool), (object) "Connect", (object) result, (object) (request.DataSource ?? "(from connection string)"));
      return new ConnectionOperationResponse()
      {
        Success = true,
        Message = $"Connection '{result}' established successfully",
        Operation = request.Operation,
        Data = (object) result
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      ConnectionOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new ConnectionOperationResponse()
      {
        Success = false,
        Message = "Error connecting: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private ConnectionOperationResponse HandleConnectFabricOperation(
    ConnectionOperationRequest request)
  {
    try
    {
      string workspaceName = request.WorkspaceName;
      string tenantName = request.TenantName ?? "myorg";
      string dataSource = ConnectionOperations.BuildPowerBiXmlaEndpoint(workspaceName, tenantName);
      string str1 = ConnectionOperations.BuildConnectionString(dataSource, request.SemanticModelName);
      string result = ConnectionOperations.Connect(str1, request.ClearCredential).GetAwaiter().GetResult();
      var data = new
      {
        ConnectionName = result,
        WorkspaceName = workspaceName,
        SemanticModelName = request.SemanticModelName,
        TenantName = tenantName,
        XmlaEndpoint = dataSource,
        ConnectionString = ConnectionOperationsTool.MaskSensitiveConnectionString(str1)
      };
      string str2;
      if (string.IsNullOrWhiteSpace(request.SemanticModelName))
        str2 = $"Connected to Fabric workspace '{workspaceName}' as '{result}'";
      else
        str2 = $"Connected to Fabric workspace '{workspaceName}' semantic model '{request.SemanticModelName}' as '{result}'";
      string str3 = str2;
      this._logger.LogInformation("{ToolName}.{Operation} completed: Workspace={WorkspaceName}, Model={SemanticModelName}, ConnectionName={ConnectionName}", (object) nameof (ConnectionOperationsTool), (object) "ConnectFabric", (object) request.WorkspaceName, (object) (request.SemanticModelName ?? "(none)"), (object) result);
      return new ConnectionOperationResponse()
      {
        Success = true,
        Message = str3,
        Operation = request.Operation,
        Data = (object) data
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      ConnectionOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new ConnectionOperationResponse()
      {
        Success = false,
        Message = "Error connecting to Fabric: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private ConnectionOperationResponse HandleConnectFolderOperation(
    ConnectionOperationRequest request)
  {
    try
    {
      if (string.IsNullOrWhiteSpace(request.FolderPath))
        throw new McpException("FolderPath is required for ConnectFolder operation");
      TmdlDeserializeResult deserializeResult = string.IsNullOrWhiteSpace(request.ConnectionName) ? ConnectionOperations.ConnectFolder(request.FolderPath) : throw new McpException("ConnectionName cannot be specified for ConnectFolder operation - it is auto-generated");
      this._logger.LogInformation("{ToolName}.{Operation} completed: ConnectionName={ConnectionName}, DatabaseName={DatabaseName}, FolderPath={FolderPath}", (object) nameof (ConnectionOperationsTool), (object) "ConnectFolder", (object) deserializeResult.ConnectionName, (object) deserializeResult.DatabaseName, (object) request.FolderPath);
      return new ConnectionOperationResponse()
      {
        Success = true,
        Message = deserializeResult.Message,
        Operation = request.Operation,
        Data = (object) new
        {
          ConnectionName = deserializeResult.ConnectionName,
          DatabaseName = deserializeResult.DatabaseName,
          FolderPath = deserializeResult.FolderPath,
          TablesLoaded = deserializeResult.TablesLoaded,
          MeasuresLoaded = deserializeResult.MeasuresLoaded,
          RelationshipsLoaded = deserializeResult.RelationshipsLoaded,
          LoadedAt = deserializeResult.LoadedAt
        }
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      ConnectionOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new ConnectionOperationResponse()
      {
        Success = false,
        Message = "Error connecting to folder: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private static string MaskSensitiveConnectionString(string connectionString)
  {
    return string.IsNullOrWhiteSpace(connectionString) ? connectionString : Regex.Replace(connectionString, "Password=[^;]+", "Password=***", (RegexOptions) 1);
  }

  private ConnectionOperationResponse HandleDisconnectOperation(ConnectionOperationRequest request)
  {
    try
    {
      ConnectionOperations.Disconnect(request.ConnectionName);
      string str = request.ConnectionName != null ? $"Disconnected from connection '{request.ConnectionName}'" : "Disconnected all connections";
      this._logger.LogInformation("{ToolName}.{Operation} completed: ConnectionName={ConnectionName}", (object) nameof (ConnectionOperationsTool), (object) "Disconnect", (object) (request.ConnectionName ?? "(all)"));
      return new ConnectionOperationResponse()
      {
        Success = true,
        Message = str,
        Operation = request.Operation
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      ConnectionOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new ConnectionOperationResponse()
      {
        Success = false,
        Message = "Error disconnecting: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private ConnectionOperationResponse HandleListLocalInstancesOperation(
    ConnectionOperationRequest request)
  {
    try
    {
      List<LocalAnalysisServicesInstance> servicesInstanceList = ConnectionOperations.ListLocalAnalysisServicesInstances();
      this._logger.LogInformation("{ToolName}.{Operation} completed: Count={Count}", (object) nameof (ConnectionOperationsTool), (object) "ListLocalInstances", (object) servicesInstanceList.Count);
      ConnectionOperationResponse operationResponse = new ConnectionOperationResponse { Success = true };
      operationResponse.Message = $"Found {servicesInstanceList.Count} local PowerBI Desktop and Analysis Services instances";
      operationResponse.Operation = request.Operation;
      operationResponse.Data = (object) servicesInstanceList;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      ConnectionOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new ConnectionOperationResponse()
      {
        Success = false,
        Message = "Error listing local instances: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private ConnectionOperationResponse HandleListConnectionsOperation(
    ConnectionOperationRequest request)
  {
    try
    {
      List<ConnectionGet> connectionGetList = ConnectionOperations.ListConnections();
      this._logger.LogInformation("{ToolName}.{Operation} completed: Count={Count}", (object) nameof (ConnectionOperationsTool), (object) "ListConnections", (object) connectionGetList.Count);
      ConnectionOperationResponse operationResponse = new ConnectionOperationResponse { Success = true };
      operationResponse.Message = $"Found {connectionGetList.Count} connections";
      operationResponse.Operation = request.Operation;
      operationResponse.Data = (object) connectionGetList;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      ConnectionOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new ConnectionOperationResponse()
      {
        Success = false,
        Message = "Error listing connections: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private ConnectionOperationResponse HandleGetConnectionOperation(
    ConnectionOperationRequest request)
  {
    try
    {
      ConnectionGet connection = ConnectionOperations.GetConnection(request.ConnectionName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: ConnectionName={ConnectionName}", (object) nameof (ConnectionOperationsTool), (object) "GetConnection", (object) request.ConnectionName);
      return new ConnectionOperationResponse()
      {
        Success = true,
        Message = $"Connection '{request.ConnectionName}' details retrieved successfully",
        Operation = request.Operation,
        Data = (object) connection
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      ConnectionOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new ConnectionOperationResponse()
      {
        Success = false,
        Message = ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private ConnectionOperationResponse HandleRenameConnectionOperation(
    ConnectionOperationRequest request)
  {
    try
    {
      ConnectionOperations.RenameConnection(request.OldConnectionName, request.NewConnectionName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: From={OldName}, To={NewName}", (object) nameof (ConnectionOperationsTool), (object) "RenameConnection", (object) request.OldConnectionName, (object) request.NewConnectionName);
      ConnectionOperationResponse operationResponse = new ConnectionOperationResponse { Success = true };
      operationResponse.Message = $"Connection renamed from '{request.OldConnectionName}' to '{request.NewConnectionName}'";
      operationResponse.Operation = request.Operation;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      ConnectionOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new ConnectionOperationResponse()
      {
        Success = false,
        Message = "Error renaming connection: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private ConnectionOperationResponse HandleGetLastUsedOperation(ConnectionOperationRequest request)
  {
    try
    {
      ConnectionGet lastUsedConnection = ConnectionOperations.GetLastUsedConnection();
      if (lastUsedConnection == null)
      {
        this._logger.LogInformation("{ToolName}.{Operation} completed: No last used connection", (object) nameof (ConnectionOperationsTool), (object) "GetLastUsed");
        return new ConnectionOperationResponse()
        {
          Success = true,
          Message = "No last used connection",
          Operation = request.Operation,
          Data = (object) null
        };
      }
      this._logger.LogInformation("{ToolName}.{Operation} completed: ConnectionName={ConnectionName}", (object) nameof (ConnectionOperationsTool), (object) "GetLastUsed", (object) lastUsedConnection.ConnectionName);
      return new ConnectionOperationResponse()
      {
        Success = true,
        Message = $"Last used connection: '{lastUsedConnection.ConnectionName}'",
        Operation = request.Operation,
        Data = (object) lastUsedConnection
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      ConnectionOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new ConnectionOperationResponse()
      {
        Success = false,
        Message = "Error getting last used connection: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private ConnectionOperationResponse HandleSetLastUsedOperation(ConnectionOperationRequest request)
  {
    try
    {
      ConnectionOperations.SetLastUsedConnection(request.ConnectionName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: ConnectionName={ConnectionName}", (object) nameof (ConnectionOperationsTool), (object) "SetLastUsed", (object) request.ConnectionName);
      return new ConnectionOperationResponse()
      {
        Success = true,
        Message = $"Last used connection set to '{request.ConnectionName}'",
        Operation = request.Operation
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      ConnectionOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new ConnectionOperationResponse()
      {
        Success = false,
        Message = "Error setting last used connection: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private ConnectionOperationResponse HandleClearLastUsedOperation(
    ConnectionOperationRequest request)
  {
    try
    {
      ConnectionOperations.ClearLastUsedConnection();
      this._logger.LogInformation("{ToolName}.{Operation} completed", (object) nameof (ConnectionOperationsTool), (object) "ClearLastUsed");
      return new ConnectionOperationResponse()
      {
        Success = true,
        Message = "Last used connection cleared",
        Operation = request.Operation
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      ConnectionOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new ConnectionOperationResponse()
      {
        Success = false,
        Message = "Error clearing last used connection: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private ConnectionOperationResponse HandleHelpOperation(
    ConnectionOperationRequest request,
    string[] operations)
  {
    this._logger.LogInformation("{ToolName}.{Operation} completed: Operations={OperationCount}", (object) nameof (ConnectionOperationsTool), (object) "Help", (object) operations.Length);
    return new ConnectionOperationResponse()
    {
      Success = true,
      Message = "Tool description retrieved successfully",
      Operation = request.Operation,
      Help = (object) new
      {
        ToolName = "connection_operations",
        Description = "Perform operations on Microsoft tabular semantic model data source connections (PowerBI Desktop, Analysis Services, Fabric).",
        SupportedOperations = operations,
        Examples = Enumerable.Where<KeyValuePair<string, OperationMetadata>>((IEnumerable<KeyValuePair<string, OperationMetadata>>) ConnectionOperationsTool.toolMetadata.Operations, (Func<KeyValuePair<string, OperationMetadata>, bool>) (p => Enumerable.Contains<string>((IEnumerable<string>) operations, p.Key, (IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase))),
        Notes = new string[2]
        {
          "Connection names are case-insensitive and must be unique.",
          "Connection strings are case-insensitive and must be unique."
        }
      }
    };
  }

  private bool ValidateRequest(string operation, ConnectionOperationRequest request)
  {
    OperationMetadata operationMetadata;
    if (!ConnectionOperationsTool.toolMetadata.Operations.TryGetValue(operation, out operationMetadata))
      return true;
    JsonObject requestDict = JsonSerializer.SerializeToNode<ConnectionOperationRequest>(request) as JsonObject;
    List<string> list1 = Enumerable.ToList<string>(Enumerable.Where<string>((IEnumerable<string>) operationMetadata.RequiredParams, (p => requestDict != null && requestDict[p] == null)));
    List<string> list2 = Enumerable.ToList<string>(Enumerable.Where<string>((IEnumerable<string>) operationMetadata.ForbiddenParams, (p => requestDict != null && requestDict[p] != null)));
    if (Enumerable.Any<string>((IEnumerable<string>) list1))
      throw new McpException($"Missing required parameters needed for {operation} operation: {string.Join(", ", (IEnumerable<string>) list1)}");
    if (Enumerable.Any<string>((IEnumerable<string>) list2))
      throw new McpException($"Forbidden parameters not allowed for {operation} operation: {string.Join(", ", (IEnumerable<string>) list2)}");
    return true;
  }

  static ConnectionOperationsTool()
  {
    ToolMetadata toolMetadata1 = new ToolMetadata();
    ToolMetadata toolMetadata2 = toolMetadata1;
    Dictionary<string, OperationMetadata> dictionary1 = new Dictionary<string, OperationMetadata>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    Dictionary<string, OperationMetadata> dictionary2 = dictionary1;
    OperationMetadata operationMetadata1 = new OperationMetadata { RequiredParams = Array.Empty<string>() };
    operationMetadata1.ForbiddenParams = new string[1]
    {
      "ConnectionName"
    };
    operationMetadata1.Description = "Establishes a connection to a Microsoft tabular semantic model data source (PowerBI Desktop, Fabric XML/A endpoint, Analysis Services).\r\nMandatory properties: Either ConnectionString OR DataSource.\r\nOptional: InitialCatalog (when using DataSource).";
    OperationMetadata operationMetadata2 = operationMetadata1;
    List<string> stringList1 = new List<string>();
    stringList1.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Connect\",\r\n        \"ConnectionString\": \"Provider=MSOLAP;Data Source=localhost:12345\"\r\n    }\r\n}");
    stringList1.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Connect\",\r\n        \"ConnectionString\": \"Data Source=powerbi://api.fabric.microsoft.com/v1.0/myorg/MyWorkspace;Initial Catalog=MyDataset;\"\r\n    }\r\n}");
    operationMetadata2.ExampleRequests = stringList1;
    OperationMetadata operationMetadata3 = operationMetadata1;
    dictionary2["Connect"] = operationMetadata3;
    Dictionary<string, OperationMetadata> dictionary3 = dictionary1;
    OperationMetadata operationMetadata4 = new OperationMetadata { RequiredParams = new string[2]
    {
      "WorkspaceName",
      "SemanticModelName"
    } };
    operationMetadata4.ForbiddenParams = new string[1]
    {
      "ConnectionName"
    };
    operationMetadata4.Description = "Connects to a Microsoft Fabric workspace and semantic model using natural workspace names.\r\nMandatory properties: WorkspaceName, SemanticModelName.\r\nOptional: TenantName (defaults to 'myorg'), ClearCredential (defaults to true).";
    OperationMetadata operationMetadata5 = operationMetadata4;
    List<string> stringList2 = new List<string>();
    stringList2.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"ConnectFabric\",\r\n        \"WorkspaceName\": \"My Premium Space\",\r\n        \"SemanticModelName\": \"SalesModel\"\r\n    }\r\n}");
    stringList2.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"ConnectFabric\",\r\n        \"WorkspaceName\": \"My Premium Space\",\r\n        \"SemanticModelName\": \"Experiment2\",\r\n        \"TenantName\": \"contoso.com\"\r\n    }\r\n}");
    stringList2.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"ConnectFabric\",\r\n        \"WorkspaceName\": \"My Premium Space\",\r\n        \"SemanticModelName\": \"SalesModel\",\r\n        \"ClearCredential\": false\r\n    }\r\n}");
    operationMetadata5.ExampleRequests = stringList2;
    OperationMetadata operationMetadata6 = operationMetadata4;
    dictionary3["ConnectFabric"] = operationMetadata6;
    Dictionary<string, OperationMetadata> dictionary4 = dictionary1;
    OperationMetadata operationMetadata7 = new OperationMetadata { RequiredParams = new string[1]
    {
      "FolderPath"
    } };
    operationMetadata7.ForbiddenParams = new string[1]
    {
      "ConnectionName"
    };
    operationMetadata7.Description = "Connects to a folder that contains the database.tmdl file.\r\nFirst checks if database.tmdl exists in the provided folder. If not found, checks in the 'definition' subfolder.\r\nMandatory properties: FolderPath.\r\nOptional: None (ConnectionName is auto-generated).";
    OperationMetadata operationMetadata8 = operationMetadata7;
    List<string> stringList3 = new List<string>();
    stringList3.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"ConnectFolder\",\r\n        \"FolderPath\": \"C:\\\\pbip\\\\Contoso.SemanticModel\"\r\n    }\r\n}");
    stringList3.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"ConnectFolder\",\r\n        \"FolderPath\": \"C:\\\\pbip\\\\Contoso.SemanticModel\\\\definition\"\r\n    }\r\n}");
    operationMetadata8.ExampleRequests = stringList3;
    OperationMetadata operationMetadata9 = operationMetadata7;
    dictionary4["ConnectFolder"] = operationMetadata9;
    Dictionary<string, OperationMetadata> dictionary5 = dictionary1;
    OperationMetadata operationMetadata10 = new OperationMetadata { RequiredParams = Array.Empty<string>() };
    operationMetadata10.Description = "Disconnects from a specific connection or all connections if no ConnectionName is specified.\r\nMandatory properties: None.\r\nOptional: ConnectionName (if omitted, disconnects all connections).";
    List<string> stringList4 = new List<string>();
    stringList4.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Disconnect\",\r\n        \"ConnectionName\": \"LocalPbi\"\r\n    }\r\n}");
    operationMetadata10.ExampleRequests = stringList4;
    dictionary5["Disconnect"] = operationMetadata10;
    Dictionary<string, OperationMetadata> dictionary6 = dictionary1;
    OperationMetadata operationMetadata11 = new OperationMetadata { RequiredParams = new string[1]
    {
      "ConnectionName"
    } };
    operationMetadata11.Description = "Retrieves detailed information about a specific connection including server details, database name, and connection status.\r\nMandatory properties: ConnectionName.\r\nOptional: None.";
    OperationMetadata operationMetadata12 = operationMetadata11;
    List<string> stringList5 = new List<string>();
    stringList5.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"GetConnection\",\r\n        \"ConnectionName\": \"LocalPbi\"\r\n    }\r\n}");
    operationMetadata12.ExampleRequests = stringList5;
    OperationMetadata operationMetadata13 = operationMetadata11;
    dictionary6["GetConnection"] = operationMetadata13;
    Dictionary<string, OperationMetadata> dictionary7 = dictionary1;
    OperationMetadata operationMetadata14 = new OperationMetadata { Description = "Lists all active connections with detailed information including connection name, server details, database name, and folder path for offline connections.\r\nMandatory properties: None.\r\nOptional: None." };
    operationMetadata14.Tips = new string[2]
    {
      "Offline connections are created as a byproduct of creating empty database/model or deserializing database from TMDL files",
      "Offline connections show a folder path instead of server details"
    };
    OperationMetadata operationMetadata15 = operationMetadata14;
    List<string> stringList6 = new List<string>();
    stringList6.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"ListConnections\"\r\n    }\r\n}");
    operationMetadata15.ExampleRequests = stringList6;
    OperationMetadata operationMetadata16 = operationMetadata14;
    dictionary7["ListConnections"] = operationMetadata16;
    Dictionary<string, OperationMetadata> dictionary8 = dictionary1;
    OperationMetadata operationMetadata17 = new OperationMetadata { Description = "Discovers and lists all local PowerBI Desktop and Analysis Services instances running on the local machine with connection details.\r\nMandatory properties: None.\r\nOptional: None." };
    List<string> stringList7 = new List<string>();
    stringList7.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"ListLocalInstances\"\r\n    }\r\n}");
    operationMetadata17.ExampleRequests = stringList7;
    dictionary8["ListLocalInstances"] = operationMetadata17;
    Dictionary<string, OperationMetadata> dictionary9 = dictionary1;
    OperationMetadata operationMetadata18 = new OperationMetadata { RequiredParams = new string[2]
    {
      "OldConnectionName",
      "NewConnectionName"
    } };
    operationMetadata18.Description = "Renames an existing connection from one name to another while preserving all connection details and state.\r\nMandatory properties: OldConnectionName, NewConnectionName.\r\nOptional: None.";
    OperationMetadata operationMetadata19 = operationMetadata18;
    List<string> stringList8 = new List<string>();
    stringList8.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"RenameConnection\",\r\n        \"OldConnectionName\": \"OldConn\",\r\n        \"NewConnectionName\": \"NewConn\"\r\n    }\r\n}");
    operationMetadata19.ExampleRequests = stringList8;
    OperationMetadata operationMetadata20 = operationMetadata18;
    dictionary9["RenameConnection"] = operationMetadata20;
    Dictionary<string, OperationMetadata> dictionary10 = dictionary1;
    OperationMetadata operationMetadata21 = new OperationMetadata { Description = "Clears the last used connection setting, requiring explicit ConnectionName for future operations until a new connection is established or set.\r\nMandatory properties: None.\r\nOptional: None." };
    List<string> stringList9 = new List<string>();
    stringList9.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"ClearLastUsed\"\r\n    }\r\n}");
    operationMetadata21.ExampleRequests = stringList9;
    dictionary10["ClearLastUsed"] = operationMetadata21;
    Dictionary<string, OperationMetadata> dictionary11 = dictionary1;
    OperationMetadata operationMetadata22 = new OperationMetadata { Description = "Retrieves details of the last used connection, or returns null if no last used connection is available.\r\nMandatory properties: None.\r\nOptional: None." };
    List<string> stringList10 = new List<string>();
    stringList10.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"GetLastUsed\"\r\n    }\r\n}");
    operationMetadata22.ExampleRequests = stringList10;
    dictionary11["GetLastUsed"] = operationMetadata22;
    Dictionary<string, OperationMetadata> dictionary12 = dictionary1;
    OperationMetadata operationMetadata23 = new OperationMetadata { RequiredParams = new string[1]
    {
      "ConnectionName"
    } };
    operationMetadata23.Description = "Sets the specified connection as the last used connection, making it the default for operations that don't specify a ConnectionName.\r\nMandatory properties: ConnectionName.\r\nOptional: None.";
    OperationMetadata operationMetadata24 = operationMetadata23;
    List<string> stringList11 = new List<string>();
    stringList11.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"SetLastUsed\",\r\n        \"ConnectionName\": \"LocalPbi\"\r\n    }\r\n}");
    operationMetadata24.ExampleRequests = stringList11;
    OperationMetadata operationMetadata25 = operationMetadata23;
    dictionary12["SetLastUsed"] = operationMetadata25;
    Dictionary<string, OperationMetadata> dictionary13 = dictionary1;
    OperationMetadata operationMetadata26 = new OperationMetadata { Description = "Describe the tool and its operations." };
    List<string> stringList12 = new List<string>();
    stringList12.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Help\"\r\n    }\r\n}");
    operationMetadata26.ExampleRequests = stringList12;
    dictionary13["Help"] = operationMetadata26;
    Dictionary<string, OperationMetadata> dictionary14 = dictionary1;
    toolMetadata2.Operations = dictionary14;
    ConnectionOperationsTool.toolMetadata = toolMetadata1;
  }
}
