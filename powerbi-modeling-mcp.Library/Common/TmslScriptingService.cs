// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.TmslScriptingService
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using Microsoft.AnalysisServices.Tabular;
using ModelContextProtocol;
using PowerBIModelingMCP.Library.Common.DataStructures;
using System;
using System.Collections.Generic;
using System.Reflection;

#nullable enable
namespace PowerBIModelingMCP.Library.Common;

public static class TmslScriptingService
{
  public static TmslOperationResult GenerateScript<T>(
    T metadataObject,
    TmslOperationType operationType,
    TmslOperationRequest? options = null)
    where T : NamedMetadataObject
  {
    if ((object) metadataObject == null)
      throw new ArgumentNullException(nameof (metadataObject));
    if (options == null)
      options = new TmslOperationRequest();
    TmslOperationResult script = new TmslOperationResult()
    {
      OperationType = operationType,
      ObjectName = metadataObject.Name ?? string.Empty,
      ObjectType = ((MemberInfo) typeof (T)).Name
    };
    try
    {
      TmslScriptingService.ValidateObjectSupport<T>();
      TmslScriptingService.ValidateOperationSupport<T>(operationType);
      string str1;
      switch (operationType)
      {
        case TmslOperationType.Create:
          str1 = JsonScripter.ScriptCreate((NamedMetadataObject) metadataObject, options.IncludeRestricted);
          break;
        case TmslOperationType.CreateOrReplace:
          str1 = JsonScripter.ScriptCreateOrReplace((NamedMetadataObject) metadataObject, options.IncludeRestricted);
          break;
        case TmslOperationType.Alter:
          str1 = JsonScripter.ScriptAlter((NamedMetadataObject) metadataObject, options.IncludeRestricted);
          break;
        case TmslOperationType.Delete:
          str1 = JsonScripter.ScriptDelete((NamedMetadataObject) metadataObject);
          break;
        case TmslOperationType.Refresh:
          str1 = TmslScriptingService.GenerateRefreshScript((NamedMetadataObject) metadataObject, options.RefreshType ?? RefreshType.Automatic);
          break;
        default:
          throw new McpException($"Unsupported TMSL operation type: {operationType}");
      }
      string str2 = str1;
      script.Success = true;
      script.TmslScript = str2;
    }
    catch (Exception ex)
    {
      script.Success = false;
      script.ErrorMessage = ex.Message;
    }
    return script;
  }

  public static TmslOperationResult GenerateScript(
    Microsoft.AnalysisServices.Tabular.Database database,
    TmslOperationType operationType,
    TmslOperationRequest? options = null)
  {
    if (database == null)
      throw new ArgumentNullException(nameof (database));
    if (options == null)
      options = new TmslOperationRequest();
    TmslOperationResult script = new TmslOperationResult()
    {
      OperationType = operationType,
      ObjectName = database.Name ?? string.Empty,
      ObjectType = "Database"
    };
    try
    {
      string str1;
      switch (operationType)
      {
        case TmslOperationType.Create:
          str1 = JsonScripter.ScriptCreate((Microsoft.AnalysisServices.Core.Database) database, options.IncludeRestricted);
          break;
        case TmslOperationType.CreateOrReplace:
          str1 = JsonScripter.ScriptCreateOrReplace((Microsoft.AnalysisServices.Core.Database) database, options.IncludeRestricted);
          break;
        case TmslOperationType.Alter:
          str1 = JsonScripter.ScriptAlter((Microsoft.AnalysisServices.Core.Database) database, options.IncludeRestricted);
          break;
        case TmslOperationType.Delete:
          str1 = JsonScripter.ScriptDelete((Microsoft.AnalysisServices.Core.Database) database);
          break;
        case TmslOperationType.Refresh:
          if (!options.RefreshType.HasValue)
            throw new McpException("RefreshType is required for Refresh operations");
          str1 = JsonScripter.ScriptRefresh((Microsoft.AnalysisServices.Core.Database) database, options.RefreshType.Value);
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof (operationType), (object) operationType, "Unsupported operation type");
      }
      string str2 = str1;
      script.TmslScript = str2;
      script.Success = true;
    }
    catch (Exception ex)
    {
      script.Success = false;
      script.ErrorMessage = ex.Message;
    }
    script.GeneratedAt = DateTime.UtcNow;
    return script;
  }

  private static string GenerateRefreshScript(
    NamedMetadataObject metadataObject,
    RefreshType refreshType)
  {
    return JsonScripter.ScriptRefresh(metadataObject, refreshType);
  }

  private static void ValidateObjectSupport<T>() where T : NamedMetadataObject
  {
    string name = ((MemberInfo) typeof (T)).Name;
    HashSet<string> stringSet1 = new HashSet<string>();
    stringSet1.Add("Database");
    stringSet1.Add("Table");
    stringSet1.Add("Partition");
    stringSet1.Add("CalculationGroup");
    stringSet1.Add("Role");
    HashSet<string> stringSet2 = stringSet1;
    if (!stringSet2.Contains(name))
      throw new McpException($"TMSL operations are not supported for object type '{name}'. Supported types are: {string.Join(", ", (IEnumerable<string>) stringSet2)}. " + "Use ExportTMDL instead for this object type.");
  }

  private static void ValidateOperationSupport<T>(TmslOperationType operationType) where T : NamedMetadataObject
  {
    string name = ((MemberInfo) typeof (T)).Name;
    if ((name == "Role") && operationType == TmslOperationType.Refresh)
      throw new McpException($"Operation '{operationType}' is not supported for object type '{name}'. " + "Roles support Create, CreateOrReplace, Alter, and Delete operations only.");
  }
}
