// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Services.PromptRegistrationService
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PowerBIModelingMCP.Library.Contracts;
using PowerBIModelingMCP.Library.Prompts;

#nullable enable
namespace PowerBIModelingMCP.Library.Services;

public class PromptRegistrationService
{
  private readonly MCPServerConfiguration _config;
  private readonly ILogger<PromptRegistrationService> _logger;

  public PromptRegistrationService(
    MCPServerConfiguration config,
    ILogger<PromptRegistrationService> logger)
  {
    this._config = config;
    this._logger = logger;
  }

  public void RegisterPrompts(IMcpServerBuilder mcpBuilder)
  {
    this._logger.LogInformation("=== Prompt Registration Started ===");
    PromptsConfiguration prompts = this._config.Prompts;
    this._logger.LogInformation("Registering prompts...");
    if (prompts.EnableConnectionPrompts)
    {
      this._logger.LogInformation("Loading common user scenario prompts...");
      mcpBuilder.WithPrompts<ConnectionPrompts>();
    }
    if (prompts.EnableDaxQueryPrompts)
    {
      this._logger.LogInformation("Loading DAX query prompts...");
      mcpBuilder.WithPrompts<DaxQueryPrompts>();
    }
    this._logger.LogInformation("=== Prompt Registration Completed ===");
  }
}
