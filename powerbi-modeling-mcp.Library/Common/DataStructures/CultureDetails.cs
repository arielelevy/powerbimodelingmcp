// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.DataStructures.CultureDetails
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

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
