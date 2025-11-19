// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Tools.DatabaseOperationsTool
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
using System.Threading.Tasks;

#nullable enable
namespace PowerBIModelingMCP.Library.Tools;

[McpServerToolType]
public class DatabaseOperationsTool
{
  private readonly ILogger<DatabaseOperationsTool> _logger;
  public static readonly ToolMetadata toolMetadata;

  public DatabaseOperationsTool(ILogger<DatabaseOperationsTool> logger) => this._logger = logger;

  [McpServerTool(Name = "database_operations")]
  [Description("Perform operations on semantic models hosted in Power BI Desktop or Fabric service. Supported operations: Help, Create, Update, List, ImportFromTmdlFolder, ExportToTmdlFolder, ExportTMDL (YAML-like format), ExportTMSL (JSON script format), DeployToFabric. Use the Operation parameter to specify which operation to perform.")]
  public async Task<DatabaseOperationResponse> ExecuteDatabaseOperation(
    McpServer mcpServer,
    DatabaseOperationRequest request)
  {
    this._logger.LogDebug("Executing {ToolName}.{Operation}: DatabaseName={DatabaseName}, Connection={ConnectionName}", (object) nameof (DatabaseOperationsTool), (object) request.Operation, (object) (request.DatabaseName ?? "(none)"), (object) (request.ConnectionName ?? "(last used)"));
    try
    {
      string[] strArray1 = new string[9]
      {
        "HELP",
        "CREATE",
        "UPDATE",
        "LIST",
        "IMPORTFROMTMDLFOLDER",
        "EXPORTTOTMDLFOLDER",
        "EXPORTTMDL",
        "EXPORTTMSL",
        "DEPLOYTOFABRIC"
      };
      string[] strArray2 = new string[2]
      {
        "UPDATE",
        "DEPLOYTOFABRIC"
      };
      string upperInvariant = request.Operation.ToUpperInvariant();
      if (!Enumerable.Contains<string>((IEnumerable<string>) strArray1, upperInvariant))
      {
        this._logger.LogWarning("Invalid operation '{Operation}' requested for {ToolName}. Valid operations: {ValidOperations}", (object) request.Operation, (object) nameof (DatabaseOperationsTool), (object) string.Join(", ", strArray1));
        return DatabaseOperationResponse.Forbidden(request.Operation, $"Invalid operation: {request.Operation}. Supported operations: {string.Join(", ", strArray1)}", request.DatabaseName);
      }
      if (!this.ValidateRequest(request.Operation, request))
        throw new McpException($"Invalid request for {request.Operation} operation.");
      if (Enumerable.Contains<string>((IEnumerable<string>) strArray2, upperInvariant))
      {
        WriteOperationResult writeOperationResult = WriteGuard.ExecuteWriteOperationWithGuards(mcpServer, request.ConnectionName, request.Operation);
        if (!writeOperationResult.Success)
        {
          this._logger.LogWarning("{ToolName}.{Operation} blocked by write guard: {Reason}", (object) nameof (DatabaseOperationsTool), (object) request.Operation, (object) writeOperationResult.Message);
          return DatabaseOperationResponse.Forbidden(request.Operation, writeOperationResult.Message, request.DatabaseName);
        }
      }
      bool allowed = WriteGuard.IsWriteAllowed("").allowed;
      DatabaseOperationResponse operationResponse;
      if (upperInvariant != null)
      {
        switch (upperInvariant.Length)
        {
          case 4:
            switch (upperInvariant[0])
            {
              case 'H':
                if ((upperInvariant == "HELP"))
                {
                  operationResponse = this.HandleHelpOperation(request, allowed ? strArray1 : Enumerable.ToArray<string>(Enumerable.Except<string>((IEnumerable<string>) strArray1, (IEnumerable<string>) strArray2)));
                  goto label_32;
                }
                break;
              case 'L':
                if ((upperInvariant == "LIST"))
                {
                  operationResponse = this.HandleListOperation(request);
                  goto label_32;
                }
                break;
            }
            break;
          case 6:
            switch (upperInvariant[0])
            {
              case 'C':
                if ((upperInvariant == "CREATE"))
                {
                  operationResponse = this.HandleCreateOperation(request);
                  goto label_32;
                }
                break;
              case 'U':
                if ((upperInvariant == "UPDATE"))
                {
                  operationResponse = this.HandleUpdateOperation(request);
                  goto label_32;
                }
                break;
            }
            break;
          case 10:
            switch (upperInvariant[8])
            {
              case 'D':
                if ((upperInvariant == "EXPORTTMDL"))
                {
                  operationResponse = this.HandleExportTMDLOperation(request);
                  goto label_32;
                }
                break;
              case 'S':
                if ((upperInvariant == "EXPORTTMSL"))
                {
                  operationResponse = this.HandleExportTMSLOperation(request);
                  goto label_32;
                }
                break;
            }
            break;
          case 14:
            if ((upperInvariant == "DEPLOYTOFABRIC"))
            {
              operationResponse = await this.HandleDeployToFabricOperation(request);
              goto label_32;
            }
            break;
          case 18:
            if ((upperInvariant == "EXPORTTOTMDLFOLDER"))
            {
              operationResponse = this.HandleExportToTmdlFolderOperation(request);
              goto label_32;
            }
            break;
          case 20:
            if ((upperInvariant == "IMPORTFROMTMDLFOLDER"))
            {
              operationResponse = this.HandleImportFromTmdlFolderOperation(request);
              goto label_32;
            }
            break;
        }
      }
      operationResponse = DatabaseOperationResponse.Forbidden(request.Operation, $"Operation {request.Operation} is not implemented", request.DatabaseName);
label_32:
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Error executing {ToolName}.{Operation}: {ErrorMessage}", (object) nameof (DatabaseOperationsTool), (object) request.Operation, (object) ex.Message);
      return new DatabaseOperationResponse()
      {
        Success = false,
        Message = "Error executing database operation: " + ex.Message,
        Operation = request.Operation,
        DatabaseName = request.DatabaseName
      };
    }
  }

  private DatabaseOperationResponse HandleListOperation(DatabaseOperationRequest request)
  {
    try
    {
      List<DatabaseGet> databaseGetList = DatabaseOperations.ListDatabases(request.ConnectionName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Count={Count}", (object) nameof (DatabaseOperationsTool), (object) "List", (object) databaseGetList.Count);
      DatabaseOperationResponse operationResponse = new DatabaseOperationResponse { Success = true };
      operationResponse.Message = $"Found {databaseGetList.Count} databases on server";
      operationResponse.Operation = request.Operation;
      operationResponse.Data = (object) databaseGetList;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      DatabaseOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new DatabaseOperationResponse()
      {
        Success = false,
        Message = "Error listing databases: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private DatabaseOperationResponse HandleUpdateOperation(DatabaseOperationRequest request)
  {
    try
    {
      if (string.IsNullOrEmpty(request.UpdateDefinition.Name))
        request.UpdateDefinition.Name = !string.IsNullOrEmpty(request.DatabaseName) ? request.DatabaseName : throw new McpException("Database name is required for Update operation");
      else if (!string.IsNullOrEmpty(request.DatabaseName) && (request.UpdateDefinition.Name != request.DatabaseName))
        throw new McpException($"Database name mismatch: Request specifies '{request.DatabaseName}' but UpdateDefinition specifies '{request.UpdateDefinition.Name}'");
      DatabaseOperationResult databaseOperationResult = DatabaseOperations.UpdateDatabase(request.ConnectionName, request.UpdateDefinition);
      List<string> stringList = new List<string>();
      if (!databaseOperationResult.HasChanges)
        stringList.Add("No changes were detected. The database is already in the requested state.");
      this._logger.LogInformation("{ToolName}.{Operation} completed: DatabaseName={DatabaseName}, HasChanges={HasChanges}", (object) nameof (DatabaseOperationsTool), (object) "Update", (object) databaseOperationResult.DatabaseName, (object) databaseOperationResult.HasChanges);
      if (Enumerable.Any<string>((IEnumerable<string>) stringList))
      {
        foreach (string str in stringList)
          this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (DatabaseOperationsTool), (object) request.Operation, (object) str);
      }
      return new DatabaseOperationResponse()
      {
        Success = true,
        Message = databaseOperationResult.HasChanges ? $"Database '{databaseOperationResult.DatabaseName}' updated successfully" : $"Database '{databaseOperationResult.DatabaseName}' is already in the requested state",
        Operation = request.Operation,
        DatabaseName = databaseOperationResult.DatabaseName,
        Data = (object) databaseOperationResult,
        Warnings = stringList
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      DatabaseOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new DatabaseOperationResponse()
      {
        Success = false,
        Message = "Error updating database: " + ex.Message,
        Operation = request.Operation,
        DatabaseName = request.DatabaseName,
        Help = (object) operationMetadata
      };
    }
  }

  private DatabaseOperationResponse HandleImportFromTmdlFolderOperation(
    DatabaseOperationRequest request)
  {
    try
    {
      TmdlDeserializeResult deserializeResult = DatabaseOperations.ImportFromTmdlFolder(request.TmdlFolderPath, request.ConnectionName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: ConnectionName={ConnectionName}, DatabaseName={DatabaseName}, FolderPath={FolderPath}", (object) nameof (DatabaseOperationsTool), (object) "ImportFromTmdlFolder", (object) deserializeResult.ConnectionName, (object) deserializeResult.DatabaseName, (object) request.TmdlFolderPath);
      return new DatabaseOperationResponse()
      {
        Success = deserializeResult.Success,
        Message = $"Successfully created offline connection '{deserializeResult.ConnectionName}' from TMDL folder",
        Operation = request.Operation,
        DatabaseName = deserializeResult.DatabaseName,
        Data = (object) deserializeResult
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      DatabaseOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new DatabaseOperationResponse()
      {
        Success = false,
        Message = "Error importing database from TMDL folder: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private DatabaseOperationResponse HandleExportToTmdlFolderOperation(
    DatabaseOperationRequest request)
  {
    try
    {
      TmdlSerializeResult tmdlFolder = DatabaseOperations.ExportToTmdlFolder(request.ConnectionName, request.TmdlFolderPath);
      this._logger.LogInformation("{ToolName}.{Operation} completed: DatabaseName={DatabaseName}, FolderPath={FolderPath}", (object) nameof (DatabaseOperationsTool), (object) "ExportToTmdlFolder", (object) tmdlFolder.DatabaseName, (object) tmdlFolder.FolderPath);
      DatabaseOperationResponse tmdlFolderOperation = new DatabaseOperationResponse { Success = tmdlFolder.Success };
      tmdlFolderOperation.Message = $"Successfully exported database '{tmdlFolder.DatabaseName}' to TMDL folder '{tmdlFolder.FolderPath}'";
      tmdlFolderOperation.Operation = request.Operation;
      tmdlFolderOperation.DatabaseName = tmdlFolder.DatabaseName;
      tmdlFolderOperation.Data = (object) tmdlFolder;
      return tmdlFolderOperation;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      DatabaseOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new DatabaseOperationResponse()
      {
        Success = false,
        Message = "Error exporting database to TMDL folder: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private async Task<DatabaseOperationResponse> HandleDeployToFabricOperation(
    DatabaseOperationRequest request)
  {
    try
    {
      DatabaseOperationResponse fabric = await DatabaseOperations.DeployToFabric(request.ConnectionName, request.DeployToFabricRequest);
      this._logger.LogInformation("{ToolName}.{Operation} completed: DatabaseName={DatabaseName}", (object) nameof (DatabaseOperationsTool), (object) "DeployToFabric", (object) (fabric.DatabaseName ?? "(unknown)"));
      if (fabric.Warnings != null && Enumerable.Any<string>((IEnumerable<string>) fabric.Warnings))
      {
        foreach (string warning in fabric.Warnings)
          this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (DatabaseOperationsTool), (object) request.Operation, (object) warning);
      }
      return fabric;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      DatabaseOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new DatabaseOperationResponse()
      {
        Success = false,
        Message = "Error deploying database to Fabric: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private DatabaseOperationResponse HandleCreateOperation(DatabaseOperationRequest request)
  {
    try
    {
      DatabaseCreate databaseCreate = this.ValidateRequest(request.Operation, request) ? request.CreateDefinition : throw new McpException($"Invalid request for {request.Operation} operation.");
      int num;
      if (databaseCreate == null)
      {
        num = 0;
      }
      else
      {
        bool? isOffline = databaseCreate.IsOffline;
        bool flag = false;
        num = isOffline.GetValueOrDefault() == flag & isOffline.HasValue ? 1 : 0;
      }
      if (num != 0)
        throw new McpException("Online database creation is not currently supported. Only offline databases can be created. Set IsOffline to true or omit the property (defaults to true).");
      List<string> stringList = new List<string>();
      DatabaseCreate createDefinition = request.CreateDefinition;
      if ((createDefinition != null ? (!createDefinition.IsOffline.HasValue ? 1 : 0) : 1) != 0)
        stringList.Add("Creating offline database (this is the only supported create operation).");
      if (!string.IsNullOrEmpty(request.ConnectionName) && ConnectionOperations.Exists(request.ConnectionName))
        throw new McpException($"Connection '{request.ConnectionName}' already exists. For creating a new database, you can only create a new offline database in a new connection. Cannot create new database on existing connections.");
      DatabaseCreateResult offlineDb = DatabaseOperations.CreateOfflineDb(request.CreateDefinition, request.ConnectionName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: DatabaseName={DatabaseName}, ConnectionName={ConnectionName}", (object) nameof (DatabaseOperationsTool), (object) "Create", (object) offlineDb.DatabaseName, (object) offlineDb.ConnectionName);
      if (Enumerable.Any<string>((IEnumerable<string>) stringList))
      {
        foreach (string str in stringList)
          this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (DatabaseOperationsTool), (object) request.Operation, (object) str);
      }
      return new DatabaseOperationResponse()
      {
        Success = true,
        Message = offlineDb.Message ?? $"Successfully created offline database '{offlineDb.DatabaseName}'",
        Operation = request.Operation,
        DatabaseName = offlineDb.DatabaseName,
        Warnings = stringList.Count > 0 ? stringList : (List<string>) null,
        Data = (object) offlineDb
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      DatabaseOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new DatabaseOperationResponse()
      {
        Success = false,
        Message = "Error creating database: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private DatabaseOperationResponse HandleExportTMDLOperation(DatabaseOperationRequest request)
  {
    try
    {
      TmdlExportResult tmdlExportResult = DatabaseOperations.ExportTMDL(request.ConnectionName, request.DatabaseName, request.TmdlExportOptions);
      this._logger.LogInformation("{ToolName}.{Operation} completed: DatabaseName={DatabaseName}", (object) nameof (DatabaseOperationsTool), (object) "ExportTMDL", (object) (request.DatabaseName ?? "loaded database"));
      return new DatabaseOperationResponse()
      {
        Success = true,
        Message = $"TMDL exported for database '{request.DatabaseName ?? "loaded database"}'",
        Operation = request.Operation,
        DatabaseName = request.DatabaseName,
        Data = (object) tmdlExportResult
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      DatabaseOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new DatabaseOperationResponse()
      {
        Success = false,
        Message = $"Failed to export TMDL for database '{request.DatabaseName}': {ex.Message}",
        Operation = request.Operation,
        DatabaseName = request.DatabaseName,
        Help = (object) operationMetadata
      };
    }
  }

  private DatabaseOperationResponse HandleExportTMSLOperation(DatabaseOperationRequest request)
  {
    try
    {
      TmslExportResult tmslExportResult = DatabaseOperations.ExportTMSL(request.ConnectionName, request.DatabaseName, request.TmslExportOptions);
      this._logger.LogInformation("{ToolName}.{Operation} completed: DatabaseName={DatabaseName}, OperationType={OperationType}", (object) nameof (DatabaseOperationsTool), (object) "ExportTMSL", (object) (request.DatabaseName ?? "loaded database"), (object) tmslExportResult.OperationType);
      DatabaseOperationResponse operationResponse = new DatabaseOperationResponse { Success = tmslExportResult.Success };
      string str;
      if (!tmslExportResult.Success)
        str = tmslExportResult.ErrorMessage ?? "Unknown error";
      else
        str = $"TMSL {tmslExportResult.OperationType} script for database '{request.DatabaseName ?? "loaded database"}' generated successfully";
      operationResponse.Message = str;
      operationResponse.Operation = request.Operation;
      operationResponse.DatabaseName = request.DatabaseName;
      operationResponse.Data = (object) tmslExportResult;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      DatabaseOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new DatabaseOperationResponse()
      {
        Success = false,
        Message = $"Failed to export TMSL for database '{request.DatabaseName}': {ex.Message}",
        Operation = request.Operation,
        DatabaseName = request.DatabaseName,
        Help = (object) operationMetadata
      };
    }
  }

  private DatabaseOperationResponse HandleHelpOperation(
    DatabaseOperationRequest request,
    string[] operations)
  {
    this._logger.LogInformation("{ToolName}.{Operation} completed: Operations={OperationCount}", (object) nameof (DatabaseOperationsTool), (object) "Help", (object) operations.Length);
    return new DatabaseOperationResponse()
    {
      Success = true,
      Message = "Tool description retrieved successfully",
      Operation = request.Operation,
      Help = (object) new
      {
        ToolName = "database_operations",
        Description = "Perform operations on Analysis Services databases",
        SupportedOperations = operations,
        Examples = Enumerable.Where<KeyValuePair<string, OperationMetadata>>((IEnumerable<KeyValuePair<string, OperationMetadata>>) DatabaseOperationsTool.toolMetadata.Operations, (Func<KeyValuePair<string, OperationMetadata>, bool>) (p => Enumerable.Contains<string>((IEnumerable<string>) operations, p.Key, (IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase))),
        Notes = new string[6]
        {
          "For Update operation, the UpdateDefinition must include the database name to update.",
          "For DeserializeFromFolder operation, the TMDL folder path must be provided.",
          "For SerializeToFolder operation, the TMDL folder path must be provided.",
          "For Deploy operation, the target connection name must be provided.",
          "For CreateOffline operation, the CreateDefinition must be provided.",
          "For ExportTMDL operation, the ConnectionName and DatabaseName must be provided."
        }
      }
    };
  }

  private bool ValidateRequest(string operation, DatabaseOperationRequest request)
  {
    OperationMetadata operationMetadata;
    if (!DatabaseOperationsTool.toolMetadata.Operations.TryGetValue(operation, out operationMetadata))
      return true;
    JsonObject requestDict = JsonSerializer.SerializeToNode<DatabaseOperationRequest>(request) as JsonObject;
    List<string> list1 = Enumerable.ToList<string>(Enumerable.Where<string>((IEnumerable<string>) operationMetadata.RequiredParams, (p => requestDict != null && requestDict[p] == null)));
    List<string> list2 = Enumerable.ToList<string>(Enumerable.Where<string>((IEnumerable<string>) operationMetadata.ForbiddenParams, (p => requestDict != null && requestDict[p] != null)));
    if (Enumerable.Any<string>((IEnumerable<string>) list1))
      throw new McpException($"Missing required parameters needed for {operation} operation: {string.Join(", ", (IEnumerable<string>) list1)}");
    if (Enumerable.Any<string>((IEnumerable<string>) list2))
      throw new McpException($"Forbidden parameters not allowed for {operation} operation: {string.Join(", ", (IEnumerable<string>) list2)}");
    return true;
  }

  static DatabaseOperationsTool()
  {
    ToolMetadata toolMetadata1 = new ToolMetadata();
    ToolMetadata toolMetadata2 = toolMetadata1;
    Dictionary<string, OperationMetadata> dictionary1 = new Dictionary<string, OperationMetadata>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    Dictionary<string, OperationMetadata> dictionary2 = dictionary1;
    OperationMetadata operationMetadata1 = new OperationMetadata { Description = "List all databases on the server.\r\nFor offline connections, returns information about the single loaded database.\r\nFor online connections, lists all databases on the server.\r\nMandatory properties: None.\r\nOptional: ConnectionName." };
    List<string> stringList1 = new List<string>();
    stringList1.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"List\"\r\n    }\r\n}");
    operationMetadata1.ExampleRequests = stringList1;
    dictionary2["List"] = operationMetadata1;
    Dictionary<string, OperationMetadata> dictionary3 = dictionary1;
    OperationMetadata operationMetadata2 = new OperationMetadata { RequiredParams = new string[1]
    {
      "UpdateDefinition"
    } };
    operationMetadata2.Description = "Update an existing database's modifiable properties.\r\nNames cannot be changed - use existing database name only.\r\nSupports updating Description, CompatibilityLevel, Collation, Language, and Annotations.\r\nMandatory properties: UpdateDefinition (with Name).\r\nOptional: DatabaseName (must match UpdateDefinition.Name if provided), CompatibilityLevel, Description, Collation, Language, Annotations.";
    OperationMetadata operationMetadata3 = operationMetadata2;
    List<string> stringList2 = new List<string>();
    stringList2.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Update\",\r\n        \"ConnectionName\": \"MyConnection\",\r\n        \"DatabaseName\": \"SalesDB\",\r\n        \"UpdateDefinition\": { \r\n            \"Name\": \"SalesDB\",\r\n            \"NewName\": \"SalesDB2\",\r\n            \"Description\": \"Updated Sales database\"\r\n        }\r\n    }\r\n}");
    operationMetadata3.ExampleRequests = stringList2;
    OperationMetadata operationMetadata4 = operationMetadata2;
    dictionary3["Update"] = operationMetadata4;
    Dictionary<string, OperationMetadata> dictionary4 = dictionary1;
    OperationMetadata operationMetadata5 = new OperationMetadata { RequiredParams = new string[1]
    {
      "CreateDefinition"
    } };
    operationMetadata5.Description = "Create a new empty database with an empty model.\r\nCurrently only supports offline database creation (IsOffline defaults to true).\r\nCreates a new offline connection automatically if ConnectionName not provided or doesn't exist.\r\nMandatory properties: CreateDefinition (with Name).\r\nOptional: IsOffline (defaults to true), Description, CompatibilityLevel, Collation, Language, Annotations, ModelName.";
    OperationMetadata operationMetadata6 = operationMetadata5;
    List<string> stringList3 = new List<string>();
    stringList3.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Create\",\r\n        \"CreateDefinition\": { \r\n            \"Name\": \"NewDB\",\r\n            \"IsOffline\": true\r\n        }\r\n    }\r\n}");
    operationMetadata6.ExampleRequests = stringList3;
    OperationMetadata operationMetadata7 = operationMetadata5;
    dictionary4["Create"] = operationMetadata7;
    Dictionary<string, OperationMetadata> dictionary5 = dictionary1;
    OperationMetadata operationMetadata8 = new OperationMetadata { RequiredParams = new string[1]
    {
      "TmdlFolderPath"
    } };
    operationMetadata8.Description = "Create an offline connection from a TMDL folder.\r\nImports and deserializes a TMDL folder structure into a new offline database connection.\r\nAutomatically generates connection name if not provided.\r\nMandatory properties: TmdlFolderPath.\r\nOptional: ConnectionName (auto-generated if not provided).";
    OperationMetadata operationMetadata9 = operationMetadata8;
    List<string> stringList4 = new List<string>();
    stringList4.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"ImportFromTmdlFolder\",\r\n        \"TmdlFolderPath\": \"C:/TMDL/Sales.SemanticModel/defintion\"\r\n    }\r\n}");
    operationMetadata9.ExampleRequests = stringList4;
    OperationMetadata operationMetadata10 = operationMetadata8;
    dictionary5["ImportFromTmdlFolder"] = operationMetadata10;
    Dictionary<string, OperationMetadata> dictionary6 = dictionary1;
    OperationMetadata operationMetadata11 = new OperationMetadata { Description = "Serialize a database to a TMDL folder.\r\nFor online connections, TmdlFolderPath is mandatory.\r\nFor offline connections, uses stored folder path if available, otherwise TmdlFolderPath is mandatory.\r\nMandatory properties: TmdlFolderPath (for online connections or offline connections without stored path).\r\nOptional: TmdlFolderPath (for offline connections with stored path)." };
    List<string> stringList5 = new List<string>();
    stringList5.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"ExportToTmdlFolder\",\r\n        \"ConnectionName\": \"MyConnection\",\r\n        \"TmdlFolderPath\": \"C:/TMDL/SalesDB\"\r\n    }\r\n}");
    operationMetadata11.ExampleRequests = stringList5;
    dictionary6["ExportToTmdlFolder"] = operationMetadata11;
    Dictionary<string, OperationMetadata> dictionary7 = dictionary1;
    OperationMetadata operationMetadata12 = new OperationMetadata { Description = "Export database to TMDL (YAML-like syntax) format.\r\nTMDL is a human-readable, declarative format for semantic models.\r\nFor online connections, DatabaseName is mandatory.\r\nFor offline connections, exports the loaded database.\r\nMandatory properties: DatabaseName (for online connections).\r\nOptional: DatabaseName (for offline connections), TmdlExportOptions." };
    List<string> stringList6 = new List<string>();
    stringList6.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"ExportTMDL\",\r\n        \"ConnectionName\": \"MyConnection\",\r\n        \"DatabaseName\": \"SalesDB\"\r\n    }\r\n}");
    operationMetadata12.ExampleRequests = stringList6;
    dictionary7["ExportTMDL"] = operationMetadata12;
    Dictionary<string, OperationMetadata> dictionary8 = dictionary1;
    OperationMetadata operationMetadata13 = new OperationMetadata { RequiredParams = new string[1]
    {
      "TmslExportOptions"
    } };
    operationMetadata13.Description = "Export database to TMSL (JSON syntax) script format with specified operation type.\r\nTMSL generates executable JSON scripts for database operations.\r\nFor online connections, DatabaseName should be specified.\r\nFor offline connections, exports the loaded database.\r\nMandatory properties: TmslExportOptions (with TmslOperationType).\r\nOptional: DatabaseName, RefreshType, IncludeRestricted, MaxReturnCharacters.";
    OperationMetadata operationMetadata14 = operationMetadata13;
    List<string> stringList7 = new List<string>();
    stringList7.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"ExportTMSL\",\r\n        \"ConnectionName\": \"MyConnection\",\r\n        \"DatabaseName\": \"SalesDB\",\r\n        \"TmslExportOptions\": {\r\n            \"TmslOperationType\": \"CreateOrReplace\"\r\n        }\r\n    }\r\n}");
    stringList7.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"ExportTMSL\",\r\n        \"DatabaseName\": \"SalesDB\",\r\n        \"TmslExportOptions\": {\r\n            \"TmslOperationType\": \"Refresh\",\r\n            \"RefreshType\": \"Full\",\r\n            \"IncludeRestricted\": true\r\n        }\r\n    }\r\n}");
    operationMetadata14.ExampleRequests = stringList7;
    OperationMetadata operationMetadata15 = operationMetadata13;
    dictionary8["ExportTMSL"] = operationMetadata15;
    Dictionary<string, OperationMetadata> dictionary9 = dictionary1;
    OperationMetadata operationMetadata16 = new OperationMetadata { RequiredParams = new string[2]
    {
      "ConnectionName",
      "DeployToFabricRequest"
    } };
    operationMetadata16.Description = "Deploy a database to Fabric using TMSL createOrReplace script.\r\nSupports deployment via direct connection string or workspace name.\r\nRequires either TargetConnectionString OR TargetWorkspaceName in the request.\r\nMandatory properties: DeployToFabricRequest (with either TargetConnectionString OR TargetWorkspaceName).\r\nOptional: TargetTenantName (defaults to 'myorg'), NewDatabaseName, IncludeRestricted, ConnectTimeoutSeconds.";
    OperationMetadata operationMetadata17 = operationMetadata16;
    List<string> stringList8 = new List<string>();
    stringList8.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"DeployToFabric\",\r\n        \"ConnectionName\": \"SourceConn\",\r\n        \"DeployToFabricRequest\": {\r\n            \"TargetConnectionString\": \"Data Source=powerbi://api.powerbi.com/v1.0/myorg/MyWorkspace\",\r\n            \"NewDatabaseName\": \"DeployedModel\",\r\n            \"ConnectTimeoutSeconds\": 300\r\n        }\r\n    }\r\n}");
    stringList8.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"DeployToFabric\",\r\n        \"ConnectionName\": \"SourceConn\",\r\n        \"DeployToFabricRequest\": {\r\n            \"TargetWorkspaceName\": \"MyWorkspace\",\r\n            \"TargetTenantName\": \"myorg\",\r\n            \"NewDatabaseName\": \"DeployedModel\",\r\n            \"IncludeRestricted\": false\r\n        }\r\n    }\r\n}");
    operationMetadata17.ExampleRequests = stringList8;
    OperationMetadata operationMetadata18 = operationMetadata16;
    dictionary9["DeployToFabric"] = operationMetadata18;
    Dictionary<string, OperationMetadata> dictionary10 = dictionary1;
    OperationMetadata operationMetadata19 = new OperationMetadata { Description = "Describe the tool and its operations.\r\nProvides comprehensive information about all available database operations.\r\nMandatory properties: None.\r\nOptional: None." };
    List<string> stringList9 = new List<string>();
    stringList9.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Help\"\r\n    }\r\n}");
    operationMetadata19.ExampleRequests = stringList9;
    dictionary10["Help"] = operationMetadata19;
    Dictionary<string, OperationMetadata> dictionary11 = dictionary1;
    toolMetadata2.Operations = dictionary11;
    DatabaseOperationsTool.toolMetadata = toolMetadata1;
  }
}
