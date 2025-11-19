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
public class MeasureOperationsTool
{
  private readonly ILogger<MeasureOperationsTool> _logger;
  public static readonly ToolMetadata toolMetadata;

  public MeasureOperationsTool(ILogger<MeasureOperationsTool> logger) => this._logger = logger;

  [McpServerTool(Name = "measure_operations")]
  [Description("Perform operations on semantic model measures. Supported operations: Help, Create, Update, Delete, Get, List, Rename, Move, ExportTMDL. Use the Operation parameter to specify which operation to perform. MeasureName is required for all operations except List. To move a measure between tables, use operation: 'Move'. Update operation cannot change table assignment. ExportTMDL exports a measure to TMDL format.")]
  public MeasureOperationResponse ExecuteMeasureOperation(
    McpServer mcpServer,
    MeasureOperationRequest request)
  {
    this._logger.LogDebug("Executing {ToolName}.{Operation}: MeasureName={MeasureName}, Table={TableName}, Connection={ConnectionName}", (object) nameof (MeasureOperationsTool), (object) request.Operation, (object) (request.MeasureName ?? "(none)"), (object) (request.TableName ?? "(none)"), (object) (request.ConnectionName ?? "(last used)"));
    try
    {
      string[] strArray1 = new string[9]
      {
        "CREATE",
        "UPDATE",
        "DELETE",
        "GET",
        "LIST",
        "RENAME",
        "MOVE",
        "EXPORTTMDL",
        "HELP"
      };
      string[] strArray2 = new string[5]
      {
        "CREATE",
        "UPDATE",
        "DELETE",
        "RENAME",
        "MOVE"
      };
      string upperInvariant = request.Operation.ToUpperInvariant();
      if (!Enumerable.Contains<string>((IEnumerable<string>) strArray1, upperInvariant))
      {
        this._logger.LogWarning("Invalid operation '{Operation}' requested for {ToolName}. Valid operations: {ValidOperations}", (object) request.Operation, (object) nameof (MeasureOperationsTool), (object) string.Join(", ", strArray1));
        return MeasureOperationResponse.Forbidden(request.Operation, $"Invalid operation: {request.Operation}. Supported operations: {string.Join(", ", strArray1)}", request.MeasureName, request.TableName);
      }
      if (!this.ValidateRequest(request.Operation, request))
        throw new McpException($"Invalid request for {request.Operation} operation.");
      if (Enumerable.Contains<string>((IEnumerable<string>) strArray2, upperInvariant))
      {
        WriteOperationResult writeOperationResult = WriteGuard.ExecuteWriteOperationWithGuards(mcpServer, request.ConnectionName, request.Operation);
        if (!writeOperationResult.Success)
        {
          this._logger.LogWarning("{ToolName}.{Operation} blocked by write guard: {Reason}", (object) nameof (MeasureOperationsTool), (object) request.Operation, (object) writeOperationResult.Message);
          return new MeasureOperationResponse()
          {
            Success = false,
            Warnings = writeOperationResult.Warnings,
            Message = writeOperationResult.Message,
            Operation = request.Operation,
            MeasureName = request.MeasureName
          };
        }
      }
      bool allowed = WriteGuard.IsWriteAllowed("").allowed;
      MeasureOperationResponse operationResponse;
      if (upperInvariant != null)
      {
        switch (upperInvariant.Length)
        {
          case 3:
            if ((upperInvariant == "GET"))
            {
              operationResponse = this.HandleGetOperation(request);
              goto label_31;
            }
            break;
          case 4:
            switch (upperInvariant[0])
            {
              case 'H':
                if ((upperInvariant == "HELP"))
                {
                  operationResponse = this.HandleHelpOperation(request, allowed ? strArray1 : Enumerable.ToArray<string>(Enumerable.Except<string>((IEnumerable<string>) strArray1, (IEnumerable<string>) strArray2)));
                  goto label_31;
                }
                break;
              case 'L':
                if ((upperInvariant == "LIST"))
                {
                  operationResponse = this.HandleListOperation(request);
                  goto label_31;
                }
                break;
              case 'M':
                if ((upperInvariant == "MOVE"))
                {
                  operationResponse = this.HandleMoveOperation(request);
                  goto label_31;
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
                  goto label_31;
                }
                break;
              case 'D':
                if ((upperInvariant == "DELETE"))
                {
                  operationResponse = this.HandleDeleteOperation(request);
                  goto label_31;
                }
                break;
              case 'R':
                if ((upperInvariant == "RENAME"))
                {
                  operationResponse = this.HandleRenameOperation(request);
                  goto label_31;
                }
                break;
              case 'U':
                if ((upperInvariant == "UPDATE"))
                {
                  operationResponse = this.HandleUpdateOperation(request);
                  goto label_31;
                }
                break;
            }
            break;
          case 10:
            if ((upperInvariant == "EXPORTTMDL"))
            {
              operationResponse = this.HandleExportTMDLOperation(request);
              goto label_31;
            }
            break;
        }
      }
      operationResponse = MeasureOperationResponse.Forbidden(request.Operation, $"Operation {request.Operation} is not implemented", request.MeasureName, request.TableName);
label_31:
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Error executing {ToolName}.{Operation}: {ErrorMessage}", (object) nameof (MeasureOperationsTool), (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      MeasureOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new MeasureOperationResponse()
      {
        Success = false,
        Message = "Error executing measure operation: " + ex.Message,
        Operation = request.Operation,
        MeasureName = request.MeasureName,
        Help = (object) operationMetadata
      };
    }
  }

  private MeasureOperationResponse HandleCreateOperation(MeasureOperationRequest request)
  {
    try
    {
      if (!string.IsNullOrEmpty(request.TableName))
      {
        if (string.IsNullOrEmpty(request.CreateDefinition.TableName))
          request.CreateDefinition.TableName = request.TableName;
        else if ((request.CreateDefinition.TableName != request.TableName))
          throw new McpException($"Table name mismatch: Request specifies '{request.TableName}' but CreateDefinition specifies '{request.CreateDefinition.TableName}'");
      }
      if (string.IsNullOrEmpty(request.CreateDefinition.Name))
        request.CreateDefinition.Name = request.MeasureName;
      else if (!string.IsNullOrEmpty(request.MeasureName) && (request.CreateDefinition.Name != request.MeasureName))
        throw new McpException($"Measure name mismatch: Request specifies '{request.MeasureName}' but CreateDefinition specifies '{request.CreateDefinition.Name}'");
      MeasureOperations.MeasureOperationResult measure = MeasureOperations.CreateMeasure(request.ConnectionName, request.CreateDefinition);
      MeasureOperationResponse operationResponse = new MeasureOperationResponse { Success = (measure.State == "Ready") };
      operationResponse.Message = (measure.State == "Ready") ? $"Measure '{request.CreateDefinition.Name}' created successfully" : measure.ErrorMessage ?? $"Failed to create measure '{request.CreateDefinition.Name}'";
      operationResponse.Operation = request.Operation;
      operationResponse.MeasureName = request.CreateDefinition.Name;
      operationResponse.TableName = request.CreateDefinition.TableName;
      operationResponse.Data = (object) measure;
      List<string> stringList1;
      if (!(measure.State != "Ready") || string.IsNullOrEmpty(measure.ErrorMessage))
      {
        stringList1 = new List<string>();
      }
      else
      {
        List<string> stringList2 = new List<string>();
        stringList2.Add(measure.ErrorMessage);
        stringList1 = stringList2;
      }
      operationResponse.Warnings = stringList1;
      MeasureOperationResponse operation = operationResponse;
      if (operation.Warnings != null && Enumerable.Any<string>((IEnumerable<string>) operation.Warnings))
      {
        foreach (string warning in operation.Warnings)
          this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (MeasureOperationsTool), (object) request.Operation, (object) warning);
      }
      if (operation.Success)
        this._logger.LogInformation("{ToolName}.{Operation} completed: Table={TableName}, Measure={MeasureName}", (object) nameof (MeasureOperationsTool), (object) request.Operation, (object) request.CreateDefinition.TableName, (object) request.CreateDefinition.Name);
      else
        this._logger.LogWarning("{ToolName}.{Operation} completed with errors: Table={TableName}, Measure={MeasureName}, Message={Message}", (object) nameof (MeasureOperationsTool), (object) request.Operation, (object) request.CreateDefinition.TableName, (object) request.CreateDefinition.Name, (object) operation.Message);
      return operation;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      MeasureOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new MeasureOperationResponse()
      {
        Success = false,
        Message = "Error occurred while handling Create operation: " + ex.Message,
        Operation = request.Operation,
        MeasureName = request.MeasureName,
        TableName = request.TableName,
        Help = (object) operationMetadata
      };
    }
  }

  private MeasureOperationResponse HandleUpdateOperation(MeasureOperationRequest request)
  {
    try
    {
      if (string.IsNullOrEmpty(request.UpdateDefinition.Name))
        request.UpdateDefinition.Name = !string.IsNullOrEmpty(request.TableName) ? request.TableName : throw new McpException("TableName is required for Update operation (either in request or UpdateDefinition)");
      else if (!string.IsNullOrEmpty(request.TableName) && (request.UpdateDefinition.Name != request.TableName))
        throw new McpException($"Table name mismatch: Request specifies '{request.TableName}' but UpdateDefinition specifies '{request.UpdateDefinition.TableName}'");
      if (string.IsNullOrEmpty(request.UpdateDefinition.Name))
        request.UpdateDefinition.Name = request.MeasureName;
      else if (!string.IsNullOrEmpty(request.MeasureName) && (request.UpdateDefinition.Name != request.MeasureName))
        throw new McpException($"Measure name mismatch: Request specifies '{request.MeasureName}' but UpdateDefinition specifies '{request.UpdateDefinition.Name}'");
      MeasureOperations.MeasureOperationResult measureOperationResult = MeasureOperations.UpdateMeasure(request.ConnectionName, request.UpdateDefinition);
      MeasureOperationResponse operationResponse1 = new MeasureOperationResponse { Success = (measureOperationResult.State == "Ready") };
      operationResponse1.Message = (measureOperationResult.State == "Ready") ? $"Measure '{request.UpdateDefinition.Name}' updated successfully" : measureOperationResult.ErrorMessage ?? $"Failed to update measure '{request.UpdateDefinition.Name}'";
      operationResponse1.Operation = request.Operation;
      operationResponse1.MeasureName = request.UpdateDefinition.Name;
      operationResponse1.TableName = request.UpdateDefinition.TableName;
      operationResponse1.Data = (object) measureOperationResult;
      List<string> stringList1;
      if (!(measureOperationResult.State != "Ready") || string.IsNullOrEmpty(measureOperationResult.ErrorMessage))
      {
        stringList1 = new List<string>();
      }
      else
      {
        List<string> stringList2 = new List<string>();
        stringList2.Add(measureOperationResult.ErrorMessage);
        stringList1 = stringList2;
      }
      operationResponse1.Warnings = stringList1;
      MeasureOperationResponse operationResponse2 = operationResponse1;
      if (operationResponse2.Warnings != null && Enumerable.Any<string>((IEnumerable<string>) operationResponse2.Warnings))
      {
        foreach (string warning in operationResponse2.Warnings)
          this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (MeasureOperationsTool), (object) request.Operation, (object) warning);
      }
      if (operationResponse2.Success)
        this._logger.LogInformation("{ToolName}.{Operation} completed: Table={TableName}, Measure={MeasureName}", (object) nameof (MeasureOperationsTool), (object) request.Operation, (object) request.UpdateDefinition.TableName, (object) request.UpdateDefinition.Name);
      else
        this._logger.LogWarning("{ToolName}.{Operation} completed with errors: Table={TableName}, Measure={MeasureName}, Message={Message}", (object) nameof (MeasureOperationsTool), (object) request.Operation, (object) request.UpdateDefinition.TableName, (object) request.UpdateDefinition.Name, (object) operationResponse2.Message);
      return operationResponse2;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      MeasureOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new MeasureOperationResponse()
      {
        Success = false,
        Message = "Error executing Update operation: " + ex.Message,
        Operation = request.Operation,
        MeasureName = request.MeasureName,
        TableName = request.TableName,
        Help = (object) operationMetadata
      };
    }
  }

  private MeasureOperationResponse HandleDeleteOperation(MeasureOperationRequest request)
  {
    try
    {
      MeasureOperations.DeleteMeasure(request.ConnectionName, request.MeasureName, request.ShouldCascadeDelete.Value);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Measure={MeasureName}, Cascade={Cascade}", (object) nameof (MeasureOperationsTool), (object) request.Operation, (object) request.MeasureName, (object) request.ShouldCascadeDelete);
      return new MeasureOperationResponse()
      {
        Success = true,
        Message = $"Measure '{request.MeasureName}' deleted successfully",
        Operation = request.Operation,
        MeasureName = request.MeasureName
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      MeasureOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new MeasureOperationResponse()
      {
        Success = false,
        Message = "Error executing Delete operation: " + ex.Message,
        Operation = request.Operation,
        MeasureName = request.MeasureName,
        Help = (object) operationMetadata
      };
    }
  }

  private MeasureOperationResponse HandleGetOperation(MeasureOperationRequest request)
  {
    try
    {
      MeasureGet measure = MeasureOperations.GetMeasure(request.ConnectionName, request.MeasureName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Measure={MeasureName}, Table={TableName}", (object) nameof (MeasureOperationsTool), (object) request.Operation, (object) request.MeasureName, (object) measure.TableName);
      return new MeasureOperationResponse()
      {
        Success = true,
        Message = $"Measure '{request.MeasureName}' retrieved successfully",
        Operation = request.Operation,
        MeasureName = request.MeasureName,
        TableName = measure.TableName,
        Data = (object) measure
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      MeasureOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new MeasureOperationResponse()
      {
        Success = false,
        Message = "Error executing Get operation: " + ex.Message,
        Operation = request.Operation,
        MeasureName = request.MeasureName,
        Help = (object) operationMetadata
      };
    }
  }

  private MeasureOperationResponse HandleListOperation(MeasureOperationRequest request)
  {
    try
    {
      int totalCount;
      List<MeasureList> measureListList = MeasureOperations.ListMeasures(request.ConnectionName, request.TableName, request.MaxResults, out totalCount);
      bool flag = request.MaxResults.HasValue && totalCount > request.MaxResults.Value;
      List<string> stringList = new List<string>();
      string str;
      if (request.TableName != null)
      {
        str = $"Found {measureListList.Count} measures in table '{request.TableName}'";
        if (flag)
          stringList.Add($"Results truncated: Showing {measureListList.Count} of {totalCount} measures (limited by MaxResults={request.MaxResults})");
      }
      else
      {
        str = $"Found {measureListList.Count} measures across all tables";
        if (flag)
          stringList.Add($"Results truncated: Showing {measureListList.Count} of {totalCount} measures (limited by MaxResults={request.MaxResults})");
      }
      this._logger.LogInformation("{ToolName}.{Operation} completed: Table={TableName}, TotalCount={TotalCount}, ReturnedCount={Count}, IsTruncated={IsTruncated}", (object) nameof (MeasureOperationsTool), (object) request.Operation, (object) (request.TableName ?? "(all)"), (object) totalCount, (object) measureListList.Count, (object) flag);
      return new MeasureOperationResponse()
      {
        Success = true,
        Message = str,
        Operation = request.Operation,
        TableName = request.TableName,
        Data = (object) measureListList,
        Warnings = Enumerable.Any<string>((IEnumerable<string>) stringList) ? stringList : (List<string>) null
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      MeasureOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new MeasureOperationResponse()
      {
        Success = false,
        Message = "Error executing List operation: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private MeasureOperationResponse HandleRenameOperation(MeasureOperationRequest request)
  {
    try
    {
      if (string.IsNullOrEmpty(request.RenameDefinition.CurrentName))
        request.RenameDefinition.CurrentName = !string.IsNullOrEmpty(request.MeasureName) ? request.MeasureName : throw new McpException("Either MeasureName or RenameDefinition.CurrentName is required.");
      MeasureOperations.RenameMeasure(request.ConnectionName, request.RenameDefinition.CurrentName, request.RenameDefinition.NewName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: From={OldName}, To={NewName}", (object) nameof (MeasureOperationsTool), (object) request.Operation, (object) request.RenameDefinition.CurrentName, (object) request.RenameDefinition.NewName);
      MeasureOperationResponse operationResponse = new MeasureOperationResponse { Success = true };
      operationResponse.Message = $"Measure '{request.RenameDefinition.CurrentName}' renamed to '{request.RenameDefinition.NewName}' successfully";
      operationResponse.Operation = request.Operation;
      operationResponse.MeasureName = request.RenameDefinition.NewName;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      MeasureOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new MeasureOperationResponse()
      {
        Success = false,
        Message = "Error executing Rename operation: " + ex.Message,
        Operation = request.Operation,
        MeasureName = request.MeasureName,
        Help = (object) operationMetadata
      };
    }
  }

  private MeasureOperationResponse HandleMoveOperation(MeasureOperationRequest request)
  {
    try
    {
      if (string.IsNullOrEmpty(request.MoveDefinition.Name))
        request.MoveDefinition.Name = !string.IsNullOrEmpty(request.MeasureName) ? request.MeasureName : throw new McpException("Either MeasureName or MoveDefinition.Name is required.");
      MeasureOperations.MoveMeasure(request.ConnectionName, request.MoveDefinition.DestinationTableName, request.MoveDefinition.Name);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Measure={MeasureName}, ToTable={DestinationTable}", (object) nameof (MeasureOperationsTool), (object) request.Operation, (object) request.MoveDefinition.Name, (object) request.MoveDefinition.DestinationTableName);
      MeasureOperationResponse operationResponse = new MeasureOperationResponse { Success = true };
      operationResponse.Message = $"Measure '{request.MoveDefinition.Name}' moved to table '{request.MoveDefinition.DestinationTableName}' successfully.";
      operationResponse.Operation = request.Operation;
      operationResponse.MeasureName = request.MoveDefinition.Name;
      operationResponse.TableName = request.MoveDefinition.DestinationTableName;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      MeasureOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new MeasureOperationResponse()
      {
        Success = false,
        Message = "Error executing Move operation: " + ex.Message,
        Operation = request.Operation,
        MeasureName = request.MeasureName,
        TableName = request.TableName,
        Help = (object) operationMetadata
      };
    }
  }

  private MeasureOperationResponse HandleExportTMDLOperation(MeasureOperationRequest request)
  {
    try
    {
      string str = MeasureOperations.ExportTMDL(request.ConnectionName, request.MeasureName, (ExportTmdl) request.TmdlExportOptions);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Measure={MeasureName}", (object) nameof (MeasureOperationsTool), (object) request.Operation, (object) request.MeasureName);
      return new MeasureOperationResponse()
      {
        Success = true,
        Message = $"TMDL exported for measure '{request.MeasureName}'",
        Operation = request.Operation,
        MeasureName = request.MeasureName,
        Data = (object) str
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      MeasureOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new MeasureOperationResponse()
      {
        Success = false,
        Message = $"Failed to export TMDL for measure '{request.MeasureName}': {ex.Message}",
        Operation = request.Operation,
        MeasureName = request.MeasureName,
        Help = (object) operationMetadata
      };
    }
  }

  private MeasureOperationResponse HandleHelpOperation(
    MeasureOperationRequest request,
    string[] operations)
  {
    this._logger.LogInformation("{ToolName}.{Operation} completed: Operations={OperationCount}", (object) nameof (MeasureOperationsTool), (object) request.Operation, (object) operations.Length);
    return new MeasureOperationResponse()
    {
      Success = true,
      Message = "Tool description retrieved successfully",
      Operation = request.Operation,
      Help = (object) new
      {
        ToolName = "measure_operations",
        Description = "Perform operations on semantic model measures.",
        SupportedOperations = operations,
        Examples = Enumerable.Where<KeyValuePair<string, OperationMetadata>>((IEnumerable<KeyValuePair<string, OperationMetadata>>) MeasureOperationsTool.toolMetadata.Operations, (Func<KeyValuePair<string, OperationMetadata>, bool>) (p => Enumerable.Contains<string>((IEnumerable<string>) operations, p.Key, (IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase))),
        Notes = new string[7]
        {
          "Use the Operation parameter to specify which operation to perform.",
          "MeasureName is required for all operations except List.",
          "NewMeasureName is required for Rename operation.",
          "To move a measure between tables, use operation: 'Move'.",
          "Update operation cannot change table assignment.",
          "ExportTMDL exports a measure to TMDL format.",
          "If the request is declined by the user, the operation should be aborted."
        }
      }
    };
  }

  private bool ValidateRequest(string operation, MeasureOperationRequest request)
  {
    OperationMetadata operationMetadata;
    if (!MeasureOperationsTool.toolMetadata.Operations.TryGetValue(operation, out operationMetadata))
      return true;
    JsonObject requestDict = JsonSerializer.SerializeToNode<MeasureOperationRequest>(request) as JsonObject;
    List<string> list1 = Enumerable.ToList<string>(Enumerable.Where<string>((IEnumerable<string>) operationMetadata.RequiredParams, (p => requestDict != null && requestDict[p] == null)));
    List<string> list2 = Enumerable.ToList<string>(Enumerable.Where<string>((IEnumerable<string>) operationMetadata.ForbiddenParams, (p => requestDict != null && requestDict[p] != null)));
    if (Enumerable.Any<string>((IEnumerable<string>) list1))
      throw new McpException($"Missing required parameters needed for {operation} operation: {string.Join(", ", (IEnumerable<string>) list1)}");
    if (Enumerable.Any<string>((IEnumerable<string>) list2))
      throw new McpException($"Forbidden parameters not allowed for {operation} operation: {string.Join(", ", (IEnumerable<string>) list2)}");
    return true;
  }

  static MeasureOperationsTool()
  {
    ToolMetadata toolMetadata1 = new ToolMetadata();
    ToolMetadata toolMetadata2 = toolMetadata1;
    Dictionary<string, OperationMetadata> dictionary1 = new Dictionary<string, OperationMetadata>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    Dictionary<string, OperationMetadata> dictionary2 = dictionary1;
    OperationMetadata operationMetadata1 = new OperationMetadata { RequiredParams = new string[1]
    {
      "CreateDefinition"
    } };
    operationMetadata1.Description = "Create a new measure.\r\nMandatory properties: CreateDefinition (with Name, Expression, TableName).\r\nOptional: Description, FormatString, IsHidden, IsSimpleMeasure, DisplayFolder, DataType, DataCategory, LineageTag, SourceLineageTag, KPI, DetailRowsExpression, FormatStringExpression, Annotations, ExtendedProperties.";
    operationMetadata1.CommonMistakes = new string[1]
    {
      "Forgetting to supply the host table of the measure to be created in CreateDefinition.TableName"
    };
    OperationMetadata operationMetadata2 = operationMetadata1;
    List<string> stringList1 = new List<string>();
    stringList1.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Create\",\r\n        \"CreateDefinition\": { \r\n            \"Name\": \"TotalSales\", \r\n            \"TableName\": \"Sales\", \r\n            \"Expression\": \"SUM([SalesAmount])\" \r\n        }\r\n    }\r\n}");
    operationMetadata2.ExampleRequests = stringList1;
    OperationMetadata operationMetadata3 = operationMetadata1;
    dictionary2["Create"] = operationMetadata3;
    Dictionary<string, OperationMetadata> dictionary3 = dictionary1;
    OperationMetadata operationMetadata4 = new OperationMetadata { RequiredParams = new string[1]
    {
      "UpdateDefinition"
    } };
    operationMetadata4.Description = "Update properties, except for name or table, of an existing measure.\r\nMandatory properties: UpdateDefinition (with Name).\r\nOptional: Expression, Description, FormatString, IsHidden, IsSimpleMeasure, DisplayFolder, DataType, DataCategory, LineageTag, SourceLineageTag, KPI, DetailRowsExpression, FormatStringExpression, Annotations, ExtendedProperties.";
    operationMetadata4.CommonMistakes = new string[1]
    {
      "Cannot change tableName via Update - use Move operation instead"
    };
    OperationMetadata operationMetadata5 = operationMetadata4;
    List<string> stringList2 = new List<string>();
    stringList2.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Update\",\r\n        \"MeasureName\": \"TotalSales\",\r\n        \"TableName\": \"Sales\",\r\n        \"UpdateDefinition\": { \r\n            \"Name\": \"TotalSales\", \r\n            \"TableName\": \"Sales\", \r\n            \"Expression\": \"SUM([SalesAmount])\" \r\n        }\r\n    }\r\n}");
    operationMetadata5.ExampleRequests = stringList2;
    OperationMetadata operationMetadata6 = operationMetadata4;
    dictionary3["Update"] = operationMetadata6;
    Dictionary<string, OperationMetadata> dictionary4 = dictionary1;
    OperationMetadata operationMetadata7 = new OperationMetadata { RequiredParams = new string[2]
    {
      "MeasureName",
      "ShouldCascadeDelete"
    } };
    operationMetadata7.Description = "Delete a measure.\r\nMandatory properties: MeasureName, ShouldCascadeDelete.\r\nOptional: None.";
    operationMetadata7.Tips = new string[1]
    {
      "Use ShouldCascadeDelete to delete dependencies of the measure"
    };
    OperationMetadata operationMetadata8 = operationMetadata7;
    List<string> stringList3 = new List<string>();
    stringList3.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Delete\",\r\n        \"MeasureName\": \"ObsoleteMeasure\",\r\n        \"ShouldCascadeDelete\": true\r\n    }\r\n}");
    operationMetadata8.ExampleRequests = stringList3;
    OperationMetadata operationMetadata9 = operationMetadata7;
    dictionary4["Delete"] = operationMetadata9;
    Dictionary<string, OperationMetadata> dictionary5 = dictionary1;
    OperationMetadata operationMetadata10 = new OperationMetadata { RequiredParams = new string[1]
    {
      "MeasureName"
    } };
    operationMetadata10.Description = "Get details of a specific measure.\r\nMandatory properties: MeasureName.\r\nOptional: None.";
    OperationMetadata operationMetadata11 = operationMetadata10;
    List<string> stringList4 = new List<string>();
    stringList4.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Get\",\r\n        \"MeasureName\": \"TotalSales\"\r\n    }\r\n}");
    operationMetadata11.ExampleRequests = stringList4;
    OperationMetadata operationMetadata12 = operationMetadata10;
    dictionary5["Get"] = operationMetadata12;
    Dictionary<string, OperationMetadata> dictionary6 = dictionary1;
    OperationMetadata operationMetadata13 = new OperationMetadata { Description = "List all measures of a table or list all measures across tables.\r\nMandatory properties: None.\r\nOptional: TableName (to filter measures by table)." };
    List<string> stringList5 = new List<string>();
    stringList5.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"List\",\r\n        \"TableName\": \"Sales\"\r\n    }\r\n}");
    operationMetadata13.ExampleRequests = stringList5;
    dictionary6["List"] = operationMetadata13;
    Dictionary<string, OperationMetadata> dictionary7 = dictionary1;
    OperationMetadata operationMetadata14 = new OperationMetadata { RequiredParams = new string[1]
    {
      "RenameDefinition"
    } };
    operationMetadata14.Description = "Rename a measure.\r\nMandatory properties: RenameDefinition (with CurrentName, NewName).\r\nOptional: TableName (in RenameDefinition), MeasureName (fallback for CurrentName).";
    OperationMetadata operationMetadata15 = operationMetadata14;
    List<string> stringList6 = new List<string>();
    stringList6.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Rename\",\r\n        \"RenameDefinition\": { \r\n            \"CurrentName\": \"OldMeasure\", \r\n            \"NewName\": \"NewMeasure\",\r\n            \"TableName\": \"Sales\"\r\n        }\r\n    }\r\n}");
    operationMetadata15.ExampleRequests = stringList6;
    OperationMetadata operationMetadata16 = operationMetadata14;
    dictionary7["Rename"] = operationMetadata16;
    Dictionary<string, OperationMetadata> dictionary8 = dictionary1;
    OperationMetadata operationMetadata17 = new OperationMetadata { RequiredParams = new string[1]
    {
      "MoveDefinition"
    } };
    operationMetadata17.ForbiddenParams = new string[2]
    {
      "CreateDefinition",
      "UpdateDefinition"
    };
    operationMetadata17.Description = "Move a measure to a different table.\r\nMandatory properties: MoveDefinition (with Name, DestinationTableName).\r\nOptional: CurrentTableName (in MoveDefinition), MeasureName (fallback for Name).";
    operationMetadata17.CommonMistakes = new string[2]
    {
      "Don't use UpdateDefinition.TableName - use MoveDefinition.DestinationTableName",
      "Don't use delete and recreate - Move operation handles the transfer"
    };
    OperationMetadata operationMetadata18 = operationMetadata17;
    List<string> stringList7 = new List<string>();
    stringList7.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Move\",\r\n        \"MoveDefinition\": {\r\n            \"Name\": \"MyMeasure\",\r\n            \"DestinationTableName\": \"NewTable\",\r\n            \"CurrentTableName\": \"OldTable\"\r\n        }\r\n    }\r\n}");
    operationMetadata18.ExampleRequests = stringList7;
    operationMetadata17.Tips = new string[1]
    {
      "Use this instead of Delete and Create for better performance"
    };
    OperationMetadata operationMetadata19 = operationMetadata17;
    dictionary8["Move"] = operationMetadata19;
    Dictionary<string, OperationMetadata> dictionary9 = dictionary1;
    OperationMetadata operationMetadata20 = new OperationMetadata { RequiredParams = new string[1]
    {
      "MeasureName"
    } };
    operationMetadata20.Description = "Export measure to TMDL format.\r\nMandatory properties: MeasureName.\r\nOptional: TmdlExportOptions.";
    OperationMetadata operationMetadata21 = operationMetadata20;
    List<string> stringList8 = new List<string>();
    stringList8.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"ExportTMDL\",\r\n        \"MeasureName\": \"TotalSales\"\r\n    }\r\n}");
    operationMetadata21.ExampleRequests = stringList8;
    OperationMetadata operationMetadata22 = operationMetadata20;
    dictionary9["ExportTMDL"] = operationMetadata22;
    Dictionary<string, OperationMetadata> dictionary10 = dictionary1;
    OperationMetadata operationMetadata23 = new OperationMetadata { Description = "Describe the tool and its operations.\r\nMandatory properties: None.\r\nOptional: None." };
    List<string> stringList9 = new List<string>();
    stringList9.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Help\"\r\n    }\r\n}");
    operationMetadata23.ExampleRequests = stringList9;
    dictionary10["Help"] = operationMetadata23;
    Dictionary<string, OperationMetadata> dictionary11 = dictionary1;
    toolMetadata2.Operations = dictionary11;
    MeasureOperationsTool.toolMetadata = toolMetadata1;
  }
}
