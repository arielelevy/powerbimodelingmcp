// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using Microsoft.AnalysisServices;
using Microsoft.AnalysisServices.Tabular;
using Microsoft.AnalysisServices.Tabular.Serialization;
using ModelContextProtocol;
using PowerBIModelingMCP.Library.Common;
using PowerBIModelingMCP.Library.Common.DataStructures;
using PowerBIModelingMCP.Library.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

#nullable enable
namespace PowerBIModelingMCP.Library.Core;

public static class DatabaseOperations
{
  public static List<DatabaseGet> ListDatabases(string? connectionName = null)
  {
    PowerBIModelingMCP.Library.Common.DataStructures.ConnectionInfo connectionInfo = ConnectionOperations.Get(connectionName);
    List<DatabaseGet> databaseGetList1 = new List<DatabaseGet>();
    if (connectionInfo.IsOffline)
    {
      Microsoft.AnalysisServices.Tabular.Database database = connectionInfo.Database;
      List<DatabaseGet> databaseGetList2 = databaseGetList1;
      DatabaseGet databaseGet = new DatabaseGet { Name = database.Name };
      databaseGet.Id = database.ID;
      databaseGet.Description = database.Description;
      databaseGet.State = database.State.ToString();
      databaseGet.CreatedTimestamp = database.CreatedTimestamp;
      databaseGet.LastProcessed = database.LastProcessed;
      databaseGet.LastUpdate = database.LastUpdate;
      databaseGet.LastSchemaUpdate = database.LastSchemaUpdate;
      databaseGet.EstimatedSize = database.EstimatedSize;
      databaseGet.CompatibilityLevel = new int?(database.CompatibilityLevel);
      databaseGet.Collation = database.Collation;
      databaseGet.Language = new int?(database.Language);
      databaseGet.Model = database.Model?.Name;
      databaseGet.ModelType = database.ModelType.ToString();
      databaseGetList2.Add(databaseGet);
    }
    else
    {
      Microsoft.AnalysisServices.Tabular.Server tabularServer = connectionInfo.TabularServer;
      if (tabularServer?.Databases != null)
      {
        foreach (Microsoft.AnalysisServices.Tabular.Database database in (ModelComponentCollection) tabularServer.Databases)
        {
          List<DatabaseGet> databaseGetList3 = databaseGetList1;
          DatabaseGet databaseGet = new DatabaseGet { Name = database.Name };
          databaseGet.Id = database.ID;
          databaseGet.Description = database.Description;
          databaseGet.State = database.State.ToString();
          databaseGet.CreatedTimestamp = database.CreatedTimestamp;
          databaseGet.LastProcessed = database.LastProcessed;
          databaseGet.LastUpdate = database.LastUpdate;
          databaseGet.LastSchemaUpdate = database.LastSchemaUpdate;
          databaseGet.EstimatedSize = database.EstimatedSize;
          databaseGet.CompatibilityLevel = new int?(database.CompatibilityLevel);
          databaseGet.Collation = database.Collation;
          databaseGet.Language = new int?(database.Language);
          databaseGet.Model = database.Model?.Name;
          databaseGet.ModelType = database.ModelType.ToString();
          databaseGetList3.Add(databaseGet);
        }
      }
    }
    return Enumerable.ToList<DatabaseGet>(Enumerable.OrderBy<DatabaseGet, string>((IEnumerable<DatabaseGet>) databaseGetList1, (d => d.Name)));
  }

  public static DatabaseOperationResult UpdateDatabase(string? connectionName, DatabaseUpdate update)
  {
    if (update == null)
      throw new McpException("Database update definition cannot be null");
    if (string.IsNullOrWhiteSpace(update.Name))
      throw new McpException("Name is required");
    PowerBIModelingMCP.Library.Common.DataStructures.ConnectionInfo connection = ConnectionOperations.Get(connectionName);
    ConnectionValidator.ValidateOnlineConnection(connection);
    Microsoft.AnalysisServices.Tabular.Server tabularServer = connection.TabularServer;
    if (tabularServer?.Databases == null)
      throw new McpException("No databases available on the server");
    Microsoft.AnalysisServices.Tabular.Database byName = tabularServer.Databases.FindByName(update.Name);
    if (byName == null)
      throw new McpException($"Database '{update.Name}' not found on server");
    bool flag = false;
    if (!string.IsNullOrEmpty(update.NewName) && (update.NewName != byName.Name))
      throw new McpException("Database name cannot be changed. Use the existing database name or omit the NewName property.");
    if (update.Description != null)
    {
      string description = string.IsNullOrEmpty(update.Description) ? (string) null : update.Description;
      if ((description != byName.Description))
      {
        byName.Description = description;
        flag = true;
      }
    }
    int? nullable;
    if (update.CompatibilityLevel.HasValue && update.CompatibilityLevel.Value != byName.CompatibilityLevel)
    {
      Microsoft.AnalysisServices.Tabular.Database database = byName;
      nullable = update.CompatibilityLevel;
      int num = nullable.Value;
      database.CompatibilityLevel = num;
      flag = true;
    }
    if (update.Collation != null)
    {
      string collation = string.IsNullOrEmpty(update.Collation) ? (string) null : update.Collation;
      if ((collation != byName.Collation))
      {
        byName.Collation = collation;
        flag = true;
      }
    }
    nullable = update.Language;
    if (nullable.HasValue)
    {
      nullable = update.Language;
      if (nullable.Value != byName.Language)
      {
        Microsoft.AnalysisServices.Tabular.Database database = byName;
        nullable = update.Language;
        int num = nullable.Value;
        database.Language = num;
        flag = true;
      }
    }
    if (update.Annotations != null && byName.Model != null && AnnotationHelpers.ReplaceAnnotations<Microsoft.AnalysisServices.Tabular.Model>(byName.Model, update.Annotations, (Func<Microsoft.AnalysisServices.Tabular.Model, ICollection<Microsoft.AnalysisServices.Tabular.Annotation>>) (m => (ICollection<Microsoft.AnalysisServices.Tabular.Annotation>) m.Annotations)))
      flag = true;
    if (!flag)
      return new DatabaseOperationResult()
      {
        State = byName.State.ToString(),
        ErrorMessage = (string) null,
        DatabaseName = byName.Name,
        HasChanges = false
      };
    try
    {
      byName.Update();
    }
    catch (Exception ex)
    {
      throw new McpException("Failed to update database properties: " + ex.Message);
    }
    return new DatabaseOperationResult()
    {
      State = byName.State.ToString(),
      ErrorMessage = (string) null,
      DatabaseName = byName.Name,
      HasChanges = true
    };
  }

  public static TmdlDeserializeResult ImportFromTmdlFolder(string folderPath, string? connectionName = null)
  {
    if (string.IsNullOrWhiteSpace(folderPath))
      throw new McpException("Folder path cannot be null or empty");
    if (!Directory.Exists(folderPath))
      throw new McpException("TMDL folder does not exist: " + folderPath);
    try
    {
      Microsoft.AnalysisServices.Tabular.Database database = Microsoft.AnalysisServices.Tabular.TmdlSerializer.DeserializeDatabaseFromFolder(folderPath);
      string baseName = connectionName ?? "TMDL-" + folderPath;
      string offlineConnection = ConnectionOperations.CreateOfflineConnection(connectionName == null ? ConnectionOperations.EnsureUniqueConnectionName(baseName) : baseName, database, folderPath);
      TmdlDeserializeResult deserializeResult = new TmdlDeserializeResult { Success = true };
      deserializeResult.ConnectionName = offlineConnection;
      deserializeResult.DatabaseName = database.Name;
      deserializeResult.FolderPath = folderPath;
      deserializeResult.TablesLoaded = database.Model?.Tables?.Count ?? 0;
      Microsoft.AnalysisServices.Tabular.Model model = database.Model;
      int? nullable;
      if (model == null)
      {
        nullable = new int?();
      }
      else
      {
        TableCollection tables = model.Tables;
        nullable = tables != null ? new int?(Enumerable.Sum<Microsoft.AnalysisServices.Tabular.Table>((IEnumerable<Microsoft.AnalysisServices.Tabular.Table>) tables, (t => t.Measures.Count))) : new int?();
      }
      deserializeResult.MeasuresLoaded = nullable ?? 0;
      deserializeResult.RelationshipsLoaded = database.Model?.Relationships?.Count ?? 0;
      deserializeResult.LoadedAt = DateTime.UtcNow;
      deserializeResult.Message = $"Successfully loaded database '{database.Name}' from TMDL folder";
      return deserializeResult;
    }
    catch (Exception ex)
    {
      throw new McpException("Failed to import TMDL folder: " + ex.Message, ex);
    }
  }

  public static TmdlSerializeResult ExportToTmdlFolder(string? connectionName, string? targetPath = null)
  {
    PowerBIModelingMCP.Library.Common.DataStructures.ConnectionInfo connection = ConnectionOperations.Get(connectionName);
    ConnectionValidator.ValidateForModelOperations(connection);
    string str;
    if (!string.IsNullOrWhiteSpace(targetPath))
    {
      str = targetPath;
    }
    else
    {
      if (!connection.IsOffline || string.IsNullOrWhiteSpace(connection.TmdlFolderPath))
        throw new McpException("Target path must be specified for online connections or offline connections without a stored folder path");
      str = connection.TmdlFolderPath;
    }
    try
    {
      Directory.CreateDirectory(str);
      Microsoft.AnalysisServices.Tabular.TmdlSerializer.SerializeDatabaseToFolder(connection.Database, str);
      List<string> tmdlFiles = DatabaseOperations.GetTmdlFiles(str);
      return new TmdlSerializeResult()
      {
        Success = true,
        FolderPath = str,
        DatabaseName = connection.Database.Name,
        FilesCreated = tmdlFiles,
        FileCount = tmdlFiles.Count,
        SerializedAt = DateTime.UtcNow,
        Message = $"Successfully exported database '{connection.Database.Name}' to TMDL folder"
      };
    }
    catch (Exception ex)
    {
      throw new McpException("Failed to export database to TMDL folder: " + ex.Message, ex);
    }
  }

  public static async Task<DatabaseOperationResponse> DeployToFabric(
    string sourceConnectionName,
    DeployToFabricRequest request)
  {
    if (string.IsNullOrWhiteSpace(sourceConnectionName))
      throw new McpException("Source connection name cannot be null or empty");
    if (request == null)
      throw new McpException("DeployToFabricRequest cannot be null");
    try
    {
      PowerBIModelingMCP.Library.Common.DataStructures.ConnectionInfo sourceInfo = ConnectionOperations.Get(sourceConnectionName);
      if (sourceInfo.Database == null)
        throw new McpException("Source connection is not bound to a database. Reconnect with an Initial Catalog selected or open an offline database.");
      DatabaseExportTmsl databaseExportTmsl = new DatabaseExportTmsl { TmslOperationType = "CreateOrReplace" };
      databaseExportTmsl.IncludeRestricted = new bool?(request.IncludeRestricted.GetValueOrDefault());
      databaseExportTmsl.MaxReturnCharacters = -1;
      DatabaseExportTmsl tmslOptions = databaseExportTmsl;
      TmslExportResult tmslExportResult = DatabaseOperations.ExportTMSL(sourceConnectionName, sourceInfo.IsOffline ? (string) null : sourceInfo.Database.Name, tmslOptions);
      string tmslScript = tmslExportResult.Success ? tmslExportResult.Content : throw new McpException("Failed to generate TMSL: " + tmslExportResult.ErrorMessage);
      if (!string.IsNullOrWhiteSpace(request.NewDatabaseName))
        tmslScript = DatabaseOperations.RewriteDbNameInTmsl(tmslScript, request.NewDatabaseName);
      string targetXmla = DatabaseOperations.ResolveTargetXmla(request);
      AccessToken accessToken = new AccessToken(await AuthService.GetAccessTokenAsync(), DateTimeOffset.UtcNow.AddHours(1.0));
      using (Microsoft.AnalysisServices.Tabular.Server server = new Microsoft.AnalysisServices.Tabular.Server())
      {
        server.AccessToken = accessToken;
        server.Connect(targetXmla);
        XmlaResultCollection resultCollection = server.Execute(tmslScript);
        if (resultCollection.ContainsErrors)
        {
          string str = string.Join("\n", Enumerable.ToArray<string>(Enumerable.SelectMany<XmlaResult, string>(Enumerable.Cast<XmlaResult>((IEnumerable) resultCollection), (Func<XmlaResult, IEnumerable<string>>) (r => Enumerable.Select<XmlaMessage, string>(Enumerable.Cast<XmlaMessage>((IEnumerable) r.Messages), (m => m.Description))))));
          throw new McpException(!string.IsNullOrEmpty(str) ? str : "TMSL execution failed with unknown errors");
        }
        string str1 = request.NewDatabaseName ?? sourceInfo.Database.Name;
        return new DatabaseOperationResponse()
        {
          Success = true,
          Operation = nameof (DeployToFabric),
          DatabaseName = str1,
          Message = $"Successfully deployed database '{str1}' to Fabric workspace"
        };
      }
    }
    catch (Exception ex)
    {
      return new DatabaseOperationResponse()
      {
        Success = false,
        Operation = nameof (DeployToFabric),
        Message = "Failed to deploy database to Fabric: " + ex.Message
      };
    }
  }

  public static DatabaseCreateResult CreateOfflineDb(
    DatabaseCreate definition,
    string? connectionName = null)
  {
    if (definition == null)
      throw new McpException("Database creation definition cannot be null");
    if (string.IsNullOrWhiteSpace(definition.Name))
      throw new McpException("Database name is required");
    string baseName = connectionName ?? definition.Name;
    string connectionName1 = connectionName == null ? ConnectionOperations.EnsureUniqueConnectionName(baseName) : baseName;
    try
    {
      Microsoft.AnalysisServices.Tabular.Database database1 = new Microsoft.AnalysisServices.Tabular.Database();
      database1.Name = definition.Name;
      database1.ID = definition.Name;
      Microsoft.AnalysisServices.Tabular.Database database2 = database1;
      if (definition.Description != null)
        database2.Description = definition.Description;
      int? nullable;
      if (definition.CompatibilityLevel.HasValue)
      {
        Microsoft.AnalysisServices.Tabular.Database database3 = database2;
        nullable = definition.CompatibilityLevel;
        int num = nullable.Value;
        database3.CompatibilityLevel = num;
      }
      if (definition.Collation != null)
        database2.Collation = definition.Collation;
      nullable = definition.Language;
      if (nullable.HasValue)
      {
        Microsoft.AnalysisServices.Tabular.Database database4 = database2;
        nullable = definition.Language;
        int num = nullable.Value;
        database4.Language = num;
      }
      Microsoft.AnalysisServices.Tabular.Model model1 = new Microsoft.AnalysisServices.Tabular.Model();
      model1.Name = definition.Name;
      model1.Culture = "en-US";
      model1.DefaultPowerBIDataSourceVersion = PowerBIDataSourceVersion.PowerBI_V3;
      model1.SourceQueryCulture = "en-US";
      model1.DataAccessOptions = new DataAccessOptions()
      {
        LegacyRedirects = true,
        ReturnErrorValuesAsNull = true
      };
      Microsoft.AnalysisServices.Tabular.Model model2 = model1;
      database2.Model = model2;
      if (definition.Annotations != null)
      {
        foreach (KeyValuePair<string, string> annotation in definition.Annotations)
        {
          if (!string.IsNullOrWhiteSpace(annotation.Key))
          {
            ModelAnnotationCollection annotations = model2.Annotations;
            Microsoft.AnalysisServices.Tabular.Annotation metadataObject = new Microsoft.AnalysisServices.Tabular.Annotation();
            metadataObject.Name = annotation.Key;
            metadataObject.Value = annotation.Value ?? string.Empty;
            annotations.Add(metadataObject);
          }
        }
      }
      string offlineConnection = ConnectionOperations.CreateOfflineConnection(connectionName1, database2, string.Empty);
      ModelOperations.AddProToolingAnnotation(ConnectionOperations.Get(offlineConnection));
      DatabaseCreateResult offlineDb = new DatabaseCreateResult { Success = true };
      offlineDb.ConnectionName = offlineConnection;
      offlineDb.DatabaseName = database2.Name;
      offlineDb.ModelName = model2.Name;
      offlineDb.CreatedAt = DateTime.UtcNow;
      offlineDb.Message = $"Successfully created empty database '{database2.Name}' with connection '{offlineConnection}'";
      return offlineDb;
    }
    catch (Exception ex)
    {
      throw new McpException("Failed to create empty database: " + ex.Message, ex);
    }
  }

  private static List<string> GetTmdlFiles(string folderPath)
  {
    return Directory.GetFiles(folderPath, "*.tmdl", SearchOption.AllDirectories)
      .Select(Path.GetFileName)
      .Where(f => f != null)
      .Cast<string>()
      .OrderBy(f => f)
      .ToList();
  }

  public static TmdlExportResult ExportTMDL(
    string? connectionName,
    string? databaseName = null,
    DatabaseExportTmdl? options = null)
  {
    try
    {
      PowerBIModelingMCP.Library.Common.DataStructures.ConnectionInfo connectionInfo = ConnectionOperations.Get(connectionName);
      Microsoft.AnalysisServices.Tabular.Database database;
      string objectName;
      if (connectionInfo.IsOffline)
      {
        database = connectionInfo.Database;
        objectName = database.Name ?? "Database";
      }
      else
      {
        if (string.IsNullOrWhiteSpace(databaseName))
          throw new ArgumentException("Database name is required for online connections");
        database = (connectionInfo.TabularServer ?? throw new InvalidOperationException("Tabular server is not available")).Databases.Find(databaseName);
        if (database == null)
          throw new ArgumentException($"Database '{databaseName}' not found");
        objectName = databaseName;
      }
      string str1;
      if (options?.SerializationOptions != null)
      {
        MetadataSerializationOptions serializationOptions = options.SerializationOptions.ToMetadataSerializationOptions();
        str1 = Microsoft.AnalysisServices.Tabular.TmdlSerializer.SerializeDatabase(database, serializationOptions);
      }
      else
        str1 = Microsoft.AnalysisServices.Tabular.TmdlSerializer.SerializeDatabase(database);
      if (options == null)
        return TmdlExportResult.CreateSuccess(objectName, "Database", str1, str1, false, (string) null, new List<string>());
      (string str2, bool flag, string str3, List<string> stringList) = ExportContentProcessor.ProcessExportContent(str1, (ExportOptionsBase) options);
      return TmdlExportResult.CreateSuccess(objectName, "Database", str1, str2, flag, str3, stringList, (ExportTmdl) options);
    }
    catch (Exception ex)
    {
      return TmdlExportResult.CreateFailure(databaseName ?? "Database", "Database", ex.Message);
    }
  }

  public static TmslExportResult ExportTMSL(
    string? connectionName,
    string? databaseName,
    DatabaseExportTmsl tmslOptions)
  {
    if (tmslOptions == null)
      throw new McpException("tmslOptions is required");
    if (string.IsNullOrWhiteSpace(tmslOptions.TmslOperationType))
      throw new McpException("TmslOperationType is required in tmslOptions");
    try
    {
      PowerBIModelingMCP.Library.Common.DataStructures.ConnectionInfo connectionInfo = ConnectionOperations.Get(connectionName);
      Microsoft.AnalysisServices.Tabular.Database database;
      if (connectionInfo.IsOffline)
      {
        database = connectionInfo.Database;
        if (database.Name != null)
          ;
      }
      else
      {
        if (string.IsNullOrWhiteSpace(databaseName))
          throw new ArgumentException("Database name is required for online connections");
        database = (connectionInfo.TabularServer ?? throw new InvalidOperationException("Tabular server is not available")).Databases.Find(databaseName);
        if (database == null)
          throw new ArgumentException($"Database '{databaseName}' not found");
      }
      TmslOperationType operationType;
      if (!Enum.TryParse<TmslOperationType>(tmslOptions.TmslOperationType, true, out operationType))
      {
        string[] names = Enum.GetNames(typeof (TmslOperationType));
        throw new McpException($"Invalid TmslOperationType '{tmslOptions.TmslOperationType}'. Valid values: {string.Join(", ", names)}");
      }
      TmslOperationRequest options = new TmslOperationRequest()
      {
        OperationType = operationType,
        IncludeRestricted = tmslOptions.IncludeRestricted.GetValueOrDefault()
      };
      if (!string.IsNullOrWhiteSpace(tmslOptions.RefreshType))
      {
        Microsoft.AnalysisServices.Tabular.RefreshType refreshType;
        if (Enum.TryParse<Microsoft.AnalysisServices.Tabular.RefreshType>(tmslOptions.RefreshType, true, out refreshType))
        {
          options.RefreshType = new Microsoft.AnalysisServices.Tabular.RefreshType?(refreshType);
        }
        else
        {
          string[] names = Enum.GetNames(typeof (Microsoft.AnalysisServices.Tabular.RefreshType));
          throw new McpException($"Invalid RefreshType '{tmslOptions.RefreshType}'. Valid values: {string.Join(", ", names)}");
        }
      }
      TmslExportResult tmslExportResult = TmslExportResult.FromLegacyResult(TmslScriptingService.GenerateScript(database, operationType, options));
      (string Content, bool IsTruncated, string SavedFilePath, List<string> Warnings) = ExportContentProcessor.ProcessExportContent(tmslExportResult.Content, (ExportOptionsBase) tmslOptions);
      tmslExportResult.Content = Content;
      tmslExportResult.IsTruncated = IsTruncated;
      tmslExportResult.SavedFilePath = SavedFilePath;
      tmslExportResult.Warnings.AddRange((IEnumerable<string>) Warnings);
      tmslExportResult.AppliedOptions = (ExportTmsl) tmslOptions;
      return tmslExportResult;
    }
    catch (Exception ex)
    {
      return TmslExportResult.CreateFailure(databaseName ?? "Database", "Database", ex.Message);
    }
  }

  private static string RewriteDbNameInTmsl(string tmsl, string newName)
  {
    try
    {
      JsonObject jsonObject1 = JsonNode.Parse(tmsl).AsObject();
      JsonObject jsonObject2 = jsonObject1["createOrReplace"]?.AsObject();
      if (jsonObject2 == null)
        throw new McpException("Invalid TMSL format. 'createOrReplace' is missing.");
      JsonObject jsonObject3 = jsonObject2["object"]?.AsObject();
      if (jsonObject3 != null)
        jsonObject3["database"] = (JsonNode) newName;
      JsonObject jsonObject4 = jsonObject2["database"]?.AsObject();
      if (jsonObject4 == null)
        throw new McpException("Invalid TMSL format. 'createOrReplace.database' is missing.");
      jsonObject4["name"] = (JsonNode) newName;
      if (jsonObject4.ContainsKey("id"))
        jsonObject4["id"] = (JsonNode) newName;
      return jsonObject1.ToJsonString(new JsonSerializerOptions()
      {
        WriteIndented = false
      });
    }
    catch (Exception ex) when (!(ex is McpException))
    {
      throw new McpException("Failed to rewrite database name in TMSL: " + ex.Message, ex);
    }
  }

  private static string ResolveTargetXmla(DeployToFabricRequest request)
  {
    if (!string.IsNullOrWhiteSpace(request.TargetConnectionString))
    {
      string connectionString = request.TargetConnectionString;
      int? connectTimeoutSeconds = request.ConnectTimeoutSeconds;
      if (connectTimeoutSeconds.HasValue)
      {
        connectTimeoutSeconds = request.ConnectTimeoutSeconds;
        int num = 0;
        if (connectTimeoutSeconds.GetValueOrDefault() > num & connectTimeoutSeconds.HasValue && !connectionString.Contains("Timeout", StringComparison.OrdinalIgnoreCase))
          connectionString += $";Timeout={request.ConnectTimeoutSeconds}";
      }
      return connectionString;
    }
    if (string.IsNullOrWhiteSpace(request.TargetWorkspaceName))
      throw new McpException("Specify TargetConnectionString or TargetWorkspaceName.");
    string str = "Data Source=" + ConnectionOperations.BuildPowerBiXmlaEndpoint(request.TargetWorkspaceName, request.TargetTenantName);
    if (request.ConnectTimeoutSeconds.HasValue)
    {
      int? connectTimeoutSeconds = request.ConnectTimeoutSeconds;
      int num = 0;
      if (connectTimeoutSeconds.GetValueOrDefault() > num & connectTimeoutSeconds.HasValue)
        str += $";Timeout={request.ConnectTimeoutSeconds}";
    }
    return str;
  }
}
