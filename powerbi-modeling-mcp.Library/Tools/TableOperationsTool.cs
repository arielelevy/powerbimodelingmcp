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
public class TableOperationsTool
{
  private readonly ILogger<TableOperationsTool> _logger;
  public static readonly ToolMetadata toolMetadata;

  public TableOperationsTool(ILogger<TableOperationsTool> logger) => this._logger = logger;

  [McpServerTool(Name = "table_operations")]
  [Description("Perform operations on semantic model tables. Supported operations: Help, Create, Update, Delete, Get, List, Refresh, Rename, GetSchema, ExportTMDL (YAML-like format), ExportTMSL (JSON script format). Use the Operation parameter to specify which operation to perform.")]
  public TableOperationResponse ExecuteTableOperation(
    McpServer mcpServer,
    TableOperationRequest request)
  {
    this._logger.LogDebug("Executing {ToolName}.{Operation}: Table={TableName}, Connection={ConnectionName}", (object) nameof (TableOperationsTool), (object) request.Operation, (object) request.TableName, (object) (request.ConnectionName ?? "(last used)"));
    try
    {
      string[] strArray1 = new string[11]
      {
        "CREATE",
        "UPDATE",
        "DELETE",
        "GET",
        "LIST",
        "REFRESH",
        "RENAME",
        "GETSCHEMA",
        "EXPORTTMDL",
        "EXPORTTMSL",
        "HELP"
      };
      string[] strArray2 = new string[5]
      {
        "CREATE",
        "UPDATE",
        "DELETE",
        "REFRESH",
        "RENAME"
      };
      if (!Enumerable.Contains<string>((IEnumerable<string>) strArray1, request.Operation.ToUpperInvariant()))
      {
        this._logger.LogWarning("Invalid operation '{Operation}' requested for {ToolName}. Valid operations: {ValidOperations}", (object) request.Operation, (object) nameof (TableOperationsTool), (object) string.Join(", ", strArray1));
        return TableOperationResponse.Forbidden(request.Operation, $"Invalid operation: {request.Operation}. Supported operations: {string.Join(", ", strArray1)}");
      }
      if (!this.ValidateRequest(request.Operation, request))
        throw new McpException($"Invalid request for {request.Operation} operation.");
      if (Enumerable.Contains<string>((IEnumerable<string>) strArray2, request.Operation.ToUpperInvariant()))
      {
        WriteOperationResult writeOperationResult = WriteGuard.ExecuteWriteOperationWithGuards(mcpServer, request.ConnectionName, request.Operation);
        if (!writeOperationResult.Success)
        {
          this._logger.LogWarning("{ToolName}.{Operation} blocked by write guard: {Reason}", (object) nameof (TableOperationsTool), (object) request.Operation, (object) writeOperationResult.Message);
          return TableOperationResponse.Forbidden(request.Operation, writeOperationResult.Message);
        }
      }
      bool allowed = WriteGuard.IsWriteAllowed("").allowed;
      string upperInvariant = request.Operation.ToUpperInvariant();
      TableOperationResponse operationResponse;
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
          case 7:
            if ((upperInvariant == "REFRESH"))
            {
              operationResponse = this.HandleRefreshOperation(request);
              goto label_36;
            }
            break;
          case 9:
            if ((upperInvariant == "GETSCHEMA"))
            {
              operationResponse = this.HandleGetSchemaOperation(request);
              goto label_36;
            }
            break;
          case 10:
            switch (upperInvariant[8])
            {
              case 'D':
                if ((upperInvariant == "EXPORTTMDL"))
                {
                  operationResponse = this.HandleExportTMDLOperation(request);
                  goto label_36;
                }
                break;
              case 'S':
                if ((upperInvariant == "EXPORTTMSL"))
                {
                  operationResponse = this.HandleExportTMSLOperation(request);
                  goto label_36;
                }
                break;
            }
            break;
        }
      }
      operationResponse = TableOperationResponse.Forbidden(request.Operation, $"Operation {request.Operation} is not implemented");
label_36:
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Error executing {ToolName}.{Operation}: {ErrorMessage}", (object) nameof (TableOperationsTool), (object) request.Operation, (object) ex.Message);
      return new TableOperationResponse()
      {
        Success = false,
        Message = "Error executing table operation: " + ex.Message,
        Operation = request.Operation,
        TableName = request.TableName
      };
    }
  }

  private TableOperationResponse HandleCreateOperation(TableOperationRequest request)
  {
    try
    {
      if (string.IsNullOrEmpty(request.CreateDefinition.Name))
        request.CreateDefinition.Name = !string.IsNullOrEmpty(request.TableName) ? request.TableName : throw new McpException("TableName is required for Create operation (either in request or CreateDefinition)");
      else if (!string.IsNullOrEmpty(request.TableName) && (request.CreateDefinition.Name != request.TableName))
        throw new McpException($"Table name mismatch: Request specifies '{request.TableName}' but CreateDefinition specifies '{request.CreateDefinition.Name}'");
      TableOperations.TableOperationResult table = TableOperations.CreateTable(request.ConnectionName, request.CreateDefinition);
      ConnectionInfo connectionInfo = ConnectionOperations.Get(request.ConnectionName);
      string[] successfulStates = new string[3]
      {
        "Ready",
        "NoData",
        "CalculationNeeded"
      };
      bool flag = Enumerable.All<PartitionOperationResult>((IEnumerable<PartitionOperationResult>) table.Partitions, (p => Enumerable.Contains<string>((IEnumerable<string>) successfulStates, p.State)));
      List<string> stringList1 = new List<string>();
      foreach (PartitionOperationResult partition in table.Partitions)
      {
        if ((partition.State == "NoData") && !connectionInfo.IsOffline)
          stringList1.Add($"Partition '{partition.PartitionName}' is in NoData state. Use refresh operations to load data into this table.");
        else if ((partition.State == "CalculationNeeded") && !connectionInfo.IsOffline)
          stringList1.Add($"Partition '{partition.PartitionName}' requires calculation. Use refresh operations to calculate and load data.");
        else if (!Enumerable.Contains<string>((IEnumerable<string>) successfulStates, partition.State))
        {
          List<string> stringList2 = stringList1;
          string str;
          if (!string.IsNullOrWhiteSpace(partition.ErrorMessage))
            str = partition.ErrorMessage;
          else
            str = $"Partition '{partition.PartitionName}' is in '{partition.State}' state";
          stringList2.Add(str);
        }
      }
      if (Enumerable.Any<string>((IEnumerable<string>) stringList1))
      {
        foreach (string str in stringList1)
          this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (TableOperationsTool), (object) request.Operation, (object) str);
      }
      this._logger.LogInformation("{ToolName}.{Operation} completed: Table={TableName}, Success={Success}", (object) nameof (TableOperationsTool), (object) request.Operation, (object) request.CreateDefinition.Name, (object) flag);
      return new TableOperationResponse()
      {
        Success = flag,
        Message = flag ? $"Table '{request.CreateDefinition.Name}' created successfully" : $"Table '{request.CreateDefinition.Name}' created with errors",
        Operation = request.Operation,
        TableName = request.CreateDefinition.Name,
        Data = (object) table,
        Warnings = stringList1
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      TableOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new TableOperationResponse()
      {
        Success = false,
        Message = "Error creating table: " + ex.Message,
        Operation = request.Operation,
        TableName = request.TableName,
        Help = (object) operationMetadata
      };
    }
  }

  private TableOperationResponse HandleUpdateOperation(TableOperationRequest request)
  {
    try
    {
      if (string.IsNullOrEmpty(request.UpdateDefinition.Name))
        request.UpdateDefinition.Name = !string.IsNullOrEmpty(request.TableName) ? request.TableName : throw new McpException("TableName is required for Update operation (either in request or UpdateDefinition)");
      else if (!string.IsNullOrEmpty(request.TableName) && (request.UpdateDefinition.Name != request.TableName))
        throw new McpException($"Table name mismatch: Request specifies '{request.TableName}' but UpdateDefinition specifies '{request.UpdateDefinition.Name}'");
      TableOperations.TableOperationResult tableOperationResult = TableOperations.UpdateTable(request.ConnectionName, request.UpdateDefinition);
      ConnectionInfo connectionInfo = ConnectionOperations.Get(request.ConnectionName);
      string[] successfulStates = new string[3]
      {
        "Ready",
        "NoData",
        "CalculationNeeded"
      };
      bool flag = Enumerable.All<PartitionOperationResult>((IEnumerable<PartitionOperationResult>) tableOperationResult.Partitions, (p => Enumerable.Contains<string>((IEnumerable<string>) successfulStates, p.State)));
      List<string> stringList1 = new List<string>();
      if (!tableOperationResult.HasChanges)
        stringList1.Add("No changes were detected. The table is already in the requested state.");
      foreach (PartitionOperationResult partition in tableOperationResult.Partitions)
      {
        if ((partition.State == "NoData") && !connectionInfo.IsOffline)
          stringList1.Add($"Partition '{partition.PartitionName}' is in NoData state. Use refresh operations to load data into this table.");
        else if ((partition.State == "CalculationNeeded") && !connectionInfo.IsOffline)
          stringList1.Add($"Partition '{partition.PartitionName}' requires calculation. Use refresh operations to calculate and load data.");
        else if (!Enumerable.Contains<string>((IEnumerable<string>) successfulStates, partition.State))
        {
          List<string> stringList2 = stringList1;
          string str;
          if (!string.IsNullOrWhiteSpace(partition.ErrorMessage))
            str = partition.ErrorMessage;
          else
            str = $"Partition '{partition.PartitionName}' is in '{partition.State}' state";
          stringList2.Add(str);
        }
      }
      if (Enumerable.Any<string>((IEnumerable<string>) stringList1))
      {
        foreach (string str in stringList1)
          this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (TableOperationsTool), (object) request.Operation, (object) str);
      }
      this._logger.LogInformation("{ToolName}.{Operation} completed: Table={TableName}, Success={Success}, HasChanges={HasChanges}", (object) nameof (TableOperationsTool), (object) request.Operation, (object) request.UpdateDefinition.Name, (object) flag, (object) tableOperationResult.HasChanges);
      return new TableOperationResponse()
      {
        Success = flag,
        Message = flag ? (tableOperationResult.HasChanges ? $"Table '{request.UpdateDefinition.Name}' updated successfully" : $"Table '{request.UpdateDefinition.Name}' is already in the requested state") : $"Table '{request.UpdateDefinition.Name}' updated with partition issues",
        Operation = request.Operation,
        TableName = request.UpdateDefinition.Name,
        Data = (object) tableOperationResult,
        Warnings = stringList1
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      TableOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new TableOperationResponse()
      {
        Success = false,
        Message = "Error updating table: " + ex.Message,
        Operation = request.Operation,
        TableName = request.TableName,
        Help = (object) operationMetadata
      };
    }
  }

  private TableOperationResponse HandleDeleteOperation(TableOperationRequest request)
  {
    try
    {
      TableOperations.DeleteTable(request.ConnectionName, request.TableName, request.ShouldCascadeDelete.Value);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Table={TableName}, Cascade={Cascade}", (object) nameof (TableOperationsTool), (object) request.Operation, (object) request.TableName, (object) request.ShouldCascadeDelete.Value);
      return new TableOperationResponse()
      {
        Success = true,
        Message = $"Table '{request.TableName}' deleted successfully",
        Operation = request.Operation,
        TableName = request.TableName
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      TableOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new TableOperationResponse()
      {
        Success = false,
        Message = "Error deleting table: " + ex.Message,
        Operation = request.Operation,
        TableName = request.TableName,
        Help = (object) operationMetadata
      };
    }
  }

  private TableOperationResponse HandleGetOperation(TableOperationRequest request)
  {
    try
    {
      TableGet table = TableOperations.GetTable(request.ConnectionName, request.TableName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Table={TableName}", (object) nameof (TableOperationsTool), (object) request.Operation, (object) request.TableName);
      return new TableOperationResponse()
      {
        Success = true,
        Message = $"Table '{request.TableName}' retrieved successfully",
        Operation = request.Operation,
        TableName = request.TableName,
        Data = (object) table
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      TableOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new TableOperationResponse()
      {
        Success = false,
        Message = "Error getting table: " + ex.Message,
        Operation = request.Operation,
        TableName = request.TableName,
        Help = (object) operationMetadata
      };
    }
  }

  private TableOperationResponse HandleListOperation(TableOperationRequest request)
  {
    try
    {
      List<TableList> tableListList = TableOperations.ListTables(request.ConnectionName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Count={Count}", (object) nameof (TableOperationsTool), (object) request.Operation, (object) tableListList.Count);
      TableOperationResponse operationResponse = new TableOperationResponse { Success = true };
      operationResponse.Message = $"Found {tableListList.Count} tables";
      operationResponse.Operation = request.Operation;
      operationResponse.Data = (object) tableListList;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      TableOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new TableOperationResponse()
      {
        Success = false,
        Message = "Error listing tables: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private TableOperationResponse HandleRefreshOperation(TableOperationRequest request)
  {
    try
    {
      TableOperations.RefreshTable(request.ConnectionName, request.TableName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Table={TableName}", (object) nameof (TableOperationsTool), (object) request.Operation, (object) request.TableName);
      return new TableOperationResponse()
      {
        Success = true,
        Message = $"Table '{request.TableName}' refreshed successfully",
        Operation = request.Operation,
        TableName = request.TableName
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      TableOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new TableOperationResponse()
      {
        Success = false,
        Message = "Error refreshing table: " + ex.Message,
        Operation = request.Operation,
        TableName = request.TableName,
        Help = (object) operationMetadata
      };
    }
  }

  private TableOperationResponse HandleRenameOperation(TableOperationRequest request)
  {
    try
    {
      if (string.IsNullOrEmpty(request.RenameDefinition.CurrentName))
        request.RenameDefinition.CurrentName = !string.IsNullOrEmpty(request.TableName) ? request.TableName : throw new McpException("Either TableName or RenameDefinition.CurrentName is required.");
      TableOperations.RenameTable(request.ConnectionName, request.RenameDefinition.CurrentName, request.RenameDefinition.NewName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: From={OldName}, To={NewName}", (object) nameof (TableOperationsTool), (object) request.Operation, (object) request.RenameDefinition.CurrentName, (object) request.RenameDefinition.NewName);
      TableOperationResponse operationResponse = new TableOperationResponse { Success = true };
      operationResponse.Message = $"Table renamed from '{request.RenameDefinition.CurrentName}' to '{request.RenameDefinition.NewName}' successfully";
      operationResponse.Operation = request.Operation;
      operationResponse.TableName = request.RenameDefinition.NewName;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      TableOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new TableOperationResponse()
      {
        Success = false,
        Message = "Error renaming table: " + ex.Message,
        Operation = request.Operation,
        TableName = request.TableName,
        Help = (object) operationMetadata
      };
    }
  }

  private TableOperationResponse HandleGetSchemaOperation(TableOperationRequest request)
  {
    try
    {
      Dictionary<string, object> tableSchema = TableOperations.GetTableSchema(request.ConnectionName, request.TableName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Table={TableName}", (object) nameof (TableOperationsTool), (object) request.Operation, (object) request.TableName);
      return new TableOperationResponse()
      {
        Success = true,
        Message = $"Table schema for '{request.TableName}' retrieved successfully",
        Operation = request.Operation,
        TableName = request.TableName,
        Data = (object) tableSchema
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      TableOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new TableOperationResponse()
      {
        Success = false,
        Message = "Error getting table schema: " + ex.Message,
        Operation = request.Operation,
        TableName = request.TableName,
        Help = (object) operationMetadata
      };
    }
  }

  private TableOperationResponse HandleExportTMDLOperation(TableOperationRequest request)
  {
    try
    {
      TmdlExportResult tmdlExportResult = TableOperations.ExportTMDL(request.ConnectionName, request.TableName, request.TmdlExportOptions);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Table={TableName}", (object) nameof (TableOperationsTool), (object) request.Operation, (object) request.TableName);
      return new TableOperationResponse()
      {
        Success = true,
        Message = $"TMDL for table '{request.TableName}' retrieved successfully",
        Operation = request.Operation,
        TableName = request.TableName,
        Data = (object) tmdlExportResult
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      TableOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new TableOperationResponse()
      {
        Success = false,
        Message = "Error getting table TMDL: " + ex.Message,
        Operation = request.Operation,
        TableName = request.TableName,
        Help = (object) operationMetadata
      };
    }
  }

  private TableOperationResponse HandleExportTMSLOperation(TableOperationRequest request)
  {
    try
    {
      if (string.IsNullOrWhiteSpace(request.TmslExportOptions.TmslOperationType))
        throw new McpException("TmslOperationType is required in TmslExportOptions. Valid values: Create, CreateOrReplace, Alter, Delete, Refresh");
      TmslExportResult tmslExportResult = TableOperations.ExportTMSL(request.ConnectionName, request.TableName, request.TmslExportOptions);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Table={TableName}, OperationType={OperationType}, Success={Success}", (object) nameof (TableOperationsTool), (object) request.Operation, (object) request.TableName, (object) request.TmslExportOptions.TmslOperationType, (object) tmslExportResult.Success);
      TableOperationResponse operationResponse = new TableOperationResponse { Success = tmslExportResult.Success };
      string str;
      if (!tmslExportResult.Success)
        str = tmslExportResult.ErrorMessage ?? "Unknown error occurred";
      else
        str = $"TMSL {request.TmslExportOptions.TmslOperationType} script for table '{request.TableName}' generated successfully";
      operationResponse.Message = str;
      operationResponse.Operation = request.Operation;
      operationResponse.TableName = request.TableName;
      operationResponse.Data = (object) tmslExportResult;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      TableOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new TableOperationResponse()
      {
        Success = false,
        Message = "Error generating table TMSL: " + ex.Message,
        Operation = request.Operation,
        TableName = request.TableName,
        Help = (object) operationMetadata
      };
    }
  }

  private TableOperationResponse HandleHelpOperation(
    TableOperationRequest request,
    string[] operations)
  {
    this._logger.LogInformation("{ToolName}.{Operation} completed: Operations={OperationCount}", (object) nameof (TableOperationsTool), (object) request.Operation, (object) operations.Length);
    return new TableOperationResponse()
    {
      Success = true,
      Message = "Help information for the table operations",
      Operation = request.Operation,
      Help = (object) new
      {
        ToolName = "table_operations",
        Description = "Perform operations on semantic model tables.",
        SupportedOperations = operations,
        Examples = Enumerable.Where<KeyValuePair<string, OperationMetadata>>((IEnumerable<KeyValuePair<string, OperationMetadata>>) TableOperationsTool.toolMetadata.Operations, (Func<KeyValuePair<string, OperationMetadata>, bool>) (p => Enumerable.Contains<string>((IEnumerable<string>) operations, p.Key, (IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase))),
        Notes = new string[7]
        {
          "The Operation parameter specifies which operation to perform.",
          "The ConnectionName parameter is optional and uses the last used connection if not provided.",
          "The TableName parameter is required for all operations except List.",
          "The NewTableName parameter is required for the Rename operation.",
          "The CreateDefinition parameter is required for the Create operation.",
          "The UpdateDefinition parameter is required for the Update operation.",
          "The ShouldCascadeDelete parameter is required for the Delete operation."
        }
      }
    };
  }

  private bool ValidateRequest(string operation, TableOperationRequest request)
  {
    OperationMetadata operationMetadata;
    if (!TableOperationsTool.toolMetadata.Operations.TryGetValue(operation, out operationMetadata))
      return true;
    JsonObject requestDict = JsonSerializer.SerializeToNode<TableOperationRequest>(request) as JsonObject;
    List<string> list1 = Enumerable.ToList<string>(Enumerable.Where<string>((IEnumerable<string>) operationMetadata.RequiredParams, (p => requestDict != null && requestDict[p] == null)));
    List<string> list2 = Enumerable.ToList<string>(Enumerable.Where<string>((IEnumerable<string>) operationMetadata.ForbiddenParams, (p => requestDict != null && requestDict[p] != null)));
    if (Enumerable.Any<string>((IEnumerable<string>) list1))
      throw new McpException($"Missing required parameters needed for {operation} operation: {string.Join(", ", (IEnumerable<string>) list1)}");
    if (Enumerable.Any<string>((IEnumerable<string>) list2))
      throw new McpException($"Forbidden parameters not allowed for {operation} operation: {string.Join(", ", (IEnumerable<string>) list2)}");
    return true;
  }

  static TableOperationsTool()
  {
    ToolMetadata toolMetadata1 = new ToolMetadata();
    ToolMetadata toolMetadata2 = toolMetadata1;
    Dictionary<string, OperationMetadata> dictionary1 = new Dictionary<string, OperationMetadata>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    Dictionary<string, OperationMetadata> dictionary2 = dictionary1;
    OperationMetadata operationMetadata1 = new OperationMetadata { RequiredParams = new string[1]
    {
      "CreateDefinition"
    } };
    operationMetadata1.Description = "Create a new table in the semantic model.\r\nMandatory properties: CreateDefinition (with Name, and one of DaxExpression, MExpression, EntityName, or SqlQuery).\r\n- DaxExpression: For calculated tables\r\n- MExpression: For M-script tables\r\n- EntityName: For entity-based tables (requires either ExpressionSourceName or DataSourceName, optionally SchemaName)\r\n- SqlQuery: For query-based tables (requires DataSourceName)\r\nOptional: Description, DataCategory, IsHidden, ShowAsVariationsOnly, IsPrivate, AlternateSourcePrecedence, ExcludeFromModelRefresh, LineageTag, SourceLineageTag, SystemManaged, PartitionName, Mode, Columns, Annotations, ExtendedProperties.\r\nNote: For DaxExpression tables, do not specify Columns as they are auto-derived from the DAX expression. For MExpression, SqlQuery, and EntityName tables, Columns will be empty unless explicitly specified.";
    operationMetadata1.CommonMistakes = new string[3]
    {
      "Not providing one of DaxExpression, MExpression, EntityName, or SqlQuery in CreateDefinition",
      "Not providing either ExpressionSourceName or DataSourceName when using EntityName",
      "Not providing DataSourceName when using SqlQuery"
    };
    operationMetadata1.Tips = new string[2]
    {
      "'Measures' is a reserved word and cannot be used as a table name",
      "DirectLake mode requires EntityName with ExpressionSourceName (and optionally SchemaName)"
    };
    OperationMetadata operationMetadata2 = operationMetadata1;
    List<string> stringList1 = new List<string>();
    stringList1.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Create\",\r\n        \"CreateDefinition\": { \r\n            \"Name\": \"Sales\", \r\n            \"DaxExpression\": \"SUMMARIZECOLUMNS(DimDate[Year], DimProduct[ProductName], DimCustomer[CustomerName] \\\"Sales\\\", [Sales])\"\r\n        }\r\n    }\r\n}");
    stringList1.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Create\",\r\n        \"TableName\": \"Sales\",\r\n        \"CreateDefinition\": {\r\n            \"Mode\": \"Import\",\r\n            \"MExpression\": \"let Source = Excel.CurrentWorkbook(){[Name=\\\"SalesData\\\"]}[Content], FilteredRows = Table.SelectRows(Source, each [Region] = \\\"West\\\") in FilteredRows\",\r\n            \"Columns\": [\r\n                {\r\n                    \"Name\": \"Region\",\r\n                    \"DataType\": \"String\",\r\n                    \"IsNullable\": true,\r\n                    \"Ordinal\": 0\r\n                },\r\n                {\r\n                    \"Name\": \"Amount\",\r\n                    \"DataType\": \"Decimal\",\r\n                    \"IsNullable\": true,\r\n                    \"Ordinal\": 1\r\n                }\r\n            ]\r\n        }\r\n    }\r\n}");
    stringList1.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Create\",\r\n        \"CreateDefinition\": { \r\n            \"Name\": \"Customer\",\r\n            \"Mode\": \"DirectLake\",\r\n            \"EntityName\": \"Customer\",\r\n            \"SchemaName\": \"dbo\",\r\n            \"ExpressionSourceName\": \"SalesLakehouse\",\r\n            \"Columns\": [\r\n                {\r\n                    \"Name\": \"CustomerId\",\r\n                    \"DataType\": \"Int64\",\r\n                    \"IsNullable\": false,\r\n                    \"IsKey\": true,\r\n                    \"Ordinal\": 0\r\n                },\r\n                {\r\n                    \"Name\": \"CustomerName\",\r\n                    \"DataType\": \"String\",\r\n                    \"IsNullable\": true,\r\n                    \"Ordinal\": 1\r\n                },\r\n                {\r\n                    \"Name\": \"Email\",\r\n                    \"DataType\": \"String\",\r\n                    \"IsNullable\": true,\r\n                    \"Ordinal\": 2\r\n                }\r\n            ]\r\n        }\r\n    }\r\n}");
    operationMetadata2.ExampleRequests = stringList1;
    OperationMetadata operationMetadata3 = operationMetadata1;
    dictionary2["Create"] = operationMetadata3;
    Dictionary<string, OperationMetadata> dictionary3 = dictionary1;
    OperationMetadata operationMetadata4 = new OperationMetadata { RequiredParams = new string[1]
    {
      "UpdateDefinition"
    } };
    operationMetadata4.Description = "Update properties of an existing table.\r\nMandatory properties: UpdateDefinition (with Name).\r\nOptional: Description, DataCategory, IsHidden, ShowAsVariationsOnly, IsPrivate, AlternateSourcePrecedence, ExcludeFromModelRefresh, LineageTag, SourceLineageTag, SystemManaged, Annotations, ExtendedProperties.";
    OperationMetadata operationMetadata5 = operationMetadata4;
    List<string> stringList2 = new List<string>();
    stringList2.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Update\",\r\n        \"UpdateDefinition\": { \r\n            \"Name\": \"Sales\", \r\n            \"Description\": \"Description of the Sales table\",\r\n            \"ShowAsVariationsOnly\": true\r\n        }\r\n    }\r\n}");
    operationMetadata5.ExampleRequests = stringList2;
    OperationMetadata operationMetadata6 = operationMetadata4;
    dictionary3["Update"] = operationMetadata6;
    Dictionary<string, OperationMetadata> dictionary4 = dictionary1;
    OperationMetadata operationMetadata7 = new OperationMetadata { RequiredParams = new string[2]
    {
      "TableName",
      "ShouldCascadeDelete"
    } };
    operationMetadata7.Description = "Delete a table from the semantic model.\r\nMandatory properties: TableName, ShouldCascadeDelete.\r\nOptional: None.\r\nNote: When ShouldCascadeDelete is true, dependent objects (columns, relationships, etc.) will be automatically deleted.";
    OperationMetadata operationMetadata8 = operationMetadata7;
    List<string> stringList3 = new List<string>();
    stringList3.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Delete\",\r\n        \"TableName\": \"ObsoleteTable\",\r\n        \"ShouldCascadeDelete\": true\r\n    }\r\n}");
    operationMetadata8.ExampleRequests = stringList3;
    OperationMetadata operationMetadata9 = operationMetadata7;
    dictionary4["Delete"] = operationMetadata9;
    Dictionary<string, OperationMetadata> dictionary5 = dictionary1;
    OperationMetadata operationMetadata10 = new OperationMetadata { RequiredParams = new string[1]
    {
      "TableName"
    } };
    operationMetadata10.Description = "Retrieve detailed information about a specific table.\r\nMandatory properties: TableName.\r\nOptional: None.\r\nReturns table properties, columns, measures, hierarchies, and partition details.";
    OperationMetadata operationMetadata11 = operationMetadata10;
    List<string> stringList4 = new List<string>();
    stringList4.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Get\",\r\n        \"TableName\": \"Sales\"\r\n    }\r\n}");
    operationMetadata11.ExampleRequests = stringList4;
    OperationMetadata operationMetadata12 = operationMetadata10;
    dictionary5["Get"] = operationMetadata12;
    Dictionary<string, OperationMetadata> dictionary6 = dictionary1;
    OperationMetadata operationMetadata13 = new OperationMetadata { Description = "List all tables in the semantic model.\r\nMandatory properties: None.\r\nOptional: None.\r\nReturns summary information for all tables including column count, measure count, hierarchy count, and partition count." };
    List<string> stringList5 = new List<string>();
    stringList5.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"List\"\r\n    }\r\n}");
    operationMetadata13.ExampleRequests = stringList5;
    dictionary6["List"] = operationMetadata13;
    Dictionary<string, OperationMetadata> dictionary7 = dictionary1;
    OperationMetadata operationMetadata14 = new OperationMetadata { RequiredParams = new string[1]
    {
      "TableName"
    } };
    operationMetadata14.Description = "Refresh data for a specific table by processing its partitions.\r\nMandatory properties: TableName.\r\nOptional: None.\r\nThis operation reloads data from the underlying data source(s).";
    OperationMetadata operationMetadata15 = operationMetadata14;
    List<string> stringList6 = new List<string>();
    stringList6.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Refresh\",\r\n        \"TableName\": \"Sales\"\r\n    }\r\n}");
    operationMetadata15.ExampleRequests = stringList6;
    OperationMetadata operationMetadata16 = operationMetadata14;
    dictionary7["Refresh"] = operationMetadata16;
    Dictionary<string, OperationMetadata> dictionary8 = dictionary1;
    OperationMetadata operationMetadata17 = new OperationMetadata { RequiredParams = new string[1]
    {
      "RenameDefinition"
    } };
    operationMetadata17.Description = "Rename a table and automatically update all references.\r\nMandatory properties: RenameDefinition (with CurrentName, NewName).\r\nOptional: TableName (fallback for CurrentName).\r\nAll DAX expressions, relationships, and other references will be automatically updated.";
    OperationMetadata operationMetadata18 = operationMetadata17;
    List<string> stringList7 = new List<string>();
    stringList7.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Rename\",\r\n        \"RenameDefinition\": { \r\n            \"CurrentName\": \"OldTableName\", \r\n            \"NewName\": \"NewTableName\"\r\n        }\r\n    }\r\n}");
    operationMetadata18.ExampleRequests = stringList7;
    OperationMetadata operationMetadata19 = operationMetadata17;
    dictionary8["Rename"] = operationMetadata19;
    Dictionary<string, OperationMetadata> dictionary9 = dictionary1;
    OperationMetadata operationMetadata20 = new OperationMetadata { RequiredParams = new string[1]
    {
      "TableName"
    } };
    operationMetadata20.Description = "Retrieve the schema information for a specific table.\r\nMandatory properties: TableName.\r\nOptional: None.\r\nReturns detailed column definitions, data types, and relationships.";
    OperationMetadata operationMetadata21 = operationMetadata20;
    List<string> stringList8 = new List<string>();
    stringList8.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"GetSchema\",\r\n        \"TableName\": \"Sales\"\r\n    }\r\n}");
    operationMetadata21.ExampleRequests = stringList8;
    OperationMetadata operationMetadata22 = operationMetadata20;
    dictionary9["GetSchema"] = operationMetadata22;
    Dictionary<string, OperationMetadata> dictionary10 = dictionary1;
    OperationMetadata operationMetadata23 = new OperationMetadata { RequiredParams = new string[1]
    {
      "TableName"
    } };
    operationMetadata23.Description = "Export table definition to TMDL (YAML-like syntax) format for human-readable declarative model definition.\r\nMandatory properties: TableName.\r\nOptional: TmdlExportOptions (with IncludeRestrictedInformation, IncludeRoleMembers).\r\nTMDL is ideal for version control and collaborative development.";
    OperationMetadata operationMetadata24 = operationMetadata23;
    List<string> stringList9 = new List<string>();
    stringList9.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"ExportTMDL\",\r\n        \"TableName\": \"Sales\"\r\n    }\r\n}");
    operationMetadata24.ExampleRequests = stringList9;
    OperationMetadata operationMetadata25 = operationMetadata23;
    dictionary10["ExportTMDL"] = operationMetadata25;
    Dictionary<string, OperationMetadata> dictionary11 = dictionary1;
    OperationMetadata operationMetadata26 = new OperationMetadata { RequiredParams = new string[2]
    {
      "TableName",
      "TmslExportOptions"
    } };
    operationMetadata26.Description = "Export table to TMSL (JSON syntax) script format with specified operation type for generating executable scripts.\r\nMandatory properties: TableName, TmslExportOptions (with TmslOperationType).\r\nOptional: RefreshType (for Refresh operations), IncludeRestricted (for Refresh operations).\r\nTMSL generates JSON scripts that can be executed against Analysis Services.";
    OperationMetadata operationMetadata27 = operationMetadata26;
    List<string> stringList10 = new List<string>();
    stringList10.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"ExportTMSL\",\r\n        \"TableName\": \"Sales\",\r\n        \"TmslExportOptions\": {\r\n            \"TmslOperationType\": \"CreateOrReplace\"\r\n        }\r\n    }\r\n}");
    stringList10.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"ExportTMSL\",\r\n        \"TableName\": \"Sales\",\r\n        \"TmslExportOptions\": {\r\n            \"TmslOperationType\": \"Refresh\",\r\n            \"RefreshType\": \"Full\",\r\n            \"IncludeRestricted\": true\r\n        }\r\n    }\r\n}");
    operationMetadata27.ExampleRequests = stringList10;
    OperationMetadata operationMetadata28 = operationMetadata26;
    dictionary11["ExportTMSL"] = operationMetadata28;
    Dictionary<string, OperationMetadata> dictionary12 = dictionary1;
    OperationMetadata operationMetadata29 = new OperationMetadata { Description = "Display comprehensive help information about the table operations tool and all available operations.\r\nMandatory properties: None.\r\nOptional: None.\r\nReturns detailed documentation for each operation with examples." };
    List<string> stringList11 = new List<string>();
    stringList11.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Help\"\r\n    }\r\n}");
    operationMetadata29.ExampleRequests = stringList11;
    dictionary12["Help"] = operationMetadata29;
    Dictionary<string, OperationMetadata> dictionary13 = dictionary1;
    toolMetadata2.Operations = dictionary13;
    TableOperationsTool.toolMetadata = toolMetadata1;
  }
}
