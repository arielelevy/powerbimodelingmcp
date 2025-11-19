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
public class NamedExpressionOperationsTool
{
  private readonly ILogger<NamedExpressionOperationsTool> _logger;
  public static readonly ToolMetadata toolMetadata;

  public NamedExpressionOperationsTool(ILogger<NamedExpressionOperationsTool> logger)
  {
    this._logger = logger;
  }

  [McpServerTool(Name = "named_expression_operations")]
  [Description("Perform operations on semantic model named expressions and Power Query parameters. Supported operations: Help, Create, Update, Delete, Get, List, Rename, CreateParameter, UpdateParameter, ExportTMDL. Use the Operation parameter to specify which operation to perform. NamedExpressionName is required for all operations except List. Parameter operations create Power Query parameters with required metadata. ExportTMDL exports a named expression to TMDL format.")]
  public NamedExpressionOperationResponse ExecuteNamedExpressionOperation(
    McpServer mcpServer,
    NamedExpressionOperationRequest request)
  {
    this._logger.LogDebug("Executing {ToolName}.{Operation}: NamedExpression={NamedExpressionName}, Connection={ConnectionName}", (object) nameof (NamedExpressionOperationsTool), (object) request.Operation, (object) (request.NamedExpressionName ?? "(none)"), (object) (request.ConnectionName ?? "(last used)"));
    try
    {
      string[] strArray1 = new string[10]
      {
        "CREATE",
        "UPDATE",
        "DELETE",
        "GET",
        "LIST",
        "RENAME",
        "CREATEPARAMETER",
        "UPDATEPARAMETER",
        "EXPORTTMDL",
        "HELP"
      };
      string[] strArray2 = new string[6]
      {
        "CREATE",
        "UPDATE",
        "DELETE",
        "RENAME",
        "CREATEPARAMETER",
        "UPDATEPARAMETER"
      };
      if (!Enumerable.Contains<string>((IEnumerable<string>) strArray1, request.Operation.ToUpperInvariant()))
      {
        this._logger.LogWarning("Invalid operation '{Operation}' requested for {ToolName}. Valid operations: {ValidOperations}", (object) request.Operation, (object) nameof (NamedExpressionOperationsTool), (object) string.Join(", ", strArray1));
        return NamedExpressionOperationResponse.Forbidden(request.Operation, $"Invalid operation: {request.Operation}. Supported operations: {string.Join(", ", strArray1)}");
      }
      if (!this.ValidateRequest(request.Operation, request))
        throw new McpException($"Invalid request for {request.Operation} operation.");
      if (Enumerable.Contains<string>((IEnumerable<string>) strArray2, request.Operation.ToUpperInvariant()))
      {
        WriteOperationResult writeOperationResult = WriteGuard.ExecuteWriteOperationWithGuards(mcpServer, request.ConnectionName, request.Operation);
        if (!writeOperationResult.Success)
        {
          this._logger.LogWarning("{ToolName}.{Operation} blocked by write guard: {Reason}", (object) nameof (NamedExpressionOperationsTool), (object) request.Operation, (object) writeOperationResult.Message);
          return NamedExpressionOperationResponse.Forbidden(request.Operation, writeOperationResult.Message);
        }
      }
      bool allowed = WriteGuard.IsWriteAllowed("").allowed;
      string upperInvariant = request.Operation.ToUpperInvariant();
      NamedExpressionOperationResponse operationResponse;
      if (upperInvariant != null)
      {
        switch (upperInvariant.Length)
        {
          case 3:
            if ((upperInvariant == "GET"))
            {
              operationResponse = this.HandleGetOperation(request);
              goto label_34;
            }
            break;
          case 4:
            switch (upperInvariant[0])
            {
              case 'H':
                if ((upperInvariant == "HELP"))
                {
                  operationResponse = this.HandleHelpOperation(request, allowed ? strArray1 : Enumerable.ToArray<string>(Enumerable.Except<string>((IEnumerable<string>) strArray1, (IEnumerable<string>) strArray2)));
                  goto label_34;
                }
                break;
              case 'L':
                if ((upperInvariant == "LIST"))
                {
                  operationResponse = this.HandleListOperation(request);
                  goto label_34;
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
                  goto label_34;
                }
                break;
              case 'D':
                if ((upperInvariant == "DELETE"))
                {
                  operationResponse = this.HandleDeleteOperation(request);
                  goto label_34;
                }
                break;
              case 'R':
                if ((upperInvariant == "RENAME"))
                {
                  operationResponse = this.HandleRenameOperation(request);
                  goto label_34;
                }
                break;
              case 'U':
                if ((upperInvariant == "UPDATE"))
                {
                  operationResponse = this.HandleUpdateOperation(request);
                  goto label_34;
                }
                break;
            }
            break;
          case 10:
            if ((upperInvariant == "EXPORTTMDL"))
            {
              operationResponse = this.HandleExportTMDLOperation(request);
              goto label_34;
            }
            break;
          case 15:
            switch (upperInvariant[0])
            {
              case 'C':
                if ((upperInvariant == "CREATEPARAMETER"))
                {
                  operationResponse = this.HandleCreateParameterOperation(request);
                  goto label_34;
                }
                break;
              case 'U':
                if ((upperInvariant == "UPDATEPARAMETER"))
                {
                  operationResponse = this.HandleUpdateParameterOperation(request);
                  goto label_34;
                }
                break;
            }
            break;
        }
      }
      operationResponse = NamedExpressionOperationResponse.Forbidden(request.Operation, $"Operation {request.Operation} is not implemented");
label_34:
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Error executing {ToolName}.{Operation}: {ErrorMessage}", (object) nameof (NamedExpressionOperationsTool), (object) request.Operation, (object) ex.Message);
      return new NamedExpressionOperationResponse()
      {
        Success = false,
        Message = "Error executing named expression operation: " + ex.Message,
        Operation = request.Operation,
        NamedExpressionName = request.NamedExpressionName
      };
    }
  }

  private NamedExpressionOperationResponse HandleCreateOperation(
    NamedExpressionOperationRequest request)
  {
    try
    {
      if (string.IsNullOrEmpty(request.CreateDefinition.Name))
        request.CreateDefinition.Name = !string.IsNullOrEmpty(request.NamedExpressionName) ? request.NamedExpressionName : throw new McpException("NamedExpressionName is required for Create operation (either in request or CreateDefinition)");
      else if (!string.IsNullOrEmpty(request.NamedExpressionName) && (request.CreateDefinition.Name != request.NamedExpressionName))
        throw new McpException($"Named expression name mismatch: Request specifies '{request.NamedExpressionName}' but CreateDefinition specifies '{request.CreateDefinition.Name}'");
      NamedExpressionOperations.NamedExpressionOperationResult namedExpression = NamedExpressionOperations.CreateNamedExpression(request.ConnectionName, request.CreateDefinition);
      List<string> stringList = namedExpression.Warnings != null ? new List<string>((IEnumerable<string>) namedExpression.Warnings) : new List<string>();
      if ((namedExpression.State != "Ready") && !string.IsNullOrEmpty(namedExpression.ErrorMessage))
        stringList.Add(namedExpression.ErrorMessage);
      NamedExpressionOperationResponse operation = new NamedExpressionOperationResponse()
      {
        Success = (namedExpression.State == "Ready"),
        Message = (namedExpression.State == "Ready") ? $"Named expression '{request.CreateDefinition.Name}' created successfully" : namedExpression.ErrorMessage ?? $"Failed to create named expression '{request.CreateDefinition.Name}'",
        Operation = request.Operation,
        NamedExpressionName = request.CreateDefinition.Name,
        Data = (object) namedExpression,
        Warnings = stringList
      };
      if (operation.Success)
        this._logger.LogInformation("{ToolName}.{Operation} completed: NamedExpression={NamedExpressionName}", (object) nameof (NamedExpressionOperationsTool), (object) request.Operation, (object) request.CreateDefinition.Name);
      else
        this._logger.LogWarning("{ToolName}.{Operation} completed with warnings: NamedExpression={NamedExpressionName}, State={State}", (object) nameof (NamedExpressionOperationsTool), (object) request.Operation, (object) request.CreateDefinition.Name, (object) namedExpression.State);
      return operation;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      NamedExpressionOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new NamedExpressionOperationResponse()
      {
        Success = false,
        Message = "Error creating named expression: " + ex.Message,
        Operation = request.Operation,
        NamedExpressionName = request.NamedExpressionName,
        Help = (object) operationMetadata
      };
    }
  }

  private NamedExpressionOperationResponse HandleUpdateOperation(
    NamedExpressionOperationRequest request)
  {
    try
    {
      if (string.IsNullOrEmpty(request.UpdateDefinition.Name))
        request.UpdateDefinition.Name = !string.IsNullOrEmpty(request.NamedExpressionName) ? request.NamedExpressionName : throw new McpException("NamedExpressionName is required for Update operation (either in request or UpdateDefinition)");
      else if (!string.IsNullOrEmpty(request.NamedExpressionName) && (request.UpdateDefinition.Name != request.NamedExpressionName))
        throw new McpException($"Named expression name mismatch: Request specifies '{request.NamedExpressionName}' but UpdateDefinition specifies '{request.UpdateDefinition.Name}'");
      NamedExpressionOperations.NamedExpressionOperationResult expressionOperationResult = NamedExpressionOperations.UpdateNamedExpression(request.ConnectionName, request.UpdateDefinition.Name, request.UpdateDefinition);
      List<string> stringList = expressionOperationResult.Warnings != null ? new List<string>((IEnumerable<string>) expressionOperationResult.Warnings) : new List<string>();
      if ((expressionOperationResult.State != "Ready") && !string.IsNullOrEmpty(expressionOperationResult.ErrorMessage))
        stringList.Add(expressionOperationResult.ErrorMessage);
      NamedExpressionOperationResponse operationResponse = new NamedExpressionOperationResponse()
      {
        Success = (expressionOperationResult.State == "Ready"),
        Message = (expressionOperationResult.State == "Ready") ? $"Named expression '{request.UpdateDefinition.Name}' updated successfully" : expressionOperationResult.ErrorMessage ?? $"Failed to update named expression '{request.UpdateDefinition.Name}'",
        Operation = request.Operation,
        NamedExpressionName = request.UpdateDefinition.Name,
        Data = (object) expressionOperationResult,
        Warnings = stringList
      };
      if (operationResponse.Success)
        this._logger.LogInformation("{ToolName}.{Operation} completed: NamedExpression={NamedExpressionName}", (object) nameof (NamedExpressionOperationsTool), (object) request.Operation, (object) request.UpdateDefinition.Name);
      else
        this._logger.LogWarning("{ToolName}.{Operation} completed with warnings: NamedExpression={NamedExpressionName}, State={State}", (object) nameof (NamedExpressionOperationsTool), (object) request.Operation, (object) request.UpdateDefinition.Name, (object) expressionOperationResult.State);
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      NamedExpressionOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new NamedExpressionOperationResponse()
      {
        Success = false,
        Message = "Error updating named expression: " + ex.Message,
        Operation = request.Operation,
        NamedExpressionName = request.NamedExpressionName,
        Help = (object) operationMetadata
      };
    }
  }

  private NamedExpressionOperationResponse HandleDeleteOperation(
    NamedExpressionOperationRequest request)
  {
    try
    {
      NamedExpressionOperations.DeleteNamedExpression(request.ConnectionName, request.NamedExpressionName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: NamedExpression={NamedExpressionName}", (object) nameof (NamedExpressionOperationsTool), (object) request.Operation, (object) request.NamedExpressionName);
      return new NamedExpressionOperationResponse()
      {
        Success = true,
        Message = $"Named expression '{request.NamedExpressionName}' deleted successfully",
        Operation = request.Operation,
        NamedExpressionName = request.NamedExpressionName
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      NamedExpressionOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new NamedExpressionOperationResponse()
      {
        Success = false,
        Message = "Error executing named expression operation: " + ex.Message,
        Operation = request.Operation,
        NamedExpressionName = request.NamedExpressionName,
        Help = (object) operationMetadata
      };
    }
  }

  private NamedExpressionOperationResponse HandleGetOperation(
    NamedExpressionOperationRequest request)
  {
    try
    {
      NamedExpressionGet namedExpression = NamedExpressionOperations.GetNamedExpression(request.ConnectionName, request.NamedExpressionName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: NamedExpression={NamedExpressionName}", (object) nameof (NamedExpressionOperationsTool), (object) request.Operation, (object) request.NamedExpressionName);
      return new NamedExpressionOperationResponse()
      {
        Success = true,
        Message = $"Named expression '{request.NamedExpressionName}' retrieved successfully",
        Operation = request.Operation,
        NamedExpressionName = request.NamedExpressionName,
        Data = (object) namedExpression
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      NamedExpressionOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new NamedExpressionOperationResponse()
      {
        Success = false,
        Message = "Error executing named expression operation: " + ex.Message,
        Operation = request.Operation,
        NamedExpressionName = request.NamedExpressionName,
        Help = (object) operationMetadata
      };
    }
  }

  private NamedExpressionOperationResponse HandleListOperation(
    NamedExpressionOperationRequest request)
  {
    try
    {
      List<NamedExpressionList> namedExpressionListList = NamedExpressionOperations.ListNamedExpressions(request.ConnectionName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Count={Count}", (object) nameof (NamedExpressionOperationsTool), (object) request.Operation, (object) namedExpressionListList.Count);
      NamedExpressionOperationResponse operationResponse = new NamedExpressionOperationResponse { Success = true };
      operationResponse.Message = $"Found {namedExpressionListList.Count} named expressions in the model";
      operationResponse.Operation = request.Operation;
      operationResponse.Data = (object) namedExpressionListList;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      NamedExpressionOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new NamedExpressionOperationResponse()
      {
        Success = false,
        Message = "Error listing named expressions: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private NamedExpressionOperationResponse HandleRenameOperation(
    NamedExpressionOperationRequest request)
  {
    try
    {
      if (string.IsNullOrEmpty(request.RenameDefinition.CurrentName))
        request.RenameDefinition.CurrentName = !string.IsNullOrEmpty(request.NamedExpressionName) ? request.NamedExpressionName : throw new McpException("Either NamedExpressionName or RenameDefinition.CurrentName is required.");
      NamedExpressionOperations.RenameNamedExpression(request.ConnectionName, request.RenameDefinition.CurrentName, request.RenameDefinition.NewName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: From={OldName}, To={NewName}", (object) nameof (NamedExpressionOperationsTool), (object) request.Operation, (object) request.RenameDefinition.CurrentName, (object) request.RenameDefinition.NewName);
      NamedExpressionOperationResponse operationResponse = new NamedExpressionOperationResponse { Success = true };
      operationResponse.Message = $"Named expression '{request.RenameDefinition.CurrentName}' renamed to '{request.RenameDefinition.NewName}' successfully";
      operationResponse.Operation = request.Operation;
      operationResponse.NamedExpressionName = request.RenameDefinition.NewName;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      NamedExpressionOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new NamedExpressionOperationResponse()
      {
        Success = false,
        Message = "Error renaming named expression: " + ex.Message,
        Operation = request.Operation,
        NamedExpressionName = request.NamedExpressionName,
        Help = (object) operationMetadata
      };
    }
  }

  private NamedExpressionOperationResponse HandleCreateParameterOperation(
    NamedExpressionOperationRequest request)
  {
    try
    {
      if (string.IsNullOrEmpty(request.CreateDefinition.Name))
        request.CreateDefinition.Name = !string.IsNullOrEmpty(request.NamedExpressionName) ? request.NamedExpressionName : throw new McpException("ParameterName is required for CreateParameter operation");
      else if (!string.IsNullOrEmpty(request.NamedExpressionName) && (request.CreateDefinition.Name != request.NamedExpressionName))
        throw new McpException($"Parameter name mismatch: Request specifies '{request.NamedExpressionName}' but CreateDefinition specifies '{request.CreateDefinition.Name}'");
      NamedExpressionOperations.NamedExpressionOperationResult parameter = NamedExpressionOperations.CreateParameter(request.ConnectionName, request.CreateDefinition);
      NamedExpressionOperationResponse operationResponse = new NamedExpressionOperationResponse { Success = (parameter.State == "Ready") };
      operationResponse.Message = (parameter.State == "Ready") ? $"Parameter '{request.CreateDefinition.Name}' created successfully" : parameter.ErrorMessage ?? $"Failed to create parameter '{request.CreateDefinition.Name}'";
      operationResponse.Operation = request.Operation;
      operationResponse.NamedExpressionName = request.CreateDefinition.Name;
      operationResponse.Data = (object) parameter;
      List<string> stringList1;
      if (!(parameter.State != "Ready") || string.IsNullOrEmpty(parameter.ErrorMessage))
      {
        stringList1 = new List<string>();
      }
      else
      {
        List<string> stringList2 = new List<string>();
        stringList2.Add(parameter.ErrorMessage);
        stringList1 = stringList2;
      }
      operationResponse.Warnings = stringList1;
      NamedExpressionOperationResponse parameterOperation = operationResponse;
      if (parameterOperation.Success)
        this._logger.LogInformation("{ToolName}.{Operation} completed: Parameter={ParameterName}", (object) nameof (NamedExpressionOperationsTool), (object) request.Operation, (object) request.CreateDefinition.Name);
      else
        this._logger.LogWarning("{ToolName}.{Operation} completed with warnings: Parameter={ParameterName}, State={State}", (object) nameof (NamedExpressionOperationsTool), (object) request.Operation, (object) request.CreateDefinition.Name, (object) parameter.State);
      return parameterOperation;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      NamedExpressionOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new NamedExpressionOperationResponse()
      {
        Success = false,
        Message = "Error creating parameter: " + ex.Message,
        Operation = request.Operation,
        NamedExpressionName = request.NamedExpressionName,
        Help = (object) operationMetadata
      };
    }
  }

  private NamedExpressionOperationResponse HandleUpdateParameterOperation(
    NamedExpressionOperationRequest request)
  {
    try
    {
      if (string.IsNullOrEmpty(request.UpdateDefinition.Name))
        request.UpdateDefinition.Name = !string.IsNullOrEmpty(request.NamedExpressionName) ? request.NamedExpressionName : throw new McpException("ParameterName is required for UpdateParameter operation");
      else if (!string.IsNullOrEmpty(request.NamedExpressionName) && (request.UpdateDefinition.Name != request.NamedExpressionName))
        throw new McpException($"Parameter name mismatch: Request specifies '{request.NamedExpressionName}' but UpdateDefinition specifies '{request.UpdateDefinition.Name}'");
      NamedExpressionOperations.NamedExpressionOperationResult expressionOperationResult = NamedExpressionOperations.UpdateParameter(request.ConnectionName, request.UpdateDefinition.Name, request.UpdateDefinition);
      NamedExpressionOperationResponse operationResponse1 = new NamedExpressionOperationResponse { Success = (expressionOperationResult.State == "Ready") };
      operationResponse1.Message = (expressionOperationResult.State == "Ready") ? $"Parameter '{request.UpdateDefinition.Name}' updated successfully" : expressionOperationResult.ErrorMessage ?? $"Failed to update parameter '{request.UpdateDefinition.Name}'";
      operationResponse1.Operation = request.Operation;
      operationResponse1.NamedExpressionName = request.UpdateDefinition.Name;
      operationResponse1.Data = (object) expressionOperationResult;
      List<string> stringList1;
      if (!(expressionOperationResult.State != "Ready") || string.IsNullOrEmpty(expressionOperationResult.ErrorMessage))
      {
        stringList1 = new List<string>();
      }
      else
      {
        List<string> stringList2 = new List<string>();
        stringList2.Add(expressionOperationResult.ErrorMessage);
        stringList1 = stringList2;
      }
      operationResponse1.Warnings = stringList1;
      NamedExpressionOperationResponse operationResponse2 = operationResponse1;
      if (operationResponse2.Success)
        this._logger.LogInformation("{ToolName}.{Operation} completed: Parameter={ParameterName}", (object) nameof (NamedExpressionOperationsTool), (object) request.Operation, (object) request.UpdateDefinition.Name);
      else
        this._logger.LogWarning("{ToolName}.{Operation} completed with warnings: Parameter={ParameterName}, State={State}", (object) nameof (NamedExpressionOperationsTool), (object) request.Operation, (object) request.UpdateDefinition.Name, (object) expressionOperationResult.State);
      return operationResponse2;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      NamedExpressionOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new NamedExpressionOperationResponse()
      {
        Success = false,
        Message = "Error updating parameter: " + ex.Message,
        Operation = request.Operation,
        NamedExpressionName = request.NamedExpressionName,
        Help = (object) operationMetadata
      };
    }
  }

  private NamedExpressionOperationResponse HandleExportTMDLOperation(
    NamedExpressionOperationRequest request)
  {
    try
    {
      string str = NamedExpressionOperations.ExportTMDL(request.ConnectionName, request.NamedExpressionName, (ExportTmdl) request.TmdlExportOptions);
      this._logger.LogInformation("{ToolName}.{Operation} completed: NamedExpression={NamedExpressionName}", (object) nameof (NamedExpressionOperationsTool), (object) request.Operation, (object) request.NamedExpressionName);
      return new NamedExpressionOperationResponse()
      {
        Success = true,
        Message = $"TMDL exported for named expression '{request.NamedExpressionName}'",
        Operation = request.Operation,
        NamedExpressionName = request.NamedExpressionName,
        Data = (object) str
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      NamedExpressionOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new NamedExpressionOperationResponse()
      {
        Success = false,
        Message = $"Failed to export TMDL for named expression '{request.NamedExpressionName}': {ex.Message}",
        Operation = request.Operation,
        NamedExpressionName = request.NamedExpressionName,
        Help = (object) operationMetadata
      };
    }
  }

  private NamedExpressionOperationResponse HandleHelpOperation(
    NamedExpressionOperationRequest request,
    string[] operations)
  {
    this._logger.LogInformation("{ToolName}.{Operation} completed: Operations={OperationCount}", (object) nameof (NamedExpressionOperationsTool), (object) request.Operation, (object) operations.Length);
    return new NamedExpressionOperationResponse()
    {
      Success = true,
      Message = "Tool description retrieved successfully",
      Operation = request.Operation,
      Help = (object) new
      {
        ToolName = "named_expression_operations",
        Description = "Perform operations on semantic model named expressions and Power Query parameters.",
        SupportedOperations = operations,
        Examples = Enumerable.Where<KeyValuePair<string, OperationMetadata>>((IEnumerable<KeyValuePair<string, OperationMetadata>>) NamedExpressionOperationsTool.toolMetadata.Operations, (Func<KeyValuePair<string, OperationMetadata>, bool>) (p => Enumerable.Contains<string>((IEnumerable<string>) operations, p.Key, (IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase))),
        Notes = new string[4]
        {
          "NamedExpressionName is required for all operations except List.",
          "NewNamedExpressionName is required for Rename operation.",
          "Parameter operations create Power Query parameters with required metadata.",
          "ExportTMDL exports a named expression to TMDL format."
        }
      }
    };
  }

  private bool ValidateRequest(string operation, NamedExpressionOperationRequest request)
  {
    OperationMetadata operationMetadata;
    if (!NamedExpressionOperationsTool.toolMetadata.Operations.TryGetValue(operation, out operationMetadata))
      return true;
    JsonObject requestDict = JsonSerializer.SerializeToNode<NamedExpressionOperationRequest>(request) as JsonObject;
    List<string> list1 = Enumerable.ToList<string>(Enumerable.Where<string>((IEnumerable<string>) operationMetadata.RequiredParams, (p => requestDict != null && requestDict[p] == null)));
    List<string> list2 = Enumerable.ToList<string>(Enumerable.Where<string>((IEnumerable<string>) operationMetadata.ForbiddenParams, (p => requestDict != null && requestDict[p] != null)));
    if (Enumerable.Any<string>((IEnumerable<string>) list1))
      throw new McpException($"Missing required parameters needed for {operation} operation: {string.Join(", ", (IEnumerable<string>) list1)}");
    if (Enumerable.Any<string>((IEnumerable<string>) list2))
      throw new McpException($"Forbidden parameters not allowed for {operation} operation: {string.Join(", ", (IEnumerable<string>) list2)}");
    return true;
  }

  static NamedExpressionOperationsTool()
  {
    ToolMetadata toolMetadata1 = new ToolMetadata();
    ToolMetadata toolMetadata2 = toolMetadata1;
    Dictionary<string, OperationMetadata> dictionary1 = new Dictionary<string, OperationMetadata>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    Dictionary<string, OperationMetadata> dictionary2 = dictionary1;
    OperationMetadata operationMetadata1 = new OperationMetadata { RequiredParams = new string[1]
    {
      "CreateDefinition"
    } };
    operationMetadata1.Description = "Create a new named expression in the semantic model. Named expressions are M expressions that can be shared across multiple tables/partitions and enable scenarios like parameterized data sources, shared logic, and temporary overrides via OOL bindings during refresh operations.\r\nMandatory properties: CreateDefinition (with Name, Expression, Kind).\r\nOptional: Description, LineageTag, SourceLineageTag, QueryGroupName, Annotations, ExtendedProperties.";
    operationMetadata1.CommonMistakes = new string[3]
    {
      "Using Create for Power Query parameters instead of CreateParameter operation",
      "Missing Kind property (required for Create, auto-set by CreateParameter)",
      "Incorrect parameter expression syntax when creating parameters manually"
    };
    operationMetadata1.Tips = new string[3]
    {
      "Use CreateParameter for Power Query parameters - it handles metadata and syntax automatically",
      "Use Create for generic M expressions like shared query logic or data transformations",
      "Kind is always set to 'M' since named expressions are Power Query expressions"
    };
    OperationMetadata operationMetadata2 = operationMetadata1;
    List<string> stringList1 = new List<string>();
    stringList1.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Create\",\r\n        \"CreateDefinition\": {\r\n            \"Name\": \"MyExpression\",\r\n            \"Expression\": \"let Source = Excel.CurrentWorkbook(){[Name=\\\"SalesData\\\"]}[Content], FilteredRows = Table.SelectRows(Source, each [Region] = \\\"West\\\") in FilteredRows\",\r\n            \"Kind\": \"M\",\r\n            \"Description\": \"My description\",\r\n            \"QueryGroupName\": \"QueryGroup1\"\r\n        }\r\n    }\r\n}");
    operationMetadata2.ExampleRequests = stringList1;
    OperationMetadata operationMetadata3 = operationMetadata1;
    dictionary2["Create"] = operationMetadata3;
    Dictionary<string, OperationMetadata> dictionary3 = dictionary1;
    OperationMetadata operationMetadata4 = new OperationMetadata { RequiredParams = new string[1]
    {
      "UpdateDefinition"
    } };
    operationMetadata4.Description = "Update an existing named expression. Names cannot be changed; use the Rename operation instead.\r\nMandatory properties: UpdateDefinition (with Name matching target expression).\r\nOptional: Expression, Kind, Description, LineageTag, SourceLineageTag, QueryGroupName, Annotations, ExtendedProperties.";
    operationMetadata4.CommonMistakes = new string[3]
    {
      "Using Update for Power Query parameters instead of UpdateParameter operation",
      "Attempting to change the Name property (use Rename operation instead)",
      "Breaking parameter metadata when updating parameter expressions manually"
    };
    operationMetadata4.Tips = new string[3]
    {
      "Use UpdateParameter for Power Query parameters - it preserves metadata and handles syntax",
      "Use Update for generic M expressions like shared query logic or data transformations",
      "Omit properties to keep current values, set to empty string to clear (except Expression which cannot be empty)"
    };
    OperationMetadata operationMetadata5 = operationMetadata4;
    List<string> stringList2 = new List<string>();
    stringList2.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Update\",\r\n        \"NamedExpressionName\": \"MyExpression\",\r\n        \"UpdateDefinition\": {\r\n            \"Name\": \"MyExpression\",\r\n            \"Expression\": \"let Source = Excel.CurrentWorkbook(){[Name=\\\"SalesData\\\"]}[Content], FilteredRows = Table.SelectRows(Source, each [Region] = \\\"West\\\") in FilteredRows\",\r\n            \"Kind\": \"M\"\r\n        }\r\n    }\r\n}");
    operationMetadata5.ExampleRequests = stringList2;
    OperationMetadata operationMetadata6 = operationMetadata4;
    dictionary3["Update"] = operationMetadata6;
    Dictionary<string, OperationMetadata> dictionary4 = dictionary1;
    OperationMetadata operationMetadata7 = new OperationMetadata { RequiredParams = new string[1]
    {
      "NamedExpressionName"
    } };
    operationMetadata7.Description = "Delete a named expression from the semantic model.\r\nMandatory properties: NamedExpressionName.";
    OperationMetadata operationMetadata8 = operationMetadata7;
    List<string> stringList3 = new List<string>();
    stringList3.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Delete\",\r\n        \"NamedExpressionName\": \"MyExpression\"\r\n    }\r\n}");
    operationMetadata8.ExampleRequests = stringList3;
    OperationMetadata operationMetadata9 = operationMetadata7;
    dictionary4["Delete"] = operationMetadata9;
    Dictionary<string, OperationMetadata> dictionary5 = dictionary1;
    OperationMetadata operationMetadata10 = new OperationMetadata { RequiredParams = new string[1]
    {
      "NamedExpressionName"
    } };
    operationMetadata10.Description = "Retrieve detailed information about a specific named expression.\r\nMandatory properties: NamedExpressionName.";
    operationMetadata10.Tips = new string[2]
    {
      "Use List operation first to see all available named expressions",
      "Parameters will have metadata like IsParameterQuery=true in their Expression"
    };
    OperationMetadata operationMetadata11 = operationMetadata10;
    List<string> stringList4 = new List<string>();
    stringList4.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Get\",\r\n        \"NamedExpressionName\": \"MyExpression\"\r\n    }\r\n}");
    operationMetadata11.ExampleRequests = stringList4;
    OperationMetadata operationMetadata12 = operationMetadata10;
    dictionary5["Get"] = operationMetadata12;
    Dictionary<string, OperationMetadata> dictionary6 = dictionary1;
    OperationMetadata operationMetadata13 = new OperationMetadata { Description = "List all named expressions in the semantic model with basic information.\r\nNo mandatory properties required." };
    operationMetadata13.Tips = new string[3]
    {
      "Use this to discover available named expressions before Get, Update, or Delete operations",
      "All named expressions have Kind='M' since they are Power Query expressions",
      "Parameters are special M expressions with metadata - use CreateParameter/UpdateParameter for better handling"
    };
    OperationMetadata operationMetadata14 = operationMetadata13;
    List<string> stringList5 = new List<string>();
    stringList5.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"List\"\r\n    }\r\n}");
    operationMetadata14.ExampleRequests = stringList5;
    OperationMetadata operationMetadata15 = operationMetadata13;
    dictionary6["List"] = operationMetadata15;
    Dictionary<string, OperationMetadata> dictionary7 = dictionary1;
    OperationMetadata operationMetadata16 = new OperationMetadata { RequiredParams = new string[1]
    {
      "RenameDefinition"
    } };
    operationMetadata16.Description = "Rename a named expression by changing its identifier.\r\nMandatory properties: RenameDefinition (with CurrentName, NewName). If CurrentName is omitted, NamedExpressionName will be used.";
    operationMetadata16.Tips = new string[3]
    {
      "Use this instead of Update operation when you need to change the name",
      "Works for both generic named expressions and Power Query parameters",
      "All references to the named expression will be automatically updated"
    };
    OperationMetadata operationMetadata17 = operationMetadata16;
    List<string> stringList6 = new List<string>();
    stringList6.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Rename\",\r\n        \"RenameDefinition\": { \r\n            \"CurrentName\": \"OldExpression\", \r\n            \"NewName\": \"NewExpression\"\r\n        }\r\n    }\r\n}");
    operationMetadata17.ExampleRequests = stringList6;
    OperationMetadata operationMetadata18 = operationMetadata16;
    dictionary7["Rename"] = operationMetadata18;
    Dictionary<string, OperationMetadata> dictionary8 = dictionary1;
    OperationMetadata operationMetadata19 = new OperationMetadata { RequiredParams = new string[1]
    {
      "CreateDefinition"
    } };
    operationMetadata19.Description = "Create a Power Query parameter with proper metadata. Expression will be automatically converted to parameter format if needed.\r\nMandatory properties: CreateDefinition (with Name, Expression). Kind will be automatically set to 'M'.\r\nOptional: Description, LineageTag, SourceLineageTag, QueryGroupName, Annotations, ExtendedProperties.";
    operationMetadata19.CommonMistakes = new string[1]
    {
      "Manually formatting parameter expression syntax (let the tool handle it)"
    };
    operationMetadata19.Tips = new string[5]
    {
      "Preferred over Create for Power Query parameters - handles metadata automatically",
      "Power Query parameters are also referred to as semantic model parameters or Power BI parameters",
      "Provide simple values in Expression - tool converts to proper parameter format",
      "Auto-adds metadata: IsParameterQuery=true, Type=\"Text\", IsParameterQueryRequired=true",
      "Example: Expression=\"MyDatabase\" becomes '\"MyDatabase\" meta [IsParameterQuery=true, Type=\"Text\", IsParameterQueryRequired=true]'"
    };
    OperationMetadata operationMetadata20 = operationMetadata19;
    List<string> stringList7 = new List<string>();
    stringList7.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"CreateParameter\",\r\n        \"CreateDefinition\": {\r\n            \"Name\": \"DataSourcePath\",\r\n            \"Expression\": \"https://mycompany.com/data/myfile.csv\",\r\n            \"Kind\": \"M\"\r\n        }\r\n    }\r\n}");
    operationMetadata20.ExampleRequests = stringList7;
    OperationMetadata operationMetadata21 = operationMetadata19;
    dictionary8["CreateParameter"] = operationMetadata21;
    Dictionary<string, OperationMetadata> dictionary9 = dictionary1;
    OperationMetadata operationMetadata22 = new OperationMetadata { RequiredParams = new string[1]
    {
      "UpdateDefinition"
    } };
    operationMetadata22.Description = "Update an existing Power Query parameter. Expression will be automatically converted to parameter format if needed. Names cannot be changed; use the Rename operation instead.\r\nMandatory properties: UpdateDefinition (with Name matching target parameter). Kind will be automatically set to 'M'.\r\nOptional: Expression, Description, LineageTag, SourceLineageTag, QueryGroupName, Annotations, ExtendedProperties.";
    operationMetadata22.CommonMistakes = new string[3]
    {
      "Using Update operation instead of UpdateParameter for Power Query parameters",
      "Manually formatting parameter expression syntax (let the tool handle it)",
      "Breaking existing parameter metadata by using generic Update operation"
    };
    operationMetadata22.Tips = new string[3]
    {
      "Preferred over Update for Power Query parameters - preserves metadata automatically",
      "Provide simple values in Expression - tool converts to proper parameter format",
      "Maintains existing parameter metadata while updating the value"
    };
    OperationMetadata operationMetadata23 = operationMetadata22;
    List<string> stringList8 = new List<string>();
    stringList8.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"UpdateParameter\",\r\n        \"UpdateDefinition\": {\r\n            \"Name\": \"DataSourcePath\",\r\n            \"Expression\": \"https://mycompany.com/data/myfile.csv\",\r\n            \"Kind\": \"M\"\r\n        }\r\n    }\r\n}");
    operationMetadata23.ExampleRequests = stringList8;
    OperationMetadata operationMetadata24 = operationMetadata22;
    dictionary9["UpdateParameter"] = operationMetadata24;
    Dictionary<string, OperationMetadata> dictionary10 = dictionary1;
    OperationMetadata operationMetadata25 = new OperationMetadata { RequiredParams = new string[1]
    {
      "NamedExpressionName"
    } };
    operationMetadata25.Description = "Export a named expression to TMDL format.\r\nMandatory properties: NamedExpressionName.\r\nOptional: TmdlExportOptions for customizing export format.";
    OperationMetadata operationMetadata26 = operationMetadata25;
    List<string> stringList9 = new List<string>();
    stringList9.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"ExportTMDL\",\r\n        \"NamedExpressionName\": \"MyExpression\"\r\n    }\r\n}");
    operationMetadata26.ExampleRequests = stringList9;
    OperationMetadata operationMetadata27 = operationMetadata25;
    dictionary10["ExportTMDL"] = operationMetadata27;
    Dictionary<string, OperationMetadata> dictionary11 = dictionary1;
    OperationMetadata operationMetadata28 = new OperationMetadata { Description = "Retrieve detailed information about the named expression operations tool and its capabilities.\r\nNo mandatory properties required." };
    List<string> stringList10 = new List<string>();
    stringList10.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Help\"\r\n    }\r\n}");
    operationMetadata28.ExampleRequests = stringList10;
    dictionary11["Help"] = operationMetadata28;
    Dictionary<string, OperationMetadata> dictionary12 = dictionary1;
    toolMetadata2.Operations = dictionary12;
    NamedExpressionOperationsTool.toolMetadata = toolMetadata1;
  }
}
