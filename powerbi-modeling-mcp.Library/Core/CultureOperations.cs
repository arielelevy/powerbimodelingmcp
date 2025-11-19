// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using Microsoft.AnalysisServices.Tabular;
using Microsoft.AnalysisServices.Tabular.Serialization;
using ModelContextProtocol;
using PowerBIModelingMCP.Library.Common;
using PowerBIModelingMCP.Library.Common.DataStructures;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

#nullable enable
namespace PowerBIModelingMCP.Library.Core;

public static class CultureOperations
{
  public static void ValidateCultureDefinition(CultureBase def, bool isCreate)
  {
    if (def == null)
      throw new McpException("Culture definition cannot be null");
    if (string.IsNullOrWhiteSpace(def.Name))
      throw new McpException("Culture name is required");
    if (!CultureOperations.IsValidCultureName(def.Name))
      throw new McpException($"Invalid culture name format: {def.Name}. Expected format like 'en-US', 'fr-FR', etc.");
    if (def.ExtendedProperties != null)
    {
      List<string> stringList = ExtendedPropertyHelpers.Validate(def.ExtendedProperties);
      if (stringList.Count > 0)
        throw new McpException("ExtendedProperties validation failed: " + string.Join(", ", (IEnumerable<string>) stringList));
    }
    AnnotationHelpers.ValidateAnnotations(def.Annotations);
  }

  public static List<string> GetValidCultureNames(
    bool includeNeutralCultures = true,
    bool includeUserCustomCultures = false)
  {
    CultureTypes cultureTypes = (CultureTypes) 2;
    if (includeNeutralCultures)
      cultureTypes = cultureTypes | (CultureTypes)1;
    if (includeUserCustomCultures)
      cultureTypes = cultureTypes | (CultureTypes)8;
    return Enumerable.ToList<string>(Enumerable.OrderBy<string, string>(Enumerable.Select<CultureInfo, string>(Enumerable.Where<CultureInfo>((IEnumerable<CultureInfo>) CultureInfo.GetCultures(cultureTypes), (c => !string.IsNullOrEmpty(c.Name))), (c => c.Name)), (name => name)));
  }

  public static List<CultureList> ListCultures(string? connectionName = null)
  {
    return Enumerable.ToList<CultureList>(Enumerable.OrderBy<CultureList, string>(Enumerable.Select<Microsoft.AnalysisServices.Tabular.Culture, CultureList>(Enumerable.Where<Microsoft.AnalysisServices.Tabular.Culture>((IEnumerable<Microsoft.AnalysisServices.Tabular.Culture>) ConnectionOperations.Get(connectionName).Database.Model.Cultures, (c => !c.IsRemoved)), (c =>
    {
      int num;
      try
      {
        num = new CultureInfo(c.Name).LCID;
      }
      catch
      {
        num = 0;
      }
      CultureList cultureList = new CultureList { Name = c.Name };
      cultureList.LCID = num;
      ObjectTranslationCollection objectTranslations = c.ObjectTranslations;
      cultureList.TranslationCount = objectTranslations != null ? objectTranslations.Count : 0;
      return cultureList;
    })), (c => c.Name)));
  }

  public static CultureGet GetCulture(string? connectionName, string cultureName)
  {
    ValidationHelpers.ValidateObjectName(cultureName, nameof (cultureName));
    return CultureOperations.MapCultureToGet(ConnectionOperations.Get(connectionName).Database.Model.Cultures.Find(cultureName) ?? throw new McpException($"Culture '{cultureName}' not found"));
  }

  public static OperationResult CreateCulture(string? connectionName, CultureCreate def)
  {
    CultureOperations.ValidateCultureDefinition((CultureBase) def, true);
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    if (info.Database.Model.Cultures.Find(def.Name) != null)
      throw new McpException($"Culture '{def.Name}' already exists");
    Microsoft.AnalysisServices.Tabular.Culture culture1 = new Microsoft.AnalysisServices.Tabular.Culture();
    culture1.Name = def.Name;
    Microsoft.AnalysisServices.Tabular.Culture culture2 = culture1;
    if (def.Annotations != null)
      CultureOperations.ApplyAnnotations(culture2, def.Annotations);
    if (def.ExtendedProperties != null)
      ExtendedPropertyHelpers.ApplyToCulture(culture2, def.ExtendedProperties);
    info.Database.Model.Cultures.Add(culture2);
    TransactionOperations.RecordOperation(info, $"Created culture '{def.Name}' in model {info.Database.Model.Name}");
    ConnectionOperations.SaveChangesWithRollback(info, "create culture");
    return OperationResult.CreateSuccess($"Culture '{def.Name}' created successfully", def.Name, new ObjectType?(ObjectType.Culture), new Operation?(Operation.Create));
  }

  public static OperationResult UpdateCulture(string? connectionName, CultureUpdate update)
  {
    CultureOperations.ValidateCultureDefinition((CultureBase) update, false);
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Culture culture = info.Database.Model.Cultures.Find(update.Name);
    if (culture == null)
      throw new McpException($"Culture '{update.Name}' not found");
    bool flag1 = false;
    if (update.Annotations != null && AnnotationHelpers.ReplaceAnnotations<Microsoft.AnalysisServices.Tabular.Culture>(culture, update.Annotations, (Func<Microsoft.AnalysisServices.Tabular.Culture, ICollection<Microsoft.AnalysisServices.Tabular.Annotation>>) (c => (ICollection<Microsoft.AnalysisServices.Tabular.Annotation>) c.Annotations)))
      flag1 = true;
    if (update.ExtendedProperties != null)
    {
      bool flag2 = culture.ExtendedProperties.Count > 0;
      ExtendedPropertyHelpers.ReplaceExtendedProperties<Microsoft.AnalysisServices.Tabular.Culture>(culture, update.ExtendedProperties, (Func<Microsoft.AnalysisServices.Tabular.Culture, ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>>) (c => (ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>) c.ExtendedProperties));
      if (flag2 || update.ExtendedProperties.Count > 0)
        flag1 = true;
    }
    if (!flag1)
      return OperationResult.CreateSuccess($"Culture '{update.Name}' is already in the requested state", update.Name, new ObjectType?(ObjectType.Culture), new Operation?(Operation.Update), false);
    TransactionOperations.RecordOperation(info, $"Updated culture '{update.Name}' in model {info.Database.Model.Name}");
    ConnectionOperations.SaveChangesWithRollback(info, "update culture");
    return OperationResult.CreateSuccess($"Culture '{update.Name}' updated successfully", update.Name, new ObjectType?(ObjectType.Culture), new Operation?(Operation.Update));
  }

  public static OperationResult DeleteCulture(string? connectionName, string cultureName)
  {
    ValidationHelpers.ValidateObjectName(cultureName, nameof (cultureName));
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    info.Database.Model.Cultures.Remove(info.Database.Model.Cultures.Find(cultureName) ?? throw new McpException($"Culture '{cultureName}' not found"));
    TransactionOperations.RecordOperation(info, $"Deleted culture '{cultureName}' from model {info.Database.Model.Name}");
    ConnectionOperations.SaveChangesWithRollback(info, "delete culture");
    return OperationResult.CreateSuccess($"Culture '{cultureName}' deleted successfully", cultureName, new ObjectType?(ObjectType.Culture), new Operation?(Operation.Delete));
  }

  public static OperationResult RenameCulture(
    string? connectionName,
    string oldName,
    string newName)
  {
    ValidationHelpers.ValidateObjectName(oldName, nameof (oldName));
    ValidationHelpers.ValidateObjectName(newName, nameof (newName));
    if (!CultureOperations.IsValidCultureName(newName))
      throw new McpException($"Invalid culture name format: {newName}. Expected format like 'en-US', 'fr-FR', etc.");
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Culture culture = info.Database.Model.Cultures.Find(oldName);
    if (culture == null)
      throw new McpException($"Culture '{oldName}' not found");
    if (info.Database.Model.Cultures.Find(newName) != null && !string.Equals(oldName, newName, StringComparison.OrdinalIgnoreCase))
      throw new McpException($"Culture '{newName}' already exists");
    culture.RequestRename(newName);
    TransactionOperations.RecordOperation(info, $"Renamed culture from '{oldName}' to '{newName}' in model {info.Database.Model.Name}");
    ConnectionOperations.SaveChangesWithRollback(info, "rename culture", ConnectionOperations.CheckpointMode.AfterRequestRename);
    return OperationResult.CreateSuccess($"Culture renamed from '{oldName}' to '{newName}' successfully", newName, new ObjectType?(ObjectType.Culture), new Operation?(Operation.Update));
  }

  public static List<CultureDetails> GetValidCultureDetails(
    bool includeNeutralCultures = true,
    bool includeUserCustomCultures = false)
  {
    CultureTypes cultureTypes = (CultureTypes) 2;
    if (includeNeutralCultures)
      cultureTypes = cultureTypes | (CultureTypes)1;
    if (includeUserCustomCultures)
      cultureTypes = cultureTypes | (CultureTypes)8;
    return Enumerable.ToList<CultureDetails>(Enumerable.OrderBy<CultureDetails, string>(Enumerable.Select<CultureInfo, CultureDetails>(Enumerable.Where<CultureInfo>((IEnumerable<CultureInfo>) CultureInfo.GetCultures(cultureTypes), (c => !string.IsNullOrEmpty(c.Name))), (c => new CultureDetails()
    {
      Name = c.Name,
      LCID = c.LCID,
      DisplayName = c.DisplayName,
      EnglishName = c.EnglishName,
      IsNeutralCulture = c.IsNeutralCulture,
      IsUserCustomCulture = (c.CultureTypes & (CultureTypes)8) > 0
    })), (ci => ci.Name)));
  }

  public static CultureDetails? GetCultureDetailsByName(string cultureName)
  {
    if (string.IsNullOrWhiteSpace(cultureName))
      return (CultureDetails) null;
    try
    {
      CultureInfo cultureInfo = new CultureInfo(cultureName);
      return new CultureDetails()
      {
        Name = cultureInfo.Name,
        LCID = cultureInfo.LCID,
        DisplayName = cultureInfo.DisplayName,
        EnglishName = cultureInfo.EnglishName,
        IsNeutralCulture = cultureInfo.IsNeutralCulture,
        IsUserCustomCulture = (cultureInfo.CultureTypes & (CultureTypes)8) > 0
      };
    }
    catch (CultureNotFoundException ex)
    {
      return (CultureDetails) null;
    }
    catch (ArgumentException ex)
    {
      return (CultureDetails) null;
    }
  }

  public static CultureDetails? GetCultureDetailsByLCID(int lcid)
  {
    try
    {
      CultureInfo cultureInfo = new CultureInfo(lcid);
      return new CultureDetails()
      {
        Name = cultureInfo.Name,
        LCID = cultureInfo.LCID,
        DisplayName = cultureInfo.DisplayName,
        EnglishName = cultureInfo.EnglishName,
        IsNeutralCulture = cultureInfo.IsNeutralCulture,
        IsUserCustomCulture = (cultureInfo.CultureTypes & (CultureTypes)8) > 0
      };
    }
    catch (CultureNotFoundException ex)
    {
      return (CultureDetails) null;
    }
    catch (ArgumentException ex)
    {
      return (CultureDetails) null;
    }
  }

  private static bool IsValidCultureName(string cultureName)
  {
    if (string.IsNullOrWhiteSpace(cultureName))
      return false;
    try
    {
      CultureInfo cultureInfo = new CultureInfo(cultureName);
      return true;
    }
    catch (CultureNotFoundException ex)
    {
      return false;
    }
    catch (ArgumentException ex)
    {
      return false;
    }
  }

  private static CultureGet MapCultureToGet(Microsoft.AnalysisServices.Tabular.Culture culture)
  {
    CultureGet cultureGet = new CultureGet { Name = culture.Name };
    cultureGet.ModifiedTime = new DateTime?(culture.ModifiedTime);
    cultureGet.StructureModifiedTime = new DateTime?(culture.StructureModifiedTime);
    cultureGet.IsRemoved = new bool?(culture.IsRemoved);
    cultureGet.Annotations = new List<KeyValuePair<string, string>>();
    cultureGet.ExtendedProperties = new List<PowerBIModelingMCP.Library.Common.DataStructures.ExtendedProperty>();
    CultureGet get = cultureGet;
    foreach (Microsoft.AnalysisServices.Tabular.Annotation annotation in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.Annotation, Microsoft.AnalysisServices.Tabular.Culture>) culture.Annotations)
      get.Annotations.Add(new KeyValuePair<string, string>(annotation.Name, annotation.Value));
    get.ExtendedProperties = ExtendedPropertyHelpers.ExtractFromCulture(culture);
    get.LinguisticMetadataReference = (string) null;
    get.ObjectTranslationReferences.Clear();
    return get;
  }

  private static void ApplyAnnotations(
    Microsoft.AnalysisServices.Tabular.Culture culture,
    List<KeyValuePair<string, string>> annotations)
  {
    foreach (KeyValuePair<string, string> annotation1 in annotations)
    {
      if (!string.IsNullOrWhiteSpace(annotation1.Key))
      {
        Microsoft.AnalysisServices.Tabular.Annotation annotation2 = new Microsoft.AnalysisServices.Tabular.Annotation();
        annotation2.Name = annotation1.Key;
        annotation2.Value = annotation1.Value ?? string.Empty;
        Microsoft.AnalysisServices.Tabular.Annotation metadataObject = annotation2;
        culture.Annotations.Add(metadataObject);
      }
    }
  }

  public static string ExportTMDL(string? connectionName, string cultureName, ExportTmdl? options)
  {
    Microsoft.AnalysisServices.Tabular.Culture @object = ConnectionOperations.Get(connectionName).Database.Model.Cultures.Find(cultureName);
    if (@object == null)
      throw new ArgumentException($"Culture '{cultureName}' not found");
    if (options?.SerializationOptions == null)
      return TmdlSerializer.SerializeObject((MetadataObject) @object);
    MetadataSerializationOptions serializationOptions = options.SerializationOptions.ToMetadataSerializationOptions();
    return TmdlSerializer.SerializeObject((MetadataObject) @object, serializationOptions);
  }

  public class CultureOperationResult
  {
    public string CultureName { get; set; } = string.Empty;

    public string? State { get; set; }

    public string? ErrorMessage { get; set; }

    public bool Success { get; set; }

    public string? Message { get; set; }

    public bool HasChanges { get; set; }
  }
}
