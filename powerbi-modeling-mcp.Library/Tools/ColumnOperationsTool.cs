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
public class ColumnOperationsTool
{
  private readonly ILogger<ColumnOperationsTool> _logger;
  public static readonly ToolMetadata toolMetadata;

  public ColumnOperationsTool(ILogger<ColumnOperationsTool> logger) => this._logger = logger;

  [McpServerTool(Name = "column_operations")]
  [Description("Perform operations on semantic model columns. Supported operations: Help, Create, Update, Delete, Get, List, Rename, ExportTMDL. Use the Operation parameter to specify which operation to perform. TableName is optional for the List operation and required for all other operations except Help. ColumnName is required for all operations except List and Help.")]
  public ColumnOperationResponse ExecuteColumnOperation(
    McpServer mcpServer,
    ColumnOperationRequest request)
  {
    this._logger.LogDebug("Executing {ToolName}.{Operation}: Table={TableName}, Column={ColumnName}, Connection={ConnectionName}", (object) nameof (ColumnOperationsTool), (object) request.Operation, (object) request.TableName, (object) request.ColumnName, (object) (request.ConnectionName ?? "(last used)"));
    try
    {
      string[] strArray1 = new string[8]
      {
        "CREATE",
        "UPDATE",
        "DELETE",
        "GET",
        "LIST",
        "RENAME",
        "EXPORTTMDL",
        "HELP"
      };
      string[] strArray2 = new string[4]
      {
        "CREATE",
        "UPDATE",
        "DELETE",
        "RENAME"
      };
      string upperInvariant = request.Operation.ToUpperInvariant();
      if (!Enumerable.Contains<string>((IEnumerable<string>) strArray1, upperInvariant))
      {
        this._logger.LogWarning("Invalid operation '{Operation}' requested for {ToolName}. Valid operations: {ValidOperations}", (object) request.Operation, (object) nameof (ColumnOperationsTool), (object) string.Join(", ", strArray1));
        return ColumnOperationResponse.Forbidden(request.Operation, $"Invalid operation: {request.Operation}. Supported operations: {string.Join(", ", strArray1)}", request.TableName, request.ColumnName);
      }
      if (!this.ValidateRequest(request.Operation, request))
        throw new McpException($"Invalid request for {request.Operation} operation.");
      if (Enumerable.Contains<string>((IEnumerable<string>) strArray2, upperInvariant))
      {
        WriteOperationResult writeOperationResult = WriteGuard.ExecuteWriteOperationWithGuards(mcpServer, request.ConnectionName, request.Operation);
        if (!writeOperationResult.Success)
        {
          this._logger.LogWarning("{ToolName}.{Operation} blocked by write guard: {Reason}", (object) nameof (ColumnOperationsTool), (object) request.Operation, (object) writeOperationResult.Message);
          return ColumnOperationResponse.Forbidden(request.Operation, writeOperationResult.Message, request.TableName, request.ColumnName);
        }
      }
      bool allowed = WriteGuard.IsWriteAllowed("").allowed;
      ColumnOperationResponse operationResponse;
      if (upperInvariant != null)
      {
        switch (upperInvariant.Length)
        {
          case 3:
            if ((upperInvariant == "GET"))
            {
              operationResponse = this.HandleGetOperation(request);
              goto label_29;
            }
            break;
          case 4:
            switch (upperInvariant[0])
            {
              case 'H':
                if ((upperInvariant == "HELP"))
                {
                  operationResponse = this.HandleHelpOperation(request, allowed ? strArray1 : Enumerable.ToArray<string>(Enumerable.Except<string>((IEnumerable<string>) strArray1, (IEnumerable<string>) strArray2)));
                  goto label_29;
                }
                break;
              case 'L':
                if ((upperInvariant == "LIST"))
                {
                  operationResponse = this.HandleListOperation(request);
                  goto label_29;
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
                  goto label_29;
                }
                break;
              case 'D':
                if ((upperInvariant == "DELETE"))
                {
                  operationResponse = this.HandleDeleteOperation(request);
                  goto label_29;
                }
                break;
              case 'R':
                if ((upperInvariant == "RENAME"))
                {
                  operationResponse = this.HandleRenameOperation(request);
                  goto label_29;
                }
                break;
              case 'U':
                if ((upperInvariant == "UPDATE"))
                {
                  operationResponse = this.HandleUpdateOperation(request);
                  goto label_29;
                }
                break;
            }
            break;
          case 10:
            if ((upperInvariant == "EXPORTTMDL"))
            {
              operationResponse = this.HandleExportTMDLOperation(request);
              goto label_29;
            }
            break;
        }
      }
      operationResponse = ColumnOperationResponse.Forbidden(request.Operation, $"Operation {request.Operation} is not implemented", request.TableName, request.ColumnName);
label_29:
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Error executing {ToolName}.{Operation}: {ErrorMessage}", (object) nameof (ColumnOperationsTool), (object) request.Operation, (object) ex.Message);
      return new ColumnOperationResponse()
      {
        Success = false,
        Message = "Error executing column operation: " + ex.Message,
        Operation = request.Operation,
        TableName = request.TableName,
        ColumnName = request.ColumnName
      };
    }
  }

  private ColumnOperationResponse HandleCreateOperation(ColumnOperationRequest request)
  {
    try
    {
      if (string.IsNullOrEmpty(request.CreateDefinition.TableName))
        request.CreateDefinition.TableName = request.TableName;
      else if ((request.CreateDefinition.TableName != request.TableName))
        throw new McpException($"Table name mismatch: Request specifies '{request.TableName}' but CreateDefinition specifies '{request.CreateDefinition.TableName}'");
      if (string.IsNullOrEmpty(request.CreateDefinition.Name))
        request.CreateDefinition.Name = request.ColumnName;
      else if (!string.IsNullOrEmpty(request.ColumnName) && (request.CreateDefinition.Name != request.ColumnName))
        throw new McpException($"Column name mismatch: Request specifies '{request.ColumnName}' but CreateDefinition specifies '{request.CreateDefinition.Name}'");
      ColumnOperations.ColumnOperationResult column = ColumnOperations.CreateColumn(request.ConnectionName, request.CreateDefinition);
      ConnectionInfo connectionInfo = ConnectionOperations.Get(request.ConnectionName);
      string[] strArray = new string[3]
      {
        "Ready",
        "NoData",
        "CalculationNeeded"
      };
      bool flag = Enumerable.Contains<string>((IEnumerable<string>) strArray, column.State);
      List<string> stringList = new List<string>();
      if ((column.State == "CalculationNeeded") && !connectionInfo.IsOffline)
        stringList.Add($"Column '{column.ColumnName}' has been successfully created but requires data refresh to calculate values.");
      else if ((column.State == "NoData") && !connectionInfo.IsOffline)
        stringList.Add($"Column '{column.ColumnName}' is in NoData state. Use refresh operations to load data.");
      else if (!Enumerable.Contains<string>((IEnumerable<string>) strArray, column.State) && !string.IsNullOrEmpty(column.ErrorMessage))
        stringList.Add(column.ErrorMessage);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Table={TableName}, Column={ColumnName}, State={State}", (object) nameof (ColumnOperationsTool), (object) "Create", (object) request.TableName, (object) request.CreateDefinition.Name, (object) column.State);
      if (Enumerable.Any<string>((IEnumerable<string>) stringList))
      {
        foreach (string str in stringList)
          this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (ColumnOperationsTool), (object) request.Operation, (object) str);
      }
      ColumnOperationResponse operation = new ColumnOperationResponse { Success = flag };
      string str1;
      if (!flag)
        str1 = column.ErrorMessage ?? $"Failed to create column '{request.CreateDefinition.Name}'";
      else
        str1 = $"Column '{request.CreateDefinition.Name}' created successfully in table '{request.TableName}'";
      operation.Message = str1;
      operation.Operation = request.Operation;
      operation.TableName = request.TableName;
      operation.ColumnName = request.CreateDefinition.Name;
      operation.Data = (object) column;
      operation.Warnings = stringList;
      return operation;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      ColumnOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new ColumnOperationResponse()
      {
        Success = false,
        Message = "Error executing Create operation: " + ex.Message,
        Operation = request.Operation,
        TableName = request.TableName,
        ColumnName = request.ColumnName,
        Help = (object) operationMetadata
      };
    }
  }

  private ColumnOperationResponse HandleUpdateOperation(ColumnOperationRequest request)
  {
    try
    {
      if (string.IsNullOrEmpty(request.UpdateDefinition.TableName))
        request.UpdateDefinition.TableName = request.TableName;
      else if ((request.UpdateDefinition.TableName != request.TableName))
        throw new McpException($"Table name mismatch: Request specifies '{request.TableName}' but UpdateDefinition specifies '{request.UpdateDefinition.TableName}'");
      if (string.IsNullOrEmpty(request.UpdateDefinition.Name))
        request.UpdateDefinition.Name = request.ColumnName;
      else if (!string.IsNullOrEmpty(request.ColumnName) && (request.UpdateDefinition.Name != request.ColumnName))
        throw new McpException($"Column name mismatch: Request specifies '{request.ColumnName}' but UpdateDefinition specifies '{request.UpdateDefinition.Name}'");
      ColumnOperations.ColumnOperationResult columnOperationResult = ColumnOperations.UpdateColumn(request.ConnectionName, request.UpdateDefinition);
      ConnectionInfo connectionInfo = ConnectionOperations.Get(request.ConnectionName);
      string[] strArray = new string[3]
      {
        "Ready",
        "NoData",
        "CalculationNeeded"
      };
      bool flag = Enumerable.Contains<string>((IEnumerable<string>) strArray, columnOperationResult.State);
      List<string> stringList = new List<string>();
      if ((columnOperationResult.State == "CalculationNeeded") && !connectionInfo.IsOffline)
        stringList.Add($"Column '{columnOperationResult.ColumnName}' has been successfully updated but requires data refresh to calculate values.");
      else if ((columnOperationResult.State == "NoData") && !connectionInfo.IsOffline)
        stringList.Add($"Column '{columnOperationResult.ColumnName}' is in NoData state. Use refresh operations to load data.");
      else if (!Enumerable.Contains<string>((IEnumerable<string>) strArray, columnOperationResult.State) && !string.IsNullOrEmpty(columnOperationResult.ErrorMessage))
        stringList.Add(columnOperationResult.ErrorMessage);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Table={TableName}, Column={ColumnName}, State={State}", (object) nameof (ColumnOperationsTool), (object) "Update", (object) request.TableName, (object) request.UpdateDefinition.Name, (object) columnOperationResult.State);
      if (Enumerable.Any<string>((IEnumerable<string>) stringList))
      {
        foreach (string str in stringList)
          this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (ColumnOperationsTool), (object) request.Operation, (object) str);
      }
      ColumnOperationResponse operationResponse = new ColumnOperationResponse { Success = flag };
      string str1;
      if (!flag)
        str1 = columnOperationResult.ErrorMessage ?? $"Failed to update column '{request.UpdateDefinition.Name}'";
      else
        str1 = $"Column '{request.UpdateDefinition.Name}' updated successfully in table '{request.TableName}'";
      operationResponse.Message = str1;
      operationResponse.Operation = request.Operation;
      operationResponse.TableName = request.TableName;
      operationResponse.ColumnName = request.UpdateDefinition.Name;
      operationResponse.Data = (object) columnOperationResult;
      operationResponse.Warnings = stringList;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      ColumnOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new ColumnOperationResponse()
      {
        Success = false,
        Message = "Error executing Update operation: " + ex.Message,
        Operation = request.Operation,
        TableName = request.TableName,
        ColumnName = request.ColumnName,
        Help = (object) operationMetadata
      };
    }
  }

  private ColumnOperationResponse HandleDeleteOperation(ColumnOperationRequest request)
  {
    try
    {
      List<string> stringList = ColumnOperations.DeleteColumn(request.ConnectionName, request.TableName, request.ColumnName, request.ShouldCascadeDelete.Value);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Table={TableName}, Column={ColumnName}, Cascade={Cascade}", (object) nameof (ColumnOperationsTool), (object) "Delete", (object) request.TableName, (object) request.ColumnName, (object) request.ShouldCascadeDelete);
      if (stringList != null && Enumerable.Any<string>((IEnumerable<string>) stringList))
      {
        foreach (string str in stringList)
          this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (ColumnOperationsTool), (object) request.Operation, (object) str);
      }
      ColumnOperationResponse operationResponse = new ColumnOperationResponse { Success = true };
      operationResponse.Message = $"Column '{request.ColumnName}' deleted successfully from table '{request.TableName}'";
      operationResponse.Operation = request.Operation;
      operationResponse.TableName = request.TableName;
      operationResponse.ColumnName = request.ColumnName;
      operationResponse.Warnings = stringList == null || !Enumerable.Any<string>((IEnumerable<string>) stringList) ? (List<string>) null : stringList;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      ColumnOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new ColumnOperationResponse()
      {
        Success = false,
        Message = "Error executing Delete operation: " + ex.Message,
        Operation = request.Operation,
        TableName = request.TableName,
        ColumnName = request.ColumnName,
        Help = (object) operationMetadata
      };
    }
  }

  private ColumnOperationResponse HandleGetOperation(ColumnOperationRequest request)
  {
    try
    {
      ColumnGet column = ColumnOperations.GetColumn(request.ConnectionName, request.TableName, request.ColumnName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Table={TableName}, Column={ColumnName}", (object) nameof (ColumnOperationsTool), (object) "Get", (object) request.TableName, (object) request.ColumnName);
      ColumnOperationResponse operation = new ColumnOperationResponse { Success = true };
      operation.Message = $"Column '{request.ColumnName}' retrieved successfully from table '{request.TableName}'";
      operation.Operation = request.Operation;
      operation.TableName = request.TableName;
      operation.ColumnName = request.ColumnName;
      operation.Data = (object) column;
      return operation;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      ColumnOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new ColumnOperationResponse()
      {
        Success = false,
        Message = "Error executing Get operation: " + ex.Message,
        Operation = request.Operation,
        TableName = request.TableName,
        ColumnName = request.ColumnName,
        Help = (object) operationMetadata
      };
    }
  }

  private ColumnOperationResponse HandleListOperation(ColumnOperationRequest request)
  {
    try
    {
      int totalCount;
      List<TableColumnList> tableColumnListList = ColumnOperations.ListColumns(request.ConnectionName, request.TableName, request.MaxResults, out totalCount);
      int num = Enumerable.Sum<TableColumnList>((IEnumerable<TableColumnList>) tableColumnListList, (t => t.Columns.Count));
      bool flag = request.MaxResults.HasValue && totalCount > request.MaxResults.Value;
      List<string> stringList = new List<string>();
      string str;
      if (string.IsNullOrWhiteSpace(request.TableName))
      {
        str = $"Found {num} columns across {tableColumnListList.Count} tables";
        if (flag)
          stringList.Add($"Results truncated: Showing {num} of {totalCount} columns (limited by MaxResults={request.MaxResults})");
      }
      else
      {
        str = $"Found {num} columns in table '{request.TableName}'";
        if (flag)
          stringList.Add($"Results truncated: Showing {num} of {totalCount} columns (limited by MaxResults={request.MaxResults})");
      }
      this._logger.LogInformation("{ToolName}.{Operation} completed: Table={TableName}, TotalCount={TotalCount}, ReturnedCount={Count}, IsTruncated={IsTruncated}", (object) nameof (ColumnOperationsTool), (object) "List", (object) (request.TableName ?? "(all tables)"), (object) totalCount, (object) num, (object) flag);
      return new ColumnOperationResponse()
      {
        Success = true,
        Message = str,
        Operation = request.Operation,
        TableName = request.TableName,
        Data = (object) tableColumnListList,
        Warnings = Enumerable.Any<string>((IEnumerable<string>) stringList) ? stringList : (List<string>) null
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      ColumnOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new ColumnOperationResponse()
      {
        Success = false,
        Message = "Error executing List operation: " + ex.Message,
        Operation = request.Operation,
        TableName = request.TableName,
        Help = (object) operationMetadata
      };
    }
  }

  private ColumnOperationResponse HandleRenameOperation(ColumnOperationRequest request)
  {
    try
    {
      if (string.IsNullOrEmpty(request.RenameDefinition.CurrentName))
        request.RenameDefinition.CurrentName = !string.IsNullOrEmpty(request.ColumnName) ? request.ColumnName : throw new McpException("Either ColumnName or RenameDefinition.CurrentName is required.");
      if (string.IsNullOrEmpty(request.RenameDefinition.TableName))
        request.RenameDefinition.TableName = !string.IsNullOrEmpty(request.TableName) ? request.TableName : throw new McpException("Either TableName or RenameDefinition.TableName is required.");
      ColumnOperations.RenameColumn(request.ConnectionName, request.RenameDefinition.TableName, request.RenameDefinition.CurrentName, request.RenameDefinition.NewName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Table={TableName}, From={OldName}, To={NewName}", (object) nameof (ColumnOperationsTool), (object) "Rename", (object) request.RenameDefinition.TableName, (object) request.RenameDefinition.CurrentName, (object) request.RenameDefinition.NewName);
      ColumnOperationResponse operationResponse = new ColumnOperationResponse { Success = true };
      operationResponse.Message = $"Column '{request.RenameDefinition.CurrentName}' renamed to '{request.RenameDefinition.NewName}' successfully in table '{request.RenameDefinition.TableName}'";
      operationResponse.Operation = request.Operation;
      operationResponse.TableName = request.RenameDefinition.TableName;
      operationResponse.ColumnName = request.RenameDefinition.NewName;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      ColumnOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new ColumnOperationResponse()
      {
        Success = false,
        Message = "Error executing Rename operation: " + ex.Message,
        Operation = request.Operation,
        TableName = request.TableName,
        ColumnName = request.ColumnName,
        Help = (object) operationMetadata
      };
    }
  }

  private ColumnOperationResponse HandleExportTMDLOperation(ColumnOperationRequest request)
  {
    try
    {
      string str = ColumnOperations.ExportTMDL(request.ConnectionName, request.TableName, request.ColumnName, (ExportTmdl) request.TmdlExportOptions);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Table={TableName}, Column={ColumnName}", (object) nameof (ColumnOperationsTool), (object) "ExportTMDL", (object) request.TableName, (object) request.ColumnName);
      ColumnOperationResponse operationResponse = new ColumnOperationResponse { Success = true };
      operationResponse.Message = $"TMDL for column '{request.ColumnName}' in table '{request.TableName}' exported successfully";
      operationResponse.Operation = request.Operation;
      operationResponse.TableName = request.TableName;
      operationResponse.ColumnName = request.ColumnName;
      operationResponse.Data = (object) str;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      ColumnOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      ColumnOperationResponse operationResponse = new ColumnOperationResponse { Success = false };
      operationResponse.Message = $"Error getting TMDL for column '{request.ColumnName}' in table '{request.TableName}': {ex.Message}";
      operationResponse.Operation = request.Operation;
      operationResponse.TableName = request.TableName;
      operationResponse.ColumnName = request.ColumnName;
      operationResponse.Help = (object) operationMetadata;
      return operationResponse;
    }
  }

  private ColumnOperationResponse HandleHelpOperation(
    ColumnOperationRequest request,
    string[] operations)
  {
    this._logger.LogInformation("{ToolName}.{Operation} completed: Operations={OperationCount}", (object) nameof (ColumnOperationsTool), (object) "Help", (object) operations.Length);
    return new ColumnOperationResponse()
    {
      Success = true,
      Message = "Tool description retrieved successfully",
      Operation = request.Operation,
      Help = (object) new
      {
        ToolName = "column_operations",
        Description = "Perform operations on semantic model columns.",
        SupportedOperations = operations,
        Examples = Enumerable.Where<KeyValuePair<string, OperationMetadata>>((IEnumerable<KeyValuePair<string, OperationMetadata>>) ColumnOperationsTool.toolMetadata.Operations, (Func<KeyValuePair<string, OperationMetadata>, bool>) (p => Enumerable.Contains<string>((IEnumerable<string>) operations, p.Key, (IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase))),
        Notes = new string[5]
        {
          "Use the Operation parameter to specify which operation to perform.",
          "TableName is required for all operations except for Help.",
          "ColumnName is required for all operations except List.",
          "NewColumnName is required for Rename operation.",
          "If the request is declined by the user, the operation should be aborted."
        }
      }
    };
  }

  private bool ValidateRequest(string operation, ColumnOperationRequest request)
  {
    OperationMetadata operationMetadata;
    if (!ColumnOperationsTool.toolMetadata.Operations.TryGetValue(operation, out operationMetadata))
      return true;
    JsonObject requestDict = JsonSerializer.SerializeToNode<ColumnOperationRequest>(request) as JsonObject;
    List<string> list1 = Enumerable.ToList<string>(Enumerable.Where<string>((IEnumerable<string>) operationMetadata.RequiredParams, (p => requestDict != null && requestDict[p] == null)));
    List<string> list2 = Enumerable.ToList<string>(Enumerable.Where<string>((IEnumerable<string>) operationMetadata.ForbiddenParams, (p => requestDict != null && requestDict[p] != null)));
    if (Enumerable.Any<string>((IEnumerable<string>) list1))
      throw new McpException($"Missing required parameters needed for {operation} operation: {string.Join(", ", (IEnumerable<string>) list1)}");
    if (Enumerable.Any<string>((IEnumerable<string>) list2))
      throw new McpException($"Forbidden parameters not allowed for {operation} operation: {string.Join(", ", (IEnumerable<string>) list2)}");
    return true;
  }

  static ColumnOperationsTool()
  {
    ToolMetadata toolMetadata1 = new ToolMetadata();
    ToolMetadata toolMetadata2 = toolMetadata1;
    Dictionary<string, OperationMetadata> dictionary1 = new Dictionary<string, OperationMetadata>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    Dictionary<string, OperationMetadata> dictionary2 = dictionary1;
    OperationMetadata operationMetadata1 = new OperationMetadata { RequiredParams = new string[1]
    {
      "CreateDefinition"
    } };
    operationMetadata1.Description = "Create a new column in a table. \r\nMandatory properties: CreateDefinition (with Name, TableName, and either Expression or SourceColumn). \r\nOptional: DataType, DataCategory, FormatString, SummarizeBy, DefaultLabel, DefaultImage, IsHidden, IsUnique, IsKey, IsNullable, DisplayFolder, SortByColumn, SourceProviderType, Description, IsAvailableInMDX, Alignment, TableDetailPosition, Annotations, ExtendedProperties, AlternateOf, GroupByColumns.";
    OperationMetadata operationMetadata2 = operationMetadata1;
    List<string> stringList1 = new List<string>();
    stringList1.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Create\",\r\n        \"TableName\": \"Sales\",\r\n        \"ColumnName\": \"Order Year\",\r\n        \"CreateDefinition\": { \r\n            \"TableName\": \"Sales\", \r\n            \"Name\": \"Order Year\", \r\n            \"Expression\": \"YEAR([Order Date])\" \r\n        }\r\n    }\r\n}");
    operationMetadata2.ExampleRequests = stringList1;
    OperationMetadata operationMetadata3 = operationMetadata1;
    dictionary2["Create"] = operationMetadata3;
    Dictionary<string, OperationMetadata> dictionary3 = dictionary1;
    OperationMetadata operationMetadata4 = new OperationMetadata { RequiredParams = new string[1]
    {
      "UpdateDefinition"
    } };
    operationMetadata4.Description = "Update an existing column. Names cannot be changed and must use the Rename operation instead. \r\nMandatory properties: UpdateDefinition (with Name, TableName). \r\nOptional: Expression, SourceColumn, DataType, DataCategory, FormatString, SummarizeBy, DefaultLabel, DefaultImage, IsHidden, IsUnique, IsKey, IsNullable, DisplayFolder, SortByColumn, SourceProviderType, Description, IsAvailableInMDX, Alignment, TableDetailPosition, Annotations, ExtendedProperties, AlternateOf, GroupByColumns.";
    OperationMetadata operationMetadata5 = operationMetadata4;
    List<string> stringList2 = new List<string>();
    stringList2.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Update\",\r\n        \"TableName\": \"Sales\",\r\n        \"ColumnName\": \"Order Year\",\r\n        \"UpdateDefinition\": { \r\n            \"TableName\": \"Sales\", \r\n            \"Name\": \"Order Year\", \r\n            \"Expression\": \"YEAR([Order Date])\" \r\n        }\r\n    }\r\n}");
    operationMetadata5.ExampleRequests = stringList2;
    OperationMetadata operationMetadata6 = operationMetadata4;
    dictionary3["Update"] = operationMetadata6;
    Dictionary<string, OperationMetadata> dictionary4 = dictionary1;
    OperationMetadata operationMetadata7 = new OperationMetadata { RequiredParams = new string[3]
    {
      "TableName",
      "ColumnName",
      "ShouldCascadeDelete"
    } };
    operationMetadata7.Description = "Delete a column from a table. \r\nMandatory properties: TableName, ColumnName, ShouldCascadeDelete. \r\nOptional: None.";
    OperationMetadata operationMetadata8 = operationMetadata7;
    List<string> stringList3 = new List<string>();
    stringList3.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Delete\",\r\n        \"TableName\": \"Sales\",\r\n        \"ColumnName\": \"ObsoleteColumn\",\r\n        \"ShouldCascadeDelete\": true\r\n    }\r\n}");
    operationMetadata8.ExampleRequests = stringList3;
    OperationMetadata operationMetadata9 = operationMetadata7;
    dictionary4["Delete"] = operationMetadata9;
    Dictionary<string, OperationMetadata> dictionary5 = dictionary1;
    OperationMetadata operationMetadata10 = new OperationMetadata { RequiredParams = new string[2]
    {
      "TableName",
      "ColumnName"
    } };
    operationMetadata10.Description = "Get details of a specific column. \r\nMandatory properties: TableName, ColumnName. \r\nOptional: None.";
    OperationMetadata operationMetadata11 = operationMetadata10;
    List<string> stringList4 = new List<string>();
    stringList4.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Get\",\r\n        \"TableName\": \"Sales\",\r\n        \"ColumnName\": \"Region\"\r\n    }\r\n}");
    operationMetadata11.ExampleRequests = stringList4;
    OperationMetadata operationMetadata12 = operationMetadata10;
    dictionary5["Get"] = operationMetadata12;
    Dictionary<string, OperationMetadata> dictionary6 = dictionary1;
    OperationMetadata operationMetadata13 = new OperationMetadata { RequiredParams = Array.Empty<string>() };
    operationMetadata13.Description = "List all columns in a table, or all columns in all tables if TableName is not provided. \r\nMandatory properties: None. \r\nOptional: TableName (if omitted, returns columns from all tables).";
    List<string> stringList5 = new List<string>();
    stringList5.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"List\",\r\n        \"TableName\": \"Sales\"\r\n    }\r\n}");
    stringList5.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"List\"\r\n    }\r\n}");
    operationMetadata13.ExampleRequests = stringList5;
    dictionary6["List"] = operationMetadata13;
    Dictionary<string, OperationMetadata> dictionary7 = dictionary1;
    OperationMetadata operationMetadata14 = new OperationMetadata { RequiredParams = new string[1]
    {
      "RenameDefinition"
    } };
    operationMetadata14.Description = "Rename a column. \r\nMandatory properties: RenameDefinition (with NewName and either CurrentName or fallback from ColumnName, and either TableName or fallback from request TableName). \r\nOptional: None.";
    OperationMetadata operationMetadata15 = operationMetadata14;
    List<string> stringList6 = new List<string>();
    stringList6.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Rename\",\r\n        \"RenameDefinition\": { \r\n            \"CurrentName\": \"OldColumnName\", \r\n            \"NewName\": \"NewColumnName\",\r\n            \"TableName\": \"Sales\"\r\n        }\r\n    }\r\n}");
    operationMetadata15.ExampleRequests = stringList6;
    OperationMetadata operationMetadata16 = operationMetadata14;
    dictionary7["Rename"] = operationMetadata16;
    Dictionary<string, OperationMetadata> dictionary8 = dictionary1;
    OperationMetadata operationMetadata17 = new OperationMetadata { RequiredParams = new string[2]
    {
      "TableName",
      "ColumnName"
    } };
    operationMetadata17.Description = "Export column to TMDL format. \r\nMandatory properties: TableName, ColumnName. \r\nOptional: TmdlExportOptions.";
    OperationMetadata operationMetadata18 = operationMetadata17;
    List<string> stringList7 = new List<string>();
    stringList7.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"ExportTMDL\",\r\n        \"TableName\": \"Sales\",\r\n        \"ColumnName\": \"Region\"\r\n    }\r\n}");
    operationMetadata18.ExampleRequests = stringList7;
    OperationMetadata operationMetadata19 = operationMetadata17;
    dictionary8["ExportTMDL"] = operationMetadata19;
    Dictionary<string, OperationMetadata> dictionary9 = dictionary1;
    OperationMetadata operationMetadata20 = new OperationMetadata { Description = "Describe the tool and its operations. \r\nMandatory properties: None. \r\nOptional: None." };
    List<string> stringList8 = new List<string>();
    stringList8.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Help\"\r\n    }\r\n}");
    operationMetadata20.ExampleRequests = stringList8;
    dictionary9["Help"] = operationMetadata20;
    Dictionary<string, OperationMetadata> dictionary10 = dictionary1;
    toolMetadata2.Operations = dictionary10;
    ColumnOperationsTool.toolMetadata = toolMetadata1;
  }
}
