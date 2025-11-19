// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
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

#nullable enable
namespace PowerBIModelingMCP.Library.Tools;

[McpServerToolType]
public class ModelOperationsTool
{
  private readonly ILogger<ModelOperationsTool> _logger;
  public static readonly ToolMetadata toolMetadata;

  public ModelOperationsTool(ILogger<ModelOperationsTool> logger) => this._logger = logger;

  [McpServerTool(Name = "model_operations")]
  [Description("Perform operations on semantic model. Supported operations: Help, Get, Create, Update, Refresh, GetStats, Rename, ExportTMDL. Use the Operation parameter to specify which operation to perform. ExportTMDL exports the model to TMDL format.")]
  public ModelOperationResponse ExecuteModelOperation(
    McpServer mcpServer,
    ModelOperationRequest request)
  {
    this._logger.LogDebug("Executing {ToolName}.{Operation}: ModelName={ModelName}, Connection={ConnectionName}", (object) nameof (ModelOperationsTool), (object) request.Operation, (object) (request.ModelName ?? "(current)"), (object) (request.ConnectionName ?? "(last used)"));
    try
    {
      string[] strArray1 = new string[8]
      {
        "HELP",
        "GET",
        "CREATE",
        "UPDATE",
        "REFRESH",
        "GETSTATS",
        "RENAME",
        "EXPORTTMDL"
      };
      string[] strArray2 = new string[4]
      {
        "CREATE",
        "UPDATE",
        "REFRESH",
        "RENAME"
      };
      string upperInvariant = request.Operation.ToUpperInvariant();
      if (!Enumerable.Contains<string>((IEnumerable<string>) strArray1, upperInvariant))
      {
        this._logger.LogWarning("Invalid operation '{Operation}' requested for {ToolName}. Valid operations: {ValidOperations}", (object) request.Operation, (object) nameof (ModelOperationsTool), (object) string.Join(", ", strArray1));
        return ModelOperationResponse.Forbidden(request.Operation, $"Invalid operation: {request.Operation}. Supported operations: {string.Join(", ", strArray1)}", request.ModelName);
      }
      if (!this.ValidateRequest(request.Operation, request))
        throw new McpException($"Invalid request for {request.Operation} operation.");
      if (Enumerable.Contains<string>((IEnumerable<string>) strArray2, upperInvariant))
      {
        WriteOperationResult writeOperationResult = WriteGuard.ExecuteWriteOperationWithGuards(mcpServer, request.ConnectionName, request.Operation);
        if (!writeOperationResult.Success)
        {
          this._logger.LogWarning("{ToolName}.{Operation} blocked by write guard: {Reason}", (object) nameof (ModelOperationsTool), (object) request.Operation, (object) writeOperationResult.Message);
          return ModelOperationResponse.Forbidden(request.Operation, writeOperationResult.Message, request.ModelName);
        }
      }
      bool allowed = WriteGuard.IsWriteAllowed("").allowed;
      ModelOperationResponse operationResponse;
      if (upperInvariant != null)
      {
        switch (upperInvariant.Length)
        {
          case 3:
            if ((upperInvariant == "GET"))
            {
              operationResponse = this.HandleGetOperation(request);
              goto label_28;
            }
            break;
          case 4:
            if ((upperInvariant == "HELP"))
            {
              operationResponse = this.HandleHelpOperation(request, allowed ? strArray1 : Enumerable.ToArray<string>(Enumerable.Except<string>((IEnumerable<string>) strArray1, (IEnumerable<string>) strArray2)));
              goto label_28;
            }
            break;
          case 6:
            switch (upperInvariant[0])
            {
              case 'C':
                if ((upperInvariant == "CREATE"))
                {
                  operationResponse = this.HandleCreateOperation(request);
                  goto label_28;
                }
                break;
              case 'R':
                if ((upperInvariant == "RENAME"))
                {
                  operationResponse = this.HandleRenameOperation(request);
                  goto label_28;
                }
                break;
              case 'U':
                if ((upperInvariant == "UPDATE"))
                {
                  operationResponse = this.HandleUpdateOperation(request);
                  goto label_28;
                }
                break;
            }
            break;
          case 7:
            if ((upperInvariant == "REFRESH"))
            {
              operationResponse = this.HandleRefreshOperation(request);
              goto label_28;
            }
            break;
          case 8:
            if ((upperInvariant == "GETSTATS"))
            {
              operationResponse = this.HandleGetStatsOperation(request);
              goto label_28;
            }
            break;
          case 10:
            if ((upperInvariant == "EXPORTTMDL"))
            {
              operationResponse = this.HandleExportTMDLOperation(request);
              goto label_28;
            }
            break;
        }
      }
      operationResponse = ModelOperationResponse.Forbidden(request.Operation, $"Operation {request.Operation} not implemented", request.ModelName);
label_28:
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Error executing {ToolName}.{Operation}: {ErrorMessage}", (object) nameof (ModelOperationsTool), (object) request.Operation, (object) ex.Message);
      return new ModelOperationResponse()
      {
        Success = false,
        Message = "Error executing model operation: " + ex.Message,
        Operation = request.Operation
      };
    }
  }

  private ModelOperationResponse HandleGetOperation(ModelOperationRequest request)
  {
    try
    {
      ModelGet modelGet = this.ValidateRequest(request.Operation, request) ? ModelOperations.GetModel(request.ConnectionName) : throw new McpException($"Invalid request for {request.Operation} operation.");
      this._logger.LogInformation("{ToolName}.{Operation} completed: ModelName={ModelName}", (object) nameof (ModelOperationsTool), (object) request.Operation, (object) modelGet.Name);
      return new ModelOperationResponse()
      {
        Success = true,
        Message = $"Retrieved model '{modelGet.Name}' successfully",
        Operation = "GET",
        ModelName = modelGet.Name,
        Data = (object) modelGet
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      ModelOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new ModelOperationResponse()
      {
        Success = false,
        Message = "Failed to get model: " + ex.Message,
        Operation = "GET",
        ModelName = request.ModelName,
        Help = (object) operationMetadata
      };
    }
  }

  private ModelOperationResponse HandleCreateOperation(ModelOperationRequest request)
  {
    try
    {
      if (!this.ValidateRequest(request.Operation, request))
        throw new McpException($"Invalid request for {request.Operation} operation.");
      if (request.CreateDefinition == null)
        throw new McpException("CreateDefinition is required for Create operation.");
      bool? isOffline = request.CreateDefinition.IsOffline;
      bool flag = false;
      if (isOffline.GetValueOrDefault() == flag & isOffline.HasValue)
        throw new McpException("Only offline database creation is currently supported. Please set IsOffline to true or omit it (defaults to true).");
      List<string> stringList = new List<string>();
      if (!request.CreateDefinition.IsOffline.HasValue)
        stringList.Add("Creating a new offline database to hold the new model (this is the only supported create operation).");
      DatabaseCreate definition = new DatabaseCreate { Name = request.CreateDefinition.Name };
      definition.Description = request.CreateDefinition.Description;
      definition.Collation = request.CreateDefinition.Collation;
      definition.Annotations = request.CreateDefinition.Annotations;
      definition.ModelName = request.CreateDefinition.ModelName;
      definition.IsOffline = request.CreateDefinition.IsOffline;
      DatabaseCreateResult offlineDb = DatabaseOperations.CreateOfflineDb(definition, request.ConnectionName);
      if (stringList.Count > 0)
      {
        foreach (string str in stringList)
          this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (ModelOperationsTool), (object) request.Operation, (object) str);
      }
      this._logger.LogInformation("{ToolName}.{Operation} completed: ModelName={ModelName}, DatabaseName={DatabaseName}", (object) nameof (ModelOperationsTool), (object) request.Operation, (object) offlineDb.ModelName, (object) offlineDb.DatabaseName);
      ModelOperationResponse operation = new ModelOperationResponse { Success = true };
      operation.Message = $"Created model '{offlineDb.ModelName}' in database '{offlineDb.DatabaseName}' successfully";
      operation.Operation = "CREATE";
      operation.ModelName = offlineDb.ModelName;
      operation.Warnings = stringList.Count > 0 ? stringList : (List<string>) null;
      operation.Data = (object) new
      {
        ConnectionName = offlineDb.ConnectionName,
        DatabaseName = offlineDb.DatabaseName,
        ModelName = offlineDb.ModelName,
        CreatedAt = offlineDb.CreatedAt,
        IsOffline = true
      };
      return operation;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      ModelOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new ModelOperationResponse()
      {
        Success = false,
        Message = "Failed to create model: " + ex.Message,
        Operation = "CREATE",
        ModelName = request.CreateDefinition?.Name,
        Help = (object) operationMetadata
      };
    }
  }

  private ModelOperationResponse HandleUpdateOperation(ModelOperationRequest request)
  {
    try
    {
      if (string.IsNullOrEmpty(request.UpdateDefinition.Name))
        request.UpdateDefinition.Name = !string.IsNullOrEmpty(request.ModelName) ? request.ModelName : throw new McpException("Model name is required in UpdateDefinition or ModelName parameter.");
      else if (!string.IsNullOrEmpty(request.ModelName) && (request.UpdateDefinition.Name != request.ModelName))
        throw new McpException($"Model name mismatch: Request specifies '{request.ModelName}' but UpdateDefinition specifies '{request.UpdateDefinition.Name}'");
      ModelOperations.UpdateModel(request.ConnectionName, request.UpdateDefinition);
      this._logger.LogInformation("{ToolName}.{Operation} completed: ModelName={ModelName}", (object) nameof (ModelOperationsTool), (object) request.Operation, (object) (request.UpdateDefinition.Name ?? request.ModelName));
      return new ModelOperationResponse()
      {
        Success = true,
        Message = "Updated model successfully",
        Operation = "UPDATE",
        ModelName = request.UpdateDefinition.Name ?? request.ModelName
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      ModelOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new ModelOperationResponse()
      {
        Success = false,
        Message = "Failed to update model: " + ex.Message,
        Operation = "UPDATE",
        ModelName = request.ModelName,
        Help = (object) operationMetadata
      };
    }
  }

  private ModelOperationResponse HandleRefreshOperation(ModelOperationRequest request)
  {
    try
    {
      string refreshType = request.RefreshType ?? "Automatic";
      ModelOperations.RefreshModel(request.ConnectionName, refreshType);
      this._logger.LogInformation("{ToolName}.{Operation} completed: RefreshType={RefreshType}", (object) nameof (ModelOperationsTool), (object) request.Operation, (object) refreshType);
      return new ModelOperationResponse()
      {
        Success = true,
        Message = $"Refreshed model with refresh type '{refreshType}' successfully",
        Operation = "REFRESH",
        ModelName = request.ModelName
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      ModelOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new ModelOperationResponse()
      {
        Success = false,
        Message = "Failed to refresh model: " + ex.Message,
        Operation = "REFRESH",
        ModelName = request.ModelName,
        Help = (object) operationMetadata
      };
    }
  }

  private ModelOperationResponse HandleGetStatsOperation(ModelOperationRequest request)
  {
    try
    {
      Dictionary<string, object> modelStats = ModelOperations.GetModelStats(request.ConnectionName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: ModelName={ModelName}", (object) nameof (ModelOperationsTool), (object) request.Operation, modelStats.ContainsKey("ModelName") ? (object) modelStats["ModelName"]?.ToString() : (object) "(unknown)");
      return new ModelOperationResponse()
      {
        Success = true,
        Message = "Retrieved model statistics successfully",
        Operation = "GETSTATS",
        ModelName = modelStats.ContainsKey("ModelName") ? modelStats["ModelName"]?.ToString() : (string) null,
        Data = (object) modelStats
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      ModelOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new ModelOperationResponse()
      {
        Success = false,
        Message = "Failed to get model statistics: " + ex.Message,
        Operation = "GETSTATS",
        Help = (object) operationMetadata
      };
    }
  }

  private ModelOperationResponse HandleRenameOperation(ModelOperationRequest request)
  {
    try
    {
      ModelOperations.RenameModel(request.ConnectionName, request.NewName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: NewName={NewName}", (object) nameof (ModelOperationsTool), (object) request.Operation, (object) request.NewName);
      return new ModelOperationResponse()
      {
        Success = true,
        Message = $"Renamed model to '{request.NewName}' successfully",
        Operation = "RENAME",
        ModelName = request.NewName
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      ModelOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new ModelOperationResponse()
      {
        Success = false,
        Message = "Failed to rename model: " + ex.Message,
        Operation = "RENAME",
        ModelName = request.ModelName,
        Help = (object) operationMetadata
      };
    }
  }

  private ModelOperationResponse HandleExportTMDLOperation(ModelOperationRequest request)
  {
    try
    {
      string str = ModelOperations.ExportTMDL(request.ConnectionName, (ExportTmdl) request.TmdlExportOptions);
      this._logger.LogInformation("{ToolName}.{Operation} completed: ModelName={ModelName}", (object) nameof (ModelOperationsTool), (object) request.Operation, (object) (request.ModelName ?? "(current)"));
      return new ModelOperationResponse()
      {
        Success = true,
        Message = "TMDL exported for model",
        Operation = request.Operation,
        ModelName = request.ModelName,
        Data = (object) str
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      ModelOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new ModelOperationResponse()
      {
        Success = false,
        Message = "Failed to export TMDL for model: " + ex.Message,
        Operation = request.Operation,
        ModelName = request.ModelName,
        Help = (object) operationMetadata
      };
    }
  }

  private ModelOperationResponse HandleHelpOperation(
    ModelOperationRequest request,
    string[] operations)
  {
    this._logger.LogInformation("{ToolName}.{Operation} completed: Operations={OperationCount}", (object) nameof (ModelOperationsTool), (object) request.Operation, (object) operations.Length);
    return new ModelOperationResponse()
    {
      Success = true,
      Message = "Tool description retrieved successfully",
      Operation = request.Operation,
      Help = (object) new
      {
        ToolName = "model_operations",
        Description = "Perform operations on semantic model.",
        SupportedOperations = operations,
        Examples = Enumerable.Where<KeyValuePair<string, OperationMetadata>>((IEnumerable<KeyValuePair<string, OperationMetadata>>) ModelOperationsTool.toolMetadata.Operations, (Func<KeyValuePair<string, OperationMetadata>, bool>) (p => Enumerable.Contains<string>((IEnumerable<string>) operations, p.Key, (IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase))),
        Notes = new string[3]
        {
          "Check the operation description for details on each parameter.",
          "Use the Help operation to get this information.",
          "Use the Get operation to retrieve the current model or a specific model by name."
        }
      }
    };
  }

  private bool ValidateRequest(string operation, ModelOperationRequest request)
  {
    OperationMetadata operationMetadata;
    if (!ModelOperationsTool.toolMetadata.Operations.TryGetValue(operation, out operationMetadata))
      return true;
    JsonObject requestDict = JsonSerializer.SerializeToNode<ModelOperationRequest>(request) as JsonObject;
    List<string> list1 = Enumerable.ToList<string>(Enumerable.Where<string>((IEnumerable<string>) operationMetadata.RequiredParams, (p => requestDict != null && requestDict[p] == null)));
    List<string> list2 = Enumerable.ToList<string>(Enumerable.Where<string>((IEnumerable<string>) operationMetadata.ForbiddenParams, (p => requestDict != null && requestDict[p] != null)));
    if (Enumerable.Any<string>((IEnumerable<string>) list1))
      throw new McpException($"Missing required parameters needed for {operation} operation: {string.Join(", ", (IEnumerable<string>) list1)}");
    if (Enumerable.Any<string>((IEnumerable<string>) list2))
      throw new McpException($"Forbidden parameters not allowed for {operation} operation: {string.Join(", ", (IEnumerable<string>) list2)}");
    return true;
  }

  static ModelOperationsTool()
  {
    ToolMetadata toolMetadata1 = new ToolMetadata();
    ToolMetadata toolMetadata2 = toolMetadata1;
    Dictionary<string, OperationMetadata> dictionary1 = new Dictionary<string, OperationMetadata>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    Dictionary<string, OperationMetadata> dictionary2 = dictionary1;
    OperationMetadata operationMetadata1 = new OperationMetadata { Description = "Describe the tool and its operations.\r\nMandatory properties: None.\r\nOptional: None." };
    List<string> stringList1 = new List<string>();
    stringList1.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Help\"\r\n    }\r\n}");
    operationMetadata1.ExampleRequests = stringList1;
    dictionary2["Help"] = operationMetadata1;
    Dictionary<string, OperationMetadata> dictionary3 = dictionary1;
    OperationMetadata operationMetadata2 = new OperationMetadata { Description = "Get the model definition and properties including metadata, configuration settings, and structural information.\r\nMandatory properties: None.\r\nOptional: ModelName." };
    List<string> stringList2 = new List<string>();
    stringList2.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Get\",\r\n        \"ConnectionName\": \"MyConnection\"\r\n    }\r\n}");
    operationMetadata2.ExampleRequests = stringList2;
    dictionary3["Get"] = operationMetadata2;
    Dictionary<string, OperationMetadata> dictionary4 = dictionary1;
    OperationMetadata operationMetadata3 = new OperationMetadata { RequiredParams = new string[1]
    {
      "CreateDefinition"
    } };
    operationMetadata3.Description = "Create a new offline model within a new database. Only offline database creation is currently supported.\r\nMandatory properties: CreateDefinition (with Name).\r\nOptional: Description, Collation, ModelName, IsOffline (defaults to true), Annotations, ExtendedProperties, BindingInfos.";
    OperationMetadata operationMetadata4 = operationMetadata3;
    List<string> stringList3 = new List<string>();
    stringList3.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Create\",\r\n        \"ConnectionName\": \"NewModel\",\r\n        \"CreateDefinition\": {\r\n            \"Name\": \"SalesModel\",\r\n            \"Description\": \"Sales analysis model\",\r\n            \"IsOffline\": true\r\n        }\r\n    }\r\n}");
    operationMetadata4.ExampleRequests = stringList3;
    OperationMetadata operationMetadata5 = operationMetadata3;
    dictionary4["Create"] = operationMetadata5;
    Dictionary<string, OperationMetadata> dictionary5 = dictionary1;
    OperationMetadata operationMetadata6 = new OperationMetadata { RequiredParams = new string[1]
    {
      "UpdateDefinition"
    } };
    operationMetadata6.Description = "Update the model properties. Model names cannot be changed and must use the Rename operation instead.\r\nMandatory properties: UpdateDefinition (with Name) OR ModelName.\r\nOptional: Description, StorageLocation, DefaultMode, DefaultDataView, Culture, Collation, DataAccessOptions, DefaultMeasureTable, DefaultMeasureName, DefaultPowerBIDataSourceVersion, ForceUniqueNames, DiscourageImplicitMeasures, DiscourageReportMeasures, DataSourceVariablesOverrideBehavior, DataSourceDefaultMaxConnections, SourceQueryCulture, MAttributes, DiscourageCompositeModels, AutomaticAggregationOptions, DirectLakeBehavior, ValueFilterBehavior, SelectionExpressionBehavior, Annotations, ExtendedProperties, BindingInfos.\r\nNote: When ExtendedProperties, Annotations, or BindingInfos are provided, existing collections will be completely replaced (replace-all semantics).";
    OperationMetadata operationMetadata7 = operationMetadata6;
    List<string> stringList4 = new List<string>();
    stringList4.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Update\",\r\n        \"ConnectionName\": \"MyConnection\",\r\n        \"UpdateDefinition\": { \r\n            \"Name\": \"SalesModel\",\r\n            \"Description\": \"Updated model to direct query\",\r\n            \"DefaultMode\": \"DirectQuery\" \r\n        }\r\n    }\r\n}");
    operationMetadata7.ExampleRequests = stringList4;
    OperationMetadata operationMetadata8 = operationMetadata6;
    dictionary5["Update"] = operationMetadata8;
    Dictionary<string, OperationMetadata> dictionary6 = dictionary1;
    OperationMetadata operationMetadata9 = new OperationMetadata { Description = "Refresh the model to reload data from data sources.\r\nMandatory properties: None.\r\nOptional: RefreshType (Automatic, Full, ClearValues, Calculate, DataOnly, Defragment)." };
    List<string> stringList5 = new List<string>();
    stringList5.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Refresh\",\r\n        \"ConnectionName\": \"MyConnection\",\r\n        \"RefreshType\": \"Full\"\r\n    }\r\n}");
    operationMetadata9.ExampleRequests = stringList5;
    dictionary6["Refresh"] = operationMetadata9;
    Dictionary<string, OperationMetadata> dictionary7 = dictionary1;
    OperationMetadata operationMetadata10 = new OperationMetadata { Description = "Get model statistics including object counts and memory usage information.\r\nMandatory properties: None.\r\nOptional: None." };
    List<string> stringList6 = new List<string>();
    stringList6.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"GetStats\",\r\n        \"ConnectionName\": \"MyConnection\"\r\n    }\r\n}");
    operationMetadata10.ExampleRequests = stringList6;
    dictionary7["GetStats"] = operationMetadata10;
    Dictionary<string, OperationMetadata> dictionary8 = dictionary1;
    OperationMetadata operationMetadata11 = new OperationMetadata { RequiredParams = new string[1]
    {
      "NewName"
    } };
    operationMetadata11.Description = "Rename the model to a new name.\r\nMandatory properties: NewName.\r\nOptional: None.";
    OperationMetadata operationMetadata12 = operationMetadata11;
    List<string> stringList7 = new List<string>();
    stringList7.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Rename\",\r\n        \"NewName\": \"NewModel\"\r\n    }\r\n}");
    operationMetadata12.ExampleRequests = stringList7;
    OperationMetadata operationMetadata13 = operationMetadata11;
    dictionary8["Rename"] = operationMetadata13;
    Dictionary<string, OperationMetadata> dictionary9 = dictionary1;
    OperationMetadata operationMetadata14 = new OperationMetadata { Description = "Export model to TMDL format.\r\nMandatory properties: None.\r\nOptional: TmdlExportOptions." };
    List<string> stringList8 = new List<string>();
    stringList8.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"ExportTMDL\",\r\n        \"ConnectionName\": \"MyConnection\"\r\n    }\r\n}");
    operationMetadata14.ExampleRequests = stringList8;
    dictionary9["ExportTMDL"] = operationMetadata14;
    Dictionary<string, OperationMetadata> dictionary10 = dictionary1;
    toolMetadata2.Operations = dictionary10;
    ModelOperationsTool.toolMetadata = toolMetadata1;
  }
}
