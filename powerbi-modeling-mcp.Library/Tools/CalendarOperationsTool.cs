// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Tools.CalendarOperationsTool
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

#nullable enable
namespace PowerBIModelingMCP.Library.Tools;

[McpServerToolType]
public class CalendarOperationsTool
{
  private readonly ILogger<CalendarOperationsTool> _logger;
  public static readonly ToolMetadata toolMetadata;

  public CalendarOperationsTool(ILogger<CalendarOperationsTool> logger) => this._logger = logger;

  [McpServerTool(Name = "calendar_operations")]
  [Description("Perform operations on semantic model calendars and their column groups. Supported operations: Help, Create, Update, Delete, Get, List, Rename, ExportTMDL, CreateColumnGroup, UpdateColumnGroup, DeleteColumnGroup, GetColumnGroup, ListColumnGroups. Use the Operation parameter to specify which operation to perform. Calendar objects define logical calendars as part of DAX Time-Intelligence support and are only supported when compatibility level >= 1701.")]
  public CalendarOperationResponse ExecuteCalendarOperation(
    McpServer mcpServer,
    CalendarOperationRequest request)
  {
    this._logger.LogDebug("Executing {ToolName}.{Operation}: Calendar={CalendarName}, Table={TableName}, Connection={ConnectionName}", (object) nameof (CalendarOperationsTool), (object) request.Operation, (object) request.CalendarName, (object) request.TableName, (object) (request.ConnectionName ?? "(last used)"));
    try
    {
      string[] strArray1 = new string[13]
      {
        "CREATE",
        "UPDATE",
        "DELETE",
        "GET",
        "LIST",
        "RENAME",
        "EXPORTTMDL",
        "CREATECOLUMNGROUP",
        "UPDATECOLUMNGROUP",
        "DELETECOLUMNGROUP",
        "GETCOLUMNGROUP",
        "LISTCOLUMNGROUPS",
        "HELP"
      };
      string[] strArray2 = new string[7]
      {
        "CREATE",
        "UPDATE",
        "DELETE",
        "RENAME",
        "CREATECOLUMNGROUP",
        "UPDATECOLUMNGROUP",
        "DELETECOLUMNGROUP"
      };
      if (!Enumerable.Contains<string>((IEnumerable<string>) strArray1, request.Operation.ToUpperInvariant()))
      {
        this._logger.LogWarning("Invalid operation '{Operation}' requested for {ToolName}. Valid operations: {ValidOperations}", (object) request.Operation, (object) nameof (CalendarOperationsTool), (object) string.Join(", ", strArray1));
        return CalendarOperationResponse.Forbidden(request.Operation, $"Invalid operation: {request.Operation}. Supported operations: {string.Join(", ", strArray1)}");
      }
      if (!this.ValidateRequest(request.Operation, request))
        throw new McpException($"Invalid request for {request.Operation} operation.");
      if (Enumerable.Contains<string>((IEnumerable<string>) strArray2, request.Operation.ToUpperInvariant()))
      {
        WriteOperationResult writeOperationResult = WriteGuard.ExecuteWriteOperationWithGuards(mcpServer, request.ConnectionName, request.Operation);
        if (!writeOperationResult.Success)
        {
          this._logger.LogWarning("{ToolName}.{Operation} blocked by write guard: {Reason}", (object) nameof (CalendarOperationsTool), (object) request.Operation, (object) writeOperationResult.Message);
          return new CalendarOperationResponse()
          {
            Success = false,
            Message = writeOperationResult.Message,
            Operation = request.Operation,
            CalendarName = request.CalendarName,
            TableName = request.TableName
          };
        }
      }
      bool allowed = WriteGuard.IsWriteAllowed("").allowed;
      string upperInvariant = request.Operation.ToUpperInvariant();
      CalendarOperationResponse operationResponse;
      if (upperInvariant != null)
      {
        switch (upperInvariant.Length)
        {
          case 3:
            if ((upperInvariant == "GET"))
            {
              operationResponse = this.HandleGetOperation(request);
              goto label_40;
            }
            break;
          case 4:
            switch (upperInvariant[0])
            {
              case 'H':
                if ((upperInvariant == "HELP"))
                {
                  operationResponse = this.HandleHelpOperation(request, allowed ? strArray1 : Enumerable.ToArray<string>(Enumerable.Except<string>((IEnumerable<string>) strArray1, (IEnumerable<string>) strArray2)));
                  goto label_40;
                }
                break;
              case 'L':
                if ((upperInvariant == "LIST"))
                {
                  operationResponse = this.HandleListOperation(request);
                  goto label_40;
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
                  goto label_40;
                }
                break;
              case 'D':
                if ((upperInvariant == "DELETE"))
                {
                  operationResponse = this.HandleDeleteOperation(request);
                  goto label_40;
                }
                break;
              case 'R':
                if ((upperInvariant == "RENAME"))
                {
                  operationResponse = this.HandleRenameOperation(request);
                  goto label_40;
                }
                break;
              case 'U':
                if ((upperInvariant == "UPDATE"))
                {
                  operationResponse = this.HandleUpdateOperation(request);
                  goto label_40;
                }
                break;
            }
            break;
          case 10:
            if ((upperInvariant == "EXPORTTMDL"))
            {
              operationResponse = this.HandleExportTmdlOperation(request);
              goto label_40;
            }
            break;
          case 14:
            if ((upperInvariant == "GETCOLUMNGROUP"))
            {
              operationResponse = this.HandleGetColumnGroupOperation(request);
              goto label_40;
            }
            break;
          case 16 /*0x10*/:
            if ((upperInvariant == "LISTCOLUMNGROUPS"))
            {
              operationResponse = this.HandleListColumnGroupsOperation(request);
              goto label_40;
            }
            break;
          case 17:
            switch (upperInvariant[0])
            {
              case 'C':
                if ((upperInvariant == "CREATECOLUMNGROUP"))
                {
                  operationResponse = this.HandleCreateColumnGroupOperation(request);
                  goto label_40;
                }
                break;
              case 'D':
                if ((upperInvariant == "DELETECOLUMNGROUP"))
                {
                  operationResponse = this.HandleDeleteColumnGroupOperation(request);
                  goto label_40;
                }
                break;
              case 'U':
                if ((upperInvariant == "UPDATECOLUMNGROUP"))
                {
                  operationResponse = this.HandleUpdateColumnGroupOperation(request);
                  goto label_40;
                }
                break;
            }
            break;
        }
      }
      operationResponse = CalendarOperationResponse.Forbidden(request.Operation, $"Operation {request.Operation} is not implemented");
label_40:
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Error executing {ToolName}.{Operation}: {ErrorMessage}", (object) nameof (CalendarOperationsTool), (object) request.Operation, (object) ex.Message);
      return new CalendarOperationResponse()
      {
        Success = false,
        Message = "Error executing calendar operation: " + ex.Message,
        Operation = request.Operation,
        CalendarName = request.CalendarName,
        TableName = request.TableName
      };
    }
  }

  private CalendarOperationResponse HandleCreateOperation(CalendarOperationRequest request)
  {
    try
    {
      if (request.CreateDefinition == null)
        throw new McpException("CreateDefinition is required for Create operation");
      if (!string.IsNullOrEmpty(request.CalendarName) && (request.CreateDefinition.Name != request.CalendarName))
        throw new McpException($"Calendar name mismatch: Request specifies '{request.CalendarName}' but CreateDefinition specifies '{request.CreateDefinition.Name}'");
      CalendarOperationResult calendar = CalendarOperations.CreateCalendar(request.ConnectionName, request.CreateDefinition);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Calendar={CalendarName}, Table={TableName}", (object) nameof (CalendarOperationsTool), (object) "Create", (object) request.CreateDefinition.Name, (object) request.CreateDefinition.TableName);
      CalendarOperationResponse operation = new CalendarOperationResponse { Success = true };
      operation.Message = $"Calendar '{request.CreateDefinition.Name}' created successfully in table '{request.CreateDefinition.TableName}'";
      operation.Operation = request.Operation;
      operation.CalendarName = request.CreateDefinition.Name;
      operation.TableName = request.CreateDefinition.TableName;
      operation.OperationResult = (object) calendar;
      return operation;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      CalendarOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new CalendarOperationResponse()
      {
        Success = false,
        Message = "Error creating calendar: " + ex.Message,
        Operation = request.Operation,
        CalendarName = request.CalendarName,
        TableName = request.TableName,
        Help = (object) operationMetadata
      };
    }
  }

  private CalendarOperationResponse HandleUpdateOperation(CalendarOperationRequest request)
  {
    try
    {
      if (request.UpdateDefinition == null)
        throw new McpException("UpdateDefinition is required for Update operation");
      if (string.IsNullOrWhiteSpace(request.UpdateDefinition.Name))
        throw new McpException("Calendar name must be specified in UpdateDefinition.Name");
      CalendarOperationResult calendarOperationResult = CalendarOperations.UpdateCalendar(request.ConnectionName, request.UpdateDefinition, request.TableName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Calendar={CalendarName}, Table={TableName}", (object) nameof (CalendarOperationsTool), (object) "Update", (object) request.UpdateDefinition.Name, (object) request.UpdateDefinition.TableName);
      return new CalendarOperationResponse()
      {
        Success = true,
        Message = $"Calendar '{request.UpdateDefinition.Name}' updated successfully",
        Operation = request.Operation,
        CalendarName = request.UpdateDefinition.Name,
        TableName = request.UpdateDefinition.TableName,
        OperationResult = (object) calendarOperationResult
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      CalendarOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new CalendarOperationResponse()
      {
        Success = false,
        Message = "Error updating calendar: " + ex.Message,
        Operation = request.Operation,
        CalendarName = request.CalendarName,
        TableName = request.TableName,
        Help = (object) operationMetadata
      };
    }
  }

  private CalendarOperationResponse HandleDeleteOperation(CalendarOperationRequest request)
  {
    try
    {
      if (string.IsNullOrWhiteSpace(request.CalendarName))
        throw new McpException("CalendarName is required for Delete operation");
      CalendarOperations.DeleteCalendar(request.ConnectionName, request.CalendarName, request.TableName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Calendar={CalendarName}, Table={TableName}", (object) nameof (CalendarOperationsTool), (object) "Delete", (object) request.CalendarName, (object) request.TableName);
      return new CalendarOperationResponse()
      {
        Success = true,
        Message = $"Calendar '{request.CalendarName}' deleted successfully",
        Operation = request.Operation,
        CalendarName = request.CalendarName,
        TableName = request.TableName
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      CalendarOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new CalendarOperationResponse()
      {
        Success = false,
        Message = "Error deleting calendar: " + ex.Message,
        Operation = request.Operation,
        CalendarName = request.CalendarName,
        TableName = request.TableName,
        Help = (object) operationMetadata
      };
    }
  }

  private CalendarOperationResponse HandleGetOperation(CalendarOperationRequest request)
  {
    try
    {
      if (string.IsNullOrWhiteSpace(request.CalendarName))
        throw new McpException("CalendarName is required for Get operation");
      CalendarGet calendar = CalendarOperations.GetCalendar(request.ConnectionName, request.CalendarName, request.TableName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Calendar={CalendarName}, Table={TableName}", (object) nameof (CalendarOperationsTool), (object) "Get", (object) request.CalendarName, (object) calendar.TableName);
      return new CalendarOperationResponse()
      {
        Success = true,
        Message = $"Calendar '{request.CalendarName}' retrieved successfully",
        Operation = request.Operation,
        CalendarName = request.CalendarName,
        TableName = calendar.TableName,
        Data = (object) calendar
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      CalendarOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new CalendarOperationResponse()
      {
        Success = false,
        Message = "Error retrieving calendar: " + ex.Message,
        Operation = request.Operation,
        CalendarName = request.CalendarName,
        TableName = request.TableName,
        Help = (object) operationMetadata
      };
    }
  }

  private CalendarOperationResponse HandleListOperation(CalendarOperationRequest request)
  {
    try
    {
      if (string.IsNullOrWhiteSpace(request.TableName))
        throw new McpException("TableName is required for List operation");
      List<CalendarList> calendarListList = CalendarOperations.ListCalendars(request.ConnectionName, request.TableName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Table={TableName}, Count={Count}", (object) nameof (CalendarOperationsTool), (object) "List", (object) request.TableName, (object) calendarListList.Count);
      CalendarOperationResponse operationResponse = new CalendarOperationResponse { Success = true };
      operationResponse.Message = $"Listed {calendarListList.Count} calendar(s) in table '{request.TableName}'";
      operationResponse.Operation = request.Operation;
      operationResponse.TableName = request.TableName;
      operationResponse.Data = (object) calendarListList;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      CalendarOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new CalendarOperationResponse()
      {
        Success = false,
        Message = "Error listing calendars: " + ex.Message,
        Operation = request.Operation,
        TableName = request.TableName,
        Help = (object) operationMetadata
      };
    }
  }

  private CalendarOperationResponse HandleRenameOperation(CalendarOperationRequest request)
  {
    try
    {
      if (request.RenameDefinition == null)
        throw new McpException("RenameDefinition is required for Rename operation");
      CalendarOperations.RenameCalendar(request.ConnectionName, request.RenameDefinition.CurrentName, request.RenameDefinition.NewName, request.TableName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Table={TableName}, From={OldName}, To={NewName}", (object) nameof (CalendarOperationsTool), (object) "Rename", (object) request.TableName, (object) request.RenameDefinition.CurrentName, (object) request.RenameDefinition.NewName);
      CalendarOperationResponse operationResponse = new CalendarOperationResponse { Success = true };
      operationResponse.Message = $"Calendar renamed from '{request.RenameDefinition.CurrentName}' to '{request.RenameDefinition.NewName}' successfully";
      operationResponse.Operation = request.Operation;
      operationResponse.CalendarName = request.RenameDefinition.NewName;
      operationResponse.TableName = request.TableName;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      CalendarOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new CalendarOperationResponse()
      {
        Success = false,
        Message = "Error renaming calendar: " + ex.Message,
        Operation = request.Operation,
        CalendarName = request.CalendarName,
        TableName = request.TableName,
        Help = (object) operationMetadata
      };
    }
  }

  private CalendarOperationResponse HandleExportTmdlOperation(CalendarOperationRequest request)
  {
    try
    {
      if (string.IsNullOrWhiteSpace(request.CalendarName))
        throw new McpException("CalendarName is required for ExportTMDL operation");
      string str = CalendarOperations.ExportTMDL(request.ConnectionName, request.CalendarName, request.TableName, (ExportTmdl) request.TmdlExportOptions);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Calendar={CalendarName}, Table={TableName}", (object) nameof (CalendarOperationsTool), (object) "ExportTMDL", (object) request.CalendarName, (object) request.TableName);
      return new CalendarOperationResponse()
      {
        Success = true,
        Message = $"Calendar '{request.CalendarName}' exported to TMDL successfully",
        Operation = request.Operation,
        CalendarName = request.CalendarName,
        TableName = request.TableName,
        Data = (object) str
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      CalendarOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new CalendarOperationResponse()
      {
        Success = false,
        Message = "Error exporting calendar to TMDL: " + ex.Message,
        Operation = request.Operation,
        CalendarName = request.CalendarName,
        TableName = request.TableName,
        Help = (object) operationMetadata
      };
    }
  }

  private CalendarOperationResponse HandleCreateColumnGroupOperation(
    CalendarOperationRequest request)
  {
    try
    {
      if (string.IsNullOrWhiteSpace(request.CalendarName))
        throw new McpException("CalendarName is required for CreateColumnGroup operation");
      if (request.ColumnGroupCreateDefinition == null)
        throw new McpException("ColumnGroupCreateDefinition is required for CreateColumnGroup operation");
      CalendarColumnGroupOperationResult columnGroup = CalendarOperations.CreateColumnGroup(request.ConnectionName, request.CalendarName, request.TableName, request.ColumnGroupCreateDefinition);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Calendar={CalendarName}, Table={TableName}, ColumnGroupType={ColumnGroupType}", (object) nameof (CalendarOperationsTool), (object) "CreateColumnGroup", (object) request.CalendarName, (object) request.TableName, (object) request.ColumnGroupCreateDefinition.GroupType);
      return new CalendarOperationResponse()
      {
        Success = true,
        Message = $"Column group created successfully in calendar '{request.CalendarName}'",
        Operation = request.Operation,
        CalendarName = request.CalendarName,
        TableName = request.TableName,
        ColumnGroupType = request.ColumnGroupCreateDefinition.GroupType,
        OperationResult = (object) columnGroup
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      CalendarOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new CalendarOperationResponse()
      {
        Success = false,
        Message = "Error creating column group: " + ex.Message,
        Operation = request.Operation,
        CalendarName = request.CalendarName,
        TableName = request.TableName,
        Help = (object) operationMetadata
      };
    }
  }

  private CalendarOperationResponse HandleUpdateColumnGroupOperation(
    CalendarOperationRequest request)
  {
    try
    {
      if (string.IsNullOrWhiteSpace(request.CalendarName))
        throw new McpException("CalendarName is required for UpdateColumnGroup operation");
      int? nullable = request.ColumnGroupUpdateDefinition != null ? request.ColumnGroupIndex : throw new McpException("ColumnGroupUpdateDefinition is required for UpdateColumnGroup operation");
      if (!nullable.HasValue)
        throw new McpException("ColumnGroupIndex is required for UpdateColumnGroup operation");
      string connectionName = request.ConnectionName;
      string calendarName = request.CalendarName;
      string tableName = request.TableName;
      nullable = request.ColumnGroupIndex;
      int columnGroupIndex = nullable.Value;
      CalendarColumnGroupUpdate updateDefinition = request.ColumnGroupUpdateDefinition;
      CalendarColumnGroupOperationResult groupOperationResult = CalendarOperations.UpdateColumnGroup(connectionName, calendarName, tableName, columnGroupIndex, updateDefinition);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Calendar={CalendarName}, Table={TableName}, Index={Index}, ColumnGroupType={ColumnGroupType}", (object) nameof (CalendarOperationsTool), (object) "UpdateColumnGroup", (object) request.CalendarName, (object) request.TableName, (object) request.ColumnGroupIndex, (object) request.ColumnGroupUpdateDefinition.GroupType);
      CalendarOperationResponse operationResponse = new CalendarOperationResponse { Success = true };
      operationResponse.Message = $"Column group at index {request.ColumnGroupIndex} updated successfully in calendar '{request.CalendarName}'";
      operationResponse.Operation = request.Operation;
      operationResponse.CalendarName = request.CalendarName;
      operationResponse.TableName = request.TableName;
      operationResponse.ColumnGroupType = request.ColumnGroupUpdateDefinition.GroupType;
      operationResponse.OperationResult = (object) groupOperationResult;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      CalendarOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new CalendarOperationResponse()
      {
        Success = false,
        Message = "Error updating column group: " + ex.Message,
        Operation = request.Operation,
        CalendarName = request.CalendarName,
        TableName = request.TableName,
        Help = (object) operationMetadata
      };
    }
  }

  private CalendarOperationResponse HandleDeleteColumnGroupOperation(
    CalendarOperationRequest request)
  {
    try
    {
      int? nullable = !string.IsNullOrWhiteSpace(request.CalendarName) ? request.ColumnGroupIndex : throw new McpException("CalendarName is required for DeleteColumnGroup operation");
      if (!nullable.HasValue)
        throw new McpException("ColumnGroupIndex is required for DeleteColumnGroup operation");
      string connectionName = request.ConnectionName;
      string calendarName = request.CalendarName;
      string tableName = request.TableName;
      nullable = request.ColumnGroupIndex;
      int columnGroupIndex = nullable.Value;
      CalendarOperations.DeleteColumnGroup(connectionName, calendarName, tableName, columnGroupIndex);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Calendar={CalendarName}, Table={TableName}, Index={Index}", (object) nameof (CalendarOperationsTool), (object) "DeleteColumnGroup", (object) request.CalendarName, (object) request.TableName, (object) request.ColumnGroupIndex);
      CalendarOperationResponse operationResponse = new CalendarOperationResponse { Success = true };
      operationResponse.Message = $"Column group at index {request.ColumnGroupIndex} deleted successfully from calendar '{request.CalendarName}'";
      operationResponse.Operation = request.Operation;
      operationResponse.CalendarName = request.CalendarName;
      operationResponse.TableName = request.TableName;
      operationResponse.ColumnGroupType = request.ColumnGroupType;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      CalendarOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new CalendarOperationResponse()
      {
        Success = false,
        Message = "Error deleting column group: " + ex.Message,
        Operation = request.Operation,
        CalendarName = request.CalendarName,
        TableName = request.TableName,
        Help = (object) operationMetadata
      };
    }
  }

  private CalendarOperationResponse HandleGetColumnGroupOperation(CalendarOperationRequest request)
  {
    try
    {
      int? nullable = !string.IsNullOrWhiteSpace(request.CalendarName) ? request.ColumnGroupIndex : throw new McpException("CalendarName is required for GetColumnGroup operation");
      if (!nullable.HasValue)
        throw new McpException("ColumnGroupIndex is required for GetColumnGroup operation");
      string connectionName = request.ConnectionName;
      string calendarName = request.CalendarName;
      string tableName = request.TableName;
      nullable = request.ColumnGroupIndex;
      int columnGroupIndex = nullable.Value;
      CalendarColumnGroupGet columnGroup = CalendarOperations.GetColumnGroup(connectionName, calendarName, tableName, columnGroupIndex);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Calendar={CalendarName}, Table={TableName}, Index={Index}, ColumnGroupType={ColumnGroupType}", (object) nameof (CalendarOperationsTool), (object) "GetColumnGroup", (object) request.CalendarName, (object) request.TableName, (object) request.ColumnGroupIndex, (object) columnGroup.GroupType);
      CalendarOperationResponse columnGroupOperation = new CalendarOperationResponse { Success = true };
      columnGroupOperation.Message = $"Column group at index {request.ColumnGroupIndex} retrieved successfully from calendar '{request.CalendarName}'";
      columnGroupOperation.Operation = request.Operation;
      columnGroupOperation.CalendarName = request.CalendarName;
      columnGroupOperation.TableName = request.TableName;
      columnGroupOperation.ColumnGroupType = columnGroup.GroupType;
      columnGroupOperation.Data = (object) columnGroup;
      return columnGroupOperation;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      CalendarOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new CalendarOperationResponse()
      {
        Success = false,
        Message = "Error retrieving column group: " + ex.Message,
        Operation = request.Operation,
        CalendarName = request.CalendarName,
        TableName = request.TableName,
        Help = (object) operationMetadata
      };
    }
  }

  private CalendarOperationResponse HandleListColumnGroupsOperation(CalendarOperationRequest request)
  {
    try
    {
      if (string.IsNullOrWhiteSpace(request.CalendarName))
        throw new McpException("CalendarName is required for ListColumnGroups operation");
      List<CalendarColumnGroupInfo> calendarColumnGroupInfoList = CalendarOperations.ListColumnGroups(request.ConnectionName, request.CalendarName, request.TableName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Calendar={CalendarName}, Table={TableName}, Count={Count}", (object) nameof (CalendarOperationsTool), (object) "ListColumnGroups", (object) request.CalendarName, (object) request.TableName, (object) calendarColumnGroupInfoList.Count);
      CalendarOperationResponse operationResponse = new CalendarOperationResponse { Success = true };
      operationResponse.Message = $"Listed {calendarColumnGroupInfoList.Count} column group(s) in calendar '{request.CalendarName}'";
      operationResponse.Operation = request.Operation;
      operationResponse.CalendarName = request.CalendarName;
      operationResponse.TableName = request.TableName;
      operationResponse.Data = (object) calendarColumnGroupInfoList;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      CalendarOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new CalendarOperationResponse()
      {
        Success = false,
        Message = "Error listing column groups: " + ex.Message,
        Operation = request.Operation,
        CalendarName = request.CalendarName,
        TableName = request.TableName,
        Help = (object) operationMetadata
      };
    }
  }

  private CalendarOperationResponse HandleHelpOperation(
    CalendarOperationRequest request,
    string[] operations)
  {
    this._logger.LogInformation("{ToolName}.{Operation} completed: Operations={OperationCount}", (object) nameof (CalendarOperationsTool), (object) "Help", (object) operations.Length);
    var data = new
    {
      SupportedOperations = operations,
      OperationDetails = Enumerable.ToDictionary<KeyValuePair<string, OperationMetadata>, string, OperationMetadata>(Enumerable.Where<KeyValuePair<string, OperationMetadata>>((IEnumerable<KeyValuePair<string, OperationMetadata>>) CalendarOperationsTool.toolMetadata.Operations, (Func<KeyValuePair<string, OperationMetadata>, bool>) (op => Enumerable.Contains<string>((IEnumerable<string>) operations, op.Key, (IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase))), (Func<KeyValuePair<string, OperationMetadata>, string>) (op => op.Key), (Func<KeyValuePair<string, OperationMetadata>, OperationMetadata>) (op => op.Value), (IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase),
      CalendarOverview = new
      {
        Description = "Calendar objects define logical calendars as part of DAX Time-Intelligence support. They are only supported when the compatibility level of the database is at 1701 or above.",
        SupportedColumnGroupTypes = new[]
        {
          new
          {
            Type = "TimeRelated",
            Description = "Simple collection of related time columns"
          },
          new
          {
            Type = "TimeUnitAssociation",
            Description = "Association with specific time unit and optional primary column"
          }
        },
        TimeUnits = new string[7]
        {
          "Years",
          "Quarters",
          "Months",
          "Days",
          "Hours",
          "Minutes",
          "Seconds"
        }
      },
      ExampleWorkflow = new string[4]
      {
        "1. Create a calendar: Use 'Create' operation with table and calendar name",
        "2. Add column groups: Use 'CreateColumnGroup' to add TimeRelated or TimeUnitAssociation groups",
        "3. Manage calendar: Use 'Update', 'Get', 'List', or 'Delete' operations as needed",
        "4. Export: Use 'ExportTMDL' to get TMDL representation"
      }
    };
    return new CalendarOperationResponse()
    {
      Success = true,
      Message = "Calendar operations help information",
      Operation = request.Operation,
      Help = (object) data
    };
  }

  private bool ValidateRequest(string operation, CalendarOperationRequest request)
  {
    string upperInvariant = operation.ToUpperInvariant();
    bool flag;
    if (upperInvariant != null)
    {
      switch (upperInvariant.Length)
      {
        case 3:
          if ((upperInvariant == "GET"))
          {
            flag = !string.IsNullOrWhiteSpace(request.CalendarName);
            goto label_32;
          }
          break;
        case 4:
          switch (upperInvariant[0])
          {
            case 'H':
              if ((upperInvariant == "HELP"))
              {
                flag = true;
                goto label_32;
              }
              break;
            case 'L':
              if ((upperInvariant == "LIST"))
              {
                flag = true;
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
                flag = request.CreateDefinition != null;
                goto label_32;
              }
              break;
            case 'D':
              if ((upperInvariant == "DELETE"))
              {
                flag = !string.IsNullOrWhiteSpace(request.CalendarName);
                goto label_32;
              }
              break;
            case 'R':
              if ((upperInvariant == "RENAME"))
              {
                flag = request.RenameDefinition != null;
                goto label_32;
              }
              break;
            case 'U':
              if ((upperInvariant == "UPDATE"))
              {
                flag = request.UpdateDefinition != null;
                goto label_32;
              }
              break;
          }
          break;
        case 10:
          if ((upperInvariant == "EXPORTTMDL"))
          {
            flag = !string.IsNullOrWhiteSpace(request.CalendarName);
            goto label_32;
          }
          break;
        case 14:
          if ((upperInvariant == "GETCOLUMNGROUP"))
          {
            flag = !string.IsNullOrWhiteSpace(request.CalendarName) && request.ColumnGroupIndex.HasValue;
            goto label_32;
          }
          break;
        case 16 /*0x10*/:
          if ((upperInvariant == "LISTCOLUMNGROUPS"))
          {
            flag = !string.IsNullOrWhiteSpace(request.CalendarName);
            goto label_32;
          }
          break;
        case 17:
          switch (upperInvariant[0])
          {
            case 'C':
              if ((upperInvariant == "CREATECOLUMNGROUP"))
              {
                flag = !string.IsNullOrWhiteSpace(request.CalendarName) && request.ColumnGroupCreateDefinition != null;
                goto label_32;
              }
              break;
            case 'D':
              if ((upperInvariant == "DELETECOLUMNGROUP"))
              {
                flag = !string.IsNullOrWhiteSpace(request.CalendarName) && request.ColumnGroupIndex.HasValue;
                goto label_32;
              }
              break;
            case 'U':
              if ((upperInvariant == "UPDATECOLUMNGROUP"))
              {
                flag = !string.IsNullOrWhiteSpace(request.CalendarName) && request.ColumnGroupUpdateDefinition != null && request.ColumnGroupIndex.HasValue;
                goto label_32;
              }
              break;
          }
          break;
      }
    }
    flag = false;
label_32:
    return flag;
  }

  static CalendarOperationsTool()
  {
    ToolMetadata toolMetadata1 = new ToolMetadata();
    ToolMetadata toolMetadata2 = toolMetadata1;
    Dictionary<string, OperationMetadata> dictionary1 = new Dictionary<string, OperationMetadata>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    Dictionary<string, OperationMetadata> dictionary2 = dictionary1;
    OperationMetadata operationMetadata1 = new OperationMetadata { RequiredParams = new string[1]
    {
      "CreateDefinition"
    } };
    operationMetadata1.Description = "Create a new calendar with optional initial column groups.\r\nMandatory properties: CreateDefinition (with Name, TableName).\r\nOptional: Description, LineageTag, SourceLineageTag, CalendarColumnGroups.";
    OperationMetadata operationMetadata2 = operationMetadata1;
    List<string> stringList1 = new List<string>();
    stringList1.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Create\",\r\n        \"CreateDefinition\": {\r\n            \"Name\": \"FiscalCalendar\",\r\n            \"TableName\": \"DimDate\",\r\n            \"Description\": \"Fiscal calendar for financial reporting\",\r\n            \"CalendarColumnGroups\": [\r\n                {\r\n                    \"GroupType\": \"TimeUnitAssociation\",\r\n                    \"TimeUnitAssociation\": {\r\n                        \"CalendarName\": \"FiscalCalendar\",\r\n                        \"GroupType\": \"TimeUnitAssociation\",\r\n                        \"TimeUnit\": \"Years\",\r\n                        \"PrimaryColumnName\": \"FiscalYear\",\r\n                        \"AssociatedColumns\": [\"FiscalYear\", \"FiscalYearName\"]\r\n                    }\r\n                }\r\n            ]\r\n        }\r\n    }\r\n}");
    stringList1.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Create\",\r\n        \"CreateDefinition\": {\r\n            \"Name\": \"StandardCalendar\",\r\n            \"TableName\": \"DateTable\",\r\n            \"Description\": \"Standard Gregorian calendar\"\r\n        }\r\n    }\r\n}");
    operationMetadata2.ExampleRequests = stringList1;
    OperationMetadata operationMetadata3 = operationMetadata1;
    dictionary2["Create"] = operationMetadata3;
    Dictionary<string, OperationMetadata> dictionary3 = dictionary1;
    OperationMetadata operationMetadata4 = new OperationMetadata { RequiredParams = new string[1]
    {
      "UpdateDefinition"
    } };
    operationMetadata4.Description = "Update properties of an existing calendar. Calendar names cannot be changed using this operation (use Rename instead).\r\nMandatory properties: UpdateDefinition (with Name, TableName).\r\nOptional: Description, LineageTag, SourceLineageTag.";
    OperationMetadata operationMetadata5 = operationMetadata4;
    List<string> stringList2 = new List<string>();
    stringList2.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Update\",\r\n        \"UpdateDefinition\": {\r\n            \"Name\": \"FiscalCalendar\",\r\n            \"TableName\": \"DimDate\",\r\n            \"Description\": \"Updated fiscal calendar for financial reporting\"\r\n        }\r\n    }\r\n}");
    operationMetadata5.ExampleRequests = stringList2;
    OperationMetadata operationMetadata6 = operationMetadata4;
    dictionary3["Update"] = operationMetadata6;
    Dictionary<string, OperationMetadata> dictionary4 = dictionary1;
    OperationMetadata operationMetadata7 = new OperationMetadata { RequiredParams = new string[1]
    {
      "CalendarName"
    } };
    operationMetadata7.Description = "Delete a calendar and all its column groups.\r\nMandatory properties: CalendarName.\r\nOptional: TableName (as search hint).";
    OperationMetadata operationMetadata8 = operationMetadata7;
    List<string> stringList3 = new List<string>();
    stringList3.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Delete\",\r\n        \"CalendarName\": \"OldFiscalCalendar\",\r\n        \"TableName\": \"DimDate\"\r\n    }\r\n}");
    operationMetadata8.ExampleRequests = stringList3;
    OperationMetadata operationMetadata9 = operationMetadata7;
    dictionary4["Delete"] = operationMetadata9;
    Dictionary<string, OperationMetadata> dictionary5 = dictionary1;
    OperationMetadata operationMetadata10 = new OperationMetadata { RequiredParams = new string[1]
    {
      "CalendarName"
    } };
    operationMetadata10.Description = "Retrieve detailed information about a calendar including all its column groups.\r\nMandatory properties: CalendarName.\r\nOptional: TableName (as search hint).";
    OperationMetadata operationMetadata11 = operationMetadata10;
    List<string> stringList4 = new List<string>();
    stringList4.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Get\",\r\n        \"CalendarName\": \"FiscalCalendar\",\r\n        \"TableName\": \"DimDate\"\r\n    }\r\n}");
    operationMetadata11.ExampleRequests = stringList4;
    OperationMetadata operationMetadata12 = operationMetadata10;
    dictionary5["Get"] = operationMetadata12;
    Dictionary<string, OperationMetadata> dictionary6 = dictionary1;
    OperationMetadata operationMetadata13 = new OperationMetadata { RequiredParams = new string[1]
    {
      "TableName"
    } };
    operationMetadata13.Description = "List all calendars in the specified table.\r\nMandatory properties: TableName.\r\nOptional: None.";
    OperationMetadata operationMetadata14 = operationMetadata13;
    List<string> stringList5 = new List<string>();
    stringList5.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"List\",\r\n        \"TableName\": \"DimDate\"\r\n    }\r\n}");
    operationMetadata14.ExampleRequests = stringList5;
    OperationMetadata operationMetadata15 = operationMetadata13;
    dictionary6["List"] = operationMetadata15;
    Dictionary<string, OperationMetadata> dictionary7 = dictionary1;
    OperationMetadata operationMetadata16 = new OperationMetadata { RequiredParams = new string[1]
    {
      "RenameDefinition"
    } };
    operationMetadata16.Description = "Rename a calendar.\r\nMandatory properties: RenameDefinition (with CurrentName, NewName).\r\nOptional: TableName (as search hint).";
    OperationMetadata operationMetadata17 = operationMetadata16;
    List<string> stringList6 = new List<string>();
    stringList6.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Rename\",\r\n        \"RenameDefinition\": {\r\n            \"CurrentName\": \"OldCalendarName\",\r\n            \"NewName\": \"NewCalendarName\"\r\n        }\r\n    }\r\n}");
    operationMetadata17.ExampleRequests = stringList6;
    OperationMetadata operationMetadata18 = operationMetadata16;
    dictionary7["Rename"] = operationMetadata18;
    Dictionary<string, OperationMetadata> dictionary8 = dictionary1;
    OperationMetadata operationMetadata19 = new OperationMetadata { RequiredParams = new string[1]
    {
      "CalendarName"
    } };
    operationMetadata19.Description = "Export a calendar to TMDL format.\r\nMandatory properties: CalendarName.\r\nOptional: TableName (as search hint), TmdlExportOptions.";
    OperationMetadata operationMetadata20 = operationMetadata19;
    List<string> stringList7 = new List<string>();
    stringList7.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"ExportTMDL\",\r\n        \"CalendarName\": \"FiscalCalendar\",\r\n        \"TableName\": \"DimDate\"\r\n    }\r\n}");
    operationMetadata20.ExampleRequests = stringList7;
    OperationMetadata operationMetadata21 = operationMetadata19;
    dictionary8["ExportTMDL"] = operationMetadata21;
    Dictionary<string, OperationMetadata> dictionary9 = dictionary1;
    OperationMetadata operationMetadata22 = new OperationMetadata { RequiredParams = new string[2]
    {
      "CalendarName",
      "ColumnGroupCreateDefinition"
    } };
    operationMetadata22.Description = "Create a new column group within a calendar.\r\nMandatory properties: CalendarName, ColumnGroupCreateDefinition (with GroupType, and either TimeRelatedGroup with Columns OR TimeUnitAssociation with TimeUnit).\r\nOptional: TableName (as search hint).";
    OperationMetadata operationMetadata23 = operationMetadata22;
    List<string> stringList8 = new List<string>();
    stringList8.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"CreateColumnGroup\",\r\n        \"CalendarName\": \"FiscalCalendar\",\r\n        \"ColumnGroupCreateDefinition\": {\r\n            \"GroupType\": \"TimeUnitAssociation\",\r\n            \"TimeUnitAssociation\": {\r\n                \"CalendarName\": \"FiscalCalendar\",\r\n                \"GroupType\": \"TimeUnitAssociation\",\r\n                \"TimeUnit\": \"Months\",\r\n                \"PrimaryColumnName\": \"FiscalMonth\",\r\n                \"AssociatedColumns\": [\"FiscalMonth\", \"FiscalMonthName\"]\r\n            }\r\n        }\r\n    }\r\n}");
    stringList8.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"CreateColumnGroup\",\r\n        \"CalendarName\": \"FiscalCalendar\",\r\n        \"ColumnGroupCreateDefinition\": {\r\n            \"GroupType\": \"TimeRelated\",\r\n            \"TimeRelatedGroup\": {\r\n                \"CalendarName\": \"FiscalCalendar\",\r\n                \"GroupType\": \"TimeRelated\",\r\n                \"Columns\": [\"DateKey\", \"FullDateAlternateKey\", \"Date\"]\r\n            }\r\n        }\r\n    }\r\n}");
    operationMetadata23.ExampleRequests = stringList8;
    OperationMetadata operationMetadata24 = operationMetadata22;
    dictionary9["CreateColumnGroup"] = operationMetadata24;
    Dictionary<string, OperationMetadata> dictionary10 = dictionary1;
    OperationMetadata operationMetadata25 = new OperationMetadata { RequiredParams = new string[3]
    {
      "CalendarName",
      "ColumnGroupIndex",
      "ColumnGroupUpdateDefinition"
    } };
    operationMetadata25.Description = "Update properties of an existing column group.\r\nMandatory properties: CalendarName, ColumnGroupIndex, ColumnGroupUpdateDefinition (with GroupType).\r\nOptional: TableName (as search hint).";
    OperationMetadata operationMetadata26 = operationMetadata25;
    List<string> stringList9 = new List<string>();
    stringList9.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"UpdateColumnGroup\",\r\n        \"CalendarName\": \"FiscalCalendar\",\r\n        \"ColumnGroupIndex\": 0,\r\n        \"ColumnGroupUpdateDefinition\": {\r\n            \"GroupType\": \"TimeUnitAssociation\",\r\n            \"TimeUnitAssociation\": {\r\n                \"CalendarName\": \"FiscalCalendar\",\r\n                \"GroupType\": \"TimeUnitAssociation\",\r\n                \"TimeUnit\": \"Years\",\r\n                \"PrimaryColumnName\": \"FiscalYear\",\r\n                \"AssociatedColumns\": [\"FiscalYear\", \"FiscalYearName\", \"FiscalYearShort\"]\r\n            }\r\n        }\r\n    }\r\n}");
    operationMetadata26.ExampleRequests = stringList9;
    OperationMetadata operationMetadata27 = operationMetadata25;
    dictionary10["UpdateColumnGroup"] = operationMetadata27;
    Dictionary<string, OperationMetadata> dictionary11 = dictionary1;
    OperationMetadata operationMetadata28 = new OperationMetadata { RequiredParams = new string[2]
    {
      "CalendarName",
      "ColumnGroupIndex"
    } };
    operationMetadata28.Description = "Delete a column group from a calendar by index.\r\nMandatory properties: CalendarName, ColumnGroupIndex.\r\nOptional: TableName (as search hint).";
    OperationMetadata operationMetadata29 = operationMetadata28;
    List<string> stringList10 = new List<string>();
    stringList10.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"DeleteColumnGroup\",\r\n        \"CalendarName\": \"FiscalCalendar\",\r\n        \"ColumnGroupIndex\": 1\r\n    }\r\n}");
    operationMetadata29.ExampleRequests = stringList10;
    OperationMetadata operationMetadata30 = operationMetadata28;
    dictionary11["DeleteColumnGroup"] = operationMetadata30;
    Dictionary<string, OperationMetadata> dictionary12 = dictionary1;
    OperationMetadata operationMetadata31 = new OperationMetadata { RequiredParams = new string[2]
    {
      "CalendarName",
      "ColumnGroupIndex"
    } };
    operationMetadata31.Description = "Get detailed information about a specific column group by index.\r\nMandatory properties: CalendarName, ColumnGroupIndex.\r\nOptional: TableName (as search hint).";
    OperationMetadata operationMetadata32 = operationMetadata31;
    List<string> stringList11 = new List<string>();
    stringList11.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"GetColumnGroup\",\r\n        \"CalendarName\": \"FiscalCalendar\",\r\n        \"ColumnGroupIndex\": 0\r\n    }\r\n}");
    operationMetadata32.ExampleRequests = stringList11;
    OperationMetadata operationMetadata33 = operationMetadata31;
    dictionary12["GetColumnGroup"] = operationMetadata33;
    Dictionary<string, OperationMetadata> dictionary13 = dictionary1;
    OperationMetadata operationMetadata34 = new OperationMetadata { RequiredParams = new string[1]
    {
      "CalendarName"
    } };
    operationMetadata34.Description = "List all column groups within a calendar.\r\nMandatory properties: CalendarName.\r\nOptional: TableName (as search hint).";
    OperationMetadata operationMetadata35 = operationMetadata34;
    List<string> stringList12 = new List<string>();
    stringList12.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"ListColumnGroups\",\r\n        \"CalendarName\": \"FiscalCalendar\"\r\n    }\r\n}");
    operationMetadata35.ExampleRequests = stringList12;
    OperationMetadata operationMetadata36 = operationMetadata34;
    dictionary13["ListColumnGroups"] = operationMetadata36;
    Dictionary<string, OperationMetadata> dictionary14 = dictionary1;
    OperationMetadata operationMetadata37 = new OperationMetadata { RequiredParams = Array.Empty<string>() };
    operationMetadata37.Description = "Get help information about calendar operations.\r\nMandatory properties: None.\r\nOptional: None.";
    List<string> stringList13 = new List<string>();
    stringList13.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Help\"\r\n    }\r\n}");
    operationMetadata37.ExampleRequests = stringList13;
    dictionary14["Help"] = operationMetadata37;
    Dictionary<string, OperationMetadata> dictionary15 = dictionary1;
    toolMetadata2.Operations = dictionary15;
    CalendarOperationsTool.toolMetadata = toolMetadata1;
  }
}
