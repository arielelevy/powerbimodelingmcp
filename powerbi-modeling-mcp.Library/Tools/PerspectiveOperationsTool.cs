// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Tools.PerspectiveOperationsTool
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
public class PerspectiveOperationsTool
{
  private readonly ILogger<PerspectiveOperationsTool> _logger;
  public static readonly ToolMetadata toolMetadata;

  public PerspectiveOperationsTool(ILogger<PerspectiveOperationsTool> logger)
  {
    this._logger = logger;
  }

  [McpServerTool(Name = "perspective_operations")]
  [Description("Perform operations on semantic model perspectives, perspective tables, perspective columns, perspective measures, and perspective hierarchies. Supported operations: Help, List, Get, Create, Update, Delete, Rename, ListTables, GetTable, AddTable, RemoveTable, UpdateTable, ListColumns, GetColumn, AddColumn, RemoveColumn, ListMeasures, GetMeasure, AddMeasure, RemoveMeasure, ListHierarchies, GetHierarchy, AddHierarchy, RemoveHierarchy, ExportTMDL. Use the Operation parameter to specify which operation to perform. PerspectiveName is required for all operations except List. TableName is required for table, column, measure, and hierarchy operations. ColumnName is required for column-specific operations. MeasureName is required for measure-specific operations. HierarchyName is required for hierarchy-specific operations.")]
  public PerspectiveOperationResponse ExecutePerspectiveOperation(
    McpServer mcpServer,
    PerspectiveOperationRequest request)
  {
    this._logger.LogDebug("Executing {ToolName}.{Operation}: Perspective={PerspectiveName}, Table={TableName}, Connection={ConnectionName}", (object) nameof (PerspectiveOperationsTool), (object) request.Operation, (object) request.PerspectiveName, (object) request.TableName, (object) (request.ConnectionName ?? "(last used)"));
    try
    {
      string[] strArray1 = new string[25]
      {
        "LIST",
        "GET",
        "CREATE",
        "UPDATE",
        "DELETE",
        "RENAME",
        "LISTTABLES",
        "GETTABLE",
        "ADDTABLE",
        "REMOVETABLE",
        "UPDATETABLE",
        "LISTCOLUMNS",
        "GETCOLUMN",
        "ADDCOLUMN",
        "REMOVECOLUMN",
        "LISTMEASURES",
        "GETMEASURE",
        "ADDMEASURE",
        "REMOVEMEASURE",
        "LISTHIERARCHIES",
        "GETHIERARCHY",
        "ADDHIERARCHY",
        "REMOVEHIERARCHY",
        "EXPORTTMDL",
        "HELP"
      };
      string[] strArray2 = new string[13]
      {
        "CREATE",
        "UPDATE",
        "DELETE",
        "RENAME",
        "ADDTABLE",
        "REMOVETABLE",
        "UPDATETABLE",
        "ADDCOLUMN",
        "REMOVECOLUMN",
        "ADDMEASURE",
        "REMOVEMEASURE",
        "ADDHIERARCHY",
        "REMOVEHIERARCHY"
      };
      if (!Enumerable.Contains<string>((IEnumerable<string>) strArray1, request.Operation.ToUpperInvariant()))
      {
        this._logger.LogWarning("Invalid operation '{Operation}' requested for {ToolName}. Valid operations: {ValidOperations}", (object) request.Operation, (object) nameof (PerspectiveOperationsTool), (object) string.Join(", ", strArray1));
        return PerspectiveOperationResponse.Forbidden(request.Operation, $"Invalid operation: {request.Operation}. Supported operations: {string.Join(", ", strArray1)}");
      }
      if (!this.ValidateRequest(request.Operation, request))
        throw new McpException($"Invalid request for {request.Operation} operation");
      if (Enumerable.Contains<string>((IEnumerable<string>) strArray2, request.Operation.ToUpperInvariant()))
      {
        WriteOperationResult writeOperationResult = WriteGuard.ExecuteWriteOperationWithGuards(mcpServer, request.ConnectionName, request.Operation);
        if (!writeOperationResult.Success)
        {
          this._logger.LogWarning("{ToolName}.{Operation} blocked by write guard: {Reason}", (object) nameof (PerspectiveOperationsTool), (object) request.Operation, (object) writeOperationResult.Message);
          return PerspectiveOperationResponse.Forbidden(request.Operation, writeOperationResult.Message);
        }
      }
      WriteGuard.IsWriteAllowed("");
      string upperInvariant = request.Operation.ToUpperInvariant();
      PerspectiveOperationResponse operationResponse;
      if (upperInvariant != null)
      {
        switch (upperInvariant.Length)
        {
          case 3:
            if ((upperInvariant == "GET"))
            {
              operationResponse = this.HandleGetPerspectiveOperation(request);
              goto label_69;
            }
            break;
          case 4:
            switch (upperInvariant[0])
            {
              case 'H':
                if ((upperInvariant == "HELP"))
                {
                  operationResponse = this.HandleHelpOperation(request, Enumerable.ToArray<string>(Enumerable.Except<string>((IEnumerable<string>) strArray1, (IEnumerable<string>) strArray2)));
                  goto label_69;
                }
                break;
              case 'L':
                if ((upperInvariant == "LIST"))
                {
                  operationResponse = this.HandleListPerspectivesOperation(request);
                  goto label_69;
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
                  operationResponse = this.HandleCreatePerspectiveOperation(request);
                  goto label_69;
                }
                break;
              case 'D':
                if ((upperInvariant == "DELETE"))
                {
                  operationResponse = this.HandleDeletePerspectiveOperation(request);
                  goto label_69;
                }
                break;
              case 'R':
                if ((upperInvariant == "RENAME"))
                {
                  operationResponse = this.HandleRenamePerspectiveOperation(request);
                  goto label_69;
                }
                break;
              case 'U':
                if ((upperInvariant == "UPDATE"))
                {
                  operationResponse = this.HandleUpdatePerspectiveOperation(request);
                  goto label_69;
                }
                break;
            }
            break;
          case 8:
            switch (upperInvariant[0])
            {
              case 'A':
                if ((upperInvariant == "ADDTABLE"))
                {
                  operationResponse = this.HandleAddTableToPerspectiveOperation(request);
                  goto label_69;
                }
                break;
              case 'G':
                if ((upperInvariant == "GETTABLE"))
                {
                  operationResponse = this.HandleGetPerspectiveTableOperation(request);
                  goto label_69;
                }
                break;
            }
            break;
          case 9:
            switch (upperInvariant[0])
            {
              case 'A':
                if ((upperInvariant == "ADDCOLUMN"))
                {
                  operationResponse = this.HandleAddColumnToPerspectiveTableOperation(request);
                  goto label_69;
                }
                break;
              case 'G':
                if ((upperInvariant == "GETCOLUMN"))
                {
                  operationResponse = this.HandleGetPerspectiveColumnOperation(request);
                  goto label_69;
                }
                break;
            }
            break;
          case 10:
            switch (upperInvariant[0])
            {
              case 'A':
                if ((upperInvariant == "ADDMEASURE"))
                {
                  operationResponse = this.HandleAddMeasureToPerspectiveTableOperation(request);
                  goto label_69;
                }
                break;
              case 'E':
                if ((upperInvariant == "EXPORTTMDL"))
                {
                  operationResponse = this.HandleExportTMDLOperation(request);
                  goto label_69;
                }
                break;
              case 'G':
                if ((upperInvariant == "GETMEASURE"))
                {
                  operationResponse = this.HandleGetPerspectiveMeasureOperation(request);
                  goto label_69;
                }
                break;
              case 'L':
                if ((upperInvariant == "LISTTABLES"))
                {
                  operationResponse = this.HandleListPerspectiveTablesOperation(request);
                  goto label_69;
                }
                break;
            }
            break;
          case 11:
            switch (upperInvariant[0])
            {
              case 'L':
                if ((upperInvariant == "LISTCOLUMNS"))
                {
                  operationResponse = this.HandleListPerspectiveColumnsOperation(request);
                  goto label_69;
                }
                break;
              case 'R':
                if ((upperInvariant == "REMOVETABLE"))
                {
                  operationResponse = this.HandleRemoveTableFromPerspectiveOperation(request);
                  goto label_69;
                }
                break;
              case 'U':
                if ((upperInvariant == "UPDATETABLE"))
                {
                  operationResponse = this.HandleUpdatePerspectiveTableOperation(request);
                  goto label_69;
                }
                break;
            }
            break;
          case 12:
            switch (upperInvariant[0])
            {
              case 'A':
                if ((upperInvariant == "ADDHIERARCHY"))
                {
                  operationResponse = this.HandleAddHierarchyToPerspectiveTableOperation(request);
                  goto label_69;
                }
                break;
              case 'G':
                if ((upperInvariant == "GETHIERARCHY"))
                {
                  operationResponse = this.HandleGetPerspectiveHierarchyOperation(request);
                  goto label_69;
                }
                break;
              case 'L':
                if ((upperInvariant == "LISTMEASURES"))
                {
                  operationResponse = this.HandleListPerspectiveMeasuresOperation(request);
                  goto label_69;
                }
                break;
              case 'R':
                if ((upperInvariant == "REMOVECOLUMN"))
                {
                  operationResponse = this.HandleRemoveColumnFromPerspectiveTableOperation(request);
                  goto label_69;
                }
                break;
            }
            break;
          case 13:
            if ((upperInvariant == "REMOVEMEASURE"))
            {
              operationResponse = this.HandleRemoveMeasureFromPerspectiveTableOperation(request);
              goto label_69;
            }
            break;
          case 15:
            switch (upperInvariant[0])
            {
              case 'L':
                if ((upperInvariant == "LISTHIERARCHIES"))
                {
                  operationResponse = this.HandleListPerspectiveHierarchiesOperation(request);
                  goto label_69;
                }
                break;
              case 'R':
                if ((upperInvariant == "REMOVEHIERARCHY"))
                {
                  operationResponse = this.HandleRemoveHierarchyFromPerspectiveTableOperation(request);
                  goto label_69;
                }
                break;
            }
            break;
        }
      }
      operationResponse = PerspectiveOperationResponse.Forbidden(request.Operation, $"Operation {request.Operation} is not implemented");
label_69:
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Error executing {ToolName}.{Operation}: {ErrorMessage}", (object) nameof (PerspectiveOperationsTool), (object) request.Operation, (object) ex.Message);
      return new PerspectiveOperationResponse()
      {
        Success = false,
        Message = "Error executing perspective operation: " + ex.Message,
        Operation = request.Operation,
        PerspectiveName = request.PerspectiveName
      };
    }
  }

  private PerspectiveOperationResponse HandleListPerspectivesOperation(
    PerspectiveOperationRequest request)
  {
    try
    {
      List<PerspectiveList> perspectiveListList = this.ValidateRequest(request.Operation, request) ? PerspectiveOperations.ListPerspectives(request.ConnectionName) : throw new McpException($"Invalid request for {request.Operation} operation");
      this._logger.LogInformation("{ToolName}.{Operation} completed: Count={Count}", (object) nameof (PerspectiveOperationsTool), (object) "List", (object) perspectiveListList.Count);
      PerspectiveOperationResponse operationResponse = new PerspectiveOperationResponse { Success = true };
      operationResponse.Message = $"Retrieved {perspectiveListList.Count} perspective(s)";
      operationResponse.Operation = request.Operation;
      operationResponse.Data = (object) perspectiveListList;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      PerspectiveOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new PerspectiveOperationResponse()
      {
        Success = false,
        Message = "Error occurred while validating request: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private PerspectiveOperationResponse HandleGetPerspectiveOperation(
    PerspectiveOperationRequest request)
  {
    try
    {
      PerspectiveGet perspective = PerspectiveOperations.GetPerspective(request.ConnectionName, request.PerspectiveName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Perspective={PerspectiveName}", (object) nameof (PerspectiveOperationsTool), (object) "Get", (object) request.PerspectiveName);
      return new PerspectiveOperationResponse()
      {
        Success = true,
        Message = $"Retrieved perspective '{request.PerspectiveName}' successfully",
        Operation = request.Operation,
        PerspectiveName = request.PerspectiveName,
        Data = (object) perspective
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      PerspectiveOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new PerspectiveOperationResponse()
      {
        Success = false,
        Message = "Error occurred while validating request: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private PerspectiveOperationResponse HandleCreatePerspectiveOperation(
    PerspectiveOperationRequest request)
  {
    try
    {
      PerspectiveOperationResult perspective = PerspectiveOperations.CreatePerspective(request.ConnectionName, request.CreateDefinition);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Perspective={PerspectiveName}", (object) nameof (PerspectiveOperationsTool), (object) "Create", (object) perspective.PerspectiveName);
      if (!perspective.Success && !string.IsNullOrEmpty(perspective.Message))
        this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (PerspectiveOperationsTool), (object) "Create", (object) perspective.Message);
      PerspectiveOperationResponse perspectiveOperation = new PerspectiveOperationResponse { Success = perspective.Success };
      perspectiveOperation.Message = perspective.Success ? $"Perspective '{perspective.PerspectiveName}' created successfully" : perspective.Message ?? "Failed to create perspective";
      perspectiveOperation.Operation = request.Operation;
      perspectiveOperation.PerspectiveName = perspective.PerspectiveName;
      perspectiveOperation.Data = (object) perspective;
      List<string> stringList1;
      if (perspective.Success || string.IsNullOrEmpty(perspective.Message))
      {
        stringList1 = new List<string>();
      }
      else
      {
        List<string> stringList2 = new List<string>();
        stringList2.Add(perspective.Message);
        stringList1 = stringList2;
      }
      perspectiveOperation.Warnings = stringList1;
      return perspectiveOperation;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      PerspectiveOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new PerspectiveOperationResponse()
      {
        Success = false,
        Message = "Error occurred while validating request: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private PerspectiveOperationResponse HandleUpdatePerspectiveOperation(
    PerspectiveOperationRequest request)
  {
    try
    {
      PerspectiveOperationResult perspectiveOperationResult = PerspectiveOperations.UpdatePerspective(request.ConnectionName, request.PerspectiveName, request.UpdateDefinition);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Perspective={PerspectiveName}", (object) nameof (PerspectiveOperationsTool), (object) "Update", (object) request.PerspectiveName);
      if (!perspectiveOperationResult.Success && !string.IsNullOrEmpty(perspectiveOperationResult.Message))
        this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (PerspectiveOperationsTool), (object) "Update", (object) perspectiveOperationResult.Message);
      PerspectiveOperationResponse operationResponse = new PerspectiveOperationResponse { Success = perspectiveOperationResult.Success };
      operationResponse.Message = perspectiveOperationResult.Success ? $"Perspective '{request.PerspectiveName}' updated successfully" : perspectiveOperationResult.Message ?? "Failed to update perspective";
      operationResponse.Operation = request.Operation;
      operationResponse.PerspectiveName = request.PerspectiveName;
      operationResponse.Data = (object) perspectiveOperationResult;
      List<string> stringList1;
      if (perspectiveOperationResult.Success || string.IsNullOrEmpty(perspectiveOperationResult.Message))
      {
        stringList1 = new List<string>();
      }
      else
      {
        List<string> stringList2 = new List<string>();
        stringList2.Add(perspectiveOperationResult.Message);
        stringList1 = stringList2;
      }
      operationResponse.Warnings = stringList1;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      PerspectiveOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new PerspectiveOperationResponse()
      {
        Success = false,
        Message = "Error occurred while validating request: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private PerspectiveOperationResponse HandleDeletePerspectiveOperation(
    PerspectiveOperationRequest request)
  {
    try
    {
      PerspectiveOperationResult perspectiveOperationResult = PerspectiveOperations.DeletePerspective(request.ConnectionName, request.PerspectiveName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Perspective={PerspectiveName}", (object) nameof (PerspectiveOperationsTool), (object) "Delete", (object) request.PerspectiveName);
      if (!perspectiveOperationResult.Success && !string.IsNullOrEmpty(perspectiveOperationResult.Message))
        this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (PerspectiveOperationsTool), (object) "Delete", (object) perspectiveOperationResult.Message);
      PerspectiveOperationResponse operationResponse = new PerspectiveOperationResponse { Success = perspectiveOperationResult.Success };
      operationResponse.Message = perspectiveOperationResult.Success ? $"Perspective '{request.PerspectiveName}' deleted successfully" : perspectiveOperationResult.Message ?? "Failed to delete perspective";
      operationResponse.Operation = request.Operation;
      operationResponse.PerspectiveName = request.PerspectiveName;
      operationResponse.Data = (object) perspectiveOperationResult;
      List<string> stringList1;
      if (perspectiveOperationResult.Success || string.IsNullOrEmpty(perspectiveOperationResult.Message))
      {
        stringList1 = new List<string>();
      }
      else
      {
        List<string> stringList2 = new List<string>();
        stringList2.Add(perspectiveOperationResult.Message);
        stringList1 = stringList2;
      }
      operationResponse.Warnings = stringList1;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      PerspectiveOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new PerspectiveOperationResponse()
      {
        Success = false,
        Message = "Error occurred while validating request: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private PerspectiveOperationResponse HandleRenamePerspectiveOperation(
    PerspectiveOperationRequest request)
  {
    try
    {
      PerspectiveOperationResult perspectiveOperationResult = PerspectiveOperations.RenamePerspective(request.ConnectionName, request.PerspectiveName, request.NewPerspectiveName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: From={OldName}, To={NewName}", (object) nameof (PerspectiveOperationsTool), (object) "Rename", (object) request.PerspectiveName, (object) request.NewPerspectiveName);
      if (!perspectiveOperationResult.Success && !string.IsNullOrEmpty(perspectiveOperationResult.Message))
        this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (PerspectiveOperationsTool), (object) "Rename", (object) perspectiveOperationResult.Message);
      PerspectiveOperationResponse operationResponse = new PerspectiveOperationResponse { Success = perspectiveOperationResult.Success };
      string str;
      if (!perspectiveOperationResult.Success)
        str = perspectiveOperationResult.Message ?? "Failed to rename perspective";
      else
        str = $"Perspective renamed from '{request.PerspectiveName}' to '{request.NewPerspectiveName}' successfully";
      operationResponse.Message = str;
      operationResponse.Operation = request.Operation;
      operationResponse.PerspectiveName = request.NewPerspectiveName;
      operationResponse.Data = (object) perspectiveOperationResult;
      List<string> stringList1;
      if (perspectiveOperationResult.Success || string.IsNullOrEmpty(perspectiveOperationResult.Message))
      {
        stringList1 = new List<string>();
      }
      else
      {
        List<string> stringList2 = new List<string>();
        stringList2.Add(perspectiveOperationResult.Message);
        stringList1 = stringList2;
      }
      operationResponse.Warnings = stringList1;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      PerspectiveOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new PerspectiveOperationResponse()
      {
        Success = false,
        Message = "Error occurred while validating request: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private PerspectiveOperationResponse HandleListPerspectiveTablesOperation(
    PerspectiveOperationRequest request)
  {
    try
    {
      List<Dictionary<string, string>> dictionaryList = PerspectiveOperations.ListPerspectiveTables(request.ConnectionName, request.PerspectiveName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Perspective={PerspectiveName}, Count={Count}", (object) nameof (PerspectiveOperationsTool), (object) "ListTables", (object) request.PerspectiveName, (object) dictionaryList.Count);
      PerspectiveOperationResponse operationResponse = new PerspectiveOperationResponse { Success = true };
      operationResponse.Message = $"Retrieved {dictionaryList.Count} perspective table(s) for perspective '{request.PerspectiveName}'";
      operationResponse.Operation = request.Operation;
      operationResponse.PerspectiveName = request.PerspectiveName;
      operationResponse.Data = (object) dictionaryList;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      PerspectiveOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new PerspectiveOperationResponse()
      {
        Success = false,
        Message = "Error occurred while validating request: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private PerspectiveOperationResponse HandleGetPerspectiveTableOperation(
    PerspectiveOperationRequest request)
  {
    try
    {
      PerspectiveTableGet perspectiveTable = PerspectiveOperations.GetPerspectiveTable(request.ConnectionName, request.PerspectiveName, request.TableName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Perspective={PerspectiveName}, Table={TableName}", (object) nameof (PerspectiveOperationsTool), (object) "GetTable", (object) request.PerspectiveName, (object) request.TableName);
      PerspectiveOperationResponse perspectiveTableOperation = new PerspectiveOperationResponse { Success = true };
      perspectiveTableOperation.Message = $"Retrieved perspective table '{request.TableName}' from perspective '{request.PerspectiveName}' successfully";
      perspectiveTableOperation.Operation = request.Operation;
      perspectiveTableOperation.PerspectiveName = request.PerspectiveName;
      perspectiveTableOperation.Data = (object) perspectiveTable;
      return perspectiveTableOperation;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      PerspectiveOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new PerspectiveOperationResponse()
      {
        Success = false,
        Message = "Error occurred while validating request: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private PerspectiveOperationResponse HandleAddTableToPerspectiveOperation(
    PerspectiveOperationRequest request)
  {
    try
    {
      PerspectiveOperationResult perspective = PerspectiveOperations.AddTableToPerspective(request.ConnectionName, request.PerspectiveName, request.TableCreateDefinition);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Perspective={PerspectiveName}, Table={TableName}", (object) nameof (PerspectiveOperationsTool), (object) "AddTable", (object) request.PerspectiveName, (object) request.TableCreateDefinition.TableName);
      if (!perspective.Success && !string.IsNullOrEmpty(perspective.Message))
        this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (PerspectiveOperationsTool), (object) "AddTable", (object) perspective.Message);
      PerspectiveOperationResponse perspectiveOperation = new PerspectiveOperationResponse { Success = perspective.Success };
      string str;
      if (!perspective.Success)
        str = perspective.Message ?? "Failed to add table to perspective";
      else
        str = $"Table '{request.TableCreateDefinition.TableName}' added to perspective '{request.PerspectiveName}' successfully";
      perspectiveOperation.Message = str;
      perspectiveOperation.Operation = request.Operation;
      perspectiveOperation.PerspectiveName = request.PerspectiveName;
      perspectiveOperation.Data = (object) perspective;
      List<string> stringList1;
      if (perspective.Success || string.IsNullOrEmpty(perspective.Message))
      {
        stringList1 = new List<string>();
      }
      else
      {
        List<string> stringList2 = new List<string>();
        stringList2.Add(perspective.Message);
        stringList1 = stringList2;
      }
      perspectiveOperation.Warnings = stringList1;
      return perspectiveOperation;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      PerspectiveOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new PerspectiveOperationResponse()
      {
        Success = false,
        Message = "Error occurred while adding table to perspective: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private PerspectiveOperationResponse HandleRemoveTableFromPerspectiveOperation(
    PerspectiveOperationRequest request)
  {
    try
    {
      PerspectiveOperationResult perspectiveOperationResult = PerspectiveOperations.RemoveTableFromPerspective(request.ConnectionName, request.PerspectiveName, request.TableName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Perspective={PerspectiveName}, Table={TableName}", (object) nameof (PerspectiveOperationsTool), (object) "RemoveTable", (object) request.PerspectiveName, (object) request.TableName);
      if (!perspectiveOperationResult.Success && !string.IsNullOrEmpty(perspectiveOperationResult.Message))
        this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (PerspectiveOperationsTool), (object) "RemoveTable", (object) perspectiveOperationResult.Message);
      PerspectiveOperationResponse operationResponse = new PerspectiveOperationResponse { Success = perspectiveOperationResult.Success };
      string str;
      if (!perspectiveOperationResult.Success)
        str = perspectiveOperationResult.Message ?? "Failed to remove table from perspective";
      else
        str = $"Table '{request.TableName}' removed from perspective '{request.PerspectiveName}' successfully";
      operationResponse.Message = str;
      operationResponse.Operation = request.Operation;
      operationResponse.PerspectiveName = request.PerspectiveName;
      operationResponse.Data = (object) perspectiveOperationResult;
      List<string> stringList1;
      if (perspectiveOperationResult.Success || string.IsNullOrEmpty(perspectiveOperationResult.Message))
      {
        stringList1 = new List<string>();
      }
      else
      {
        List<string> stringList2 = new List<string>();
        stringList2.Add(perspectiveOperationResult.Message);
        stringList1 = stringList2;
      }
      operationResponse.Warnings = stringList1;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      PerspectiveOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new PerspectiveOperationResponse()
      {
        Success = false,
        Message = "Error occurred while removing table from perspective: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private PerspectiveOperationResponse HandleUpdatePerspectiveTableOperation(
    PerspectiveOperationRequest request)
  {
    try
    {
      if (string.IsNullOrWhiteSpace(request.TableUpdateDefinition.TableName))
        throw new McpException("TableName is required for update_perspective_table operation");
      PerspectiveOperationResult perspectiveOperationResult = PerspectiveOperations.UpdatePerspectiveTable(request.ConnectionName, request.PerspectiveName, request.TableUpdateDefinition.TableName, request.TableUpdateDefinition);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Perspective={PerspectiveName}, Table={TableName}", (object) nameof (PerspectiveOperationsTool), (object) "UpdateTable", (object) request.PerspectiveName, (object) request.TableUpdateDefinition.TableName);
      if (!perspectiveOperationResult.Success && !string.IsNullOrEmpty(perspectiveOperationResult.Message))
        this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (PerspectiveOperationsTool), (object) "UpdateTable", (object) perspectiveOperationResult.Message);
      PerspectiveOperationResponse operationResponse = new PerspectiveOperationResponse { Success = perspectiveOperationResult.Success };
      string str;
      if (!perspectiveOperationResult.Success)
        str = perspectiveOperationResult.Message ?? "Failed to update perspective table";
      else
        str = $"Perspective table '{request.TableUpdateDefinition.TableName}' in perspective '{request.PerspectiveName}' updated successfully";
      operationResponse.Message = str;
      operationResponse.Operation = request.Operation;
      operationResponse.PerspectiveName = request.PerspectiveName;
      operationResponse.Data = (object) perspectiveOperationResult;
      List<string> stringList1;
      if (perspectiveOperationResult.Success || string.IsNullOrEmpty(perspectiveOperationResult.Message))
      {
        stringList1 = new List<string>();
      }
      else
      {
        List<string> stringList2 = new List<string>();
        stringList2.Add(perspectiveOperationResult.Message);
        stringList1 = stringList2;
      }
      operationResponse.Warnings = stringList1;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      PerspectiveOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new PerspectiveOperationResponse()
      {
        Success = false,
        Message = "Error occurred while updating perspective table: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private PerspectiveOperationResponse HandleListPerspectiveColumnsOperation(
    PerspectiveOperationRequest request)
  {
    try
    {
      List<Dictionary<string, string>> dictionaryList = PerspectiveOperations.ListPerspectiveColumns(request.ConnectionName, request.PerspectiveName, request.TableName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Perspective={PerspectiveName}, Table={TableName}, Count={Count}", (object) nameof (PerspectiveOperationsTool), (object) "ListColumns", (object) request.PerspectiveName, (object) request.TableName, (object) dictionaryList.Count);
      PerspectiveOperationResponse operationResponse = new PerspectiveOperationResponse { Success = true };
      operationResponse.Message = $"Retrieved {dictionaryList.Count} perspective column(s) for table '{request.TableName}' in perspective '{request.PerspectiveName}'";
      operationResponse.Operation = request.Operation;
      operationResponse.PerspectiveName = request.PerspectiveName;
      operationResponse.Data = (object) dictionaryList;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      PerspectiveOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new PerspectiveOperationResponse()
      {
        Success = false,
        Message = "Error occurred while retrieving perspective columns: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private PerspectiveOperationResponse HandleGetPerspectiveColumnOperation(
    PerspectiveOperationRequest request)
  {
    try
    {
      PerspectiveColumnGet perspectiveColumn = PerspectiveOperations.GetPerspectiveColumn(request.ConnectionName, request.PerspectiveName, request.TableName, request.ColumnName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Perspective={PerspectiveName}, Table={TableName}, Column={ColumnName}", (object) nameof (PerspectiveOperationsTool), (object) "GetColumn", (object) request.PerspectiveName, (object) request.TableName, (object) request.ColumnName);
      PerspectiveOperationResponse perspectiveColumnOperation = new PerspectiveOperationResponse { Success = true };
      perspectiveColumnOperation.Message = $"Retrieved perspective column '{request.ColumnName}' from table '{request.TableName}' in perspective '{request.PerspectiveName}' successfully";
      perspectiveColumnOperation.Operation = request.Operation;
      perspectiveColumnOperation.PerspectiveName = request.PerspectiveName;
      perspectiveColumnOperation.Data = (object) perspectiveColumn;
      return perspectiveColumnOperation;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      PerspectiveOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new PerspectiveOperationResponse()
      {
        Success = false,
        Message = "Error occurred while retrieving perspective column: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private PerspectiveOperationResponse HandleAddColumnToPerspectiveTableOperation(
    PerspectiveOperationRequest request)
  {
    try
    {
      PerspectiveOperationResult perspectiveTable = PerspectiveOperations.AddColumnToPerspectiveTable(request.ConnectionName, request.PerspectiveName, request.ColumnCreateDefinition);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Perspective={PerspectiveName}, Table={TableName}, Column={ColumnName}", (object) nameof (PerspectiveOperationsTool), (object) "AddColumn", (object) request.PerspectiveName, (object) request.ColumnCreateDefinition.TableName, (object) request.ColumnCreateDefinition.ColumnName);
      if (!perspectiveTable.Success && !string.IsNullOrEmpty(perspectiveTable.Message))
        this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (PerspectiveOperationsTool), (object) "AddColumn", (object) perspectiveTable.Message);
      PerspectiveOperationResponse perspectiveTableOperation = new PerspectiveOperationResponse { Success = perspectiveTable.Success };
      string str;
      if (!perspectiveTable.Success)
        str = perspectiveTable.Message ?? "Failed to add column to perspective table";
      else
        str = $"Column '{request.ColumnCreateDefinition.ColumnName}' added to perspective table '{request.ColumnCreateDefinition.TableName}' in perspective '{request.PerspectiveName}' successfully";
      perspectiveTableOperation.Message = str;
      perspectiveTableOperation.Operation = request.Operation;
      perspectiveTableOperation.PerspectiveName = request.PerspectiveName;
      perspectiveTableOperation.Data = (object) perspectiveTable;
      List<string> stringList1;
      if (perspectiveTable.Success || string.IsNullOrEmpty(perspectiveTable.Message))
      {
        stringList1 = new List<string>();
      }
      else
      {
        List<string> stringList2 = new List<string>();
        stringList2.Add(perspectiveTable.Message);
        stringList1 = stringList2;
      }
      perspectiveTableOperation.Warnings = stringList1;
      return perspectiveTableOperation;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      PerspectiveOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new PerspectiveOperationResponse()
      {
        Success = false,
        Message = "Error occurred while adding column to perspective table: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private PerspectiveOperationResponse HandleRemoveColumnFromPerspectiveTableOperation(
    PerspectiveOperationRequest request)
  {
    try
    {
      PerspectiveOperationResult perspectiveOperationResult = PerspectiveOperations.RemoveColumnFromPerspectiveTable(request.ConnectionName, request.PerspectiveName, request.TableName, request.ColumnName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Perspective={PerspectiveName}, Table={TableName}, Column={ColumnName}", (object) nameof (PerspectiveOperationsTool), (object) "RemoveColumn", (object) request.PerspectiveName, (object) request.TableName, (object) request.ColumnName);
      if (!perspectiveOperationResult.Success && !string.IsNullOrEmpty(perspectiveOperationResult.Message))
        this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (PerspectiveOperationsTool), (object) "RemoveColumn", (object) perspectiveOperationResult.Message);
      PerspectiveOperationResponse operationResponse = new PerspectiveOperationResponse { Success = perspectiveOperationResult.Success };
      string str;
      if (!perspectiveOperationResult.Success)
        str = perspectiveOperationResult.Message ?? "Failed to remove column from perspective table";
      else
        str = $"Column '{request.ColumnName}' removed from perspective table '{request.TableName}' in perspective '{request.PerspectiveName}' successfully";
      operationResponse.Message = str;
      operationResponse.Operation = request.Operation;
      operationResponse.PerspectiveName = request.PerspectiveName;
      operationResponse.Data = (object) perspectiveOperationResult;
      List<string> stringList1;
      if (perspectiveOperationResult.Success || string.IsNullOrEmpty(perspectiveOperationResult.Message))
      {
        stringList1 = new List<string>();
      }
      else
      {
        List<string> stringList2 = new List<string>();
        stringList2.Add(perspectiveOperationResult.Message);
        stringList1 = stringList2;
      }
      operationResponse.Warnings = stringList1;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      PerspectiveOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new PerspectiveOperationResponse()
      {
        Success = false,
        Message = "Error occurred while removing column from perspective table: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private PerspectiveOperationResponse HandleListPerspectiveMeasuresOperation(
    PerspectiveOperationRequest request)
  {
    try
    {
      List<Dictionary<string, string>> dictionaryList = PerspectiveOperations.ListPerspectiveMeasures(request.ConnectionName, request.PerspectiveName, request.TableName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Perspective={PerspectiveName}, Table={TableName}, Count={Count}", (object) nameof (PerspectiveOperationsTool), (object) "ListMeasures", (object) request.PerspectiveName, (object) request.TableName, (object) dictionaryList.Count);
      PerspectiveOperationResponse operationResponse = new PerspectiveOperationResponse { Success = true };
      operationResponse.Message = $"Retrieved {dictionaryList.Count} perspective measure(s) for table '{request.TableName}' in perspective '{request.PerspectiveName}'";
      operationResponse.Operation = request.Operation;
      operationResponse.PerspectiveName = request.PerspectiveName;
      operationResponse.Data = (object) dictionaryList;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      PerspectiveOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new PerspectiveOperationResponse()
      {
        Success = false,
        Message = "Error occurred while retrieving perspective measure: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private PerspectiveOperationResponse HandleGetPerspectiveMeasureOperation(
    PerspectiveOperationRequest request)
  {
    try
    {
      PerspectiveMeasureGet perspectiveMeasure = PerspectiveOperations.GetPerspectiveMeasure(request.ConnectionName, request.PerspectiveName, request.TableName, request.MeasureName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Perspective={PerspectiveName}, Table={TableName}, Measure={MeasureName}", (object) nameof (PerspectiveOperationsTool), (object) "GetMeasure", (object) request.PerspectiveName, (object) request.TableName, (object) request.MeasureName);
      PerspectiveOperationResponse measureOperation = new PerspectiveOperationResponse { Success = true };
      measureOperation.Message = $"Retrieved perspective measure '{request.MeasureName}' from table '{request.TableName}' in perspective '{request.PerspectiveName}' successfully";
      measureOperation.Operation = request.Operation;
      measureOperation.PerspectiveName = request.PerspectiveName;
      measureOperation.Data = (object) perspectiveMeasure;
      return measureOperation;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      PerspectiveOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new PerspectiveOperationResponse()
      {
        Success = false,
        Message = "Error occurred while retrieving perspective measure: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private PerspectiveOperationResponse HandleAddMeasureToPerspectiveTableOperation(
    PerspectiveOperationRequest request)
  {
    try
    {
      PerspectiveOperationResult perspectiveTable = PerspectiveOperations.AddMeasureToPerspectiveTable(request.ConnectionName, request.PerspectiveName, request.MeasureCreateDefinition);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Perspective={PerspectiveName}, Table={TableName}, Measure={MeasureName}", (object) nameof (PerspectiveOperationsTool), (object) "AddMeasure", (object) request.PerspectiveName, (object) request.MeasureCreateDefinition.TableName, (object) request.MeasureCreateDefinition.MeasureName);
      if (!perspectiveTable.Success && !string.IsNullOrEmpty(perspectiveTable.Message))
        this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (PerspectiveOperationsTool), (object) "AddMeasure", (object) perspectiveTable.Message);
      PerspectiveOperationResponse perspectiveTableOperation = new PerspectiveOperationResponse { Success = perspectiveTable.Success };
      string str;
      if (!perspectiveTable.Success)
        str = perspectiveTable.Message ?? "Failed to add measure to perspective table";
      else
        str = $"Measure '{request.MeasureCreateDefinition.MeasureName}' added to perspective table '{request.MeasureCreateDefinition.TableName}' in perspective '{request.PerspectiveName}' successfully";
      perspectiveTableOperation.Message = str;
      perspectiveTableOperation.Operation = request.Operation;
      perspectiveTableOperation.PerspectiveName = request.PerspectiveName;
      perspectiveTableOperation.Data = (object) perspectiveTable;
      List<string> stringList1;
      if (perspectiveTable.Success || string.IsNullOrEmpty(perspectiveTable.Message))
      {
        stringList1 = new List<string>();
      }
      else
      {
        List<string> stringList2 = new List<string>();
        stringList2.Add(perspectiveTable.Message);
        stringList1 = stringList2;
      }
      perspectiveTableOperation.Warnings = stringList1;
      return perspectiveTableOperation;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      PerspectiveOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new PerspectiveOperationResponse()
      {
        Success = false,
        Message = "Error occurred while adding measure to perspective table: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private PerspectiveOperationResponse HandleRemoveMeasureFromPerspectiveTableOperation(
    PerspectiveOperationRequest request)
  {
    try
    {
      PerspectiveOperationResult perspectiveOperationResult = PerspectiveOperations.RemoveMeasureFromPerspectiveTable(request.ConnectionName, request.PerspectiveName, request.TableName, request.MeasureName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Perspective={PerspectiveName}, Table={TableName}, Measure={MeasureName}", (object) nameof (PerspectiveOperationsTool), (object) "RemoveMeasure", (object) request.PerspectiveName, (object) request.TableName, (object) request.MeasureName);
      if (!perspectiveOperationResult.Success && !string.IsNullOrEmpty(perspectiveOperationResult.Message))
        this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (PerspectiveOperationsTool), (object) "RemoveMeasure", (object) perspectiveOperationResult.Message);
      PerspectiveOperationResponse operationResponse = new PerspectiveOperationResponse { Success = perspectiveOperationResult.Success };
      string str;
      if (!perspectiveOperationResult.Success)
        str = perspectiveOperationResult.Message ?? "Failed to remove measure from perspective table";
      else
        str = $"Measure '{request.MeasureName}' removed from perspective table '{request.TableName}' in perspective '{request.PerspectiveName}' successfully";
      operationResponse.Message = str;
      operationResponse.Operation = request.Operation;
      operationResponse.PerspectiveName = request.PerspectiveName;
      operationResponse.Data = (object) perspectiveOperationResult;
      List<string> stringList1;
      if (perspectiveOperationResult.Success || string.IsNullOrEmpty(perspectiveOperationResult.Message))
      {
        stringList1 = new List<string>();
      }
      else
      {
        List<string> stringList2 = new List<string>();
        stringList2.Add(perspectiveOperationResult.Message);
        stringList1 = stringList2;
      }
      operationResponse.Warnings = stringList1;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      PerspectiveOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new PerspectiveOperationResponse()
      {
        Success = false,
        Message = "Error occurred while removing measure from perspective table: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private PerspectiveOperationResponse HandleListPerspectiveHierarchiesOperation(
    PerspectiveOperationRequest request)
  {
    try
    {
      List<Dictionary<string, string>> dictionaryList = PerspectiveOperations.ListPerspectiveHierarchies(request.ConnectionName, request.PerspectiveName, request.TableName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Perspective={PerspectiveName}, Table={TableName}, Count={Count}", (object) nameof (PerspectiveOperationsTool), (object) "ListHierarchies", (object) request.PerspectiveName, (object) request.TableName, (object) dictionaryList.Count);
      PerspectiveOperationResponse operationResponse = new PerspectiveOperationResponse { Success = true };
      operationResponse.Message = $"Retrieved {dictionaryList.Count} perspective hierarchy(ies) for table '{request.TableName}' in perspective '{request.PerspectiveName}'";
      operationResponse.Operation = request.Operation;
      operationResponse.PerspectiveName = request.PerspectiveName;
      operationResponse.Data = (object) dictionaryList;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      PerspectiveOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new PerspectiveOperationResponse()
      {
        Success = false,
        Message = "Error occurred while retrieving perspective hierarchy: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private PerspectiveOperationResponse HandleGetPerspectiveHierarchyOperation(
    PerspectiveOperationRequest request)
  {
    try
    {
      PerspectiveHierarchyGet perspectiveHierarchy = PerspectiveOperations.GetPerspectiveHierarchy(request.ConnectionName, request.PerspectiveName, request.TableName, request.HierarchyName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Perspective={PerspectiveName}, Table={TableName}, Hierarchy={HierarchyName}", (object) nameof (PerspectiveOperationsTool), (object) "GetHierarchy", (object) request.PerspectiveName, (object) request.TableName, (object) request.HierarchyName);
      PerspectiveOperationResponse hierarchyOperation = new PerspectiveOperationResponse { Success = true };
      hierarchyOperation.Message = $"Retrieved perspective hierarchy '{request.HierarchyName}' from table '{request.TableName}' in perspective '{request.PerspectiveName}' successfully";
      hierarchyOperation.Operation = request.Operation;
      hierarchyOperation.PerspectiveName = request.PerspectiveName;
      hierarchyOperation.Data = (object) perspectiveHierarchy;
      return hierarchyOperation;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      PerspectiveOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new PerspectiveOperationResponse()
      {
        Success = false,
        Message = "Error occurred while retrieving perspective hierarchy: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private PerspectiveOperationResponse HandleAddHierarchyToPerspectiveTableOperation(
    PerspectiveOperationRequest request)
  {
    try
    {
      PerspectiveOperationResult perspectiveTable = PerspectiveOperations.AddHierarchyToPerspectiveTable(request.ConnectionName, request.PerspectiveName, request.HierarchyCreateDefinition);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Perspective={PerspectiveName}, Table={TableName}, Hierarchy={HierarchyName}", (object) nameof (PerspectiveOperationsTool), (object) "AddHierarchy", (object) request.PerspectiveName, (object) request.HierarchyCreateDefinition.TableName, (object) request.HierarchyCreateDefinition.HierarchyName);
      if (!perspectiveTable.Success && !string.IsNullOrEmpty(perspectiveTable.Message))
        this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (PerspectiveOperationsTool), (object) "AddHierarchy", (object) perspectiveTable.Message);
      PerspectiveOperationResponse perspectiveTableOperation = new PerspectiveOperationResponse { Success = perspectiveTable.Success };
      string str;
      if (!perspectiveTable.Success)
        str = perspectiveTable.Message ?? "Failed to add hierarchy to perspective table";
      else
        str = $"Hierarchy '{request.HierarchyCreateDefinition.HierarchyName}' added to perspective table '{request.HierarchyCreateDefinition.TableName}' in perspective '{request.PerspectiveName}' successfully";
      perspectiveTableOperation.Message = str;
      perspectiveTableOperation.Operation = request.Operation;
      perspectiveTableOperation.PerspectiveName = request.PerspectiveName;
      perspectiveTableOperation.Data = (object) perspectiveTable;
      List<string> stringList1;
      if (perspectiveTable.Success || string.IsNullOrEmpty(perspectiveTable.Message))
      {
        stringList1 = new List<string>();
      }
      else
      {
        List<string> stringList2 = new List<string>();
        stringList2.Add(perspectiveTable.Message);
        stringList1 = stringList2;
      }
      perspectiveTableOperation.Warnings = stringList1;
      return perspectiveTableOperation;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      PerspectiveOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new PerspectiveOperationResponse()
      {
        Success = false,
        Message = "Error occurred while adding hierarchy to perspective table: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private PerspectiveOperationResponse HandleRemoveHierarchyFromPerspectiveTableOperation(
    PerspectiveOperationRequest request)
  {
    try
    {
      PerspectiveOperationResult perspectiveOperationResult = PerspectiveOperations.RemoveHierarchyFromPerspectiveTable(request.ConnectionName, request.PerspectiveName, request.TableName, request.HierarchyName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Perspective={PerspectiveName}, Table={TableName}, Hierarchy={HierarchyName}", (object) nameof (PerspectiveOperationsTool), (object) "RemoveHierarchy", (object) request.PerspectiveName, (object) request.TableName, (object) request.HierarchyName);
      if (!perspectiveOperationResult.Success && !string.IsNullOrEmpty(perspectiveOperationResult.Message))
        this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (PerspectiveOperationsTool), (object) "RemoveHierarchy", (object) perspectiveOperationResult.Message);
      PerspectiveOperationResponse operationResponse = new PerspectiveOperationResponse { Success = perspectiveOperationResult.Success };
      string str;
      if (!perspectiveOperationResult.Success)
        str = perspectiveOperationResult.Message ?? "Failed to remove hierarchy from perspective table";
      else
        str = $"Hierarchy '{request.HierarchyName}' removed from perspective table '{request.TableName}' in perspective '{request.PerspectiveName}' successfully";
      operationResponse.Message = str;
      operationResponse.Operation = request.Operation;
      operationResponse.PerspectiveName = request.PerspectiveName;
      operationResponse.Data = (object) perspectiveOperationResult;
      List<string> stringList1;
      if (perspectiveOperationResult.Success || string.IsNullOrEmpty(perspectiveOperationResult.Message))
      {
        stringList1 = new List<string>();
      }
      else
      {
        List<string> stringList2 = new List<string>();
        stringList2.Add(perspectiveOperationResult.Message);
        stringList1 = stringList2;
      }
      operationResponse.Warnings = stringList1;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      PerspectiveOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new PerspectiveOperationResponse()
      {
        Success = false,
        Message = "Error occurred while removing hierarchy from perspective table: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private PerspectiveOperationResponse HandleExportTMDLOperation(PerspectiveOperationRequest request)
  {
    try
    {
      string str = PerspectiveOperations.ExportTMDL(request.ConnectionName, request.PerspectiveName, (ExportTmdl) request.TmdlExportOptions);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Perspective={PerspectiveName}", (object) nameof (PerspectiveOperationsTool), (object) "ExportTMDL", (object) request.PerspectiveName);
      return new PerspectiveOperationResponse()
      {
        Success = true,
        Message = $"TMDL exported for perspective '{request.PerspectiveName}'",
        Operation = request.Operation,
        PerspectiveName = request.PerspectiveName,
        Data = (object) str
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      PerspectiveOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new PerspectiveOperationResponse()
      {
        Success = false,
        Message = "Failed to export TMDL for perspective: " + ex.Message,
        Operation = request.Operation,
        PerspectiveName = request.PerspectiveName,
        Help = (object) operationMetadata
      };
    }
  }

  private PerspectiveOperationResponse HandleHelpOperation(
    PerspectiveOperationRequest request,
    string[] operations)
  {
    this._logger.LogInformation("{ToolName}.{Operation} completed: Operations={OperationCount}", (object) nameof (PerspectiveOperationsTool), (object) "Help", (object) operations.Length);
    return new PerspectiveOperationResponse()
    {
      Success = true,
      Message = "Help information for perspective operations",
      Operation = request.Operation,
      Help = (object) new
      {
        ToolName = "perspective_operations",
        Description = "Perform operations on semantic model perspectives, perspective tables, perspective columns, perspective measures, and perspective hierarchies.",
        SupportedOperations = operations,
        Examples = Enumerable.Where<KeyValuePair<string, OperationMetadata>>((IEnumerable<KeyValuePair<string, OperationMetadata>>) PerspectiveOperationsTool.toolMetadata.Operations, (Func<KeyValuePair<string, OperationMetadata>, bool>) (p => Enumerable.Contains<string>((IEnumerable<string>) operations, p.Key, (IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase))),
        Notes = new string[10]
        {
          "For CREATE, UPDATE, DELETE, and RENAME operations, the 'perspective_name' parameter is required.",
          "For operations that require a table name, the 'table_name' parameter is required.",
          "For operations that require a column name, the 'column_name' parameter is required.",
          "For operations that require a measure name, the 'measure_name' parameter is required.",
          "For operations that require a hierarchy name, the 'hierarchy_name' parameter is required.",
          "For operations that require a new perspective name, the 'new_perspective_name' parameter is required.",
          "For operations that require a new table name, the 'new_table_name' parameter is required.",
          "For operations that require a new column name, the 'new_column_name' parameter is required.",
          "For operations that require a new measure name, the 'new_measure_name' parameter is required.",
          "For operations that require a new hierarchy name, the 'new_hierarchy_name' parameter is required."
        }
      }
    };
  }

  private bool ValidateRequest(string operation, PerspectiveOperationRequest request)
  {
    OperationMetadata operationMetadata;
    if (!PerspectiveOperationsTool.toolMetadata.Operations.TryGetValue(operation, out operationMetadata))
      return true;
    JsonObject requestDict = JsonSerializer.SerializeToNode<PerspectiveOperationRequest>(request) as JsonObject;
    List<string> list1 = Enumerable.ToList<string>(Enumerable.Where<string>((IEnumerable<string>) operationMetadata.RequiredParams, (p => requestDict != null && requestDict[p] == null)));
    List<string> list2 = Enumerable.ToList<string>(Enumerable.Where<string>((IEnumerable<string>) operationMetadata.ForbiddenParams, (p => requestDict != null && requestDict[p] != null)));
    if (Enumerable.Any<string>((IEnumerable<string>) list1))
      throw new McpException($"Missing required parameters needed for {operation} operation: {string.Join(", ", (IEnumerable<string>) list1)}");
    if (Enumerable.Any<string>((IEnumerable<string>) list2))
      throw new McpException($"Forbidden parameters not allowed for {operation} operation: {string.Join(", ", (IEnumerable<string>) list2)}");
    return true;
  }

  static PerspectiveOperationsTool()
  {
    ToolMetadata toolMetadata1 = new ToolMetadata();
    ToolMetadata toolMetadata2 = toolMetadata1;
    Dictionary<string, OperationMetadata> dictionary1 = new Dictionary<string, OperationMetadata>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    Dictionary<string, OperationMetadata> dictionary2 = dictionary1;
    OperationMetadata operationMetadata1 = new OperationMetadata { Description = "List all perspectives in the model with summary information. \r\nMandatory properties: None. \r\nOptional: None." };
    List<string> stringList1 = new List<string>();
    stringList1.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"List\"\r\n    }\r\n}");
    operationMetadata1.ExampleRequests = stringList1;
    dictionary2["List"] = operationMetadata1;
    Dictionary<string, OperationMetadata> dictionary3 = dictionary1;
    OperationMetadata operationMetadata2 = new OperationMetadata { RequiredParams = new string[1]
    {
      "PerspectiveName"
    } };
    operationMetadata2.Description = "Get detailed information about a specific perspective including all tables, columns, measures, and hierarchies. \r\nMandatory properties: PerspectiveName. \r\nOptional: None.";
    OperationMetadata operationMetadata3 = operationMetadata2;
    List<string> stringList2 = new List<string>();
    stringList2.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Get\",\r\n        \"PerspectiveName\": \"SalesPerspective\"\r\n    }\r\n}");
    operationMetadata3.ExampleRequests = stringList2;
    OperationMetadata operationMetadata4 = operationMetadata2;
    dictionary3["Get"] = operationMetadata4;
    Dictionary<string, OperationMetadata> dictionary4 = dictionary1;
    OperationMetadata operationMetadata5 = new OperationMetadata { RequiredParams = new string[1]
    {
      "CreateDefinition"
    } };
    operationMetadata5.Description = "Create a new perspective in the model. \r\nMandatory properties: CreateDefinition (with Name). \r\nOptional: Description, Annotations, ExtendedProperties.";
    OperationMetadata operationMetadata6 = operationMetadata5;
    List<string> stringList3 = new List<string>();
    stringList3.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Create\",\r\n        \"CreateDefinition\": { \r\n            \"Name\": \"SalesPerspective\" \r\n        }\r\n    }\r\n}");
    operationMetadata6.ExampleRequests = stringList3;
    OperationMetadata operationMetadata7 = operationMetadata5;
    dictionary4["Create"] = operationMetadata7;
    Dictionary<string, OperationMetadata> dictionary5 = dictionary1;
    OperationMetadata operationMetadata8 = new OperationMetadata { RequiredParams = new string[2]
    {
      "PerspectiveName",
      "UpdateDefinition"
    } };
    operationMetadata8.Description = "Update an existing perspective's properties. Note: perspective names cannot be changed; use Rename operation instead. \r\nMandatory properties: PerspectiveName, UpdateDefinition. \r\nOptional: Description, Annotations, ExtendedProperties.";
    OperationMetadata operationMetadata9 = operationMetadata8;
    List<string> stringList4 = new List<string>();
    stringList4.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Update\",\r\n        \"PerspectiveName\": \"SalesPerspective\",\r\n        \"UpdateDefinition\": { \r\n            \"Name\": \"SalesPerspective\",\r\n            \"Description\": \"Sales perspective for the sales team\" \r\n        }\r\n    }\r\n}");
    operationMetadata9.ExampleRequests = stringList4;
    OperationMetadata operationMetadata10 = operationMetadata8;
    dictionary5["Update"] = operationMetadata10;
    Dictionary<string, OperationMetadata> dictionary6 = dictionary1;
    OperationMetadata operationMetadata11 = new OperationMetadata { RequiredParams = new string[1]
    {
      "PerspectiveName"
    } };
    operationMetadata11.Description = "Delete a perspective from the model. \r\nMandatory properties: PerspectiveName. \r\nOptional: None.";
    OperationMetadata operationMetadata12 = operationMetadata11;
    List<string> stringList5 = new List<string>();
    stringList5.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Delete\",\r\n        \"PerspectiveName\": \"ObsoletePerspective\"\r\n    }\r\n}");
    operationMetadata12.ExampleRequests = stringList5;
    OperationMetadata operationMetadata13 = operationMetadata11;
    dictionary6["Delete"] = operationMetadata13;
    Dictionary<string, OperationMetadata> dictionary7 = dictionary1;
    OperationMetadata operationMetadata14 = new OperationMetadata { RequiredParams = new string[2]
    {
      "PerspectiveName",
      "NewPerspectiveName"
    } };
    operationMetadata14.Description = "Rename an existing perspective to a new name. \r\nMandatory properties: PerspectiveName, NewPerspectiveName. \r\nOptional: None.";
    OperationMetadata operationMetadata15 = operationMetadata14;
    List<string> stringList6 = new List<string>();
    stringList6.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Rename\",\r\n        \"PerspectiveName\": \"Sales\",\r\n        \"NewPerspectiveName\": \"SalesNew\"\r\n    }\r\n}");
    operationMetadata15.ExampleRequests = stringList6;
    OperationMetadata operationMetadata16 = operationMetadata14;
    dictionary7["Rename"] = operationMetadata16;
    Dictionary<string, OperationMetadata> dictionary8 = dictionary1;
    OperationMetadata operationMetadata17 = new OperationMetadata { RequiredParams = new string[1]
    {
      "PerspectiveName"
    } };
    operationMetadata17.Description = "List all tables included in a specific perspective with summary information. \r\nMandatory properties: PerspectiveName. \r\nOptional: None.";
    OperationMetadata operationMetadata18 = operationMetadata17;
    List<string> stringList7 = new List<string>();
    stringList7.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"ListTables\",\r\n        \"PerspectiveName\": \"Sales\"\r\n    }\r\n}");
    operationMetadata18.ExampleRequests = stringList7;
    OperationMetadata operationMetadata19 = operationMetadata17;
    dictionary8["ListTables"] = operationMetadata19;
    Dictionary<string, OperationMetadata> dictionary9 = dictionary1;
    OperationMetadata operationMetadata20 = new OperationMetadata { RequiredParams = new string[2]
    {
      "PerspectiveName",
      "TableName"
    } };
    operationMetadata20.Description = "Get detailed information about a specific table in a perspective including all columns, measures, and hierarchies. \r\nMandatory properties: PerspectiveName, TableName. \r\nOptional: None.";
    OperationMetadata operationMetadata21 = operationMetadata20;
    List<string> stringList8 = new List<string>();
    stringList8.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"GetTable\",\r\n        \"PerspectiveName\": \"Sales\",\r\n        \"TableName\": \"FactSales\"\r\n    }\r\n}");
    operationMetadata21.ExampleRequests = stringList8;
    OperationMetadata operationMetadata22 = operationMetadata20;
    dictionary9["GetTable"] = operationMetadata22;
    Dictionary<string, OperationMetadata> dictionary10 = dictionary1;
    OperationMetadata operationMetadata23 = new OperationMetadata { RequiredParams = new string[2]
    {
      "PerspectiveName",
      "TableCreateDefinition"
    } };
    operationMetadata23.Description = "Add a model table to a perspective. \r\nMandatory properties: PerspectiveName, TableCreateDefinition (with TableName). \r\nOptional: IncludeAll, Annotations, ExtendedProperties.";
    OperationMetadata operationMetadata24 = operationMetadata23;
    List<string> stringList9 = new List<string>();
    stringList9.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"AddTable\",\r\n        \"PerspectiveName\": \"Sales\",\r\n        \"TableCreateDefinition\": { \r\n            \"TableName\": \"FactSales\" \r\n        }\r\n    }\r\n}");
    operationMetadata24.ExampleRequests = stringList9;
    OperationMetadata operationMetadata25 = operationMetadata23;
    dictionary10["AddTable"] = operationMetadata25;
    Dictionary<string, OperationMetadata> dictionary11 = dictionary1;
    OperationMetadata operationMetadata26 = new OperationMetadata { RequiredParams = new string[2]
    {
      "PerspectiveName",
      "TableName"
    } };
    operationMetadata26.Description = "Remove a table from a perspective. \r\nMandatory properties: PerspectiveName, TableName. \r\nOptional: None.";
    OperationMetadata operationMetadata27 = operationMetadata26;
    List<string> stringList10 = new List<string>();
    stringList10.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"RemoveTable\",\r\n        \"PerspectiveName\": \"Sales\",\r\n        \"TableName\": \"FactSales\"\r\n    }\r\n}");
    operationMetadata27.ExampleRequests = stringList10;
    OperationMetadata operationMetadata28 = operationMetadata26;
    dictionary11["RemoveTable"] = operationMetadata28;
    Dictionary<string, OperationMetadata> dictionary12 = dictionary1;
    OperationMetadata operationMetadata29 = new OperationMetadata { RequiredParams = new string[2]
    {
      "PerspectiveName",
      "TableUpdateDefinition"
    } };
    operationMetadata29.Description = "Update properties of a table in a perspective. Note: table names cannot be changed; remove and re-add the table instead. \r\nMandatory properties: PerspectiveName, TableUpdateDefinition (with TableName). \r\nOptional: IncludeAll, Annotations, ExtendedProperties.";
    OperationMetadata operationMetadata30 = operationMetadata29;
    List<string> stringList11 = new List<string>();
    stringList11.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"UpdateTable\",\r\n        \"PerspectiveName\": \"Sales\",\r\n        \"TableUpdateDefinition\": { \r\n            \"TableName\": \"FactSales\" \r\n        }\r\n    }\r\n}");
    operationMetadata30.ExampleRequests = stringList11;
    OperationMetadata operationMetadata31 = operationMetadata29;
    dictionary12["UpdateTable"] = operationMetadata31;
    Dictionary<string, OperationMetadata> dictionary13 = dictionary1;
    OperationMetadata operationMetadata32 = new OperationMetadata { RequiredParams = new string[2]
    {
      "PerspectiveName",
      "TableName"
    } };
    operationMetadata32.Description = "List all columns included in a specific perspective table. \r\nMandatory properties: PerspectiveName, TableName. \r\nOptional: None.";
    OperationMetadata operationMetadata33 = operationMetadata32;
    List<string> stringList12 = new List<string>();
    stringList12.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"ListColumns\",\r\n        \"PerspectiveName\": \"Sales\",\r\n        \"TableName\": \"FactSales\"\r\n    }\r\n}");
    operationMetadata33.ExampleRequests = stringList12;
    OperationMetadata operationMetadata34 = operationMetadata32;
    dictionary13["ListColumns"] = operationMetadata34;
    Dictionary<string, OperationMetadata> dictionary14 = dictionary1;
    OperationMetadata operationMetadata35 = new OperationMetadata { RequiredParams = new string[3]
    {
      "PerspectiveName",
      "TableName",
      "ColumnName"
    } };
    operationMetadata35.Description = "Get detailed information about a specific column in a perspective table. \r\nMandatory properties: PerspectiveName, TableName, ColumnName. \r\nOptional: None.";
    OperationMetadata operationMetadata36 = operationMetadata35;
    List<string> stringList13 = new List<string>();
    stringList13.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"GetColumn\",\r\n        \"PerspectiveName\": \"Sales\",\r\n        \"TableName\": \"FactSales\",\r\n        \"ColumnName\": \"Amount\"\r\n    }\r\n}");
    operationMetadata36.ExampleRequests = stringList13;
    OperationMetadata operationMetadata37 = operationMetadata35;
    dictionary14["GetColumn"] = operationMetadata37;
    Dictionary<string, OperationMetadata> dictionary15 = dictionary1;
    OperationMetadata operationMetadata38 = new OperationMetadata { RequiredParams = new string[2]
    {
      "PerspectiveName",
      "ColumnCreateDefinition"
    } };
    operationMetadata38.Description = "Add a column to a perspective table. \r\nMandatory properties: PerspectiveName, ColumnCreateDefinition (with TableName, ColumnName). \r\nOptional: Annotations, ExtendedProperties.";
    OperationMetadata operationMetadata39 = operationMetadata38;
    List<string> stringList14 = new List<string>();
    stringList14.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"AddColumn\",\r\n        \"PerspectiveName\": \"Sales\",\r\n        \"ColumnCreateDefinition\": { \r\n            \"TableName\": \"FactSales\", \r\n            \"ColumnName\": \"Amount\" \r\n        }\r\n    }\r\n}");
    operationMetadata39.ExampleRequests = stringList14;
    OperationMetadata operationMetadata40 = operationMetadata38;
    dictionary15["AddColumn"] = operationMetadata40;
    Dictionary<string, OperationMetadata> dictionary16 = dictionary1;
    OperationMetadata operationMetadata41 = new OperationMetadata { RequiredParams = new string[3]
    {
      "PerspectiveName",
      "TableName",
      "ColumnName"
    } };
    operationMetadata41.Description = "Remove a column from a perspective table. \r\nMandatory properties: PerspectiveName, TableName, ColumnName. \r\nOptional: None.";
    OperationMetadata operationMetadata42 = operationMetadata41;
    List<string> stringList15 = new List<string>();
    stringList15.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"RemoveColumn\",\r\n        \"PerspectiveName\": \"Sales\",\r\n        \"TableName\": \"FactSales\",\r\n        \"ColumnName\": \"Amount\"\r\n    }\r\n}");
    operationMetadata42.ExampleRequests = stringList15;
    OperationMetadata operationMetadata43 = operationMetadata41;
    dictionary16["RemoveColumn"] = operationMetadata43;
    Dictionary<string, OperationMetadata> dictionary17 = dictionary1;
    OperationMetadata operationMetadata44 = new OperationMetadata { RequiredParams = new string[2]
    {
      "PerspectiveName",
      "TableName"
    } };
    operationMetadata44.Description = "List all measures included in a specific perspective table. \r\nMandatory properties: PerspectiveName, TableName. \r\nOptional: None.";
    OperationMetadata operationMetadata45 = operationMetadata44;
    List<string> stringList16 = new List<string>();
    stringList16.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"ListMeasures\",\r\n        \"PerspectiveName\": \"Sales\",\r\n        \"TableName\": \"FactSales\"\r\n    }\r\n}");
    operationMetadata45.ExampleRequests = stringList16;
    OperationMetadata operationMetadata46 = operationMetadata44;
    dictionary17["ListMeasures"] = operationMetadata46;
    Dictionary<string, OperationMetadata> dictionary18 = dictionary1;
    OperationMetadata operationMetadata47 = new OperationMetadata { RequiredParams = new string[3]
    {
      "PerspectiveName",
      "TableName",
      "MeasureName"
    } };
    operationMetadata47.Description = "Get detailed information about a specific measure in a perspective table. \r\nMandatory properties: PerspectiveName, TableName, MeasureName. \r\nOptional: None.";
    OperationMetadata operationMetadata48 = operationMetadata47;
    List<string> stringList17 = new List<string>();
    stringList17.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"GetMeasure\",\r\n        \"PerspectiveName\": \"Sales\",\r\n        \"TableName\": \"FactSales\",\r\n        \"MeasureName\": \"TotalSales\"\r\n    }\r\n}");
    operationMetadata48.ExampleRequests = stringList17;
    OperationMetadata operationMetadata49 = operationMetadata47;
    dictionary18["GetMeasure"] = operationMetadata49;
    Dictionary<string, OperationMetadata> dictionary19 = dictionary1;
    OperationMetadata operationMetadata50 = new OperationMetadata { RequiredParams = new string[2]
    {
      "PerspectiveName",
      "MeasureCreateDefinition"
    } };
    operationMetadata50.Description = "Add a measure to a perspective table. \r\nMandatory properties: PerspectiveName, MeasureCreateDefinition (with TableName, MeasureName). \r\nOptional: Annotations, ExtendedProperties.";
    OperationMetadata operationMetadata51 = operationMetadata50;
    List<string> stringList18 = new List<string>();
    stringList18.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"AddMeasure\",\r\n        \"PerspectiveName\": \"Sales\",\r\n        \"MeasureCreateDefinition\": { \r\n            \"TableName\": \"FactSales\", \r\n            \"MeasureName\": \"TotalSales\" \r\n        }\r\n    }\r\n}");
    operationMetadata51.ExampleRequests = stringList18;
    OperationMetadata operationMetadata52 = operationMetadata50;
    dictionary19["AddMeasure"] = operationMetadata52;
    Dictionary<string, OperationMetadata> dictionary20 = dictionary1;
    OperationMetadata operationMetadata53 = new OperationMetadata { RequiredParams = new string[3]
    {
      "PerspectiveName",
      "TableName",
      "MeasureName"
    } };
    operationMetadata53.Description = "Remove a measure from a perspective table. \r\nMandatory properties: PerspectiveName, TableName, MeasureName. \r\nOptional: None.";
    OperationMetadata operationMetadata54 = operationMetadata53;
    List<string> stringList19 = new List<string>();
    stringList19.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"RemoveMeasure\",\r\n        \"PerspectiveName\": \"Sales\",\r\n        \"TableName\": \"FactSales\",\r\n        \"MeasureName\": \"TotalSales\"\r\n    }\r\n}");
    operationMetadata54.ExampleRequests = stringList19;
    OperationMetadata operationMetadata55 = operationMetadata53;
    dictionary20["RemoveMeasure"] = operationMetadata55;
    Dictionary<string, OperationMetadata> dictionary21 = dictionary1;
    OperationMetadata operationMetadata56 = new OperationMetadata { RequiredParams = new string[2]
    {
      "PerspectiveName",
      "TableName"
    } };
    operationMetadata56.Description = "List all hierarchies included in a specific perspective table. \r\nMandatory properties: PerspectiveName, TableName. \r\nOptional: None.";
    OperationMetadata operationMetadata57 = operationMetadata56;
    List<string> stringList20 = new List<string>();
    stringList20.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"ListHierarchies\",\r\n        \"PerspectiveName\": \"Sales\",\r\n        \"TableName\": \"FactSales\"\r\n    }\r\n}");
    operationMetadata57.ExampleRequests = stringList20;
    OperationMetadata operationMetadata58 = operationMetadata56;
    dictionary21["ListHierarchies"] = operationMetadata58;
    Dictionary<string, OperationMetadata> dictionary22 = dictionary1;
    OperationMetadata operationMetadata59 = new OperationMetadata { RequiredParams = new string[3]
    {
      "PerspectiveName",
      "TableName",
      "HierarchyName"
    } };
    operationMetadata59.Description = "Get detailed information about a specific hierarchy in a perspective table. \r\nMandatory properties: PerspectiveName, TableName, HierarchyName. \r\nOptional: None.";
    OperationMetadata operationMetadata60 = operationMetadata59;
    List<string> stringList21 = new List<string>();
    stringList21.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"GetHierarchy\",\r\n        \"PerspectiveName\": \"Sales\",\r\n        \"TableName\": \"FactSales\",\r\n        \"HierarchyName\": \"Geography\"\r\n    }\r\n}");
    operationMetadata60.ExampleRequests = stringList21;
    OperationMetadata operationMetadata61 = operationMetadata59;
    dictionary22["GetHierarchy"] = operationMetadata61;
    Dictionary<string, OperationMetadata> dictionary23 = dictionary1;
    OperationMetadata operationMetadata62 = new OperationMetadata { RequiredParams = new string[2]
    {
      "PerspectiveName",
      "HierarchyCreateDefinition"
    } };
    operationMetadata62.Description = "Add a hierarchy to a perspective table. \r\nMandatory properties: PerspectiveName, HierarchyCreateDefinition (with TableName, HierarchyName). \r\nOptional: Annotations, ExtendedProperties.";
    OperationMetadata operationMetadata63 = operationMetadata62;
    List<string> stringList22 = new List<string>();
    stringList22.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"AddHierarchy\",\r\n        \"PerspectiveName\": \"Sales\",\r\n        \"HierarchyCreateDefinition\": { \r\n            \"TableName\": \"FactSales\", \r\n            \"HierarchyName\": \"Geography\" \r\n        }\r\n    }\r\n}");
    operationMetadata63.ExampleRequests = stringList22;
    OperationMetadata operationMetadata64 = operationMetadata62;
    dictionary23["AddHierarchy"] = operationMetadata64;
    Dictionary<string, OperationMetadata> dictionary24 = dictionary1;
    OperationMetadata operationMetadata65 = new OperationMetadata { RequiredParams = new string[3]
    {
      "PerspectiveName",
      "TableName",
      "HierarchyName"
    } };
    operationMetadata65.Description = "Remove a hierarchy from a perspective table. \r\nMandatory properties: PerspectiveName, TableName, HierarchyName. \r\nOptional: None.";
    OperationMetadata operationMetadata66 = operationMetadata65;
    List<string> stringList23 = new List<string>();
    stringList23.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"RemoveHierarchy\",\r\n        \"PerspectiveName\": \"Sales\",\r\n        \"TableName\": \"FactSales\",\r\n        \"HierarchyName\": \"Geography\"\r\n    }\r\n}");
    operationMetadata66.ExampleRequests = stringList23;
    OperationMetadata operationMetadata67 = operationMetadata65;
    dictionary24["RemoveHierarchy"] = operationMetadata67;
    Dictionary<string, OperationMetadata> dictionary25 = dictionary1;
    OperationMetadata operationMetadata68 = new OperationMetadata { RequiredParams = new string[1]
    {
      "PerspectiveName"
    } };
    operationMetadata68.Description = "Export perspective definition to TMDL (Tabular Model Definition Language) format. \r\nMandatory properties: PerspectiveName. \r\nOptional: TmdlExportOptions.";
    OperationMetadata operationMetadata69 = operationMetadata68;
    List<string> stringList24 = new List<string>();
    stringList24.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"ExportTMDL\",\r\n        \"PerspectiveName\": \"Sales\"\r\n    }\r\n}");
    operationMetadata69.ExampleRequests = stringList24;
    OperationMetadata operationMetadata70 = operationMetadata68;
    dictionary25["ExportTMDL"] = operationMetadata70;
    Dictionary<string, OperationMetadata> dictionary26 = dictionary1;
    OperationMetadata operationMetadata71 = new OperationMetadata { Description = "Describe the perspective_operations tool and its available operations. \r\nMandatory properties: None. \r\nOptional: None." };
    List<string> stringList25 = new List<string>();
    stringList25.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"help\"\r\n    }\r\n}");
    operationMetadata71.ExampleRequests = stringList25;
    dictionary26["Help"] = operationMetadata71;
    Dictionary<string, OperationMetadata> dictionary27 = dictionary1;
    toolMetadata2.Operations = dictionary27;
    PerspectiveOperationsTool.toolMetadata = toolMetadata1;
  }
}
