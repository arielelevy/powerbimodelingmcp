// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using Microsoft.Extensions.AI;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.Collections.Generic;
using System.ComponentModel;

#nullable enable
namespace PowerBIModelingMCP.Library.Prompts;

[McpServerPromptType]
public class ConnectionPrompts
{
  [McpServerPrompt(Name = "ConnectToFabric")]
  [Description("Connects to a semantic model in a Fabric Workspace.")]
  public IEnumerable<ChatMessage> ConnectToFabric([Description("Name of the workspace to connect to")] string workspaceName, [Description("Name of semantic model within the fabric workspace")] string semanticModelName)
  {
    return (IEnumerable<ChatMessage>) new ChatMessage[1]
    {
      new ChatMessage(ChatRole.User, $"Connect to semantic model '{semanticModelName}' in Fabric workspace '{workspaceName}'.")
    };
  }

  [McpServerPrompt(Name = "ConnectToPowerBIDesktop")]
  [Description("Searches for the Power BI Desktop Analysis Services instance that matches the file name and connects to it.")]
  public IEnumerable<ChatMessage> ConnectToPowerBIDesktop([Description("Name of the Power BI Desktop file")] string name)
  {
    return (IEnumerable<ChatMessage>) new ChatMessage[1]
    {
      new ChatMessage(ChatRole.User, $"Connect to Power BI Desktop with name '{name}'.")
    };
  }

  [McpServerPrompt(Name = "ConnectToPBIP")]
  [Description("Loads the TMDL definition from the semantic model in the Power BI Project (pbip) files.")]
  public IEnumerable<PromptMessage> ConnectToPowerBIProject([Description("Path to PowerBI Project semantic model definition")] string? pbipPath = null)
  {
    PromptMessage[] powerBiProject = new PromptMessage[2];
    TextResourceContents resourceContents = new TextResourceContents { Uri = "resource://powerbi_project_instructions" };
    EmbeddedResourceBlock embeddedResourceBlock = new EmbeddedResourceBlock
    {
      Resource = (ResourceContents) resourceContents
    };
    PromptMessage promptMessage = new PromptMessage { Role = Role.User };
    promptMessage.Content = (ContentBlock) embeddedResourceBlock;
    powerBiProject[0] = promptMessage;
    powerBiProject[1] = new PromptMessage()
    {
      Role = Role.User,
      Content = (ContentBlock) new TextContentBlock()
      {
        Text = (string.IsNullOrEmpty(pbipPath) ? "Look for my semantic model within the workspace and open semantic model from PBIP folder" : $"Open semantic model from PBIP folder '{pbipPath}'.")
      }
    };
    return (IEnumerable<PromptMessage>) powerBiProject;
  }
}
