// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using Microsoft.AnalysisServices.Tabular;
using Microsoft.AnalysisServices.Tabular.Serialization;
using ModelContextProtocol;
using PowerBIModelingMCP.Library.Common.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable
namespace PowerBIModelingMCP.Library.Core;

public static class NamedExpressionOperations
{
  public static void ValidateNamedExpressionDefinition(NamedExpressionBase def, bool isCreate)
  {
    if (def == null)
      throw new McpException("Named expression definition cannot be null");
    if (string.IsNullOrWhiteSpace(def.Name))
      throw new McpException("Name is required");
    if (isCreate && string.IsNullOrWhiteSpace(def.Expression))
      throw new McpException("Expression is required for creating named expressions");
    if (isCreate && string.IsNullOrWhiteSpace(def.Kind))
      throw new McpException("Kind is required for creating named expressions");
    if (!string.IsNullOrWhiteSpace(def.Kind) && !Enum.IsDefined(typeof (ExpressionKind), (object) def.Kind))
    {
      string[] names = Enum.GetNames(typeof (ExpressionKind));
      throw new McpException($"Invalid Kind '{def.Kind}'. Valid values are: {string.Join(", ", names)}");
    }
    if (def.ExtendedProperties != null)
    {
      List<string> stringList = ExtendedPropertyHelpers.Validate(def.ExtendedProperties);
      if (stringList.Count > 0)
        throw new McpException("ExtendedProperties validation failed: " + string.Join(", ", (IEnumerable<string>) stringList));
    }
    AnnotationHelpers.ValidateAnnotations(def.Annotations);
  }

  public static Microsoft.AnalysisServices.Tabular.NamedExpression FindNamedExpression(
    Microsoft.AnalysisServices.Tabular.Model model,
    string namedExpressionName)
  {
    return model.Expressions.Find(namedExpressionName) ?? throw new McpException($"Named expression '{namedExpressionName}' not found in model");
  }

  public static List<NamedExpressionList> ListNamedExpressions(string? connectionName = null)
  {
    return Enumerable.ToList<NamedExpressionList>(Enumerable.Select<Microsoft.AnalysisServices.Tabular.NamedExpression, NamedExpressionList>((IEnumerable<Microsoft.AnalysisServices.Tabular.NamedExpression>) ConnectionOperations.Get(connectionName).Database.Model.Expressions, (e =>
    {
      return new NamedExpressionList()
      {
        Name = e.Name,
        Description = !string.IsNullOrEmpty(e.Description) ? e.Description : (string) null,
        Kind = e.Kind.ToString(),
        QueryGroupName = e.QueryGroup?.Name
      };
    })));
  }

  public static NamedExpressionGet GetNamedExpression(
    string? connectionName,
    string namedExpressionName)
  {
    if (string.IsNullOrWhiteSpace(namedExpressionName))
      throw new McpException("namedExpressionName is required");
    Microsoft.AnalysisServices.Tabular.NamedExpression namedExpression1 = NamedExpressionOperations.FindNamedExpression(ConnectionOperations.Get(connectionName).Database.Model, namedExpressionName);
    NamedExpressionGet namedExpressionGet = new NamedExpressionGet { Name = namedExpression1.Name };
    namedExpressionGet.Expression = namedExpression1.Expression ?? string.Empty;
    namedExpressionGet.Description = namedExpression1.Description;
    namedExpressionGet.Kind = namedExpression1.Kind.ToString();
    namedExpressionGet.LineageTag = namedExpression1.LineageTag;
    namedExpressionGet.SourceLineageTag = namedExpression1.SourceLineageTag;
    namedExpressionGet.QueryGroupName = namedExpression1.QueryGroup?.Name;
    namedExpressionGet.ModifiedTime = new DateTime?(namedExpression1.ModifiedTime);
    namedExpressionGet.Annotations = new List<KeyValuePair<string, string>>();
    namedExpressionGet.ExtendedProperties = new List<PowerBIModelingMCP.Library.Common.DataStructures.ExtendedProperty>();
    NamedExpressionGet namedExpression2 = namedExpressionGet;
    if (namedExpression2.Annotations != null)
    {
      foreach (Microsoft.AnalysisServices.Tabular.Annotation annotation in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.Annotation, Microsoft.AnalysisServices.Tabular.NamedExpression>) namedExpression1.Annotations)
        namedExpression2.Annotations.Add(new KeyValuePair<string, string>(annotation.Name, annotation.Value ?? string.Empty));
    }
    namedExpression2.ExtendedProperties = ExtendedPropertyHelpers.ExtractFromNamedExpression(namedExpression1);
    return namedExpression2;
  }

  public static NamedExpressionOperations.NamedExpressionOperationResult CreateNamedExpression(
    string? connectionName,
    NamedExpressionCreate def)
  {
    NamedExpressionOperations.ValidateNamedExpressionDefinition((NamedExpressionBase) def, true);
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Model model = info.Database.Model;
    if (model.Expressions.Find(def.Name) != null)
      throw new McpException($"Named expression '{def.Name}' already exists in the model");
    Microsoft.AnalysisServices.Tabular.NamedExpression namedExpression1 = new Microsoft.AnalysisServices.Tabular.NamedExpression();
    namedExpression1.Name = def.Name;
    namedExpression1.Expression = def.Expression;
    Microsoft.AnalysisServices.Tabular.NamedExpression namedExpression2 = namedExpression1;
    ExpressionKind expressionKind;
    if (Enum.TryParse<ExpressionKind>(def.Kind, out expressionKind))
      namedExpression2.Kind = expressionKind;
    if (!string.IsNullOrWhiteSpace(def.Description))
      namedExpression2.Description = def.Description;
    if (!string.IsNullOrWhiteSpace(def.LineageTag))
      namedExpression2.LineageTag = def.LineageTag;
    if (!string.IsNullOrWhiteSpace(def.SourceLineageTag))
      namedExpression2.SourceLineageTag = def.SourceLineageTag;
    List<string> stringList1 = (List<string>) null;
    if (!string.IsNullOrWhiteSpace(def.QueryGroupName))
    {
      bool wasCreated;
      Microsoft.AnalysisServices.Tabular.QueryGroup createQueryGroup = QueryGroupOperations.FindOrCreateQueryGroup(info.Database, def.QueryGroupName, out wasCreated);
      if (wasCreated)
      {
        List<string> stringList2 = new List<string>();
        stringList2.Add($"Query group '{def.QueryGroupName}' was automatically created");
        stringList1 = stringList2;
      }
      namedExpression2.QueryGroup = createQueryGroup;
    }
    if (def.Annotations != null)
    {
      foreach (KeyValuePair<string, string> annotation in def.Annotations)
      {
        NamedExpressionAnnotationCollection annotations = namedExpression2.Annotations;
        Microsoft.AnalysisServices.Tabular.Annotation metadataObject = new Microsoft.AnalysisServices.Tabular.Annotation();
        metadataObject.Name = annotation.Key;
        metadataObject.Value = annotation.Value;
        annotations.Add(metadataObject);
      }
    }
    if (def.ExtendedProperties != null)
      ExtendedPropertyHelpers.ApplyToNamedExpression(namedExpression2, def.ExtendedProperties);
    model.Expressions.Add(namedExpression2);
    TransactionOperations.RecordOperation(info, $"Created named expression '{def.Name}'");
    ConnectionOperations.SaveChangesWithRollback(info, "create named expression");
    return new NamedExpressionOperations.NamedExpressionOperationResult()
    {
      State = "Ready",
      ErrorMessage = (string) null,
      NamedExpressionName = namedExpression2.Name,
      Warnings = stringList1
    };
  }

  public static NamedExpressionOperations.NamedExpressionOperationResult UpdateNamedExpression(
    string? connectionName,
    string namedExpressionName,
    NamedExpressionUpdate update)
  {
    NamedExpressionOperations.ValidateNamedExpressionDefinition((NamedExpressionBase) update, false);
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.NamedExpression namedExpression = NamedExpressionOperations.FindNamedExpression(info.Database.Model, namedExpressionName);
    bool flag = false;
    if ((update.Name != namedExpressionName))
      throw new McpException($"Name in update definition ('{update.Name}') must match the target named expression name ('{namedExpressionName}'). Use RenameNamedExpression to rename objects.");
    if (update.Expression != null)
    {
      if (string.IsNullOrEmpty(update.Expression))
        throw new McpException("Expression cannot be empty. Provide a valid expression or omit this property to keep the current value.");
      if ((namedExpression.Expression != update.Expression))
      {
        namedExpression.Expression = update.Expression;
        flag = true;
      }
    }
    if (!string.IsNullOrWhiteSpace(update.Kind))
    {
      ExpressionKind expressionKind;
      if (Enum.TryParse<ExpressionKind>(update.Kind, true, out expressionKind))
      {
        if (namedExpression.Kind != expressionKind)
        {
          namedExpression.Kind = expressionKind;
          flag = true;
        }
      }
      else
      {
        string[] names = Enum.GetNames(typeof (ExpressionKind));
        throw new McpException($"Invalid Kind '{update.Kind}'. Valid values are: {string.Join(", ", names)}");
      }
    }
    if (update.Description != null)
    {
      string description = string.IsNullOrEmpty(update.Description) ? (string) null : update.Description;
      if ((namedExpression.Description != description))
      {
        namedExpression.Description = description;
        flag = true;
      }
    }
    if (update.LineageTag != null)
    {
      string lineageTag = string.IsNullOrEmpty(update.LineageTag) ? (string) null : update.LineageTag;
      if ((namedExpression.LineageTag != lineageTag))
      {
        namedExpression.LineageTag = lineageTag;
        flag = true;
      }
    }
    if (update.SourceLineageTag != null)
    {
      string sourceLineageTag = string.IsNullOrEmpty(update.SourceLineageTag) ? (string) null : update.SourceLineageTag;
      if ((namedExpression.SourceLineageTag != sourceLineageTag))
      {
        namedExpression.SourceLineageTag = sourceLineageTag;
        flag = true;
      }
    }
    List<string> stringList1 = (List<string>) null;
    if (update.QueryGroupName != null)
    {
      Microsoft.AnalysisServices.Tabular.QueryGroup queryGroup = (Microsoft.AnalysisServices.Tabular.QueryGroup) null;
      if (!string.IsNullOrEmpty(update.QueryGroupName))
      {
        bool wasCreated;
        queryGroup = QueryGroupOperations.FindOrCreateQueryGroup(info.Database, update.QueryGroupName, out wasCreated);
        if (wasCreated)
        {
          List<string> stringList2 = new List<string>();
          stringList2.Add($"Query group '{update.QueryGroupName}' was automatically created");
          stringList1 = stringList2;
        }
      }
      if (namedExpression.QueryGroup != queryGroup)
      {
        namedExpression.QueryGroup = queryGroup;
        flag = true;
      }
    }
    if (update.Annotations != null && AnnotationHelpers.ReplaceAnnotations<Microsoft.AnalysisServices.Tabular.NamedExpression>(namedExpression, update.Annotations, (Func<Microsoft.AnalysisServices.Tabular.NamedExpression, ICollection<Microsoft.AnalysisServices.Tabular.Annotation>>) (ne => (ICollection<Microsoft.AnalysisServices.Tabular.Annotation>) ne.Annotations)))
      flag = true;
    if (update.ExtendedProperties != null)
    {
      int num = namedExpression.ExtendedProperties.Count > 0 ? 1 : 0;
      ExtendedPropertyHelpers.ReplaceNamedExpressionProperties(namedExpression, update.ExtendedProperties);
      if (num != 0 || update.ExtendedProperties.Count > 0)
        flag = true;
    }
    if (!flag)
      return new NamedExpressionOperations.NamedExpressionOperationResult()
      {
        State = "Ready",
        ErrorMessage = (string) null,
        NamedExpressionName = namedExpression.Name,
        HasChanges = false,
        Warnings = stringList1
      };
    TransactionOperations.RecordOperation(info, $"Updated named expression '{namedExpressionName}'");
    ConnectionOperations.SaveChangesWithRollback(info, "update named expression");
    return new NamedExpressionOperations.NamedExpressionOperationResult()
    {
      State = "Ready",
      ErrorMessage = (string) null,
      NamedExpressionName = namedExpression.Name,
      HasChanges = true,
      Warnings = stringList1
    };
  }

  public static void RenameNamedExpression(string? connectionName, string oldName, string newName)
  {
    if (string.IsNullOrWhiteSpace(oldName))
      throw new McpException("oldName is required");
    if (string.IsNullOrWhiteSpace(newName))
      throw new McpException("newName is required");
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Model model = info.Database.Model;
    Microsoft.AnalysisServices.Tabular.NamedExpression namedExpression = NamedExpressionOperations.FindNamedExpression(model, oldName);
    if (model.Expressions.Find(newName) != null && !string.Equals(oldName, newName, StringComparison.OrdinalIgnoreCase))
      throw new McpException($"Named expression '{newName}' already exists in the model");
    namedExpression.RequestRename(newName);
    TransactionOperations.RecordOperation(info, $"Renamed named expression from '{oldName}' to '{newName}'");
    ConnectionOperations.SaveChangesWithRollback(info, "rename named expression", ConnectionOperations.CheckpointMode.AfterRequestRename);
  }

  public static void DeleteNamedExpression(string? connectionName, string namedExpressionName)
  {
    if (string.IsNullOrWhiteSpace(namedExpressionName))
      throw new McpException("namedExpressionName is required");
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Model model = info.Database.Model;
    model.Expressions.Remove(NamedExpressionOperations.FindNamedExpression(model, namedExpressionName));
    TransactionOperations.RecordOperation(info, $"Deleted named expression '{namedExpressionName}'");
    ConnectionOperations.SaveChangesWithRollback(info, "delete named expression");
  }

  public static NamedExpressionOperations.NamedExpressionOperationResult CreateParameter(
    string? connectionName,
    NamedExpressionCreate def)
  {
    NamedExpressionOperations.ValidateParameterDefinition((NamedExpressionBase) def, true);
    if (!NamedExpressionOperations.IsParameterExpression(def.Expression))
      def.Expression = NamedExpressionOperations.BuildParameterExpression(def.Expression);
    def.Kind = "M";
    return NamedExpressionOperations.ValidateParameterExpression(def.Expression) ? NamedExpressionOperations.CreateNamedExpression(connectionName, def) : throw new McpException("Invalid parameter expression format");
  }

  public static NamedExpressionOperations.NamedExpressionOperationResult UpdateParameter(
    string? connectionName,
    string parameterName,
    NamedExpressionUpdate update)
  {
    NamedExpressionOperations.ValidateParameterDefinition((NamedExpressionBase) update, false);
    if (!string.IsNullOrEmpty(update.Expression) && !NamedExpressionOperations.IsParameterExpression(update.Expression))
      update.Expression = NamedExpressionOperations.BuildParameterExpression(update.Expression);
    update.Kind = "M";
    if (!string.IsNullOrEmpty(update.Expression) && !NamedExpressionOperations.ValidateParameterExpression(update.Expression))
      throw new McpException("Invalid parameter expression format");
    return NamedExpressionOperations.UpdateNamedExpression(connectionName, parameterName, update);
  }

  public static string BuildParameterExpression(string value, string type = "Text", bool isRequired = true)
  {
    return $"{NamedExpressionOperations.EscapeParameterValue(value, type)} meta {$"[IsParameterQuery=true, Type=\"{type}\", IsParameterQueryRequired={isRequired.ToString().ToLowerInvariant()}]"}";
  }

  private static string EscapeParameterValue(string value, string type)
  {
    string upperInvariant = type.ToUpperInvariant();
    return (upperInvariant == "TEXT") ? $"\"{value.Replace("\"", "\"\"")}\"" : ((upperInvariant == "NUMBER") ? value : ((upperInvariant == "LOGICAL") ? ((value.ToLowerInvariant() == "true") ? "true" : "false") : $"\"{value.Replace("\"", "\"\"")}\""));
  }

  private static void ValidateParameterDefinition(NamedExpressionBase def, bool isCreate)
  {
    NamedExpressionOperations.ValidateNamedExpressionDefinition(def, isCreate);
    if (!string.IsNullOrEmpty(def.Expression) && NamedExpressionOperations.IsParameterExpression(def.Expression) && !NamedExpressionOperations.ValidateParameterExpression(def.Expression))
      throw new McpException("Existing parameter expression format is invalid. Missing required metadata properties.");
  }

  private static bool IsParameterExpression(string expression)
  {
    if (!expression.Contains(" meta"))
      return false;
    int num1 = expression.IndexOf(" meta");
    if (num1 == -1)
      return false;
    int num2 = num1 + 5;
    int num3 = -1;
    for (int index = num2; index < expression.Length; ++index)
    {
      if (expression[index] == '[')
      {
        num3 = index;
        break;
      }
      if (!char.IsWhiteSpace(expression[index]))
        return false;
    }
    if (num3 == -1)
      return false;
    string str1 = expression.Substring(num3 + 1);
    int num4 = str1.IndexOf(']');
    if (num4 == -1)
      return false;
    string[] strArray = str1.Substring(0, num4).Split(',', (StringSplitOptions) 0);
    Dictionary<string, string> dictionary = new Dictionary<string, string>();
    foreach (string str2 in strArray)
    {
      string str3 = str2.Trim();
      int num5 = str3.IndexOf('=');
      if (num5 > 0)
      {
        string str4 = str3.Substring(0, num5).Trim();
        string str5 = str3.Substring(num5 + 1).Trim();
        dictionary[str4] = str5;
      }
    }
    return dictionary.ContainsKey("IsParameterQuery") && dictionary["IsParameterQuery"].Equals("true", StringComparison.OrdinalIgnoreCase);
  }

  private static bool ValidateParameterExpression(string expression)
  {
    if (!expression.Contains(" meta"))
      return false;
    int num1 = expression.IndexOf(" meta");
    if (num1 == -1)
      return false;
    int num2 = num1 + 5;
    int num3 = -1;
    for (int index = num2; index < expression.Length; ++index)
    {
      if (expression[index] == '[')
      {
        num3 = index;
        break;
      }
      if (!char.IsWhiteSpace(expression[index]))
        return false;
    }
    if (num3 == -1)
      return false;
    string str1 = expression.Substring(num3 + 1);
    int num4 = str1.IndexOf(']');
    if (num4 == -1)
      return false;
    string[] strArray = str1.Substring(0, num4).Split(',', (StringSplitOptions) 0);
    Dictionary<string, string> dictionary = new Dictionary<string, string>();
    foreach (string str2 in strArray)
    {
      string str3 = str2.Trim();
      int num5 = str3.IndexOf('=');
      if (num5 > 0)
      {
        string str4 = str3.Substring(0, num5).Trim();
        string str5 = str3.Substring(num5 + 1).Trim();
        dictionary[str4] = str5;
      }
    }
    if (!dictionary.ContainsKey("IsParameterQuery") || !dictionary["IsParameterQuery"].Equals("true", StringComparison.OrdinalIgnoreCase) || !dictionary.ContainsKey("Type") || string.IsNullOrWhiteSpace(dictionary["Type"]) || !dictionary.ContainsKey("IsParameterQueryRequired"))
      return false;
    string str6 = dictionary["IsParameterQueryRequired"].Trim('"');
    return str6.Equals("true", StringComparison.OrdinalIgnoreCase) || str6.Equals("false", StringComparison.OrdinalIgnoreCase);
  }

  public static string ExportTMDL(
    string? connectionName,
    string namedExpressionName,
    ExportTmdl? options)
  {
    Microsoft.AnalysisServices.Tabular.NamedExpression @object = ConnectionOperations.Get(connectionName).Database.Model.Expressions.Find(namedExpressionName);
    if (@object == null)
      throw new ArgumentException($"Named expression '{namedExpressionName}' not found");
    if (options?.SerializationOptions == null)
      return TmdlSerializer.SerializeObject((MetadataObject) @object);
    MetadataSerializationOptions serializationOptions = options.SerializationOptions.ToMetadataSerializationOptions();
    return TmdlSerializer.SerializeObject((MetadataObject) @object, serializationOptions);
  }

  public class NamedExpressionOperationResult
  {
    public string State { get; set; } = "Ready";

    public string? ErrorMessage { get; set; }

    public string NamedExpressionName { get; set; } = string.Empty;

    public bool HasChanges { get; set; }

    public List<string>? Warnings { get; set; }
  }
}
