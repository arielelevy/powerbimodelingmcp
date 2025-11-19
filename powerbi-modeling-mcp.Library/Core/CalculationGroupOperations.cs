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
using System.Runtime.CompilerServices;

#nullable enable
namespace PowerBIModelingMCP.Library.Core;

public static class CalculationGroupOperations
{
  public static void ValidateCalculationGroupBase(CalculationGroupBase def, bool isCreate)
  {
    if (def == null)
      throw new McpException("Calculation group definition cannot be null");
    if (string.IsNullOrWhiteSpace(def.Name))
      throw new McpException("Name is required");
    if (def.Precedence.HasValue && def.Precedence.Value < 0)
      throw new McpException("Precedence must be non-negative");
    if (def.MultipleOrEmptySelectionExpression != null && isCreate && string.IsNullOrWhiteSpace(def.MultipleOrEmptySelectionExpression.Expression))
      throw new McpException("Expression is required for MultipleOrEmptySelectionExpression");
    if (def.NoSelectionExpression != null && isCreate && string.IsNullOrWhiteSpace(def.NoSelectionExpression.Expression))
      throw new McpException("Expression is required for NoSelectionExpression");
  }

  public static void ValidateCalculationItemBase(CalculationItemBase def, bool isCreate)
  {
    if (def == null)
      throw new McpException("Calculation item definition cannot be null");
    if (string.IsNullOrWhiteSpace(def.Name))
      throw new McpException("Name is required");
    if (isCreate && string.IsNullOrWhiteSpace(def.Expression))
      throw new McpException("Expression is required for creation");
    if (def.Ordinal.HasValue && def.Ordinal.Value < 0)
      throw new McpException("Ordinal must be non-negative");
  }

  private static CalculationGroupExpressionInfo? ConvertToExpressionInfo(
    Microsoft.AnalysisServices.Tabular.CalculationGroupExpression? expr)
  {
    if (expr == null)
      return (CalculationGroupExpressionInfo) null;
    return new CalculationGroupExpressionInfo()
    {
      Expression = expr.Expression,
      Description = expr.Description,
      FormatStringExpression = expr.FormatStringDefinition?.Expression,
      State = expr.State.ToString(),
      ErrorMessage = expr.ErrorMessage,
      ModifiedTime = new DateTime?(expr.ModifiedTime)
    };
  }

  private static void UpdateCalculationGroupExpression(
    Microsoft.AnalysisServices.Tabular.CalculationGroupExpression target,
    CalculationGroupExpressionInfo source)
  {
    if (!string.IsNullOrWhiteSpace(source.Expression))
      target.Expression = source.Expression;
    if (source.Description != null)
      target.Description = source.Description;
    if (source.FormatStringExpression == null)
      return;
    if (string.IsNullOrWhiteSpace(source.FormatStringExpression))
    {
      target.FormatStringDefinition = (Microsoft.AnalysisServices.Tabular.FormatStringDefinition) null;
    }
    else
    {
      if (target.FormatStringDefinition == null)
        target.FormatStringDefinition = new Microsoft.AnalysisServices.Tabular.FormatStringDefinition();
      target.FormatStringDefinition.Expression = source.FormatStringExpression;
    }
  }

  private static Microsoft.AnalysisServices.Tabular.CalculationGroupExpression CreateCalculationGroupExpression(
    CalculationGroupExpressionInfo source)
  {
    Microsoft.AnalysisServices.Tabular.CalculationGroupExpression calculationGroupExpression = new Microsoft.AnalysisServices.Tabular.CalculationGroupExpression()
    {
      Expression = source.Expression
    };
    if (!string.IsNullOrWhiteSpace(source.Description))
      calculationGroupExpression.Description = source.Description;
    if (!string.IsNullOrWhiteSpace(source.FormatStringExpression))
      calculationGroupExpression.FormatStringDefinition = new Microsoft.AnalysisServices.Tabular.FormatStringDefinition()
      {
        Expression = source.FormatStringExpression
      };
    return calculationGroupExpression;
  }

  private static Microsoft.AnalysisServices.Tabular.CalculationGroup FindCalculationGroup(
    Microsoft.AnalysisServices.Tabular.Model model,
    string calculationGroupName)
  {
    foreach (Microsoft.AnalysisServices.Tabular.Table table in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.Table, Microsoft.AnalysisServices.Tabular.Model>) model.Tables)
    {
      if (table.CalculationGroup != null && (table.Name == calculationGroupName))
        return table.CalculationGroup;
    }
    throw new McpException($"Calculation group '{calculationGroupName}' not found in the model");
  }

  private static Microsoft.AnalysisServices.Tabular.Table GetCalculationGroupTable(
    Microsoft.AnalysisServices.Tabular.Model model,
    string calculationGroupName)
  {
    foreach (Microsoft.AnalysisServices.Tabular.Table table in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.Table, Microsoft.AnalysisServices.Tabular.Model>) model.Tables)
    {
      if (table.CalculationGroup != null && (table.Name == calculationGroupName))
        return table;
    }
    throw new McpException($"Table for calculation group '{calculationGroupName}' not found in the model");
  }

  public static List<CalculationGroupList> ListCalculationGroups(string? connectionName = null)
  {
    Microsoft.AnalysisServices.Tabular.Database database = ConnectionOperations.Get(connectionName).Database;
    List<CalculationGroupList> calculationGroupListList1 = new List<CalculationGroupList>();
    foreach (Microsoft.AnalysisServices.Tabular.Table table in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.Table, Microsoft.AnalysisServices.Tabular.Model>) database.Model.Tables)
    {
      if (table.CalculationGroup != null)
      {
        List<CalculationGroupList> calculationGroupListList2 = calculationGroupListList1;
        CalculationGroupList calculationGroupList = new CalculationGroupList { Name = table.Name };
        calculationGroupList.Description = !string.IsNullOrEmpty(table.Description) ? table.Description : (string) null;
        calculationGroupList.CalculationItems = Enumerable.ToList<CalculationItemList>(Enumerable.Select<Microsoft.AnalysisServices.Tabular.CalculationItem, CalculationItemList>(Enumerable.OrderBy<Microsoft.AnalysisServices.Tabular.CalculationItem, int>((IEnumerable<Microsoft.AnalysisServices.Tabular.CalculationItem>) table.CalculationGroup.CalculationItems, (item => item.Ordinal)), (item =>
        {
          return new CalculationItemList()
          {
            Name = item.Name,
            Description = !string.IsNullOrEmpty(item.Description) ? item.Description : (string) null,
            Ordinal = item.Ordinal
          };
        })));
        calculationGroupListList2.Add(calculationGroupList);
      }
    }
    return calculationGroupListList1;
  }

  public static CalculationGroupGet GetCalculationGroup(
    string? connectionName,
    string calculationGroupName)
  {
    if (string.IsNullOrWhiteSpace(calculationGroupName))
      throw new McpException("calculationGroupName is required");
    Microsoft.AnalysisServices.Tabular.Database database = ConnectionOperations.Get(connectionName).Database;
    Microsoft.AnalysisServices.Tabular.CalculationGroup calculationGroup1 = CalculationGroupOperations.FindCalculationGroup(database.Model, calculationGroupName);
    Microsoft.AnalysisServices.Tabular.Table calculationGroupTable = CalculationGroupOperations.GetCalculationGroupTable(database.Model, calculationGroupName);
    CalculationGroupGet calculationGroupGet = new CalculationGroupGet { Name = calculationGroupTable.Name };
    calculationGroupGet.Description = calculationGroup1.Description;
    calculationGroupGet.IsHidden = new bool?(calculationGroupTable.IsHidden);
    calculationGroupGet.Precedence = new int?(calculationGroup1.Precedence);
    calculationGroupGet.MultipleOrEmptySelectionExpression = CalculationGroupOperations.ConvertToExpressionInfo(calculationGroup1.MultipleOrEmptySelectionExpression);
    calculationGroupGet.NoSelectionExpression = CalculationGroupOperations.ConvertToExpressionInfo(calculationGroup1.NoSelectionExpression);
    calculationGroupGet.ModifiedTime = new DateTime?(calculationGroupTable.ModifiedTime);
    calculationGroupGet.StructureModifiedTime = new DateTime?(calculationGroupTable.StructureModifiedTime);
    CalculationGroupGet calculationGroup2 = calculationGroupGet;
    foreach (Microsoft.AnalysisServices.Tabular.CalculationItem calculationItem in Enumerable.OrderBy<Microsoft.AnalysisServices.Tabular.CalculationItem, int>((IEnumerable<Microsoft.AnalysisServices.Tabular.CalculationItem>) calculationGroup1.CalculationItems, (i => i.Ordinal)))
    {
      CalculationItemGet calculationItemGet1 = new CalculationItemGet { Name = calculationItem.Name };
      calculationItemGet1.Description = calculationItem.Description;
      calculationItemGet1.Expression = calculationItem.Expression;
      calculationItemGet1.Ordinal = new int?(calculationItem.Ordinal);
      calculationItemGet1.FormatStringExpression = calculationItem.FormatStringDefinition?.Expression;
      calculationItemGet1.State = calculationItem.State.ToString();
      calculationItemGet1.ErrorMessage = calculationItem.ErrorMessage;
      calculationItemGet1.ModifiedTime = new DateTime?(calculationItem.ModifiedTime);
      CalculationItemGet calculationItemGet2 = calculationItemGet1;
      calculationGroup2.CalculationItems.Add(calculationItemGet2);
    }
    calculationGroup2.Annotations = new List<KeyValuePair<string, string>>();
    foreach (Microsoft.AnalysisServices.Tabular.Annotation annotation in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.Annotation, Microsoft.AnalysisServices.Tabular.CalculationGroup>) calculationGroup1.Annotations)
      calculationGroup2.Annotations.Add(new KeyValuePair<string, string>(annotation.Name, annotation.Value));
    return calculationGroup2;
  }

  public static string ExportTMDL(
    string? connectionName,
    string calculationGroupName,
    ExportTmdl? options)
  {
    if (string.IsNullOrWhiteSpace(calculationGroupName))
      throw new McpException("calculationGroupName is required");
    Microsoft.AnalysisServices.Tabular.CalculationGroup calculationGroup = CalculationGroupOperations.FindCalculationGroup(ConnectionOperations.Get(connectionName).Database.Model, calculationGroupName);
    if (calculationGroup == null)
      throw new McpException($"Calculation group '{calculationGroupName}' not found");
    if (options?.SerializationOptions == null)
      return TmdlSerializer.SerializeObject((MetadataObject) calculationGroup);
    MetadataSerializationOptions serializationOptions = options.SerializationOptions.ToMetadataSerializationOptions();
    return TmdlSerializer.SerializeObject((MetadataObject) calculationGroup, serializationOptions);
  }

  public static CalculationGroupOperationResult CreateCalculationGroup(
    string? connectionName,
    CalculationGroupCreate def)
  {
    CalculationGroupOperations.ValidateCalculationGroupBase((CalculationGroupBase) def, true);
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Database database = info.Database;
    if (!database.Model.DiscourageImplicitMeasures)
    {
      database.Model.DiscourageImplicitMeasures = true;
      TransactionOperations.RecordOperation(info, "Set Model.DiscourageImplicitMeasures to true (required for calculation groups)");
    }
    foreach (Microsoft.AnalysisServices.Tabular.Table table in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.Table, Microsoft.AnalysisServices.Tabular.Model>) database.Model.Tables)
    {
      if (table.CalculationGroup != null && (table.Name == def.Name))
        throw new McpException($"Calculation group '{def.Name}' already exists");
    }
    if (def.CalculationItems != null && def.CalculationItems.Count > 0)
    {
      int num = Enumerable.Any<CalculationItemCreate>((IEnumerable<CalculationItemCreate>) def.CalculationItems, (item => item.Ordinal.HasValue)) ? 1 : 0;
      bool flag = Enumerable.All<CalculationItemCreate>((IEnumerable<CalculationItemCreate>) def.CalculationItems, (item => item.Ordinal.HasValue));
      if (num != 0 && !flag)
        throw new McpException("Either all calculation items must have ordinals specified or none. Mixed ordinals are not allowed.");
      foreach (CalculationItemBase calculationItem in def.CalculationItems)
        CalculationGroupOperations.ValidateCalculationItemBase(calculationItem, true);
      if (flag)
      {
        HashSet<int> intSet = new HashSet<int>();
        foreach (CalculationItemCreate calculationItem in def.CalculationItems)
        {
          if (!intSet.Add(calculationItem.Ordinal.Value))
            throw new McpException($"Duplicate ordinal {calculationItem.Ordinal.Value} found");
        }
        List<int> list = Enumerable.ToList<int>(Enumerable.OrderBy<int, int>((IEnumerable<int>) intSet, (o => o)));
        for (int index = 0; index < list.Count; ++index)
        {
          if (list[index] != index)
            throw new McpException($"Calculation item ordinals must be continuous starting from 0. Missing ordinal {index}");
        }
      }
      HashSet<string> stringSet = new HashSet<string>();
      foreach (CalculationItemCreate calculationItem in def.CalculationItems)
      {
        if (!stringSet.Add(calculationItem.Name))
          throw new McpException($"Duplicate calculation item name '{calculationItem.Name}' found");
      }
    }
    Microsoft.AnalysisServices.Tabular.CalculationGroup target = new Microsoft.AnalysisServices.Tabular.CalculationGroup();
    if (!string.IsNullOrWhiteSpace(def.Description))
      target.Description = def.Description;
    if (def.Precedence.HasValue)
      target.Precedence = def.Precedence.Value;
    if (def.MultipleOrEmptySelectionExpression != null)
      target.MultipleOrEmptySelectionExpression = CalculationGroupOperations.CreateCalculationGroupExpression(def.MultipleOrEmptySelectionExpression);
    if (def.NoSelectionExpression != null)
      target.NoSelectionExpression = CalculationGroupOperations.CreateCalculationGroupExpression(def.NoSelectionExpression);
    if (def.Annotations != null)
      AnnotationHelpers.ApplyAnnotations<Microsoft.AnalysisServices.Tabular.CalculationGroup>(target, def.Annotations, (Func<Microsoft.AnalysisServices.Tabular.CalculationGroup, ICollection<Microsoft.AnalysisServices.Tabular.Annotation>>) (cg => (ICollection<Microsoft.AnalysisServices.Tabular.Annotation>) cg.Annotations));
    if (def.CalculationItems != null && def.CalculationItems.Count > 0)
    {
      List<CalculationItemCreate> list = Enumerable.ToList<CalculationItemCreate>((IEnumerable<CalculationItemCreate>) def.CalculationItems);
      if (Enumerable.All<CalculationItemCreate>((IEnumerable<CalculationItemCreate>) list, (item => !item.Ordinal.HasValue)))
      {
        for (int index = 0; index < list.Count; ++index)
          list[index].Ordinal = new int?(index);
      }
      foreach (CalculationItemCreate calculationItemCreate in Enumerable.OrderBy<CalculationItemCreate, int?>((IEnumerable<CalculationItemCreate>) list, (item => item.Ordinal)))
      {
        Microsoft.AnalysisServices.Tabular.CalculationItem calculationItem = new Microsoft.AnalysisServices.Tabular.CalculationItem();
        calculationItem.Name = calculationItemCreate.Name;
        calculationItem.Expression = calculationItemCreate.Expression;
        calculationItem.Ordinal = calculationItemCreate.Ordinal.Value;
        Microsoft.AnalysisServices.Tabular.CalculationItem metadataObject = calculationItem;
        if (!string.IsNullOrWhiteSpace(calculationItemCreate.Description))
          metadataObject.Description = calculationItemCreate.Description;
        if (!string.IsNullOrWhiteSpace(calculationItemCreate.FormatStringExpression))
          metadataObject.FormatStringDefinition = new Microsoft.AnalysisServices.Tabular.FormatStringDefinition()
          {
            Expression = calculationItemCreate.FormatStringExpression
          };
        target.CalculationItems.Add(metadataObject);
      }
    }
    Microsoft.AnalysisServices.Tabular.Table table1 = new Microsoft.AnalysisServices.Tabular.Table();
    table1.Name = def.Name;
    Microsoft.AnalysisServices.Tabular.Table metadataObject1 = table1;
    bool? isHidden = def.IsHidden;
    if (isHidden.HasValue)
    {
      Microsoft.AnalysisServices.Tabular.Table table2 = metadataObject1;
      isHidden = def.IsHidden;
      int num = isHidden.Value ? 1 : 0;
      table2.IsHidden = num != 0;
    }
    Microsoft.AnalysisServices.Tabular.DataColumn dataColumn = new Microsoft.AnalysisServices.Tabular.DataColumn();
    dataColumn.Name = def.Name;
    dataColumn.DataType = DataType.String;
    Microsoft.AnalysisServices.Tabular.DataColumn metadataObject2 = dataColumn;
    metadataObject1.Columns.Add((Microsoft.AnalysisServices.Tabular.Column) metadataObject2);
    metadataObject1.CalculationGroup = target;
    Microsoft.AnalysisServices.Tabular.Partition partition = new Microsoft.AnalysisServices.Tabular.Partition();
    partition.Name = "Partition_" + def.Name;
    Microsoft.AnalysisServices.Tabular.Partition metadataObject3 = partition;
    metadataObject3.Source = (Microsoft.AnalysisServices.Tabular.PartitionSource) new CalculationGroupSource();
    metadataObject1.Partitions.Add(metadataObject3);
    database.Model.Tables.Add(metadataObject1);
    List<CalculationItemCreate> calculationItems = def.CalculationItems;
    int count = calculationItems != null ? calculationItems.Count : 0;
    TransactionOperations.RecordOperation(info, $"Created calculation group '{def.Name}' with {count} calculation items");
    ConnectionOperations.SaveChangesWithRollback(info, "create calculation group");
    CalculationGroupOperationResult calculationGroup = new CalculationGroupOperationResult()
    {
      CalculationGroupName = def.Name,
      CalculationItemCount = target.CalculationItems.Count
    };
    foreach (Microsoft.AnalysisServices.Tabular.CalculationItem calculationItem in Enumerable.OrderBy<Microsoft.AnalysisServices.Tabular.CalculationItem, int>((IEnumerable<Microsoft.AnalysisServices.Tabular.CalculationItem>) target.CalculationItems, (i => i.Ordinal)))
      calculationGroup.CalculationItems.Add(new CalculationItemOperationResult()
      {
        State = calculationItem.State.ToString(),
        ErrorMessage = calculationItem.ErrorMessage,
        CalculationItemName = calculationItem.Name,
        CalculationGroupName = def.Name,
        Ordinal = calculationItem.Ordinal
      });
    return calculationGroup;
  }

  public static CalculationGroupOperationResult UpdateCalculationGroup(
    string? connectionName,
    CalculationGroupUpdate update)
  {
    CalculationGroupOperations.ValidateCalculationGroupBase((CalculationGroupBase) update, false);
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Database database = info.Database;
    Microsoft.AnalysisServices.Tabular.CalculationGroup calculationGroup1 = CalculationGroupOperations.FindCalculationGroup(database.Model, update.Name);
    Microsoft.AnalysisServices.Tabular.Table calculationGroupTable = CalculationGroupOperations.GetCalculationGroupTable(database.Model, update.Name);
    bool flag = false;
    if (update.Description != null && (calculationGroup1.Description != update.Description))
    {
      calculationGroup1.Description = update.Description;
      flag = true;
    }
    if (update.IsHidden.HasValue)
    {
      int num1 = calculationGroupTable.IsHidden ? 1 : 0;
      bool? isHidden = update.IsHidden;
      int num2 = isHidden.Value ? 1 : 0;
      if (num1 != num2)
      {
        Microsoft.AnalysisServices.Tabular.Table table = calculationGroupTable;
        isHidden = update.IsHidden;
        int num3 = isHidden.Value ? 1 : 0;
        table.IsHidden = num3 != 0;
        flag = true;
      }
    }
    if (update.Precedence.HasValue)
    {
      int precedence1 = calculationGroup1.Precedence;
      int? precedence2 = update.Precedence;
      int num4 = precedence2.Value;
      if (precedence1 != num4)
      {
        Microsoft.AnalysisServices.Tabular.CalculationGroup calculationGroup2 = calculationGroup1;
        precedence2 = update.Precedence;
        int num5 = precedence2.Value;
        calculationGroup2.Precedence = num5;
        flag = true;
      }
    }
    if (update.MultipleOrEmptySelectionExpression != null)
    {
      if (calculationGroup1.MultipleOrEmptySelectionExpression == null)
      {
        calculationGroup1.MultipleOrEmptySelectionExpression = CalculationGroupOperations.CreateCalculationGroupExpression(update.MultipleOrEmptySelectionExpression);
        flag = true;
      }
      else
      {
        string str1 = calculationGroup1.MultipleOrEmptySelectionExpression.Expression ?? "";
        string str2 = calculationGroup1.MultipleOrEmptySelectionExpression.Description ?? "";
        string str3 = calculationGroup1.MultipleOrEmptySelectionExpression.FormatStringDefinition?.Expression ?? "";
        string str4 = update.MultipleOrEmptySelectionExpression.Expression ?? "";
        string str5 = update.MultipleOrEmptySelectionExpression.Description ?? "";
        string str6 = update.MultipleOrEmptySelectionExpression.FormatStringExpression ?? "";
        if ((str1 != str4) || (str2 != str5) || (str3 != str6))
        {
          CalculationGroupOperations.UpdateCalculationGroupExpression(calculationGroup1.MultipleOrEmptySelectionExpression, update.MultipleOrEmptySelectionExpression);
          flag = true;
        }
      }
    }
    if (update.NoSelectionExpression != null)
    {
      if (calculationGroup1.NoSelectionExpression == null)
      {
        calculationGroup1.NoSelectionExpression = CalculationGroupOperations.CreateCalculationGroupExpression(update.NoSelectionExpression);
        flag = true;
      }
      else
      {
        string str7 = calculationGroup1.NoSelectionExpression.Expression ?? "";
        string str8 = calculationGroup1.NoSelectionExpression.Description ?? "";
        string str9 = calculationGroup1.NoSelectionExpression.FormatStringDefinition?.Expression ?? "";
        string str10 = update.NoSelectionExpression.Expression ?? "";
        string str11 = update.NoSelectionExpression.Description ?? "";
        string str12 = update.NoSelectionExpression.FormatStringExpression ?? "";
        if ((str7 != str10) || (str8 != str11) || (str9 != str12))
        {
          CalculationGroupOperations.UpdateCalculationGroupExpression(calculationGroup1.NoSelectionExpression, update.NoSelectionExpression);
          flag = true;
        }
      }
    }
    if (update.Annotations != null && AnnotationHelpers.ReplaceAnnotations<Microsoft.AnalysisServices.Tabular.CalculationGroup>(calculationGroup1, update.Annotations, (Func<Microsoft.AnalysisServices.Tabular.CalculationGroup, ICollection<Microsoft.AnalysisServices.Tabular.Annotation>>) (cg => (ICollection<Microsoft.AnalysisServices.Tabular.Annotation>) cg.Annotations)))
      flag = true;
    if (!flag)
    {
      CalculationGroupOperationResult groupOperationResult = new CalculationGroupOperationResult()
      {
        CalculationGroupName = update.Name,
        CalculationItemCount = calculationGroup1.CalculationItems.Count,
        HasChanges = false
      };
      foreach (Microsoft.AnalysisServices.Tabular.CalculationItem calculationItem in Enumerable.OrderBy<Microsoft.AnalysisServices.Tabular.CalculationItem, int>((IEnumerable<Microsoft.AnalysisServices.Tabular.CalculationItem>) calculationGroup1.CalculationItems, (i => i.Ordinal)))
        groupOperationResult.CalculationItems.Add(new CalculationItemOperationResult()
        {
          State = calculationItem.State.ToString(),
          ErrorMessage = calculationItem.ErrorMessage,
          CalculationItemName = calculationItem.Name,
          CalculationGroupName = update.Name,
          Ordinal = calculationItem.Ordinal,
          HasChanges = false
        });
      return groupOperationResult;
    }
    TransactionOperations.RecordOperation(info, $"Updated calculation group '{update.Name}'");
    ConnectionOperations.SaveChangesWithRollback(info, "update calculation group");
    CalculationGroupOperationResult groupOperationResult1 = new CalculationGroupOperationResult()
    {
      CalculationGroupName = update.Name,
      CalculationItemCount = calculationGroup1.CalculationItems.Count,
      HasChanges = true
    };
    foreach (Microsoft.AnalysisServices.Tabular.CalculationItem calculationItem in Enumerable.OrderBy<Microsoft.AnalysisServices.Tabular.CalculationItem, int>((IEnumerable<Microsoft.AnalysisServices.Tabular.CalculationItem>) calculationGroup1.CalculationItems, (i => i.Ordinal)))
      groupOperationResult1.CalculationItems.Add(new CalculationItemOperationResult()
      {
        State = calculationItem.State.ToString(),
        ErrorMessage = calculationItem.ErrorMessage,
        CalculationItemName = calculationItem.Name,
        CalculationGroupName = update.Name,
        Ordinal = calculationItem.Ordinal,
        HasChanges = true
      });
    return groupOperationResult1;
  }

  public static void RenameCalculationGroup(string? connectionName, string oldName, string newName)
  {
    if (string.IsNullOrWhiteSpace(oldName))
      throw new McpException("oldName is required");
    if (string.IsNullOrWhiteSpace(newName))
      throw new McpException("newName is required");
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Database database = info.Database;
    Microsoft.AnalysisServices.Tabular.Table calculationGroupTable = CalculationGroupOperations.GetCalculationGroupTable(database.Model, oldName);
    foreach (Microsoft.AnalysisServices.Tabular.Table table in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.Table, Microsoft.AnalysisServices.Tabular.Model>) database.Model.Tables)
    {
      if (table.CalculationGroup != null && (table.Name == newName) && !string.Equals(oldName, newName, StringComparison.OrdinalIgnoreCase))
        throw new McpException($"Calculation group '{newName}' already exists");
    }
    calculationGroupTable.RequestRename(newName);
    TransactionOperations.RecordOperation(info, $"Renamed calculation group '{oldName}' to '{newName}'");
    ConnectionOperations.SaveChangesWithRollback(info, "rename calculation group", ConnectionOperations.CheckpointMode.AfterRequestRename);
  }

  public static void DeleteCalculationGroup(string? connectionName, string calculationGroupName)
  {
    if (string.IsNullOrWhiteSpace(calculationGroupName))
      throw new McpException("calculationGroupName is required");
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Database database = info.Database;
    Microsoft.AnalysisServices.Tabular.Table calculationGroupTable = CalculationGroupOperations.GetCalculationGroupTable(database.Model, calculationGroupName);
    List<string> stringList = new List<string>();
    foreach (Microsoft.AnalysisServices.Tabular.Table table in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.Table, Microsoft.AnalysisServices.Tabular.Model>) database.Model.Tables)
    {
      foreach (Microsoft.AnalysisServices.Tabular.Measure measure in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.Measure, Microsoft.AnalysisServices.Tabular.Table>) table.Measures)
      {
        if (!string.IsNullOrWhiteSpace(measure.Expression) && measure.Expression.Contains($"'{calculationGroupName}'"))
          stringList.Add($"[{measure.Name}]");
      }
    }
    if (stringList.Count > 0)
      throw new McpException($"Cannot delete calculation group '{calculationGroupName}' because it is referenced by: {string.Join(", ", (IEnumerable<string>) stringList)}");
    database.Model.Tables.Remove(calculationGroupTable);
    TransactionOperations.RecordOperation(info, $"Deleted calculation group '{calculationGroupName}'");
    ConnectionOperations.SaveChangesWithRollback(info, "delete calculation group");
  }

  public static List<CalculationItemList> ListCalculationItems(
    string? connectionName,
    string calculationGroupName)
  {
    if (string.IsNullOrWhiteSpace(calculationGroupName))
      throw new McpException("calculationGroupName is required");
    return Enumerable.ToList<CalculationItemList>(Enumerable.Select<Microsoft.AnalysisServices.Tabular.CalculationItem, CalculationItemList>(Enumerable.OrderBy<Microsoft.AnalysisServices.Tabular.CalculationItem, int>((IEnumerable<Microsoft.AnalysisServices.Tabular.CalculationItem>) CalculationGroupOperations.FindCalculationGroup(ConnectionOperations.Get(connectionName).Database.Model, calculationGroupName).CalculationItems, (item => item.Ordinal)), (item =>
    {
      return new CalculationItemList()
      {
        Name = item.Name,
        Description = !string.IsNullOrEmpty(item.Description) ? item.Description : (string) null,
        Ordinal = item.Ordinal
      };
    })));
  }

  public static CalculationItemGet GetCalculationItem(
    string? connectionName,
    string calculationGroupName,
    string calculationItemName)
  {
    if (string.IsNullOrWhiteSpace(calculationGroupName))
      throw new McpException("calculationGroupName is required");
    if (string.IsNullOrWhiteSpace(calculationItemName))
      throw new McpException("calculationItemName is required");
    Microsoft.AnalysisServices.Tabular.CalculationItem calculationItem1 = CalculationGroupOperations.FindCalculationGroup(ConnectionOperations.Get(connectionName).Database.Model, calculationGroupName).CalculationItems.Find(calculationItemName) ?? throw new McpException($"Calculation item '{calculationItemName}' not found in calculation group '{calculationGroupName}'");
    CalculationItemGet calculationItem2 = new CalculationItemGet { Name = calculationItem1.Name };
    calculationItem2.Description = calculationItem1.Description;
    calculationItem2.Expression = calculationItem1.Expression;
    calculationItem2.Ordinal = new int?(calculationItem1.Ordinal);
    calculationItem2.FormatStringExpression = calculationItem1.FormatStringDefinition?.Expression;
    calculationItem2.State = calculationItem1.State.ToString();
    calculationItem2.ErrorMessage = calculationItem1.ErrorMessage;
    calculationItem2.ModifiedTime = new DateTime?(calculationItem1.ModifiedTime);
    return calculationItem2;
  }

  public static CalculationItemOperationResult CreateCalculationItem(
    string? connectionName,
    string calculationGroupName,
    CalculationItemCreate def)
  {
    if (string.IsNullOrWhiteSpace(calculationGroupName))
      throw new McpException("calculationGroupName is required");
    CalculationGroupOperations.ValidateCalculationItemBase((CalculationItemBase) def, true);
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.CalculationGroup calculationGroup = CalculationGroupOperations.FindCalculationGroup(info.Database.Model, calculationGroupName);
    if (calculationGroup.CalculationItems.Contains(def.Name))
      throw new McpException($"Calculation item '{def.Name}' already exists in calculation group '{calculationGroupName}'");
    int ordinal;
    if (def.Ordinal.HasValue)
    {
      ordinal = def.Ordinal.Value;
      if (Enumerable.Any<Microsoft.AnalysisServices.Tabular.CalculationItem>((IEnumerable<Microsoft.AnalysisServices.Tabular.CalculationItem>) calculationGroup.CalculationItems, (item => item.Ordinal == ordinal)))
        throw new McpException($"Calculation item with ordinal {ordinal} already exists in calculation group '{calculationGroupName}'");
    }
    else
      ordinal = calculationGroup.CalculationItems.Count > 0 ? Enumerable.Max<Microsoft.AnalysisServices.Tabular.CalculationItem>((IEnumerable<Microsoft.AnalysisServices.Tabular.CalculationItem>) calculationGroup.CalculationItems, (item => item.Ordinal)) + 1 : 0;
    Microsoft.AnalysisServices.Tabular.CalculationItem calculationItem = new Microsoft.AnalysisServices.Tabular.CalculationItem();
    calculationItem.Name = def.Name;
    calculationItem.Expression = def.Expression;
    calculationItem.Ordinal = ordinal;
    Microsoft.AnalysisServices.Tabular.CalculationItem metadataObject = calculationItem;
    if (!string.IsNullOrWhiteSpace(def.Description))
      metadataObject.Description = def.Description;
    if (!string.IsNullOrWhiteSpace(def.FormatStringExpression))
      metadataObject.FormatStringDefinition = new Microsoft.AnalysisServices.Tabular.FormatStringDefinition()
      {
        Expression = def.FormatStringExpression
      };
    calculationGroup.CalculationItems.Add(metadataObject);
    TransactionOperations.RecordOperation(info, $"Created calculation item '{def.Name}' in calculation group '{calculationGroupName}'");
    ConnectionOperations.SaveChangesWithRollback(info, "create calculation item");
    return new CalculationItemOperationResult()
    {
      State = metadataObject.State.ToString(),
      ErrorMessage = metadataObject.ErrorMessage,
      CalculationItemName = metadataObject.Name,
      CalculationGroupName = calculationGroupName,
      Ordinal = metadataObject.Ordinal
    };
  }

  public static CalculationItemOperationResult UpdateCalculationItem(
    string? connectionName,
    string calculationGroupName,
    string calculationItemName,
    CalculationItemUpdate update)
  {
    if (string.IsNullOrWhiteSpace(calculationGroupName))
      throw new McpException("calculationGroupName is required");
    if (string.IsNullOrWhiteSpace(calculationItemName))
      throw new McpException("calculationItemName is required");
    CalculationGroupOperations.ValidateCalculationItemBase((CalculationItemBase) update, false);
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.CalculationGroup calculationGroup = CalculationGroupOperations.FindCalculationGroup(info.Database.Model, calculationGroupName);
    Microsoft.AnalysisServices.Tabular.CalculationItem calculationItem = calculationGroup.CalculationItems.Find(calculationItemName) ?? throw new McpException($"Calculation item '{calculationItemName}' not found in calculation group '{calculationGroupName}'");
    bool flag = false;
    if (update.Name != null && (calculationItem.Name != update.Name))
      throw new McpException($"Cannot change calculation item name from '{calculationItem.Name}' to '{update.Name}'. Use the rename operation to change calculation item names.");
    if (update.Description != null)
    {
      string description = string.IsNullOrEmpty(update.Description) ? (string) null : update.Description;
      if ((calculationItem.Description != description))
      {
        calculationItem.Description = description;
        flag = true;
      }
    }
    if (update.Expression != null)
    {
      if (string.IsNullOrEmpty(update.Expression))
        throw new McpException("Expression cannot be empty");
      if ((calculationItem.Expression != update.Expression))
      {
        calculationItem.Expression = update.Expression;
        flag = true;
      }
    }
    int? ordinal1 = update.Ordinal;
    if (ordinal1.HasValue)
    {
      int ordinal2 = calculationItem.Ordinal;
      ordinal1 = update.Ordinal;
      int num1 = ordinal1.Value;
      if (ordinal2 != num1)
      {
        if (Enumerable.Any<Microsoft.AnalysisServices.Tabular.CalculationItem>((IEnumerable<Microsoft.AnalysisServices.Tabular.CalculationItem>) calculationGroup.CalculationItems, (item => item != calculationItem && item.Ordinal == update.Ordinal.Value)))
        {
          DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(69, 2);
          interpolatedStringHandler.AppendLiteral("Calculation item with ordinal ");
          ref DefaultInterpolatedStringHandler local = ref interpolatedStringHandler;
          ordinal1 = update.Ordinal;
          int num2 = ordinal1.Value;
          local.AppendFormatted<int>(num2);
          interpolatedStringHandler.AppendLiteral(" already exists in calculation group '");
          interpolatedStringHandler.AppendFormatted(calculationGroupName);
          interpolatedStringHandler.AppendLiteral("'");
          throw new McpException(interpolatedStringHandler.ToStringAndClear());
        }
        Microsoft.AnalysisServices.Tabular.CalculationItem calculationItem1 = calculationItem;
        ordinal1 = update.Ordinal;
        int num3 = ordinal1.Value;
        calculationItem1.Ordinal = num3;
        flag = true;
      }
    }
    if (update.FormatStringExpression != null)
    {
      if (string.IsNullOrEmpty(update.FormatStringExpression))
      {
        if (calculationItem.FormatStringDefinition != null)
        {
          calculationItem.FormatStringDefinition = (Microsoft.AnalysisServices.Tabular.FormatStringDefinition) null;
          flag = true;
        }
      }
      else
      {
        if (calculationItem.FormatStringDefinition == null)
          calculationItem.FormatStringDefinition = new Microsoft.AnalysisServices.Tabular.FormatStringDefinition();
        if ((calculationItem.FormatStringDefinition.Expression != update.FormatStringExpression))
        {
          calculationItem.FormatStringDefinition.Expression = update.FormatStringExpression;
          flag = true;
        }
      }
    }
    if (!flag)
      return new CalculationItemOperationResult()
      {
        State = calculationItem.State.ToString(),
        ErrorMessage = calculationItem.ErrorMessage,
        CalculationItemName = calculationItem.Name,
        CalculationGroupName = calculationGroupName,
        Ordinal = calculationItem.Ordinal,
        HasChanges = false
      };
    TransactionOperations.RecordOperation(info, $"Updated calculation item '{calculationItemName}' in calculation group '{calculationGroupName}'");
    ConnectionOperations.SaveChangesWithRollback(info, "update calculation item");
    return new CalculationItemOperationResult()
    {
      State = calculationItem.State.ToString(),
      ErrorMessage = calculationItem.ErrorMessage,
      CalculationItemName = calculationItem.Name,
      CalculationGroupName = calculationGroupName,
      Ordinal = calculationItem.Ordinal,
      HasChanges = true
    };
  }

  public static void RenameCalculationItem(
    string? connectionName,
    string calculationGroupName,
    string oldName,
    string newName)
  {
    if (string.IsNullOrWhiteSpace(calculationGroupName))
      throw new McpException("calculationGroupName is required");
    if (string.IsNullOrWhiteSpace(oldName))
      throw new McpException("oldName is required");
    if (string.IsNullOrWhiteSpace(newName))
      throw new McpException("newName is required");
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.CalculationGroup calculationGroup = CalculationGroupOperations.FindCalculationGroup(info.Database.Model, calculationGroupName);
    Microsoft.AnalysisServices.Tabular.CalculationItem calculationItem = calculationGroup.CalculationItems.Find(oldName) ?? throw new McpException($"Calculation item '{oldName}' not found in calculation group '{calculationGroupName}'");
    if (calculationGroup.CalculationItems.Contains(newName) && !string.Equals(oldName, newName, StringComparison.OrdinalIgnoreCase))
      throw new McpException($"Calculation item '{newName}' already exists in calculation group '{calculationGroupName}'");
    calculationItem.RequestRename(newName);
    TransactionOperations.RecordOperation(info, $"Renamed calculation item '{oldName}' to '{newName}' in calculation group '{calculationGroupName}'");
    ConnectionOperations.SaveChangesWithRollback(info, "rename calculation item", ConnectionOperations.CheckpointMode.AfterRequestRename);
  }

  public static void DeleteCalculationItem(
    string? connectionName,
    string calculationGroupName,
    string calculationItemName)
  {
    if (string.IsNullOrWhiteSpace(calculationGroupName))
      throw new McpException("calculationGroupName is required");
    if (string.IsNullOrWhiteSpace(calculationItemName))
      throw new McpException("calculationItemName is required");
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Database database = info.Database;
    Microsoft.AnalysisServices.Tabular.CalculationGroup calculationGroup = CalculationGroupOperations.FindCalculationGroup(database.Model, calculationGroupName);
    Microsoft.AnalysisServices.Tabular.CalculationItem metadataObject = calculationGroup.CalculationItems.Find(calculationItemName) ?? throw new McpException($"Calculation item '{calculationItemName}' not found in calculation group '{calculationGroupName}'");
    if (calculationGroup.CalculationItems.Count == 1)
      throw new McpException("Cannot delete the last calculation item from a calculation group. Delete the calculation group instead.");
    List<string> stringList = new List<string>();
    foreach (Microsoft.AnalysisServices.Tabular.Table table in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.Table, Microsoft.AnalysisServices.Tabular.Model>) database.Model.Tables)
    {
      foreach (Microsoft.AnalysisServices.Tabular.Measure measure in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.Measure, Microsoft.AnalysisServices.Tabular.Table>) table.Measures)
      {
        if (!string.IsNullOrWhiteSpace(measure.Expression))
        {
          if (measure.Expression.Contains($"'{calculationGroupName}'[{calculationItemName}]"))
            stringList.Add($"[{measure.Name}]");
        }
      }
    }
    if (stringList.Count > 0)
      throw new McpException($"Cannot delete calculation item '{calculationItemName}' because it is referenced by: {string.Join(", ", (IEnumerable<string>) stringList)}");
    calculationGroup.CalculationItems.Remove(metadataObject);
    TransactionOperations.RecordOperation(info, $"Deleted calculation item '{calculationItemName}' from calculation group '{calculationGroupName}'");
    ConnectionOperations.SaveChangesWithRollback(info, "delete calculation item");
  }

  public static void ReorderCalculationItems(
    string? connectionName,
    string calculationGroupName,
    List<string> calculationItemNamesInOrder)
  {
    if (string.IsNullOrWhiteSpace(calculationGroupName))
      throw new McpException("calculationGroupName is required");
    if (calculationItemNamesInOrder == null || calculationItemNamesInOrder.Count == 0)
      throw new McpException("calculationItemNamesInOrder cannot be null or empty");
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.CalculationGroup calculationGroup = CalculationGroupOperations.FindCalculationGroup(info.Database.Model, calculationGroupName);
    if (calculationItemNamesInOrder.Count != Enumerable.Count<string>(Enumerable.Distinct<string>((IEnumerable<string>) calculationItemNamesInOrder)))
      throw new McpException("Duplicate calculation item names found in calculationItemNamesInOrder");
    List<string> list = Enumerable.ToList<string>(Enumerable.Select<Microsoft.AnalysisServices.Tabular.CalculationItem, string>((IEnumerable<Microsoft.AnalysisServices.Tabular.CalculationItem>) calculationGroup.CalculationItems, (item => item.Name)));
    if (calculationItemNamesInOrder.Count != list.Count)
      throw new McpException($"Number of calculation items provided ({calculationItemNamesInOrder.Count}) does not match the number of calculation items in the group ({list.Count})");
    foreach (string str in calculationItemNamesInOrder)
    {
      if (!list.Contains(str))
        throw new McpException($"Calculation item '{str}' not found in calculation group '{calculationGroupName}'");
    }
    for (int i = 0; i < calculationItemNamesInOrder.Count; i++)
    {
      Microsoft.AnalysisServices.Tabular.CalculationItem calculationItem = Enumerable.FirstOrDefault<Microsoft.AnalysisServices.Tabular.CalculationItem>((IEnumerable<Microsoft.AnalysisServices.Tabular.CalculationItem>) calculationGroup.CalculationItems, (item => (item.Name == calculationItemNamesInOrder[i])));
      if (calculationItem != null)
        calculationItem.Ordinal = i;
    }
    TransactionOperations.RecordOperation(info, $"Reordered calculation items in calculation group '{calculationGroupName}'");
    ConnectionOperations.SaveChangesWithRollback(info, "reorder calculation items");
  }
}
