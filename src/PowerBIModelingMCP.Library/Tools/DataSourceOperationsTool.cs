// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Tools.DataSourceOperationsTool
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
public class DataSourceOperationsTool
{
  private readonly ILogger<DataSourceOperationsTool> _logger;
  public static readonly ToolMetadata toolMetadata;

  public DataSourceOperationsTool(ILogger<DataSourceOperationsTool> logger)
  {
    this._logger = logger;
  }

  [McpServerTool(Name = "data_source_operations")]
  [Description("Perform operations on SSAS data sources in semantic models. Supported operations: Help, Create, Update, Delete, Get, List, Rename, Test, ExportTMDL. Use the Operation parameter to specify which operation to perform. ExportTMDL exports an SSAS data source to TMDL format. Note: Power BI uses named expressions instead of data sources.")]
  public DataSourceOperationResponse ExecuteDataSourceOperation(
    McpServer mcpServer,
    DataSourceOperationRequest request)
  {
    this._logger.LogDebug("Executing {ToolName}.{Operation}: DataSource={DataSourceName}, Connection={ConnectionName}", (object) nameof (DataSourceOperationsTool), (object) request.Operation, (object) request.DataSourceName, (object) (request.ConnectionName ?? "(last used)"));
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
        "TEST",
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
      string upperInvariant1 = request.Operation.ToUpperInvariant();
      if (!Enumerable.Contains<string>((IEnumerable<string>) strArray1, upperInvariant1))
      {
        this._logger.LogWarning("Invalid operation '{Operation}' requested for {ToolName}. Valid operations: {ValidOperations}", (object) request.Operation, (object) nameof (DataSourceOperationsTool), (object) string.Join(", ", strArray1));
        return DataSourceOperationResponse.Forbidden(request.Operation, $"Invalid operation: {request.Operation}. Supported operations: {string.Join(", ", strArray1)}");
      }
      if (!this.ValidateRequest(request.Operation, request))
        throw new McpException($"Invalid request for {request.Operation} operation.");
      if (Enumerable.Contains<string>((IEnumerable<string>) strArray2, request.Operation.ToUpperInvariant()))
      {
        WriteOperationResult writeOperationResult = WriteGuard.ExecuteWriteOperationWithGuards(mcpServer, request.ConnectionName, request.Operation);
        if (!writeOperationResult.Success)
        {
          this._logger.LogWarning("{ToolName}.{Operation} blocked by write guard: {Reason}", (object) nameof (DataSourceOperationsTool), (object) request.Operation, (object) writeOperationResult.Message);
          return DataSourceOperationResponse.Forbidden(request.Operation, writeOperationResult.Message);
        }
      }
      bool allowed = WriteGuard.IsWriteAllowed("").allowed;
      string upperInvariant2 = request.Operation.ToUpperInvariant();
      DataSourceOperationResponse operationResponse;
      if (upperInvariant2 != null)
      {
        switch (upperInvariant2.Length)
        {
          case 3:
            if ((upperInvariant2 == "GET"))
            {
              operationResponse = this.HandleGetOperation(request);
              goto label_31;
            }
            break;
          case 4:
            switch (upperInvariant2[0])
            {
              case 'H':
                if ((upperInvariant2 == "HELP"))
                {
                  operationResponse = this.HandleHelpOperation(request, allowed ? strArray1 : Enumerable.ToArray<string>(Enumerable.Except<string>((IEnumerable<string>) strArray1, (IEnumerable<string>) strArray2)));
                  goto label_31;
                }
                break;
              case 'L':
                if ((upperInvariant2 == "LIST"))
                {
                  operationResponse = this.HandleListOperation(request);
                  goto label_31;
                }
                break;
              case 'T':
                if ((upperInvariant2 == "TEST"))
                {
                  operationResponse = this.HandleTestOperation(request);
                  goto label_31;
                }
                break;
            }
            break;
          case 6:
            switch (upperInvariant2[0])
            {
              case 'C':
                if ((upperInvariant2 == "CREATE"))
                {
                  operationResponse = this.HandleCreateOperation(request);
                  goto label_31;
                }
                break;
              case 'D':
                if ((upperInvariant2 == "DELETE"))
                {
                  operationResponse = this.HandleDeleteOperation(request);
                  goto label_31;
                }
                break;
              case 'R':
                if ((upperInvariant2 == "RENAME"))
                {
                  operationResponse = this.HandleRenameOperation(request);
                  goto label_31;
                }
                break;
              case 'U':
                if ((upperInvariant2 == "UPDATE"))
                {
                  operationResponse = this.HandleUpdateOperation(request);
                  goto label_31;
                }
                break;
            }
            break;
          case 10:
            if ((upperInvariant2 == "EXPORTTMDL"))
            {
              operationResponse = this.HandleExportTMDLOperation(request);
              goto label_31;
            }
            break;
        }
      }
      operationResponse = DataSourceOperationResponse.Forbidden(request.Operation, $"Operation {request.Operation} not implemented", request.DataSourceName);
label_31:
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Error executing {ToolName}.{Operation}: {ErrorMessage}", (object) nameof (DataSourceOperationsTool), (object) request.Operation, (object) ex.Message);
      return new DataSourceOperationResponse()
      {
        Success = false,
        Message = "Error executing data source operation: " + ex.Message,
        Operation = request.Operation,
        DataSourceName = request.DataSourceName
      };
    }
  }

  private DataSourceOperationResponse HandleCreateOperation(DataSourceOperationRequest request)
  {
    try
    {
      if (!string.IsNullOrEmpty(request.DataSourceName))
      {
        if (string.IsNullOrEmpty(request.CreateDefinition.Name))
          request.CreateDefinition.Name = request.DataSourceName;
        else if ((request.CreateDefinition.Name != request.DataSourceName))
          throw new McpException($"Data source name mismatch: Request specifies '{request.DataSourceName}' but CreateDefinition specifies '{request.CreateDefinition.Name}'");
      }
      OperationResult dataSource = DataSourceOperations.CreateDataSource(request.ConnectionName, request.CreateDefinition);
      this._logger.LogInformation("{ToolName}.{Operation} completed: DataSource={DataSourceName}", (object) nameof (DataSourceOperationsTool), (object) request.Operation, (object) request.CreateDefinition.Name);
      return new DataSourceOperationResponse()
      {
        Success = true,
        Message = $"Data source '{request.CreateDefinition.Name}' created successfully",
        Operation = request.Operation,
        DataSourceName = request.CreateDefinition.Name,
        Data = (object) dataSource
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      DataSourceOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new DataSourceOperationResponse()
      {
        Success = false,
        Message = "Error creating data source: " + ex.Message,
        Operation = request.Operation,
        DataSourceName = request.DataSourceName,
        Help = (object) operationMetadata
      };
    }
  }

  private DataSourceOperationResponse HandleUpdateOperation(DataSourceOperationRequest request)
  {
    try
    {
      if (!string.IsNullOrEmpty(request.DataSourceName))
      {
        if (string.IsNullOrEmpty(request.UpdateDefinition.Name))
          request.UpdateDefinition.Name = request.DataSourceName;
        else if ((request.UpdateDefinition.Name != request.DataSourceName))
          throw new McpException($"Data source name mismatch: Request specifies '{request.DataSourceName}' but UpdateDefinition specifies '{request.UpdateDefinition.Name}'");
      }
      OperationResult operationResult = DataSourceOperations.UpdateDataSource(request.ConnectionName, request.UpdateDefinition);
      this._logger.LogInformation("{ToolName}.{Operation} completed: DataSource={DataSourceName}", (object) nameof (DataSourceOperationsTool), (object) request.Operation, (object) request.UpdateDefinition.Name);
      return new DataSourceOperationResponse()
      {
        Success = true,
        Message = $"Data source '{request.UpdateDefinition.Name}' updated successfully",
        Operation = request.Operation,
        DataSourceName = request.UpdateDefinition.Name,
        Data = (object) operationResult
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      DataSourceOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new DataSourceOperationResponse()
      {
        Success = false,
        Message = "Error updating data source: " + ex.Message,
        Operation = request.Operation,
        DataSourceName = request.DataSourceName,
        Help = (object) operationMetadata
      };
    }
  }

  private DataSourceOperationResponse HandleDeleteOperation(DataSourceOperationRequest request)
  {
    try
    {
      DataSourceOperations.DeleteDataSource(request.ConnectionName, request.DataSourceName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: DataSource={DataSourceName}", (object) nameof (DataSourceOperationsTool), (object) request.Operation, (object) request.DataSourceName);
      return new DataSourceOperationResponse()
      {
        Success = true,
        Message = $"Data source '{request.DataSourceName}' deleted successfully",
        Operation = request.Operation,
        DataSourceName = request.DataSourceName
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      DataSourceOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new DataSourceOperationResponse()
      {
        Success = false,
        Message = "Error deleting data source: " + ex.Message,
        Operation = request.Operation,
        DataSourceName = request.DataSourceName,
        Help = (object) operationMetadata
      };
    }
  }

  private DataSourceOperationResponse HandleGetOperation(DataSourceOperationRequest request)
  {
    try
    {
      DataSourceGet dataSource = DataSourceOperations.GetDataSource(request.ConnectionName, request.DataSourceName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: DataSource={DataSourceName}", (object) nameof (DataSourceOperationsTool), (object) request.Operation, (object) request.DataSourceName);
      return new DataSourceOperationResponse()
      {
        Success = true,
        Message = $"Data source '{request.DataSourceName}' retrieved successfully",
        Operation = request.Operation,
        DataSourceName = request.DataSourceName,
        Data = (object) dataSource
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      DataSourceOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new DataSourceOperationResponse()
      {
        Success = false,
        Message = "Error getting data source: " + ex.Message,
        Operation = request.Operation,
        DataSourceName = request.DataSourceName,
        Help = (object) operationMetadata
      };
    }
  }

  private DataSourceOperationResponse HandleListOperation(DataSourceOperationRequest request)
  {
    try
    {
      List<DataSourceList> dataSourceListList = DataSourceOperations.ListDataSources(request.ConnectionName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Count={Count}", (object) nameof (DataSourceOperationsTool), (object) request.Operation, (object) dataSourceListList.Count);
      DataSourceOperationResponse operationResponse = new DataSourceOperationResponse { Success = true };
      operationResponse.Message = $"Found {dataSourceListList.Count} data sources";
      operationResponse.Operation = request.Operation;
      operationResponse.Data = (object) dataSourceListList;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      DataSourceOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new DataSourceOperationResponse()
      {
        Success = false,
        Message = "Failed to list data sources: " + ex.Message,
        Operation = "LIST",
        Help = (object) operationMetadata
      };
    }
  }

  private DataSourceOperationResponse HandleRenameOperation(DataSourceOperationRequest request)
  {
    try
    {
      if (string.IsNullOrEmpty(request.RenameDefinition.CurrentName))
        request.RenameDefinition.CurrentName = !string.IsNullOrEmpty(request.DataSourceName) ? request.DataSourceName : throw new McpException("Either DataSourceName or RenameDefinition.CurrentName is required.");
      DataSourceOperations.RenameDataSource(request.ConnectionName, request.RenameDefinition.CurrentName, request.RenameDefinition.NewName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: From={OldName}, To={NewName}", (object) nameof (DataSourceOperationsTool), (object) request.Operation, (object) request.RenameDefinition.CurrentName, (object) request.RenameDefinition.NewName);
      DataSourceOperationResponse operationResponse = new DataSourceOperationResponse { Success = true };
      operationResponse.Message = $"Data source '{request.RenameDefinition.CurrentName}' renamed to '{request.RenameDefinition.NewName}' successfully";
      operationResponse.Operation = "RENAME";
      operationResponse.DataSourceName = request.RenameDefinition.NewName;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      DataSourceOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new DataSourceOperationResponse()
      {
        Success = false,
        Message = "Failed to rename data source: " + ex.Message,
        Operation = "RENAME",
        DataSourceName = request.DataSourceName,
        Help = (object) operationMetadata
      };
    }
  }

  private DataSourceOperationResponse HandleTestOperation(DataSourceOperationRequest request)
  {
    try
    {
      OperationResult operationResult = DataSourceOperations.TestDataSource(request.ConnectionName, request.DataSourceName);
      if (operationResult.Success)
        this._logger.LogInformation("{ToolName}.{Operation} completed: DataSource={DataSourceName}, Status=Success", (object) nameof (DataSourceOperationsTool), (object) request.Operation, (object) request.DataSourceName);
      else
        this._logger.LogWarning("{ToolName}.{Operation} completed: DataSource={DataSourceName}, Status=Failed", (object) nameof (DataSourceOperationsTool), (object) request.Operation, (object) request.DataSourceName);
      return new DataSourceOperationResponse()
      {
        Success = operationResult.Success,
        Message = operationResult.Success ? $"Data source '{request.DataSourceName}' tested successfully" : $"Data source '{request.DataSourceName}' test failed",
        Operation = request.Operation,
        DataSourceName = request.DataSourceName,
        Data = (object) operationResult
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      DataSourceOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new DataSourceOperationResponse()
      {
        Success = false,
        Message = "Error testing data source: " + ex.Message,
        Operation = request.Operation,
        DataSourceName = request.DataSourceName,
        Help = (object) operationMetadata
      };
    }
  }

  private DataSourceOperationResponse HandleExportTMDLOperation(DataSourceOperationRequest request)
  {
    try
    {
      string str = DataSourceOperations.ExportTMDL(request.ConnectionName, request.DataSourceName, (ExportTmdl) request.TmdlExportOptions);
      this._logger.LogInformation("{ToolName}.{Operation} completed: DataSource={DataSourceName}", (object) nameof (DataSourceOperationsTool), (object) request.Operation, (object) request.DataSourceName);
      return new DataSourceOperationResponse()
      {
        Success = true,
        Message = $"TMDL for data source '{request.DataSourceName}' retrieved successfully",
        Operation = request.Operation,
        DataSourceName = request.DataSourceName,
        Data = (object) str
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      DataSourceOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new DataSourceOperationResponse()
      {
        Success = false,
        Message = "Error getting data source TMDL: " + ex.Message,
        Operation = request.Operation,
        DataSourceName = request.DataSourceName,
        Help = (object) operationMetadata
      };
    }
  }

  private DataSourceOperationResponse HandleHelpOperation(
    DataSourceOperationRequest request,
    string[] operations)
  {
    this._logger.LogInformation("{ToolName}.{Operation} completed: Operations={OperationCount}", (object) nameof (DataSourceOperationsTool), (object) request.Operation, (object) operations.Length);
    return new DataSourceOperationResponse()
    {
      Success = true,
      Message = "Help information retrieved successfully",
      Operation = request.Operation,
      Help = (object) new
      {
        ToolName = "data_source_operations",
        Description = "Perform operations on SSAS data sources in semantic models.",
        SupportedOperations = operations,
        Examples = Enumerable.Where<KeyValuePair<string, OperationMetadata>>((IEnumerable<KeyValuePair<string, OperationMetadata>>) DataSourceOperationsTool.toolMetadata.Operations, (Func<KeyValuePair<string, OperationMetadata>, bool>) (p => Enumerable.Contains<string>((IEnumerable<string>) operations, p.Key, (IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase))),
        Notes = new string[4]
        {
          "Use the Operation parameter to specify which operation to perform.",
          "For operations that require an SSAS data source name, specify the name in the DataSourceName parameter.",
          "For operations that require an SSAS data source definition, specify the definition in the CreateDefinition or UpdateDefinition parameter.",
          "Note: Power BI uses named expressions instead of data sources for centralized connectivity information."
        }
      }
    };
  }

  private bool ValidateRequest(string operation, DataSourceOperationRequest request)
  {
    OperationMetadata operationMetadata;
    if (!DataSourceOperationsTool.toolMetadata.Operations.TryGetValue(operation, out operationMetadata))
      return true;
    JsonObject requestDict = JsonSerializer.SerializeToNode<DataSourceOperationRequest>(request) as JsonObject;
    List<string> list1 = Enumerable.ToList<string>(Enumerable.Where<string>((IEnumerable<string>) operationMetadata.RequiredParams, (p => requestDict != null && requestDict[p] == null)));
    List<string> list2 = Enumerable.ToList<string>(Enumerable.Where<string>((IEnumerable<string>) operationMetadata.ForbiddenParams, (p => requestDict != null && requestDict[p] != null)));
    if (Enumerable.Any<string>((IEnumerable<string>) list1))
      throw new McpException($"Missing required parameters needed for {operation} operation: {string.Join(", ", (IEnumerable<string>) list1)}");
    if (Enumerable.Any<string>((IEnumerable<string>) list2))
      throw new McpException($"Forbidden parameters not allowed for {operation} operation: {string.Join(", ", (IEnumerable<string>) list2)}");
    return true;
  }

  static DataSourceOperationsTool()
  {
    ToolMetadata toolMetadata1 = new ToolMetadata();
    ToolMetadata toolMetadata2 = toolMetadata1;
    Dictionary<string, OperationMetadata> dictionary1 = new Dictionary<string, OperationMetadata>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    Dictionary<string, OperationMetadata> dictionary2 = dictionary1;
    OperationMetadata operationMetadata1 = new OperationMetadata { Description = "List all SSAS data sources in the model.\r\nMandatory properties: None.\r\nOptional: None." };
    List<string> stringList1 = new List<string>();
    stringList1.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"List\"\r\n    }\r\n}");
    operationMetadata1.ExampleRequests = stringList1;
    dictionary2["List"] = operationMetadata1;
    Dictionary<string, OperationMetadata> dictionary3 = dictionary1;
    OperationMetadata operationMetadata2 = new OperationMetadata { RequiredParams = new string[1]
    {
      "DataSourceName"
    } };
    operationMetadata2.Description = "Get details of a specific SSAS data source.\r\nMandatory properties: DataSourceName.\r\nOptional: None.";
    OperationMetadata operationMetadata3 = operationMetadata2;
    List<string> stringList2 = new List<string>();
    stringList2.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Get\",\r\n        \"DataSourceName\": \"SalesDataSource\"\r\n    }\r\n}");
    operationMetadata3.ExampleRequests = stringList2;
    OperationMetadata operationMetadata4 = operationMetadata2;
    dictionary3["Get"] = operationMetadata4;
    Dictionary<string, OperationMetadata> dictionary4 = dictionary1;
    OperationMetadata operationMetadata5 = new OperationMetadata { RequiredParams = new string[1]
    {
      "CreateDefinition"
    } };
    operationMetadata5.Description = "Create a new SSAS data source.\r\nNote: Power BI uses named expressions instead of data sources as the centralized place to store connectivity information.\r\nMandatory properties: CreateDefinition (with Name, ConnectionString).\r\nOptional: Description, Provider, ImpersonationMode, Account, Password, MaxConnections, Isolation, Timeout, Annotations, ExtendedProperties.";
    OperationMetadata operationMetadata6 = operationMetadata5;
    List<string> stringList3 = new List<string>();
    stringList3.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Create\",\r\n        \"CreateDefinition\": { \r\n            \"Name\": \"SalesDataSource\", \r\n            \"ConnectionString\": \"Server=localhost;Database=SalesNew;Trusted_Connection=True\" \r\n        }\r\n    }\r\n}");
    operationMetadata6.ExampleRequests = stringList3;
    OperationMetadata operationMetadata7 = operationMetadata5;
    dictionary4["Create"] = operationMetadata7;
    Dictionary<string, OperationMetadata> dictionary5 = dictionary1;
    OperationMetadata operationMetadata8 = new OperationMetadata { RequiredParams = new string[1]
    {
      "UpdateDefinition"
    } };
    operationMetadata8.Description = "Update an existing SSAS data source's properties.\r\nNames cannot be changed - use Rename operation for name changes.\r\nMandatory properties: UpdateDefinition (with Name).\r\nOptional: Description, ConnectionString, Provider, ImpersonationMode, Account, Password, MaxConnections, Isolation, Timeout, Annotations, ExtendedProperties.";
    OperationMetadata operationMetadata9 = operationMetadata8;
    List<string> stringList4 = new List<string>();
    stringList4.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Update\",\r\n        \"DataSourceName\": \"SalesDataSource\",\r\n        \"UpdateDefinition\": { \r\n            \"Name\": \"SalesDataSource\", \r\n            \"ConnectionString\": \"Server=localhost;Database=SalesNew;Trusted_Connection=True\" \r\n        }\r\n    }\r\n}");
    operationMetadata9.ExampleRequests = stringList4;
    OperationMetadata operationMetadata10 = operationMetadata8;
    dictionary5["Update"] = operationMetadata10;
    Dictionary<string, OperationMetadata> dictionary6 = dictionary1;
    OperationMetadata operationMetadata11 = new OperationMetadata { RequiredParams = new string[1]
    {
      "DataSourceName"
    } };
    operationMetadata11.Description = "Delete an SSAS data source.\r\nCannot delete SSAS data sources that are referenced by table partitions.\r\nMandatory properties: DataSourceName.\r\nOptional: None.";
    OperationMetadata operationMetadata12 = operationMetadata11;
    List<string> stringList5 = new List<string>();
    stringList5.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Delete\",\r\n        \"DataSourceName\": \"ObsoleteDataSource\"\r\n    }\r\n}");
    operationMetadata12.ExampleRequests = stringList5;
    OperationMetadata operationMetadata13 = operationMetadata11;
    dictionary6["Delete"] = operationMetadata13;
    Dictionary<string, OperationMetadata> dictionary7 = dictionary1;
    OperationMetadata operationMetadata14 = new OperationMetadata { RequiredParams = new string[1]
    {
      "RenameDefinition"
    } };
    operationMetadata14.Description = "Rename an SSAS data source.\r\nMandatory properties: RenameDefinition (with CurrentName, NewName).\r\nOptional: None.";
    OperationMetadata operationMetadata15 = operationMetadata14;
    List<string> stringList6 = new List<string>();
    stringList6.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Rename\",\r\n        \"RenameDefinition\": { \r\n            \"CurrentName\": \"OldDataSource\", \r\n            \"NewName\": \"NewDataSource\"\r\n        }\r\n    }\r\n}");
    operationMetadata15.ExampleRequests = stringList6;
    OperationMetadata operationMetadata16 = operationMetadata14;
    dictionary7["Rename"] = operationMetadata16;
    Dictionary<string, OperationMetadata> dictionary8 = dictionary1;
    OperationMetadata operationMetadata17 = new OperationMetadata { RequiredParams = new string[1]
    {
      "DataSourceName"
    } };
    operationMetadata17.Description = "Test an SSAS data source connection by validating its configuration.\r\nMandatory properties: DataSourceName.\r\nOptional: None.";
    OperationMetadata operationMetadata18 = operationMetadata17;
    List<string> stringList7 = new List<string>();
    stringList7.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Test\",\r\n        \"ConnectionName\": \"MyConnection\",\r\n        \"DataSourceName\": \"SalesSource\"\r\n    }\r\n}");
    operationMetadata18.ExampleRequests = stringList7;
    OperationMetadata operationMetadata19 = operationMetadata17;
    dictionary8["Test"] = operationMetadata19;
    Dictionary<string, OperationMetadata> dictionary9 = dictionary1;
    OperationMetadata operationMetadata20 = new OperationMetadata { RequiredParams = new string[1]
    {
      "DataSourceName"
    } };
    operationMetadata20.Description = "Export SSAS data source to TMDL format.\r\nMandatory properties: DataSourceName.\r\nOptional: TmdlExportOptions.";
    OperationMetadata operationMetadata21 = operationMetadata20;
    List<string> stringList8 = new List<string>();
    stringList8.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"ExportTMDL\",\r\n        \"DataSourceName\": \"SalesDataSource\"\r\n    }\r\n}");
    operationMetadata21.ExampleRequests = stringList8;
    OperationMetadata operationMetadata22 = operationMetadata20;
    dictionary9["ExportTMDL"] = operationMetadata22;
    Dictionary<string, OperationMetadata> dictionary10 = dictionary1;
    OperationMetadata operationMetadata23 = new OperationMetadata { Description = "Describe the tool and its operations.\r\nProvides comprehensive information about all available SSAS data source operations.\r\nMandatory properties: None.\r\nOptional: None." };
    List<string> stringList9 = new List<string>();
    stringList9.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Help\"\r\n    }\r\n}");
    operationMetadata23.ExampleRequests = stringList9;
    dictionary10["Help"] = operationMetadata23;
    Dictionary<string, OperationMetadata> dictionary11 = dictionary1;
    toolMetadata2.Operations = dictionary11;
    DataSourceOperationsTool.toolMetadata = toolMetadata1;
  }
}
