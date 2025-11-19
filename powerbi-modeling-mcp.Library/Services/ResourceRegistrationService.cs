// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using PowerBIModelingMCP.Library.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

#nullable enable
namespace PowerBIModelingMCP.Library.Services;

public class ResourceRegistrationService
{
  private readonly MarkdownResourceParser _parser;
  private readonly MCPServerConfiguration _config;
  private readonly ILogger<ResourceRegistrationService> _logger;
  private readonly Dictionary<string, ParsedResourceDefinition> _loadedResources = new Dictionary<string, ParsedResourceDefinition>();

  public ResourceRegistrationService(
    MarkdownResourceParser parser,
    MCPServerConfiguration config,
    ILogger<ResourceRegistrationService> logger)
  {
    this._parser = parser;
    this._config = config;
    this._logger = logger;
  }

  public void RegisterResources(IMcpServerBuilder mcpBuilder)
  {
    this._logger.LogInformation("=== Resource Registration Started ===");
    ResourcesConfiguration resources = this._config.Resources;
    if (this._config.Resources.EnableDynamicResourceLoading)
    {
      this._logger.LogInformation("Loading resources dynamically...");
      this.LoadAndRegisterResourcesFromFolder(mcpBuilder).GetAwaiter().GetResult();
    }
    else
    {
      this._logger.LogInformation("Registering static resources...");
      mcpBuilder.WithResources<ResourceLoader>();
    }
    this._logger.LogInformation("=== Resource Registration Completed ===");
  }

  private async Task LoadAndRegisterResourcesFromFolder(IMcpServerBuilder mcpBuilder)
  {
    try
    {
      string resourcesDirectory = Path.Combine(AppContext.BaseDirectory, "Resources");
      this._logger.LogInformation("Loading markdown resources from: {Directory}", (object) resourcesDirectory);
      if (Directory.Exists(resourcesDirectory))
      {
        await this.LoadResourcesFromDirectoryAsync(resourcesDirectory);
        if (this._loadedResources.Count <= 0)
          return;
        this.RegisterLoadedResources(mcpBuilder, (IReadOnlyDictionary<string, ParsedResourceDefinition>) this._loadedResources);
        this._logger.LogInformation("Registered {Count} markdown resources", (object) this._loadedResources.Count);
      }
      else
        this._logger.LogWarning("Resources directory not found: {Directory}", (object) resourcesDirectory);
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to load markdown resources");
    }
  }

  private async Task LoadResourcesFromDirectoryAsync(string resourcesDirectory)
  {
    try
    {
      if (!Directory.Exists(resourcesDirectory))
      {
        this._logger.LogWarning("Resources directory not found: " + resourcesDirectory);
      }
      else
      {
        List<ParsedResourceDefinition> list = Enumerable.ToList<ParsedResourceDefinition>(await this._parser.ParseDirectoryAsync(resourcesDirectory));
        if (list.Count == 0)
        {
          this._logger.LogInformation("No resource files found in directory: " + resourcesDirectory);
        }
        else
        {
          this._logger.LogInformation("Found {Count} resource definitions", (object) list.Count);
          this._loadedResources.Clear();
          foreach (ParsedResourceDefinition resourceDefinition in list)
          {
            this._loadedResources[resourceDefinition.Name] = resourceDefinition;
            this._logger.LogInformation("Loaded resource: {Name} - {Description}", (object) resourceDefinition.Name, (object) resourceDefinition.Description);
          }
          this._logger.LogInformation("Successfully loaded {Count} resources from markdown files", (object) this._loadedResources.Count);
        }
      }
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, "Failed to load resources from markdown files");
    }
  }

  private void RegisterLoadedResources(
    IMcpServerBuilder mcpBuilder,
    IReadOnlyDictionary<string, ParsedResourceDefinition> resources)
  {
    List<McpServerResource> target = new List<McpServerResource>();
    foreach (KeyValuePair<string, ParsedResourceDefinition> resource in (IEnumerable<KeyValuePair<string, ParsedResourceDefinition>>) resources)
      target.Add(this.CreateResource(resource.Value));
    mcpBuilder.WithResources<List<McpServerResource>>(target);
  }

  private McpServerResource CreateResource(ParsedResourceDefinition resourceDefinition)
  {
    return McpServerResource.Create((Delegate) (() => resourceDefinition.Text), new McpServerResourceCreateOptions()
    {
      Name = resourceDefinition.Name,
      Description = resourceDefinition.Description,
      UriTemplate = resourceDefinition.UriTemplate,
      MimeType = resourceDefinition.MimeType
    });
  }
}
