// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Core.MeasureOperations
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using Microsoft.AnalysisServices.Tabular;
using Microsoft.AnalysisServices.Tabular.Serialization;
using ModelContextProtocol;
using PowerBIModelingMCP.Library.Common;
using PowerBIModelingMCP.Library.Common.DataStructures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

#nullable enable
namespace PowerBIModelingMCP.Library.Core;

public static class MeasureOperations
{
  public static void ValidateMeasureDefinition(MeasureBase def, bool isCreate)
  {
    if (def == null)
      throw new McpException("Measure definition cannot be null");
    if (string.IsNullOrWhiteSpace(def.Name))
      throw new McpException("Name is required");
    if (isCreate && string.IsNullOrWhiteSpace(def.Expression))
      throw new McpException("Expression is required for creation");
    Microsoft.AnalysisServices.DataType dataType;
    if (!string.IsNullOrWhiteSpace(def.DataType) && !Enum.TryParse<Microsoft.AnalysisServices.DataType>(def.DataType, out dataType))
    {
      string[] names = Enum.GetNames(typeof (Microsoft.AnalysisServices.DataType));
      throw new McpException($"Invalid DataType '{def.DataType}'. Valid values are: {string.Join(", ", names)}");
    }
    if (def.ExtendedProperties != null)
    {
      List<string> stringList = ExtendedPropertyHelpers.Validate(def.ExtendedProperties);
      if (stringList.Count > 0)
        throw new McpException("ExtendedProperties validation failed: " + string.Join(", ", (IEnumerable<string>) stringList));
    }
    AnnotationHelpers.ValidateAnnotations(def.Annotations);
  }

  public static Microsoft.AnalysisServices.Tabular.Measure FindMeasure(
    Microsoft.AnalysisServices.Tabular.Model model,
    string measureName)
  {
    foreach (Microsoft.AnalysisServices.Tabular.Table table in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.Table, Microsoft.AnalysisServices.Tabular.Model>) model.Tables)
    {
      Microsoft.AnalysisServices.Tabular.Measure measure = table.Measures.Find(measureName);
      if (measure != null)
        return measure;
    }
    throw new McpException($"Measure [{measureName}] not found in the model");
  }

  public static List<MeasureList> ListMeasures(
    string? connectionName,
    string? tableName,
    int? maxResults,
    out int totalCount)
  {
    Microsoft.AnalysisServices.Tabular.Database database = ConnectionOperations.Get(connectionName).Database;
    IEnumerable<Microsoft.AnalysisServices.Tabular.Measure> measures;
    if (!string.IsNullOrWhiteSpace(tableName))
      measures = (IEnumerable<Microsoft.AnalysisServices.Tabular.Measure>) (database.Model.Tables.Find(tableName) ?? throw new McpException($"Table '{tableName}' not found")).Measures;
    else
      measures = Enumerable.SelectMany<Microsoft.AnalysisServices.Tabular.Table, Microsoft.AnalysisServices.Tabular.Measure>((IEnumerable<Microsoft.AnalysisServices.Tabular.Table>) database.Model.Tables, (Func<Microsoft.AnalysisServices.Tabular.Table, IEnumerable<Microsoft.AnalysisServices.Tabular.Measure>>) (t => (IEnumerable<Microsoft.AnalysisServices.Tabular.Measure>) t.Measures));
    List<MeasureList> list = Enumerable.ToList<MeasureList>(Enumerable.Select<Microsoft.AnalysisServices.Tabular.Measure, MeasureList>(measures, (m =>
    {
      return new MeasureList()
      {
        Name = m.Name,
        Description = !string.IsNullOrEmpty(m.Description) ? m.Description : (string) null,
        DisplayFolder = !string.IsNullOrEmpty(m.DisplayFolder) ? m.DisplayFolder : (string) null
      };
    })));
    totalCount = list.Count;
    return maxResults.HasValue && maxResults.Value > 0 ? Enumerable.ToList<MeasureList>(Enumerable.Take<MeasureList>((IEnumerable<MeasureList>) list, maxResults.Value)) : list;
  }

  public static List<Dictionary<string, string>> ListAllMeasures(string? connectionName)
  {
    Microsoft.AnalysisServices.Tabular.Database database = ConnectionOperations.Get(connectionName).Database;
    List<Dictionary<string, string>> dictionaryList = new List<Dictionary<string, string>>();
    foreach (Microsoft.AnalysisServices.Tabular.Table table in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.Table, Microsoft.AnalysisServices.Tabular.Model>) database.Model.Tables)
    {
      foreach (Microsoft.AnalysisServices.Tabular.Measure measure in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.Measure, Microsoft.AnalysisServices.Tabular.Table>) table.Measures)
        dictionaryList.Add(new Dictionary<string, string>()
        {
          ["TableName"] = table.Name,
          ["MeasureName"] = measure.Name,
          ["Expression"] = measure.Expression ?? "",
          ["IsHidden"] = measure.IsHidden.ToString()
        });
    }
    return dictionaryList;
  }

  public static MeasureGet GetMeasure(string? connectionName, string measureName)
  {
    if (string.IsNullOrWhiteSpace(measureName))
      throw new McpException("measureName is required");
    Microsoft.AnalysisServices.Tabular.Measure measure1 = MeasureOperations.FindMeasure(ConnectionOperations.Get(connectionName).Database.Model, measureName);
    Microsoft.AnalysisServices.Tabular.Table table = measure1.Table;
    MeasureGet measureGet = new MeasureGet { TableName = table.Name, Name = measure1.Name };
    measureGet.Expression = measure1.Expression;
    measureGet.Description = measure1.Description;
    measureGet.FormatString = measure1.FormatString;
    measureGet.IsHidden = new bool?(measure1.IsHidden);
    measureGet.IsSimpleMeasure = new bool?(measure1.IsSimpleMeasure);
    measureGet.DisplayFolder = measure1.DisplayFolder;
    measureGet.DataType = measure1.DataType.ToString();
    measureGet.DataCategory = measure1.DataCategory;
    measureGet.LineageTag = measure1.LineageTag;
    measureGet.SourceLineageTag = measure1.SourceLineageTag;
    measureGet.DetailRowsExpression = measure1.DetailRowsDefinition?.Expression;
    measureGet.FormatStringExpression = measure1.FormatStringDefinition?.Expression;
    measureGet.State = measure1.State.ToString();
    measureGet.ErrorMessage = measure1.ErrorMessage;
    measureGet.ModifiedTime = new DateTime?(measure1.ModifiedTime);
    measureGet.StructureModifiedTime = new DateTime?(measure1.StructureModifiedTime);
    MeasureGet measure2 = measureGet;
    if (measure1.KPI != null)
    {
      KPIDefinition kpiDefinition = new KPIDefinition()
      {
        StatusExpression = measure1.KPI.StatusExpression,
        StatusGraphic = measure1.KPI.StatusGraphic,
        TrendExpression = measure1.KPI.TrendExpression,
        TrendGraphic = measure1.KPI.TrendGraphic,
        TargetExpression = measure1.KPI.TargetExpression,
        TargetFormatString = measure1.KPI.TargetFormatString,
        TargetDescription = measure1.KPI.TargetDescription,
        StatusDescription = measure1.KPI.StatusDescription,
        TrendDescription = measure1.KPI.TrendDescription,
        Annotations = new List<KeyValuePair<string, string>>()
      };
      foreach (Microsoft.AnalysisServices.Tabular.Annotation annotation in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.Annotation, Microsoft.AnalysisServices.Tabular.KPI>) measure1.KPI.Annotations)
        kpiDefinition.Annotations.Add(new KeyValuePair<string, string>(annotation.Name, annotation.Value));
      measure2.KPI = System.Text.Json.JsonSerializer.Serialize<KPIDefinition>(kpiDefinition);
    }
    measure2.Annotations = new List<KeyValuePair<string, string>>();
    foreach (Microsoft.AnalysisServices.Tabular.Annotation annotation in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.Annotation, Microsoft.AnalysisServices.Tabular.Measure>) measure1.Annotations)
      measure2.Annotations.Add(new KeyValuePair<string, string>(annotation.Name, annotation.Value));
    measure2.ExtendedProperties = ExtendedPropertyHelpers.ExtractFromMeasure(measure1);
    return measure2;
  }

  public static MeasureOperations.MeasureOperationResult CreateMeasure(
    string? connectionName,
    MeasureCreate def)
  {
    MeasureOperations.ValidateMeasureDefinition((MeasureBase) def, true);
    PowerBIModelingMCP.Library.Common.DataStructures.ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Database database = info.Database;
    foreach (Microsoft.AnalysisServices.Tabular.Table table in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.Table, Microsoft.AnalysisServices.Tabular.Model>) database.Model.Tables)
    {
      if (table.Measures.Contains(def.Name))
        throw new McpException($"Measure '{def.Name}' already exists in table '{table.Name}'");
    }
    if (string.IsNullOrWhiteSpace(def.TableName))
      throw new McpException("Must specify host table name when creating a measure.");
    Microsoft.AnalysisServices.Tabular.Table table1 = database.Model.Tables.Find(def.TableName) ?? throw new McpException($"Table '{def.TableName}' not found");
    Microsoft.AnalysisServices.Tabular.Measure measure1 = new Microsoft.AnalysisServices.Tabular.Measure();
    measure1.Name = def.Name;
    measure1.Expression = def.Expression;
    Microsoft.AnalysisServices.Tabular.Measure measure2 = measure1;
    MeasureOperations.ApplyMeasureProperties(measure2, (MeasureBase) def);
    table1.Measures.Add(measure2);
    TransactionOperations.RecordOperation(info, $"Created measure '{def.Name}' in table '{table1.Name}'");
    ConnectionOperations.SaveChangesWithRollback(info, "create measure");
    return new MeasureOperations.MeasureOperationResult()
    {
      State = measure2.State.ToString(),
      ErrorMessage = measure2.ErrorMessage,
      MeasureName = measure2.Name,
      TableName = table1.Name
    };
  }

  public static MeasureOperations.MeasureOperationResult UpdateMeasure(
    string? connectionName,
    MeasureUpdate update)
  {
    MeasureOperations.ValidateMeasureDefinition((MeasureBase) update, false);
    PowerBIModelingMCP.Library.Common.DataStructures.ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Measure measure = MeasureOperations.FindMeasure(info.Database.Model, update.Name);
    Microsoft.AnalysisServices.Tabular.Table table = measure.Table;
    if (!string.IsNullOrEmpty(update.TableName) && string.Compare(table.Name, update.TableName, true) != 0)
      throw new McpException($"Measure [{update.Name}] doesn't belong to table '{update.TableName}'.");
    if (!MeasureOperations.ApplyMeasureUpdates(measure, update))
      return new MeasureOperations.MeasureOperationResult()
      {
        State = measure.State.ToString(),
        ErrorMessage = measure.ErrorMessage,
        MeasureName = measure.Name,
        TableName = table.Name,
        HasChanges = false
      };
    TransactionOperations.RecordOperation(info, $"Updated measure '{update.Name}' in table '{table.Name}'");
    ConnectionOperations.SaveChangesWithRollback(info, "update measure");
    return new MeasureOperations.MeasureOperationResult()
    {
      State = measure.State.ToString(),
      ErrorMessage = measure.ErrorMessage,
      MeasureName = measure.Name,
      TableName = table.Name,
      HasChanges = true
    };
  }

  public static void RenameMeasure(string? connectionName, string oldName, string newName)
  {
    if (string.IsNullOrWhiteSpace(oldName))
      throw new McpException("oldName is required");
    if (string.IsNullOrWhiteSpace(newName))
      throw new McpException("newName is required");
    PowerBIModelingMCP.Library.Common.DataStructures.ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Database database = info.Database;
    Microsoft.AnalysisServices.Tabular.Measure measure = MeasureOperations.FindMeasure(database.Model, oldName);
    Microsoft.AnalysisServices.Tabular.Table table1 = measure.Table;
    foreach (Microsoft.AnalysisServices.Tabular.Table table2 in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.Table, Microsoft.AnalysisServices.Tabular.Model>) database.Model.Tables)
    {
      if (table2.Measures.Contains(newName) && !string.Equals(oldName, newName, StringComparison.OrdinalIgnoreCase))
        throw new McpException($"Measure '{newName}' already exists in table '{table2.Name}'");
    }
    measure.RequestRename(newName);
    TransactionOperations.RecordOperation(info, $"Renamed measure '{oldName}' to '{newName}' in table '{table1.Name}'");
    ConnectionOperations.SaveChangesWithRollback(info, "rename measure", ConnectionOperations.CheckpointMode.AfterRequestRename);
  }

  public static void DeleteMeasure(
    string? connectionName,
    string measureName,
    bool shouldCascadeDelete)
  {
    if (string.IsNullOrWhiteSpace(measureName))
      throw new McpException("measureName is required");
    PowerBIModelingMCP.Library.Common.DataStructures.ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Database database = info.Database;
    Microsoft.AnalysisServices.Tabular.Measure measure = MeasureOperations.FindMeasure(database.Model, measureName);
    Microsoft.AnalysisServices.Tabular.Table table = measure.Table;
    List<string> stringList = StructuralDependencyHelper.CheckAndDeleteDependenciesIfRequired(database, (NamedMetadataObject) measure, shouldCascadeDelete);
    if (!shouldCascadeDelete && Enumerable.Any<string>((IEnumerable<string>) stringList))
      throw new McpException($"Cannot delete measure '{measureName}' because it is used by: {string.Join(", ", (IEnumerable<string>) stringList)}");
    table.Measures.Remove(measure);
    TransactionOperations.RecordOperation(info, $"Deleted measure '{measureName}' from table '{table.Name}'");
    ConnectionOperations.SaveChangesWithRollback(info, "delete measure");
  }

  public static void MoveMeasure(string? connectionName, string targetTableName, string measureName)
  {
    if (string.IsNullOrWhiteSpace(targetTableName))
      throw new McpException("targetTableName is required");
    if (string.IsNullOrWhiteSpace(measureName))
      throw new McpException("measureName is required");
    PowerBIModelingMCP.Library.Common.DataStructures.ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Database database = info.Database;
    Microsoft.AnalysisServices.Tabular.Table table1 = database.Model.Tables.Find(targetTableName) ?? throw new McpException($"Target table '{targetTableName}' not found");
    Microsoft.AnalysisServices.Tabular.Measure measure1 = MeasureOperations.FindMeasure(database.Model, measureName);
    Microsoft.AnalysisServices.Tabular.Table table2 = measure1.Table;
    if (table2 == table1)
      throw new McpException($"Measure '{measureName}' is already in table '{targetTableName}'");
    List<string> stringList = StructuralDependencyHelper.CheckAndDeleteDependenciesIfRequired(database, (NamedMetadataObject) measure1, true);
    Microsoft.AnalysisServices.Tabular.Measure measure2 = new Microsoft.AnalysisServices.Tabular.Measure();
    measure1.CopyTo(measure2);
    table2.Measures.Remove(measure1);
    table1.Measures.Add(measure2);
    foreach (string str1 in stringList)
    {
      if (str1.Contains("Perspective"))
      {
        string str2 = str1.Split(',', (StringSplitOptions) 0)[0].Replace("Perspective: ", "");
        Microsoft.AnalysisServices.Tabular.Perspective perspective = database.Model.Perspectives.Find(str2);
        if (perspective != null)
        {
          if (perspective.PerspectiveTables.Find(targetTableName) == null)
          {
            Microsoft.AnalysisServices.Tabular.PerspectiveTable metadataObject = new Microsoft.AnalysisServices.Tabular.PerspectiveTable()
            {
              Table = table1
            };
            perspective.PerspectiveTables.Add(metadataObject);
          }
          PerspectiveMeasureCreate perspectiveMeasureCreate = new PerspectiveMeasureCreate { MeasureName = measure2.Name };
          perspectiveMeasureCreate.TableName = table1.Name;
          PerspectiveMeasureCreate def = perspectiveMeasureCreate;
          PerspectiveOperations.AddMeasureToPerspectiveTable(connectionName, str2, def);
        }
      }
      else if (str1.Contains("Culture"))
      {
        string[] strArray = str1.Split(',', (StringSplitOptions) 0);
        string name = strArray[0].Replace("Culture: ", "");
        string str3 = strArray[1].Replace(" Measure Translation: ", "");
        string str4 = strArray[2].Replace(" TranslatedProperty: ", "");
        if (database.Model.Cultures.Find(name) != null)
        {
          ObjectTranslationCreate translationDef = new ObjectTranslationCreate
          {
            CultureName = name,
            ObjectType = "Measure",
            MeasureName = measure2.Name,
            Property = str4,
            Value = str3
          };
          ObjectTranslationOperations.CreateObjectTranslation(connectionName, translationDef);
        }
      }
    }
    TransactionOperations.RecordOperation(info, $"Moved measure '{measureName}' from table '{table2.Name}' to table '{targetTableName}'");
    ConnectionOperations.SaveChangesWithRollback(info, "move measure");
  }

  public static MeasureOperations.MeasureValidationResult ValidateMeasureExpression(
    string? connectionName,
    string expression)
  {
    if (string.IsNullOrWhiteSpace(expression))
      throw new McpException("expression is required");
    PowerBIModelingMCP.Library.Common.DataStructures.ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Model model = info.Database.Model;
    Microsoft.AnalysisServices.Tabular.Table table1 = Enumerable.FirstOrDefault<Microsoft.AnalysisServices.Tabular.Table>((IEnumerable<Microsoft.AnalysisServices.Tabular.Table>) model.Tables) ?? throw new McpException("No tables found in the model");
    HashSet<string> stringSet = new HashSet<string>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    foreach (Microsoft.AnalysisServices.Tabular.Table table2 in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.Table, Microsoft.AnalysisServices.Tabular.Model>) model.Tables)
    {
      foreach (Microsoft.AnalysisServices.Tabular.Measure measure in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.Measure, Microsoft.AnalysisServices.Tabular.Table>) table2.Measures)
        stringSet.Add(measure.Name);
    }
    int num = 1;
    string str;
    do
    {
      str = $"__TempMeasure{num}";
      ++num;
    }
    while (stringSet.Contains(str));
    MeasureOperations.MeasureValidationResult validationResult = new MeasureOperations.MeasureValidationResult()
    {
      Expression = expression
    };
    Stopwatch stopwatch = Stopwatch.StartNew();
    Microsoft.AnalysisServices.Tabular.Measure metadataObject = (Microsoft.AnalysisServices.Tabular.Measure) null;
    bool flag = false;
    try
    {
      Microsoft.AnalysisServices.Tabular.Measure measure = new Microsoft.AnalysisServices.Tabular.Measure();
      measure.Name = str;
      measure.Expression = expression;
      metadataObject = measure;
      table1.Measures.Add(metadataObject);
      flag = true;
      ConnectionOperations.SaveChangesIfNeeded(info);
      validationResult.ObjectState = metadataObject.State.ToString();
      if (metadataObject.State != ObjectState.Ready)
      {
        validationResult.IsValid = false;
        validationResult.ErrorMessage = metadataObject.ErrorMessage ?? "Unknown error during validation.";
      }
      else
      {
        validationResult.IsValid = true;
        validationResult.Message = "Expression is valid.";
      }
    }
    catch (Exception ex)
    {
      validationResult.IsValid = false;
      validationResult.ErrorMessage = ex.Message;
    }
    finally
    {
      stopwatch.Stop();
      validationResult.ValidationTimeMs = stopwatch.ElapsedMilliseconds;
      if (flag)
      {
        if (metadataObject != null)
        {
          try
          {
            table1.Measures.Remove(metadataObject);
            ConnectionOperations.SaveChangesIfNeeded(info, ConnectionOperations.CheckpointMode.ForceEvenInTransaction);
          }
          catch (Exception ex)
          {
          }
        }
      }
    }
    return validationResult;
  }

  private static void ApplyMeasureProperties(Microsoft.AnalysisServices.Tabular.Measure measure, MeasureBase def)
  {
    if (!string.IsNullOrWhiteSpace(def.Description))
      measure.Description = def.Description;
    if (!string.IsNullOrWhiteSpace(def.FormatString))
      measure.FormatString = def.FormatString;
    measure.IsHidden = def.IsHidden.GetValueOrDefault();
    measure.IsSimpleMeasure = def.IsSimpleMeasure.GetValueOrDefault();
    if (!string.IsNullOrWhiteSpace(def.DisplayFolder))
      measure.DisplayFolder = def.DisplayFolder;
    if (!string.IsNullOrWhiteSpace(def.DataType))
      throw new McpException("Cannot set the data type of a new measure explicitly.");
    if (!string.IsNullOrWhiteSpace(def.DataCategory))
      measure.DataCategory = def.DataCategory;
    if (!string.IsNullOrWhiteSpace(def.LineageTag))
      measure.LineageTag = def.LineageTag;
    if (!string.IsNullOrWhiteSpace(def.SourceLineageTag))
      measure.SourceLineageTag = def.SourceLineageTag;
    if (!string.IsNullOrWhiteSpace(def.DetailRowsExpression))
    {
      if (measure.DetailRowsDefinition == null)
        measure.DetailRowsDefinition = new Microsoft.AnalysisServices.Tabular.DetailRowsDefinition();
      measure.DetailRowsDefinition.Expression = def.DetailRowsExpression;
    }
    if (!string.IsNullOrWhiteSpace(def.FormatStringExpression))
    {
      if (measure.FormatStringDefinition == null)
        measure.FormatStringDefinition = new Microsoft.AnalysisServices.Tabular.FormatStringDefinition();
      measure.FormatStringDefinition.Expression = def.FormatStringExpression;
    }
    if (!string.IsNullOrWhiteSpace(def.KPI))
    {
      try
      {
        KPIDefinition kpiDef = System.Text.Json.JsonSerializer.Deserialize<KPIDefinition>(def.KPI);
        if (kpiDef != null)
        {
          Microsoft.AnalysisServices.Tabular.KPI kpi = new Microsoft.AnalysisServices.Tabular.KPI();
          MeasureOperations.ApplyKPIProperties(kpi, kpiDef);
          measure.KPI = kpi;
        }
      }
      catch (Exception ex)
      {
        throw new McpException("Invalid KPI definition: " + ex.Message);
      }
    }
    if (def.Annotations != null)
      AnnotationHelpers.ApplyAnnotations<Microsoft.AnalysisServices.Tabular.Measure>(measure, def.Annotations, (Func<Microsoft.AnalysisServices.Tabular.Measure, ICollection<Microsoft.AnalysisServices.Tabular.Annotation>>) (m => (ICollection<Microsoft.AnalysisServices.Tabular.Annotation>) m.Annotations));
    if (def.ExtendedProperties == null)
      return;
    ExtendedPropertyHelpers.ApplyToMeasure(measure, def.ExtendedProperties);
  }

  private static void ApplyKPIProperties(Microsoft.AnalysisServices.Tabular.KPI kpi, KPIDefinition kpiDef)
  {
    if (!string.IsNullOrWhiteSpace(kpiDef.StatusExpression))
      kpi.StatusExpression = kpiDef.StatusExpression;
    if (!string.IsNullOrWhiteSpace(kpiDef.StatusGraphic))
      kpi.StatusGraphic = kpiDef.StatusGraphic;
    if (!string.IsNullOrWhiteSpace(kpiDef.TrendExpression))
      kpi.TrendExpression = kpiDef.TrendExpression;
    if (!string.IsNullOrWhiteSpace(kpiDef.TrendGraphic))
      kpi.TrendGraphic = kpiDef.TrendGraphic;
    if (!string.IsNullOrWhiteSpace(kpiDef.TargetExpression))
      kpi.TargetExpression = kpiDef.TargetExpression;
    if (!string.IsNullOrWhiteSpace(kpiDef.TargetFormatString))
      kpi.TargetFormatString = kpiDef.TargetFormatString;
    if (!string.IsNullOrWhiteSpace(kpiDef.TargetDescription))
      kpi.TargetDescription = kpiDef.TargetDescription;
    if (!string.IsNullOrWhiteSpace(kpiDef.StatusDescription))
      kpi.StatusDescription = kpiDef.StatusDescription;
    if (!string.IsNullOrWhiteSpace(kpiDef.TrendDescription))
      kpi.TrendDescription = kpiDef.TrendDescription;
    if (kpiDef.Annotations == null)
      return;
    AnnotationHelpers.ApplyAnnotations<Microsoft.AnalysisServices.Tabular.KPI>(kpi, kpiDef.Annotations, (Func<Microsoft.AnalysisServices.Tabular.KPI, ICollection<Microsoft.AnalysisServices.Tabular.Annotation>>) (k => (ICollection<Microsoft.AnalysisServices.Tabular.Annotation>) k.Annotations));
  }

  private static bool ApplyMeasureUpdates(Microsoft.AnalysisServices.Tabular.Measure measure, MeasureUpdate update)
  {
    bool flag1 = false;
    if (update.Expression != null && (measure.Expression != update.Expression))
    {
      measure.Expression = !string.IsNullOrWhiteSpace(update.Expression) ? update.Expression : throw new McpException("Expression cannot be empty. Use a valid DAX expression.");
      flag1 = true;
    }
    if (update.Description != null)
    {
      string description = string.IsNullOrEmpty(update.Description) ? (string) null : update.Description;
      if ((measure.Description != description))
      {
        measure.Description = description;
        flag1 = true;
      }
    }
    if (update.FormatString != null)
    {
      string formatString = string.IsNullOrEmpty(update.FormatString) ? (string) null : update.FormatString;
      if ((measure.FormatString != formatString))
      {
        measure.FormatString = formatString;
        flag1 = true;
      }
    }
    bool? nullable;
    if (update.IsHidden.HasValue)
    {
      int num1 = measure.IsHidden ? 1 : 0;
      nullable = update.IsHidden;
      int num2 = nullable.Value ? 1 : 0;
      if (num1 != num2)
      {
        Microsoft.AnalysisServices.Tabular.Measure measure1 = measure;
        nullable = update.IsHidden;
        int num3 = nullable.Value ? 1 : 0;
        measure1.IsHidden = num3 != 0;
        flag1 = true;
      }
    }
    nullable = update.IsSimpleMeasure;
    if (nullable.HasValue)
    {
      int num4 = measure.IsSimpleMeasure ? 1 : 0;
      nullable = update.IsSimpleMeasure;
      int num5 = nullable.Value ? 1 : 0;
      if (num4 != num5)
      {
        Microsoft.AnalysisServices.Tabular.Measure measure2 = measure;
        nullable = update.IsSimpleMeasure;
        int num6 = nullable.Value ? 1 : 0;
        measure2.IsSimpleMeasure = num6 != 0;
        flag1 = true;
      }
    }
    if (update.DisplayFolder != null)
    {
      string displayFolder = string.IsNullOrEmpty(update.DisplayFolder) ? (string) null : update.DisplayFolder;
      if ((measure.DisplayFolder != displayFolder))
      {
        measure.DisplayFolder = displayFolder;
        flag1 = true;
      }
    }
    if (!string.IsNullOrWhiteSpace(update.DataType))
      throw new InvalidOperationException("Cannot change the data type of measure explicitly.");
    if (update.DataCategory != null)
    {
      string dataCategory = string.IsNullOrEmpty(update.DataCategory) ? (string) null : update.DataCategory;
      if ((measure.DataCategory != dataCategory))
      {
        measure.DataCategory = dataCategory;
        flag1 = true;
      }
    }
    if (update.LineageTag != null)
    {
      string lineageTag = string.IsNullOrEmpty(update.LineageTag) ? (string) null : update.LineageTag;
      if ((measure.LineageTag != lineageTag))
      {
        measure.LineageTag = lineageTag;
        flag1 = true;
      }
    }
    if (update.SourceLineageTag != null)
    {
      string sourceLineageTag = string.IsNullOrEmpty(update.SourceLineageTag) ? (string) null : update.SourceLineageTag;
      if ((measure.SourceLineageTag != sourceLineageTag))
      {
        measure.SourceLineageTag = sourceLineageTag;
        flag1 = true;
      }
    }
    if (update.DetailRowsExpression != null)
    {
      string expression = measure.DetailRowsDefinition?.Expression;
      if (string.IsNullOrEmpty(update.DetailRowsExpression))
      {
        if (measure.DetailRowsDefinition != null)
        {
          measure.DetailRowsDefinition = (Microsoft.AnalysisServices.Tabular.DetailRowsDefinition) null;
          flag1 = true;
        }
      }
      else if ((expression != update.DetailRowsExpression))
      {
        if (measure.DetailRowsDefinition == null)
          measure.DetailRowsDefinition = new Microsoft.AnalysisServices.Tabular.DetailRowsDefinition();
        measure.DetailRowsDefinition.Expression = update.DetailRowsExpression;
        flag1 = true;
      }
    }
    if (update.FormatStringExpression != null)
    {
      string expression = measure.FormatStringDefinition?.Expression;
      if (string.IsNullOrEmpty(update.FormatStringExpression))
      {
        if (measure.FormatStringDefinition != null)
        {
          measure.FormatStringDefinition = (Microsoft.AnalysisServices.Tabular.FormatStringDefinition) null;
          flag1 = true;
        }
      }
      else if ((expression != update.FormatStringExpression))
      {
        if (measure.FormatStringDefinition == null)
          measure.FormatStringDefinition = new Microsoft.AnalysisServices.Tabular.FormatStringDefinition();
        measure.FormatStringDefinition.Expression = update.FormatStringExpression;
        flag1 = true;
      }
    }
    bool flag2 = MeasureOperations.ApplyKPIUpdates(measure, update.KPI ?? string.Empty) | flag1;
    if (update.Annotations != null && AnnotationHelpers.ReplaceAnnotations<Microsoft.AnalysisServices.Tabular.Measure>(measure, update.Annotations, (Func<Microsoft.AnalysisServices.Tabular.Measure, ICollection<Microsoft.AnalysisServices.Tabular.Annotation>>) (m => (ICollection<Microsoft.AnalysisServices.Tabular.Annotation>) m.Annotations)))
      flag2 = true;
    if (update.ExtendedProperties != null)
    {
      int num = measure.ExtendedProperties.Count > 0 ? 1 : 0;
      ExtendedPropertyHelpers.ReplaceExtendedProperties<Microsoft.AnalysisServices.Tabular.Measure>(measure, update.ExtendedProperties, (Func<Microsoft.AnalysisServices.Tabular.Measure, ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>>) (m => (ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>) m.ExtendedProperties));
      if (num != 0 || update.ExtendedProperties.Count > 0)
        flag2 = true;
    }
    return flag2;
  }

  private static bool ApplyKPIUpdates(Microsoft.AnalysisServices.Tabular.Measure measure, string kpiJson)
  {
    bool flag = false;
    if (string.IsNullOrEmpty(kpiJson))
    {
      if (measure.KPI != null)
      {
        measure.KPI = (Microsoft.AnalysisServices.Tabular.KPI) null;
        flag = true;
      }
    }
    else
    {
      try
      {
        KPIDefinition kpiDef = System.Text.Json.JsonSerializer.Deserialize<KPIDefinition>(kpiJson);
        if (kpiDef != null)
        {
          if (measure.KPI == null)
          {
            measure.KPI = new Microsoft.AnalysisServices.Tabular.KPI();
            flag = true;
          }
          flag = MeasureOperations.ApplyKPIPropertyUpdates(measure.KPI, kpiDef) | flag;
        }
      }
      catch (Exception ex)
      {
        throw new McpException("Invalid KPI definition: " + ex.Message);
      }
    }
    return flag;
  }

  private static bool ApplyKPIPropertyUpdates(Microsoft.AnalysisServices.Tabular.KPI kpi, KPIDefinition kpiDef)
  {
    bool flag = false;
    if (kpiDef.StatusExpression != null)
    {
      string statusExpression = string.IsNullOrEmpty(kpiDef.StatusExpression) ? (string) null : kpiDef.StatusExpression;
      if ((kpi.StatusExpression != statusExpression))
      {
        kpi.StatusExpression = statusExpression;
        flag = true;
      }
    }
    if (kpiDef.StatusGraphic != null)
    {
      string statusGraphic = string.IsNullOrEmpty(kpiDef.StatusGraphic) ? (string) null : kpiDef.StatusGraphic;
      if ((kpi.StatusGraphic != statusGraphic))
      {
        kpi.StatusGraphic = statusGraphic;
        flag = true;
      }
    }
    if (kpiDef.TrendExpression != null)
    {
      string trendExpression = string.IsNullOrEmpty(kpiDef.TrendExpression) ? (string) null : kpiDef.TrendExpression;
      if ((kpi.TrendExpression != trendExpression))
      {
        kpi.TrendExpression = trendExpression;
        flag = true;
      }
    }
    if (kpiDef.TrendGraphic != null)
    {
      string trendGraphic = string.IsNullOrEmpty(kpiDef.TrendGraphic) ? (string) null : kpiDef.TrendGraphic;
      if ((kpi.TrendGraphic != trendGraphic))
      {
        kpi.TrendGraphic = trendGraphic;
        flag = true;
      }
    }
    if (kpiDef.TargetExpression != null)
    {
      string targetExpression = string.IsNullOrEmpty(kpiDef.TargetExpression) ? (string) null : kpiDef.TargetExpression;
      if ((kpi.TargetExpression != targetExpression))
      {
        kpi.TargetExpression = targetExpression;
        flag = true;
      }
    }
    if (kpiDef.TargetFormatString != null)
    {
      string targetFormatString = string.IsNullOrEmpty(kpiDef.TargetFormatString) ? (string) null : kpiDef.TargetFormatString;
      if ((kpi.TargetFormatString != targetFormatString))
      {
        kpi.TargetFormatString = targetFormatString;
        flag = true;
      }
    }
    if (kpiDef.TargetDescription != null)
    {
      string targetDescription = string.IsNullOrEmpty(kpiDef.TargetDescription) ? (string) null : kpiDef.TargetDescription;
      if ((kpi.TargetDescription != targetDescription))
      {
        kpi.TargetDescription = targetDescription;
        flag = true;
      }
    }
    if (kpiDef.StatusDescription != null)
    {
      string statusDescription = string.IsNullOrEmpty(kpiDef.StatusDescription) ? (string) null : kpiDef.StatusDescription;
      if ((kpi.StatusDescription != statusDescription))
      {
        kpi.StatusDescription = statusDescription;
        flag = true;
      }
    }
    if (kpiDef.TrendDescription != null)
    {
      string trendDescription = string.IsNullOrEmpty(kpiDef.TrendDescription) ? (string) null : kpiDef.TrendDescription;
      if ((kpi.TrendDescription != trendDescription))
      {
        kpi.TrendDescription = trendDescription;
        flag = true;
      }
    }
    if (kpiDef.Annotations != null)
      flag = AnnotationHelpers.ReplaceAnnotations<Microsoft.AnalysisServices.Tabular.KPI>(kpi, kpiDef.Annotations, (Func<Microsoft.AnalysisServices.Tabular.KPI, ICollection<Microsoft.AnalysisServices.Tabular.Annotation>>) (k => (ICollection<Microsoft.AnalysisServices.Tabular.Annotation>) k.Annotations)) | flag;
    return flag;
  }

  public static string ExportTMDL(string? connectionName, string measureName, ExportTmdl? options)
  {
    Microsoft.AnalysisServices.Tabular.Measure measure = MeasureOperations.FindMeasure(ConnectionOperations.Get(connectionName).Database.Model, measureName);
    if (measure == null)
      throw new ArgumentException($"Measure '{measureName}' not found");
    if (options?.SerializationOptions == null)
      return Microsoft.AnalysisServices.Tabular.TmdlSerializer.SerializeObject((MetadataObject) measure);
    MetadataSerializationOptions serializationOptions = options.SerializationOptions.ToMetadataSerializationOptions();
    return Microsoft.AnalysisServices.Tabular.TmdlSerializer.SerializeObject((MetadataObject) measure, serializationOptions);
  }

  public class MeasureOperationResult
  {
    public string State { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }

    public string MeasureName { get; set; } = string.Empty;

    public string TableName { get; set; } = string.Empty;

    public bool HasChanges { get; set; }
  }

  public class MeasureValidationResult
  {
    public bool IsValid { get; set; }

    public string? ObjectState { get; set; }

    public string? ErrorMessage { get; set; }

    public string Expression { get; set; } = string.Empty;

    public string? Message { get; set; }

    public long ValidationTimeMs { get; set; }
  }
}
