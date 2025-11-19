// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using ModelContextProtocol;
using PowerBIModelingMCP.Library.Common.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
namespace PowerBIModelingMCP.Library.Core;

public static class FabricOperations
{
  private static readonly HttpClient _httpClient = new HttpClient();
  private static readonly string _baseUrl = Environment.GetEnvironmentVariable("FABRIC_BASE_URL") ?? "https://api.fabric.microsoft.com";
  private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions()
  {
    PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
  };

  public static async Task<WorkspacesListResult> ListWorkspacesAsync(
    string? accessToken = null,
    CancellationToken cancellationToken = default (CancellationToken))
  {
    List<FabricWorkspaceGet> allWorkspaces = new List<FabricWorkspaceGet>();
    string continuationToken = (string) null;
    do
    {
      FabricOperations.FabricWorkspacesResponse fabricPageAsync = await FabricOperations.GetFabricPageAsync<FabricOperations.FabricWorkspacesResponse>(FabricOperations.BuildWorkspacesUrl(continuationToken), accessToken, cancellationToken);
      List<FabricOperations.FabricWorkspaceResponse> workspaceResponseList = fabricPageAsync?.Value;
      if (workspaceResponseList != null)
      {
        foreach (FabricOperations.FabricWorkspaceResponse apiWorkspace in workspaceResponseList)
          allWorkspaces.Add(FabricOperations.MapToWorkspace(apiWorkspace));
      }
      else if (fabricPageAsync != null)
        Console.Error.WriteLine("[WARN] FabricOperations.ListWorkspacesAsync: Deserialized page but 'value' collection is null. JSON schema may have changed.");
      continuationToken = fabricPageAsync?.ContinuationToken;
    }
    while (!string.IsNullOrEmpty(continuationToken));
    WorkspacesListResult workspacesListResult = new WorkspacesListResult()
    {
      Workspaces = Enumerable.ToList<FabricWorkspaceGet>(Enumerable.OrderBy<FabricWorkspaceGet, string>((IEnumerable<FabricWorkspaceGet>) allWorkspaces, (w => w.Name))),
      Count = allWorkspaces.Count
    };
    allWorkspaces = (List<FabricWorkspaceGet>) null;
    return workspacesListResult;
  }

  public static async Task<ItemsListResult> ListItemsAsync(
    Guid? workspaceId = null,
    string? accessToken = null,
    CancellationToken cancellationToken = default (CancellationToken))
  {
    if (!workspaceId.HasValue)
      throw new ArgumentException("workspaceId is required to list items", nameof (workspaceId));
    List<FabricItemGet> allItems = new List<FabricItemGet>();
    string continuationToken = (string) null;
    do
    {
      FabricOperations.FabricItemsResponse fabricPageAsync = await FabricOperations.GetFabricPageAsync<FabricOperations.FabricItemsResponse>(FabricOperations.BuildItemsUrl(workspaceId, continuationToken), accessToken, cancellationToken);
      List<FabricOperations.FabricItemResponse> fabricItemResponseList = fabricPageAsync?.Value;
      if (fabricItemResponseList != null)
      {
        foreach (FabricOperations.FabricItemResponse apiItem in fabricItemResponseList)
          allItems.Add(FabricOperations.MapToItem(apiItem));
      }
      continuationToken = fabricPageAsync?.ContinuationToken;
    }
    while (!string.IsNullOrEmpty(continuationToken));
    ItemsListResult itemsListResult = new ItemsListResult()
    {
      Items = Enumerable.ToList<FabricItemGet>(Enumerable.OrderBy<FabricItemGet, string>((IEnumerable<FabricItemGet>) allItems, (i => i.Name))),
      Count = allItems.Count
    };
    allItems = (List<FabricItemGet>) null;
    return itemsListResult;
  }

  public static async Task<SemanticModelsListResult> ListSemanticModelsAsync(
    Guid? workspaceId = null,
    string? accessToken = null,
    CancellationToken cancellationToken = default (CancellationToken))
  {
    List<FabricSemanticModelGet> allSemanticModels = new List<FabricSemanticModelGet>();
    string continuationToken = (string) null;
    do
    {
      FabricOperations.FabricItemsResponse fabricPageAsync = await FabricOperations.GetFabricPageAsync<FabricOperations.FabricItemsResponse>(FabricOperations.BuildItemsUrl(workspaceId, continuationToken, "SemanticModel"), accessToken, cancellationToken);
      List<FabricOperations.FabricItemResponse> fabricItemResponseList = fabricPageAsync?.Value;
      if (fabricItemResponseList != null)
      {
        foreach (FabricOperations.FabricItemResponse apiItem in fabricItemResponseList)
        {
          if (FabricOperations.IsSemanticModelType(apiItem.Type))
            allSemanticModels.Add(FabricOperations.MapToSemanticModel(apiItem));
        }
      }
      continuationToken = fabricPageAsync?.ContinuationToken;
    }
    while (!string.IsNullOrEmpty(continuationToken));
    SemanticModelsListResult modelsListResult = new SemanticModelsListResult()
    {
      SemanticModels = Enumerable.ToList<FabricSemanticModelGet>(Enumerable.OrderBy<FabricSemanticModelGet, string>((IEnumerable<FabricSemanticModelGet>) allSemanticModels, (sm => sm.Name))),
      Count = allSemanticModels.Count
    };
    allSemanticModels = (List<FabricSemanticModelGet>) null;
    return modelsListResult;
  }

  public static async Task<FabricWorkspaceGet?> GetWorkspaceAsync(
    Guid workspaceId,
    string? accessToken = null,
    CancellationToken cancellationToken = default (CancellationToken))
  {
    FabricOperations.FabricWorkspaceResponse fabricPageAsync = await FabricOperations.GetFabricPageAsync<FabricOperations.FabricWorkspaceResponse>($"{FabricOperations._baseUrl}/v1/workspaces/{workspaceId}", accessToken, cancellationToken);
    return fabricPageAsync == null ? (FabricWorkspaceGet) null : FabricOperations.MapToWorkspace(fabricPageAsync);
  }

  public static async Task<FabricItemGet?> GetItemAsync(
    Guid workspaceId,
    Guid itemId,
    string itemType,
    string? accessToken = null,
    CancellationToken cancellationToken = default (CancellationToken))
  {
    string segmentForItemType = FabricOperations.GetUrlSegmentForItemType(itemType);
    if (segmentForItemType == null)
      throw new McpException($"Getting details for item type '{itemType}' is not supported using workspace-scoped endpoints.");
    FabricOperations.FabricItemResponse fabricPageAsync = await FabricOperations.GetFabricPageAsync<FabricOperations.FabricItemResponse>($"{FabricOperations._baseUrl}/v1/workspaces/{workspaceId}/{segmentForItemType}/{itemId}", accessToken, cancellationToken);
    return fabricPageAsync == null ? (FabricItemGet) null : FabricOperations.MapToItem(fabricPageAsync);
  }

  public static async Task<FabricSemanticModelGet?> GetSemanticModelAsync(
    Guid workspaceId,
    Guid modelId,
    string? accessToken = null,
    CancellationToken cancellationToken = default (CancellationToken))
  {
    FabricOperations.FabricItemResponse fabricPageAsync = await FabricOperations.GetFabricPageAsync<FabricOperations.FabricItemResponse>($"{FabricOperations._baseUrl}/v1/workspaces/{workspaceId}/semanticModels/{modelId}", accessToken, cancellationToken);
    return fabricPageAsync != null ? FabricOperations.MapToSemanticModel(fabricPageAsync) : (FabricSemanticModelGet) null;
  }

  private static string BuildWorkspacesUrl(string? continuationToken)
  {
    StringBuilder stringBuilder = new StringBuilder(FabricOperations._baseUrl + "/v1/workspaces");
    List<string> stringList = new List<string>();
    if (!string.IsNullOrWhiteSpace(continuationToken))
      stringList.Add("continuationToken=" + Uri.EscapeDataString(continuationToken));
    if (stringList.Count > 0)
      stringBuilder.Append('?').Append(string.Join<string>('&', (IEnumerable<string>) stringList));
    return stringBuilder.ToString();
  }

  private static string BuildItemsUrl(Guid? workspaceId, string? continuationToken, string? type = null)
  {
    StringBuilder stringBuilder1;
    if (!workspaceId.HasValue)
      stringBuilder1 = new StringBuilder(FabricOperations._baseUrl + "/v1/items");
    else
      stringBuilder1 = new StringBuilder($"{FabricOperations._baseUrl}/v1/workspaces/{workspaceId.Value}/items");
    StringBuilder stringBuilder2 = stringBuilder1;
    List<string> stringList = new List<string>();
    if (!string.IsNullOrWhiteSpace(type))
      stringList.Add("type=" + Uri.EscapeDataString(type));
    if (!string.IsNullOrWhiteSpace(continuationToken))
      stringList.Add("continuationToken=" + Uri.EscapeDataString(continuationToken));
    if (stringList.Count > 0)
      stringBuilder2.Append('?').Append(string.Join<string>('&', (IEnumerable<string>) stringList));
    return stringBuilder2.ToString();
  }

  private static async Task<T?> GetWithRetryAsync<T>(
    string url,
    string accessToken,
    CancellationToken cancellationToken)
    where T : class
  {
    TimeSpan delay = TimeSpan.FromSeconds(1.0);
    for (int attempt = 0; attempt <= 3; ++attempt)
    {
      T withRetryAsync;
      int num;
      HttpRequestException? lastException = null;
      try
      {
        using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url))
        {
          request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
          request.Headers.Add("User-Agent", "MCP-PBIModeling");
          Console.Error.WriteLine($"[INFO] Making Fabric API request to: {url} (attempt {attempt + 1}/{4})");
          using (HttpResponseMessage response = await FabricOperations._httpClient.SendAsync(request, cancellationToken))
          {
            if (response.IsSuccessStatusCode)
            {
              withRetryAsync = JsonSerializer.Deserialize<T>(await response.Content.ReadAsStringAsync(cancellationToken), FabricOperations._jsonOptions);
              goto label_26;
            }
            if (response.StatusCode == (HttpStatusCode)429 && attempt < 3)
            {
              await Task.Delay((TimeSpan?) response.Headers.RetryAfter?.Delta ?? TimeSpan.FromSeconds(Math.Pow(2.0, (double) (attempt + 1))), cancellationToken);
              continue;
            }
            string message1 = await response.Content.ReadAsStringAsync(cancellationToken);
            string message2 = $"Fabric API request failed with status {response.StatusCode}: {message1}";
            if (response.StatusCode == (HttpStatusCode)403 && message1.Contains("InsufficientScopes", StringComparison.OrdinalIgnoreCase))
            {
              Console.Error.WriteLine("[ERROR] FabricOperations.GetWithRetryAsync: " + message2);
              throw new InsufficientScopesException(message1);
            }
            Console.Error.WriteLine("[ERROR] FabricOperations.GetWithRetryAsync: " + message2);
            throw new McpException(message2);
          }
        }
      }
      catch (HttpRequestException ex) when (attempt < 3)
      {
        lastException = ex;
        num = 1;
      }
      if (num == 1)
      {
        Console.Error.WriteLine($"[ERROR] FabricOperations.GetWithRetryAsync: HttpRequestException on attempt {attempt + 1}: {lastException?.Message}");
        await Task.Delay(delay, cancellationToken);
        delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 2.0);
        continue;
      }
      continue;
label_26:
      return withRetryAsync;
    }
    string message = $"Fabric API request failed after {4} attempts";
    Console.Error.WriteLine("[ERROR] FabricOperations.GetWithRetryAsync: " + message);
    throw new McpException(message);
  }

  private static async Task<T?> GetFabricPageAsync<T>(
    string url,
    string? accessToken,
    CancellationToken cancellationToken)
    where T : class
  {
    string accessToken1 = accessToken;
    if (accessToken1 == null)
      accessToken1 = await FabricAuthService.GetAccessTokenAsync(cancellationToken: cancellationToken);
    return await FabricOperations.GetWithRetryAsync<T>(url, accessToken1, cancellationToken);
  }

  private static FabricWorkspaceGet MapToWorkspace(
    FabricOperations.FabricWorkspaceResponse apiWorkspace)
  {
    return new FabricWorkspaceGet()
    {
      Id = apiWorkspace.Id,
      Name = apiWorkspace.Name ?? apiWorkspace.DisplayName,
      Description = apiWorkspace.Description,
      Type = apiWorkspace.Type,
      State = apiWorkspace.State,
      CapacityId = apiWorkspace.CapacityId,
      Raw = (object) apiWorkspace
    };
  }

  private static FabricItemGet MapToItem(FabricOperations.FabricItemResponse apiItem)
  {
    return new FabricItemGet()
    {
      Id = apiItem.Id,
      Name = apiItem.Name ?? apiItem.DisplayName,
      Description = apiItem.Description,
      Type = apiItem.Type,
      WorkspaceId = apiItem.WorkspaceId,
      State = apiItem.State,
      Raw = (object) apiItem
    };
  }

  private static FabricSemanticModelGet MapToSemanticModel(
    FabricOperations.FabricItemResponse apiItem)
  {
    FabricSemanticModelGet semanticModel = new FabricSemanticModelGet { Id = apiItem.Id };
    semanticModel.Name = apiItem.Name;
    semanticModel.Description = apiItem.Description;
    semanticModel.Type = apiItem.Type;
    semanticModel.WorkspaceId = apiItem.WorkspaceId;
    semanticModel.State = apiItem.State;
    semanticModel.Raw = (object) apiItem;
    return semanticModel;
  }

  private static bool IsSemanticModelType(string? type)
  {
    if (string.IsNullOrWhiteSpace(type))
      return false;
    return type.Equals("SemanticModel", StringComparison.OrdinalIgnoreCase) || type.Equals("Dataset", StringComparison.OrdinalIgnoreCase);
  }

  private static string? GetUrlSegmentForItemType(string? type)
  {
    if (string.IsNullOrWhiteSpace(type))
      return (string) null;
    string lowerInvariant = type.ToLowerInvariant();
    string segmentForItemType;
    if (lowerInvariant != null)
    {
      switch (lowerInvariant.Length)
      {
        case 6:
          switch (lowerInvariant[2])
          {
            case 'f':
              if ((lowerInvariant == "reflex"))
              {
                segmentForItemType = "reflexes";
                goto label_83;
              }
              goto label_82;
            case 'p':
              if ((lowerInvariant == "report"))
              {
                segmentForItemType = "reports";
                goto label_83;
              }
              goto label_82;
            default:
              goto label_82;
          }
        case 7:
          switch (lowerInvariant[0])
          {
            case 'c':
              if ((lowerInvariant == "copyjob"))
              {
                segmentForItemType = "copyJobs";
                goto label_83;
              }
              goto label_82;
            case 'd':
              if ((lowerInvariant == "dataset"))
                break;
              goto label_82;
            case 'm':
              if ((lowerInvariant == "mlmodel"))
              {
                segmentForItemType = "mlModels";
                goto label_83;
              }
              goto label_82;
            default:
              goto label_82;
          }
          break;
        case 8:
          switch (lowerInvariant[4])
          {
            case 'b':
              if ((lowerInvariant == "notebook"))
              {
                segmentForItemType = "notebooks";
                goto label_83;
              }
              goto label_82;
            case 'f':
              if ((lowerInvariant == "dataflow"))
              {
                segmentForItemType = "dataflows";
                goto label_83;
              }
              goto label_82;
            case 'm':
              if ((lowerInvariant == "datamart"))
              {
                segmentForItemType = "datamarts";
                goto label_83;
              }
              goto label_82;
            default:
              goto label_82;
          }
        case 9:
          switch (lowerInvariant[0])
          {
            case 'd':
              if ((lowerInvariant == "dashboard"))
              {
                segmentForItemType = "dashboards";
                goto label_83;
              }
              goto label_82;
            case 'l':
              if ((lowerInvariant == "lakehouse"))
              {
                segmentForItemType = "lakehouses";
                goto label_83;
              }
              goto label_82;
            case 'w':
              if ((lowerInvariant == "warehouse"))
              {
                segmentForItemType = "warehouses";
                goto label_83;
              }
              goto label_82;
            default:
              goto label_82;
          }
        case 10:
          switch (lowerInvariant[0])
          {
            case 'e':
              if ((lowerInvariant == "eventhouse"))
              {
                segmentForItemType = "eventhouses";
                goto label_83;
              }
              goto label_82;
            case 'g':
              if ((lowerInvariant == "graphqlapi"))
              {
                segmentForItemType = "graphQLApis";
                goto label_83;
              }
              goto label_82;
            default:
              goto label_82;
          }
        case 11:
          switch (lowerInvariant[3])
          {
            case 'd':
              if (!(lowerInvariant == "kqldatabase"))
              {
                if ((lowerInvariant == "sqldatabase"))
                {
                  segmentForItemType = "sqlDatabases";
                  goto label_83;
                }
                goto label_82;
              }
              segmentForItemType = "kqlDatabases";
              goto label_83;
            case 'e':
              if ((lowerInvariant == "sqlendpoint"))
              {
                segmentForItemType = "sqlEndpoints";
                goto label_83;
              }
              goto label_82;
            case 'i':
              if ((lowerInvariant == "environment"))
              {
                segmentForItemType = "environments";
                goto label_83;
              }
              goto label_82;
            case 'n':
              if ((lowerInvariant == "eventstream"))
              {
                segmentForItemType = "eventstreams";
                goto label_83;
              }
              goto label_82;
            case 'q':
              if ((lowerInvariant == "kqlqueryset"))
              {
                segmentForItemType = "kqlQuerysets";
                goto label_83;
              }
              goto label_82;
            default:
              goto label_82;
          }
        case 12:
          switch (lowerInvariant[0])
          {
            case 'd':
              if ((lowerInvariant == "datapipeline"))
              {
                segmentForItemType = "dataPipelines";
                goto label_83;
              }
              goto label_82;
            case 'k':
              if ((lowerInvariant == "kqldashboard"))
              {
                segmentForItemType = "kqlDashboards";
                goto label_83;
              }
              goto label_82;
            case 'm':
              if ((lowerInvariant == "mlexperiment"))
              {
                segmentForItemType = "mlExperiments";
                goto label_83;
              }
              goto label_82;
            default:
              goto label_82;
          }
        case 13:
          if ((lowerInvariant == "semanticmodel"))
            break;
          goto label_82;
        case 15:
          switch (lowerInvariant[0])
          {
            case 'p':
              if ((lowerInvariant == "paginatedreport"))
              {
                segmentForItemType = "paginatedReports";
                goto label_83;
              }
              goto label_82;
            case 'v':
              if ((lowerInvariant == "variablelibrary"))
              {
                segmentForItemType = "variableLibraries";
                goto label_83;
              }
              goto label_82;
            default:
              goto label_82;
          }
        case 16 /*0x10*/:
          switch (lowerInvariant[0])
          {
            case 'a':
              if ((lowerInvariant == "apacheairflowjob"))
              {
                segmentForItemType = "apacheAirflowJobs";
                goto label_83;
              }
              goto label_82;
            case 'm':
              if ((lowerInvariant == "mirroreddatabase"))
              {
                segmentForItemType = "mirroredDatabases";
                goto label_83;
              }
              goto label_82;
            default:
              goto label_82;
          }
        case 17:
          switch (lowerInvariant[0])
          {
            case 'm':
              if ((lowerInvariant == "mirroredwarehouse"))
              {
                segmentForItemType = "mirroredWarehouses";
                goto label_83;
              }
              goto label_82;
            case 'w':
              if ((lowerInvariant == "warehousesnapshot"))
              {
                segmentForItemType = "warehouseSnapshots";
                goto label_83;
              }
              goto label_82;
            default:
              goto label_82;
          }
        case 18:
          switch (lowerInvariant[0])
          {
            case 'd':
              if ((lowerInvariant == "digitaltwinbuilder"))
              {
                segmentForItemType = "digitalTwinBuilders";
                goto label_83;
              }
              goto label_82;
            case 'm':
              if ((lowerInvariant == "mounteddatafactory"))
              {
                segmentForItemType = "mountedDataFactories";
                goto label_83;
              }
              goto label_82;
            case 's':
              if ((lowerInvariant == "sparkjobdefinition"))
              {
                segmentForItemType = "sparkJobDefinitions";
                goto label_83;
              }
              goto label_82;
            default:
              goto label_82;
          }
        case 22:
          if ((lowerInvariant == "digitaltwinbuilderflow"))
          {
            segmentForItemType = "digitalTwinBuilderFlows";
            goto label_83;
          }
          goto label_82;
        case 30:
          if ((lowerInvariant == "mirroredazuredatabrickscatalog"))
          {
            segmentForItemType = "mirroredAzureDatabricksCatalogs";
            goto label_83;
          }
          goto label_82;
        default:
          goto label_82;
      }
      segmentForItemType = "semanticModels";
      goto label_83;
    }
label_82:
    segmentForItemType = (string) null;
label_83:
    return segmentForItemType;
  }

  private class FabricWorkspacesResponse
  {
    [JsonPropertyName("value")]
    public List<FabricOperations.FabricWorkspaceResponse>? Value { get; set; }

    public string? ContinuationToken { get; set; }

    public string? ContinuationUri { get; set; }
  }

  private class FabricWorkspaceResponse
  {
    public Guid Id { get; set; }

    public string? Name { get; set; }

    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    public string? Description { get; set; }

    public string? Type { get; set; }

    public string? State { get; set; }

    public Guid? CapacityId { get; set; }
  }

  private class FabricItemsResponse
  {
    [JsonPropertyName("value")]
    public List<FabricOperations.FabricItemResponse>? Value { get; set; }

    public string? ContinuationToken { get; set; }

    public string? ContinuationUri { get; set; }
  }

  private class FabricItemResponse
  {
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    public string? Name { get; set; }

    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("workspaceId")]
    public Guid? WorkspaceId { get; set; }

    public string? State { get; set; }
  }
}
