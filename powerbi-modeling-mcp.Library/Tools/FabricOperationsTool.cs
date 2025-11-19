// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Tools.FabricOperationsTool
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
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;

#nullable enable
namespace PowerBIModelingMCP.Library.Tools;

[McpServerToolType]
public class FabricOperationsTool
{
  private readonly ILogger<FabricOperationsTool> _logger;
  public static readonly ToolMetadata toolMetadata;

  public FabricOperationsTool(ILogger<FabricOperationsTool> logger) => this._logger = logger;

  public FabricOperationResponse ExecuteFabricOperation(
    McpServer mcpServer,
    FabricOperationRequest request)
  {
    this._logger.LogDebug("Executing {ToolName}.{Operation}: Workspace={WorkspaceId}, Item={ItemId}, Connection={ConnectionName}", (object) nameof (FabricOperationsTool), (object) request.Operation, (object) request.WorkspaceId, (object) request.ItemId, (object) "(N/A)");
    try
    {
      string[] strArray = new string[7]
      {
        "LISTWORKSPACES",
        "LISTITEMS",
        "LISTSEMANTICMODELS",
        "GETWORKSPACE",
        "GETITEM",
        "GETSEMANTICMODEL",
        "HELP"
      };
      string upperInvariant = request.Operation.ToUpperInvariant();
      if (!Enumerable.Contains<string>((IEnumerable<string>) strArray, upperInvariant))
      {
        this._logger.LogWarning("Invalid operation '{Operation}' requested for {ToolName}. Valid operations: {ValidOperations}", (object) request.Operation, (object) nameof (FabricOperationsTool), (object) string.Join(", ", strArray));
        return new FabricOperationResponse()
        {
          Success = false,
          Message = $"Invalid operation: {request.Operation}. Supported operations: {string.Join(", ", strArray)}",
          Operation = request.Operation
        };
      }
      if (!this.ValidateRequest(request.Operation, request))
        throw new McpException($"Invalid request for {request.Operation} operation.");
      FabricOperationResponse operationResponse;
      if (upperInvariant != null)
      {
        switch (upperInvariant.Length)
        {
          case 4:
            if ((upperInvariant == "HELP"))
            {
              operationResponse = this.HandleHelpOperation(request);
              goto label_22;
            }
            break;
          case 7:
            if ((upperInvariant == "GETITEM"))
            {
              operationResponse = this.HandleGetItemOperation(request);
              goto label_22;
            }
            break;
          case 9:
            if ((upperInvariant == "LISTITEMS"))
            {
              operationResponse = this.HandleListItemsOperation(request);
              goto label_22;
            }
            break;
          case 12:
            if ((upperInvariant == "GETWORKSPACE"))
            {
              operationResponse = this.HandleGetWorkspaceOperation(request);
              goto label_22;
            }
            break;
          case 14:
            if ((upperInvariant == "LISTWORKSPACES"))
            {
              operationResponse = this.HandleListWorkspacesOperation(request);
              goto label_22;
            }
            break;
          case 16 /*0x10*/:
            if ((upperInvariant == "GETSEMANTICMODEL"))
            {
              operationResponse = this.HandleGetSemanticModelOperation(request);
              goto label_22;
            }
            break;
          case 18:
            if ((upperInvariant == "LISTSEMANTICMODELS"))
            {
              operationResponse = this.HandleListSemanticModelsOperation(request);
              goto label_22;
            }
            break;
        }
      }
      operationResponse = new FabricOperationResponse()
      {
        Success = false,
        Message = $"Unknown operation: {request.Operation}. Supported operations are: Help, ListWorkspaces, GetWorkspace, ListItems, GetItem, ListSemanticModels, GetSemanticModel",
        Operation = request.Operation
      };
label_22:
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Error executing {ToolName}.{Operation}: {ErrorMessage}", (object) nameof (FabricOperationsTool), (object) request.Operation, (object) ex.Message);
      Console.Error.WriteLine($"[ERROR] FabricOperationsTool.ExecuteFabricOperation: {ex}");
      return new FabricOperationResponse()
      {
        Success = false,
        Message = $"Error executing {request.Operation}: {ex.Message}",
        Operation = request.Operation
      };
    }
  }

  private FabricOperationResponse HandleHelpOperation(FabricOperationRequest request)
  {
    this._logger.LogInformation("{ToolName}.{Operation} completed: Operations={OperationCount}", (object) nameof (FabricOperationsTool), (object) request.Operation, (object) FabricOperationsTool.toolMetadata.Operations.Keys.Count);
    return new FabricOperationResponse()
    {
      Success = true,
      Message = "Tool description retrieved successfully",
      Operation = "Help",
      Help = (object) new
      {
        ToolName = "fabric_operations",
        Description = "List Fabric workspaces, items, and semantic models with automatic pagination (v1 REST).",
        RequiredPermissions = "Appropriate Fabric service / admin rights for the requested resources.",
        SupportedOperations = Enumerable.ToList<string>((IEnumerable<string>) FabricOperationsTool.toolMetadata.Operations.Keys),
        Authentication = new
        {
          TokenPrecedence = new string[3]
          {
            "1. FABRIC_ACCESS_TOKEN (pre-acquired bearer token)",
            "2. AccessToken property in request (override)",
            "3. Interactive browser login (prompt if needed)"
          },
          Scopes = new string[1]
          {
            "https://api.fabric.microsoft.com/.default"
          },
          EnvironmentVariables = new string[3]
          {
            "FABRIC_ACCESS_TOKEN - Optional pre-fetched access token",
            "FABRIC_TENANT_ID or AZURE_TENANT_ID - Tenant to authenticate against (optional)",
            "FABRIC_BASE_URL - Override base API URL (default https://api.fabric.microsoft.com)"
          }
        },
        Behavior = new
        {
          Pagination = "Automatic; follows continuationToken until exhausted.",
          WorkspaceSchema = "Uses 'value' array and 'displayName' fallback for workspace name."
        },
        Examples = FabricOperationsTool.toolMetadata.Operations,
        Notes = new string[6]
        {
          "ListWorkspaces supports optional Type filter (personal, workspace, adminworkspace).",
          "ListItems/ ListSemanticModels accept optional WorkspaceId filter.",
          "Semantic model detection treats 'SemanticModel' and legacy 'Dataset' types equivalently.",
          "Get operations require exact IDs - use List operations first to obtain IDs.",
          "All workspace, item, and model identifiers must be valid GUIDs.",
          "Set FABRIC_ACCESS_TOKEN to avoid interactive prompts in automated scenarios."
        }
      }
    };
  }

  private FabricOperationResponse HandleListWorkspacesOperation(FabricOperationRequest request)
  {
    try
    {
      WorkspacesListResult result = FabricOperations.ListWorkspacesAsync(request.AccessToken).GetAwaiter().GetResult();
      this._logger.LogInformation("{ToolName}.{Operation} completed: Count={Count}", (object) nameof (FabricOperationsTool), (object) request.Operation, (object) result.Count);
      FabricOperationResponse operationResponse = new FabricOperationResponse { Success = true };
      operationResponse.Message = $"Successfully retrieved {result.Count} workspaces";
      operationResponse.Operation = "ListWorkspaces";
      operationResponse.Data = (object) result;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      FabricOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new FabricOperationResponse()
      {
        Success = false,
        Message = "Failed to list workspaces: " + ex.Message,
        Operation = "ListWorkspaces",
        Help = (object) operationMetadata
      };
    }
  }

  private FabricOperationResponse HandleListItemsOperation(FabricOperationRequest request)
  {
    try
    {
      ItemsListResult result = FabricOperations.ListItemsAsync(request.WorkspaceId, request.AccessToken).GetAwaiter().GetResult();
      this._logger.LogInformation("{ToolName}.{Operation} completed: Count={Count}", (object) nameof (FabricOperationsTool), (object) request.Operation, (object) result.Count);
      FabricOperationResponse operationResponse = new FabricOperationResponse { Success = true };
      operationResponse.Message = $"Successfully retrieved {result.Count} items";
      operationResponse.Operation = "ListItems";
      operationResponse.Data = (object) result;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      FabricOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new FabricOperationResponse()
      {
        Success = false,
        Message = "Failed to list items: " + ex.Message,
        Operation = "ListItems",
        Help = (object) operationMetadata
      };
    }
  }

  private FabricOperationResponse HandleListSemanticModelsOperation(FabricOperationRequest request)
  {
    try
    {
      SemanticModelsListResult result = FabricOperations.ListSemanticModelsAsync(request.WorkspaceId, request.AccessToken).GetAwaiter().GetResult();
      this._logger.LogInformation("{ToolName}.{Operation} completed: Count={Count}", (object) nameof (FabricOperationsTool), (object) request.Operation, (object) result.Count);
      FabricOperationResponse operationResponse = new FabricOperationResponse { Success = true };
      operationResponse.Message = $"Successfully retrieved {result.Count} semantic models";
      operationResponse.Operation = "ListSemanticModels";
      operationResponse.Data = (object) result;
      return operationResponse;
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      FabricOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new FabricOperationResponse()
      {
        Success = false,
        Message = "Failed to list semantic models: " + ex.Message,
        Operation = "ListSemanticModels",
        Help = (object) operationMetadata
      };
    }
  }

  private FabricOperationResponse HandleGetWorkspaceOperation(FabricOperationRequest request)
  {
    try
    {
      FabricWorkspaceGet result = FabricOperations.GetWorkspaceAsync(request.WorkspaceId.Value, request.AccessToken).GetAwaiter().GetResult();
      if (result == null)
        throw new McpException($"Workspace '{request.WorkspaceId}' not found");
      this._logger.LogInformation("{ToolName}.{Operation} completed: Workspace={WorkspaceId}", (object) nameof (FabricOperationsTool), (object) request.Operation, (object) request.WorkspaceId);
      return new FabricOperationResponse()
      {
        Success = true,
        Message = "Workspace retrieved successfully",
        Operation = "GetWorkspace",
        Data = (object) result
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      FabricOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new FabricOperationResponse()
      {
        Success = false,
        Message = "Failed to get workspace: " + ex.Message,
        Operation = "GetWorkspace",
        Help = (object) operationMetadata
      };
    }
  }

  private FabricOperationResponse HandleGetItemOperation(FabricOperationRequest request)
  {
    try
    {
      Guid? nullable = request.WorkspaceId;
      Guid workspaceId = nullable.Value;
      nullable = request.ItemId;
      Guid itemId = nullable.Value;
      string itemType = request.ItemType;
      string accessToken = request.AccessToken;
      CancellationToken cancellationToken = new CancellationToken();
      FabricItemGet result = FabricOperations.GetItemAsync(workspaceId, itemId, itemType, accessToken, cancellationToken).GetAwaiter().GetResult();
      if (result == null)
        throw new McpException($"Item '{request.ItemId}' not found");
      this._logger.LogInformation("{ToolName}.{Operation} completed: Workspace={WorkspaceId}, Item={ItemId}, Type={ItemType}", (object) nameof (FabricOperationsTool), (object) request.Operation, (object) request.WorkspaceId, (object) request.ItemId, (object) request.ItemType);
      return new FabricOperationResponse()
      {
        Success = true,
        Message = "Item retrieved successfully",
        Operation = "GetItem",
        Data = (object) result
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      FabricOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new FabricOperationResponse()
      {
        Success = false,
        Message = "Failed to get item: " + ex.Message,
        Operation = "GetItem",
        Help = (object) operationMetadata
      };
    }
  }

  private FabricOperationResponse HandleGetSemanticModelOperation(FabricOperationRequest request)
  {
    try
    {
      Guid? nullable = request.WorkspaceId;
      Guid workspaceId = nullable.Value;
      nullable = request.ModelId;
      Guid modelId = nullable.Value;
      string accessToken = request.AccessToken;
      CancellationToken cancellationToken = new CancellationToken();
      FabricSemanticModelGet result = FabricOperations.GetSemanticModelAsync(workspaceId, modelId, accessToken, cancellationToken).GetAwaiter().GetResult();
      if (result == null)
        throw new McpException($"Semantic model '{request.ModelId}' not found");
      this._logger.LogInformation("{ToolName}.{Operation} completed: Workspace={WorkspaceId}, Model={ModelId}", (object) nameof (FabricOperationsTool), (object) request.Operation, (object) request.WorkspaceId, (object) request.ModelId);
      return new FabricOperationResponse()
      {
        Success = true,
        Message = "Semantic model retrieved successfully",
        Operation = "GetSemanticModel",
        Data = (object) result
      };
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to execute {Operation} operation: {ErrorMessage}", (object) request.Operation, (object) ex.Message);
      OperationMetadata operationMetadata;
      FabricOperationsTool.toolMetadata.Operations.TryGetValue(request.Operation, out operationMetadata);
      return new FabricOperationResponse()
      {
        Success = false,
        Message = "Failed to get semantic model: " + ex.Message,
        Operation = "GetSemanticModel",
        Help = (object) operationMetadata
      };
    }
  }

  private bool ValidateRequest(string operation, FabricOperationRequest request)
  {
    OperationMetadata operationMetadata;
    if (!FabricOperationsTool.toolMetadata.Operations.TryGetValue(operation, out operationMetadata))
      return true;
    JsonObject requestDict = JsonSerializer.SerializeToNode<FabricOperationRequest>(request) as JsonObject;
    List<string> list1 = Enumerable.ToList<string>(Enumerable.Where<string>((IEnumerable<string>) operationMetadata.RequiredParams, (p => requestDict != null && requestDict[p] == null)));
    List<string> list2 = Enumerable.ToList<string>(Enumerable.Where<string>((IEnumerable<string>) operationMetadata.ForbiddenParams, (p => requestDict != null && requestDict[p] != null)));
    if (Enumerable.Any<string>((IEnumerable<string>) list1))
      throw new McpException($"Missing required parameters needed for {operation} operation: {string.Join(", ", (IEnumerable<string>) list1)}");
    if (Enumerable.Any<string>((IEnumerable<string>) list2))
      throw new McpException($"Forbidden parameters not allowed for {operation} operation: {string.Join(", ", (IEnumerable<string>) list2)}");
    return true;
  }

  static FabricOperationsTool()
  {
    ToolMetadata toolMetadata1 = new ToolMetadata();
    ToolMetadata toolMetadata2 = toolMetadata1;
    Dictionary<string, OperationMetadata> dictionary1 = new Dictionary<string, OperationMetadata>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    Dictionary<string, OperationMetadata> dictionary2 = dictionary1;
    OperationMetadata operationMetadata1 = new OperationMetadata { Description = "List all accessible Fabric workspaces. \r\nMandatory properties: None. \r\nOptional: AccessToken." };
    operationMetadata1.Tips = new string[3]
    {
      "No parameters required",
      "Automatically handles pagination",
      "Returns all accessible workspaces"
    };
    OperationMetadata operationMetadata2 = operationMetadata1;
    List<string> stringList1 = new List<string>();
    stringList1.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"ListWorkspaces\"\r\n    }\r\n}");
    stringList1.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"ListWorkspaces\",\r\n        \"AccessToken\": \"your-token-here\"\r\n    }\r\n}");
    operationMetadata2.ExampleRequests = stringList1;
    OperationMetadata operationMetadata3 = operationMetadata1;
    dictionary2["ListWorkspaces"] = operationMetadata3;
    Dictionary<string, OperationMetadata> dictionary3 = dictionary1;
    OperationMetadata operationMetadata4 = new OperationMetadata { RequiredParams = new string[1]
    {
      "WorkspaceId"
    } };
    operationMetadata4.Description = "List all Fabric items within a specified workspace. \r\nMandatory properties: WorkspaceId. \r\nOptional: AccessToken.";
    operationMetadata4.CommonMistakes = new string[2]
    {
      "Forgetting to provide WorkspaceId parameter",
      "Using invalid GUID format for WorkspaceId"
    };
    operationMetadata4.Tips = new string[2]
    {
      "Get WorkspaceId from ListWorkspaces operation first",
      "Returns all item types in the workspace"
    };
    OperationMetadata operationMetadata5 = operationMetadata4;
    List<string> stringList2 = new List<string>();
    stringList2.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"ListItems\",\r\n        \"WorkspaceId\": \"12345678-1234-1234-1234-123456789012\"\r\n    }\r\n}");
    stringList2.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"ListItems\",\r\n        \"WorkspaceId\": \"12345678-1234-1234-1234-123456789012\",\r\n        \"AccessToken\": \"your-token-here\"\r\n    }\r\n}");
    operationMetadata5.ExampleRequests = stringList2;
    OperationMetadata operationMetadata6 = operationMetadata4;
    dictionary3["ListItems"] = operationMetadata6;
    Dictionary<string, OperationMetadata> dictionary4 = dictionary1;
    OperationMetadata operationMetadata7 = new OperationMetadata { Description = "List semantic models within a workspace or across the tenant. \r\nMandatory properties: None. \r\nOptional: WorkspaceId, AccessToken." };
    operationMetadata7.Tips = new string[4]
    {
      "WorkspaceId is optional - omit to list across entire tenant (requires admin permissions)",
      "Treats 'SemanticModel' and legacy 'Dataset' types equivalently",
      "Use workspace-scoped listing for better performance and lower permissions",
      "Tenant-wide listing uses /v1/items endpoint, workspace-scoped uses /v1/workspaces/{id}/items"
    };
    operationMetadata7.CommonMistakes = new string[3]
    {
      "Attempting tenant-wide listing without sufficient admin permissions",
      "Expecting only 'SemanticModel' type when legacy 'Dataset' items also exist",
      "Not realizing tenant-wide listing may return thousands of results"
    };
    OperationMetadata operationMetadata8 = operationMetadata7;
    List<string> stringList3 = new List<string>();
    stringList3.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"ListSemanticModels\",\r\n        \"WorkspaceId\": \"12345678-1234-1234-1234-123456789012\"\r\n    }\r\n}");
    stringList3.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"ListSemanticModels\"\r\n    }\r\n}");
    operationMetadata8.ExampleRequests = stringList3;
    OperationMetadata operationMetadata9 = operationMetadata7;
    dictionary4["ListSemanticModels"] = operationMetadata9;
    Dictionary<string, OperationMetadata> dictionary5 = dictionary1;
    OperationMetadata operationMetadata10 = new OperationMetadata { RequiredParams = new string[1]
    {
      "WorkspaceId"
    } };
    operationMetadata10.Description = "Get detailed information for a single workspace by its ID. \r\nMandatory properties: WorkspaceId. \r\nOptional: AccessToken.";
    operationMetadata10.CommonMistakes = new string[2]
    {
      "Using workspace name instead of WorkspaceId GUID",
      "Providing invalid GUID format"
    };
    operationMetadata10.Tips = new string[1]
    {
      "Use ListWorkspaces to find the correct WorkspaceId first"
    };
    OperationMetadata operationMetadata11 = operationMetadata10;
    List<string> stringList4 = new List<string>();
    stringList4.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"GetWorkspace\",\r\n        \"WorkspaceId\": \"12345678-1234-1234-1234-123456789012\"\r\n    }\r\n}");
    operationMetadata11.ExampleRequests = stringList4;
    OperationMetadata operationMetadata12 = operationMetadata10;
    dictionary5["GetWorkspace"] = operationMetadata12;
    Dictionary<string, OperationMetadata> dictionary6 = dictionary1;
    OperationMetadata operationMetadata13 = new OperationMetadata { RequiredParams = new string[3]
    {
      "WorkspaceId",
      "ItemId",
      "ItemType"
    } };
    operationMetadata13.Description = "Get detailed information for a single Fabric item by its ID. \r\nMandatory properties: WorkspaceId, ItemId, ItemType. \r\nOptional: AccessToken.";
    operationMetadata13.CommonMistakes = new string[3]
    {
      "Missing ItemType parameter - it's required even though it might seem redundant",
      "Using incorrect ItemType values (must match Fabric item type exactly)",
      "Providing item name instead of ItemId GUID"
    };
    operationMetadata13.Tips = new string[2]
    {
      "Use ListItems to find the correct ItemId and ItemType first",
      "Common ItemType values: SemanticModel, Report, Dashboard, Dataflow, Notebook"
    };
    OperationMetadata operationMetadata14 = operationMetadata13;
    List<string> stringList5 = new List<string>();
    stringList5.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"GetItem\",\r\n        \"WorkspaceId\": \"12345678-1234-1234-1234-123456789012\",\r\n        \"ItemType\": \"SemanticModel\",\r\n        \"ItemId\": \"aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee\"\r\n    }\r\n}");
    operationMetadata14.ExampleRequests = stringList5;
    OperationMetadata operationMetadata15 = operationMetadata13;
    dictionary6["GetItem"] = operationMetadata15;
    Dictionary<string, OperationMetadata> dictionary7 = dictionary1;
    OperationMetadata operationMetadata16 = new OperationMetadata { RequiredParams = new string[2]
    {
      "WorkspaceId",
      "ModelId"
    } };
    operationMetadata16.Description = "Get detailed information for a single semantic model by its ID within a workspace. \r\nMandatory properties: WorkspaceId, ModelId. \r\nOptional: AccessToken.";
    operationMetadata16.CommonMistakes = new string[2]
    {
      "Using model name instead of ModelId GUID",
      "Omitting WorkspaceId - it's always required for semantic model operations"
    };
    operationMetadata16.Tips = new string[2]
    {
      "Use ListSemanticModels to find the correct ModelId first",
      "ModelId and ItemId are the same for semantic model items"
    };
    OperationMetadata operationMetadata17 = operationMetadata16;
    List<string> stringList6 = new List<string>();
    stringList6.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"GetSemanticModel\",\r\n        \"WorkspaceId\": \"12345678-1234-1234-1234-123456789012\",\r\n        \"ModelId\": \"bbbbbbbb-1111-2222-3333-cccccccccccc\"\r\n    }\r\n}");
    operationMetadata17.ExampleRequests = stringList6;
    OperationMetadata operationMetadata18 = operationMetadata16;
    dictionary7["GetSemanticModel"] = operationMetadata18;
    Dictionary<string, OperationMetadata> dictionary8 = dictionary1;
    OperationMetadata operationMetadata19 = new OperationMetadata { Description = "Describe the Fabric operations tool and its available operations. \r\nMandatory properties: None. \r\nOptional: AccessToken." };
    List<string> stringList7 = new List<string>();
    stringList7.Add("{\r\n    \"request\": {\r\n        \"Operation\": \"Help\"\r\n    }\r\n}");
    operationMetadata19.ExampleRequests = stringList7;
    dictionary8["Help"] = operationMetadata19;
    Dictionary<string, OperationMetadata> dictionary9 = dictionary1;
    toolMetadata2.Operations = dictionary9;
    FabricOperationsTool.toolMetadata = toolMetadata1;
  }
}
