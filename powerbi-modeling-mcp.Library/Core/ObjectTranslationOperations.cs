// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using Microsoft.AnalysisServices.Tabular;
using ModelContextProtocol;
using PowerBIModelingMCP.Library.Common.DataStructures;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

#nullable enable
namespace PowerBIModelingMCP.Library.Core;

public static class ObjectTranslationOperations
{
  public static void ValidateObjectTranslationDefinition(ObjectTranslationBase def, bool isCreate)
  {
    if (def == null)
      throw new McpException("Object translation definition cannot be null");
    if (string.IsNullOrWhiteSpace(def.CultureName))
      throw new McpException("Culture name is required");
    if (string.IsNullOrWhiteSpace(def.ObjectType))
      throw new McpException("Object type is required");
    if (string.IsNullOrWhiteSpace(def.Property))
      throw new McpException("Property is required");
    if (!ObjectTranslationOperations.IsValidCultureName(def.CultureName))
      throw new McpException($"Invalid culture name format: {def.CultureName}. Expected format like 'en-US', 'fr-FR', etc.");
    TranslationHelper.ValidateObjectType(def.ObjectType);
    TranslationHelper.ValidateTranslatableProperty(def.ObjectType, def.Property);
    TranslationHelper.ValidateObjectIdentification(def);
  }

  public static bool IsValidCultureName(string cultureName)
  {
    if (string.IsNullOrWhiteSpace(cultureName))
      return false;
    try
    {
      CultureInfo.GetCultureInfo(cultureName);
      return true;
    }
    catch
    {
      return false;
    }
  }

  public static Culture EnsureCultureExists(
    Model model,
    string cultureName,
    bool createIfNotExists)
  {
    Culture culture1 = model.Cultures.Find(cultureName);
    if (culture1 == null)
    {
      if (!createIfNotExists)
        throw new McpException($"Culture '{cultureName}' does not exist in the model");
      CultureCreate def = new CultureCreate { Name = cultureName };
      def.ExtendedProperties = new List<PowerBIModelingMCP.Library.Common.DataStructures.ExtendedProperty>();
      def.Annotations = new List<KeyValuePair<string, string>>();
      OperationResult culture2 = CultureOperations.CreateCulture((string) null, def);
      if (!culture2.Success)
        throw new McpException($"Failed to create culture '{cultureName}': {culture2.Message}");
      culture1 = model.Cultures.Find(cultureName);
      if (culture1 == null)
        throw new McpException($"Culture '{cultureName}' was created but cannot be found in the model");
    }
    return culture1;
  }

  public static NamedMetadataObject? FindTranslatableObject(
    Model model,
    ObjectTranslationBase translation)
  {
    string objectType = translation.ObjectType;
    NamedMetadataObject translatableObject;
    if (objectType != null)
    {
      switch (objectType.Length)
      {
        case 3:
          if ((objectType == "KPI"))
          {
            translatableObject = ObjectTranslationOperations.FindKpi(model, translation);
            goto label_18;
          }
          break;
        case 5:
          switch (objectType[0])
          {
            case 'L':
              if ((objectType == "Level"))
              {
                translatableObject = ObjectTranslationOperations.FindLevel(model, translation);
                goto label_18;
              }
              break;
            case 'M':
              if ((objectType == "Model"))
              {
                translatableObject = (NamedMetadataObject) model;
                goto label_18;
              }
              break;
            case 'T':
              if ((objectType == "Table"))
              {
                translatableObject = (NamedMetadataObject) model.Tables.Find(translation.TableName);
                goto label_18;
              }
              break;
          }
          break;
        case 6:
          if ((objectType == "Column"))
          {
            translatableObject = ObjectTranslationOperations.FindColumn(model, translation);
            goto label_18;
          }
          break;
        case 7:
          if ((objectType == "Measure"))
          {
            translatableObject = ObjectTranslationOperations.FindMeasure(model, translation);
            goto label_18;
          }
          break;
        case 9:
          if ((objectType == "Hierarchy"))
          {
            translatableObject = ObjectTranslationOperations.FindHierarchy(model, translation);
            goto label_18;
          }
          break;
      }
    }
    translatableObject = (NamedMetadataObject) null;
label_18:
    return translatableObject;
  }

  private static NamedMetadataObject? FindMeasure(Model model, ObjectTranslationBase translation)
  {
    if (string.IsNullOrWhiteSpace(translation.MeasureName))
      return (NamedMetadataObject) null;
    if (string.IsNullOrWhiteSpace(translation.TableName))
    {
      foreach (Table table in (MetadataObjectCollection<Table, Model>) model.Tables)
      {
        Measure measure = table.Measures.Find(translation.MeasureName);
        if (measure != null)
          return (NamedMetadataObject) measure;
      }
      return (NamedMetadataObject) null;
    }
    Table table1 = model.Tables.Find(translation.TableName);
    return table1 == null ? (NamedMetadataObject) null : (NamedMetadataObject) table1.Measures.Find(translation.MeasureName);
  }

  private static NamedMetadataObject? FindColumn(Model model, ObjectTranslationBase translation)
  {
    if (string.IsNullOrWhiteSpace(translation.TableName) || string.IsNullOrWhiteSpace(translation.ColumnName))
      return (NamedMetadataObject) null;
    Table table = model.Tables.Find(translation.TableName);
    return table == null ? (NamedMetadataObject) null : (NamedMetadataObject) table.Columns.Find(translation.ColumnName);
  }

  private static NamedMetadataObject? FindHierarchy(Model model, ObjectTranslationBase translation)
  {
    if (string.IsNullOrWhiteSpace(translation.TableName) || string.IsNullOrWhiteSpace(translation.HierarchyName))
      return (NamedMetadataObject) null;
    Table table = model.Tables.Find(translation.TableName);
    return table == null ? (NamedMetadataObject) null : (NamedMetadataObject) table.Hierarchies.Find(translation.HierarchyName);
  }

  private static NamedMetadataObject? FindLevel(Model model, ObjectTranslationBase translation)
  {
    if (string.IsNullOrWhiteSpace(translation.TableName) || string.IsNullOrWhiteSpace(translation.HierarchyName) || string.IsNullOrWhiteSpace(translation.LevelName))
      return (NamedMetadataObject) null;
    Hierarchy hierarchy = model.Tables.Find(translation.TableName)?.Hierarchies.Find(translation.HierarchyName);
    return hierarchy == null ? (NamedMetadataObject) null : (NamedMetadataObject) hierarchy.Levels.Find(translation.LevelName);
  }

  private static NamedMetadataObject? FindKpi(Model model, ObjectTranslationBase translation)
  {
    if (string.IsNullOrWhiteSpace(translation.MeasureName))
      return (NamedMetadataObject) null;
    if (string.IsNullOrWhiteSpace(translation.TableName))
    {
      foreach (Table table in (MetadataObjectCollection<Table, Model>) model.Tables)
      {
        Measure kpi = table.Measures.Find(translation.MeasureName);
        if (kpi?.KPI != null)
          return (NamedMetadataObject) kpi;
      }
      return (NamedMetadataObject) null;
    }
    Measure measure = model.Tables.Find(translation.TableName)?.Measures.Find(translation.MeasureName);
    return measure?.KPI == null ? (NamedMetadataObject) null : (NamedMetadataObject) measure;
  }

  private static ObjectTranslation GetOrCreateObjectTranslation(
    Culture culture,
    NamedMetadataObject targetObject,
    string property)
  {
    TranslatedProperty translatedProperty1;
    if (!(property == "Caption"))
    {
      if (!(property == "Description"))
      {
        if (!(property == "DisplayFolder"))
          throw new McpException("Invalid translated property: " + property);
        translatedProperty1 = TranslatedProperty.DisplayFolder;
      }
      else
        translatedProperty1 = TranslatedProperty.Description;
    }
    else
      translatedProperty1 = TranslatedProperty.Caption;
    TranslatedProperty translatedProperty = translatedProperty1;
    ObjectTranslation objectTranslation = Enumerable.FirstOrDefault<ObjectTranslation>((IEnumerable<ObjectTranslation>) culture.ObjectTranslations, (ot => ot.Object == targetObject && ot.Property == translatedProperty));
    if (objectTranslation != null)
      return objectTranslation;
    ObjectTranslation metadataObject = new ObjectTranslation()
    {
      Object = (MetadataObject) targetObject,
      Property = translatedProperty
    };
    culture.ObjectTranslations.Add(metadataObject);
    return metadataObject;
  }

  public static List<ObjectTranslationList> ListObjectTranslations(
    string? connectionName,
    string? cultureName = null,
    string? objectType = null,
    string? objectName = null)
  {
    Model model = ConnectionOperations.Get(connectionName).Database.Model;
    List<ObjectTranslationList> objectTranslationListList = new List<ObjectTranslationList>();
    List<Culture> list;
    if (!string.IsNullOrEmpty(cultureName))
    {
      List<Culture> cultureList = new List<Culture>();
      cultureList.Add(model.Cultures.Find(cultureName));
      list = Enumerable.ToList<Culture>(Enumerable.Where<Culture>((IEnumerable<Culture>) cultureList, (c => c != null)));
    }
    else
      list = Enumerable.ToList<Culture>((IEnumerable<Culture>) model.Cultures);
    foreach (Culture culture in list)
    {
      foreach (ObjectTranslation objectTranslation in (MetadataObjectCollection<ObjectTranslation, Culture>) culture.ObjectTranslations)
      {
        if (objectTranslation.Object is NamedMetadataObject namedMetadataObject)
        {
          string objectTypeName = ObjectTranslationOperations.GetObjectTypeName(namedMetadataObject);
          string str = objectTranslation.Property.ToString();
          if (string.IsNullOrEmpty(objectType) || objectTypeName.Equals(objectType, StringComparison.OrdinalIgnoreCase))
          {
            Dictionary<string, string> objectIdentifiers = ObjectTranslationOperations.GetObjectIdentifiers(namedMetadataObject);
            objectTranslationListList.Add(new ObjectTranslationList()
            {
              CultureName = culture.Name,
              ObjectType = objectTypeName,
              Property = str,
              Value = objectTranslation.Value,
              ObjectIdentifiers = objectIdentifiers
            });
          }
        }
      }
    }
    return Enumerable.ToList<ObjectTranslationList>(Enumerable.ThenBy<ObjectTranslationList, string>(Enumerable.ThenBy<ObjectTranslationList, string>(Enumerable.OrderBy<ObjectTranslationList, string>((IEnumerable<ObjectTranslationList>) objectTranslationListList, (r => r.CultureName)), (r => r.ObjectType)), (r => r.Property)));
  }

  public static ObjectTranslationGet? GetObjectTranslation(
    string? connectionName,
    ObjectTranslationBase translation)
  {
    ObjectTranslationOperations.ValidateObjectTranslationDefinition(translation, false);
    ConnectionInfo connectionInfo = ConnectionOperations.Get(connectionName);
    Model model = connectionInfo.Database.Model;
    Culture culture = model.Cultures.Find(translation.CultureName);
    if (culture == null)
      return (ObjectTranslationGet) null;
    NamedMetadataObject targetObject = ObjectTranslationOperations.FindTranslatableObject(model, translation);
    if (targetObject == null)
      return (ObjectTranslationGet) null;
    string property = translation.Property;
    TranslatedProperty translatedProperty1;
    if (!(property == "Caption"))
    {
      if (!(property == "Description"))
      {
        if (!(property == "DisplayFolder"))
          throw new McpException("Invalid translated property: " + translation.Property);
        translatedProperty1 = TranslatedProperty.DisplayFolder;
      }
      else
        translatedProperty1 = TranslatedProperty.Description;
    }
    else
      translatedProperty1 = TranslatedProperty.Caption;
    TranslatedProperty translatedProperty = translatedProperty1;
    ObjectTranslation objectTranslation = Enumerable.FirstOrDefault<ObjectTranslation>((IEnumerable<ObjectTranslation>) culture.ObjectTranslations, (ot => ot.Object == targetObject && ot.Property == translatedProperty));
    if (objectTranslation == null)
      return (ObjectTranslationGet) null;
    ObjectTranslationGet objectTranslationGet = new ObjectTranslationGet
            {
                CultureName = translation.CultureName,
                ObjectType = translation.ObjectType,
                Property = translation.Property,
                Value = objectTranslation.Value,
                ModifiedTime = new DateTime?(connectionInfo.Database.LastUpdate)
            };ObjectTranslationGet target = objectTranslationGet;
    ObjectTranslationOperations.CopyIdentificationProperties(translation, (ObjectTranslationBase) target);
    return target;
  }

  public static ObjectTranslationOperations.ObjectTranslationOperationResult CreateObjectTranslation(
    string? connectionName,
    ObjectTranslationCreate translationDef)
  {
    ObjectTranslationOperations.ValidateObjectTranslationDefinition((ObjectTranslationBase) translationDef, true);
    try
    {
      ConnectionInfo info = ConnectionOperations.Get(connectionName);
      Model model = info.Database.Model;
      Culture culture = ObjectTranslationOperations.EnsureCultureExists(model, translationDef.CultureName, translationDef.CreateCultureIfNotExists);
      NamedMetadataObject translatableObject = ObjectTranslationOperations.FindTranslatableObject(model, (ObjectTranslationBase) translationDef);
      if (translatableObject == null)
        return new ObjectTranslationOperations.ObjectTranslationOperationResult()
        {
          Success = false,
          ErrorMessage = $"Object of type '{translationDef.ObjectType}' not found",
          CultureName = translationDef.CultureName,
          ObjectType = translationDef.ObjectType,
          ObjectDisplayName = TranslationHelper.GetObjectDisplayName((ObjectTranslationBase) translationDef),
          Property = translationDef.Property
        };
      ObjectTranslationOperations.GetOrCreateObjectTranslation(culture, translatableObject, translationDef.Property).Value = translationDef.Value;
      TransactionOperations.RecordOperation(info, $"Created translation for {translationDef.ObjectType} property '{translationDef.Property}' in culture '{translationDef.CultureName}'");
      ConnectionOperations.SaveChangesWithRollback(info, "create object translation");
      ObjectTranslationOperations.ObjectTranslationOperationResult objectTranslation = new ObjectTranslationOperations.ObjectTranslationOperationResult();
      objectTranslation.Success = true;
      objectTranslation.Message = $"Translation created successfully for {translationDef.ObjectType} property '{translationDef.Property}' in culture '{translationDef.CultureName}'";
      objectTranslation.CultureName = translationDef.CultureName;
      objectTranslation.ObjectType = translationDef.ObjectType;
      objectTranslation.ObjectDisplayName = TranslationHelper.GetObjectDisplayName((ObjectTranslationBase) translationDef);
      objectTranslation.Property = translationDef.Property;
      objectTranslation.Value = translationDef.Value;
      return objectTranslation;
    }
    catch (Exception ex)
    {
      return new ObjectTranslationOperations.ObjectTranslationOperationResult()
      {
        Success = false,
        ErrorMessage = ex.Message,
        CultureName = translationDef.CultureName,
        ObjectType = translationDef.ObjectType,
        ObjectDisplayName = TranslationHelper.GetObjectDisplayName((ObjectTranslationBase) translationDef),
        Property = translationDef.Property
      };
    }
  }

  public static ObjectTranslationOperations.ObjectTranslationOperationResult UpdateObjectTranslation(
    string? connectionName,
    ObjectTranslationUpdate translationDef)
  {
    ObjectTranslationOperations.ValidateObjectTranslationDefinition((ObjectTranslationBase) translationDef, false);
    try
    {
      ConnectionInfo info = ConnectionOperations.Get(connectionName);
      Model model = info.Database.Model;
      Culture culture = model.Cultures.Find(translationDef.CultureName);
      if (culture == null)
        return new ObjectTranslationOperations.ObjectTranslationOperationResult()
        {
          Success = false,
          ErrorMessage = $"Culture '{translationDef.CultureName}' not found",
          CultureName = translationDef.CultureName,
          ObjectType = translationDef.ObjectType,
          ObjectDisplayName = TranslationHelper.GetObjectDisplayName((ObjectTranslationBase) translationDef),
          Property = translationDef.Property
        };
      NamedMetadataObject translatableObject = ObjectTranslationOperations.FindTranslatableObject(model, (ObjectTranslationBase) translationDef);
      if (translatableObject == null)
        return new ObjectTranslationOperations.ObjectTranslationOperationResult()
        {
          Success = false,
          ErrorMessage = $"Object of type '{translationDef.ObjectType}' not found",
          CultureName = translationDef.CultureName,
          ObjectType = translationDef.ObjectType,
          ObjectDisplayName = TranslationHelper.GetObjectDisplayName((ObjectTranslationBase) translationDef),
          Property = translationDef.Property
        };
      ObjectTranslationOperations.GetOrCreateObjectTranslation(culture, translatableObject, translationDef.Property).Value = translationDef.Value;
      TransactionOperations.RecordOperation(info, $"Updated translation for {translationDef.ObjectType} property '{translationDef.Property}' in culture '{translationDef.CultureName}'");
      ConnectionOperations.SaveChangesWithRollback(info, "update object translation");
      ObjectTranslationOperations.ObjectTranslationOperationResult translationOperationResult = new ObjectTranslationOperations.ObjectTranslationOperationResult();
      translationOperationResult.Success = true;
      translationOperationResult.Message = $"Translation updated successfully for {translationDef.ObjectType} property '{translationDef.Property}' in culture '{translationDef.CultureName}'";
      translationOperationResult.CultureName = translationDef.CultureName;
      translationOperationResult.ObjectType = translationDef.ObjectType;
      translationOperationResult.ObjectDisplayName = TranslationHelper.GetObjectDisplayName((ObjectTranslationBase) translationDef);
      translationOperationResult.Property = translationDef.Property;
      translationOperationResult.Value = translationDef.Value;
      return translationOperationResult;
    }
    catch (Exception ex)
    {
      return new ObjectTranslationOperations.ObjectTranslationOperationResult()
      {
        Success = false,
        ErrorMessage = ex.Message,
        CultureName = translationDef.CultureName,
        ObjectType = translationDef.ObjectType,
        ObjectDisplayName = TranslationHelper.GetObjectDisplayName((ObjectTranslationBase) translationDef),
        Property = translationDef.Property
      };
    }
  }

  public static ObjectTranslationOperations.ObjectTranslationOperationResult DeleteObjectTranslation(
    string? connectionName,
    ObjectTranslationDelete translationDef)
  {
    ObjectTranslationOperations.ValidateObjectTranslationDefinition((ObjectTranslationBase) translationDef, false);
    try
    {
      ConnectionInfo info = ConnectionOperations.Get(connectionName);
      Model model = info.Database.Model;
      Culture culture = model.Cultures.Find(translationDef.CultureName);
      if (culture == null)
        return new ObjectTranslationOperations.ObjectTranslationOperationResult()
        {
          Success = false,
          ErrorMessage = $"Culture '{translationDef.CultureName}' not found",
          CultureName = translationDef.CultureName,
          ObjectType = translationDef.ObjectType,
          ObjectDisplayName = TranslationHelper.GetObjectDisplayName((ObjectTranslationBase) translationDef),
          Property = translationDef.Property
        };
      NamedMetadataObject targetObject = ObjectTranslationOperations.FindTranslatableObject(model, (ObjectTranslationBase) translationDef);
      if (targetObject == null)
        return new ObjectTranslationOperations.ObjectTranslationOperationResult()
        {
          Success = false,
          ErrorMessage = $"Object of type '{translationDef.ObjectType}' not found",
          CultureName = translationDef.CultureName,
          ObjectType = translationDef.ObjectType,
          ObjectDisplayName = TranslationHelper.GetObjectDisplayName((ObjectTranslationBase) translationDef),
          Property = translationDef.Property
        };
      string property = translationDef.Property;
      TranslatedProperty translatedProperty1;
      if (!(property == "Caption"))
      {
        if (!(property == "Description"))
        {
          if (!(property == "DisplayFolder"))
            throw new McpException("Invalid translated property: " + translationDef.Property);
          translatedProperty1 = TranslatedProperty.DisplayFolder;
        }
        else
          translatedProperty1 = TranslatedProperty.Description;
      }
      else
        translatedProperty1 = TranslatedProperty.Caption;
      TranslatedProperty translatedProperty = translatedProperty1;
      ObjectTranslation metadataObject = Enumerable.FirstOrDefault<ObjectTranslation>((IEnumerable<ObjectTranslation>) culture.ObjectTranslations, (ot => ot.Object == targetObject && ot.Property == translatedProperty));
      if (metadataObject == null)
      {
        ObjectTranslationOperations.ObjectTranslationOperationResult translationOperationResult = new ObjectTranslationOperations.ObjectTranslationOperationResult();
        translationOperationResult.Success = false;
        translationOperationResult.ErrorMessage = $"Translation for {translationDef.ObjectType} property '{translationDef.Property}' in culture '{translationDef.CultureName}' not found";
        translationOperationResult.CultureName = translationDef.CultureName;
        translationOperationResult.ObjectType = translationDef.ObjectType;
        translationOperationResult.ObjectDisplayName = TranslationHelper.GetObjectDisplayName((ObjectTranslationBase) translationDef);
        translationOperationResult.Property = translationDef.Property;
        return translationOperationResult;
      }
      culture.ObjectTranslations.Remove(metadataObject);
      TransactionOperations.RecordOperation(info, $"Deleted translation for {translationDef.ObjectType} property '{translationDef.Property}' in culture '{translationDef.CultureName}'");
      ConnectionOperations.SaveChangesWithRollback(info, "delete object translation");
      ObjectTranslationOperations.ObjectTranslationOperationResult translationOperationResult1 = new ObjectTranslationOperations.ObjectTranslationOperationResult();
      translationOperationResult1.Success = true;
      translationOperationResult1.Message = $"Translation deleted successfully for {translationDef.ObjectType} property '{translationDef.Property}' in culture '{translationDef.CultureName}'";
      translationOperationResult1.CultureName = translationDef.CultureName;
      translationOperationResult1.ObjectType = translationDef.ObjectType;
      translationOperationResult1.ObjectDisplayName = TranslationHelper.GetObjectDisplayName((ObjectTranslationBase) translationDef);
      translationOperationResult1.Property = translationDef.Property;
      return translationOperationResult1;
    }
    catch (Exception ex)
    {
      return new ObjectTranslationOperations.ObjectTranslationOperationResult()
      {
        Success = false,
        ErrorMessage = ex.Message,
        CultureName = translationDef.CultureName,
        ObjectType = translationDef.ObjectType,
        ObjectDisplayName = TranslationHelper.GetObjectDisplayName((ObjectTranslationBase) translationDef),
        Property = translationDef.Property
      };
    }
  }

  private static string GetObjectTypeName(NamedMetadataObject obj)
  {
    string objectTypeName;
    switch (obj)
    {
      case Model _:
        objectTypeName = "Model";
        break;
      case Table _:
        objectTypeName = "Table";
        break;
      case Measure _:
        objectTypeName = "Measure";
        break;
      case Column _:
        objectTypeName = "Column";
        break;
      case Hierarchy _:
        objectTypeName = "Hierarchy";
        break;
      case Level _:
        objectTypeName = "Level";
        break;
      default:
        objectTypeName = "Unknown";
        break;
    }
    return objectTypeName;
  }

  private static Dictionary<string, string> GetObjectIdentifiers(NamedMetadataObject obj)
  {
    Dictionary<string, string> objectIdentifiers = new Dictionary<string, string>();
    switch (obj)
    {
      case Model model:
        objectIdentifiers["ModelName"] = model.Name ?? "";
        break;
      case Table table:
        objectIdentifiers["TableName"] = table.Name;
        break;
      case Measure measure:
        objectIdentifiers["MeasureName"] = measure.Name;
        if (measure.Table != null)
        {
          objectIdentifiers["TableName"] = measure.Table.Name;
          break;
        }
        break;
      case Column column:
        objectIdentifiers["ColumnName"] = column.Name;
        if (column.Table != null)
        {
          objectIdentifiers["TableName"] = column.Table.Name;
          break;
        }
        break;
      case Hierarchy hierarchy:
        objectIdentifiers["HierarchyName"] = hierarchy.Name;
        if (hierarchy.Table != null)
        {
          objectIdentifiers["TableName"] = hierarchy.Table.Name;
          break;
        }
        break;
      case Level level:
        objectIdentifiers["LevelName"] = level.Name;
        if (level.Hierarchy?.Table != null)
        {
          objectIdentifiers["TableName"] = level.Hierarchy.Table.Name;
          objectIdentifiers["HierarchyName"] = level.Hierarchy.Name;
          break;
        }
        break;
    }
    return objectIdentifiers;
  }

  private static void CopyIdentificationProperties(
    ObjectTranslationBase source,
    ObjectTranslationBase target)
  {
    target.ModelName = source.ModelName;
    target.TableName = source.TableName;
    target.MeasureName = source.MeasureName;
    target.ColumnName = source.ColumnName;
    target.HierarchyName = source.HierarchyName;
    target.LevelName = source.LevelName;
  }

  public class ObjectTranslationOperationResult
  {
    public string CultureName { get; set; } = string.Empty;

    public string ObjectType { get; set; } = string.Empty;

    public string ObjectDisplayName { get; set; } = string.Empty;

    public string Property { get; set; } = string.Empty;

    public string? Value { get; set; }

    public string? ErrorMessage { get; set; }

    public bool Success { get; set; }

    public string? Message { get; set; }

    public bool HasChanges { get; set; }
  }
}
