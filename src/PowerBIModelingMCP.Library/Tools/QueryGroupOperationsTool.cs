// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Tools.QueryGroupOperationsTool
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

#nullable enable
namespace PowerBIModelingMCP.Library.Tools;

[McpServerToolType]
public class QueryGroupOperationsTool
{
  private readonly ILogger<QueryGroupOperationsTool> _logger;
  public static readonly ToolMetadata toolMetadata;

  public QueryGroupOperationsTool(ILogger<QueryGroupOperationsTool> logger)
  {
    this._logger = logger;
  }

  [McpServerTool(Name = "query_group_operations")]
  [Description("Perform operations on semantic model query groups. Supported operations: HELP, CREATE, UPDATE, DELETE, GET, LIST, ExportTMDL. Use the Operation parameter to specify which operation to perform.")]
  public QueryGroupOperationResponse ExecuteQueryGroupOperation(
    McpServer mcpServer,
    QueryGroupOperationRequest request)
  {
    this._logger.LogDebug("Executing {ToolName}.{Operation}: QueryGroup={QueryGroupName}, Connection={ConnectionName}", (object) nameof (QueryGroupOperationsTool), (object) request.Operation, (object) request.QueryGroupName, (object) (request.ConnectionName ?? "(last used)"));
    try
    {
      string[] strArray1 = new string[7]
      {
        "CREATE",
        "UPDATE",
        "DELETE",
        "GET",
        "LIST",
        "ExportTMDL",
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
        this._logger.LogWarning("Invalid operation '{Operation}' requested for {ToolName}. Valid operations: {ValidOperations}", (object) request.Operation, (object) nameof (QueryGroupOperationsTool), (object) string.Join(", ", strArray1));
        return QueryGroupOperationResponse.Forbidden(request.Operation, $"Invalid operation: {request.Operation}. Supported operations: {string.Join(", ", strArray1)}");
      }
      if (!this.ValidateRequest(request.Operation, request))
        throw new McpException($"Invalid request for {request.Operation} operation.");
      if (Enumerable.Contains<string>((IEnumerable<string>) strArray2, request.Operation.ToUpperInvariant()))
      {
        WriteOperationResult writeOperationResult = WriteGuard.ExecuteWriteOperationWithGuards(mcpServer, request.ConnectionName, request.Operation);
        if (!writeOperationResult.Success)
        {
          this._logger.LogWarning("{ToolName}.{Operation} blocked by write guard: {Reason}", (object) nameof (QueryGroupOperationsTool), (object) request.Operation, (object) writeOperationResult.Message);
          return new QueryGroupOperationResponse()
          {
            Success = false,
            Message = writeOperationResult.Message,
            Operation = request.Operation
          };
        }
      }
      bool allowed = WriteGuard.IsWriteAllowed("").allowed;
      string upperInvariant = request.Operation.ToUpperInvariant();
      QueryGroupOperationResponse operationResponse;
      if (upperInvariant != null)
      {
        switch (upperInvariant.Length)
        {
          case 3:
            if ((upperInvariant == "GET"))
            {
              operationResponse = this.HandleGetOperation(request);
              goto label_27;
            }
            break;
          case 4:
            switch (upperInvariant[0])
            {
              case 'H':
                if ((upperInvariant == "HELP"))
                {
                  operationResponse = this.HandleHelpOperation(request, allowed ? strArray1 : Enumerable.ToArray<string>(Enumerable.Except<string>((IEnumerable<string>) strArray1, (IEnumerable<string>) strArray2)));
                  goto label_27;
                }
                break;
              case 'L':
                if ((upperInvariant == "LIST"))
                {
                  operationResponse = this.HandleListOperation(request);
                  goto label_27;
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
                  goto label_27;
                }
                break;
              case 'D':
                if ((upperInvariant == "DELETE"))
                {
                  operationResponse = this.HandleDeleteOperation(request);
                  goto label_27;
                }
                break;
              case 'U':
                if ((upperInvariant == "UPDATE"))
                {
                  operationResponse = this.HandleUpdateOperation(request);
                  goto label_27;
                }
                break;
            }
            break;
          case 10:
            if ((upperInvariant == "EXPORTTMDL"))
            {
              operationResponse = this.HandleExportTMDLOperation(request);
              goto label_27;
            }
            break;
        }
      }
      operationResponse = QueryGroupOperationResponse.Forbidden(request.Operation.ToUpperInvariant(), $"Operation '{request.Operation}' is not implemented");
label_27:
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Error executing {ToolName}.{Operation}: {ErrorMessage}", (object) nameof (QueryGroupOperationsTool), (object) request.Operation, (object) ex.Message);
      return new QueryGroupOperationResponse()
      {
        Success = false,
        Message = "Error executing query group operation: " + ex.Message,
        Operation = request.Operation.ToUpperInvariant()
      };
    }
  }

  private QueryGroupOperationResponse HandleCreateOperation(QueryGroupOperationRequest request)
  {
    try
    {
      QueryGroupOperationResult queryGroup = QueryGroupOperations.CreateQueryGroup(request.ConnectionName, request.CreateDefinition);
      this._logger.LogInformation("{ToolName}.{Operation} completed: QueryGroup={QueryGroupName}", (object) nameof (QueryGroupOperationsTool), (object) "CREATE", (object) queryGroup.QueryGroupName);
      return new QueryGroupOperationResponse()
      {
        Success = true,
        Message = "Query group created successfully",
        Operation = "CREATE",
        QueryGroupName = queryGroup.QueryGroupName,
        OperationResult = queryGroup
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      QueryGroupOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new QueryGroupOperationResponse()
      {
        Success = false,
        Message = "Failed to create query group: " + ex.Message,
        Operation = "CREATE",
        Help = (object) operationMetadata
      };
    }
  }

  private QueryGroupOperationResponse HandleUpdateOperation(QueryGroupOperationRequest request)
  {
    try
    {
      if (!string.IsNullOrWhiteSpace(request.UpdateDefinition.Name))
      {
        if ((request.UpdateDefinition.Name != request.QueryGroupName))
          throw new McpException($"Query group name mismatch: Request specifies '{request.QueryGroupName}' but UpdateDefinition specifies '{request.UpdateDefinition.Name}'");
      }
      else
        request.UpdateDefinition.Name = request.QueryGroupName;
      QueryGroupOperationResult groupOperationResult = QueryGroupOperations.UpdateQueryGroup(request.ConnectionName, request.UpdateDefinition);
      this._logger.LogInformation("{ToolName}.{Operation} completed: QueryGroup={QueryGroupName}", (object) nameof (QueryGroupOperationsTool), (object) "UPDATE", (object) request.UpdateDefinition.Name);
      return new QueryGroupOperationResponse()
      {
        Success = true,
        Message = $"Query group '{request.UpdateDefinition.Name}' updated successfully",
        Operation = "UPDATE",
        QueryGroupName = request.UpdateDefinition.Name,
        OperationResult = groupOperationResult
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      QueryGroupOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new QueryGroupOperationResponse()
      {
        Success = false,
        Message = "Failed to update query group: " + ex.Message,
        Operation = "UPDATE",
        Help = (object) operationMetadata
      };
    }
  }

  private QueryGroupOperationResponse HandleDeleteOperation(QueryGroupOperationRequest request)
  {
    try
    {
      QueryGroupOperations.DeleteQueryGroup(request.ConnectionName, request.QueryGroupName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: QueryGroup={QueryGroupName}", (object) nameof (QueryGroupOperationsTool), (object) "DELETE", (object) request.QueryGroupName);
      return new QueryGroupOperationResponse()
      {
        Success = true,
        Message = $"Query group '{request.QueryGroupName}' deleted successfully",
        Operation = "DELETE",
        QueryGroupName = request.QueryGroupName
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      QueryGroupOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new QueryGroupOperationResponse()
      {
        Success = false,
        Message = "Failed to delete query group: " + ex.Message,
        Operation = "DELETE",
        Help = (object) operationMetadata
      };
    }
  }

  private QueryGroupOperationResponse HandleGetOperation(QueryGroupOperationRequest request)
  {
    try
    {
      QueryGroupGet queryGroup = QueryGroupOperations.GetQueryGroup(request.ConnectionName, request.QueryGroupName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: QueryGroup={QueryGroupName}", (object) nameof (QueryGroupOperationsTool), (object) "GET", (object) request.QueryGroupName);
      return new QueryGroupOperationResponse()
      {
        Success = true,
        Message = $"Query group '{request.QueryGroupName}' retrieved successfully",
        Operation = "GET",
        QueryGroupName = request.QueryGroupName,
        Data = (object) queryGroup
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      QueryGroupOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new QueryGroupOperationResponse()
      {
        Success = false,
        Message = "Failed to get query group: " + ex.Message,
        Operation = "GET",
        Help = (object) operationMetadata
      };
    }
  }

  private QueryGroupOperationResponse HandleListOperation(QueryGroupOperationRequest request)
  {
    try
    {
      List<QueryGroupList> queryGroupListList = QueryGroupOperations.ListQueryGroups(request.ConnectionName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Count={Count}", (object) nameof (QueryGroupOperationsTool), (object) "LIST", (object) queryGroupListList.Count);
      QueryGroupOperationResponse operationResponse = new QueryGroupOperationResponse { Success = true };
      operationResponse.Message = $"Found {queryGroupListList.Count} query groups";
      operationResponse.Operation = "LIST";
      operationResponse.Data = (object) queryGroupListList;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      QueryGroupOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new QueryGroupOperationResponse()
      {
        Success = false,
        Message = "Failed to list query groups: " + ex.Message,
        Operation = "LIST",
        Help = (object) operationMetadata
      };
    }
  }

  private QueryGroupOperationResponse HandleExportTMDLOperation(QueryGroupOperationRequest request)
  {
    try
    {
      string str = QueryGroupOperations.ExportTMDL(request.ConnectionName, request.QueryGroupName, (ExportTmdl) request.TmdlExportOptions);
      this._logger.LogInformation("{ToolName}.{Operation} completed: QueryGroup={QueryGroupName}", (object) nameof (QueryGroupOperationsTool), (object) "ExportTMDL", (object) request.QueryGroupName);
      return new QueryGroupOperationResponse()
      {
        Success = true,
        Message = "Query group TMDL exported successfully",
        Operation = "ExportTMDL",
        QueryGroupName = request.QueryGroupName,
        Data = (object) str
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      QueryGroupOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new QueryGroupOperationResponse()
      {
        Success = false,
        Message = "Failed to export query group TMDL: " + ex.Message,
        Operation = "ExportTMDL",
        Help = (object) operationMetadata
      };
    }
  }

  private QueryGroupOperationResponse HandleHelpOperation(
    QueryGroupOperationRequest request,
    string[] operations)
  {
    this._logger.LogInformation("{ToolName}.{Operation} completed: Operations={OperationCount}", (object) nameof (QueryGroupOperationsTool), (object) request.Operation, (object) operations.Length);
    return new QueryGroupOperationResponse()
    {
      Success = true,
      Message = "Query group operations tool help",
      Operation = request.Operation,
      Help = (object) new
      {
        ToolName = "query_group_operations",
        Description = "Perform operations on semantic model query groups.",
        SupportedOperations = operations,
        Examples = Enumerable.Where<KeyValuePair<string, OperationMetadata>>((IEnumerable<KeyValuePair<string, OperationMetadata>>) QueryGroupOperationsTool.toolMetadata.Operations, (Func<KeyValuePair<string, OperationMetadata>, bool>) (p => Enumerable.Contains<string>((IEnumerable<string>) operations, p.Key, (IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase))),
        Notes = new string[7]
        {
          "Query groups organize partitions and named expressions. They are NOT related to DAX queries.",
          "The Operation parameter specifies which operation to perform.",
          "For CREATE and UPDATE operations, the QueryGroupName parameter is required.",
          "For CREATE operations, the CreateDefinition parameter is required.",
          "For UPDATE operations, the UpdateDefinition parameter is required.",
          "For DELETE and GET operations, the QueryGroupName parameter is required.",
          "For ExportTMDL operation, the QueryGroupName parameter is required."
        }
      }
    };
  }

  private bool ValidateRequest(string operation, QueryGroupOperationRequest request)
  {
    OperationMetadata operationMetadata;
    if (!QueryGroupOperationsTool.toolMetadata.Operations.TryGetValue(operation, out operationMetadata))
      return true;
    JsonObject requestDict = JsonSerializer.SerializeToNode<QueryGroupOperationRequest>(request) as JsonObject;
    List<string> list1 = Enumerable.ToList<string>(Enumerable.Where<string>((IEnumerable<string>) operationMetadata.RequiredParams, (p => requestDict != null && requestDict[p] == null)));
    List<string> list2 = Enumerable.ToList<string>(Enumerable.Where<string>((IEnumerable<string>) operationMetadata.ForbiddenParams, (p => requestDict != null && requestDict[p] != null)));
    if (Enumerable.Any<string>((IEnumerable<string>) list1))
      throw new McpException($"Missing required parameters needed for {operation} operation: {string.Join(", ", (IEnumerable<string>) list1)}");
    if (Enumerable.Any<string>((IEnumerable<string>) list2))
      throw new McpException($"Forbidden parameters not allowed for {operation} operation: {string.Join(", ", (IEnumerable<string>) list2)}");
    return true;
  }

  static QueryGroupOperationsTool()
  {
    ToolMetadata toolMetadata1 = new ToolMetadata();
    ToolMetadata toolMetadata2 = toolMetadata1;
    Dictionary<string, OperationMetadata> dictionary1 = new Dictionary<string, OperationMetadata>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    Dictionary<string, OperationMetadata> dictionary2 = dictionary1;
    OperationMetadata operationMetadata1 = new OperationMetadata { RequiredParams = new string[1]
    {
      "CreateDefinition"
    } };
    operationMetadata1.Description = "Create a new query group in the semantic model. Query groups organize related queries and expressions.\r\nMandatory properties: CreateDefinition (with Folder).\r\nOptional: Description, Annotations.";
    OperationMetadata operationMetadata2 = operationMetadata1;
    List<string> stringList1 = new List<string>();
    stringList1.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Create\",\r\n        \"CreateDefinition\": { \r\n            \"Folder\": \"Sales\",\r\n            \"Description\": \"Sales-related queries\"\r\n        }\r\n    }\r\n}");
    operationMetadata2.ExampleRequests = stringList1;
    OperationMetadata operationMetadata3 = operationMetadata1;
    dictionary2["CREATE"] = operationMetadata3;
    Dictionary<string, OperationMetadata> dictionary3 = dictionary1;
    OperationMetadata operationMetadata4 = new OperationMetadata { RequiredParams = new string[2]
    {
      "QueryGroupName",
      "UpdateDefinition"
    } };
    operationMetadata4.Description = "Update an existing query group's properties. Names cannot be changed - use separate rename operation if needed.\r\nMandatory properties: QueryGroupName, UpdateDefinition (with Name).\r\nOptional: Description, Folder, Annotations.";
    OperationMetadata operationMetadata5 = operationMetadata4;
    List<string> stringList2 = new List<string>();
    stringList2.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Update\",\r\n        \"QueryGroupName\": \"SalesQueries\",\r\n        \"UpdateDefinition\": { \r\n            \"Name\": \"SalesQueries\",\r\n            \"Folder\": \"Sales\",\r\n            \"Description\": \"Updated queries for sales data\" \r\n        }\r\n    }\r\n}");
    operationMetadata5.ExampleRequests = stringList2;
    OperationMetadata operationMetadata6 = operationMetadata4;
    dictionary3["UPDATE"] = operationMetadata6;
    Dictionary<string, OperationMetadata> dictionary4 = dictionary1;
    OperationMetadata operationMetadata7 = new OperationMetadata { RequiredParams = new string[1]
    {
      "QueryGroupName"
    } };
    operationMetadata7.Description = "Delete a query group from the semantic model. Checks for dependencies (partitions and named expressions) before deletion.\r\nMandatory properties: QueryGroupName.";
    OperationMetadata operationMetadata8 = operationMetadata7;
    List<string> stringList3 = new List<string>();
    stringList3.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Delete\",\r\n        \"QueryGroupName\": \"ObsoleteQueryGroup\"\r\n    }\r\n}");
    operationMetadata8.ExampleRequests = stringList3;
    OperationMetadata operationMetadata9 = operationMetadata7;
    dictionary4["DELETE"] = operationMetadata9;
    Dictionary<string, OperationMetadata> dictionary5 = dictionary1;
    OperationMetadata operationMetadata10 = new OperationMetadata { RequiredParams = new string[1]
    {
      "QueryGroupName"
    } };
    operationMetadata10.Description = "Retrieve detailed metadata for a specific query group including properties and annotations.\r\nMandatory properties: QueryGroupName.";
    OperationMetadata operationMetadata11 = operationMetadata10;
    List<string> stringList4 = new List<string>();
    stringList4.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Get\",\r\n        \"QueryGroupName\": \"SalesQueries\"\r\n    }\r\n}");
    operationMetadata11.ExampleRequests = stringList4;
    OperationMetadata operationMetadata12 = operationMetadata10;
    dictionary5["GET"] = operationMetadata12;
    Dictionary<string, OperationMetadata> dictionary6 = dictionary1;
    OperationMetadata operationMetadata13 = new OperationMetadata { Description = "List all query groups in the semantic model with basic information (name, description, folder).\r\nOptional: None." };
    List<string> stringList5 = new List<string>();
    stringList5.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"List\"\r\n    }\r\n}");
    operationMetadata13.ExampleRequests = stringList5;
    dictionary6["LIST"] = operationMetadata13;
    Dictionary<string, OperationMetadata> dictionary7 = dictionary1;
    OperationMetadata operationMetadata14 = new OperationMetadata { RequiredParams = new string[1]
    {
      "QueryGroupName"
    } };
    operationMetadata14.Description = "Export a query group to TMDL format.\r\nMandatory properties: QueryGroupName.\r\nOptional: TmdlExportOptions.";
    OperationMetadata operationMetadata15 = operationMetadata14;
    List<string> stringList6 = new List<string>();
    stringList6.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"ExportTMDL\",\r\n        \"QueryGroupName\": \"SalesQueries\"\r\n    }\r\n}");
    operationMetadata15.ExampleRequests = stringList6;
    OperationMetadata operationMetadata16 = operationMetadata14;
    dictionary7["ExportTMDL"] = operationMetadata16;
    Dictionary<string, OperationMetadata> dictionary8 = dictionary1;
    OperationMetadata operationMetadata17 = new OperationMetadata { Description = "Display comprehensive help information for the query group operations tool including supported operations and examples.\r\nOptional: None." };
    List<string> stringList7 = new List<string>();
    stringList7.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Help\"\r\n    }\r\n}");
    operationMetadata17.ExampleRequests = stringList7;
    dictionary8["Help"] = operationMetadata17;
    Dictionary<string, OperationMetadata> dictionary9 = dictionary1;
    toolMetadata2.Operations = dictionary9;
    QueryGroupOperationsTool.toolMetadata = toolMetadata1;
  }
}
