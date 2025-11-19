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
public class PartitionOperationsTool
{
  private readonly ILogger<PartitionOperationsTool> _logger;
  public static readonly ToolMetadata toolMetadata;

  public PartitionOperationsTool(ILogger<PartitionOperationsTool> logger) => this._logger = logger;

  [McpServerTool(Name = "partition_operations")]
  [Description("Perform operations on semantic model partitions. Supported operations: Help, List, Get, Create, Update, Delete, Refresh, Rename, ExportTMDL (YAML-like format), ExportTMSL (JSON script format). Use the Operation parameter to specify which operation to perform.")]
  public PartitionOperationResponse ExecutePartitionOperation(
    McpServer mcpServer,
    PartitionOperationRequest request)
  {
    this._logger.LogDebug("Executing {ToolName}.{Operation}: Table={TableName}, Partition={PartitionName}, Connection={ConnectionName}", (object) nameof (PartitionOperationsTool), (object) request.Operation, (object) (request.TableName ?? "(none)"), (object) (request.PartitionName ?? "(none)"), (object) (request.ConnectionName ?? "(last used)"));
    try
    {
      string[] strArray1 = new string[10]
      {
        "LIST",
        "GET",
        "CREATE",
        "UPDATE",
        "DELETE",
        "REFRESH",
        "RENAME",
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
        this._logger.LogWarning("Invalid operation '{Operation}' requested for {ToolName}. Valid operations: {ValidOperations}", (object) request.Operation, (object) nameof (PartitionOperationsTool), (object) string.Join(", ", strArray1));
        return PartitionOperationResponse.Forbidden(request.Operation, $"Invalid operation: {request.Operation}. Supported operations: {string.Join(", ", strArray1)}");
      }
      if (!this.ValidateRequest(request.Operation, request))
        throw new McpException($"Invalid request for {request.Operation} operation.");
      if (Enumerable.Contains<string>((IEnumerable<string>) strArray2, request.Operation.ToUpperInvariant()))
      {
        WriteOperationResult writeOperationResult = WriteGuard.ExecuteWriteOperationWithGuards(mcpServer, request.ConnectionName, request.Operation);
        if (!writeOperationResult.Success)
        {
          this._logger.LogWarning("{ToolName}.{Operation} blocked by write guard: {Reason}", (object) nameof (PartitionOperationsTool), (object) request.Operation, (object) writeOperationResult.Message);
          return PartitionOperationResponse.Forbidden(request.Operation, writeOperationResult.Message);
        }
      }
      bool allowed = WriteGuard.IsWriteAllowed("").allowed;
      string upperInvariant = request.Operation.ToUpperInvariant();
      PartitionOperationResponse operationResponse;
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
          case 7:
            if ((upperInvariant == "REFRESH"))
            {
              operationResponse = this.HandleRefreshOperation(request);
              goto label_34;
            }
            break;
          case 10:
            switch (upperInvariant[8])
            {
              case 'D':
                if ((upperInvariant == "EXPORTTMDL"))
                {
                  operationResponse = this.HandleExportTMDLOperation(request);
                  goto label_34;
                }
                break;
              case 'S':
                if ((upperInvariant == "EXPORTTMSL"))
                {
                  operationResponse = this.HandleExportTMSLOperation(request);
                  goto label_34;
                }
                break;
            }
            break;
        }
      }
      operationResponse = PartitionOperationResponse.Forbidden(request.Operation, $"Operation {request.Operation} not implemented");
label_34:
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Error executing {ToolName}.{Operation}: {ErrorMessage}", (object) nameof (PartitionOperationsTool), (object) request.Operation, (object) ex.Message);
      return new PartitionOperationResponse()
      {
        Success = false,
        Message = "Error executing partition operation: " + ex.Message,
        Operation = request.Operation,
        TableName = request.TableName,
        PartitionName = request.PartitionName
      };
    }
  }

  private PartitionOperationResponse HandleListOperation(PartitionOperationRequest request)
  {
    try
    {
      List<PartitionGet> partitionGetList = PartitionOperations.ListPartitions(request.ConnectionName, request.TableName);
      string str1;
      if (!string.IsNullOrEmpty(request.TableName))
        str1 = $"Found {partitionGetList.Count} partitions in table '{request.TableName}'";
      else
        str1 = $"Found {partitionGetList.Count} partitions across all tables";
      string str2 = str1;
      this._logger.LogInformation("{ToolName}.{Operation} completed: Table={TableName}, Count={Count}", (object) nameof (PartitionOperationsTool), (object) "List", (object) (request.TableName ?? "(all)"), (object) partitionGetList.Count);
      return new PartitionOperationResponse()
      {
        Success = true,
        Message = str2,
        Operation = "LIST",
        TableName = request.TableName,
        Data = (object) partitionGetList
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      PartitionOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new PartitionOperationResponse()
      {
        Success = false,
        Message = "Failed to list partitions: " + ex.Message,
        Operation = "LIST",
        TableName = request.TableName,
        Help = (object) operationMetadata
      };
    }
  }

  private PartitionOperationResponse HandleGetOperation(PartitionOperationRequest request)
  {
    try
    {
      PartitionGet partition = PartitionOperations.GetPartition(request.ConnectionName, request.TableName, request.PartitionName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Table={TableName}, Partition={PartitionName}", (object) nameof (PartitionOperationsTool), (object) "Get", (object) request.TableName, (object) request.PartitionName);
      PartitionOperationResponse operation = new PartitionOperationResponse { Success = true };
      operation.Message = $"Retrieved partition '{request.PartitionName}' from table '{request.TableName}' successfully";
      operation.Operation = "GET";
      operation.TableName = request.TableName;
      operation.PartitionName = request.PartitionName;
      operation.Data = (object) partition;
      return operation;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      PartitionOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new PartitionOperationResponse()
      {
        Success = false,
        Message = "Failed to get partition: " + ex.Message,
        Operation = "GET",
        TableName = request.TableName,
        PartitionName = request.PartitionName,
        Help = (object) operationMetadata
      };
    }
  }

  private PartitionOperationResponse HandleCreateOperation(PartitionOperationRequest request)
  {
    try
    {
      if (string.IsNullOrEmpty(request.CreateDefinition.TableName))
        request.CreateDefinition.TableName = !string.IsNullOrEmpty(request.TableName) ? request.TableName : throw new McpException("TableName is required for Create operation (either in request or CreateDefinition)");
      else if (!string.IsNullOrEmpty(request.TableName) && (request.CreateDefinition.TableName != request.TableName))
        throw new McpException($"Table name mismatch: Request specifies '{request.TableName}' but CreateDefinition specifies '{request.CreateDefinition.TableName}'");
      if (string.IsNullOrEmpty(request.CreateDefinition.Name))
        request.CreateDefinition.Name = !string.IsNullOrEmpty(request.PartitionName) ? request.PartitionName : throw new McpException("PartitionName is required for Create operation (either in request or CreateDefinition)");
      else if (!string.IsNullOrEmpty(request.PartitionName) && (request.CreateDefinition.Name != request.PartitionName))
        throw new McpException($"Partition name mismatch: Request specifies '{request.PartitionName}' but CreateDefinition specifies '{request.CreateDefinition.Name}'");
      PartitionOperationResult partition = PartitionOperations.CreatePartition(request.ConnectionName, request.CreateDefinition);
      ConnectionInfo connectionInfo = ConnectionOperations.Get(request.ConnectionName);
      string[] strArray = new string[3]
      {
        "Ready",
        "NoData",
        "CalculationNeeded"
      };
      bool flag = Enumerable.Contains<string>((IEnumerable<string>) strArray, partition.State);
      List<string> stringList = partition.Warnings != null ? new List<string>((IEnumerable<string>) partition.Warnings) : new List<string>();
      if ((partition.State == "NoData") && !connectionInfo.IsOffline)
        stringList.Add($"Partition '{partition.PartitionName}' is in NoData state. Use refresh operations to load data into this partition.");
      else if ((partition.State == "CalculationNeeded") && !connectionInfo.IsOffline)
        stringList.Add($"Partition '{partition.PartitionName}' requires calculation. Use refresh operations to calculate and load data.");
      else if (!Enumerable.Contains<string>((IEnumerable<string>) strArray, partition.State) && !string.IsNullOrEmpty(partition.ErrorMessage))
        stringList.Add(partition.ErrorMessage);
      PartitionOperationResponse operationResponse = new PartitionOperationResponse { Success = flag };
      string str1;
      if (!flag)
        str1 = partition.ErrorMessage ?? $"Failed to create partition '{request.CreateDefinition.Name}'";
      else
        str1 = $"Partition '{request.CreateDefinition.Name}' created successfully in table '{request.CreateDefinition.TableName}'";
      operationResponse.Message = str1;
      operationResponse.Operation = "CREATE";
      operationResponse.TableName = request.CreateDefinition.TableName;
      operationResponse.PartitionName = request.CreateDefinition.Name;
      operationResponse.Data = (object) partition;
      operationResponse.Warnings = stringList;
      PartitionOperationResponse operation = operationResponse;
      if (operation.Success)
        this._logger.LogInformation("{ToolName}.{Operation} completed: Table={TableName}, Partition={PartitionName}, State={State}", (object) nameof (PartitionOperationsTool), (object) request.Operation, (object) request.CreateDefinition.TableName, (object) request.CreateDefinition.Name, (object) partition.State);
      else
        this._logger.LogWarning("{ToolName}.{Operation} failed: Table={TableName}, Partition={PartitionName}, State={State}", (object) nameof (PartitionOperationsTool), (object) request.Operation, (object) request.CreateDefinition.TableName, (object) request.CreateDefinition.Name, (object) partition.State);
      if (Enumerable.Any<string>((IEnumerable<string>) stringList))
      {
        foreach (string str2 in stringList)
          this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (PartitionOperationsTool), (object) request.Operation, (object) str2);
      }
      return operation;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      PartitionOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new PartitionOperationResponse()
      {
        Success = false,
        Message = "Failed to create partition: " + ex.Message,
        Operation = "CREATE",
        TableName = request.TableName,
        Help = (object) operationMetadata
      };
    }
  }

  private PartitionOperationResponse HandleUpdateOperation(PartitionOperationRequest request)
  {
    try
    {
      if (string.IsNullOrEmpty(request.UpdateDefinition.TableName))
        request.UpdateDefinition.TableName = !string.IsNullOrEmpty(request.TableName) ? request.TableName : throw new McpException("TableName is required for Update operation (either in request or UpdateDefinition)");
      else if (!string.IsNullOrEmpty(request.TableName) && (request.UpdateDefinition.TableName != request.TableName))
        throw new McpException($"Table name mismatch: Request specifies '{request.TableName}' but UpdateDefinition specifies '{request.UpdateDefinition.TableName}'");
      if (string.IsNullOrEmpty(request.UpdateDefinition.Name))
        request.UpdateDefinition.Name = !string.IsNullOrEmpty(request.PartitionName) ? request.PartitionName : throw new McpException("PartitionName is required for Update operation (either in request or UpdateDefinition)");
      else if (!string.IsNullOrEmpty(request.PartitionName) && (request.UpdateDefinition.Name != request.PartitionName))
        throw new McpException($"Partition name mismatch: Request specifies '{request.PartitionName}' but UpdateDefinition specifies '{request.UpdateDefinition.Name}'");
      PartitionOperationResult partitionOperationResult = PartitionOperations.UpdatePartition(request.ConnectionName, request.UpdateDefinition);
      ConnectionInfo connectionInfo = ConnectionOperations.Get(request.ConnectionName);
      string[] strArray = new string[3]
      {
        "Ready",
        "NoData",
        "CalculationNeeded"
      };
      bool flag = Enumerable.Contains<string>((IEnumerable<string>) strArray, partitionOperationResult.State);
      List<string> stringList = partitionOperationResult.Warnings != null ? new List<string>((IEnumerable<string>) partitionOperationResult.Warnings) : new List<string>();
      if ((partitionOperationResult.State == "NoData") && !connectionInfo.IsOffline)
        stringList.Add($"Partition '{partitionOperationResult.PartitionName}' is in NoData state. Use refresh operations to load data into this partition.");
      else if ((partitionOperationResult.State == "CalculationNeeded") && !connectionInfo.IsOffline)
        stringList.Add($"Partition '{partitionOperationResult.PartitionName}' requires calculation. Use refresh operations to calculate and load data.");
      else if (!Enumerable.Contains<string>((IEnumerable<string>) strArray, partitionOperationResult.State) && !string.IsNullOrEmpty(partitionOperationResult.ErrorMessage))
        stringList.Add(partitionOperationResult.ErrorMessage);
      PartitionOperationResponse operationResponse1 = new PartitionOperationResponse { Success = flag };
      string str1;
      if (!flag)
        str1 = partitionOperationResult.ErrorMessage ?? $"Failed to update partition '{request.UpdateDefinition.Name}'";
      else
        str1 = $"Partition '{request.UpdateDefinition.Name}' updated successfully in table '{request.UpdateDefinition.TableName}'";
      operationResponse1.Message = str1;
      operationResponse1.Operation = "UPDATE";
      operationResponse1.TableName = request.UpdateDefinition.TableName;
      operationResponse1.PartitionName = request.UpdateDefinition.Name;
      operationResponse1.Data = (object) partitionOperationResult;
      operationResponse1.Warnings = stringList;
      PartitionOperationResponse operationResponse2 = operationResponse1;
      if (operationResponse2.Success)
        this._logger.LogInformation("{ToolName}.{Operation} completed: Table={TableName}, Partition={PartitionName}, State={State}", (object) nameof (PartitionOperationsTool), (object) request.Operation, (object) request.UpdateDefinition.TableName, (object) request.UpdateDefinition.Name, (object) partitionOperationResult.State);
      else
        this._logger.LogWarning("{ToolName}.{Operation} failed: Table={TableName}, Partition={PartitionName}, State={State}", (object) nameof (PartitionOperationsTool), (object) request.Operation, (object) request.UpdateDefinition.TableName, (object) request.UpdateDefinition.Name, (object) partitionOperationResult.State);
      if (Enumerable.Any<string>((IEnumerable<string>) stringList))
      {
        foreach (string str2 in stringList)
          this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (PartitionOperationsTool), (object) request.Operation, (object) str2);
      }
      return operationResponse2;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      PartitionOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new PartitionOperationResponse()
      {
        Success = false,
        Message = "Failed to update partition: " + ex.Message,
        Operation = "UPDATE",
        TableName = request.TableName,
        Help = (object) operationMetadata
      };
    }
  }

  private PartitionOperationResponse HandleDeleteOperation(PartitionOperationRequest request)
  {
    try
    {
      PartitionOperations.DeletePartition(request.ConnectionName, request.TableName, request.PartitionName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Table={TableName}, Partition={PartitionName}", (object) nameof (PartitionOperationsTool), (object) "Delete", (object) request.TableName, (object) request.PartitionName);
      PartitionOperationResponse operationResponse = new PartitionOperationResponse { Success = true };
      operationResponse.Message = $"Partition '{request.PartitionName}' deleted successfully from table '{request.TableName}'";
      operationResponse.Operation = "DELETE";
      operationResponse.TableName = request.TableName;
      operationResponse.PartitionName = request.PartitionName;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      PartitionOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new PartitionOperationResponse()
      {
        Success = false,
        Message = "Failed to delete partition: " + ex.Message,
        Operation = "DELETE",
        TableName = request.TableName,
        PartitionName = request.PartitionName,
        Help = (object) operationMetadata
      };
    }
  }

  private PartitionOperationResponse HandleRefreshOperation(PartitionOperationRequest request)
  {
    try
    {
      string refreshType = request.RefreshType ?? "Automatic";
      PartitionOperations.RefreshPartition(request.ConnectionName, request.TableName, request.PartitionName, refreshType);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Table={TableName}, Partition={PartitionName}, RefreshType={RefreshType}", (object) nameof (PartitionOperationsTool), (object) "Refresh", (object) request.TableName, (object) request.PartitionName, (object) refreshType);
      PartitionOperationResponse operationResponse = new PartitionOperationResponse { Success = true };
      operationResponse.Message = $"Partition '{request.PartitionName}' in table '{request.TableName}' refreshed successfully with refresh type '{refreshType}'";
      operationResponse.Operation = "REFRESH";
      operationResponse.TableName = request.TableName;
      operationResponse.PartitionName = request.PartitionName;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      PartitionOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new PartitionOperationResponse()
      {
        Success = false,
        Message = "Failed to refresh partition: " + ex.Message,
        Operation = "REFRESH",
        TableName = request.TableName,
        PartitionName = request.PartitionName,
        Help = (object) operationMetadata
      };
    }
  }

  private PartitionOperationResponse HandleRenameOperation(PartitionOperationRequest request)
  {
    try
    {
      PartitionOperations.RenamePartition(request.ConnectionName, request.TableName, request.PartitionName, request.NewName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Table={TableName}, From={OldName}, To={NewName}", (object) nameof (PartitionOperationsTool), (object) "Rename", (object) request.TableName, (object) request.PartitionName, (object) request.NewName);
      PartitionOperationResponse operationResponse = new PartitionOperationResponse { Success = true };
      operationResponse.Message = $"Partition '{request.PartitionName}' renamed to '{request.NewName}' successfully in table '{request.TableName}'";
      operationResponse.Operation = "RENAME";
      operationResponse.TableName = request.TableName;
      operationResponse.PartitionName = request.NewName;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      PartitionOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new PartitionOperationResponse()
      {
        Success = false,
        Message = "Failed to rename partition: " + ex.Message,
        Operation = "RENAME",
        TableName = request.TableName,
        PartitionName = request.PartitionName,
        Help = (object) operationMetadata
      };
    }
  }

  private PartitionOperationResponse HandleExportTMDLOperation(PartitionOperationRequest request)
  {
    try
    {
      TmdlExportResult tmdlExportResult = PartitionOperations.ExportTMDL(request.ConnectionName, request.TableName, request.PartitionName, request.TmdlExportOptions);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Table={TableName}, Partition={PartitionName}", (object) nameof (PartitionOperationsTool), (object) request.Operation, (object) request.TableName, (object) request.PartitionName);
      PartitionOperationResponse operationResponse = new PartitionOperationResponse { Success = true };
      operationResponse.Message = $"TMDL exported for partition '{request.PartitionName}' in table '{request.TableName}'";
      operationResponse.Operation = request.Operation;
      operationResponse.TableName = request.TableName;
      operationResponse.PartitionName = request.PartitionName;
      operationResponse.Data = (object) tmdlExportResult;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      PartitionOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new PartitionOperationResponse()
      {
        Success = false,
        Message = "Failed to export TMDL for partition: " + ex.Message,
        Operation = request.Operation,
        TableName = request.TableName,
        PartitionName = request.PartitionName,
        Help = (object) operationMetadata
      };
    }
  }

  private PartitionOperationResponse HandleExportTMSLOperation(PartitionOperationRequest request)
  {
    try
    {
      TmslExportResult tmslExportResult = PartitionOperations.ExportTMSL(request.ConnectionName, request.TableName, request.PartitionName, request.TmslExportOptions);
      if (tmslExportResult.Success)
        this._logger.LogInformation("{ToolName}.{Operation} completed: Table={TableName}, Partition={PartitionName}, TmslType={TmslType}", (object) nameof (PartitionOperationsTool), (object) request.Operation, (object) request.TableName, (object) request.PartitionName, (object) request.TmslExportOptions.TmslOperationType);
      else
        this._logger.LogWarning("{ToolName}.{Operation} failed: Table={TableName}, Partition={PartitionName}, Message={Message}", (object) nameof (PartitionOperationsTool), (object) request.Operation, (object) request.TableName, (object) request.PartitionName, (object) tmslExportResult.ErrorMessage);
      PartitionOperationResponse operationResponse = new PartitionOperationResponse { Success = tmslExportResult.Success };
      string str;
      if (!tmslExportResult.Success)
        str = tmslExportResult.ErrorMessage ?? "Unknown error occurred";
      else
        str = $"TMSL {request.TmslExportOptions.TmslOperationType} script for partition '{request.PartitionName}' in table '{request.TableName}' generated successfully";
      operationResponse.Message = str;
      operationResponse.Operation = request.Operation;
      operationResponse.TableName = request.TableName;
      operationResponse.PartitionName = request.PartitionName;
      operationResponse.Data = (object) tmslExportResult;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      PartitionOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new PartitionOperationResponse()
      {
        Success = false,
        Message = "Error exporting partition TMSL: " + ex.Message,
        Operation = request.Operation,
        TableName = request.TableName,
        PartitionName = request.PartitionName,
        Help = (object) operationMetadata
      };
    }
  }

  private PartitionOperationResponse HandleHelpOperation(
    PartitionOperationRequest request,
    string[] operations)
  {
    this._logger.LogInformation("{ToolName}.{Operation} completed: Operations={OperationCount}", (object) nameof (PartitionOperationsTool), (object) request.Operation, (object) operations.Length);
    return new PartitionOperationResponse()
    {
      Success = true,
      Message = "Help information for partition operations",
      Operation = request.Operation,
      Help = (object) new
      {
        ToolName = "partition_operations",
        Description = "Perform operations on semantic model partitions.",
        SupportedOperations = operations,
        Examples = Enumerable.Where<KeyValuePair<string, OperationMetadata>>((IEnumerable<KeyValuePair<string, OperationMetadata>>) PartitionOperationsTool.toolMetadata.Operations, (Func<KeyValuePair<string, OperationMetadata>, bool>) (p => Enumerable.Contains<string>((IEnumerable<string>) operations, p.Key, (IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase))),
        Notes = new string[8]
        {
          "The ConnectionName parameter is optional and will use the last used connection if not provided.",
          "The Operation parameter specifies which operation to perform.",
          "The TableName parameter is required for most operations and specifies the table containing the partition.",
          "The PartitionName parameter is required for Get, Delete, Refresh, and Rename operations.",
          "The NewName parameter is required for Rename operation.",
          "The RefreshType parameter is required for Refresh operation and specifies the type of refresh to perform.",
          "The CreateDefinition parameter is required for Create operation and specifies the partition definition.",
          "The UpdateDefinition parameter is required for Update operation and specifies the partition update definition."
        }
      }
    };
  }

  private bool ValidateRequest(string operation, PartitionOperationRequest request)
  {
    OperationMetadata operationMetadata;
    if (!PartitionOperationsTool.toolMetadata.Operations.TryGetValue(operation, out operationMetadata))
      return true;
    JsonObject requestDict = JsonSerializer.SerializeToNode<PartitionOperationRequest>(request) as JsonObject;
    List<string> list1 = Enumerable.ToList<string>(Enumerable.Where<string>((IEnumerable<string>) operationMetadata.RequiredParams, (p => requestDict != null && requestDict[p] == null)));
    List<string> list2 = Enumerable.ToList<string>(Enumerable.Where<string>((IEnumerable<string>) operationMetadata.ForbiddenParams, (p => requestDict != null && requestDict[p] != null)));
    if (Enumerable.Any<string>((IEnumerable<string>) list1))
      throw new McpException($"Missing required parameters needed for {operation} operation: {string.Join(", ", (IEnumerable<string>) list1)}");
    if (Enumerable.Any<string>((IEnumerable<string>) list2))
      throw new McpException($"Forbidden parameters not allowed for {operation} operation: {string.Join(", ", (IEnumerable<string>) list2)}");
    return true;
  }

  static PartitionOperationsTool()
  {
    ToolMetadata toolMetadata1 = new ToolMetadata();
    ToolMetadata toolMetadata2 = toolMetadata1;
    Dictionary<string, OperationMetadata> dictionary1 = new Dictionary<string, OperationMetadata>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    Dictionary<string, OperationMetadata> dictionary2 = dictionary1;
    OperationMetadata operationMetadata1 = new OperationMetadata { Description = "List all partitions in the model or in a table.\r\nMandatory properties: None.\r\nOptional: TableName (for filtering by specific table)." };
    List<string> stringList1 = new List<string>();
    stringList1.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"List\",\r\n        \"TableName\": \"Sales\"\r\n    }\r\n}");
    operationMetadata1.ExampleRequests = stringList1;
    dictionary2["List"] = operationMetadata1;
    Dictionary<string, OperationMetadata> dictionary3 = dictionary1;
    OperationMetadata operationMetadata2 = new OperationMetadata { RequiredParams = new string[2]
    {
      "TableName",
      "PartitionName"
    } };
    operationMetadata2.Description = "Get details of a specific partition.\r\nMandatory properties: TableName, PartitionName.\r\nOptional: None.";
    OperationMetadata operationMetadata3 = operationMetadata2;
    List<string> stringList2 = new List<string>();
    stringList2.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Get\",\r\n        \"TableName\": \"Sales\",\r\n        \"PartitionName\": \"Partition1\"\r\n    }\r\n}");
    operationMetadata3.ExampleRequests = stringList2;
    OperationMetadata operationMetadata4 = operationMetadata2;
    dictionary3["Get"] = operationMetadata4;
    Dictionary<string, OperationMetadata> dictionary4 = dictionary1;
    OperationMetadata operationMetadata5 = new OperationMetadata { RequiredParams = new string[1]
    {
      "CreateDefinition"
    } };
    operationMetadata5.Description = "Create a new partition in a table.\r\n\r\nRequest-level properties:\r\n- Mandatory: CreateDefinition\r\n- Optional: ConnectionName, TableName (if not specified in CreateDefinition)\r\n\r\nCreateDefinition mandatory properties:\r\n- Name, SourceType, and source-specific properties\r\n- Either request TableName or CreateDefinition.TableName is required\r\n\r\nSource-specific mandatory properties in CreateDefinition:\r\n- Calculated: Expression\r\n- M: Expression  \r\n- Entity: EntityName, ExpressionSourceName or DataSourceName\r\n- PolicyRange: StartDateTime, EndDateTime\r\n- Query: Query, DataSourceName\r\n\r\nCreateDefinition optional properties:\r\n- TableName, Mode (defaults to Import), Description, QueryGroupName, Annotations, ExtendedProperties\r\n- Source-specific optional: RetainDataTillForceCalculate (Calculated), Attributes (M), Granularity/RefreshBookmark (PolicyRange), SchemaName (Entity)\r\n\r\nNote: For Entity partitions, you must provide either ExpressionSourceName or DataSourceName (but not both).";
    OperationMetadata operationMetadata6 = operationMetadata5;
    List<string> stringList3 = new List<string>();
    stringList3.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Create\",\r\n        \"TableName\": \"Sales\",\r\n        \"CreateDefinition\": { \r\n            \"TableName\": \"Sales\", \r\n            \"Name\": \"Partition1\",\r\n            \"Mode\": \"Import\",\r\n            \"SourceType\": \"M\",\r\n            \"Expression\": \"let Source = Sql.Databases(\\\"mydb.database.windows.net\\\"), MySource = Source{[Name=\\\"MySource\\\"]}[Data], dbo_MyTable = MySource{[Schema=\\\"dbo\\\",Item=\\\"MyTable\\\"]}[Data] in dbo_MyTable\",\r\n            \"QueryGroupName\": \"QueryGroup1\",\r\n        }\r\n    }\r\n}");
    operationMetadata6.ExampleRequests = stringList3;
    OperationMetadata operationMetadata7 = operationMetadata5;
    dictionary4["Create"] = operationMetadata7;
    Dictionary<string, OperationMetadata> dictionary5 = dictionary1;
    OperationMetadata operationMetadata8 = new OperationMetadata { RequiredParams = new string[1]
    {
      "UpdateDefinition"
    } };
    operationMetadata8.Description = "Update an existing partition. Names cannot be changed - use Rename operation instead.\r\n\r\nRequest-level properties:\r\n- Mandatory: UpdateDefinition\r\n- Optional: ConnectionName, TableName (if not specified in UpdateDefinition)\r\n\r\nUpdateDefinition mandatory properties:\r\n- Name\r\n- Either request TableName or UpdateDefinition.TableName is required\r\n\r\nUpdateDefinition optional properties:\r\n- TableName, Description, Mode, SourceType, QueryGroupName, Annotations, ExtendedProperties\r\n- Source-specific optional: Expression, Query, DataSourceName, StartDateTime, EndDateTime, Granularity, RefreshBookmark, RetainDataTillForceCalculate, Attributes, EntityName, SchemaName, ExpressionSourceName\r\n\r\nNote: Only properties that are specified will be updated. Null values are skipped, empty strings clear the property.\r\nFor Entity partitions: EntityName, SchemaName, ExpressionSourceName, and DataSourceName can all be updated independently.";
    OperationMetadata operationMetadata9 = operationMetadata8;
    List<string> stringList4 = new List<string>();
    stringList4.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Update\",\r\n        \"TableName\": \"Sales\",\r\n        \"UpdateDefinition\": { \r\n            \"TableName\": \"Sales\", \r\n            \"Name\": \"Partition1\",\r\n            \"Description\": \"Updated partition description\",\r\n            \"QueryGroupName\": \"QueryGroup2\"\r\n        }\r\n    }\r\n}");
    operationMetadata9.ExampleRequests = stringList4;
    OperationMetadata operationMetadata10 = operationMetadata8;
    dictionary5["Update"] = operationMetadata10;
    Dictionary<string, OperationMetadata> dictionary6 = dictionary1;
    OperationMetadata operationMetadata11 = new OperationMetadata { RequiredParams = new string[2]
    {
      "TableName",
      "PartitionName"
    } };
    operationMetadata11.Description = "Delete a partition from a table.\r\nMandatory properties: TableName, PartitionName.\r\nOptional: None.\r\nNote: Cannot delete the last partition in a table.";
    OperationMetadata operationMetadata12 = operationMetadata11;
    List<string> stringList5 = new List<string>();
    stringList5.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Delete\",\r\n        \"TableName\": \"Sales\",\r\n        \"PartitionName\": \"ObsoletePartition\"\r\n    }\r\n}");
    operationMetadata12.ExampleRequests = stringList5;
    OperationMetadata operationMetadata13 = operationMetadata11;
    dictionary6["Delete"] = operationMetadata13;
    Dictionary<string, OperationMetadata> dictionary7 = dictionary1;
    OperationMetadata operationMetadata14 = new OperationMetadata { RequiredParams = new string[2]
    {
      "TableName",
      "PartitionName"
    } };
    operationMetadata14.Description = "Refresh a partition in a table.\r\nMandatory properties: TableName, PartitionName.\r\nOptional: RefreshType (defaults to Automatic, valid values: Automatic, Full, ClearValues, Calculate, DataOnly, Defragment).";
    OperationMetadata operationMetadata15 = operationMetadata14;
    List<string> stringList6 = new List<string>();
    stringList6.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Refresh\",\r\n        \"TableName\": \"Sales\",\r\n        \"PartitionName\": \"Partition1\",\r\n        \"RefreshType\": \"Full\"\r\n    }\r\n}");
    operationMetadata15.ExampleRequests = stringList6;
    OperationMetadata operationMetadata16 = operationMetadata14;
    dictionary7["Refresh"] = operationMetadata16;
    Dictionary<string, OperationMetadata> dictionary8 = dictionary1;
    OperationMetadata operationMetadata17 = new OperationMetadata { RequiredParams = new string[3]
    {
      "TableName",
      "PartitionName",
      "NewName"
    } };
    operationMetadata17.Description = "Rename a partition.\r\nMandatory properties: TableName, PartitionName, NewName.\r\nOptional: None.";
    OperationMetadata operationMetadata18 = operationMetadata17;
    List<string> stringList7 = new List<string>();
    stringList7.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Rename\",\r\n        \"TableName\": \"Sales\",\r\n        \"PartitionName\": \"OldPartition\",\r\n        \"NewName\": \"NewPartition\"\r\n    }\r\n}");
    operationMetadata18.ExampleRequests = stringList7;
    OperationMetadata operationMetadata19 = operationMetadata17;
    dictionary8["Rename"] = operationMetadata19;
    Dictionary<string, OperationMetadata> dictionary9 = dictionary1;
    OperationMetadata operationMetadata20 = new OperationMetadata { RequiredParams = new string[2]
    {
      "TableName",
      "PartitionName"
    } };
    operationMetadata20.Description = "Export partition to TMDL (YAML-like syntax) format. TMDL is a human-readable, declarative format for semantic models.\r\nMandatory properties: TableName, PartitionName.\r\nOptional: TmdlExportOptions (with FormatTmdl, SaveToFile, FilePath, TruncateAfter).";
    OperationMetadata operationMetadata21 = operationMetadata20;
    List<string> stringList8 = new List<string>();
    stringList8.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"ExportTMDL\",\r\n        \"TableName\": \"Sales\",\r\n        \"PartitionName\": \"Partition1\"\r\n    }\r\n}");
    operationMetadata21.ExampleRequests = stringList8;
    OperationMetadata operationMetadata22 = operationMetadata20;
    dictionary9["ExportTMDL"] = operationMetadata22;
    Dictionary<string, OperationMetadata> dictionary10 = dictionary1;
    OperationMetadata operationMetadata23 = new OperationMetadata { RequiredParams = new string[3]
    {
      "TableName",
      "PartitionName",
      "TmslExportOptions"
    } };
    operationMetadata23.Description = "Export partition to TMSL (JSON syntax) script format with specified operation type. TMSL generates executable JSON scripts for partition operations.\r\nMandatory properties: TableName, PartitionName, TmslExportOptions (with TmslOperationType).\r\nOptional: TmslExportOptions properties (IncludeRestricted, RefreshType for Refresh operations, SaveToFile, FilePath, TruncateAfter).\r\nValid TmslOperationType values: Create, Delete, Refresh, Alter.";
    OperationMetadata operationMetadata24 = operationMetadata23;
    List<string> stringList9 = new List<string>();
    stringList9.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"ExportTMSL\",\r\n        \"TableName\": \"Sales\",\r\n        \"PartitionName\": \"Partition1\",\r\n        \"TmslExportOptions\": {\r\n            \"TmslOperationType\": \"Create\",\r\n            \"IncludeRestricted\": false\r\n        }\r\n    }\r\n}");
    stringList9.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"ExportTMSL\",\r\n        \"TableName\": \"Sales\",\r\n        \"PartitionName\": \"Partition1\",\r\n        \"TmslExportOptions\": {\r\n            \"TmslOperationType\": \"Refresh\",\r\n            \"RefreshType\": \"Full\"\r\n        }\r\n    }\r\n}");
    operationMetadata24.ExampleRequests = stringList9;
    OperationMetadata operationMetadata25 = operationMetadata23;
    dictionary10["ExportTMSL"] = operationMetadata25;
    Dictionary<string, OperationMetadata> dictionary11 = dictionary1;
    OperationMetadata operationMetadata26 = new OperationMetadata { Description = "Describe the tool and its operations.\r\nMandatory properties: None.\r\nOptional: None." };
    List<string> stringList10 = new List<string>();
    stringList10.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Help\"\r\n    }\r\n}");
    operationMetadata26.ExampleRequests = stringList10;
    dictionary11["Help"] = operationMetadata26;
    Dictionary<string, OperationMetadata> dictionary12 = dictionary1;
    toolMetadata2.Operations = dictionary12;
    PartitionOperationsTool.toolMetadata = toolMetadata1;
  }
}
