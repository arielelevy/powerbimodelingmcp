// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using Azure.Core;
using Azure.Identity;
using ModelContextProtocol;
using System;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
namespace PowerBIModelingMCP.Library.Core;

public static class FabricAuthService
{
  private static readonly string[] RequiredScopes = new string[1]
  {
    "https://api.fabric.microsoft.com/.default"
  };
  private static TokenCredential? _credential;
  private static readonly object _lock = new object();

  public static TokenCredential TokenCredential
  {
    get
    {
      lock (FabricAuthService._lock)
      {
        if (FabricAuthService._credential == null)
        {
          InteractiveBrowserCredentialOptions options = new InteractiveBrowserCredentialOptions();
          string str = Environment.GetEnvironmentVariable("FABRIC_TENANT_ID") ?? Environment.GetEnvironmentVariable("AZURE_TENANT_ID");
          if (!string.IsNullOrWhiteSpace(str))
            options.TenantId = str;
          FabricAuthService._credential = (TokenCredential) new InteractiveBrowserCredential(options);
        }
        return FabricAuthService._credential;
      }
    }
  }

  public static async Task<string> GetAccessTokenAsync(
    string? overrideToken = null,
    CancellationToken cancellationToken = default (CancellationToken))
  {
    string environmentVariable = Environment.GetEnvironmentVariable("FABRIC_ACCESS_TOKEN");
    if (!string.IsNullOrWhiteSpace(environmentVariable))
    {
      Console.Error.WriteLine("[INFO] Using access token from FABRIC_ACCESS_TOKEN environment variable");
      return environmentVariable;
    }
    if (!string.IsNullOrWhiteSpace(overrideToken))
      return overrideToken;
    try
    {
      return (await FabricAuthService.TokenCredential.GetTokenAsync(new TokenRequestContext(FabricAuthService.RequiredScopes), cancellationToken)).Token;
    }
    catch (Exception ex)
    {
      Console.Error.WriteLine($"[ERROR] FabricAuthService.GetAccessTokenAsync: {ex}");
      throw new McpException("Failed to acquire Fabric access token via interactive browser authentication. Ensure you have Fabric admin permissions and can log in through the browser. Error: " + ex.Message);
    }
  }

  public static async Task<string> GetAccessTokenWithInteractiveFallbackAsync(
    CancellationToken cancellationToken = default (CancellationToken))
  {
    string environmentVariable = Environment.GetEnvironmentVariable("FABRIC_ACCESS_TOKEN");
    if (!string.IsNullOrWhiteSpace(environmentVariable))
    {
      Console.Error.WriteLine("[INFO] Using access token from FABRIC_ACCESS_TOKEN environment variable (interactive fallback)");
      return environmentVariable;
    }
    try
    {
      Console.WriteLine("[INFO] Attempting interactive browser authentication for Fabric admin access...");
      InteractiveBrowserCredentialOptions options = new InteractiveBrowserCredentialOptions();
      string str = Environment.GetEnvironmentVariable("FABRIC_TENANT_ID") ?? Environment.GetEnvironmentVariable("AZURE_TENANT_ID");
      if (!string.IsNullOrWhiteSpace(str))
        options.TenantId = str;
      AccessToken tokenAsync = await new InteractiveBrowserCredential(options).GetTokenAsync(new TokenRequestContext(FabricAuthService.RequiredScopes), cancellationToken);
      Console.WriteLine("[INFO] Successfully obtained Fabric access token via interactive browser authentication.");
      return tokenAsync.Token;
    }
    catch (Exception ex)
    {
      Console.Error.WriteLine($"[ERROR] FabricAuthService.GetAccessTokenWithInteractiveFallbackAsync: {ex}");
      throw new McpException("Failed to acquire Fabric access token via interactive browser authentication. Ensure you have Fabric admin permissions in your Azure AD tenant. Error: " + ex.Message);
    }
  }

  public static void ClearCredential()
  {
    lock (FabricAuthService._lock)
      FabricAuthService._credential = (TokenCredential) null;
  }
}
