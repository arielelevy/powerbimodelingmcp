// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using System;

#nullable enable
namespace PowerBIModelingMCP.Library.Contracts;

public class MCPServerConfiguration
{
  public ToolMode Mode { get; set; } = ToolMode.ReadWrite;

  public CompatibilityMode Compatibility { get; set; }

  public bool SkipConfirmation { get; set; }

  public ToolsConfiguration Tools { get; set; } = new ToolsConfiguration();

  public PromptsConfiguration Prompts { get; set; } = new PromptsConfiguration();

  public ResourcesConfiguration Resources { get; set; } = new ResourcesConfiguration();

  public void SetToolMode(ToolMode mode) => this.Mode = mode;

  public void SetToolMode(string mode)
  {
    string lowerInvariant = mode?.ToLowerInvariant();
    if (!(lowerInvariant == "readonly") && !(lowerInvariant == "read-only"))
    {
      if (!(lowerInvariant == "readwrite") && !(lowerInvariant == "read-write"))
        throw new ArgumentException($"Invalid tool mode '{mode}'. Supported modes: 'readonly', 'readwrite'.");
      this.Mode = ToolMode.ReadWrite;
    }
    else
      this.Mode = ToolMode.ReadOnly;
  }

  public void SetCompatibilityMode(CompatibilityMode compatibility)
  {
    this.Compatibility = compatibility;
  }

  public void SetCompatibilityMode(string compatibility)
  {
    string lowerInvariant = compatibility?.ToLowerInvariant();
    if (!(lowerInvariant == "powerbi"))
    {
      if (!(lowerInvariant == "full"))
        throw new ArgumentException($"Invalid compatibility mode '{compatibility}'. Supported modes: 'powerbi', 'full'.");
      this.Compatibility = CompatibilityMode.Full;
    }
    else
      this.Compatibility = CompatibilityMode.PowerBI;
  }

  public bool IsValid()
  {
    return Enum.IsDefined(typeof (ToolMode), (object) this.Mode) && Enum.IsDefined(typeof (CompatibilityMode), (object) this.Compatibility);
  }

  public string GetEnabledToolMode() => this.Mode.ToString();

  public string GetEnabledCompatibilityMode() => this.Compatibility.ToString();
}
