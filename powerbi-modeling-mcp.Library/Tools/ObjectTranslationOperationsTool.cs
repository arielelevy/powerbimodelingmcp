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
public class ObjectTranslationOperationsTool
{
  private readonly ILogger<ObjectTranslationOperationsTool> _logger;
  public static readonly ToolMetadata toolMetadata;

  public ObjectTranslationOperationsTool(ILogger<ObjectTranslationOperationsTool> logger)
  {
    this._logger = logger;
  }

  [McpServerTool(Name = "object_translation_operations")]
  [Description("Perform operations on object translations within semantic model cultures. Supported operations: Help, Create, Update, Delete, Get, List. Use the Operation parameter to specify which operation to perform. Supported object types: Model, Table, Measure, Column, Hierarchy, Level, KPI.")]
  public ObjectTranslationOperationResponse ExecuteObjectTranslationOperation(
    McpServer mcpServer,
    ObjectTranslationOperationRequest request)
  {
    this._logger.LogDebug("Executing {ToolName}.{Operation}: Connection={ConnectionName}", (object) nameof (ObjectTranslationOperationsTool), (object) request.Operation, (object) (request.ConnectionName ?? "(last used)"));
    try
    {
      string[] strArray1 = new string[6]
      {
        "CREATE",
        "UPDATE",
        "DELETE",
        "GET",
        "LIST",
        "HELP"
      };
      string[] strArray2 = new string[3]
      {
        "CREATE",
        "UPDATE",
        "DELETE"
      };
      if (!Enumerable.Contains<string>((IEnumerable<string>) strArray1, request.Operation.ToUpperInvariant()))
      {
        this._logger.LogWarning("Invalid operation '{Operation}' requested for {ToolName}. Valid operations: {ValidOperations}", (object) request.Operation, (object) nameof (ObjectTranslationOperationsTool), (object) string.Join(", ", strArray1));
        return ObjectTranslationOperationResponse.Forbidden(request.Operation, $"Invalid operation: {request.Operation}. Supported operations: {string.Join(", ", strArray1)}");
      }
      if (!this.ValidateRequest(request.Operation, request))
        throw new McpException($"Invalid request for {request.Operation} operation.");
      if (Enumerable.Contains<string>((IEnumerable<string>) strArray2, request.Operation.ToUpperInvariant()))
      {
        WriteOperationResult writeOperationResult = WriteGuard.ExecuteWriteOperationWithGuards(mcpServer, request.ConnectionName, request.Operation);
        if (!writeOperationResult.Success)
        {
          this._logger.LogWarning("{ToolName}.{Operation} blocked by write guard: {Reason}", (object) nameof (ObjectTranslationOperationsTool), (object) request.Operation, (object) writeOperationResult.Message);
          return ObjectTranslationOperationResponse.Forbidden(request.Operation, writeOperationResult.Message);
        }
      }
      bool allowed = WriteGuard.IsWriteAllowed("").allowed;
      string upperInvariant = request.Operation.ToUpperInvariant();
      return (upperInvariant == "CREATE") ? this.HandleCreateOperation(request) : ((upperInvariant == "UPDATE") ? this.HandleUpdateOperation(request) : ((upperInvariant == "DELETE") ? this.HandleDeleteOperation(request) : ((upperInvariant == "GET") ? this.HandleGetOperation(request) : ((upperInvariant == "LIST") ? this.HandleListOperation(request) : ((upperInvariant == "HELP") ? this.HandleHelpOperation(request, allowed ? strArray1 : Enumerable.ToArray<string>(Enumerable.Except<string>((IEnumerable<string>) strArray1, (IEnumerable<string>) strArray2))) : ObjectTranslationOperationResponse.Forbidden(request.Operation, $"Operation {request.Operation} is not implemented"))))));
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Error executing {ToolName}.{Operation}: {ErrorMessage}", (object) nameof (ObjectTranslationOperationsTool), (object) request.Operation, (object) ex.Message);
      return new ObjectTranslationOperationResponse()
      {
        Success = false,
        Message = "Error executing object translation operation: " + ex.Message,
        Operation = request.Operation
      };
    }
  }

  private ObjectTranslationOperationResponse HandleCreateOperation(
    ObjectTranslationOperationRequest request)
  {
    try
    {
      ObjectTranslationOperations.ObjectTranslationOperationResult objectTranslation = ObjectTranslationOperations.CreateObjectTranslation(request.ConnectionName, request.CreateDefinition);
      ObjectTranslationOperationResponse operation = new ObjectTranslationOperationResponse()
      {
        Success = objectTranslation.Success,
        Message = objectTranslation.Message ?? (objectTranslation.Success ? "Object translation creation completed" : objectTranslation.ErrorMessage ?? "Object translation creation failed"),
        Operation = request.Operation,
        CultureName = objectTranslation.CultureName,
        ObjectType = objectTranslation.ObjectType,
        ObjectDisplayName = objectTranslation.ObjectDisplayName,
        Property = objectTranslation.Property,
        Value = objectTranslation.Value,
        Data = (object) objectTranslation
      };
      if (operation.Success)
        this._logger.LogInformation("{ToolName}.{Operation} completed: Culture={CultureName}, ObjectType={ObjectType}, Object={ObjectName}, Property={Property}", (object) nameof (ObjectTranslationOperationsTool), (object) request.Operation, (object) objectTranslation.CultureName, (object) objectTranslation.ObjectType, (object) objectTranslation.ObjectDisplayName, (object) objectTranslation.Property);
      else
        this._logger.LogWarning("{ToolName}.{Operation} failed: Culture={CultureName}, ObjectType={ObjectType}, Message={Message}", (object) nameof (ObjectTranslationOperationsTool), (object) request.Operation, (object) objectTranslation.CultureName, (object) objectTranslation.ObjectType, (object) objectTranslation.ErrorMessage);
      return operation;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      ObjectTranslationOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new ObjectTranslationOperationResponse()
      {
        Success = false,
        Message = "Failed to create object translation: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private ObjectTranslationOperationResponse HandleUpdateOperation(
    ObjectTranslationOperationRequest request)
  {
    try
    {
      ObjectTranslationOperations.ObjectTranslationOperationResult translationOperationResult = ObjectTranslationOperations.UpdateObjectTranslation(request.ConnectionName, request.UpdateDefinition);
      ObjectTranslationOperationResponse operationResponse = new ObjectTranslationOperationResponse()
      {
        Success = translationOperationResult.Success,
        Message = translationOperationResult.Message ?? (translationOperationResult.Success ? "Object translation update completed" : translationOperationResult.ErrorMessage ?? "Object translation update failed"),
        Operation = request.Operation,
        CultureName = translationOperationResult.CultureName,
        ObjectType = translationOperationResult.ObjectType,
        ObjectDisplayName = translationOperationResult.ObjectDisplayName,
        Property = translationOperationResult.Property,
        Value = translationOperationResult.Value,
        Data = (object) translationOperationResult
      };
      if (operationResponse.Success)
        this._logger.LogInformation("{ToolName}.{Operation} completed: Culture={CultureName}, ObjectType={ObjectType}, Object={ObjectName}, Property={Property}", (object) nameof (ObjectTranslationOperationsTool), (object) request.Operation, (object) translationOperationResult.CultureName, (object) translationOperationResult.ObjectType, (object) translationOperationResult.ObjectDisplayName, (object) translationOperationResult.Property);
      else
        this._logger.LogWarning("{ToolName}.{Operation} failed: Culture={CultureName}, ObjectType={ObjectType}, Message={Message}", (object) nameof (ObjectTranslationOperationsTool), (object) request.Operation, (object) translationOperationResult.CultureName, (object) translationOperationResult.ObjectType, (object) translationOperationResult.ErrorMessage);
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      ObjectTranslationOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new ObjectTranslationOperationResponse()
      {
        Success = false,
        Message = "Failed to update object translation: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private ObjectTranslationOperationResponse HandleDeleteOperation(
    ObjectTranslationOperationRequest request)
  {
    try
    {
      ObjectTranslationOperations.ObjectTranslationOperationResult translationOperationResult = ObjectTranslationOperations.DeleteObjectTranslation(request.ConnectionName, request.DeleteDefinition);
      ObjectTranslationOperationResponse operationResponse = new ObjectTranslationOperationResponse()
      {
        Success = translationOperationResult.Success,
        Message = translationOperationResult.Message ?? (translationOperationResult.Success ? "Object translation deletion completed" : translationOperationResult.ErrorMessage ?? "Object translation deletion failed"),
        Operation = request.Operation,
        CultureName = translationOperationResult.CultureName,
        ObjectType = translationOperationResult.ObjectType,
        ObjectDisplayName = translationOperationResult.ObjectDisplayName,
        Property = translationOperationResult.Property,
        Data = (object) translationOperationResult
      };
      if (operationResponse.Success)
        this._logger.LogInformation("{ToolName}.{Operation} completed: Culture={CultureName}, ObjectType={ObjectType}, Object={ObjectName}, Property={Property}", (object) nameof (ObjectTranslationOperationsTool), (object) request.Operation, (object) translationOperationResult.CultureName, (object) translationOperationResult.ObjectType, (object) translationOperationResult.ObjectDisplayName, (object) translationOperationResult.Property);
      else
        this._logger.LogWarning("{ToolName}.{Operation} failed: Culture={CultureName}, ObjectType={ObjectType}, Message={Message}", (object) nameof (ObjectTranslationOperationsTool), (object) request.Operation, (object) translationOperationResult.CultureName, (object) translationOperationResult.ObjectType, (object) translationOperationResult.ErrorMessage);
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      ObjectTranslationOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new ObjectTranslationOperationResponse()
      {
        Success = false,
        Message = "Failed to delete object translation: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private ObjectTranslationOperationResponse HandleGetOperation(
    ObjectTranslationOperationRequest request)
  {
    try
    {
      ObjectTranslationGet objectTranslation = ObjectTranslationOperations.GetObjectTranslation(request.ConnectionName, request.GetDefinition);
      if (objectTranslation == null)
      {
        this._logger.LogWarning("{ToolName}.{Operation} completed: Translation not found", (object) nameof (ObjectTranslationOperationsTool), (object) request.Operation);
        return new ObjectTranslationOperationResponse()
        {
          Success = false,
          Message = "Object translation not found",
          Operation = request.Operation,
          CultureName = request.GetDefinition?.CultureName,
          ObjectType = request.GetDefinition?.ObjectType,
          ObjectDisplayName = request.GetDefinition != null ? TranslationHelper.GetObjectDisplayName(request.GetDefinition) : (string) null,
          Property = request.GetDefinition?.Property
        };
      }
      this._logger.LogInformation("{ToolName}.{Operation} completed: Culture={CultureName}, ObjectType={ObjectType}, Object={ObjectName}, Property={Property}", (object) nameof (ObjectTranslationOperationsTool), (object) request.Operation, (object) objectTranslation.CultureName, (object) objectTranslation.ObjectType, (object) TranslationHelper.GetObjectDisplayName((ObjectTranslationBase) objectTranslation), (object) objectTranslation.Property);
      return new ObjectTranslationOperationResponse()
      {
        Success = true,
        Message = "Object translation retrieved successfully",
        Operation = request.Operation,
        CultureName = objectTranslation.CultureName,
        ObjectType = objectTranslation.ObjectType,
        ObjectDisplayName = TranslationHelper.GetObjectDisplayName((ObjectTranslationBase) objectTranslation),
        Property = objectTranslation.Property,
        Value = objectTranslation.Value,
        Data = (object) objectTranslation
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      ObjectTranslationOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new ObjectTranslationOperationResponse()
      {
        Success = false,
        Message = "Failed to get object translation: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private ObjectTranslationOperationResponse HandleListOperation(
    ObjectTranslationOperationRequest request)
  {
    try
    {
      List<ObjectTranslationList> objectTranslationListList = ObjectTranslationOperations.ListObjectTranslations(request.ConnectionName, request.ListFilters?.FilterCultureName, request.ListFilters?.FilterObjectType, request.ListFilters?.FilterObjectName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Count={Count}", (object) nameof (ObjectTranslationOperationsTool), (object) request.Operation, (object) objectTranslationListList.Count);
      ObjectTranslationOperationResponse operationResponse = new ObjectTranslationOperationResponse { Success = true };
      operationResponse.Message = $"Retrieved {objectTranslationListList.Count} object translations";
      operationResponse.Operation = request.Operation;
      operationResponse.Data = (object) objectTranslationListList;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      ObjectTranslationOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new ObjectTranslationOperationResponse()
      {
        Success = false,
        Message = "Failed to list object translations: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private ObjectTranslationOperationResponse HandleHelpOperation(
    ObjectTranslationOperationRequest request,
    string[] operations)
  {
    this._logger.LogInformation("{ToolName}.{Operation} completed: Operations={OperationCount}", (object) nameof (ObjectTranslationOperationsTool), (object) request.Operation, (object) operations.Length);
    return new ObjectTranslationOperationResponse()
    {
      Success = true,
      Message = "Tool description retrieved successfully",
      Operation = request.Operation,
      Help = (object) new
      {
        ToolName = "object_translation_operations",
        Description = "Perform operations on object translations within semantic model cultures.",
        SupportedOperations = operations,
        Examples = Enumerable.Where<KeyValuePair<string, OperationMetadata>>((IEnumerable<KeyValuePair<string, OperationMetadata>>) ObjectTranslationOperationsTool.toolMetadata.Operations, (Func<KeyValuePair<string, OperationMetadata>, bool>) (p => Enumerable.Contains<string>((IEnumerable<string>) operations, p.Key, (IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase))),
        SupportedObjectTypes = Enumerable.ToArray<string>((IEnumerable<string>) TranslationHelper.ValidObjectTypes),
        ObjectIdentificationRequirements = new
        {
          Model = new string[1]{ "ModelName" },
          Table = new string[1]{ "TableName" },
          Measure = new string[2]
          {
            "MeasureName",
            "TableName (optional)"
          },
          Column = new string[2]
          {
            "TableName",
            "ColumnName"
          },
          Hierarchy = new string[2]
          {
            "TableName",
            "HierarchyName"
          },
          Level = new string[3]
          {
            "TableName",
            "HierarchyName",
            "LevelName"
          },
          KPI = new string[2]
          {
            "MeasureName",
            "TableName (optional)"
          }
        },
        Notes = new string[6]
        {
          "For Create operations, use CreateDefinition with appropriate object identification properties.",
          "For Update operations, use UpdateDefinition with appropriate object identification properties.",
          "For Get operations, use GetDefinition with object identification properties.",
          "For Delete operations, use DeleteDefinition with object identification properties.",
          "For List operation, use optional filters (FilterCultureName, FilterObjectType, FilterObjectName).",
          "Object identification uses specific properties instead of composite names."
        }
      }
    };
  }

  private bool ValidateRequest(string operation, ObjectTranslationOperationRequest request)
  {
    OperationMetadata operationMetadata;
    if (!ObjectTranslationOperationsTool.toolMetadata.Operations.TryGetValue(operation, out operationMetadata))
      return true;
    JsonObject requestDict = JsonSerializer.SerializeToNode<ObjectTranslationOperationRequest>(request) as JsonObject;
    List<string> list1 = Enumerable.ToList<string>(Enumerable.Where<string>((IEnumerable<string>) operationMetadata.RequiredParams, (p => requestDict != null && requestDict[p] == null)));
    List<string> list2 = Enumerable.ToList<string>(Enumerable.Where<string>((IEnumerable<string>) operationMetadata.ForbiddenParams, (p => requestDict != null && requestDict[p] != null)));
    if (Enumerable.Any<string>((IEnumerable<string>) list1))
      throw new McpException($"Missing required parameters needed for {operation} operation: {string.Join(", ", (IEnumerable<string>) list1)}");
    if (Enumerable.Any<string>((IEnumerable<string>) list2))
      throw new McpException($"Forbidden parameters not allowed for {operation} operation: {string.Join(", ", (IEnumerable<string>) list2)}");
    return true;
  }

  static ObjectTranslationOperationsTool()
  {
    ToolMetadata toolMetadata1 = new ToolMetadata();
    ToolMetadata toolMetadata2 = toolMetadata1;
    Dictionary<string, OperationMetadata> dictionary1 = new Dictionary<string, OperationMetadata>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    Dictionary<string, OperationMetadata> dictionary2 = dictionary1;
    OperationMetadata operationMetadata1 = new OperationMetadata { RequiredParams = new string[1]
    {
      "CreateDefinition"
    } };
    operationMetadata1.Description = "Create a new object translation. \r\nMandatory properties: CreateDefinition (with CultureName, ObjectType, Property, Value, and object identification properties based on ObjectType).\r\nOptional: CreateCultureIfNotExists (default: true).\r\nObject identification requirements by ObjectType:\r\n- Model: ModelName\r\n- Table: TableName  \r\n- Measure/KPI: MeasureName (TableName optional)\r\n- Column: TableName, ColumnName\r\n- Hierarchy: TableName, HierarchyName\r\n- Level: TableName, HierarchyName, LevelName";
    OperationMetadata operationMetadata2 = operationMetadata1;
    List<string> stringList1 = new List<string>();
    stringList1.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Create\",\r\n        \"CreateDefinition\": { \r\n            \"CultureName\": \"fr-FR\", \r\n            \"ObjectType\": \"Table\", \r\n            \"TableName\": \"Sales\", \r\n            \"Property\": \"Caption\", \r\n            \"Value\": \"Table des Ventes\" \r\n        }\r\n    }\r\n}");
    stringList1.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Create\",\r\n        \"CreateDefinition\": { \r\n            \"CultureName\": \"es-ES\", \r\n            \"ObjectType\": \"Measure\", \r\n            \"MeasureName\": \"Total Sales\", \r\n            \"TableName\": \"Sales\", \r\n            \"Property\": \"DisplayFolder\", \r\n            \"Value\": \"Métricas de Ventas\" \r\n        }\r\n    }\r\n}");
    stringList1.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Create\",\r\n        \"CreateDefinition\": { \r\n            \"CultureName\": \"de-DE\", \r\n            \"ObjectType\": \"Column\", \r\n            \"TableName\": \"Products\", \r\n            \"ColumnName\": \"CategoryName\", \r\n            \"Property\": \"Caption\", \r\n            \"Value\": \"Kategoriename\" \r\n        }\r\n    }\r\n}");
    stringList1.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Create\",\r\n        \"CreateDefinition\": { \r\n            \"CultureName\": \"it-IT\", \r\n            \"ObjectType\": \"Hierarchy\", \r\n            \"TableName\": \"Date\", \r\n            \"HierarchyName\": \"Calendar\", \r\n            \"Property\": \"Description\", \r\n            \"Value\": \"Gerarchia del calendario per l'analisi temporale\" \r\n        }\r\n    }\r\n}");
    stringList1.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Create\",\r\n        \"CreateDefinition\": { \r\n            \"CultureName\": \"nl-NL\", \r\n            \"ObjectType\": \"Measure\", \r\n            \"MeasureName\": \"Average Revenue\", \r\n            \"TableName\": \"Sales\", \r\n            \"Property\": \"Description\", \r\n            \"Value\": \"Gemiddelde inkomsten per verkoop berekening\" \r\n        }\r\n    }\r\n}");
    operationMetadata2.ExampleRequests = stringList1;
    OperationMetadata operationMetadata3 = operationMetadata1;
    dictionary2["Create"] = operationMetadata3;
    Dictionary<string, OperationMetadata> dictionary3 = dictionary1;
    OperationMetadata operationMetadata4 = new OperationMetadata { RequiredParams = new string[1]
    {
      "UpdateDefinition"
    } };
    operationMetadata4.Description = "Update an existing object translation.\r\nMandatory properties: UpdateDefinition (with CultureName, ObjectType, Property, Value, and object identification properties based on ObjectType).\r\nObject identification requirements by ObjectType:\r\n- Model: ModelName\r\n- Table: TableName  \r\n- Measure/KPI: MeasureName (TableName optional)\r\n- Column: TableName, ColumnName\r\n- Hierarchy: TableName, HierarchyName\r\n- Level: TableName, HierarchyName, LevelName";
    OperationMetadata operationMetadata5 = operationMetadata4;
    List<string> stringList2 = new List<string>();
    stringList2.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Update\",\r\n        \"UpdateDefinition\": { \r\n            \"CultureName\": \"fr-FR\", \r\n            \"ObjectType\": \"Table\", \r\n            \"TableName\": \"Sales\", \r\n            \"Property\": \"Caption\", \r\n            \"Value\": \"Tableau des Ventes Mis à Jour\" \r\n        }\r\n    }\r\n}");
    stringList2.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Update\",\r\n        \"UpdateDefinition\": { \r\n            \"CultureName\": \"pt-BR\", \r\n            \"ObjectType\": \"KPI\", \r\n            \"MeasureName\": \"Revenue Target\", \r\n            \"Property\": \"DisplayFolder\", \r\n            \"Value\": \"Indicadores de Performance\\\\Vendas\" \r\n        }\r\n    }\r\n}");
    stringList2.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Update\",\r\n        \"UpdateDefinition\": { \r\n            \"CultureName\": \"ja-JP\", \r\n            \"ObjectType\": \"Level\", \r\n            \"TableName\": \"Date\", \r\n            \"HierarchyName\": \"Calendar\", \r\n            \"LevelName\": \"Year\", \r\n            \"Property\": \"Caption\", \r\n            \"Value\": \"年\" \r\n        }\r\n    }\r\n}");
    operationMetadata5.ExampleRequests = stringList2;
    OperationMetadata operationMetadata6 = operationMetadata4;
    dictionary3["Update"] = operationMetadata6;
    Dictionary<string, OperationMetadata> dictionary4 = dictionary1;
    OperationMetadata operationMetadata7 = new OperationMetadata { RequiredParams = new string[1]
    {
      "DeleteDefinition"
    } };
    operationMetadata7.Description = "Delete an object translation.\r\nMandatory properties: DeleteDefinition (with CultureName, ObjectType, Property, and object identification properties based on ObjectType).\r\nObject identification requirements by ObjectType:\r\n- Model: ModelName\r\n- Table: TableName  \r\n- Measure/KPI: MeasureName (TableName optional)\r\n- Column: TableName, ColumnName\r\n- Hierarchy: TableName, HierarchyName\r\n- Level: TableName, HierarchyName, LevelName";
    OperationMetadata operationMetadata8 = operationMetadata7;
    List<string> stringList3 = new List<string>();
    stringList3.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Delete\",\r\n        \"DeleteDefinition\": {\r\n            \"CultureName\": \"fr-FR\",\r\n            \"ObjectType\": \"Table\",\r\n            \"TableName\": \"Sales\",\r\n            \"Property\": \"Caption\"\r\n        }\r\n    }\r\n}");
    stringList3.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Delete\",\r\n        \"DeleteDefinition\": {\r\n            \"CultureName\": \"es-ES\",\r\n            \"ObjectType\": \"Measure\",\r\n            \"MeasureName\": \"Total Sales\",\r\n            \"TableName\": \"Sales\",\r\n            \"Property\": \"DisplayFolder\"\r\n        }\r\n    }\r\n}");
    stringList3.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Delete\",\r\n        \"DeleteDefinition\": {\r\n            \"CultureName\": \"de-DE\",\r\n            \"ObjectType\": \"Column\",\r\n            \"TableName\": \"Products\",\r\n            \"ColumnName\": \"CategoryName\",\r\n            \"Property\": \"Caption\"\r\n        }\r\n    }\r\n}");
    operationMetadata8.ExampleRequests = stringList3;
    OperationMetadata operationMetadata9 = operationMetadata7;
    dictionary4["Delete"] = operationMetadata9;
    Dictionary<string, OperationMetadata> dictionary5 = dictionary1;
    OperationMetadata operationMetadata10 = new OperationMetadata { RequiredParams = new string[1]
    {
      "GetDefinition"
    } };
    operationMetadata10.Description = "Get an object translation.\r\nMandatory properties: GetDefinition (with CultureName, ObjectType, Property, and object identification properties based on ObjectType).\r\nObject identification requirements by ObjectType:\r\n- Model: ModelName\r\n- Table: TableName  \r\n- Measure/KPI: MeasureName (TableName optional)\r\n- Column: TableName, ColumnName\r\n- Hierarchy: TableName, HierarchyName\r\n- Level: TableName, HierarchyName, LevelName";
    OperationMetadata operationMetadata11 = operationMetadata10;
    List<string> stringList4 = new List<string>();
    stringList4.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Get\",\r\n        \"GetDefinition\": {\r\n            \"CultureName\": \"fr-FR\",\r\n            \"ObjectType\": \"Table\",\r\n            \"TableName\": \"Sales\",\r\n            \"Property\": \"Caption\"\r\n        }\r\n    }\r\n}");
    stringList4.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Get\",\r\n        \"GetDefinition\": {\r\n            \"CultureName\": \"zh-CN\",\r\n            \"ObjectType\": \"Hierarchy\",\r\n            \"TableName\": \"Geography\",\r\n            \"HierarchyName\": \"Location\",\r\n            \"Property\": \"DisplayFolder\"\r\n        }\r\n    }\r\n}");
    stringList4.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Get\",\r\n        \"GetDefinition\": {\r\n            \"CultureName\": \"ko-KR\",\r\n            \"ObjectType\": \"Level\",\r\n            \"TableName\": \"Date\",\r\n            \"HierarchyName\": \"Calendar\",\r\n            \"LevelName\": \"Month\",\r\n            \"Property\": \"Caption\"\r\n        }\r\n    }\r\n}");
    stringList4.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Get\",\r\n        \"GetDefinition\": {\r\n            \"CultureName\": \"ar-SA\",\r\n            \"ObjectType\": \"KPI\",\r\n            \"MeasureName\": \"Sales Growth\",\r\n            \"Property\": \"Description\"\r\n        }\r\n    }\r\n}");
    operationMetadata11.ExampleRequests = stringList4;
    OperationMetadata operationMetadata12 = operationMetadata10;
    dictionary5["Get"] = operationMetadata12;
    Dictionary<string, OperationMetadata> dictionary6 = dictionary1;
    OperationMetadata operationMetadata13 = new OperationMetadata { Description = "List all object translations, with optional filters.\r\nOptional: ListFilters (with FilterCultureName, FilterObjectType, FilterObjectName, FilterProperty)." };
    List<string> stringList5 = new List<string>();
    stringList5.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"List\",\r\n        \"ListFilters\": {\r\n            \"FilterCultureName\": \"fr-FR\"\r\n        }\r\n    }\r\n}");
    stringList5.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"List\",\r\n        \"ListFilters\": {\r\n            \"FilterObjectType\": \"Measure\"\r\n        }\r\n    }\r\n}");
    stringList5.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"List\",\r\n        \"ListFilters\": {\r\n            \"FilterCultureName\": \"es-ES\",\r\n            \"FilterObjectType\": \"Column\",\r\n            \"FilterObjectName\": \"Sales\"\r\n        }\r\n    }\r\n}");
    operationMetadata13.ExampleRequests = stringList5;
    dictionary6["List"] = operationMetadata13;
    Dictionary<string, OperationMetadata> dictionary7 = dictionary1;
    OperationMetadata operationMetadata14 = new OperationMetadata { Description = "Describe the tool and its operations.\r\nNo mandatory properties required." };
    List<string> stringList6 = new List<string>();
    stringList6.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Help\"\r\n    }\r\n}");
    operationMetadata14.ExampleRequests = stringList6;
    dictionary7["Help"] = operationMetadata14;
    Dictionary<string, OperationMetadata> dictionary8 = dictionary1;
    toolMetadata2.Operations = dictionary8;
    ObjectTranslationOperationsTool.toolMetadata = toolMetadata1;
  }
}
