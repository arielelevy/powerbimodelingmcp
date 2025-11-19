// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using Microsoft.AnalysisServices.Tabular;
using ModelContextProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public static class ExtendedPropertyHelpers
{
  public static List<string> Validate(List<ExtendedProperty> properties)
  {
    List<string> stringList = new List<string>();
    HashSet<string> stringSet = new HashSet<string>();
    foreach (ExtendedProperty property in properties)
    {
      if (string.IsNullOrWhiteSpace(property.Name))
      {
        stringList.Add("Extended property name cannot be null or empty");
      }
      else
      {
        if (!stringSet.Add(property.Name))
          stringList.Add("Duplicate extended property name: " + property.Name);
        if (string.IsNullOrWhiteSpace(property.Type))
          stringList.Add("Extended property type cannot be null or empty for property: " + property.Name);
        else if ((property.Type != "String") && (property.Type != "Json") && (property.Type != "System.String") && (property.Type != "System.Json"))
          stringList.Add($"Extended property type must be 'String' or 'Json' for property: {property.Name}. Current type: {property.Type}");
        if (property.Value == null)
          stringList.Add("Extended property value cannot be null for property: " + property.Name);
        if ((property.Type == "Json") || (property.Type == "System.Json"))
        {
          if (!string.IsNullOrEmpty(property.Value))
          {
            try
            {
              JsonDocument.Parse(property.Value);
            }
            catch (JsonException ex)
            {
              stringList.Add("Extended property with type 'Json' must contain well-formed JSON for property: " + property.Name);
            }
          }
        }
      }
    }
    return stringList;
  }

  public static ExtendedProperty? FindByName(List<ExtendedProperty> properties, string name)
  {
    return Enumerable.FirstOrDefault<ExtendedProperty>((IEnumerable<ExtendedProperty>) properties, (p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase)));
  }

  public static void ApplyExtendedProperties<T>(
    T tabularObject,
    List<ExtendedProperty> properties,
    Func<T, ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>> extendedPropertiesCollection)
  {
    ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty> extendedProperties = extendedPropertiesCollection(tabularObject);
    foreach (ExtendedProperty property in properties)
    {
      if ((property.Type == "Json") || (property.Type == "System.Json"))
      {
        if (!string.IsNullOrEmpty(property.Value))
        {
          try
          {
            JsonDocument.Parse(property.Value);
          }
          catch (JsonException ex)
          {
            throw new McpException($"Extended property '{property.Name}' with type 'Json' contains invalid JSON: {((Exception) ex).Message}");
          }
        }
      }
      Microsoft.AnalysisServices.Tabular.ExtendedProperty extendedProperty1;
      if ((property.Type == "Json") || (property.Type == "System.Json"))
      {
        JsonExtendedProperty extendedProperty2 = new JsonExtendedProperty { Name = property.Name };
        extendedProperty2.Value = property.Value;
        extendedProperty1 = (Microsoft.AnalysisServices.Tabular.ExtendedProperty) extendedProperty2;
      }
      else
      {
        StringExtendedProperty extendedProperty3 = new StringExtendedProperty { Name = property.Name };
        extendedProperty3.Value = property.Value;
        extendedProperty1 = (Microsoft.AnalysisServices.Tabular.ExtendedProperty) extendedProperty3;
      }
      extendedProperties.Add(extendedProperty1);
    }
  }

  public static List<ExtendedProperty> ExtractExtendedProperties<T>(
    T tabularObject,
    Func<T, IEnumerable<Microsoft.AnalysisServices.Tabular.ExtendedProperty>> extendedPropertiesCollection)
  {
    List<ExtendedProperty> extendedProperties = new List<ExtendedProperty>();
    foreach (Microsoft.AnalysisServices.Tabular.ExtendedProperty extendedProperty1 in extendedPropertiesCollection(tabularObject))
    {
      if (!extendedProperty1.IsRemoved)
      {
        string str1;
        string str2;
        switch (extendedProperty1)
        {
          case JsonExtendedProperty extendedProperty2:
            str1 = "Json";
            str2 = extendedProperty2.Value ?? string.Empty;
            break;
          case StringExtendedProperty extendedProperty3:
            str1 = "String";
            str2 = extendedProperty3.Value ?? string.Empty;
            break;
          default:
            str1 = "String";
            str2 = extendedProperty1.ToString() ?? string.Empty;
            break;
        }
        extendedProperties.Add(new ExtendedProperty()
        {
          Name = extendedProperty1.Name,
          Value = str2,
          Type = str1
        });
      }
    }
    return extendedProperties;
  }

  public static void ReplaceExtendedProperties<T>(
    T tabularObject,
    List<ExtendedProperty> properties,
    Func<T, ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>> extendedPropertiesCollection)
  {
    ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty> extendedProperties = extendedPropertiesCollection(tabularObject);
    extendedProperties.Clear();
    foreach (ExtendedProperty property in properties)
    {
      if ((property.Type == "Json") || (property.Type == "System.Json"))
      {
        if (!string.IsNullOrEmpty(property.Value))
        {
          try
          {
            JsonDocument.Parse(property.Value);
          }
          catch (JsonException ex)
          {
            throw new McpException($"Extended property '{property.Name}' with type 'Json' contains invalid JSON: {((Exception) ex).Message}");
          }
        }
      }
      Microsoft.AnalysisServices.Tabular.ExtendedProperty extendedProperty1;
      if ((property.Type == "Json") || (property.Type == "System.Json"))
      {
        JsonExtendedProperty extendedProperty2 = new JsonExtendedProperty { Name = property.Name };
        extendedProperty2.Value = property.Value;
        extendedProperty1 = (Microsoft.AnalysisServices.Tabular.ExtendedProperty) extendedProperty2;
      }
      else
      {
        StringExtendedProperty extendedProperty3 = new StringExtendedProperty { Name = property.Name };
        extendedProperty3.Value = property.Value;
        extendedProperty1 = (Microsoft.AnalysisServices.Tabular.ExtendedProperty) extendedProperty3;
      }
      extendedProperties.Add(extendedProperty1);
    }
  }

  public static void ApplyToCulture(Culture culture, List<ExtendedProperty> properties)
  {
    ExtendedPropertyHelpers.ApplyExtendedProperties<Culture>(culture, properties, (Func<Culture, ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>>) (c => (ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>) c.ExtendedProperties));
  }

  public static List<ExtendedProperty> ExtractFromCulture(Culture culture)
  {
    return ExtendedPropertyHelpers.ExtractExtendedProperties<Culture>(culture, (Func<Culture, IEnumerable<Microsoft.AnalysisServices.Tabular.ExtendedProperty>>) (c => (IEnumerable<Microsoft.AnalysisServices.Tabular.ExtendedProperty>) c.ExtendedProperties));
  }

  public static void ReplaceCultureProperties(Culture culture, List<ExtendedProperty> properties)
  {
    ExtendedPropertyHelpers.ReplaceExtendedProperties<Culture>(culture, properties, (Func<Culture, ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>>) (c => (ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>) c.ExtendedProperties));
  }

  public static void ApplyToTable(Table table, List<ExtendedProperty> properties)
  {
    ExtendedPropertyHelpers.ApplyExtendedProperties<Table>(table, properties, (Func<Table, ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>>) (t => (ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>) t.ExtendedProperties));
  }

  public static List<ExtendedProperty> ExtractFromTable(Table table)
  {
    return ExtendedPropertyHelpers.ExtractExtendedProperties<Table>(table, (Func<Table, IEnumerable<Microsoft.AnalysisServices.Tabular.ExtendedProperty>>) (t => (IEnumerable<Microsoft.AnalysisServices.Tabular.ExtendedProperty>) t.ExtendedProperties));
  }

  public static void ReplaceTableProperties(Table table, List<ExtendedProperty> properties)
  {
    ExtendedPropertyHelpers.ReplaceExtendedProperties<Table>(table, properties, (Func<Table, ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>>) (t => (ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>) t.ExtendedProperties));
  }

  public static void ApplyToMeasure(Measure measure, List<ExtendedProperty> properties)
  {
    ExtendedPropertyHelpers.ApplyExtendedProperties<Measure>(measure, properties, (Func<Measure, ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>>) (m => (ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>) m.ExtendedProperties));
  }

  public static List<ExtendedProperty> ExtractFromMeasure(Measure measure)
  {
    return ExtendedPropertyHelpers.ExtractExtendedProperties<Measure>(measure, (Func<Measure, IEnumerable<Microsoft.AnalysisServices.Tabular.ExtendedProperty>>) (m => (IEnumerable<Microsoft.AnalysisServices.Tabular.ExtendedProperty>) m.ExtendedProperties));
  }

  public static void ApplyToColumn(Column column, List<ExtendedProperty> properties)
  {
    ExtendedPropertyHelpers.ApplyExtendedProperties<Column>(column, properties, (Func<Column, ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>>) (c => (ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>) c.ExtendedProperties));
  }

  public static List<ExtendedProperty> ExtractFromColumn(Column column)
  {
    return ExtendedPropertyHelpers.ExtractExtendedProperties<Column>(column, (Func<Column, IEnumerable<Microsoft.AnalysisServices.Tabular.ExtendedProperty>>) (c => (IEnumerable<Microsoft.AnalysisServices.Tabular.ExtendedProperty>) c.ExtendedProperties));
  }

  public static void ApplyToHierarchy(Hierarchy hierarchy, List<ExtendedProperty> properties)
  {
    ExtendedPropertyHelpers.ApplyExtendedProperties<Hierarchy>(hierarchy, properties, (Func<Hierarchy, ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>>) (h => (ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>) h.ExtendedProperties));
  }

  public static List<ExtendedProperty> ExtractFromHierarchy(Hierarchy hierarchy)
  {
    return ExtendedPropertyHelpers.ExtractExtendedProperties<Hierarchy>(hierarchy, (Func<Hierarchy, IEnumerable<Microsoft.AnalysisServices.Tabular.ExtendedProperty>>) (h => (IEnumerable<Microsoft.AnalysisServices.Tabular.ExtendedProperty>) h.ExtendedProperties));
  }

  public static void ReplaceHierarchyProperties(
    Hierarchy hierarchy,
    List<ExtendedProperty> properties)
  {
    ExtendedPropertyHelpers.ReplaceExtendedProperties<Hierarchy>(hierarchy, properties, (Func<Hierarchy, ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>>) (h => (ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>) h.ExtendedProperties));
  }

  public static void ApplyToLevel(Level level, List<ExtendedProperty> properties)
  {
    ExtendedPropertyHelpers.ApplyExtendedProperties<Level>(level, properties, (Func<Level, ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>>) (l => (ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>) l.ExtendedProperties));
  }

  public static List<ExtendedProperty> ExtractFromLevel(Level level)
  {
    return ExtendedPropertyHelpers.ExtractExtendedProperties<Level>(level, (Func<Level, IEnumerable<Microsoft.AnalysisServices.Tabular.ExtendedProperty>>) (l => (IEnumerable<Microsoft.AnalysisServices.Tabular.ExtendedProperty>) l.ExtendedProperties));
  }

  public static void ReplaceLevelProperties(Level level, List<ExtendedProperty> properties)
  {
    ExtendedPropertyHelpers.ReplaceExtendedProperties<Level>(level, properties, (Func<Level, ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>>) (l => (ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>) l.ExtendedProperties));
  }

  public static void ApplyToModel(Model model, List<ExtendedProperty> properties)
  {
    ExtendedPropertyHelpers.ApplyExtendedProperties<Model>(model, properties, (Func<Model, ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>>) (m => (ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>) m.ExtendedProperties));
  }

  public static List<ExtendedProperty> ExtractFromModel(Model model)
  {
    return ExtendedPropertyHelpers.ExtractExtendedProperties<Model>(model, (Func<Model, IEnumerable<Microsoft.AnalysisServices.Tabular.ExtendedProperty>>) (m => (IEnumerable<Microsoft.AnalysisServices.Tabular.ExtendedProperty>) m.ExtendedProperties));
  }

  public static void ReplaceModelProperties(Model model, List<ExtendedProperty> properties)
  {
    ExtendedPropertyHelpers.ReplaceExtendedProperties<Model>(model, properties, (Func<Model, ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>>) (m => (ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>) m.ExtendedProperties));
  }

  public static void ApplyToNamedExpression(
    NamedExpression namedExpression,
    List<ExtendedProperty> properties)
  {
    ExtendedPropertyHelpers.ApplyExtendedProperties<NamedExpression>(namedExpression, properties, (Func<NamedExpression, ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>>) (x => (ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>) x.ExtendedProperties));
  }

  public static List<ExtendedProperty> ExtractFromNamedExpression(NamedExpression namedExpression)
  {
    return ExtendedPropertyHelpers.ExtractExtendedProperties<NamedExpression>(namedExpression, (Func<NamedExpression, IEnumerable<Microsoft.AnalysisServices.Tabular.ExtendedProperty>>) (x => (IEnumerable<Microsoft.AnalysisServices.Tabular.ExtendedProperty>) x.ExtendedProperties));
  }

  public static void ReplaceNamedExpressionProperties(
    NamedExpression namedExpression,
    List<ExtendedProperty> properties)
  {
    ExtendedPropertyHelpers.ReplaceExtendedProperties<NamedExpression>(namedExpression, properties, (Func<NamedExpression, ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>>) (x => (ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>) x.ExtendedProperties));
  }

  public static void ApplyToPartition(Partition partition, List<ExtendedProperty> properties)
  {
    ExtendedPropertyHelpers.ApplyExtendedProperties<Partition>(partition, properties, (Func<Partition, ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>>) (x => (ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>) x.ExtendedProperties));
  }

  public static List<ExtendedProperty> ExtractFromPartition(Partition partition)
  {
    return ExtendedPropertyHelpers.ExtractExtendedProperties<Partition>(partition, (Func<Partition, IEnumerable<Microsoft.AnalysisServices.Tabular.ExtendedProperty>>) (x => (IEnumerable<Microsoft.AnalysisServices.Tabular.ExtendedProperty>) x.ExtendedProperties));
  }

  public static void ReplacePartitionProperties(
    Partition partition,
    List<ExtendedProperty> properties)
  {
    ExtendedPropertyHelpers.ReplaceExtendedProperties<Partition>(partition, properties, (Func<Partition, ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>>) (x => (ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>) x.ExtendedProperties));
  }

  public static void ApplyToRelationship(
    Relationship relationship,
    List<ExtendedProperty> properties)
  {
    ExtendedPropertyHelpers.ApplyExtendedProperties<Relationship>(relationship, properties, (Func<Relationship, ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>>) (r => (ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>) r.ExtendedProperties));
  }

  public static List<ExtendedProperty> ExtractFromRelationship(Relationship relationship)
  {
    return ExtendedPropertyHelpers.ExtractExtendedProperties<Relationship>(relationship, (Func<Relationship, IEnumerable<Microsoft.AnalysisServices.Tabular.ExtendedProperty>>) (r => (IEnumerable<Microsoft.AnalysisServices.Tabular.ExtendedProperty>) r.ExtendedProperties));
  }

  public static void ReplaceRelationshipProperties(
    Relationship relationship,
    List<ExtendedProperty> properties)
  {
    ExtendedPropertyHelpers.ReplaceExtendedProperties<Relationship>(relationship, properties, (Func<Relationship, ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>>) (r => (ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>) r.ExtendedProperties));
  }

  public static void ApplyToModelRole(ModelRole modelRole, List<ExtendedProperty> properties)
  {
    ExtendedPropertyHelpers.ApplyExtendedProperties<ModelRole>(modelRole, properties, (Func<ModelRole, ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>>) (r => (ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>) r.ExtendedProperties));
  }

  public static List<ExtendedProperty> ExtractFromModelRole(ModelRole modelRole)
  {
    return ExtendedPropertyHelpers.ExtractExtendedProperties<ModelRole>(modelRole, (Func<ModelRole, IEnumerable<Microsoft.AnalysisServices.Tabular.ExtendedProperty>>) (r => (IEnumerable<Microsoft.AnalysisServices.Tabular.ExtendedProperty>) r.ExtendedProperties));
  }

  public static void ReplaceModelRoleProperties(
    ModelRole modelRole,
    List<ExtendedProperty> properties)
  {
    ExtendedPropertyHelpers.ReplaceExtendedProperties<ModelRole>(modelRole, properties, (Func<ModelRole, ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>>) (r => (ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>) r.ExtendedProperties));
  }

  public static void ApplyToTablePermission(
    TablePermission tablePermission,
    List<ExtendedProperty> properties)
  {
    ExtendedPropertyHelpers.ApplyExtendedProperties<TablePermission>(tablePermission, properties, (Func<TablePermission, ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>>) (tp => (ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>) tp.ExtendedProperties));
  }

  public static List<ExtendedProperty> ExtractFromTablePermission(TablePermission tablePermission)
  {
    return ExtendedPropertyHelpers.ExtractExtendedProperties<TablePermission>(tablePermission, (Func<TablePermission, IEnumerable<Microsoft.AnalysisServices.Tabular.ExtendedProperty>>) (tp => (IEnumerable<Microsoft.AnalysisServices.Tabular.ExtendedProperty>) tp.ExtendedProperties));
  }

  public static void ReplaceTablePermissionProperties(
    TablePermission tablePermission,
    List<ExtendedProperty> properties)
  {
    ExtendedPropertyHelpers.ReplaceExtendedProperties<TablePermission>(tablePermission, properties, (Func<TablePermission, ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>>) (tp => (ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>) tp.ExtendedProperties));
  }

  public static void ApplyToFunction(Function function, List<ExtendedProperty> properties)
  {
    ExtendedPropertyHelpers.ApplyExtendedProperties<Function>(function, properties, (Func<Function, ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>>) (f => (ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>) f.ExtendedProperties));
  }

  public static List<ExtendedProperty> ExtractFromFunction(Function function)
  {
    return ExtendedPropertyHelpers.ExtractExtendedProperties<Function>(function, (Func<Function, IEnumerable<Microsoft.AnalysisServices.Tabular.ExtendedProperty>>) (f => (IEnumerable<Microsoft.AnalysisServices.Tabular.ExtendedProperty>) f.ExtendedProperties));
  }

  public static void ReplaceFunctionProperties(Function function, List<ExtendedProperty> properties)
  {
    ExtendedPropertyHelpers.ReplaceExtendedProperties<Function>(function, properties, (Func<Function, ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>>) (f => (ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>) f.ExtendedProperties));
  }

  public static void ApplyToBindingInfo(Microsoft.AnalysisServices.Tabular.BindingInfo bindingInfo, List<ExtendedProperty> properties)
  {
    ExtendedPropertyHelpers.ApplyExtendedProperties<Microsoft.AnalysisServices.Tabular.BindingInfo>(bindingInfo, properties, (Func<Microsoft.AnalysisServices.Tabular.BindingInfo, ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>>) (b => (ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>) b.ExtendedProperties));
  }

  public static List<ExtendedProperty> ExtractFromBindingInfo(Microsoft.AnalysisServices.Tabular.BindingInfo bindingInfo)
  {
    return ExtendedPropertyHelpers.ExtractExtendedProperties<Microsoft.AnalysisServices.Tabular.BindingInfo>(bindingInfo, (Func<Microsoft.AnalysisServices.Tabular.BindingInfo, IEnumerable<Microsoft.AnalysisServices.Tabular.ExtendedProperty>>) (b => (IEnumerable<Microsoft.AnalysisServices.Tabular.ExtendedProperty>) b.ExtendedProperties));
  }

  public static void ReplaceBindingInfoProperties(
    Microsoft.AnalysisServices.Tabular.BindingInfo bindingInfo,
    List<ExtendedProperty> properties)
  {
    ExtendedPropertyHelpers.ReplaceExtendedProperties<Microsoft.AnalysisServices.Tabular.BindingInfo>(bindingInfo, properties, (Func<Microsoft.AnalysisServices.Tabular.BindingInfo, ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>>) (b => (ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>) b.ExtendedProperties));
  }

  public static void ApplyToDataSource(DataSource dataSource, List<ExtendedProperty> properties)
  {
    ExtendedPropertyHelpers.ApplyExtendedProperties<DataSource>(dataSource, properties, (Func<DataSource, ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>>) (ds => (ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>) ds.ExtendedProperties));
  }

  public static void ReplaceDataSourceProperties(
    DataSource dataSource,
    List<ExtendedProperty> properties)
  {
    ExtendedPropertyHelpers.ReplaceExtendedProperties<DataSource>(dataSource, properties, (Func<DataSource, ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>>) (ds => (ICollection<Microsoft.AnalysisServices.Tabular.ExtendedProperty>) ds.ExtendedProperties));
  }

  public static List<ExtendedProperty> ExtractFromDataSource(DataSource dataSource)
  {
    return ExtendedPropertyHelpers.ExtractExtendedProperties<DataSource>(dataSource, (Func<DataSource, IEnumerable<Microsoft.AnalysisServices.Tabular.ExtendedProperty>>) (ds => (IEnumerable<Microsoft.AnalysisServices.Tabular.ExtendedProperty>) ds.ExtendedProperties));
  }
}
