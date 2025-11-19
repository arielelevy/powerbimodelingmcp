// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public class CultureDetails
{
  public required string Name { get; set; }

  public required int LCID { get; set; }

  public string? DisplayName { get; set; }

  public string? EnglishName { get; set; }

  public bool IsNeutralCulture { get; set; }

  public bool IsUserCustomCulture { get; set; }

  public CultureDetails()
  {
  }

  public CultureDetails(string name, int lcid)
  {
    this.Name = name;
    this.LCID = lcid;
  }
}
