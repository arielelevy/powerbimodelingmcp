// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Services.ResourceLoader
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using ModelContextProtocol.Server;
using System;
using System.ComponentModel;
using System.IO;

#nullable enable
namespace PowerBIModelingMCP.Library.Services;

[McpServerResourceType]
public class ResourceLoader
{
  private static string LoadResource(string fileName)
  {
    string str = !string.IsNullOrEmpty(fileName) ? Path.Combine(Path.Combine(AppContext.BaseDirectory, "Resources"), fileName) : throw new ArgumentNullException(nameof (fileName));
    return !File.Exists(str) ? "Resource file not found: " + str : File.ReadAllText(str);
  }

  [McpServerResource(Name = "DAX Query Instructions and Examples", UriTemplate = "resource://dax_query_instructions_and_examples", MimeType = "text/plain")]
  [Description("Guidelines for writing Power BI DAX queries")]
  public string dax_query_instructions_and_examples()
  {
    return ResourceLoader.LoadResource("dax_query_instructions_and_examples.md");
  }

  [McpServerResource(Name = "DAX UDF Instructions and Examples", UriTemplate = "resource://dax_udf_instructions_and_examples", MimeType = "text/plain")]
  [Description("Guidelines for creating Power BI DAX user-defined functions (UDFs)")]
  public string dax_udf_instructions_and_examples()
  {
    return ResourceLoader.LoadResource("dax_udf_instructions_and_examples.md");
  }

  [McpServerResource(Name = "Calendar Instructions and Examples", UriTemplate = "resource://calendar_instructions_and_examples", MimeType = "text/plain")]
  [Description("Guidelines for creating Power BI calendar objects")]
  public string calendar_instructions_and_examples()
  {
    return ResourceLoader.LoadResource("calendar_instructions_and_examples.md");
  }

  [McpServerResource(Name = "PowerBI Project Instructions", UriTemplate = "resource://powerbi_project_instructions", MimeType = "text/plain")]
  [Description("Instructions for structuring Power BI projects")]
  public string powerbi_project_instructions()
  {
    return ResourceLoader.LoadResource("powerbi_project_instructions.md");
  }
}
