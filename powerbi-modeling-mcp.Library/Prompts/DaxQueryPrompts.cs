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
public class DaxQueryPrompts
{
  [McpServerPrompt(Name = "RunDAXQueryWithMetrics")]
  [Description("Executes the DAX query with option to clear the cache and return only the execution metrics.")]
  public IEnumerable<ChatMessage> RunDAXQueryWithMetrics([Description("The DAX query text to execute")] string daxQueryText, [Description("Whether to clear the DAX query cache before running the query (yes/no or y/n). Default: yes")] string clearCache = "yes")
  {
    string lowerInvariant = clearCache.Trim().ToLowerInvariant();
    return (IEnumerable<ChatMessage>) new ChatMessage[1]
    {
      new ChatMessage(ChatRole.User, $"{((lowerInvariant == "yes") || (lowerInvariant == "y") ? "Clear the DAX query cache, then run" : "Run") + " the DAX query and return ONLY the execution metrics."}\n{"Wrap the raw `executionMetrics` JSON in a single fenced code block with `json`."}\n{daxQueryText}")
    };
  }

  [McpServerPrompt(Name = "AnalyzeDAXQuery")]
  [Description("Analyzes DAX query performance by running it with a cleared cache and reviewing execution metrics for potential issues.")]
  public IEnumerable<ChatMessage> AnalyzeDAXQuery([Description("The DAX query text to analyze")] string daxQueryText)
  {
    return (IEnumerable<ChatMessage>) new ChatMessage[1]
    {
      new ChatMessage(ChatRole.User, "Clear the DAX query cache, then run the DAX query and return the execution metrics. Present all fields exactly as returned, then analyze the metrics to identify potential performance issues.\n" + daxQueryText)
    };
  }

  [McpServerPrompt(Name = "CreateDAXQuery")]
  [Description("Creates a DAX query based on your semantic model and natural language question.")]
  public IEnumerable<PromptMessage> CreateDAXQuery([Description("Natural language description of the data you want to query")] string request)
  {
    PromptMessage[] daxQuery = new PromptMessage[2];
    TextResourceContents resourceContents = new TextResourceContents { Uri = "resource://dax_query_instructions_and_examples" };
    EmbeddedResourceBlock embeddedResourceBlock = new EmbeddedResourceBlock
    {
      Resource = (ResourceContents) resourceContents
    };
    PromptMessage promptMessage = new PromptMessage { Role = Role.User };
    promptMessage.Content = (ContentBlock) embeddedResourceBlock;
    daxQuery[0] = promptMessage;
    daxQuery[1] = new PromptMessage()
    {
      Role = Role.User,
      Content = (ContentBlock) new TextContentBlock()
      {
        Text = $"Generate a DAX query for this request: {request}\n\nValidate the query syntax but do NOT execute it. Return only the DAX query code."
      }
    };
    return (IEnumerable<PromptMessage>) daxQuery;
  }
}
