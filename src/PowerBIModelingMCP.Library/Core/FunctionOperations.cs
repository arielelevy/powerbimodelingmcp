// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Core.FunctionOperations
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using Microsoft.AnalysisServices.Tabular;
using Microsoft.AnalysisServices.Tabular.Serialization;
using ModelContextProtocol;
using PowerBIModelingMCP.Library.Common.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable
namespace PowerBIModelingMCP.Library.Core;

public static class FunctionOperations
{
  public static void ValidateFunctionDefinition(FunctionBase def, bool isCreate)
  {
    if (def == null)
      throw new McpException("Function definition cannot be null");
    if (string.IsNullOrWhiteSpace(def.Name))
      throw new McpException("Name is required");
    if (isCreate && string.IsNullOrWhiteSpace(def.Expression))
      throw new McpException("Expression is required for creating functions");
    if (def.ExtendedProperties != null)
    {
      List<string> stringList = ExtendedPropertyHelpers.Validate(def.ExtendedProperties);
      if (stringList.Count > 0)
        throw new McpException("ExtendedProperties validation failed: " + string.Join(", ", (IEnumerable<string>) stringList));
    }
    AnnotationHelpers.ValidateAnnotations(def.Annotations);
  }

  public static Microsoft.AnalysisServices.Tabular.Function FindFunction(
    Microsoft.AnalysisServices.Tabular.Model model,
    string functionName)
  {
    return model.Functions.Find(functionName) ?? throw new McpException($"Function '{functionName}' not found in model");
  }

  public static List<FunctionList> ListFunctions(string? connectionName = null)
  {
    return Enumerable.ToList<FunctionList>(Enumerable.Select<Microsoft.AnalysisServices.Tabular.Function, FunctionList>((IEnumerable<Microsoft.AnalysisServices.Tabular.Function>) ConnectionOperations.Get(connectionName).Database.Model.Functions, (f =>
    {
      return new FunctionList()
      {
        Name = f.Name,
        Description = !string.IsNullOrEmpty(f.Description) ? f.Description : (string) null
      };
    })));
  }

  public static FunctionGet GetFunction(string? connectionName, string functionName)
  {
    if (string.IsNullOrWhiteSpace(functionName))
      throw new McpException("functionName is required");
    Microsoft.AnalysisServices.Tabular.Function function1 = FunctionOperations.FindFunction(ConnectionOperations.Get(connectionName).Database.Model, functionName);
    FunctionGet functionGet = new FunctionGet { Name = function1.Name };
    functionGet.Expression = function1.Expression ?? string.Empty;
    functionGet.Description = function1.Description;
    functionGet.IsHidden = new bool?(function1.IsHidden);
    functionGet.LineageTag = function1.LineageTag;
    functionGet.SourceLineageTag = function1.SourceLineageTag;
    functionGet.ModifiedTime = new DateTime?(function1.ModifiedTime);
    functionGet.StructureModifiedTime = new DateTime?(function1.StructureModifiedTime);
    functionGet.State = function1.State.ToString();
    functionGet.ErrorMessage = function1.ErrorMessage;
    functionGet.Annotations = new List<KeyValuePair<string, string>>();
    functionGet.ExtendedProperties = new List<PowerBIModelingMCP.Library.Common.DataStructures.ExtendedProperty>();
    FunctionGet function2 = functionGet;
    foreach (Microsoft.AnalysisServices.Tabular.Annotation annotation in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.Annotation, Microsoft.AnalysisServices.Tabular.Function>) function1.Annotations)
      function2.Annotations.Add(new KeyValuePair<string, string>(annotation.Name, annotation.Value ?? string.Empty));
    function2.ExtendedProperties = ExtendedPropertyHelpers.ExtractFromFunction(function1);
    return function2;
  }

  public static FunctionOperations.FunctionOperationResult CreateFunction(
    string? connectionName,
    FunctionCreate def)
  {
    FunctionOperations.ValidateFunctionDefinition((FunctionBase) def, true);
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Model model = info.Database.Model;
    if (model.Functions.Find(def.Name) != null)
      throw new McpException($"Function '{def.Name}' already exists in the model");
    Microsoft.AnalysisServices.Tabular.Function function1 = new Microsoft.AnalysisServices.Tabular.Function();
    function1.Name = def.Name;
    function1.Expression = def.Expression;
    Microsoft.AnalysisServices.Tabular.Function function2 = function1;
    if (!string.IsNullOrWhiteSpace(def.Description))
      function2.Description = def.Description;
    if (def.IsHidden.HasValue)
      function2.IsHidden = def.IsHidden.Value;
    if (!string.IsNullOrWhiteSpace(def.LineageTag))
      function2.LineageTag = def.LineageTag;
    if (!string.IsNullOrWhiteSpace(def.SourceLineageTag))
      function2.SourceLineageTag = def.SourceLineageTag;
    if (def.Annotations != null)
    {
      foreach (KeyValuePair<string, string> annotation in def.Annotations)
      {
        FunctionAnnotationCollection annotations = function2.Annotations;
        Microsoft.AnalysisServices.Tabular.Annotation metadataObject = new Microsoft.AnalysisServices.Tabular.Annotation();
        metadataObject.Name = annotation.Key;
        metadataObject.Value = annotation.Value;
        annotations.Add(metadataObject);
      }
    }
    if (def.ExtendedProperties != null)
      ExtendedPropertyHelpers.ApplyToFunction(function2, def.ExtendedProperties);
    model.Functions.Add(function2);
    TransactionOperations.RecordOperation(info, $"Created function '{def.Name}'");
    ConnectionOperations.SaveChangesWithRollback(info, "create function");
    return new FunctionOperations.FunctionOperationResult()
    {
      State = function2.State.ToString(),
      ErrorMessage = function2.ErrorMessage,
      FunctionName = function2.Name
    };
  }

  public static FunctionOperations.FunctionOperationResult UpdateFunction(
    string? connectionName,
    FunctionUpdate update)
  {
    FunctionOperations.ValidateFunctionDefinition((FunctionBase) update, false);
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Function function1 = FunctionOperations.FindFunction(info.Database.Model, update.Name);
    bool flag = false;
    if (!string.IsNullOrWhiteSpace(update.Expression) && (function1.Expression != update.Expression))
    {
      function1.Expression = update.Expression;
      flag = true;
    }
    if (update.Description != null)
    {
      string description = string.IsNullOrEmpty(update.Description) ? (string) null : update.Description;
      if ((function1.Description != description))
      {
        function1.Description = description;
        flag = true;
      }
    }
    if (update.IsHidden.HasValue)
    {
      int num1 = function1.IsHidden ? 1 : 0;
      bool? isHidden = update.IsHidden;
      int num2 = isHidden.Value ? 1 : 0;
      if (num1 != num2)
      {
        Microsoft.AnalysisServices.Tabular.Function function2 = function1;
        isHidden = update.IsHidden;
        int num3 = isHidden.Value ? 1 : 0;
        function2.IsHidden = num3 != 0;
        flag = true;
      }
    }
    if (update.LineageTag != null)
    {
      string lineageTag = string.IsNullOrEmpty(update.LineageTag) ? (string) null : update.LineageTag;
      if ((function1.LineageTag != lineageTag))
      {
        function1.LineageTag = lineageTag;
        flag = true;
      }
    }
    if (update.SourceLineageTag != null)
    {
      string sourceLineageTag = string.IsNullOrEmpty(update.SourceLineageTag) ? (string) null : update.SourceLineageTag;
      if ((function1.SourceLineageTag != sourceLineageTag))
      {
        function1.SourceLineageTag = sourceLineageTag;
        flag = true;
      }
    }
    if (update.Annotations != null)
    {
      function1.Annotations.Clear();
      foreach (KeyValuePair<string, string> annotation in update.Annotations)
      {
        FunctionAnnotationCollection annotations = function1.Annotations;
        Microsoft.AnalysisServices.Tabular.Annotation metadataObject = new Microsoft.AnalysisServices.Tabular.Annotation();
        metadataObject.Name = annotation.Key;
        metadataObject.Value = annotation.Value;
        annotations.Add(metadataObject);
      }
      flag = true;
    }
    if (update.ExtendedProperties != null)
    {
      ExtendedPropertyHelpers.ReplaceFunctionProperties(function1, update.ExtendedProperties);
      flag = true;
    }
    TransactionOperations.RecordOperation(info, $"Updated function '{update.Name}'");
    ConnectionOperations.SaveChangesWithRollback(info, "update function");
    return new FunctionOperations.FunctionOperationResult()
    {
      State = function1.State.ToString(),
      ErrorMessage = function1.ErrorMessage,
      FunctionName = function1.Name,
      HasChanges = flag
    };
  }

  public static void RenameFunction(string? connectionName, string oldName, string newName)
  {
    if (string.IsNullOrWhiteSpace(oldName))
      throw new McpException("oldName is required");
    if (string.IsNullOrWhiteSpace(newName))
      throw new McpException("newName is required");
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Model model = info.Database.Model;
    Microsoft.AnalysisServices.Tabular.Function function = FunctionOperations.FindFunction(model, oldName);
    if (model.Functions.Find(newName) != null && !string.Equals(oldName, newName, StringComparison.OrdinalIgnoreCase))
      throw new McpException($"Function '{newName}' already exists in the model");
    function.RequestRename(newName);
    TransactionOperations.RecordOperation(info, $"Renamed function from '{oldName}' to '{newName}'");
    ConnectionOperations.SaveChangesWithRollback(info, "rename function", ConnectionOperations.CheckpointMode.AfterRequestRename);
  }

  public static void DeleteFunction(string? connectionName, string functionName)
  {
    if (string.IsNullOrWhiteSpace(functionName))
      throw new McpException("functionName is required");
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Model model = info.Database.Model;
    model.Functions.Remove(FunctionOperations.FindFunction(model, functionName));
    TransactionOperations.RecordOperation(info, $"Deleted function '{functionName}'");
    ConnectionOperations.SaveChangesWithRollback(info, "delete function");
  }

  public static string ExportTMDL(string? connectionName, string functionName, ExportTmdl? options)
  {
    Microsoft.AnalysisServices.Tabular.Function @object = ConnectionOperations.Get(connectionName).Database.Model.Functions.Find(functionName);
    if (@object == null)
      throw new ArgumentException($"Function '{functionName}' not found");
    if (options?.SerializationOptions == null)
      return TmdlSerializer.SerializeObject((MetadataObject) @object);
    MetadataSerializationOptions serializationOptions = options.SerializationOptions.ToMetadataSerializationOptions();
    return TmdlSerializer.SerializeObject((MetadataObject) @object, serializationOptions);
  }

  public class FunctionOperationResult
  {
    public string State { get; set; } = "Ready";

    public string? ErrorMessage { get; set; }

    public string FunctionName { get; set; } = string.Empty;

    public bool HasChanges { get; set; }
  }
}
