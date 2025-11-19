// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Tools.CultureOperationsTool
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
public class CultureOperationsTool
{
  private readonly ILogger<CultureOperationsTool> _logger;
  public static readonly ToolMetadata toolMetadata;

  public CultureOperationsTool(ILogger<CultureOperationsTool> logger) => this._logger = logger;

  [McpServerTool(Name = "culture_operations")]
  [Description("Perform operations on semantic model cultures. Supported operations: Help, Create, Update, Delete, Get, List, Rename, GetValidNames, GetValidDetails, GetDetailsByName, GetDetailsByLCID, ExportTMDL. Use the Operation parameter to specify which operation to perform. CultureName is required for most operations except List, GetValidNames, and GetValidDetails. LCID is required for GetDetailsByLCID operation. GetValidNames returns culture names only, GetValidDetails returns full culture information including LCIDs. ExportTMDL exports a culture to TMDL format.")]
  public CultureOperationResponse ExecuteCultureOperation(
    McpServer mcpServer,
    CultureOperationRequest request)
  {
    this._logger.LogDebug("Executing {ToolName}.{Operation}: CultureName={CultureName}, Connection={ConnectionName}", (object) nameof (CultureOperationsTool), (object) request.Operation, (object) (request.CultureName ?? "(none)"), (object) (request.ConnectionName ?? "(last used)"));
    try
    {
      string[] strArray1 = new string[12]
      {
        "CREATE",
        "UPDATE",
        "DELETE",
        "GET",
        "LIST",
        "RENAME",
        "GETVALIDNAMES",
        "GETVALIDDETAILS",
        "GETDETAILSBYNAME",
        "GETDETAILSBYLCID",
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
        this._logger.LogWarning("Invalid operation '{Operation}' requested for {ToolName}. Valid operations: {ValidOperations}", (object) request.Operation, (object) nameof (CultureOperationsTool), (object) string.Join(", ", strArray1));
        return CultureOperationResponse.Forbidden(request.Operation, $"Invalid operation: {request.Operation}. Supported operations: {string.Join(", ", strArray1)}", request.CultureName);
      }
      if (!this.ValidateRequest(request.Operation, request))
        throw new McpException($"Invalid request for {request.Operation} operation.");
      if (Enumerable.Contains<string>((IEnumerable<string>) strArray2, upperInvariant))
      {
        WriteOperationResult writeOperationResult = WriteGuard.ExecuteWriteOperationWithGuards(mcpServer, request.ConnectionName, request.Operation);
        if (!writeOperationResult.Success)
        {
          this._logger.LogWarning("{ToolName}.{Operation} blocked by write guard: {Reason}", (object) nameof (CultureOperationsTool), (object) request.Operation, (object) writeOperationResult.Message);
          return CultureOperationResponse.Forbidden(request.Operation, writeOperationResult.Message, request.CultureName);
        }
      }
      bool allowed = WriteGuard.IsWriteAllowed("").allowed;
      CultureOperationResponse operationResponse;
      if (upperInvariant != null)
      {
        switch (upperInvariant.Length)
        {
          case 3:
            if ((upperInvariant == "GET"))
            {
              operationResponse = this.HandleGetOperation(request);
              goto label_38;
            }
            break;
          case 4:
            switch (upperInvariant[0])
            {
              case 'H':
                if ((upperInvariant == "HELP"))
                {
                  operationResponse = this.HandleHelpOperation(request, allowed ? strArray1 : Enumerable.ToArray<string>(Enumerable.Except<string>((IEnumerable<string>) strArray1, (IEnumerable<string>) strArray2)));
                  goto label_38;
                }
                break;
              case 'L':
                if ((upperInvariant == "LIST"))
                {
                  operationResponse = this.HandleListOperation(request);
                  goto label_38;
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
                  goto label_38;
                }
                break;
              case 'D':
                if ((upperInvariant == "DELETE"))
                {
                  operationResponse = this.HandleDeleteOperation(request);
                  goto label_38;
                }
                break;
              case 'R':
                if ((upperInvariant == "RENAME"))
                {
                  operationResponse = this.HandleRenameOperation(request);
                  goto label_38;
                }
                break;
              case 'U':
                if ((upperInvariant == "UPDATE"))
                {
                  operationResponse = this.HandleUpdateOperation(request);
                  goto label_38;
                }
                break;
            }
            break;
          case 10:
            if ((upperInvariant == "EXPORTTMDL"))
            {
              operationResponse = this.HandleExportTMDLOperation(request);
              goto label_38;
            }
            break;
          case 13:
            if ((upperInvariant == "GETVALIDNAMES"))
            {
              operationResponse = this.HandleGetValidNamesOperation(request);
              goto label_38;
            }
            break;
          case 15:
            if ((upperInvariant == "GETVALIDDETAILS"))
            {
              operationResponse = this.HandleGetValidDetailsOperation(request);
              goto label_38;
            }
            break;
          case 16 /*0x10*/:
            switch (upperInvariant[12])
            {
              case 'L':
                if ((upperInvariant == "GETDETAILSBYLCID"))
                {
                  operationResponse = this.HandleGetDetailsByLCIDOperation(request);
                  goto label_38;
                }
                break;
              case 'N':
                if ((upperInvariant == "GETDETAILSBYNAME"))
                {
                  operationResponse = this.HandleGetDetailsByNameOperation(request);
                  goto label_38;
                }
                break;
            }
            break;
        }
      }
      operationResponse = CultureOperationResponse.Forbidden(request.Operation, $"Operation {request.Operation} is not implemented", request.CultureName);
label_38:
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Error executing {ToolName}.{Operation}: {ErrorMessage}", (object) nameof (CultureOperationsTool), (object) request.Operation, (object) ex.Message);
      return new CultureOperationResponse()
      {
        Success = false,
        Message = "Error executing culture operation: " + ex.Message,
        Operation = request.Operation,
        CultureName = request.CultureName
      };
    }
  }

  private CultureOperationResponse HandleCreateOperation(CultureOperationRequest request)
  {
    try
    {
      OperationResult culture = CultureOperations.CreateCulture(request.ConnectionName, request.CreateDefinition);
      this._logger.LogInformation("{ToolName}.{Operation} completed: CultureName={CultureName}", (object) nameof (CultureOperationsTool), (object) "Create", (object) request.CreateDefinition.Name);
      return new CultureOperationResponse()
      {
        Success = culture.Success,
        Message = culture.Message ?? "Culture creation completed",
        Operation = request.Operation,
        CultureName = request.CreateDefinition.Name,
        Data = (object) culture
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      CultureOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new CultureOperationResponse()
      {
        Success = false,
        Message = "Failed to create culture: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private CultureOperationResponse HandleUpdateOperation(CultureOperationRequest request)
  {
    try
    {
      OperationResult operationResult = CultureOperations.UpdateCulture(request.ConnectionName, request.UpdateDefinition);
      this._logger.LogInformation("{ToolName}.{Operation} completed: CultureName={CultureName}", (object) nameof (CultureOperationsTool), (object) "Update", (object) request.UpdateDefinition.Name);
      return new CultureOperationResponse()
      {
        Success = operationResult.Success,
        Message = operationResult.Message ?? "Culture update completed",
        Operation = request.Operation,
        CultureName = request.UpdateDefinition.Name,
        Data = (object) operationResult
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      CultureOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new CultureOperationResponse()
      {
        Success = false,
        Message = "Failed to update culture: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private CultureOperationResponse HandleDeleteOperation(CultureOperationRequest request)
  {
    try
    {
      OperationResult operationResult = CultureOperations.DeleteCulture(request.ConnectionName, request.CultureName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: CultureName={CultureName}", (object) nameof (CultureOperationsTool), (object) "Delete", (object) request.CultureName);
      return new CultureOperationResponse()
      {
        Success = operationResult.Success,
        Message = operationResult.Message ?? "Culture deletion completed",
        Operation = request.Operation,
        CultureName = request.CultureName,
        Data = (object) operationResult
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      CultureOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new CultureOperationResponse()
      {
        Success = false,
        Message = "Failed to delete culture: " + ex.Message,
        Operation = request.Operation,
        CultureName = request.CultureName,
        Help = (object) operationMetadata
      };
    }
  }

  private CultureOperationResponse HandleGetOperation(CultureOperationRequest request)
  {
    try
    {
      CultureGet culture = CultureOperations.GetCulture(request.ConnectionName, request.CultureName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: CultureName={CultureName}", (object) nameof (CultureOperationsTool), (object) "Get", (object) request.CultureName);
      return new CultureOperationResponse()
      {
        Success = true,
        Message = $"Culture '{request.CultureName}' retrieved successfully",
        Operation = request.Operation,
        CultureName = request.CultureName,
        Data = (object) culture
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      CultureOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new CultureOperationResponse()
      {
        Success = false,
        Message = "Failed to get culture: " + ex.Message,
        Operation = request.Operation,
        CultureName = request.CultureName,
        Help = (object) operationMetadata
      };
    }
  }

  private CultureOperationResponse HandleListOperation(CultureOperationRequest request)
  {
    try
    {
      List<CultureList> cultureListList = CultureOperations.ListCultures(request.ConnectionName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Count={Count}", (object) nameof (CultureOperationsTool), (object) "List", (object) cultureListList.Count);
      CultureOperationResponse operationResponse = new CultureOperationResponse { Success = true };
      operationResponse.Message = $"Found {cultureListList.Count} culture(s)";
      operationResponse.Operation = request.Operation;
      operationResponse.Data = (object) cultureListList;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      CultureOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new CultureOperationResponse()
      {
        Success = false,
        Message = "Failed to list cultures: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private CultureOperationResponse HandleRenameOperation(CultureOperationRequest request)
  {
    try
    {
      OperationResult operationResult = CultureOperations.RenameCulture(request.ConnectionName, request.CultureName, request.NewCultureName);
      this._logger.LogInformation("{ToolName}.{Operation} completed: From={OldName}, To={NewName}", (object) nameof (CultureOperationsTool), (object) "Rename", (object) request.CultureName, (object) request.NewCultureName);
      return new CultureOperationResponse()
      {
        Success = operationResult.Success,
        Message = operationResult.Message ?? "Culture rename completed",
        Operation = request.Operation,
        CultureName = request.NewCultureName,
        Data = (object) operationResult
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      CultureOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new CultureOperationResponse()
      {
        Success = false,
        Message = "Failed to rename culture: " + ex.Message,
        Operation = request.Operation,
        CultureName = request.CultureName,
        Help = (object) operationMetadata
      };
    }
  }

  private CultureOperationResponse HandleGetValidNamesOperation(CultureOperationRequest request)
  {
    try
    {
      List<string> validCultureNames = CultureOperations.GetValidCultureNames(request.IncludeNeutralCultures, request.IncludeUserCustomCultures);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Count={Count}", (object) nameof (CultureOperationsTool), (object) "GetValidNames", (object) validCultureNames.Count);
      CultureOperationResponse validNamesOperation = new CultureOperationResponse { Success = true };
      validNamesOperation.Message = $"Found {validCultureNames.Count} valid culture name(s)";
      validNamesOperation.Operation = request.Operation;
      validNamesOperation.Data = (object) validCultureNames;
      return validNamesOperation;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      CultureOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new CultureOperationResponse()
      {
        Success = false,
        Message = "Failed to get valid culture names: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private CultureOperationResponse HandleGetValidDetailsOperation(CultureOperationRequest request)
  {
    try
    {
      List<CultureDetails> validCultureDetails = CultureOperations.GetValidCultureDetails(request.IncludeNeutralCultures, request.IncludeUserCustomCultures);
      this._logger.LogInformation("{ToolName}.{Operation} completed: Count={Count}", (object) nameof (CultureOperationsTool), (object) "GetValidDetails", (object) validCultureDetails.Count);
      CultureOperationResponse detailsOperation = new CultureOperationResponse { Success = true };
      detailsOperation.Message = $"Found {validCultureDetails.Count} valid culture(s) with details";
      detailsOperation.Operation = request.Operation;
      detailsOperation.Data = (object) validCultureDetails;
      return detailsOperation;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      CultureOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new CultureOperationResponse()
      {
        Success = false,
        Message = "Failed to get valid culture details: " + ex.Message,
        Operation = request.Operation,
        Help = (object) operationMetadata
      };
    }
  }

  private CultureOperationResponse HandleGetDetailsByNameOperation(CultureOperationRequest request)
  {
    try
    {
      CultureDetails cultureDetailsByName = CultureOperations.GetCultureDetailsByName(request.CultureName);
      if (cultureDetailsByName == null)
        throw new McpException($"Culture '{request.CultureName}' not found or invalid");
      this._logger.LogInformation("{ToolName}.{Operation} completed: CultureName={CultureName}", (object) nameof (CultureOperationsTool), (object) "GetDetailsByName", (object) request.CultureName);
      return new CultureOperationResponse()
      {
        Success = true,
        Message = $"Culture details retrieved for '{request.CultureName}'",
        Operation = request.Operation,
        CultureName = request.CultureName,
        Data = (object) cultureDetailsByName
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      CultureOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new CultureOperationResponse()
      {
        Success = false,
        Message = $"Failed to get culture details for '{request.CultureName}': {ex.Message}",
        Operation = request.Operation,
        CultureName = request.CultureName,
        Help = (object) operationMetadata
      };
    }
  }

  private CultureOperationResponse HandleGetDetailsByLCIDOperation(CultureOperationRequest request)
  {
    try
    {
      CultureDetails cultureDetailsByLcid = CultureOperations.GetCultureDetailsByLCID(request.LCID.Value);
      if (cultureDetailsByLcid == null)
        throw new McpException($"Culture with LCID '{request.LCID}' not found or invalid");
      this._logger.LogInformation("{ToolName}.{Operation} completed: LCID={LCID}, CultureName={CultureName}", (object) nameof (CultureOperationsTool), (object) "GetDetailsByLCID", (object) request.LCID, (object) cultureDetailsByLcid.Name);
      CultureOperationResponse detailsByLcidOperation = new CultureOperationResponse { Success = true };
      detailsByLcidOperation.Message = $"Culture details retrieved for LCID '{request.LCID}'";
      detailsByLcidOperation.Operation = request.Operation;
      detailsByLcidOperation.CultureName = cultureDetailsByLcid.Name;
      detailsByLcidOperation.Data = (object) cultureDetailsByLcid;
      return detailsByLcidOperation;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      CultureOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      CultureOperationResponse detailsByLcidOperation = new CultureOperationResponse { Success = false };
      detailsByLcidOperation.Message = $"Failed to get culture details for LCID '{request.LCID}': {ex.Message}";
      detailsByLcidOperation.Operation = request.Operation;
      detailsByLcidOperation.Help = (object) operationMetadata;
      return detailsByLcidOperation;
    }
  }

  private CultureOperationResponse HandleExportTMDLOperation(CultureOperationRequest request)
  {
    try
    {
      string str = CultureOperations.ExportTMDL(request.ConnectionName, request.CultureName, (ExportTmdl) request.TmdlExportOptions);
      this._logger.LogInformation("{ToolName}.{Operation} completed: CultureName={CultureName}", (object) nameof (CultureOperationsTool), (object) "ExportTMDL", (object) request.CultureName);
      return new CultureOperationResponse()
      {
        Success = true,
        Message = $"TMDL exported for culture '{request.CultureName}'",
        Operation = request.Operation,
        CultureName = request.CultureName,
        Data = (object) str
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      CultureOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new CultureOperationResponse()
      {
        Success = false,
        Message = $"Failed to export TMDL for culture '{request.CultureName}': {ex.Message}",
        Operation = request.Operation,
        CultureName = request.CultureName,
        Help = (object) operationMetadata
      };
    }
  }

  private CultureOperationResponse HandleHelpOperation(
    CultureOperationRequest request,
    string[] operations)
  {
    this._logger.LogInformation("{ToolName}.{Operation} completed: Operations={OperationCount}", (object) nameof (CultureOperationsTool), (object) "Help", (object) operations.Length);
    return new CultureOperationResponse()
    {
      Success = true,
      Message = "Tool description retrieved successfully",
      Operation = request.Operation,
      Help = (object) new
      {
        ToolName = "culture_operations",
        Description = "Perform operations on semantic model cultures.",
        SupportedOperations = operations,
        Examples = Enumerable.Where<KeyValuePair<string, OperationMetadata>>((IEnumerable<KeyValuePair<string, OperationMetadata>>) CultureOperationsTool.toolMetadata.Operations, (Func<KeyValuePair<string, OperationMetadata>, bool>) (p => Enumerable.Contains<string>((IEnumerable<string>) operations, p.Key, (IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase))),
        Notes = new string[4]
        {
          "CultureName is required for most operations except List, GetValidNames, and GetValidDetails.",
          "LCID is required for GetDetailsByLCID operation.",
          "GetValidNames returns culture names only, GetValidDetails returns full culture information including LCIDs.",
          "ExportTMDL exports a culture to TMDL format."
        }
      }
    };
  }

  private bool ValidateRequest(string operation, CultureOperationRequest request)
  {
    OperationMetadata operationMetadata;
    if (!CultureOperationsTool.toolMetadata.Operations.TryGetValue(operation, out operationMetadata))
      return true;
    JsonObject requestDict = JsonSerializer.SerializeToNode<CultureOperationRequest>(request) as JsonObject;
    List<string> list1 = Enumerable.ToList<string>(Enumerable.Where<string>((IEnumerable<string>) operationMetadata.RequiredParams, (p => requestDict != null && requestDict[p] == null)));
    List<string> list2 = Enumerable.ToList<string>(Enumerable.Where<string>((IEnumerable<string>) operationMetadata.ForbiddenParams, (p => requestDict != null && requestDict[p] != null)));
    if (Enumerable.Any<string>((IEnumerable<string>) list1))
      throw new McpException($"Missing required parameters needed for {operation} operation: {string.Join(", ", (IEnumerable<string>) list1)}");
    if (Enumerable.Any<string>((IEnumerable<string>) list2))
      throw new McpException($"Forbidden parameters not allowed for {operation} operation: {string.Join(", ", (IEnumerable<string>) list2)}");
    return true;
  }

  static CultureOperationsTool()
  {
    ToolMetadata toolMetadata1 = new ToolMetadata();
    ToolMetadata toolMetadata2 = toolMetadata1;
    Dictionary<string, OperationMetadata> dictionary1 = new Dictionary<string, OperationMetadata>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    Dictionary<string, OperationMetadata> dictionary2 = dictionary1;
    OperationMetadata operationMetadata1 = new OperationMetadata { Description = "List all cultures in the model.\r\nMandatory properties: None.\r\nOptional: IncludeNeutralCultures, IncludeUserCustomCultures." };
    List<string> stringList1 = new List<string>();
    stringList1.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"List\"\r\n    }\r\n}");
    operationMetadata1.ExampleRequests = stringList1;
    dictionary2["List"] = operationMetadata1;
    Dictionary<string, OperationMetadata> dictionary3 = dictionary1;
    OperationMetadata operationMetadata2 = new OperationMetadata { RequiredParams = new string[1]
    {
      "CultureName"
    } };
    operationMetadata2.Description = "Get details of a specific culture.\r\nMandatory properties: CultureName.\r\nOptional: None.";
    OperationMetadata operationMetadata3 = operationMetadata2;
    List<string> stringList2 = new List<string>();
    stringList2.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Get\",\r\n        \"CultureName\": \"fr-FR\"\r\n    }\r\n}");
    operationMetadata3.ExampleRequests = stringList2;
    OperationMetadata operationMetadata4 = operationMetadata2;
    dictionary3["Get"] = operationMetadata4;
    Dictionary<string, OperationMetadata> dictionary4 = dictionary1;
    OperationMetadata operationMetadata5 = new OperationMetadata { RequiredParams = new string[1]
    {
      "CreateDefinition"
    } };
    operationMetadata5.Description = "Create a new culture.\r\nMandatory properties: CreateDefinition (with Name).\r\nOptional: Annotations, ExtendedProperties.";
    OperationMetadata operationMetadata6 = operationMetadata5;
    List<string> stringList3 = new List<string>();
    stringList3.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Create\",\r\n        \"CreateDefinition\": { \r\n            \"Name\": \"fr-FR\"\r\n        }\r\n    }\r\n}");
    operationMetadata6.ExampleRequests = stringList3;
    OperationMetadata operationMetadata7 = operationMetadata5;
    dictionary4["Create"] = operationMetadata7;
    Dictionary<string, OperationMetadata> dictionary5 = dictionary1;
    OperationMetadata operationMetadata8 = new OperationMetadata { RequiredParams = new string[1]
    {
      "UpdateDefinition"
    } };
    operationMetadata8.Description = "Update an existing culture. Names cannot be changed - use Rename operation instead.\r\nMandatory properties: UpdateDefinition (with Name).\r\nOptional: Annotations, ExtendedProperties.";
    OperationMetadata operationMetadata9 = operationMetadata8;
    List<string> stringList4 = new List<string>();
    stringList4.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Update\",\r\n        \"CultureName\": \"fr-FR\",\r\n        \"UpdateDefinition\": { \r\n            \"Name\": \"fr-FR\"\r\n        }\r\n    }\r\n}");
    operationMetadata9.ExampleRequests = stringList4;
    OperationMetadata operationMetadata10 = operationMetadata8;
    dictionary5["Update"] = operationMetadata10;
    Dictionary<string, OperationMetadata> dictionary6 = dictionary1;
    OperationMetadata operationMetadata11 = new OperationMetadata { RequiredParams = new string[1]
    {
      "CultureName"
    } };
    operationMetadata11.Description = "Delete a culture.\r\nMandatory properties: CultureName.\r\nOptional: None.";
    OperationMetadata operationMetadata12 = operationMetadata11;
    List<string> stringList5 = new List<string>();
    stringList5.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Delete\",\r\n        \"CultureName\": \"ObsoleteCulture\"\r\n    }\r\n}");
    operationMetadata12.ExampleRequests = stringList5;
    OperationMetadata operationMetadata13 = operationMetadata11;
    dictionary6["Delete"] = operationMetadata13;
    Dictionary<string, OperationMetadata> dictionary7 = dictionary1;
    OperationMetadata operationMetadata14 = new OperationMetadata { RequiredParams = new string[2]
    {
      "CultureName",
      "NewCultureName"
    } };
    operationMetadata14.Description = "Rename a culture.\r\nMandatory properties: CultureName, NewCultureName.\r\nOptional: None.";
    OperationMetadata operationMetadata15 = operationMetadata14;
    List<string> stringList6 = new List<string>();
    stringList6.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Rename\",\r\n        \"CultureName\": \"fr-FR\",\r\n        \"NewCultureName\": \"es-ES\"\r\n    }\r\n}");
    operationMetadata15.ExampleRequests = stringList6;
    OperationMetadata operationMetadata16 = operationMetadata14;
    dictionary7["Rename"] = operationMetadata16;
    Dictionary<string, OperationMetadata> dictionary8 = dictionary1;
    OperationMetadata operationMetadata17 = new OperationMetadata { Description = "Get valid culture names.\r\nMandatory properties: None.\r\nOptional: IncludeNeutralCultures, IncludeUserCustomCultures." };
    List<string> stringList7 = new List<string>();
    stringList7.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"GetValidNames\",\r\n        \"IncludeNeutralCultures\": true,\r\n        \"IncludeUserCustomCultures\": false\r\n    }\r\n}");
    operationMetadata17.ExampleRequests = stringList7;
    dictionary8["GetValidNames"] = operationMetadata17;
    Dictionary<string, OperationMetadata> dictionary9 = dictionary1;
    OperationMetadata operationMetadata18 = new OperationMetadata { Description = "Get valid culture details.\r\nMandatory properties: None.\r\nOptional: IncludeNeutralCultures, IncludeUserCustomCultures." };
    List<string> stringList8 = new List<string>();
    stringList8.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"GetValidDetails\"\r\n    }\r\n}");
    operationMetadata18.ExampleRequests = stringList8;
    dictionary9["GetValidDetails"] = operationMetadata18;
    Dictionary<string, OperationMetadata> dictionary10 = dictionary1;
    OperationMetadata operationMetadata19 = new OperationMetadata { RequiredParams = new string[1]
    {
      "CultureName"
    } };
    operationMetadata19.Description = "Get details by culture name.\r\nMandatory properties: CultureName.\r\nOptional: None.";
    OperationMetadata operationMetadata20 = operationMetadata19;
    List<string> stringList9 = new List<string>();
    stringList9.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"GetDetailsByName\",\r\n        \"CultureName\": \"fr-FR\"\r\n    }\r\n}");
    operationMetadata20.ExampleRequests = stringList9;
    OperationMetadata operationMetadata21 = operationMetadata19;
    dictionary10["GetDetailsByName"] = operationMetadata21;
    Dictionary<string, OperationMetadata> dictionary11 = dictionary1;
    OperationMetadata operationMetadata22 = new OperationMetadata { RequiredParams = new string[1]
    {
      "LCID"
    } };
    operationMetadata22.Description = "Get details by LCID.\r\nMandatory properties: LCID.\r\nOptional: None.";
    OperationMetadata operationMetadata23 = operationMetadata22;
    List<string> stringList10 = new List<string>();
    stringList10.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"GetDetailsByLCID\",\r\n        \"LCID\": 1033\r\n    }\r\n}");
    operationMetadata23.ExampleRequests = stringList10;
    OperationMetadata operationMetadata24 = operationMetadata22;
    dictionary11["GetDetailsByLCID"] = operationMetadata24;
    Dictionary<string, OperationMetadata> dictionary12 = dictionary1;
    OperationMetadata operationMetadata25 = new OperationMetadata { RequiredParams = new string[1]
    {
      "CultureName"
    } };
    operationMetadata25.Description = "Export culture to TMDL format.\r\nMandatory properties: CultureName.\r\nOptional: TmdlExportOptions.";
    OperationMetadata operationMetadata26 = operationMetadata25;
    List<string> stringList11 = new List<string>();
    stringList11.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"ExportTMDL\",\r\n        \"CultureName\": \"fr-FR\"\r\n    }\r\n}");
    operationMetadata26.ExampleRequests = stringList11;
    OperationMetadata operationMetadata27 = operationMetadata25;
    dictionary12["ExportTMDL"] = operationMetadata27;
    Dictionary<string, OperationMetadata> dictionary13 = dictionary1;
    OperationMetadata operationMetadata28 = new OperationMetadata { Description = "Describe the tool and its operations.\r\nMandatory properties: None.\r\nOptional: None." };
    List<string> stringList12 = new List<string>();
    stringList12.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Help\"\r\n    }\r\n}");
    operationMetadata28.ExampleRequests = stringList12;
    dictionary13["Help"] = operationMetadata28;
    Dictionary<string, OperationMetadata> dictionary14 = dictionary1;
    toolMetadata2.Operations = dictionary14;
    CultureOperationsTool.toolMetadata = toolMetadata1;
  }
}
