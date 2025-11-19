// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
#nullable enable
namespace PowerBIModelingMCP.Library.Common;

public class TransactionSetupResult
{
  public string? TransactionId { get; set; }

  public bool OwnsTransaction { get; set; }
}
