// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.DataStructures.TranslationHelper
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public static class TranslationHelper
{
  private static readonly HashSet<string> CaptionSupportedObjects;
  private static readonly HashSet<string> DescriptionSupportedObjects;
  private static readonly HashSet<string> DisplayFolderSupportedObjects;
  public static readonly HashSet<string> ValidObjectTypes;

  public static List<string> GetValidProperties(string objectType)
  {
    List<string> validProperties = new List<string>();
    if (TranslationHelper.CaptionSupportedObjects.Contains(objectType))
      validProperties.Add("Caption");
    if (TranslationHelper.DescriptionSupportedObjects.Contains(objectType))
      validProperties.Add("Description");
    if (TranslationHelper.DisplayFolderSupportedObjects.Contains(objectType))
      validProperties.Add("DisplayFolder");
    return validProperties;
  }

  public static void ValidateTranslatableProperty(string objectType, string property)
  {
    List<string> validProperties = TranslationHelper.GetValidProperties(objectType);
    if (!validProperties.Contains(property))
      throw new ArgumentException($"Property '{property}' is not translatable for object type '{objectType}'. Valid properties for {objectType}: {string.Join(", ", (IEnumerable<string>) validProperties)}");
  }

  public static void ValidateObjectType(string objectType)
  {
    if (!TranslationHelper.ValidObjectTypes.Contains(objectType))
      throw new ArgumentException($"Object type '{objectType}' is not supported for translations. Valid object types: {string.Join(", ", (IEnumerable<string>) TranslationHelper.ValidObjectTypes)}");
  }

  public static void ValidateObjectIdentification(ObjectTranslationBase translation)
  {
    List<string> stringList = new List<string>();
    string objectType = translation.ObjectType;
    if (objectType != null)
    {
      switch (objectType.Length)
      {
        case 3:
          if ((objectType == "KPI"))
            break;
          goto label_31;
        case 5:
          switch (objectType[0])
          {
            case 'L':
              if ((objectType == "Level"))
              {
                if (string.IsNullOrWhiteSpace(translation.TableName))
                  stringList.Add("TableName");
                if (string.IsNullOrWhiteSpace(translation.HierarchyName))
                  stringList.Add("HierarchyName");
                if (string.IsNullOrWhiteSpace(translation.LevelName))
                {
                  stringList.Add("LevelName");
                  goto label_32;
                }
                goto label_32;
              }
              goto label_31;
            case 'M':
              if ((objectType == "Model"))
              {
                if (string.IsNullOrWhiteSpace(translation.ModelName))
                {
                  stringList.Add("ModelName");
                  goto label_32;
                }
                goto label_32;
              }
              goto label_31;
            case 'T':
              if ((objectType == "Table"))
              {
                if (string.IsNullOrWhiteSpace(translation.TableName))
                {
                  stringList.Add("TableName");
                  goto label_32;
                }
                goto label_32;
              }
              goto label_31;
            default:
              goto label_31;
          }
        case 6:
          if ((objectType == "Column"))
          {
            if (string.IsNullOrWhiteSpace(translation.TableName))
              stringList.Add("TableName");
            if (string.IsNullOrWhiteSpace(translation.ColumnName))
            {
              stringList.Add("ColumnName");
              goto label_32;
            }
            goto label_32;
          }
          goto label_31;
        case 7:
          if ((objectType == "Measure"))
            break;
          goto label_31;
        case 9:
          if ((objectType == "Hierarchy"))
          {
            if (string.IsNullOrWhiteSpace(translation.TableName))
              stringList.Add("TableName");
            if (string.IsNullOrWhiteSpace(translation.HierarchyName))
            {
              stringList.Add("HierarchyName");
              goto label_32;
            }
            goto label_32;
          }
          goto label_31;
        default:
          goto label_31;
      }
      if (string.IsNullOrWhiteSpace(translation.MeasureName))
        stringList.Add("MeasureName");
label_32:
      if (!Enumerable.Any<string>((IEnumerable<string>) stringList))
        return;
      throw new ArgumentException($"Missing required identification properties for {translation.ObjectType}: {string.Join(", ", (IEnumerable<string>) stringList)}");
    }
label_31:
    throw new ArgumentException("Unknown object type: " + translation.ObjectType);
  }

  public static string GetObjectDisplayName(ObjectTranslationBase translation)
  {
    string objectType = translation.ObjectType;
    string objectDisplayName;
    if (objectType != null)
    {
      switch (objectType.Length)
      {
        case 3:
          if ((objectType == "KPI"))
          {
            objectDisplayName = string.IsNullOrWhiteSpace(translation.TableName) ? "KPI: " + translation.MeasureName : $"KPI: {translation.TableName}.{translation.MeasureName}";
            goto label_18;
          }
          break;
        case 5:
          switch (objectType[0])
          {
            case 'L':
              if ((objectType == "Level"))
              {
                objectDisplayName = $"Level: {translation.TableName}.{translation.HierarchyName}.{translation.LevelName}";
                goto label_18;
              }
              break;
            case 'M':
              if ((objectType == "Model"))
              {
                objectDisplayName = "Model: " + translation.ModelName;
                goto label_18;
              }
              break;
            case 'T':
              if ((objectType == "Table"))
              {
                objectDisplayName = "Table: " + translation.TableName;
                goto label_18;
              }
              break;
          }
          break;
        case 6:
          if ((objectType == "Column"))
          {
            objectDisplayName = $"Column: {translation.TableName}.{translation.ColumnName}";
            goto label_18;
          }
          break;
        case 7:
          if ((objectType == "Measure"))
          {
            objectDisplayName = string.IsNullOrWhiteSpace(translation.TableName) ? "Measure: " + translation.MeasureName : $"Measure: {translation.TableName}.{translation.MeasureName}";
            goto label_18;
          }
          break;
        case 9:
          if ((objectType == "Hierarchy"))
          {
            objectDisplayName = $"Hierarchy: {translation.TableName}.{translation.HierarchyName}";
            goto label_18;
          }
          break;
      }
    }
    objectDisplayName = "Unknown: " + translation.ObjectType;
label_18:
    return objectDisplayName;
  }

  static TranslationHelper()
  {
    HashSet<string> stringSet1 = new HashSet<string>();
    stringSet1.Add("Model");
    stringSet1.Add("Table");
    stringSet1.Add("Column");
    stringSet1.Add("Measure");
    stringSet1.Add("Hierarchy");
    stringSet1.Add("Level");
    stringSet1.Add("KPI");
    TranslationHelper.CaptionSupportedObjects = stringSet1;
    HashSet<string> stringSet2 = new HashSet<string>();
    stringSet2.Add("Model");
    stringSet2.Add("Table");
    stringSet2.Add("Column");
    stringSet2.Add("Measure");
    stringSet2.Add("Hierarchy");
    stringSet2.Add("Level");
    stringSet2.Add("KPI");
    TranslationHelper.DescriptionSupportedObjects = stringSet2;
    HashSet<string> stringSet3 = new HashSet<string>();
    stringSet3.Add("Measure");
    stringSet3.Add("Hierarchy");
    stringSet3.Add("Column");
    TranslationHelper.DisplayFolderSupportedObjects = stringSet3;
    HashSet<string> stringSet4 = new HashSet<string>();
    stringSet4.Add("Model");
    stringSet4.Add("Table");
    stringSet4.Add("Measure");
    stringSet4.Add("Column");
    stringSet4.Add("Hierarchy");
    stringSet4.Add("Level");
    stringSet4.Add("KPI");
    TranslationHelper.ValidObjectTypes = stringSet4;
  }

  public static class TranslatableProperties
  {
    public const string Caption = "Caption";
    public const string Description = "Description";
    public const string DisplayFolder = "DisplayFolder";
  }
}
