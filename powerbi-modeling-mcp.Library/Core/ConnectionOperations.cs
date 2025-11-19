// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using Microsoft.AnalysisServices;
using Microsoft.AnalysisServices.AdomdClient;
using ModelContextProtocol;
using PowerBIModelingMCP.Library.Common.DataStructures;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

#nullable enable
namespace PowerBIModelingMCP.Library.Core;

public static class ConnectionOperations
{
  private static readonly ConcurrentDictionary<string, PowerBIModelingMCP.Library.Common.DataStructures.ConnectionInfo> _namedConnections = new ConcurrentDictionary<string, PowerBIModelingMCP.Library.Common.DataStructures.ConnectionInfo>();
  private static string? _lastUsedConnectionName = (string) null;

  public static async Task<string> Connect(string serverConnectionString, bool clearCredential)
  {
    (bool isPBIDesktopInstance, string str1) = !string.IsNullOrWhiteSpace(serverConnectionString) ? ConnectionOperations.GenerateConnectionNameFromConnectionString(serverConnectionString) : throw new McpException("serverConnectionString is required and cannot be empty");
    ConnectionOperations.ValidateConnectionName(str1);
    string enhancedConnectionString = ConnectionOperations.AddApplicationNameToConnectionString(serverConnectionString);
    bool isCloudConnection = ConnectionOperations.IsFabricConnectionString(enhancedConnectionString);
    int num = isCloudConnection ? 1 : 0;
    AccessToken? nullable = new AccessToken?();
    if (num != 0)
      nullable = new AccessToken?(new AccessToken(await AuthService.GetAccessTokenAsync(clearCredential: clearCredential), DateTimeOffset.UtcNow.AddHours(1.0)));
    AdomdConnection adomdConnection = new AdomdConnection(enhancedConnectionString);
    if (nullable.HasValue)
      adomdConnection.AccessToken = nullable.Value;
    try
    {
      adomdConnection.Open();
    }
    catch (Exception ex)
    {
      Console.Error.WriteLine($"Exception caught AdomdClient.AdomdConnection.Open: {ex}");
      throw new McpException("Failed to open ADOMD connection: " + ex.Message);
    }
    string sessionId = adomdConnection.SessionID;
    if (string.IsNullOrWhiteSpace(sessionId))
      Console.Error.WriteLine("Warning: ADOMD connection opened but SessionID is null or empty. Proceeding without session sharing.");
    string connectionString1 = string.IsNullOrWhiteSpace(sessionId) ? enhancedConnectionString : ConnectionOperations.AddSessionIdToConnectionString(enhancedConnectionString, sessionId);
    Microsoft.AnalysisServices.Tabular.Server server = new Microsoft.AnalysisServices.Tabular.Server();
    if (nullable.HasValue)
      server.AccessToken = nullable.Value;
    try
    {
      server.Connect(connectionString1);
    }
    catch (Exception ex1)
    {
      Console.Error.WriteLine($"Exception caught Tabular.Server.Connect: {ex1}");
      Exception exception = ex1;
      if (ConnectionOperations.IsLikelyDataSourceUrl(serverConnectionString))
      {
        try
        {
          string connectionString2 = ConnectionOperations.AddApplicationNameToConnectionString(ConnectionOperations.BuildConnectionString(serverConnectionString, (string) null));
          if (!string.IsNullOrWhiteSpace(sessionId))
            connectionString2 = ConnectionOperations.AddSessionIdToConnectionString(connectionString2, sessionId);
          server.Connect(connectionString2);
          enhancedConnectionString = connectionString2;
          exception = (Exception) null;
        }
        catch (Exception ex2)
        {
          Console.Error.WriteLine($"Exception caught on retry with Data Source treatment: {ex2}");
        }
      }
      if (exception != null)
      {
        try
        {
          adomdConnection.Close();
          adomdConnection.Dispose();
        }
        catch (Exception ex3)
        {
          Console.Error.WriteLine($"Error disposing ADOMD connection: {ex3}");
        }
        server.Disconnect();
        throw new McpException("Failed to connect to TOM Server: " + exception.Message);
      }
    }
    string databaseName = ConnectionOperations.ExtractDatabaseName(enhancedConnectionString);
    Microsoft.AnalysisServices.Tabular.Database database;
    if (databaseName != null)
    {
      database = server.Databases.FindByName(databaseName);
      if (database == null)
        throw new McpException($"Database '{databaseName}' not found");
      if (!isPBIDesktopInstance)
        str1 = $"{str1}-{databaseName}";
      str1 = ConnectionOperations.EnsureUniqueConnectionName(str1);
    }
    else
    {
      if (server.Databases.Count <= 0)
        throw new McpException("No databases found on the server");
      database = server.Databases[0];
      string name = database.Name;
      if (!isPBIDesktopInstance)
        str1 = $"{str1}-{name}";
      str1 = ConnectionOperations.EnsureUniqueConnectionName(str1);
    }
    if (ConnectionOperations._namedConnections.ContainsKey(str1))
      throw new McpException($"Connection '{str1}' already exists. Use a different name or disconnect the existing connection first.");
    PowerBIModelingMCP.Library.Common.DataStructures.ConnectionInfo connectionInfo = new PowerBIModelingMCP.Library.Common.DataStructures.ConnectionInfo()
    {
      ConnectionName = str1,
      TabularServer = server,
      Database = database,
      AdomdConnection = adomdConnection,
      ServerConnectionString = enhancedConnectionString,
      IsCloudConnection = isCloudConnection,
      ConnectedAt = new DateTime?(DateTime.UtcNow)
    };
    ConnectionOperations._namedConnections[str1] = connectionInfo;
    ConnectionOperations.SetLastUsedConnection(str1);
    string str2 = str1;
    str1 = (string) null;
    enhancedConnectionString = (string) null;
    return str2;
  }

  public static ConnectionGet GetConnection(string connectionName)
  {
    if (string.IsNullOrWhiteSpace(connectionName))
      throw new McpException("connectionName is required and cannot be empty");
    PowerBIModelingMCP.Library.Common.DataStructures.ConnectionInfo connectionInfo;
    if (!ConnectionOperations._namedConnections.TryGetValue(connectionName, out connectionInfo))
      throw new McpException($"Connection '{connectionName}' not found. Available connections: {string.Join(", ", (IEnumerable<string>) ConnectionOperations._namedConnections.Keys)}");
    return new ConnectionGet()
    {
      ConnectionName = connectionInfo.ConnectionName,
      ServerConnectionString = connectionInfo.ServerConnectionString,
      DatabaseName = connectionInfo.Database?.Name,
      ServerName = ConnectionOperations.ExtractServerName(connectionInfo.ServerConnectionString),
      IsCloudConnection = connectionInfo.IsCloudConnection,
      IsOffline = connectionInfo.IsOffline,
      TmdlFolderPath = connectionInfo.TmdlFolderPath,
      ConnectedAt = connectionInfo.ConnectedAt,
      LastUsedAt = connectionInfo.LastUsedAt,
      HasTransaction = connectionInfo.Transaction != null,
      HasTrace = connectionInfo.Trace != null,
      SessionId = string.IsNullOrWhiteSpace(connectionInfo.SessionId) ? (string) null : connectionInfo.SessionId
    };
  }

  public static PowerBIModelingMCP.Library.Common.DataStructures.ConnectionInfo Get(
    string? connectionName = null)
  {
    if (string.IsNullOrEmpty(connectionName))
    {
      connectionName = ConnectionOperations._lastUsedConnectionName;
      if (string.IsNullOrEmpty(connectionName))
        throw new McpException("No connectionName provided and no last used connection available. Please connect to a server first or specify a connection name.");
    }
    PowerBIModelingMCP.Library.Common.DataStructures.ConnectionInfo connectionInfo;
    if (!ConnectionOperations._namedConnections.TryGetValue(connectionName, out connectionInfo))
      throw new McpException($"Connection '{connectionName}' not found. Available connections: {string.Join(", ", (IEnumerable<string>) ConnectionOperations._namedConnections.Keys)}");
    ConnectionOperations.SetLastUsedConnection(connectionName);
    connectionInfo.LastUsedAt = new DateTime?(DateTime.UtcNow);
    return connectionInfo;
  }

  public static bool Exists(string connectionName)
  {
    return ConnectionOperations._namedConnections.ContainsKey(connectionName);
  }

  public static void Disconnect(string? connectionName = null)
  {
    if (string.IsNullOrEmpty(connectionName))
    {
      ConnectionOperations.DisconnectAll();
    }
    else
    {
      PowerBIModelingMCP.Library.Common.DataStructures.ConnectionInfo connectionInfo;
      if (!ConnectionOperations._namedConnections.TryRemove(connectionName, out connectionInfo))
        throw new McpException($"Connection '{connectionName}' not found. Available connections: {string.Join(", ", (IEnumerable<string>) ConnectionOperations._namedConnections.Keys)}");
      ConnectionOperations.DisposeConnection(connectionInfo);
      if (!(ConnectionOperations._lastUsedConnectionName == connectionName))
        return;
      ConnectionOperations._lastUsedConnectionName = (string) null;
    }
  }

  public static List<string> ListConnectionNames()
  {
    return Enumerable.ToList<string>((IEnumerable<string>) ConnectionOperations._namedConnections.Keys);
  }

  public static List<ConnectionGet> ListConnections()
  {
    List<ConnectionGet> connectionGetList = new List<ConnectionGet>();
    foreach (KeyValuePair<string, PowerBIModelingMCP.Library.Common.DataStructures.ConnectionInfo> namedConnection in ConnectionOperations._namedConnections)
    {
      PowerBIModelingMCP.Library.Common.DataStructures.ConnectionInfo connectionInfo = namedConnection.Value;
      ConnectionGet connectionGet = new ConnectionGet()
      {
        ConnectionName = connectionInfo.ConnectionName,
        ServerConnectionString = connectionInfo.ServerConnectionString,
        DatabaseName = connectionInfo.Database?.Name,
        ServerName = connectionInfo.TabularServer?.Name,
        IsCloudConnection = connectionInfo.IsCloudConnection,
        IsOffline = connectionInfo.IsOffline,
        TmdlFolderPath = connectionInfo.TmdlFolderPath,
        ConnectedAt = connectionInfo.ConnectedAt,
        LastUsedAt = connectionInfo.LastUsedAt,
        HasTransaction = connectionInfo.Transaction != null,
        HasTrace = connectionInfo.Trace != null,
        SessionId = string.IsNullOrWhiteSpace(connectionInfo.SessionId) ? (string) null : connectionInfo.SessionId
      };
      connectionGetList.Add(connectionGet);
    }
    return connectionGetList;
  }

  public static void RenameConnection(string oldName, string newName)
  {
    if (string.IsNullOrWhiteSpace(oldName))
      throw new McpException("oldName is required and cannot be empty");
    if (string.IsNullOrWhiteSpace(newName))
      throw new McpException("newName is required and cannot be empty");
    ConnectionOperations.ValidateConnectionName(newName);
    PowerBIModelingMCP.Library.Common.DataStructures.ConnectionInfo connectionInfo1;
    if (!ConnectionOperations._namedConnections.TryGetValue(oldName, out connectionInfo1))
      throw new McpException($"Connection '{oldName}' not found");
    if (ConnectionOperations._namedConnections.ContainsKey(newName))
      throw new McpException($"Connection '{newName}' already exists");
    PowerBIModelingMCP.Library.Common.DataStructures.ConnectionInfo connectionInfo2;
    ConnectionOperations._namedConnections.TryRemove(oldName, out connectionInfo2);
    connectionInfo1.ConnectionName = newName;
    ConnectionOperations._namedConnections[newName] = connectionInfo1;
    if (!(ConnectionOperations._lastUsedConnectionName == oldName))
      return;
    ConnectionOperations.SetLastUsedConnection(newName);
  }

  public static ConnectionGet? GetLastUsedConnection()
  {
    if (string.IsNullOrEmpty(ConnectionOperations._lastUsedConnectionName))
      return (ConnectionGet) null;
    PowerBIModelingMCP.Library.Common.DataStructures.ConnectionInfo connectionInfo;
    if (!ConnectionOperations._namedConnections.TryGetValue(ConnectionOperations._lastUsedConnectionName, out connectionInfo))
    {
      ConnectionOperations._lastUsedConnectionName = (string) null;
      return (ConnectionGet) null;
    }
    return new ConnectionGet()
    {
      ConnectionName = connectionInfo.ConnectionName,
      ServerConnectionString = connectionInfo.ServerConnectionString,
      DatabaseName = connectionInfo.Database?.Name,
      ServerName = ConnectionOperations.ExtractServerName(connectionInfo.ServerConnectionString),
      IsCloudConnection = connectionInfo.IsCloudConnection,
      IsOffline = connectionInfo.IsOffline,
      TmdlFolderPath = connectionInfo.TmdlFolderPath,
      ConnectedAt = connectionInfo.ConnectedAt,
      LastUsedAt = connectionInfo.LastUsedAt,
      HasTransaction = connectionInfo.Transaction != null,
      HasTrace = connectionInfo.Trace != null
    };
  }

  public static void SetLastUsedConnection(string connectionName)
  {
    if (string.IsNullOrWhiteSpace(connectionName))
      throw new McpException("connectionName is required and cannot be empty");
    ConnectionOperations._lastUsedConnectionName = ConnectionOperations._namedConnections.ContainsKey(connectionName) ? connectionName : throw new McpException($"Connection '{connectionName}' not found");
  }

  public static void ClearLastUsedConnection()
  {
    ConnectionOperations._lastUsedConnectionName = (string) null;
  }

  public static async Task<List<ConnectionOperations.FabricWorkspace>> ListFabricWorkspacesAsync()
  {
    await Task.CompletedTask;
    return new List<ConnectionOperations.FabricWorkspace>();
  }

  public static async Task<List<ConnectionOperations.FabricSemanticModel>> ListSemanticModelsAsync(
    string workspaceName)
  {
    if (string.IsNullOrWhiteSpace(workspaceName))
      throw new McpException("workspaceName is required and cannot be empty");
    await Task.CompletedTask;
    return new List<ConnectionOperations.FabricSemanticModel>();
  }

  public static TmdlDeserializeResult ConnectFolder(string folderPath, string? connectionName = null)
  {
    if (string.IsNullOrWhiteSpace(folderPath))
      throw new McpException("Folder path cannot be null or empty");
    string folderPath1 = Directory.Exists(folderPath) ? ConnectionOperations.ResolveDefinitionPath(folderPath) : throw new McpException("Folder does not exist: " + folderPath);
    return File.Exists(Path.Combine(folderPath1, "database.tmdl")) ? DatabaseOperations.ImportFromTmdlFolder(folderPath1, connectionName) : throw new McpException("Required file 'database.tmdl' not found in: " + folderPath1);
  }

  private static string ResolveDefinitionPath(string folderPath)
  {
    folderPath = Path.GetFullPath(folderPath);
    if (File.Exists(Path.Combine(folderPath, "database.tmdl")))
      return folderPath;
    string str1 = Path.Combine(folderPath, "definition");
    string str2 = Path.Combine(str1, "database.tmdl");
    return Directory.Exists(str1) && File.Exists(str2) ? str1 : throw new McpException($"'database.tmdl' not found in '{folderPath}' or '{str1}'");
  }

  public static List<LocalAnalysisServicesInstance> ListLocalAnalysisServicesInstances()
  {
    List<LocalAnalysisServicesInstance> servicesInstanceList = new List<LocalAnalysisServicesInstance>();
    try
    {
      Dictionary<int, int> localListenerPorts = ConnectionOperations.GetLocalListenerPorts();
      foreach (Process process in Process.GetProcessesByName("msmdsrv"))
      {
        int num;
        if (localListenerPorts.TryGetValue(process.Id, out num))
        {
          Process parentProcess = ConnectionOperations.GetParentProcess(process.Id);
          string connectionString = ConnectionOperations.AddApplicationNameToConnectionString($"Data Source=localhost:{num}");
          servicesInstanceList.Add(new LocalAnalysisServicesInstance()
          {
            ProcessId = process.Id,
            Port = num,
            ConnectionString = connectionString,
            ParentProcessName = parentProcess?.ProcessName,
            ParentWindowTitle = parentProcess?.MainWindowTitle,
            StartTime = process.StartTime
          });
        }
      }
    }
    catch (Exception ex)
    {
      throw new McpException("Failed to enumerate local Analysis Services instances: " + ex.Message);
    }
    return Enumerable.ToList<LocalAnalysisServicesInstance>(Enumerable.OrderBy<LocalAnalysisServicesInstance, int>((IEnumerable<LocalAnalysisServicesInstance>) servicesInstanceList, (i => i.Port)));
  }

  public static string CreateOfflineConnection(
    string connectionName,
    Microsoft.AnalysisServices.Tabular.Database database,
    string folderPath)
  {
    if (string.IsNullOrWhiteSpace(connectionName))
      throw new McpException("connectionName is required and cannot be empty");
    if (database == null)
      throw new McpException("database cannot be null");
    if (ConnectionOperations._namedConnections.ContainsKey(connectionName))
      throw new McpException($"Connection '{connectionName}' already exists. Use a different name or disconnect the existing connection first.");
    PowerBIModelingMCP.Library.Common.DataStructures.ConnectionInfo connectionInfo = new PowerBIModelingMCP.Library.Common.DataStructures.ConnectionInfo()
    {
      ConnectionName = connectionName,
      TabularServer = (Microsoft.AnalysisServices.Tabular.Server) null,
      Database = database,
      AdomdConnection = (AdomdConnection) null,
      ServerConnectionString = (string) null,
      IsCloudConnection = false,
      AuthenticationContext = (string) null,
      IsOffline = true,
      TmdlFolderPath = folderPath,
      ConnectedAt = new DateTime?(DateTime.UtcNow),
      IsLLMCreated = true
    };
    ConnectionOperations._namedConnections[connectionName] = connectionInfo;
    ConnectionOperations.SetLastUsedConnection(connectionName);
    return connectionName;
  }

  public static string EnsureUniqueConnectionName(string baseName)
  {
    if (!ConnectionOperations._namedConnections.ContainsKey(baseName))
      return baseName;
    int num = 2;
    string str;
    do
    {
      str = $"{baseName} {num}";
      ++num;
    }
    while (ConnectionOperations._namedConnections.ContainsKey(str));
    return str;
  }

  public static void SaveChangesIfNeeded(
    PowerBIModelingMCP.Library.Common.DataStructures.ConnectionInfo info,
    ConnectionOperations.CheckpointMode checkpointMode = ConnectionOperations.CheckpointMode.Default)
  {
    if (info.Database == null)
      throw new McpException("Database cannot be null");
    if (info.IsOffline)
      return;
    if (checkpointMode == ConnectionOperations.CheckpointMode.AfterRequestRename)
      info.Database.Model.SaveChanges();
    ModelOperations.AddProToolingAnnotation(info);
    if (TransactionOperations.IsInTransaction() && checkpointMode != ConnectionOperations.CheckpointMode.ForceEvenInTransaction)
      return;
    info.Database.Model.SaveChanges();
  }

  public static void UndoLocalChangesIfNeeded(PowerBIModelingMCP.Library.Common.DataStructures.ConnectionInfo info)
  {
    if (info.Database == null)
      throw new McpException("Database cannot be null");
    if (info.IsOffline || TransactionOperations.IsInTransaction() || !info.Database.Model.HasLocalChanges)
      return;
    info.Database.Model.UndoLocalChanges();
  }

  public static void SaveChangesWithRollback(
    PowerBIModelingMCP.Library.Common.DataStructures.ConnectionInfo info,
    string operationName,
    ConnectionOperations.CheckpointMode checkpointMode = ConnectionOperations.CheckpointMode.Default)
  {
    try
    {
      ConnectionOperations.SaveChangesIfNeeded(info, checkpointMode);
    }
    catch (Exception ex)
    {
      ConnectionOperations.UndoLocalChangesIfNeeded(info);
      throw new McpException($"Failed to {operationName}: {ex.Message}");
    }
  }

  public static void Sync(PowerBIModelingMCP.Library.Common.DataStructures.ConnectionInfo info)
  {
    if (info == null)
      throw new McpException("ConnectionInfo cannot be null");
    if (info.Database == null)
      throw new McpException("Database cannot be null");
    if (info.Database.Model == null)
      throw new McpException("Model cannot be null");
    if (info.IsOffline || info.Database.Model.HasLocalChanges || info.Database.Model.Sync().Impact.IsEmpty)
      return;
    info.LastSynced = new DateTime?(DateTime.UtcNow);
  }

  internal static void GetConnectionDetails(
    string? connectionName,
    out string? serverName,
    out string databaseName,
    out bool isLLMCreated)
  {
    PowerBIModelingMCP.Library.Common.DataStructures.ConnectionInfo connectionInfo = ConnectionOperations.Get(connectionName);
    serverName = connectionInfo.TabularServer != null ? connectionInfo.TabularServer.Name : (string) null;
    databaseName = connectionInfo.Database.Name;
    isLLMCreated = connectionInfo.IsLLMCreated;
  }

  private static void ValidateConnectionName(string connectionName)
  {
    if (string.IsNullOrWhiteSpace(connectionName))
      throw new McpException("Connection name cannot be null or empty");
    if (!Regex.IsMatch(connectionName, "^(?:[a-zA-Z0-9_\\- ]|%20)+$"))
      throw new McpException("Connection name can only contain letters, numbers, underscores, hyphens, and spaces");
    if (connectionName.Length > 100)
      throw new McpException("Connection name cannot exceed 100 characters");
  }

  private static bool IsLikelyDataSourceUrl(string input)
  {
    return !string.IsNullOrWhiteSpace(input) && (input.StartsWith("powerbi://", StringComparison.OrdinalIgnoreCase) || input.Contains("://") && !input.Contains(';'));
  }

  private static string ResolveXmlaHost()
  {
    string environmentVariable = Environment.GetEnvironmentVariable("XMLA_ENDPOINT_HOST");
    return !string.IsNullOrWhiteSpace(environmentVariable) ? environmentVariable : "api.powerbi.com";
  }

  internal static string BuildPowerBiXmlaEndpoint(string workspaceName, string? tenantName = null)
  {
    return $"powerbi://{ConnectionOperations.ResolveXmlaHost()}/v1.0/{(string.IsNullOrWhiteSpace(tenantName) ? "myorg" : tenantName)}/{Uri.EscapeDataString(workspaceName)}";
  }

  internal static string BuildConnectionString(string dataSource, string? initialCatalog)
  {
    if (string.IsNullOrWhiteSpace(dataSource))
      throw new McpException("DataSource is required.");
    if (!dataSource.StartsWith("powerbi://", StringComparison.OrdinalIgnoreCase) && (dataSource.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || dataSource.StartsWith("https://", StringComparison.OrdinalIgnoreCase)))
    {
      Uri uri = new Uri(dataSource);
      dataSource = $"powerbi://{uri.Host}{uri.PathAndQuery}".TrimEnd('/');
    }
    string connectionString = "Data Source=" + dataSource;
    if (!string.IsNullOrWhiteSpace(initialCatalog))
      connectionString = $"{connectionString};Initial Catalog={initialCatalog}";
    return ConnectionOperations.AddApplicationNameToConnectionString(connectionString);
  }

  private static string AddApplicationNameToConnectionString(string connectionString)
  {
    if (string.IsNullOrWhiteSpace(connectionString) || connectionString.Contains("Application Name=", StringComparison.OrdinalIgnoreCase))
      return connectionString;
    string str = connectionString.EndsWith(";") ? "" : ";";
    return $"{connectionString}{str}Application Name=MCP-PBIModeling";
  }

  private static string AddSessionIdToConnectionString(string connectionString, string sessionId)
  {
    if (string.IsNullOrWhiteSpace(connectionString))
      throw new McpException("Connection string cannot be null or empty when adding SessionId.");
    if (string.IsNullOrWhiteSpace(sessionId))
      throw new McpException("SessionId cannot be null or empty.");
    if (connectionString.Contains("SessionId=", StringComparison.OrdinalIgnoreCase))
      throw new McpException("Connection string already contains SessionId parameter. Manual SessionId specification is not supported.");
    string str = connectionString.EndsWith(";") ? "" : ";";
    return $"{connectionString}{str}SessionId={sessionId}";
  }

  internal static bool IsFabricConnectionString(string connectionString)
  {
    return connectionString.Contains("powerbi://", StringComparison.OrdinalIgnoreCase);
  }

  private static string? ExtractDatabaseName(string connectionString)
  {
    foreach (string str in connectionString.Split(';', (StringSplitOptions) 0))
    {
      string[] strArray = str.Split('=', (StringSplitOptions) 0);
      if (strArray.Length == 2 && (strArray[0].Trim().Equals("Initial Catalog", StringComparison.OrdinalIgnoreCase) || strArray[0].Trim().Equals("Database", StringComparison.OrdinalIgnoreCase)))
        return strArray[1].Trim();
    }
    return (string) null;
  }

  private static string? ExtractServerName(string? connectionString)
  {
    if (string.IsNullOrEmpty(connectionString))
      return (string) null;
    foreach (string str in connectionString.Split(';', (StringSplitOptions) 0))
    {
      string[] strArray = str.Split('=', (StringSplitOptions) 0);
      if (strArray.Length == 2 && (strArray[0].Trim().Equals("Data Source", StringComparison.OrdinalIgnoreCase) || strArray[0].Trim().Equals("Server", StringComparison.OrdinalIgnoreCase)))
        return strArray[1].Trim();
    }
    return (string) null;
  }

  private static string? ExtractWorkspaceName(string? connectionString)
  {
    if (string.IsNullOrEmpty(connectionString) || !ConnectionOperations.IsFabricConnectionString(connectionString))
      return (string) null;
    string serverName = ConnectionOperations.ExtractServerName(connectionString);
    if (string.IsNullOrEmpty(serverName))
      return (string) null;
    string[] strArray = serverName.Split('/', (StringSplitOptions) 0);
    return strArray.Length == 6 ? strArray[5] : (string) null;
  }

  private static void DisconnectAll()
  {
    foreach (KeyValuePair<string, PowerBIModelingMCP.Library.Common.DataStructures.ConnectionInfo> keyValuePair in Enumerable.ToList<KeyValuePair<string, PowerBIModelingMCP.Library.Common.DataStructures.ConnectionInfo>>((IEnumerable<KeyValuePair<string, PowerBIModelingMCP.Library.Common.DataStructures.ConnectionInfo>>) ConnectionOperations._namedConnections))
    {
      PowerBIModelingMCP.Library.Common.DataStructures.ConnectionInfo connectionInfo;
      if (ConnectionOperations._namedConnections.TryRemove(keyValuePair.Key, out connectionInfo))
        ConnectionOperations.DisposeConnection(connectionInfo);
    }
    ConnectionOperations._lastUsedConnectionName = (string) null;
  }

  private static void DisposeConnection(PowerBIModelingMCP.Library.Common.DataStructures.ConnectionInfo connectionInfo)
  {
    if (connectionInfo.Transaction != null)
    {
      try
      {
        connectionInfo.TabularServer?.RollbackTransaction();
        connectionInfo.Transaction.Operations.Add($"Server transaction automatically rolled back during connection disposal at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
      }
      catch (Exception ex)
      {
      }
      finally
      {
        connectionInfo.Transaction = (TransactionContext) null;
      }
    }
    try
    {
      connectionInfo.AdomdConnection?.Close();
      connectionInfo.AdomdConnection?.Dispose();
    }
    catch (Exception ex)
    {
    }
    try
    {
      connectionInfo.TabularServer?.Disconnect();
    }
    catch (Exception ex)
    {
    }
  }

  internal static (bool isPBIDesktopInstance, string connectionName) GenerateConnectionNameFromConnectionString(
    string cs)
  {
    if (string.IsNullOrEmpty(cs))
      throw new McpException("Connection string cannot be null or empty");
    if (ConnectionOperations.IsFabricConnectionString(cs))
      return (false, "Fabric-" + (ConnectionOperations.ExtractWorkspaceName(cs) ?? throw new McpException("Workspace must be present in Fabric connection string")));
    string serverName = ConnectionOperations.ExtractServerName(cs);
    if (serverName == null)
      throw new McpException("Server name must be present in connection string");
    LocalAnalysisServicesInstance servicesInstance = Enumerable.FirstOrDefault<LocalAnalysisServicesInstance>((IEnumerable<LocalAnalysisServicesInstance>) ConnectionOperations.ListLocalAnalysisServicesInstances(), (i => serverName.EndsWith($":{i.Port}")));
    string str = serverName.Split(':', (StringSplitOptions) 0)[1];
    return servicesInstance != null && (servicesInstance.ParentProcessName == "PBIDesktop") ? (true, $"PBIDesktop-{servicesInstance.ParentWindowTitle}-{str}") : (false, "Local-" + str);
  }

  private static Dictionary<int, int> GetLocalListenerPorts()
  {
    Dictionary<int, int> localListenerPorts = new Dictionary<int, int>();
    int dwOutBufLen = 0;
    int extendedTcpTable = (int) ConnectionOperations.GetExtendedTcpTable(IntPtr.Zero, ref dwOutBufLen, true, 2, 5);
    IntPtr pTcpTable = Marshal.AllocHGlobal(dwOutBufLen);
    try
    {
      if (ConnectionOperations.GetExtendedTcpTable(pTcpTable, ref dwOutBufLen, true, 2, 5) != 0U)
        return localListenerPorts;
      uint num1 = (uint) Marshal.ReadInt32(pTcpTable);
      IntPtr num2 = IntPtr.Add(pTcpTable, 4);
      int num3 = Marshal.SizeOf<ConnectionOperations.TcpRow>();
      for (int index = 0; (long) index < (long) num1; ++index)
      {
        ConnectionOperations.TcpRow structure = Marshal.PtrToStructure<ConnectionOperations.TcpRow>(num2);
        if (structure.State == 2U)
        {
          int processId = (int) structure.ProcessId;
          if (!localListenerPorts.ContainsKey(processId))
            localListenerPorts.Add(processId, structure.LocalPort);
        }
        num2 = IntPtr.Add(num2, num3);
      }
    }
    finally
    {
      Marshal.FreeHGlobal(pTcpTable);
    }
    return localListenerPorts;
  }

  private static Process? GetParentProcess(int processId)
  {
    try
    {
      using (Process processById = Process.GetProcessById(processId))
        return ConnectionOperations.GetParentProcess(processById.Handle);
    }
    catch (Exception ex)
    {
      return (Process) null;
    }
  }

  private static Process? GetParentProcess(IntPtr handle)
  {
    ConnectionOperations.ProcessBasicInformation processInformation = new ConnectionOperations.ProcessBasicInformation();
    if (ConnectionOperations.NtQueryInformationProcess(handle, 0, ref processInformation, Marshal.SizeOf<ConnectionOperations.ProcessBasicInformation>(processInformation), out int _) != 0)
      return (Process) null;
    try
    {
      return Process.GetProcessById(processInformation.ParentProcessId);
    }
    catch (ArgumentException ex)
    {
      return (Process) null;
    }
  }

  [DllImport("iphlpapi.dll", CharSet = (CharSet) 4, SetLastError = true)]
  private static extern uint GetExtendedTcpTable(
    IntPtr pTcpTable,
    ref int dwOutBufLen,
    bool sort,
    int ipVersion,
    int tblClass,
    uint reserved = 0);

  [DllImport("ntdll.dll")]
  private static extern int NtQueryInformationProcess(
    IntPtr processHandle,
    int processInformationClass,
    ref ConnectionOperations.ProcessBasicInformation processInformation,
    int processInformationLength,
    out int returnLength);

  public class FabricWorkspace
  {
    public required string Name { get; set; }

    public required string Id { get; set; }

    public required string Region { get; set; }
  }

  public class FabricSemanticModel
  {
    public required string Name { get; set; }

    public required string Id { get; set; }

    public required string WorkspaceId { get; set; }
  }

  public enum CheckpointMode
  {
    Default,
    ForceEvenInTransaction,
    AfterRequestRename,
  }

  private struct TcpRow
  {
    public uint State;
    public uint LocalAddr;
    [MarshalAs((UnmanagedType) 30, SizeConst = 4)]
    public byte[] LocalPortBytes;
    public uint RemoteAddr;
    [MarshalAs((UnmanagedType) 30, SizeConst = 4)]
    public byte[] RemotePort;
    public uint ProcessId;

    public int LocalPort
    {
      get
      {
        return (int) BitConverter.ToUInt16(new byte[2]
        {
          this.LocalPortBytes[1],
          this.LocalPortBytes[0]
        }, 0);
      }
    }
  }

  private struct ProcessBasicInformation
  {
    private IntPtr Reserved1;
    private IntPtr PebBaseAddress;
    private IntPtr Reserved2_0;
    private IntPtr Reserved2_1;
    private IntPtr UniqueProcessId;
    private IntPtr InheritedFromUniqueProcessId;

    public int ParentProcessId => this.InheritedFromUniqueProcessId.ToInt32();
  }
}
