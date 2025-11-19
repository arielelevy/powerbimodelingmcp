// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Core.ModelOperations
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
using System.Reflection;

#nullable enable
namespace PowerBIModelingMCP.Library.Core;

public static class ModelOperations
{
  public static bool AddProToolingAnnotation(ConnectionInfo info)
  {
    if (info == null)
      throw new ArgumentNullException(nameof (info));
    Microsoft.AnalysisServices.Tabular.Model model = info.Database.Model;
    string targetValue = "MCP-PBIModeling";
    Microsoft.AnalysisServices.Tabular.Annotation annotation = Enumerable.FirstOrDefault<Microsoft.AnalysisServices.Tabular.Annotation>((IEnumerable<Microsoft.AnalysisServices.Tabular.Annotation>) model.Annotations, (a => (a.Name == "PBI_ProTooling")));
    if (annotation != null)
    {
      List<string> stringList;
      try
      {
        stringList = System.Text.Json.JsonSerializer.Deserialize<List<string>>(annotation.Value) ?? new List<string>();
      }
      catch
      {
        stringList = new List<string>();
      }
      if (Enumerable.Any<string>((IEnumerable<string>) stringList, (v => string.Equals(v, targetValue, StringComparison.OrdinalIgnoreCase))))
        return false;
      stringList.Add(targetValue);
      annotation.Value = System.Text.Json.JsonSerializer.Serialize<List<string>>(stringList);
      return true;
    }
    List<string> stringList1 = new List<string>();
    stringList1.Add(targetValue);
    List<string> stringList2 = stringList1;
    ModelAnnotationCollection annotations = model.Annotations;
    Microsoft.AnalysisServices.Tabular.Annotation metadataObject = new Microsoft.AnalysisServices.Tabular.Annotation();
    metadataObject.Name = "PBI_ProTooling";
    metadataObject.Value = System.Text.Json.JsonSerializer.Serialize<List<string>>(stringList2);
    annotations.Add(metadataObject);
    return true;
  }

  private static void ValidateBase(ModelBase def, bool isCreate)
  {
    if (def == null)
      throw new McpException("Model definition cannot be null");
    if (def.ExtendedProperties != null)
    {
      List<string> stringList = ExtendedPropertyHelpers.Validate(def.ExtendedProperties);
      if (stringList.Count > 0)
        throw new McpException("ExtendedProperties validation failed: " + string.Join(", ", (IEnumerable<string>) stringList));
    }
    AnnotationHelpers.ValidateAnnotations(def.Annotations);
    if (def.BindingInfos == null)
      return;
    ModelOperations.ValidateBindingInfos(def.BindingInfos, (Microsoft.AnalysisServices.Tabular.Model) null);
  }

  private static void ValidateBindingInfos(List<PowerBIModelingMCP.Library.Common.DataStructures.BindingInfo> bindingInfos, Microsoft.AnalysisServices.Tabular.Model? model)
  {
    if (bindingInfos == null)
      return;
    HashSet<string> stringSet = new HashSet<string>();
    foreach (PowerBIModelingMCP.Library.Common.DataStructures.BindingInfo bindingInfo in bindingInfos)
    {
      if (string.IsNullOrWhiteSpace(bindingInfo.Name))
        throw new McpException("BindingInfo Name is required");
      if (!stringSet.Add(bindingInfo.Name))
        throw new McpException("Duplicate BindingInfo name: " + bindingInfo.Name);
      if (string.IsNullOrWhiteSpace(bindingInfo.Type))
        throw new McpException($"BindingInfo Type is required for '{bindingInfo.Name}'");
      if (!(bindingInfo.Type.ToLowerInvariant() == "databindinghint"))
        throw new McpException($"Unsupported BindingInfo type '{bindingInfo.Type}'. Currently supported types: DataBindingHint");
      if (string.IsNullOrWhiteSpace(bindingInfo.ConnectionId))
        throw new McpException($"ConnectionId is required for DataBindingHint '{bindingInfo.Name}'");
      if (model != null)
      {
        string name = bindingInfo.TargetDataSourceReferenceName ?? bindingInfo.Name;
        if (model.DataSources.Find(name) != null)
          ;
      }
      if (bindingInfo.ExtendedProperties != null)
      {
        List<string> stringList = ExtendedPropertyHelpers.Validate(bindingInfo.ExtendedProperties);
        if (stringList.Count > 0)
          throw new McpException($"BindingInfo '{bindingInfo.Name}' ExtendedProperties validation failed: {string.Join(", ", (IEnumerable<string>) stringList)}");
      }
      AnnotationHelpers.ValidateAnnotations(bindingInfo.Annotations, $"BindingInfo '{bindingInfo.Name}'");
    }
  }

  public static ModelGet GetModel(string? connectionName = null)
  {
    Microsoft.AnalysisServices.Tabular.Model model1 = ConnectionOperations.Get(connectionName).Database.Model;
    ModelGet modelGet = new ModelGet { Name = model1.Name };
    modelGet.Description = model1.Description;
    modelGet.StorageLocation = model1.StorageLocation;
    modelGet.DefaultMode = model1.DefaultMode.ToString();
    modelGet.DefaultDataView = model1.DefaultDataView.ToString();
    modelGet.Culture = model1.Culture;
    modelGet.Collation = model1.Collation;
    modelGet.ModifiedTime = new DateTime?(model1.ModifiedTime);
    modelGet.StructureModifiedTime = new DateTime?(model1.StructureModifiedTime);
    modelGet.DataAccessOptions = model1.DataAccessOptions != null ? System.Text.Json.JsonSerializer.Serialize<DataAccessOptions>(model1.DataAccessOptions) : (string) null;
    modelGet.DefaultPowerBIDataSourceVersion = model1.DefaultPowerBIDataSourceVersion.ToString();
    modelGet.ForceUniqueNames = new bool?(model1.ForceUniqueNames);
    modelGet.DiscourageImplicitMeasures = new bool?(model1.DiscourageImplicitMeasures);
    modelGet.DiscourageReportMeasures = new bool?(model1.DiscourageReportMeasures);
    modelGet.DataSourceVariablesOverrideBehavior = model1.DataSourceVariablesOverrideBehavior.ToString();
    modelGet.DataSourceDefaultMaxConnections = new int?(model1.DataSourceDefaultMaxConnections);
    modelGet.SourceQueryCulture = model1.SourceQueryCulture;
    modelGet.MAttributes = model1.MAttributes;
    modelGet.DiscourageCompositeModels = new bool?(model1.DiscourageCompositeModels);
    modelGet.DirectLakeBehavior = model1.DirectLakeBehavior.ToString();
    modelGet.ValueFilterBehavior = model1.ValueFilterBehavior.ToString();
    modelGet.SelectionExpressionBehavior = model1.SelectionExpressionBehavior.ToString();
    ModelGet model2 = modelGet;
    if (model1.DefaultMeasure != null)
    {
      model2.DefaultMeasureTable = model1.DefaultMeasure.Table?.Name;
      model2.DefaultMeasureName = model1.DefaultMeasure.Name;
    }
    if (model1.AutomaticAggregationOptions != null)
    {
      PowerBIModelingMCP.Library.Common.DataStructures.AutomaticAggregationOptions aggregationOptions = new PowerBIModelingMCP.Library.Common.DataStructures.AutomaticAggregationOptions()
      {
        AggregationTableMaxRows = new long?(model1.AutomaticAggregationOptions.AggregationTableMaxRows),
        AggregationTableSizeLimit = new long?(model1.AutomaticAggregationOptions.AggregationTableSizeLimit),
        DetailTableMinRows = new long?(model1.AutomaticAggregationOptions.DetailTableMinRows),
        QueryCoverage = new double?(model1.AutomaticAggregationOptions.QueryCoverage)
      };
      model2.AutomaticAggregationOptions = System.Text.Json.JsonSerializer.Serialize<PowerBIModelingMCP.Library.Common.DataStructures.AutomaticAggregationOptions>(aggregationOptions);
    }
    model2.ExtendedProperties = ExtendedPropertyHelpers.ExtractFromModel(model1);
    model2.Annotations = new List<KeyValuePair<string, string>>();
    foreach (Microsoft.AnalysisServices.Tabular.Annotation annotation in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.Annotation, Microsoft.AnalysisServices.Tabular.Model>) model1.Annotations)
      model2.Annotations.Add(new KeyValuePair<string, string>(annotation.Name, annotation.Value));
    model2.BindingInfos = new List<PowerBIModelingMCP.Library.Common.DataStructures.BindingInfo>();
    foreach (Microsoft.AnalysisServices.Tabular.BindingInfo bindingInfo1 in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.BindingInfo, Microsoft.AnalysisServices.Tabular.Model>) model1.BindingInfoCollection)
    {
      PowerBIModelingMCP.Library.Common.DataStructures.BindingInfo bindingInfo2 = new PowerBIModelingMCP.Library.Common.DataStructures.BindingInfo()
      {
        Name = bindingInfo1.Name,
        Description = bindingInfo1.Description
      };
      if (bindingInfo1 is Microsoft.AnalysisServices.Tabular.DataBindingHint dataBindingHint)
      {
        bindingInfo2.Type = "DataBindingHint";
        bindingInfo2.ConnectionId = dataBindingHint.ConnectionId;
        bindingInfo2.TargetDataSourceReferenceName = bindingInfo1.Name;
      }
      else
        bindingInfo2.Type = ((MemberInfo) bindingInfo1.GetType()).Name;
      if (bindingInfo1.Annotations.Count > 0)
      {
        bindingInfo2.Annotations = new List<KeyValuePair<string, string>>();
        foreach (Microsoft.AnalysisServices.Tabular.Annotation annotation in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.Annotation, Microsoft.AnalysisServices.Tabular.BindingInfo>) bindingInfo1.Annotations)
          bindingInfo2.Annotations.Add(new KeyValuePair<string, string>(annotation.Name, annotation.Value));
      }
      bindingInfo2.ExtendedProperties = ExtendedPropertyHelpers.ExtractFromBindingInfo(bindingInfo1);
      model2.BindingInfos.Add(bindingInfo2);
    }
    return model2;
  }

  public static OperationResult UpdateModel(string? connectionName, ModelUpdate update)
  {
    ModelOperations.ValidateBase((ModelBase) update, false);
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Model model1 = info.Database.Model;
    bool flag = false;
    if (!string.IsNullOrWhiteSpace(update.Name) && (model1.Name != update.Name))
      throw new McpException("Model name changes are not allowed in UpdateModel. Use the RenameModel operation instead.");
    if (update.Description != null)
    {
      string description = string.IsNullOrEmpty(update.Description) ? (string) null : update.Description;
      if ((model1.Description != description))
      {
        model1.Description = description;
        flag = true;
      }
    }
    if (update.StorageLocation != null)
    {
      string storageLocation = string.IsNullOrEmpty(update.StorageLocation) ? (string) null : update.StorageLocation;
      if ((model1.StorageLocation != storageLocation))
      {
        model1.StorageLocation = storageLocation;
        flag = true;
      }
    }
    if (!string.IsNullOrWhiteSpace(update.DefaultMode))
    {
      ModeType modeType;
      if (Enum.TryParse<ModeType>(update.DefaultMode, true, out modeType))
      {
        if (model1.DefaultMode != modeType)
        {
          model1.DefaultMode = modeType;
          flag = true;
        }
      }
      else
      {
        string[] names = Enum.GetNames(typeof (ModeType));
        throw new McpException($"Invalid DefaultMode '{update.DefaultMode}'. Valid values are: {string.Join(", ", names)}");
      }
    }
    if (!string.IsNullOrWhiteSpace(update.DefaultDataView))
    {
      DataViewType dataViewType;
      if (Enum.TryParse<DataViewType>(update.DefaultDataView, true, out dataViewType))
      {
        if (model1.DefaultDataView != dataViewType)
        {
          model1.DefaultDataView = dataViewType;
          flag = true;
        }
      }
      else
      {
        string[] names = Enum.GetNames(typeof (DataViewType));
        throw new McpException($"Invalid DefaultDataView '{update.DefaultDataView}'. Valid values are: {string.Join(", ", names)}");
      }
    }
    if (update.Culture != null)
    {
      string culture = string.IsNullOrEmpty(update.Culture) ? (string) null : update.Culture;
      if ((model1.Culture != culture))
      {
        model1.Culture = culture;
        flag = true;
      }
    }
    if (update.Collation != null)
    {
      string collation = string.IsNullOrEmpty(update.Collation) ? (string) null : update.Collation;
      if ((model1.Collation != collation))
      {
        model1.Collation = collation;
        flag = true;
      }
    }
    if (update.DataAccessOptions != null)
    {
      if (string.IsNullOrEmpty(update.DataAccessOptions))
      {
        if (model1.DataAccessOptions != null)
        {
          model1.DataAccessOptions = (DataAccessOptions) null;
          flag = true;
        }
      }
      else
      {
        try
        {
          DataAccessOptions dataAccessOptions = System.Text.Json.JsonSerializer.Deserialize<DataAccessOptions>(update.DataAccessOptions);
          model1.DataAccessOptions = dataAccessOptions;
          flag = true;
        }
        catch (Exception ex)
        {
          throw new McpException("Invalid DataAccessOptions: " + ex.Message);
        }
      }
    }
    if (update.DefaultMeasureTable != null || update.DefaultMeasureName != null)
    {
      if (string.IsNullOrEmpty(update.DefaultMeasureTable) && string.IsNullOrEmpty(update.DefaultMeasureName))
      {
        if (model1.DefaultMeasure != null)
        {
          model1.DefaultMeasure = (Microsoft.AnalysisServices.Tabular.Measure) null;
          flag = true;
        }
      }
      else
      {
        if (string.IsNullOrWhiteSpace(update.DefaultMeasureTable) || string.IsNullOrWhiteSpace(update.DefaultMeasureName))
          throw new McpException("For DefaultMeasure, both DefaultMeasureTable and DefaultMeasureName must be provided together (both non-empty to set, both empty to clear)");
        Microsoft.AnalysisServices.Tabular.Measure measure = (model1.Tables.Find(update.DefaultMeasureTable) ?? throw new McpException($"Table '{update.DefaultMeasureTable}' not found for DefaultMeasure")).Measures.Find(update.DefaultMeasureName);
        if (measure == null)
          throw new McpException($"Measure '{update.DefaultMeasureName}' not found in table '{update.DefaultMeasureTable}'");
        if (model1.DefaultMeasure != measure)
        {
          model1.DefaultMeasure = measure;
          flag = true;
        }
      }
    }
    if (!string.IsNullOrWhiteSpace(update.DefaultPowerBIDataSourceVersion))
    {
      PowerBIDataSourceVersion dataSourceVersion;
      if (Enum.TryParse<PowerBIDataSourceVersion>(update.DefaultPowerBIDataSourceVersion, true, out dataSourceVersion))
      {
        if (model1.DefaultPowerBIDataSourceVersion != dataSourceVersion)
        {
          model1.DefaultPowerBIDataSourceVersion = dataSourceVersion;
          flag = true;
        }
      }
      else
      {
        string[] names = Enum.GetNames(typeof (PowerBIDataSourceVersion));
        throw new McpException($"Invalid DefaultPowerBIDataSourceVersion '{update.DefaultPowerBIDataSourceVersion}'. Valid values are: {string.Join(", ", names)}");
      }
    }
    bool? nullable1;
    if (update.ForceUniqueNames.HasValue)
    {
      int num1 = model1.ForceUniqueNames ? 1 : 0;
      nullable1 = update.ForceUniqueNames;
      int num2 = nullable1.Value ? 1 : 0;
      if (num1 != num2)
      {
        Microsoft.AnalysisServices.Tabular.Model model2 = model1;
        nullable1 = update.ForceUniqueNames;
        int num3 = nullable1.Value ? 1 : 0;
        model2.ForceUniqueNames = num3 != 0;
        flag = true;
      }
    }
    nullable1 = update.DiscourageImplicitMeasures;
    if (nullable1.HasValue)
    {
      int num4 = model1.DiscourageImplicitMeasures ? 1 : 0;
      nullable1 = update.DiscourageImplicitMeasures;
      int num5 = nullable1.Value ? 1 : 0;
      if (num4 != num5)
      {
        Microsoft.AnalysisServices.Tabular.Model model3 = model1;
        nullable1 = update.DiscourageImplicitMeasures;
        int num6 = nullable1.Value ? 1 : 0;
        model3.DiscourageImplicitMeasures = num6 != 0;
        flag = true;
      }
    }
    nullable1 = update.DiscourageCompositeModels;
    if (nullable1.HasValue)
    {
      int num7 = model1.DiscourageCompositeModels ? 1 : 0;
      nullable1 = update.DiscourageCompositeModels;
      int num8 = nullable1.Value ? 1 : 0;
      if (num7 != num8)
      {
        Microsoft.AnalysisServices.Tabular.Model model4 = model1;
        nullable1 = update.DiscourageCompositeModels;
        int num9 = nullable1.Value ? 1 : 0;
        model4.DiscourageCompositeModels = num9 != 0;
        flag = true;
      }
    }
    if (!string.IsNullOrWhiteSpace(update.DataSourceVariablesOverrideBehavior))
    {
      DataSourceVariablesOverrideBehaviorType overrideBehaviorType;
      if (Enum.TryParse<DataSourceVariablesOverrideBehaviorType>(update.DataSourceVariablesOverrideBehavior, true, out overrideBehaviorType))
      {
        if (model1.DataSourceVariablesOverrideBehavior != overrideBehaviorType)
        {
          model1.DataSourceVariablesOverrideBehavior = overrideBehaviorType;
          flag = true;
        }
      }
      else
      {
        string[] names = Enum.GetNames(typeof (DataSourceVariablesOverrideBehaviorType));
        throw new McpException($"Invalid DataSourceVariablesOverrideBehavior '{update.DataSourceVariablesOverrideBehavior}'. Valid values are: {string.Join(", ", names)}");
      }
    }
    if (update.DataSourceDefaultMaxConnections.HasValue)
    {
      int defaultMaxConnections1 = model1.DataSourceDefaultMaxConnections;
      int? defaultMaxConnections2 = update.DataSourceDefaultMaxConnections;
      int num10 = defaultMaxConnections2.Value;
      if (defaultMaxConnections1 != num10)
      {
        Microsoft.AnalysisServices.Tabular.Model model5 = model1;
        defaultMaxConnections2 = update.DataSourceDefaultMaxConnections;
        int num11 = defaultMaxConnections2.Value;
        model5.DataSourceDefaultMaxConnections = num11;
        flag = true;
      }
    }
    if (update.SourceQueryCulture != null)
    {
      string sourceQueryCulture = string.IsNullOrEmpty(update.SourceQueryCulture) ? (string) null : update.SourceQueryCulture;
      if ((model1.SourceQueryCulture != sourceQueryCulture))
      {
        model1.SourceQueryCulture = sourceQueryCulture;
        flag = true;
      }
    }
    if (update.MAttributes != null)
    {
      string mattributes = string.IsNullOrEmpty(update.MAttributes) ? (string) null : update.MAttributes;
      if ((model1.MAttributes != mattributes))
      {
        model1.MAttributes = mattributes;
        flag = true;
      }
    }
    nullable1 = update.DiscourageReportMeasures;
    if (nullable1.HasValue)
    {
      int num12 = model1.DiscourageReportMeasures ? 1 : 0;
      nullable1 = update.DiscourageReportMeasures;
      int num13 = nullable1.Value ? 1 : 0;
      if (num12 != num13)
      {
        Microsoft.AnalysisServices.Tabular.Model model6 = model1;
        nullable1 = update.DiscourageReportMeasures;
        int num14 = nullable1.Value ? 1 : 0;
        model6.DiscourageReportMeasures = num14 != 0;
        flag = true;
      }
    }
    if (!string.IsNullOrWhiteSpace(update.DirectLakeBehavior))
    {
      DirectLakeBehavior directLakeBehavior;
      if (Enum.TryParse<DirectLakeBehavior>(update.DirectLakeBehavior, true, out directLakeBehavior))
      {
        if (model1.DirectLakeBehavior != directLakeBehavior)
        {
          model1.DirectLakeBehavior = directLakeBehavior;
          flag = true;
        }
      }
      else
      {
        string[] names = Enum.GetNames(typeof (DirectLakeBehavior));
        throw new McpException($"Invalid DirectLakeBehavior '{update.DirectLakeBehavior}'. Valid values are: {string.Join(", ", names)}");
      }
    }
    if (!string.IsNullOrWhiteSpace(update.ValueFilterBehavior))
    {
      ValueFilterBehaviorType filterBehaviorType;
      if (Enum.TryParse<ValueFilterBehaviorType>(update.ValueFilterBehavior, true, out filterBehaviorType))
      {
        if (model1.ValueFilterBehavior != filterBehaviorType)
        {
          model1.ValueFilterBehavior = filterBehaviorType;
          flag = true;
        }
      }
      else
      {
        string[] names = Enum.GetNames(typeof (ValueFilterBehaviorType));
        throw new McpException($"Invalid ValueFilterBehavior '{update.ValueFilterBehavior}'. Valid values are: {string.Join(", ", names)}");
      }
    }
    if (!string.IsNullOrWhiteSpace(update.SelectionExpressionBehavior))
    {
      SelectionExpressionBehaviorType expressionBehaviorType;
      if (Enum.TryParse<SelectionExpressionBehaviorType>(update.SelectionExpressionBehavior, true, out expressionBehaviorType))
      {
        if (model1.SelectionExpressionBehavior != expressionBehaviorType)
        {
          model1.SelectionExpressionBehavior = expressionBehaviorType;
          flag = true;
        }
      }
      else
      {
        string[] names = Enum.GetNames(typeof (SelectionExpressionBehaviorType));
        throw new McpException($"Invalid SelectionExpressionBehavior '{update.SelectionExpressionBehavior}'. Valid values are: {string.Join(", ", names)}");
      }
    }
    if (update.AutomaticAggregationOptions != null)
    {
      if (string.IsNullOrEmpty(update.AutomaticAggregationOptions))
      {
        if (model1.AutomaticAggregationOptions != null)
        {
          model1.AutomaticAggregationOptions = (Microsoft.AnalysisServices.Tabular.AutomaticAggregationOptions) null;
          flag = true;
        }
      }
      else
      {
        try
        {
          PowerBIModelingMCP.Library.Common.DataStructures.AutomaticAggregationOptions aggregationOptions1 = System.Text.Json.JsonSerializer.Deserialize<PowerBIModelingMCP.Library.Common.DataStructures.AutomaticAggregationOptions>(update.AutomaticAggregationOptions);
          if (aggregationOptions1 != null)
          {
            if (model1.AutomaticAggregationOptions == null)
            {
              model1.AutomaticAggregationOptions = new Microsoft.AnalysisServices.Tabular.AutomaticAggregationOptions();
              flag = true;
            }
            long? nullable2;
            if (aggregationOptions1.AggregationTableMaxRows.HasValue)
            {
              long aggregationTableMaxRows = model1.AutomaticAggregationOptions.AggregationTableMaxRows;
              nullable2 = aggregationOptions1.AggregationTableMaxRows;
              long num15 = nullable2.Value;
              if (aggregationTableMaxRows != num15)
              {
                Microsoft.AnalysisServices.Tabular.AutomaticAggregationOptions aggregationOptions2 = model1.AutomaticAggregationOptions;
                nullable2 = aggregationOptions1.AggregationTableMaxRows;
                long num16 = nullable2.Value;
                aggregationOptions2.AggregationTableMaxRows = num16;
                flag = true;
              }
            }
            nullable2 = aggregationOptions1.AggregationTableSizeLimit;
            if (nullable2.HasValue)
            {
              long aggregationTableSizeLimit = model1.AutomaticAggregationOptions.AggregationTableSizeLimit;
              nullable2 = aggregationOptions1.AggregationTableSizeLimit;
              long num17 = nullable2.Value;
              if (aggregationTableSizeLimit != num17)
              {
                Microsoft.AnalysisServices.Tabular.AutomaticAggregationOptions aggregationOptions3 = model1.AutomaticAggregationOptions;
                nullable2 = aggregationOptions1.AggregationTableSizeLimit;
                long num18 = nullable2.Value;
                aggregationOptions3.AggregationTableSizeLimit = num18;
                flag = true;
              }
            }
            nullable2 = aggregationOptions1.DetailTableMinRows;
            if (nullable2.HasValue)
            {
              long detailTableMinRows = model1.AutomaticAggregationOptions.DetailTableMinRows;
              nullable2 = aggregationOptions1.DetailTableMinRows;
              long num19 = nullable2.Value;
              if (detailTableMinRows != num19)
              {
                Microsoft.AnalysisServices.Tabular.AutomaticAggregationOptions aggregationOptions4 = model1.AutomaticAggregationOptions;
                nullable2 = aggregationOptions1.DetailTableMinRows;
                long num20 = nullable2.Value;
                aggregationOptions4.DetailTableMinRows = num20;
                flag = true;
              }
            }
            if (aggregationOptions1.QueryCoverage.HasValue)
            {
              double queryCoverage1 = model1.AutomaticAggregationOptions.QueryCoverage;
              double? queryCoverage2 = aggregationOptions1.QueryCoverage;
              double num21 = queryCoverage2.Value;
              if (queryCoverage1 != num21)
              {
                Microsoft.AnalysisServices.Tabular.AutomaticAggregationOptions aggregationOptions5 = model1.AutomaticAggregationOptions;
                queryCoverage2 = aggregationOptions1.QueryCoverage;
                double num22 = queryCoverage2.Value;
                aggregationOptions5.QueryCoverage = num22;
                flag = true;
              }
            }
          }
        }
        catch (Exception ex)
        {
          throw new McpException("Invalid AutomaticAggregationOptions: " + ex.Message);
        }
      }
    }
    if (update.ExtendedProperties != null)
    {
      int num = model1.ExtendedProperties.Count > 0 ? 1 : 0;
      ExtendedPropertyHelpers.ReplaceExtendedProperties<Microsoft.AnalysisServices.Tabular.Model>(model1, update.ExtendedProperties, (Func<Microsoft.AnalysisServices.Tabular.Model, ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>>) (obj => (ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>) obj.ExtendedProperties));
      if (num != 0 || update.ExtendedProperties.Count > 0)
        flag = true;
    }
    if (update.Annotations != null && AnnotationHelpers.ReplaceAnnotations<Microsoft.AnalysisServices.Tabular.Model>(model1, update.Annotations, (Func<Microsoft.AnalysisServices.Tabular.Model, ICollection<Microsoft.AnalysisServices.Tabular.Annotation>>) (obj => (ICollection<Microsoft.AnalysisServices.Tabular.Annotation>) obj.Annotations)))
      flag = true;
    if (update.BindingInfos != null)
    {
      ModelOperations.ValidateBindingInfos(update.BindingInfos, model1);
      model1.BindingInfoCollection.Clear();
      if (update.BindingInfos.Count > 0)
      {
        foreach (PowerBIModelingMCP.Library.Common.DataStructures.BindingInfo bindingInfo1 in update.BindingInfos)
        {
          if (!(bindingInfo1.Type.ToLowerInvariant() == "databindinghint"))
            throw new McpException($"Unsupported BindingInfo type '{bindingInfo1.Type}'. Currently supported types: DataBindingHint");
          Microsoft.AnalysisServices.Tabular.DataBindingHint dataBindingHint = new Microsoft.AnalysisServices.Tabular.DataBindingHint();
          dataBindingHint.Name = bindingInfo1.Name;
          dataBindingHint.ConnectionId = bindingInfo1.ConnectionId ?? throw new McpException($"ConnectionId is required for DataBindingHint '{bindingInfo1.Name}'");
          Microsoft.AnalysisServices.Tabular.BindingInfo bindingInfo2 = (Microsoft.AnalysisServices.Tabular.BindingInfo) dataBindingHint;
          if (!string.IsNullOrWhiteSpace(bindingInfo1.Description))
            bindingInfo2.Description = bindingInfo1.Description;
          if (bindingInfo1.Annotations != null)
          {
            foreach (KeyValuePair<string, string> annotation in bindingInfo1.Annotations)
            {
              BindingInfoAnnotationCollection annotations = bindingInfo2.Annotations;
              Microsoft.AnalysisServices.Tabular.Annotation metadataObject = new Microsoft.AnalysisServices.Tabular.Annotation();
              metadataObject.Name = annotation.Key;
              metadataObject.Value = annotation.Value;
              annotations.Add(metadataObject);
            }
          }
          if (bindingInfo1.ExtendedProperties != null)
            ExtendedPropertyHelpers.ApplyToBindingInfo(bindingInfo2, bindingInfo1.ExtendedProperties);
          model1.BindingInfoCollection.Add(bindingInfo2);
        }
      }
      flag = true;
    }
    if (!flag)
      return OperationResult.CreateSuccess("Model is already in the requested state", model1.Name, new ObjectType?(ObjectType.Model), new Operation?(Operation.Update), false);
    TransactionOperations.RecordOperation(info, "Updated model properties");
    ConnectionOperations.SaveChangesWithRollback(info, "update model");
    return OperationResult.CreateSuccess($"Model '{model1.Name}' updated successfully", model1.Name, new ObjectType?(ObjectType.Model), new Operation?(Operation.Update));
  }

  public static void RefreshModel(string? connectionName = null, string? refreshType = "Automatic")
  {
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Model model = info.Database.Model;
    RefreshType refreshType1;
    if (string.IsNullOrWhiteSpace(refreshType))
      refreshType1 = RefreshType.Automatic;
    else if (!Enum.TryParse<RefreshType>(refreshType, true, out refreshType1))
    {
      string[] names = Enum.GetNames(typeof (RefreshType));
      throw new McpException($"Invalid refresh type '{refreshType}'. Valid values are: {string.Join(", ", names)}");
    }
    int type = (int) refreshType1;
    model.RequestRefresh((RefreshType) type);
    TransactionOperations.RecordOperation(info, $"Refreshed model with refresh type '{refreshType1}'");
    ConnectionOperations.SaveChangesWithRollback(info, "refresh model");
  }

  public static Dictionary<string, object> GetModelStats(string? connectionName = null)
  {
    Microsoft.AnalysisServices.Tabular.Database database = ConnectionOperations.Get(connectionName).Database;
    Microsoft.AnalysisServices.Tabular.Model model = database?.Model != null ? database.Model : throw new McpException("Model not found in the specified database");
    return new Dictionary<string, object>()
    {
      ["ModelName"] = (object) model.Name,
      ["DatabaseName"] = (object) database.Name,
      ["CompatibilityLevel"] = (object) database.CompatibilityLevel,
      ["TableCount"] = (object) model.Tables.Count,
      ["TotalMeasureCount"] = (object) Enumerable.Sum<Microsoft.AnalysisServices.Tabular.Table>((IEnumerable<Microsoft.AnalysisServices.Tabular.Table>) model.Tables, (t => t.Measures.Count)),
      ["TotalColumnCount"] = (object) Enumerable.Sum<Microsoft.AnalysisServices.Tabular.Table>((IEnumerable<Microsoft.AnalysisServices.Tabular.Table>) model.Tables, (t => t.Columns.Count)),
      ["TotalPartitionCount"] = (object) Enumerable.Sum<Microsoft.AnalysisServices.Tabular.Table>((IEnumerable<Microsoft.AnalysisServices.Tabular.Table>) model.Tables, (t => t.Partitions.Count)),
      ["RelationshipCount"] = (object) model.Relationships.Count,
      ["RoleCount"] = (object) model.Roles.Count,
      ["DataSourceCount"] = (object) model.DataSources.Count,
      ["CultureCount"] = (object) model.Cultures.Count,
      ["PerspectiveCount"] = (object) model.Perspectives.Count,
      ["Tables"] = (object) Enumerable.ToList(Enumerable.Select((IEnumerable<Microsoft.AnalysisServices.Tabular.Table>) model.Tables, t => new
      {
        Name = t.Name,
        ColumnCount = t.Columns.Count,
        MeasureCount = t.Measures.Count,
        PartitionCount = t.Partitions.Count,
        IsHidden = t.IsHidden
      }))
    };
  }

  public static void RenameModel(string? connectionName, string newName)
  {
    if (string.IsNullOrWhiteSpace(newName))
      throw new McpException("New model name cannot be null or empty");
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    string name = info.Database.Model.Name;
    if ((name == newName))
      throw new McpException($"Model is already named '{newName}'");
    TransactionOperations.RecordOperation(info, $"Renamed model from '{name}' to '{newName}'");
    ConnectionOperations.SaveChangesWithRollback(info, "rename model", ConnectionOperations.CheckpointMode.AfterRequestRename);
  }

  public static string ExportTMDL(string? connectionName, ExportTmdl? options)
  {
    Microsoft.AnalysisServices.Tabular.Model model = ConnectionOperations.Get(connectionName).Database.Model;
    if (options?.SerializationOptions == null)
      return TmdlSerializer.SerializeObject((MetadataObject) model);
    MetadataSerializationOptions serializationOptions = options.SerializationOptions.ToMetadataSerializationOptions();
    return TmdlSerializer.SerializeObject((MetadataObject) model, serializationOptions);
  }
}
