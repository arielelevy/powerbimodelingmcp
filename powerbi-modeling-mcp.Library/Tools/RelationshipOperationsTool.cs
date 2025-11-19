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
public class RelationshipOperationsTool
{
  private readonly ILogger<RelationshipOperationsTool> _logger;
  public static readonly ToolMetadata toolMetadata;

  public RelationshipOperationsTool(ILogger<RelationshipOperationsTool> logger)
  {
    this._logger = logger;
  }

  [McpServerTool(Name = "relationship_operations")]
  [Description("Perform operations on semantic model relationships. Supported operations: Help, List, Get, Create, Update, Delete, Rename, Activate, Deactivate, Find, ExportTMDL. Use the Operation parameter to specify which operation to perform.")]
  public RelationshipOperationResponse ExecuteRelationshipOperation(
    McpServer mcpServer,
    RelationshipOperationRequest request)
  {
    this._logger.LogDebug("Executing {ToolName}.{Operation}: Relationship={RelationshipName}, Connection={ConnectionName}", (object) nameof (RelationshipOperationsTool), (object) request.Operation, (object) request.RelationshipName, (object) (request.ConnectionName ?? "(last used)"));
    try
    {
      string[] strArray1 = new string[11]
      {
        "LIST",
        "GET",
        "CREATE",
        "UPDATE",
        "DELETE",
        "RENAME",
        "ACTIVATE",
        "DEACTIVATE",
        "FIND",
        "EXPORTTMDL",
        "HELP"
      };
      string[] strArray2 = new string[6]
      {
        "CREATE",
        "UPDATE",
        "DELETE",
        "RENAME",
        "ACTIVATE",
        "DEACTIVATE"
      };
      if (!Enumerable.Contains<string>((IEnumerable<string>) strArray1, request.Operation.ToUpperInvariant()))
      {
        this._logger.LogWarning("Invalid operation '{Operation}' requested for {ToolName}. Valid operations: {ValidOperations}", (object) request.Operation, (object) nameof (RelationshipOperationsTool), (object) string.Join(", ", strArray1));
        return RelationshipOperationResponse.Forbidden(request.Operation, $"Invalid operation: {request.Operation}. Supported operations: {string.Join(", ", strArray1)}");
      }
      if (!this.ValidateRequest(request.Operation, request))
        throw new McpException($"Invalid request for {request.Operation} operation.");
      if (Enumerable.Contains<string>((IEnumerable<string>) strArray2, request.Operation.ToUpperInvariant()))
      {
        WriteOperationResult writeOperationResult = WriteGuard.ExecuteWriteOperationWithGuards(mcpServer, request.ConnectionName, request.Operation);
        if (!writeOperationResult.Success)
        {
          this._logger.LogWarning("{ToolName}.{Operation} blocked by write guard: {Reason}", (object) nameof (RelationshipOperationsTool), (object) request.Operation, (object) writeOperationResult.Message);
          return RelationshipOperationResponse.Forbidden(request.Operation, writeOperationResult.Message);
        }
      }
      bool allowed = WriteGuard.IsWriteAllowed("").allowed;
      string upperInvariant = request.Operation.ToUpperInvariant();
      RelationshipOperationResponse operationResponse;
      if (upperInvariant != null)
      {
        switch (upperInvariant.Length)
        {
          case 3:
            if ((upperInvariant == "GET"))
            {
              operationResponse = this.HandleGetOperation(request);
              goto label_36;
            }
            break;
          case 4:
            switch (upperInvariant[0])
            {
              case 'F':
                if ((upperInvariant == "FIND"))
                {
                  operationResponse = this.HandleFindOperation(request);
                  goto label_36;
                }
                break;
              case 'H':
                if ((upperInvariant == "HELP"))
                {
                  operationResponse = this.HandleHelpOperation(request, allowed ? strArray1 : Enumerable.ToArray<string>(Enumerable.Except<string>((IEnumerable<string>) strArray1, (IEnumerable<string>) strArray2)));
                  goto label_36;
                }
                break;
              case 'L':
                if ((upperInvariant == "LIST"))
                {
                  operationResponse = this.HandleListOperation(request);
                  goto label_36;
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
                  goto label_36;
                }
                break;
              case 'D':
                if ((upperInvariant == "DELETE"))
                {
                  operationResponse = this.HandleDeleteOperation(request);
                  goto label_36;
                }
                break;
              case 'R':
                if ((upperInvariant == "RENAME"))
                {
                  operationResponse = this.HandleRenameOperation(request);
                  goto label_36;
                }
                break;
              case 'U':
                if ((upperInvariant == "UPDATE"))
                {
                  operationResponse = this.HandleUpdateOperation(request);
                  goto label_36;
                }
                break;
            }
            break;
          case 8:
            if ((upperInvariant == "ACTIVATE"))
            {
              operationResponse = this.HandleActivateOperation(request);
              goto label_36;
            }
            break;
          case 10:
            switch (upperInvariant[0])
            {
              case 'D':
                if ((upperInvariant == "DEACTIVATE"))
                {
                  operationResponse = this.HandleDeactivateOperation(request);
                  goto label_36;
                }
                break;
              case 'E':
                if ((upperInvariant == "EXPORTTMDL"))
                {
                  operationResponse = this.HandleExportTMDLOperation(request);
                  goto label_36;
                }
                break;
            }
            break;
        }
      }
      operationResponse = RelationshipOperationResponse.Forbidden(request.Operation, $"Operation {request.Operation} not implemented");
label_36:
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Error executing {ToolName}.{Operation}: {ErrorMessage}", (object) nameof (RelationshipOperationsTool), (object) request.Operation, (object) ex.Message);
      return new RelationshipOperationResponse()
      {
        Success = false,
        Message = "Error executing relationship operation: " + ex.Message,
        Operation = request.Operation
      };
    }
  }

  private RelationshipOperationResponse HandleListOperation(RelationshipOperationRequest request)
  {
    try
    {
      List<RelationshipList> relationshipListList = RelationshipOperations.ListRelationships(request.ConnectionName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Count={Count}", (object) nameof (RelationshipOperationsTool), (object) "LIST", (object) relationshipListList.Count);
      RelationshipOperationResponse operationResponse = new RelationshipOperationResponse { Success = true };
      operationResponse.Message = $"Found {relationshipListList.Count} relationships";
      operationResponse.Operation = "LIST";
      operationResponse.Data = (object) relationshipListList;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      RelationshipOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new RelationshipOperationResponse()
      {
        Success = false,
        Message = "Failed to list relationships: " + ex.Message,
        Operation = "LIST",
        Help = (object) operationMetadata
      };
    }
  }

  private RelationshipOperationResponse HandleGetOperation(RelationshipOperationRequest request)
  {
    try
    {
      RelationshipGet relationship = RelationshipOperations.GetRelationship(request.ConnectionName, request.RelationshipName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Relationship={RelationshipName}", (object) nameof (RelationshipOperationsTool), (object) "GET", (object) request.RelationshipName);
      return new RelationshipOperationResponse()
      {
        Success = true,
        Message = $"Retrieved relationship '{request.RelationshipName}'",
        Operation = "GET",
        RelationshipName = request.RelationshipName,
        Data = (object) relationship
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      RelationshipOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new RelationshipOperationResponse()
      {
        Success = false,
        Message = "Failed to get relationship: " + ex.Message,
        Operation = "GET",
        RelationshipName = request.RelationshipName,
        Help = (object) operationMetadata
      };
    }
  }

  private RelationshipOperationResponse HandleCreateOperation(RelationshipOperationRequest request)
  {
    try
    {
      if (string.IsNullOrEmpty(request.RelationshipDefinition.Name))
      {
        if (!string.IsNullOrEmpty(request.RelationshipName))
          request.RelationshipDefinition.Name = request.RelationshipName;
      }
      else if (!string.IsNullOrEmpty(request.RelationshipName) && (request.RelationshipDefinition.Name != request.RelationshipName))
        throw new McpException($"Relationship name mismatch: Request specifies '{request.RelationshipName}' but RelationshipDefinition specifies '{request.RelationshipDefinition.Name}'");
      RelationshipOperationResult relationship = RelationshipOperations.CreateRelationship(request.ConnectionName, request.RelationshipDefinition);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Relationship={RelationshipName}", (object) nameof (RelationshipOperationsTool), (object) "CREATE", (object) relationship.RelationshipName);
      if (relationship.Warnings != null && Enumerable.Any<string>((IEnumerable<string>) relationship.Warnings))
      {
        foreach (string warning in relationship.Warnings)
          this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (RelationshipOperationsTool), (object) "CREATE", (object) warning);
      }
      return new RelationshipOperationResponse()
      {
        Success = true,
        Message = $"Created relationship '{relationship.RelationshipName}'",
        Operation = "CREATE",
        RelationshipName = relationship.RelationshipName,
        Data = (object) relationship,
        Warnings = relationship.Warnings
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      RelationshipOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new RelationshipOperationResponse()
      {
        Success = false,
        Message = "Failed to create relationship: " + ex.Message,
        Operation = "CREATE",
        Help = (object) operationMetadata
      };
    }
  }

  private RelationshipOperationResponse HandleUpdateOperation(RelationshipOperationRequest request)
  {
    try
    {
      if (string.IsNullOrEmpty(request.RelationshipUpdate.Name))
        request.RelationshipUpdate.Name = !string.IsNullOrEmpty(request.RelationshipName) ? request.RelationshipName : throw new McpException("RelationshipName is required for Update operation");
      else if (!string.IsNullOrEmpty(request.RelationshipName) && (request.RelationshipUpdate.Name != request.RelationshipName))
        throw new McpException($"Relationship name mismatch: Request specifies '{request.RelationshipName}' but RelationshipUpdate specifies '{request.RelationshipUpdate.Name}'");
      RelationshipOperationResult relationshipOperationResult = RelationshipOperations.UpdateRelationship(request.ConnectionName, request.RelationshipUpdate);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Relationship={RelationshipName}", (object) nameof (RelationshipOperationsTool), (object) "UPDATE", (object) relationshipOperationResult.RelationshipName);
      return new RelationshipOperationResponse()
      {
        Success = true,
        Message = $"Updated relationship '{relationshipOperationResult.RelationshipName}'",
        Operation = "UPDATE",
        RelationshipName = relationshipOperationResult.RelationshipName,
        Data = (object) relationshipOperationResult
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      RelationshipOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new RelationshipOperationResponse()
      {
        Success = false,
        Message = "Failed to update relationship: " + ex.Message,
        Operation = "UPDATE",
        Help = (object) operationMetadata
      };
    }
  }

  private RelationshipOperationResponse HandleDeleteOperation(RelationshipOperationRequest request)
  {
    try
    {
      RelationshipOperations.DeleteRelationship(request.ConnectionName, request.RelationshipName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Relationship={RelationshipName}", (object) nameof (RelationshipOperationsTool), (object) "DELETE", (object) request.RelationshipName);
      return new RelationshipOperationResponse()
      {
        Success = true,
        Message = $"Deleted relationship '{request.RelationshipName}'",
        Operation = "DELETE",
        RelationshipName = request.RelationshipName
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      RelationshipOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new RelationshipOperationResponse()
      {
        Success = false,
        Message = "Failed to delete relationship: " + ex.Message,
        Operation = "DELETE",
        RelationshipName = request.RelationshipName,
        Help = (object) operationMetadata
      };
    }
  }

  private RelationshipOperationResponse HandleRenameOperation(RelationshipOperationRequest request)
  {
    try
    {
      if (string.IsNullOrEmpty(request.RenameDefinition.CurrentName))
        request.RenameDefinition.CurrentName = !string.IsNullOrEmpty(request.RelationshipName) ? request.RelationshipName : throw new McpException("Either RelationshipName or RenameDefinition.CurrentName is required.");
      RelationshipOperations.RenameRelationship(request.ConnectionName, request.RenameDefinition.CurrentName, request.RenameDefinition.NewName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: From={OldName}, To={NewName}", (object) nameof (RelationshipOperationsTool), (object) "RENAME", (object) request.RenameDefinition.CurrentName, (object) request.RenameDefinition.NewName);
      RelationshipOperationResponse operationResponse = new RelationshipOperationResponse { Success = true };
      operationResponse.Message = $"Renamed relationship '{request.RenameDefinition.CurrentName}' to '{request.RenameDefinition.NewName}'";
      operationResponse.Operation = "RENAME";
      operationResponse.RelationshipName = request.RenameDefinition.NewName;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      RelationshipOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new RelationshipOperationResponse()
      {
        Success = false,
        Message = "Failed to rename relationship: " + ex.Message,
        Operation = "RENAME",
        RelationshipName = request.RelationshipName,
        Help = (object) operationMetadata
      };
    }
  }

  private RelationshipOperationResponse HandleActivateOperation(RelationshipOperationRequest request)
  {
    try
    {
      RelationshipOperationResult relationshipOperationResult = RelationshipOperations.ActivateRelationship(request.ConnectionName, request.RelationshipName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Relationship={RelationshipName}, HasChanges={HasChanges}", (object) nameof (RelationshipOperationsTool), (object) "ACTIVATE", (object) request.RelationshipName, (object) relationshipOperationResult.HasChanges);
      if (relationshipOperationResult.Warnings != null && Enumerable.Any<string>((IEnumerable<string>) relationshipOperationResult.Warnings))
      {
        foreach (string warning in relationshipOperationResult.Warnings)
          this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (RelationshipOperationsTool), (object) "ACTIVATE", (object) warning);
      }
      return new RelationshipOperationResponse()
      {
        Success = true,
        Message = relationshipOperationResult.HasChanges ? $"Activated relationship '{request.RelationshipName}'" : $"Relationship '{request.RelationshipName}' was already active",
        Operation = "ACTIVATE",
        RelationshipName = request.RelationshipName,
        Data = (object) relationshipOperationResult,
        Warnings = relationshipOperationResult.Warnings
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      RelationshipOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new RelationshipOperationResponse()
      {
        Success = false,
        Message = "Failed to activate relationship: " + ex.Message,
        Operation = "ACTIVATE",
        RelationshipName = request.RelationshipName,
        Help = (object) operationMetadata
      };
    }
  }

  private RelationshipOperationResponse HandleDeactivateOperation(
    RelationshipOperationRequest request)
  {
    try
    {
      RelationshipOperationResult relationshipOperationResult = RelationshipOperations.DeactivateRelationship(request.ConnectionName, request.RelationshipName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Relationship={RelationshipName}, HasChanges={HasChanges}", (object) nameof (RelationshipOperationsTool), (object) "DEACTIVATE", (object) request.RelationshipName, (object) relationshipOperationResult.HasChanges);
      if (relationshipOperationResult.Warnings != null && Enumerable.Any<string>((IEnumerable<string>) relationshipOperationResult.Warnings))
      {
        foreach (string warning in relationshipOperationResult.Warnings)
          this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (RelationshipOperationsTool), (object) "DEACTIVATE", (object) warning);
      }
      return new RelationshipOperationResponse()
      {
        Success = true,
        Message = relationshipOperationResult.HasChanges ? $"Deactivated relationship '{request.RelationshipName}'" : $"Relationship '{request.RelationshipName}' was already inactive",
        Operation = "DEACTIVATE",
        RelationshipName = request.RelationshipName,
        Data = (object) relationshipOperationResult,
        Warnings = relationshipOperationResult.Warnings
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      RelationshipOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new RelationshipOperationResponse()
      {
        Success = false,
        Message = "Failed to deactivate relationship: " + ex.Message,
        Operation = "DEACTIVATE",
        RelationshipName = request.RelationshipName,
        Help = (object) operationMetadata
      };
    }
  }

  private RelationshipOperationResponse HandleFindOperation(RelationshipOperationRequest request)
  {
    try
    {
      List<string> relationshipsForTable = RelationshipOperations.FindRelationshipsForTable(request.ConnectionName, request.TableName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Table={TableName}, Count={Count}", (object) nameof (RelationshipOperationsTool), (object) "FIND", (object) request.TableName, (object) relationshipsForTable.Count);
      RelationshipOperationResponse operation = new RelationshipOperationResponse { Success = true };
      operation.Message = $"Found {relationshipsForTable.Count} relationships for table '{request.TableName}'";
      operation.Operation = "FIND";
      operation.Data = (object) relationshipsForTable;
      return operation;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      RelationshipOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new RelationshipOperationResponse()
      {
        Success = false,
        Message = "Failed to find relationships for table: " + ex.Message,
        Operation = "FIND",
        Help = (object) operationMetadata
      };
    }
  }

  private RelationshipOperationResponse HandleExportTMDLOperation(
    RelationshipOperationRequest request)
  {
    try
    {
      string str = RelationshipOperations.ExportTMDL(request.ConnectionName, request.RelationshipName, (ExportTmdl) request.TmdlExportOptions);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Relationship={RelationshipName}", (object) nameof (RelationshipOperationsTool), (object) "ExportTMDL", (object) request.RelationshipName);
      return new RelationshipOperationResponse()
      {
        Success = true,
        Message = "Relationship TMDL exported successfully",
        Operation = "ExportTMDL",
        RelationshipName = request.RelationshipName,
        Data = (object) str
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      RelationshipOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new RelationshipOperationResponse()
      {
        Success = false,
        Message = "Failed to export relationship TMDL: " + ex.Message,
        Operation = "ExportTMDL",
        Help = (object) operationMetadata
      };
    }
  }

  private RelationshipOperationResponse HandleHelpOperation(
    RelationshipOperationRequest request,
    string[] operations)
  {
    this._logger.LogInformation("{ToolName}.{Operation} completed: Operations={OperationCount}", (object) nameof (RelationshipOperationsTool), (object) request.Operation, (object) operations.Length);
    return new RelationshipOperationResponse()
    {
      Success = true,
      Message = "Help information for relationship operations",
      Operation = request.Operation,
      Help = (object) new
      {
        ToolName = "relationship_operations",
        Description = "Perform operations on semantic model relationships.",
        SupportedOperations = operations,
        Examples = Enumerable.Where<KeyValuePair<string, OperationMetadata>>((IEnumerable<KeyValuePair<string, OperationMetadata>>) RelationshipOperationsTool.toolMetadata.Operations, (Func<KeyValuePair<string, OperationMetadata>, bool>) (p => Enumerable.Contains<string>((IEnumerable<string>) operations, p.Key, (IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase))),
        Notes = new string[3]
        {
          "Relationship names are case-insensitive.",
          "Relationship names must be unique within the model.",
          "Relationship names must not contain spaces or special characters."
        }
      }
    };
  }

  private bool ValidateRequest(string operation, RelationshipOperationRequest request)
  {
    OperationMetadata operationMetadata;
    if (!RelationshipOperationsTool.toolMetadata.Operations.TryGetValue(operation, out operationMetadata))
      return true;
    JsonObject requestDict = JsonSerializer.SerializeToNode<RelationshipOperationRequest>(request) as JsonObject;
    List<string> list1 = Enumerable.ToList<string>(Enumerable.Where<string>((IEnumerable<string>) operationMetadata.RequiredParams, (p => requestDict != null && requestDict[p] == null)));
    List<string> list2 = Enumerable.ToList<string>(Enumerable.Where<string>((IEnumerable<string>) operationMetadata.ForbiddenParams, (p => requestDict != null && requestDict[p] != null)));
    if (Enumerable.Any<string>((IEnumerable<string>) list1))
      throw new McpException($"Missing required parameters needed for {operation} operation: {string.Join(", ", (IEnumerable<string>) list1)}");
    if (Enumerable.Any<string>((IEnumerable<string>) list2))
      throw new McpException($"Forbidden parameters not allowed for {operation} operation: {string.Join(", ", (IEnumerable<string>) list2)}");
    return true;
  }

  static RelationshipOperationsTool()
  {
    ToolMetadata toolMetadata1 = new ToolMetadata();
    ToolMetadata toolMetadata2 = toolMetadata1;
    Dictionary<string, OperationMetadata> dictionary1 = new Dictionary<string, OperationMetadata>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    Dictionary<string, OperationMetadata> dictionary2 = dictionary1;
    OperationMetadata operationMetadata1 = new OperationMetadata { Description = "List all relationships in the model.\r\nReturns detailed information about each relationship including tables, columns, cardinality, and filtering behavior.\r\nMandatory properties: None.\r\nOptional: None." };
    List<string> stringList1 = new List<string>();
    stringList1.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"List\"\r\n    }\r\n}");
    operationMetadata1.ExampleRequests = stringList1;
    dictionary2["List"] = operationMetadata1;
    Dictionary<string, OperationMetadata> dictionary3 = dictionary1;
    OperationMetadata operationMetadata2 = new OperationMetadata { RequiredParams = new string[1]
    {
      "RelationshipName"
    } };
    operationMetadata2.Description = "Get detailed information about a specific relationship.\r\nReturns comprehensive relationship properties including tables, columns, cardinality, filtering behavior, and current state.\r\nMandatory properties: RelationshipName.\r\nOptional: None.";
    OperationMetadata operationMetadata3 = operationMetadata2;
    List<string> stringList2 = new List<string>();
    stringList2.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Get\",\r\n        \"RelationshipName\": \"SalesToRegion\"\r\n    }\r\n}");
    operationMetadata3.ExampleRequests = stringList2;
    OperationMetadata operationMetadata4 = operationMetadata2;
    dictionary3["Get"] = operationMetadata4;
    Dictionary<string, OperationMetadata> dictionary4 = dictionary1;
    OperationMetadata operationMetadata5 = new OperationMetadata { RequiredParams = new string[1]
    {
      "RelationshipDefinition"
    } };
    operationMetadata5.Description = "Create a new relationship between two tables.\r\nIn PowerBI relationships, the 'from' side is the many side (fact table in BI terms, child table in database terms), \r\nand the 'to' side is the one side (dimension table in BI terms, parent/lookup table in database terms). \r\nThis follows standard foreign key relationship patterns where multiple records in the child table reference a single record in the parent table. \r\nIf no relationship name is provided, one will be auto-generated using the pattern FromTable_FromColumn_ToTable_ToColumn.\r\nMandatory properties: RelationshipDefinition (with FromTable, FromColumn, ToTable, ToColumn).\r\nOptional: Name, IsActive, Type, CrossFilteringBehavior, FromCardinality, ToCardinality, SecurityFilteringBehavior, JoinOnDateBehavior, RelyOnReferentialIntegrity, Annotations, ExtendedProperties.";
    operationMetadata5.CommonMistakes = new string[1]
    {
      "Setting the dimension table as 'from' side and fact table as 'to' side - should be the other way around"
    };
    operationMetadata5.Tips = new string[8]
    {
      "PowerBI relationships are predominantly many-to-one",
      "Occasionally one-to-one relationships are used, in which case CrossFilteringBehavior must be BothDirections",
      "Many-to-many relationships are rarely used and should be created with caution",
      "CrossFilteringBehavior values: OneDirection, BothDirections, Automatic",
      "FromCardinality/ToCardinality values: One, Many",
      "SecurityFilteringBehavior values: OneDirection, BothDirections",
      "JoinOnDateBehavior values: DateAndTime, DatePartOnly",
      "Type values: SingleColumn"
    };
    OperationMetadata operationMetadata6 = operationMetadata5;
    List<string> stringList3 = new List<string>();
    stringList3.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Create\",\r\n        \"RelationshipDefinition\": { \r\n            \"Name\": \"SalesToRegion\", \r\n            \"FromTable\": \"Sales\",\r\n            \"FromColumn\": \"RegionID\",\r\n            \"FromCardinality\": \"Many\",\r\n            \"ToTable\": \"Region\",\r\n            \"ToColumn\": \"RegionID\",\r\n            \"ToCardinality\": \"One\"\r\n        }\r\n    }\r\n}");
    stringList3.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Create\",\r\n        \"RelationshipDefinition\": { \r\n            \"FromTable\": \"Sales\",\r\n            \"FromColumn\": \"ProductID\",\r\n            \"FromCardinality\": \"Many\",\r\n            \"ToTable\": \"Product\",\r\n            \"ToColumn\": \"ID\",\r\n            \"ToCardinality\": \"One\"\r\n        }\r\n    }\r\n}");
    operationMetadata6.ExampleRequests = stringList3;
    OperationMetadata operationMetadata7 = operationMetadata5;
    dictionary4["Create"] = operationMetadata7;
    Dictionary<string, OperationMetadata> dictionary5 = dictionary1;
    OperationMetadata operationMetadata8 = new OperationMetadata { RequiredParams = new string[1]
    {
      "RelationshipUpdate"
    } };
    operationMetadata8.Description = "Update an existing relationship's properties.\r\nNames cannot be changed and must use the Rename operation instead.\r\nMandatory properties: RelationshipUpdate (with Name).\r\nOptional: IsActive, Type, CrossFilteringBehavior, FromCardinality, ToCardinality, SecurityFilteringBehavior, JoinOnDateBehavior, RelyOnReferentialIntegrity, Annotations, ExtendedProperties.";
    operationMetadata8.Tips = new string[5]
    {
      "CrossFilteringBehavior values: OneDirection, BothDirections, Automatic",
      "FromCardinality/ToCardinality values: One, Many",
      "SecurityFilteringBehavior values: OneDirection, BothDirections",
      "JoinOnDateBehavior values: DateAndTime, DatePartOnly",
      "Type values: SingleColumn (currently only supported value)"
    };
    OperationMetadata operationMetadata9 = operationMetadata8;
    List<string> stringList4 = new List<string>();
    stringList4.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Update\",\r\n        \"RelationshipUpdate\": { \r\n            \"Name\": \"SalesToRegion\", \r\n            \"FromTable\": \"Sales\",\r\n            \"FromColumn\": \"RegionID\",\r\n            \"ToTable\": \"Region\",\r\n            \"ToColumn\": \"RegionID\",\r\n            \"IsActive\": false\r\n        }\r\n    }\r\n}");
    operationMetadata9.ExampleRequests = stringList4;
    OperationMetadata operationMetadata10 = operationMetadata8;
    dictionary5["Update"] = operationMetadata10;
    Dictionary<string, OperationMetadata> dictionary6 = dictionary1;
    OperationMetadata operationMetadata11 = new OperationMetadata { RequiredParams = new string[1]
    {
      "RelationshipName"
    } };
    operationMetadata11.Description = "Delete a relationship from the model.\r\nThis operation permanently removes the relationship and cannot be undone.\r\nMandatory properties: RelationshipName.\r\nOptional: None.";
    OperationMetadata operationMetadata12 = operationMetadata11;
    List<string> stringList5 = new List<string>();
    stringList5.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Delete\",\r\n        \"RelationshipName\": \"ObsoleteRelationship\"\r\n    }\r\n}");
    operationMetadata12.ExampleRequests = stringList5;
    OperationMetadata operationMetadata13 = operationMetadata11;
    dictionary6["Delete"] = operationMetadata13;
    Dictionary<string, OperationMetadata> dictionary7 = dictionary1;
    OperationMetadata operationMetadata14 = new OperationMetadata { RequiredParams = new string[1]
    {
      "RenameDefinition"
    } };
    operationMetadata14.Description = "Rename a relationship in the model.\r\nEither specify CurrentName in RenameDefinition or use RelationshipName parameter to identify the relationship to rename.\r\nMandatory properties: RenameDefinition (with NewName), CurrentName (either in RenameDefinition or as RelationshipName).\r\nOptional: None.";
    OperationMetadata operationMetadata15 = operationMetadata14;
    List<string> stringList6 = new List<string>();
    stringList6.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Rename\",\r\n        \"RenameDefinition\": { \r\n            \"CurrentName\": \"OldRelationship\", \r\n            \"NewName\": \"NewRelationship\"\r\n        }\r\n    }\r\n}");
    operationMetadata15.ExampleRequests = stringList6;
    OperationMetadata operationMetadata16 = operationMetadata14;
    dictionary7["Rename"] = operationMetadata16;
    Dictionary<string, OperationMetadata> dictionary8 = dictionary1;
    OperationMetadata operationMetadata17 = new OperationMetadata { RequiredParams = new string[1]
    {
      "RelationshipName"
    } };
    operationMetadata17.Description = "Activate a relationship in the model.\r\nSets the relationship's IsActive property to true, enabling it for data filtering and calculations.\r\nMandatory properties: RelationshipName.\r\nOptional: None.";
    OperationMetadata operationMetadata18 = operationMetadata17;
    List<string> stringList7 = new List<string>();
    stringList7.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Activate\",\r\n        \"RelationshipName\": \"SalesToRegion\"\r\n    }\r\n}");
    operationMetadata18.ExampleRequests = stringList7;
    OperationMetadata operationMetadata19 = operationMetadata17;
    dictionary8["Activate"] = operationMetadata19;
    Dictionary<string, OperationMetadata> dictionary9 = dictionary1;
    OperationMetadata operationMetadata20 = new OperationMetadata { RequiredParams = new string[1]
    {
      "RelationshipName"
    } };
    operationMetadata20.Description = "Deactivate a relationship in the model.\r\nSets the relationship's IsActive property to false, disabling it from data filtering and calculations.\r\nMandatory properties: RelationshipName.\r\nOptional: None.";
    OperationMetadata operationMetadata21 = operationMetadata20;
    List<string> stringList8 = new List<string>();
    stringList8.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Deactivate\",\r\n        \"RelationshipName\": \"SalesToRegion\"\r\n    }\r\n}");
    operationMetadata21.ExampleRequests = stringList8;
    OperationMetadata operationMetadata22 = operationMetadata20;
    dictionary9["Deactivate"] = operationMetadata22;
    Dictionary<string, OperationMetadata> dictionary10 = dictionary1;
    OperationMetadata operationMetadata23 = new OperationMetadata { RequiredParams = new string[1]
    {
      "TableName"
    } };
    operationMetadata23.Description = "Find all relationships connected to a specific table.\r\nReturns list of relationships where the table appears as either the 'from' or 'to' side.\r\nMandatory properties: TableName.\r\nOptional: None.";
    OperationMetadata operationMetadata24 = operationMetadata23;
    List<string> stringList9 = new List<string>();
    stringList9.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Find\",\r\n        \"TableName\": \"Sales\"\r\n    }\r\n}");
    operationMetadata24.ExampleRequests = stringList9;
    OperationMetadata operationMetadata25 = operationMetadata23;
    dictionary10["Find"] = operationMetadata25;
    Dictionary<string, OperationMetadata> dictionary11 = dictionary1;
    OperationMetadata operationMetadata26 = new OperationMetadata { RequiredParams = new string[1]
    {
      "RelationshipName"
    } };
    operationMetadata26.Description = "Export a relationship to TMDL format.\r\nMandatory properties: RelationshipName.\r\nOptional: TmdlExportOptions (for controlling export formatting and scope).";
    OperationMetadata operationMetadata27 = operationMetadata26;
    List<string> stringList10 = new List<string>();
    stringList10.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"ExportTMDL\",\r\n        \"RelationshipName\": \"SalesToRegion\"\r\n    }\r\n}");
    operationMetadata27.ExampleRequests = stringList10;
    OperationMetadata operationMetadata28 = operationMetadata26;
    dictionary11["ExportTMDL"] = operationMetadata28;
    Dictionary<string, OperationMetadata> dictionary12 = dictionary1;
    OperationMetadata operationMetadata29 = new OperationMetadata { Description = "Describe the relationship operations tool and its available operations.\r\nReturns comprehensive help information including supported operations, examples, and usage notes.\r\nMandatory properties: None.\r\nOptional: None." };
    List<string> stringList11 = new List<string>();
    stringList11.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Help\"\r\n    }\r\n}");
    operationMetadata29.ExampleRequests = stringList11;
    dictionary12["Help"] = operationMetadata29;
    Dictionary<string, OperationMetadata> dictionary13 = dictionary1;
    toolMetadata2.Operations = dictionary13;
    RelationshipOperationsTool.toolMetadata = toolMetadata1;
  }
}
