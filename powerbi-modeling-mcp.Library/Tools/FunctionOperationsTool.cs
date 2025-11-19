// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Tools.FunctionOperationsTool
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
public class FunctionOperationsTool
{
  private readonly ILogger<FunctionOperationsTool> _logger;
  public static readonly ToolMetadata toolMetadata;

  public FunctionOperationsTool(ILogger<FunctionOperationsTool> logger) => this._logger = logger;

  [McpServerTool(Name = "function_operations")]
  [Description("Perform operations on semantic model functions. Supported operations: Help, Create, Update, Delete, Get, List, Rename, ExportTMDL. Use the Operation parameter to specify which operation to perform. FunctionName is required for all operations except List. ExportTMDL exports a function to TMDL format.")]
  public FunctionOperationResponse ExecuteFunctionOperation(
    McpServer mcpServer,
    FunctionOperationRequest request)
  {
    this._logger.LogDebug("Executing {ToolName}.{Operation}: FunctionName={FunctionName}, Connection={ConnectionName}", (object) nameof (FunctionOperationsTool), (object) request.Operation, (object) (request.FunctionName ?? "(none)"), (object) (request.ConnectionName ?? "(last used)"));
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
      if (!Enumerable.Contains<string>((IEnumerable<string>) strArray1, request.Operation.ToUpperInvariant()))
      {
        this._logger.LogWarning("Invalid operation '{Operation}' requested for {ToolName}. Valid operations: {ValidOperations}", (object) request.Operation, (object) nameof (FunctionOperationsTool), (object) string.Join(", ", strArray1));
        return FunctionOperationResponse.Forbidden(request.Operation, $"Invalid operation: {request.Operation}. Supported operations: {string.Join(", ", strArray1)}");
      }
      if (!this.ValidateRequest(request.Operation, request))
        throw new McpException($"Invalid request for {request.Operation} operation.");
      if (Enumerable.Contains<string>((IEnumerable<string>) strArray2, request.Operation.ToUpperInvariant()))
      {
        WriteOperationResult writeOperationResult = WriteGuard.ExecuteWriteOperationWithGuards(mcpServer, request.ConnectionName, request.Operation);
        if (!writeOperationResult.Success)
        {
          this._logger.LogWarning("{ToolName}.{Operation} blocked by write guard: {Reason}", (object) nameof (FunctionOperationsTool), (object) request.Operation, (object) writeOperationResult.Message);
          return FunctionOperationResponse.Forbidden(request.Operation, writeOperationResult.Message);
        }
      }
      bool allowed = WriteGuard.IsWriteAllowed("").allowed;
      string upperInvariant = request.Operation.ToUpperInvariant();
      FunctionOperationResponse operationResponse;
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
      operationResponse = FunctionOperationResponse.Forbidden(request.Operation, $"Operation {request.Operation} is not implemented");
label_29:
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Error executing {ToolName}.{Operation}: {ErrorMessage}", (object) nameof (FunctionOperationsTool), (object) request.Operation, (object) ex.Message);
      return new FunctionOperationResponse()
      {
        Success = false,
        Message = "Error executing function operation: " + ex.Message,
        Operation = request.Operation,
        FunctionName = request.FunctionName
      };
    }
  }

  private FunctionOperationResponse HandleCreateOperation(FunctionOperationRequest request)
  {
    try
    {
      if (string.IsNullOrEmpty(request.CreateDefinition.Name))
        request.CreateDefinition.Name = !string.IsNullOrEmpty(request.FunctionName) ? request.FunctionName : throw new McpException("FunctionName is required for Create operation (either in request or CreateDefinition)");
      else if (!string.IsNullOrEmpty(request.FunctionName) && (request.CreateDefinition.Name != request.FunctionName))
        throw new McpException($"Function name mismatch: Request specifies '{request.FunctionName}' but CreateDefinition specifies '{request.CreateDefinition.Name}'");
      FunctionOperations.FunctionOperationResult function = FunctionOperations.CreateFunction(request.ConnectionName, request.CreateDefinition);
      FunctionOperationResponse operationResponse = new FunctionOperationResponse { Success = (function.State == "Ready") };
      operationResponse.Message = (function.State == "Ready") ? $"Function '{request.CreateDefinition.Name}' created successfully" : function.ErrorMessage ?? $"Failed to create function '{request.CreateDefinition.Name}'";
      operationResponse.Operation = request.Operation;
      operationResponse.FunctionName = request.CreateDefinition.Name;
      operationResponse.Data = (object) function;
      List<string> stringList1;
      if (!(function.State != "Ready") || string.IsNullOrEmpty(function.ErrorMessage))
      {
        stringList1 = new List<string>();
      }
      else
      {
        List<string> stringList2 = new List<string>();
        stringList2.Add(function.ErrorMessage);
        stringList1 = stringList2;
      }
      operationResponse.Warnings = stringList1;
      FunctionOperationResponse operation = operationResponse;
      if (operation.Warnings != null && Enumerable.Any<string>((IEnumerable<string>) operation.Warnings))
      {
        foreach (string warning in operation.Warnings)
          this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (FunctionOperationsTool), (object) request.Operation, (object) warning);
      }
      if (operation.Success)
        this._logger.LogInformation("{ToolName}.{Operation} completed: FunctionName={FunctionName}", (object) nameof (FunctionOperationsTool), (object) request.Operation, (object) request.CreateDefinition.Name);
      else
        this._logger.LogWarning("{ToolName}.{Operation} completed with errors: FunctionName={FunctionName}, Message={Message}", (object) nameof (FunctionOperationsTool), (object) request.Operation, (object) request.CreateDefinition.Name, (object) operation.Message);
      return operation;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      FunctionOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new FunctionOperationResponse()
      {
        Success = false,
        Message = "Error creating function: " + ex.Message,
        Operation = request.Operation,
        FunctionName = request.FunctionName,
        Help = (object) operationMetadata
      };
    }
  }

  private FunctionOperationResponse HandleUpdateOperation(FunctionOperationRequest request)
  {
    try
    {
      if (request.UpdateDefinition == null)
        throw new McpException("UpdateDefinition is required for Update operation");
      if (string.IsNullOrWhiteSpace(request.UpdateDefinition.Name))
        throw new McpException("Function name must be specified in UpdateDefinition.Name");
      FunctionOperations.FunctionOperationResult functionOperationResult = FunctionOperations.UpdateFunction(request.ConnectionName, request.UpdateDefinition);
      FunctionOperationResponse operationResponse1 = new FunctionOperationResponse { Success = (functionOperationResult.State == "Ready") };
      operationResponse1.Message = (functionOperationResult.State == "Ready") ? $"Function '{request.UpdateDefinition.Name}' updated successfully" : functionOperationResult.ErrorMessage ?? $"Failed to update function '{request.UpdateDefinition.Name}'";
      operationResponse1.Operation = request.Operation;
      operationResponse1.FunctionName = request.UpdateDefinition.Name;
      operationResponse1.Data = (object) functionOperationResult;
      List<string> stringList1;
      if (!(functionOperationResult.State != "Ready") || string.IsNullOrEmpty(functionOperationResult.ErrorMessage))
      {
        stringList1 = new List<string>();
      }
      else
      {
        List<string> stringList2 = new List<string>();
        stringList2.Add(functionOperationResult.ErrorMessage);
        stringList1 = stringList2;
      }
      operationResponse1.Warnings = stringList1;
      FunctionOperationResponse operationResponse2 = operationResponse1;
      if (operationResponse2.Warnings != null && Enumerable.Any<string>((IEnumerable<string>) operationResponse2.Warnings))
      {
        foreach (string warning in operationResponse2.Warnings)
          this._logger.LogWarning("{ToolName}.{Operation} warning: {Warning}", (object) nameof (FunctionOperationsTool), (object) request.Operation, (object) warning);
      }
      if (operationResponse2.Success)
        this._logger.LogInformation("{ToolName}.{Operation} completed: FunctionName={FunctionName}", (object) nameof (FunctionOperationsTool), (object) request.Operation, (object) request.UpdateDefinition.Name);
      else
        this._logger.LogWarning("{ToolName}.{Operation} completed with errors: FunctionName={FunctionName}, Message={Message}", (object) nameof (FunctionOperationsTool), (object) request.Operation, (object) request.UpdateDefinition.Name, (object) operationResponse2.Message);
      return operationResponse2;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      FunctionOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new FunctionOperationResponse()
      {
        Success = false,
        Message = "Error updating function: " + ex.Message,
        Operation = request.Operation,
        FunctionName = request.FunctionName,
        Help = (object) operationMetadata
      };
    }
  }

  private FunctionOperationResponse HandleDeleteOperation(FunctionOperationRequest request)
  {
    try
    {
      FunctionOperations.DeleteFunction(request.ConnectionName, request.FunctionName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: FunctionName={FunctionName}", (object) nameof (FunctionOperationsTool), (object) request.Operation, (object) request.FunctionName);
      return new FunctionOperationResponse()
      {
        Success = true,
        Message = $"Function '{request.FunctionName}' deleted successfully",
        Operation = request.Operation,
        FunctionName = request.FunctionName
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      FunctionOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new FunctionOperationResponse()
      {
        Success = false,
        Message = "Error executing function operation: " + ex.Message,
        Operation = request.Operation,
        FunctionName = request.FunctionName,
        Help = (object) operationMetadata
      };
    }
  }

  private FunctionOperationResponse HandleGetOperation(FunctionOperationRequest request)
  {
    try
    {
      FunctionGet function = FunctionOperations.GetFunction(request.ConnectionName, request.FunctionName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: FunctionName={FunctionName}", (object) nameof (FunctionOperationsTool), (object) request.Operation, (object) request.FunctionName);
      return new FunctionOperationResponse()
      {
        Success = true,
        Message = $"Function '{request.FunctionName}' retrieved successfully",
        Operation = request.Operation,
        FunctionName = request.FunctionName,
        Data = (object) function
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      FunctionOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new FunctionOperationResponse()
      {
        Success = false,
        Message = "Error executing function operation: " + ex.Message,
        Operation = request.Operation,
        FunctionName = request.FunctionName,
        Help = (object) operationMetadata
      };
    }
  }

  private FunctionOperationResponse HandleListOperation(FunctionOperationRequest request)
  {
    try
    {
      List<FunctionList> functionListList = FunctionOperations.ListFunctions(request.ConnectionName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Count={Count}", (object) nameof (FunctionOperationsTool), (object) request.Operation, (object) functionListList.Count);
      FunctionOperationResponse operationResponse = new FunctionOperationResponse { Success = true };
      operationResponse.Message = $"Found {functionListList.Count} functions in the model";
      operationResponse.Operation = request.Operation;
      operationResponse.Data = (object) functionListList;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      FunctionOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new FunctionOperationResponse()
      {
        Success = false,
        Message = "Error listing functions: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private FunctionOperationResponse HandleRenameOperation(FunctionOperationRequest request)
  {
    try
    {
      if (string.IsNullOrEmpty(request.RenameDefinition.CurrentName))
        request.RenameDefinition.CurrentName = !string.IsNullOrEmpty(request.FunctionName) ? request.FunctionName : throw new McpException("Either FunctionName or RenameDefinition.CurrentName is required.");
      FunctionOperations.RenameFunction(request.ConnectionName, request.RenameDefinition.CurrentName, request.RenameDefinition.NewName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: From={OldName}, To={NewName}", (object) nameof (FunctionOperationsTool), (object) request.Operation, (object) request.RenameDefinition.CurrentName, (object) request.RenameDefinition.NewName);
      FunctionOperationResponse operationResponse = new FunctionOperationResponse { Success = true };
      operationResponse.Message = $"Function '{request.RenameDefinition.CurrentName}' renamed to '{request.RenameDefinition.NewName}' successfully";
      operationResponse.Operation = request.Operation;
      operationResponse.FunctionName = request.RenameDefinition.NewName;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      FunctionOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new FunctionOperationResponse()
      {
        Success = false,
        Message = "Error renaming function: " + ex.Message,
        Operation = request.Operation,
        FunctionName = request.FunctionName,
        Help = (object) operationMetadata
      };
    }
  }

  private FunctionOperationResponse HandleExportTMDLOperation(FunctionOperationRequest request)
  {
    try
    {
      string str = FunctionOperations.ExportTMDL(request.ConnectionName, request.FunctionName, (ExportTmdl) request.TmdlExportOptions);
      this._logger.LogInformation("{ToolName}.{Operation} completed: FunctionName={FunctionName}", (object) nameof (FunctionOperationsTool), (object) request.Operation, (object) request.FunctionName);
      return new FunctionOperationResponse()
      {
        Success = true,
        Message = $"Function '{request.FunctionName}' exported to TMDL format successfully",
        Operation = request.Operation,
        FunctionName = request.FunctionName,
        Data = (object) str
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      FunctionOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new FunctionOperationResponse()
      {
        Success = false,
        Message = $"Failed to export TMDL for function '{request.FunctionName}': {ex.Message}",
        Operation = request.Operation,
        FunctionName = request.FunctionName,
        Help = (object) operationMetadata
      };
    }
  }

  private FunctionOperationResponse HandleHelpOperation(
    FunctionOperationRequest request,
    string[] operations)
  {
    this._logger.LogInformation("{ToolName}.{Operation} completed: Operations={OperationCount}", (object) nameof (FunctionOperationsTool), (object) request.Operation, (object) operations.Length);
    return new FunctionOperationResponse()
    {
      Success = true,
      Message = "Tool description retrieved successfully",
      Operation = request.Operation,
      Help = (object) new
      {
        ToolName = "function_operations",
        Description = "Perform operations on semantic model functions.",
        SupportedOperations = operations,
        Examples = Enumerable.Where<KeyValuePair<string, OperationMetadata>>((IEnumerable<KeyValuePair<string, OperationMetadata>>) FunctionOperationsTool.toolMetadata.Operations, (Func<KeyValuePair<string, OperationMetadata>, bool>) (p => Enumerable.Contains<string>((IEnumerable<string>) operations, p.Key, (IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase))),
        Notes = new string[3]
        {
          "FunctionName is required for all operations except List.",
          "Functions are DAX/M functions defined in the semantic model.",
          "ExportTMDL exports a function to TMDL format."
        }
      }
    };
  }

  private bool ValidateRequest(string operation, FunctionOperationRequest request)
  {
    OperationMetadata operationMetadata;
    if (!FunctionOperationsTool.toolMetadata.Operations.TryGetValue(operation, out operationMetadata))
      return true;
    JsonObject requestDict = JsonSerializer.SerializeToNode<FunctionOperationRequest>(request) as JsonObject;
    List<string> list1 = Enumerable.ToList<string>(Enumerable.Where<string>((IEnumerable<string>) operationMetadata.RequiredParams, (p => requestDict != null && requestDict[p] == null)));
    List<string> list2 = Enumerable.ToList<string>(Enumerable.Where<string>((IEnumerable<string>) operationMetadata.ForbiddenParams, (p => requestDict != null && requestDict[p] != null)));
    if (Enumerable.Any<string>((IEnumerable<string>) list1))
      throw new McpException($"Missing required parameters needed for {operation} operation: {string.Join(", ", (IEnumerable<string>) list1)}");
    if (Enumerable.Any<string>((IEnumerable<string>) list2))
      throw new McpException($"Forbidden parameters not allowed for {operation} operation: {string.Join(", ", (IEnumerable<string>) list2)}");
    return true;
  }

  static FunctionOperationsTool()
  {
    ToolMetadata toolMetadata1 = new ToolMetadata();
    ToolMetadata toolMetadata2 = toolMetadata1;
    Dictionary<string, OperationMetadata> dictionary1 = new Dictionary<string, OperationMetadata>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    Dictionary<string, OperationMetadata> dictionary2 = dictionary1;
    OperationMetadata operationMetadata1 = new OperationMetadata { RequiredParams = new string[1]
    {
      "CreateDefinition"
    } };
    operationMetadata1.Description = "Creates a new user-defined DAX function in the semantic model.\r\nMandatory properties: CreateDefinition (with Name, Expression).\r\nOptional: Description, IsHidden, LineageTag, SourceLineageTag, Annotations, ExtendedProperties.";
    OperationMetadata operationMetadata2 = operationMetadata1;
    List<string> stringList1 = new List<string>();
    stringList1.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Create\",\r\n        \"CreateDefinition\": {\r\n            \"Name\": \"CircleArea\",\r\n            \"Expression\": \"(radius) => PI() * radius * radius\",\r\n            \"Description\": \"Calculates the area of a circle given its radius\"\r\n        }\r\n    }\r\n}");
    stringList1.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Create\",\r\n        \"CreateDefinition\": {\r\n            \"Name\": \"DoubleValue\",\r\n            \"Expression\": \"(inputValue : Scalar Val) => inputValue * 2\",\r\n            \"Description\": \"Doubles the input value\"\r\n        }\r\n    }\r\n}");
    stringList1.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Create\",\r\n        \"CreateDefinition\": {\r\n            \"Name\": \"Mode\",\r\n            \"Expression\": \"(tab : AnyRef, col : AnyRef) => MINX(TOPN(1, ADDCOLUMNS(VALUES(col), \\\"Freq\\\", CALCULATE(COUNTROWS(tab))), [Freq], DESC), col)\",\r\n            \"Description\": \"Finds the most frequently occurring value in the column\"\r\n        }\r\n    }\r\n}");
    stringList1.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Create\",\r\n        \"CreateDefinition\": {\r\n            \"Name\": \"PriorYearValue\",\r\n            \"Expression\": \"(expression : Scalar Expr, dateColumn : AnyRef) => CALCULATE(expression, SAMEPERIODLASTYEAR(dateColumn))\",\r\n            \"Description\": \"Calculates the value of any scalar expression in the previous year using the specified date column\"\r\n        }\r\n    }\r\n}");
    stringList1.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Create\",\r\n        \"CreateDefinition\": {\r\n            \"Name\": \"TodayAsDate\",\r\n            \"Expression\": \"() => TREATAS({ TODAY() }, 'Date'[Date])\",\r\n            \"Description\": \"Returns today's date as a table expression that can be used in filter contexts\"\r\n        }\r\n    }\r\n}");
    stringList1.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Create\",\r\n        \"CreateDefinition\": {\r\n            \"Name\": \"Top3ProductsBySales\",\r\n            \"Expression\": \"() => TOPN(3, VALUES('Product'[ProductKey]), [Sales], DESC)\",\r\n            \"Description\": \"Returns the top 3 products by Sales amount\"\r\n        }\r\n    }\r\n}");
    stringList1.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Create\",\r\n        \"CreateDefinition\": {\r\n            \"Name\": \"SplitString\",\r\n            \"Expression\": \"(s : String, delimiter : String) => VAR str = SUBSTITUTE(s, delimiter, \\\"|\\\") VAR len = PATHLENGTH(str) RETURN SELECTCOLUMNS(GENERATESERIES(1, len), \\\"Value\\\", PATHITEM(str, [Value], TEXT))\",\r\n            \"Description\": \"Splits a string by a delimiter and returns a table with the split values\"\r\n        }\r\n    }\r\n}");
    operationMetadata2.ExampleRequests = stringList1;
    OperationMetadata operationMetadata3 = operationMetadata1;
    dictionary2["Create"] = operationMetadata3;
    Dictionary<string, OperationMetadata> dictionary3 = dictionary1;
    OperationMetadata operationMetadata4 = new OperationMetadata { RequiredParams = new string[1]
    {
      "UpdateDefinition"
    } };
    operationMetadata4.Description = "Updates an existing user-defined DAX function in the semantic model. Names cannot be changed, use Rename operation instead.\r\nMandatory properties: UpdateDefinition (with Name).\r\nOptional: Expression, Description, IsHidden, LineageTag, SourceLineageTag, Annotations, ExtendedProperties.";
    OperationMetadata operationMetadata5 = operationMetadata4;
    List<string> stringList2 = new List<string>();
    stringList2.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Update\",\r\n        \"FunctionName\": \"MyFunction\",\r\n        \"UpdateDefinition\": {\r\n            \"Name\": \"CircleArea\",\r\n            \"Expression\": \"(radius : SCALAR NUMERIC) => PI() * radius * radius\"\r\n        }\r\n    }\r\n}");
    stringList2.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Update\",\r\n        \"UpdateDefinition\": {\r\n            \"Name\": \"Mode\",\r\n            \"Expression\": \"(tab : AnyRef, col : AnyRef) => MINX(TOPN(1, ADDCOLUMNS(VALUES(col), \\\"Freq\\\", CALCULATE(COUNTROWS(tab))), [Freq], DESC, col, ASC), col)\"\r\n        }\r\n    }\r\n}");
    operationMetadata5.ExampleRequests = stringList2;
    OperationMetadata operationMetadata6 = operationMetadata4;
    dictionary3["Update"] = operationMetadata6;
    Dictionary<string, OperationMetadata> dictionary4 = dictionary1;
    OperationMetadata operationMetadata7 = new OperationMetadata { RequiredParams = new string[1]
    {
      "FunctionName"
    } };
    operationMetadata7.Description = "Deletes a user-defined DAX function from the semantic model.\r\nMandatory properties: FunctionName.\r\nOptional: None.";
    OperationMetadata operationMetadata8 = operationMetadata7;
    List<string> stringList3 = new List<string>();
    stringList3.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Delete\",\r\n        \"FunctionName\": \"MyFunction\"\r\n    }\r\n}");
    operationMetadata8.ExampleRequests = stringList3;
    OperationMetadata operationMetadata9 = operationMetadata7;
    dictionary4["Delete"] = operationMetadata9;
    Dictionary<string, OperationMetadata> dictionary5 = dictionary1;
    OperationMetadata operationMetadata10 = new OperationMetadata { RequiredParams = new string[1]
    {
      "FunctionName"
    } };
    operationMetadata10.Description = "Retrieves detailed information about a specific user-defined DAX function.\r\nMandatory properties: FunctionName.\r\nOptional: None.";
    OperationMetadata operationMetadata11 = operationMetadata10;
    List<string> stringList4 = new List<string>();
    stringList4.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Get\",\r\n        \"FunctionName\": \"MyFunction\"\r\n    }\r\n}");
    operationMetadata11.ExampleRequests = stringList4;
    OperationMetadata operationMetadata12 = operationMetadata10;
    dictionary5["Get"] = operationMetadata12;
    Dictionary<string, OperationMetadata> dictionary6 = dictionary1;
    OperationMetadata operationMetadata13 = new OperationMetadata { Description = "Lists all user-defined DAX functions in the semantic model with basic information.\r\nMandatory properties: None.\r\nOptional: None." };
    List<string> stringList5 = new List<string>();
    stringList5.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"List\"\r\n    }\r\n}");
    operationMetadata13.ExampleRequests = stringList5;
    dictionary6["List"] = operationMetadata13;
    Dictionary<string, OperationMetadata> dictionary7 = dictionary1;
    OperationMetadata operationMetadata14 = new OperationMetadata { RequiredParams = new string[1]
    {
      "RenameDefinition"
    } };
    operationMetadata14.Description = "Renames a user-defined DAX function in the semantic model.\r\nMandatory properties: RenameDefinition (with CurrentName, NewName).\r\nOptional: None.";
    OperationMetadata operationMetadata15 = operationMetadata14;
    List<string> stringList6 = new List<string>();
    stringList6.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Rename\",\r\n        \"RenameDefinition\": { \r\n            \"CurrentName\": \"OldFunction\", \r\n            \"NewName\": \"NewFunction\"\r\n        }\r\n    }\r\n}");
    operationMetadata15.ExampleRequests = stringList6;
    OperationMetadata operationMetadata16 = operationMetadata14;
    dictionary7["Rename"] = operationMetadata16;
    Dictionary<string, OperationMetadata> dictionary8 = dictionary1;
    OperationMetadata operationMetadata17 = new OperationMetadata { RequiredParams = new string[1]
    {
      "FunctionName"
    } };
    operationMetadata17.Description = "Exports a user-defined DAX function definition to TMDL format.\r\nMandatory properties: FunctionName.\r\nOptional: TmdlExportOptions.";
    OperationMetadata operationMetadata18 = operationMetadata17;
    List<string> stringList7 = new List<string>();
    stringList7.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"ExportTMDL\",\r\n        \"FunctionName\": \"MyFunction\"\r\n    }\r\n}");
    operationMetadata18.ExampleRequests = stringList7;
    OperationMetadata operationMetadata19 = operationMetadata17;
    dictionary8["ExportTMDL"] = operationMetadata19;
    Dictionary<string, OperationMetadata> dictionary9 = dictionary1;
    OperationMetadata operationMetadata20 = new OperationMetadata { Description = "Provides detailed information about the function operations tool and its capabilities.\r\nMandatory properties: None.\r\nOptional: None." };
    List<string> stringList8 = new List<string>();
    stringList8.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Help\"\r\n    }\r\n}");
    operationMetadata20.ExampleRequests = stringList8;
    dictionary9["Help"] = operationMetadata20;
    Dictionary<string, OperationMetadata> dictionary10 = dictionary1;
    toolMetadata2.Operations = dictionary10;
    FunctionOperationsTool.toolMetadata = toolMetadata1;
  }
}
