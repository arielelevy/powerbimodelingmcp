// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

#nullable enable
namespace PowerBIModelingMCP.Library.Services;

public class MarkdownResourceParser
{
  private static readonly Regex YamlFrontMatterRegex = new Regex("^---\\s*\\n(.*?)\\n---\\s*$", (RegexOptions) 18);
  private readonly IDeserializer _yamlDeserializer;

  public MarkdownResourceParser()
  {
    this._yamlDeserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
  }

  public async Task<ParsedResourceDefinition> ParseFileAsync(string filePath)
  {
    return File.Exists(filePath) ? this.ParseContent(await File.ReadAllTextAsync(filePath, new CancellationToken()), Path.GetFileNameWithoutExtension(filePath)) : throw new FileNotFoundException("Resource file not found:" + filePath);
  }

  public ParsedResourceDefinition ParseContent(string content, string fileName)
  {
    Match match = MarkdownResourceParser.YamlFrontMatterRegex.Match(content);
    if (!((Group) match).Success)
      throw new InvalidOperationException("No YAML frontmatter found in markdown content");
    ResourceMetadata resourceMetadata = this._yamlDeserializer.Deserialize<ResourceMetadata>(((Capture) match.Groups[1]).Value);
    int num = ((Capture) match).Index + ((Capture) match).Length;
    string str1 = content.Substring(num).Trim();
    string str2 = !string.IsNullOrWhiteSpace(resourceMetadata.Name) ? resourceMetadata.Name : fileName;
    return new ParsedResourceDefinition()
    {
      Name = str2,
      Description = resourceMetadata.Description ?? string.Empty,
      UriTemplate = resourceMetadata.UriTemplate ?? string.Empty,
      Text = str1 ?? string.Empty
    };
  }

  public async Task<IEnumerable<ParsedResourceDefinition>> ParseDirectoryAsync(string directoryPath)
  {
    if (!Directory.Exists(directoryPath))
      throw new DirectoryNotFoundException("Directory not found: " + directoryPath);
    List<ParsedResourceDefinition> resourceDefinitions = new List<ParsedResourceDefinition>();
    string[] strArray = Directory.GetFiles(directoryPath, "*.md", SearchOption.TopDirectoryOnly);
    for (int index = 0; index < strArray.Length; ++index)
    {
      string filePath = strArray[index];
      try
      {
        resourceDefinitions.Add(await this.ParseFileAsync(filePath));
      }
      catch (Exception ex)
      {
      }
    }
    strArray = (string[]) null;
    IEnumerable<ParsedResourceDefinition> directoryAsync = (IEnumerable<ParsedResourceDefinition>) resourceDefinitions;
    resourceDefinitions = (List<ParsedResourceDefinition>) null;
    return directoryAsync;
  }
}
