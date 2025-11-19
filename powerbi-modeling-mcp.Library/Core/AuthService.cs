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

public static class AuthService
{
  public static readonly string DefaultClientId = "46abaad5-7937-47fe-922d-8dcaca09f642";
  private static readonly string[] RequiredScopes = new string[1]
  {
    "https://analysis.windows.net/powerbi/api/.default"
  };
  private static TokenCredential? _credential;
  private static readonly object _lock = new object();

  public static TokenCredential TokenCredential
  {
    get
    {
      lock (AuthService._lock)
      {
        if (AuthService._credential == null)
        {
          string environmentVariable = Environment.GetEnvironmentVariable("PBI_MODELING_MCP_CLIENT_ID");
          string str = !string.IsNullOrEmpty(environmentVariable) ? environmentVariable : AuthService.DefaultClientId;
          AuthService._credential = (TokenCredential) new InteractiveBrowserCredential(new InteractiveBrowserCredentialOptions()
          {
            ClientId = str
          });
        }
        return AuthService._credential;
      }
    }
  }

  public static async Task<string> GetAccessTokenAsync(
    string? overrideToken = null,
    bool clearCredential = true,
    CancellationToken cancellationToken = default (CancellationToken))
  {
    string token;
    try
    {
      if (clearCredential)
        AuthService.ClearCredential();
      token = (await AuthService.TokenCredential.GetTokenAsync(new TokenRequestContext(AuthService.RequiredScopes), cancellationToken)).Token;
    }
    catch (Exception ex)
    {
      throw new McpException("Failed to acquire access token via interactive browser authentication. Ensure you have permissions and can log in through the browser. Error: " + ex.Message);
    }
    return token;
  }

  public static void ClearCredential()
  {
    lock (AuthService._lock)
      AuthService._credential = (TokenCredential) null;
  }
}
