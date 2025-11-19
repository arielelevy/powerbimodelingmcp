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
using System.Reflection;

#nullable enable
namespace PowerBIModelingMCP.Library.Core;

public static class CalendarOperations
{
  public static void ValidateCalendarBase(CalendarBase def, bool isCreate)
  {
    if (def == null)
      throw new McpException("Calendar definition cannot be null");
    if (string.IsNullOrWhiteSpace(def.Name))
      throw new McpException("Name is required");
    if (string.IsNullOrWhiteSpace(def.TableName))
      throw new McpException("TableName is required");
  }

  public static void ValidateCalendarColumnGroupBase(CalendarColumnGroupBase def, bool isCreate)
  {
    if (def == null)
      throw new McpException("Calendar column group definition cannot be null");
    if (string.IsNullOrWhiteSpace(def.CalendarName))
      throw new McpException("CalendarName is required");
    if (string.IsNullOrWhiteSpace(def.GroupType))
      throw new McpException("GroupType is required");
    if ((def.GroupType != "TimeRelated") && (def.GroupType != "TimeUnitAssociation"))
      throw new McpException("GroupType must be either 'TimeRelated' or 'TimeUnitAssociation'");
  }

  public static void ValidateCalendarColumnGroupCreate(CalendarColumnGroupCreate def)
  {
    if (def == null)
      throw new McpException("Calendar column group create definition cannot be null");
    if (string.IsNullOrWhiteSpace(def.GroupType))
      throw new McpException("GroupType is required");
    if ((def.GroupType == "TimeRelated"))
    {
      if (def.TimeRelatedGroup == null)
        throw new McpException("TimeRelatedGroup is required when GroupType is 'TimeRelated'");
      CalendarOperations.ValidateCalendarColumnGroupBase((CalendarColumnGroupBase) def.TimeRelatedGroup, true);
      if (def.TimeRelatedGroup.Columns == null || def.TimeRelatedGroup.Columns.Count == 0)
        throw new McpException("At least one column is required for TimeRelated column groups");
    }
    else
    {
      if (!(def.GroupType == "TimeUnitAssociation"))
        throw new McpException("GroupType must be either 'TimeRelated' or 'TimeUnitAssociation'");
      if (def.TimeUnitAssociation == null)
        throw new McpException("TimeUnitAssociation is required when GroupType is 'TimeUnitAssociation'");
      CalendarOperations.ValidateCalendarColumnGroupBase((CalendarColumnGroupBase) def.TimeUnitAssociation, true);
      if (string.IsNullOrWhiteSpace(def.TimeUnitAssociation.TimeUnit))
        throw new McpException("TimeUnit is required for TimeUnitAssociation column groups");
    }
  }

  private static Microsoft.AnalysisServices.Tabular.Calendar FindCalendar(
    Microsoft.AnalysisServices.Tabular.Model model,
    string tableName,
    string calendarName)
  {
    return (model.Tables.Find(tableName) ?? throw new McpException($"Table '{tableName}' not found in model")).Calendars.Find(calendarName) ?? throw new McpException($"Calendar '{calendarName}' not found in table '{tableName}'");
  }

  private static Microsoft.AnalysisServices.Tabular.Table GetCalendarTable(
    Microsoft.AnalysisServices.Tabular.Model model,
    string calendarName)
  {
    foreach (Microsoft.AnalysisServices.Tabular.Table table in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.Table, Microsoft.AnalysisServices.Tabular.Model>) model.Tables)
    {
      if (table.Calendars.Find(calendarName) != null)
        return table;
    }
    throw new McpException($"Calendar '{calendarName}' not found in any table");
  }

  public static List<CalendarList> ListCalendars(string? connectionName, string tableName)
  {
    if (string.IsNullOrWhiteSpace(tableName))
      throw new McpException("tableName is required");
    return Enumerable.ToList<CalendarList>(Enumerable.Select<Microsoft.AnalysisServices.Tabular.Calendar, CalendarList>((IEnumerable<Microsoft.AnalysisServices.Tabular.Calendar>) (ConnectionOperations.Get(connectionName).Database.Model.Tables.Find(tableName) ?? throw new McpException($"Table '{tableName}' not found")).Calendars, (c =>
    {
      return new CalendarList()
      {
        Name = c.Name,
        Description = !string.IsNullOrEmpty(c.Description) ? c.Description : (string) null,
        TableName = tableName,
        ColumnGroups = Enumerable.ToList<ColumnGroupList>(Enumerable.Select<Microsoft.AnalysisServices.Tabular.CalendarColumnGroup, ColumnGroupList>((IEnumerable<Microsoft.AnalysisServices.Tabular.CalendarColumnGroup>) c.CalendarColumnGroups, (cg => CalendarOperations.ConvertColumnGroupToList(cg))))
      };
    })));
  }

  public static CalendarGet GetCalendar(
    string? connectionName,
    string calendarName,
    string? tableName = null)
  {
    if (string.IsNullOrWhiteSpace(calendarName))
      throw new McpException("calendarName is required");
    Microsoft.AnalysisServices.Tabular.Model model = ConnectionOperations.Get(connectionName).Database.Model;
    Microsoft.AnalysisServices.Tabular.Calendar calendar1;
    Microsoft.AnalysisServices.Tabular.Table calendarTable;
    if (!string.IsNullOrWhiteSpace(tableName))
    {
      calendar1 = CalendarOperations.FindCalendar(model, tableName, calendarName);
      calendarTable = model.Tables.Find(tableName);
    }
    else
    {
      calendarTable = CalendarOperations.GetCalendarTable(model, calendarName);
      calendar1 = calendarTable.Calendars.Find(calendarName);
    }
    CalendarGet calendarGet = new CalendarGet
    {
      Name = calendar1.Name,
      TableName = calendarTable.Name
    };
    calendarGet.Description = calendar1.Description;
    calendarGet.LineageTag = calendar1.LineageTag;
    calendarGet.SourceLineageTag = calendar1.SourceLineageTag;
    calendarGet.ModifiedTime = new DateTime?(calendar1.ModifiedTime);
    calendarGet.CalendarColumnGroups = new List<CalendarColumnGroupInfo>();
    CalendarGet calendar2 = calendarGet;
    foreach (Microsoft.AnalysisServices.Tabular.CalendarColumnGroup calendarColumnGroup in (MetadataObjectCollection<Microsoft.AnalysisServices.Tabular.CalendarColumnGroup, Microsoft.AnalysisServices.Tabular.Calendar>) calendar1.CalendarColumnGroups)
      calendar2.CalendarColumnGroups.Add(CalendarOperations.ConvertColumnGroupToInfo(calendarColumnGroup, calendarName));
    return calendar2;
  }

  private static CalendarColumnGroupInfo ConvertColumnGroupToInfo(
    Microsoft.AnalysisServices.Tabular.CalendarColumnGroup columnGroup,
    string calendarName)
  {
    switch (columnGroup)
    {
      case Microsoft.AnalysisServices.Tabular.TimeRelatedColumnGroup relatedColumnGroup:
        CalendarColumnGroupInfo info1 = new CalendarColumnGroupInfo { GroupType = "TimeRelated" };
        TimeRelatedColumnGroupInfo relatedColumnGroupInfo = new TimeRelatedColumnGroupInfo
        {
          CalendarName = calendarName,
          GroupType = "TimeRelated"
        };
        relatedColumnGroupInfo.ModifiedTime = new DateTime?(relatedColumnGroup.ModifiedTime);
        relatedColumnGroupInfo.Columns = Enumerable.ToList<string>(Enumerable.Select<Microsoft.AnalysisServices.Tabular.Column, string>((IEnumerable<Microsoft.AnalysisServices.Tabular.Column>) relatedColumnGroup.Columns, (c => c.Name)));
        info1.TimeRelatedGroup = relatedColumnGroupInfo;
        return info1;
      case Microsoft.AnalysisServices.Tabular.TimeUnitColumnAssociation columnAssociation:
        CalendarColumnGroupInfo info2 = new CalendarColumnGroupInfo { GroupType = "TimeUnitAssociation" };
        TimeUnitColumnAssociationInfo columnAssociationInfo = new TimeUnitColumnAssociationInfo
        {
          CalendarName = calendarName,
          GroupType = "TimeUnitAssociation",
          TimeUnit = columnAssociation.TimeUnit.ToString()
        };
        columnAssociationInfo.ModifiedTime = new DateTime?(columnAssociation.ModifiedTime);
        columnAssociationInfo.PrimaryColumnName = columnAssociation.PrimaryColumn?.Name;
        columnAssociationInfo.AssociatedColumns = Enumerable.ToList<string>(Enumerable.Select<Microsoft.AnalysisServices.Tabular.Column, string>((IEnumerable<Microsoft.AnalysisServices.Tabular.Column>) columnAssociation.AssociatedColumns, (c => c.Name)));
        info2.TimeUnitAssociation = columnAssociationInfo;
        return info2;
      default:
        throw new McpException("Unknown calendar column group type: " + ((MemberInfo) columnGroup.GetType()).Name);
    }
  }

  private static ColumnGroupList ConvertColumnGroupToList(Microsoft.AnalysisServices.Tabular.CalendarColumnGroup columnGroup)
  {
    switch (columnGroup)
    {
      case Microsoft.AnalysisServices.Tabular.TimeRelatedColumnGroup relatedColumnGroup:
        ColumnGroupList list1 = new ColumnGroupList { Name = $"TimeRelated-{relatedColumnGroup.Columns.Count}cols" };
        list1.Description = $"Time-related column group with {relatedColumnGroup.Columns.Count} columns";
        list1.GroupType = "TimeRelated";
        list1.ColumnNames = Enumerable.ToList<string>(Enumerable.Select<Microsoft.AnalysisServices.Tabular.Column, string>((IEnumerable<Microsoft.AnalysisServices.Tabular.Column>) relatedColumnGroup.Columns, (c => c.Name)));
        list1.PrimaryColumnName = (string) null;
        return list1;
      case Microsoft.AnalysisServices.Tabular.TimeUnitColumnAssociation columnAssociation:
        ColumnGroupList list2 = new ColumnGroupList { Name = $"{columnAssociation.TimeUnit}" };
        list2.Description = $"Time unit association for {columnAssociation.TimeUnit}";
        list2.GroupType = "TimeUnitAssociation";
        list2.ColumnNames = Enumerable.ToList<string>(Enumerable.Select<Microsoft.AnalysisServices.Tabular.Column, string>((IEnumerable<Microsoft.AnalysisServices.Tabular.Column>) columnAssociation.AssociatedColumns, (c => c.Name)));
        list2.PrimaryColumnName = columnAssociation.PrimaryColumn?.Name;
        return list2;
      default:
        throw new McpException("Unknown calendar column group type: " + ((MemberInfo) columnGroup.GetType()).Name);
    }
  }

  public static string ExportTMDL(
    string? connectionName,
    string calendarName,
    string? tableName = null,
    ExportTmdl? options = null)
  {
    if (string.IsNullOrWhiteSpace(calendarName))
      throw new McpException("calendarName is required");
    Microsoft.AnalysisServices.Tabular.Model model = ConnectionOperations.Get(connectionName).Database.Model;
    Microsoft.AnalysisServices.Tabular.Calendar @object = string.IsNullOrWhiteSpace(tableName) ? CalendarOperations.GetCalendarTable(model, calendarName).Calendars.Find(calendarName) : CalendarOperations.FindCalendar(model, tableName, calendarName);
    if (@object == null)
      throw new McpException($"Calendar '{calendarName}' not found");
    if (options?.SerializationOptions == null)
      return TmdlSerializer.SerializeObject((MetadataObject) @object);
    MetadataSerializationOptions serializationOptions = options.SerializationOptions.ToMetadataSerializationOptions();
    return TmdlSerializer.SerializeObject((MetadataObject) @object, serializationOptions);
  }

  public static CalendarOperationResult CreateCalendar(string? connectionName, CalendarCreate def)
  {
    CalendarOperations.ValidateCalendarBase((CalendarBase) def, true);
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Table table = info.Database.Model.Tables.Find(def.TableName);
    if (table == null)
      throw new McpException($"Table '{def.TableName}' not found in model");
    if (table.Calendars.Find(def.Name) != null)
      throw new McpException($"Calendar '{def.Name}' already exists in table '{def.TableName}'");
    Microsoft.AnalysisServices.Tabular.Calendar calendar1 = new Microsoft.AnalysisServices.Tabular.Calendar();
    calendar1.Name = def.Name;
    Microsoft.AnalysisServices.Tabular.Calendar calendar2 = calendar1;
    if (!string.IsNullOrWhiteSpace(def.Description))
      calendar2.Description = def.Description;
    if (!string.IsNullOrWhiteSpace(def.LineageTag))
      calendar2.LineageTag = def.LineageTag;
    if (!string.IsNullOrWhiteSpace(def.SourceLineageTag))
      calendar2.SourceLineageTag = def.SourceLineageTag;
    table.Calendars.Add(calendar2);
    List<CalendarColumnGroupOperationResult> groupOperationResultList = new List<CalendarColumnGroupOperationResult>();
    if (def.CalendarColumnGroups != null && def.CalendarColumnGroups.Count > 0)
    {
      foreach (CalendarColumnGroupCreate calendarColumnGroup in def.CalendarColumnGroups)
      {
        try
        {
          CalendarColumnGroupOperationResult columnGroupInternal = CalendarOperations.CreateColumnGroupInternal(calendar2, table, calendarColumnGroup);
          groupOperationResultList.Add(columnGroupInternal);
        }
        catch (Exception ex)
        {
          throw new McpException("Failed to create column group: " + ex.Message);
        }
      }
    }
    TransactionOperations.RecordOperation(info, $"Created calendar '{def.Name}' in table '{def.TableName}'");
    ConnectionOperations.SaveChangesWithRollback(info, "create calendar");
    return new CalendarOperationResult()
    {
      CalendarName = calendar2.Name,
      TableName = def.TableName,
      ColumnGroupCount = calendar2.CalendarColumnGroups.Count,
      ColumnGroups = groupOperationResultList
    };
  }

  public static CalendarOperationResult UpdateCalendar(
    string? connectionName,
    CalendarUpdate update,
    string? tableName = null)
  {
    CalendarOperations.ValidateCalendarBase((CalendarBase) update, false);
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Model model = info.Database.Model;
    Microsoft.AnalysisServices.Tabular.Table calendarTable;
    Microsoft.AnalysisServices.Tabular.Calendar calendar;
    if (!string.IsNullOrWhiteSpace(tableName))
    {
      calendar = CalendarOperations.FindCalendar(model, tableName, update.Name);
      calendarTable = model.Tables.Find(tableName);
    }
    else
    {
      calendarTable = CalendarOperations.GetCalendarTable(model, update.Name);
      calendar = calendarTable.Calendars.Find(update.Name);
    }
    bool flag = false;
    if (update.Description != null)
    {
      string description = string.IsNullOrEmpty(update.Description) ? (string) null : update.Description;
      if ((calendar.Description != description))
      {
        calendar.Description = description;
        flag = true;
      }
    }
    if (update.LineageTag != null)
    {
      string lineageTag = string.IsNullOrEmpty(update.LineageTag) ? (string) null : update.LineageTag;
      if ((calendar.LineageTag != lineageTag))
      {
        calendar.LineageTag = lineageTag;
        flag = true;
      }
    }
    if (update.SourceLineageTag != null)
    {
      string sourceLineageTag = string.IsNullOrEmpty(update.SourceLineageTag) ? (string) null : update.SourceLineageTag;
      if ((calendar.SourceLineageTag != sourceLineageTag))
      {
        calendar.SourceLineageTag = sourceLineageTag;
        flag = true;
      }
    }
    if (flag)
    {
      TransactionOperations.RecordOperation(info, $"Updated calendar '{update.Name}' in table '{calendarTable.Name}'");
      ConnectionOperations.SaveChangesWithRollback(info, "update calendar");
    }
    return new CalendarOperationResult()
    {
      CalendarName = calendar.Name,
      TableName = calendarTable.Name,
      ColumnGroupCount = calendar.CalendarColumnGroups.Count,
      ColumnGroups = Enumerable.ToList<CalendarColumnGroupOperationResult>(Enumerable.Select<Microsoft.AnalysisServices.Tabular.CalendarColumnGroup, CalendarColumnGroupOperationResult>((IEnumerable<Microsoft.AnalysisServices.Tabular.CalendarColumnGroup>) calendar.CalendarColumnGroups, ((cg, index) => CalendarOperations.CreateColumnGroupOperationResult(cg, calendar.Name, index))))
    };
  }

  public static void RenameCalendar(
    string? connectionName,
    string oldName,
    string newName,
    string? tableName = null)
  {
    if (string.IsNullOrWhiteSpace(oldName))
      throw new McpException("oldName is required");
    if (string.IsNullOrWhiteSpace(newName))
      throw new McpException("newName is required");
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Model model = info.Database.Model;
    Microsoft.AnalysisServices.Tabular.Calendar calendar;
    Microsoft.AnalysisServices.Tabular.Table calendarTable;
    if (!string.IsNullOrWhiteSpace(tableName))
    {
      calendar = CalendarOperations.FindCalendar(model, tableName, oldName);
      calendarTable = model.Tables.Find(tableName);
    }
    else
    {
      calendarTable = CalendarOperations.GetCalendarTable(model, oldName);
      calendar = calendarTable.Calendars.Find(oldName);
    }
    if (calendarTable.Calendars.Find(newName) != null && !string.Equals(oldName, newName, StringComparison.OrdinalIgnoreCase))
      throw new McpException($"Calendar '{newName}' already exists in table '{calendarTable.Name}'");
    calendar.RequestRename(newName);
    TransactionOperations.RecordOperation(info, $"Renamed calendar from '{oldName}' to '{newName}' in table '{calendarTable.Name}'");
    ConnectionOperations.SaveChangesWithRollback(info, "rename calendar", ConnectionOperations.CheckpointMode.AfterRequestRename);
  }

  public static void DeleteCalendar(string? connectionName, string calendarName, string? tableName = null)
  {
    if (string.IsNullOrWhiteSpace(calendarName))
      throw new McpException("calendarName is required");
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Model model = info.Database.Model;
    Microsoft.AnalysisServices.Tabular.Calendar calendar;
    Microsoft.AnalysisServices.Tabular.Table calendarTable;
    if (!string.IsNullOrWhiteSpace(tableName))
    {
      calendar = CalendarOperations.FindCalendar(model, tableName, calendarName);
      calendarTable = model.Tables.Find(tableName);
    }
    else
    {
      calendarTable = CalendarOperations.GetCalendarTable(model, calendarName);
      calendar = calendarTable.Calendars.Find(calendarName);
    }
    calendarTable.Calendars.Remove(calendar);
    TransactionOperations.RecordOperation(info, $"Deleted calendar '{calendarName}' from table '{calendarTable.Name}'");
    ConnectionOperations.SaveChangesWithRollback(info, "delete calendar");
  }

  public static List<CalendarColumnGroupInfo> ListColumnGroups(
    string? connectionName,
    string calendarName,
    string? tableName = null)
  {
    if (string.IsNullOrWhiteSpace(calendarName))
      throw new McpException("calendarName is required");
    Microsoft.AnalysisServices.Tabular.Model model = ConnectionOperations.Get(connectionName).Database.Model;
    return Enumerable.ToList<CalendarColumnGroupInfo>(Enumerable.Select<Microsoft.AnalysisServices.Tabular.CalendarColumnGroup, CalendarColumnGroupInfo>((IEnumerable<Microsoft.AnalysisServices.Tabular.CalendarColumnGroup>) (string.IsNullOrWhiteSpace(tableName) ? CalendarOperations.GetCalendarTable(model, calendarName).Calendars.Find(calendarName) : CalendarOperations.FindCalendar(model, tableName, calendarName)).CalendarColumnGroups, (cg => CalendarOperations.ConvertColumnGroupToInfo(cg, calendarName))));
  }

  public static CalendarColumnGroupGet GetColumnGroup(
    string? connectionName,
    string calendarName,
    string? tableName,
    int columnGroupIndex)
  {
    if (string.IsNullOrWhiteSpace(calendarName))
      throw new McpException("calendarName is required");
    if (columnGroupIndex < 0)
      throw new McpException("columnGroupIndex must be non-negative");
    Microsoft.AnalysisServices.Tabular.Model model = ConnectionOperations.Get(connectionName).Database.Model;
    Microsoft.AnalysisServices.Tabular.Calendar calendar = string.IsNullOrWhiteSpace(tableName) ? CalendarOperations.GetCalendarTable(model, calendarName).Calendars.Find(calendarName) : CalendarOperations.FindCalendar(model, tableName, calendarName);
    if (columnGroupIndex >= calendar.CalendarColumnGroups.Count)
      throw new McpException($"Column group index {columnGroupIndex} is out of range. Calendar has {calendar.CalendarColumnGroups.Count} column groups.");
    CalendarColumnGroupInfo info = CalendarOperations.ConvertColumnGroupToInfo(calendar.CalendarColumnGroups[columnGroupIndex], calendarName);
    CalendarColumnGroupGet columnGroup = new CalendarColumnGroupGet { GroupType = info.GroupType };
    columnGroup.TimeRelatedGroup = info.TimeRelatedGroup;
    columnGroup.TimeUnitAssociation = info.TimeUnitAssociation;
    return columnGroup;
  }

  public static CalendarColumnGroupOperationResult CreateColumnGroup(
    string? connectionName,
    string calendarName,
    string? tableName,
    CalendarColumnGroupCreate def)
  {
    CalendarOperations.ValidateCalendarColumnGroupCreate(def);
    if (string.IsNullOrWhiteSpace(calendarName))
      throw new McpException("calendarName is required");
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Model model = info.Database.Model;
    Microsoft.AnalysisServices.Tabular.Calendar calendar;
    Microsoft.AnalysisServices.Tabular.Table calendarTable;
    if (!string.IsNullOrWhiteSpace(tableName))
    {
      calendar = CalendarOperations.FindCalendar(model, tableName, calendarName);
      calendarTable = model.Tables.Find(tableName);
    }
    else
    {
      calendarTable = CalendarOperations.GetCalendarTable(model, calendarName);
      calendar = calendarTable.Calendars.Find(calendarName);
    }
    CalendarColumnGroupOperationResult columnGroupInternal = CalendarOperations.CreateColumnGroupInternal(calendar, calendarTable, def);
    TransactionOperations.RecordOperation(info, $"Created column group in calendar '{calendarName}' in table '{calendarTable.Name}'");
    ConnectionOperations.SaveChangesWithRollback(info, "create column group");
    return columnGroupInternal;
  }

  private static CalendarColumnGroupOperationResult CreateColumnGroupInternal(
    Microsoft.AnalysisServices.Tabular.Calendar calendar,
    Microsoft.AnalysisServices.Tabular.Table table,
    CalendarColumnGroupCreate def)
  {
    Microsoft.AnalysisServices.Tabular.CalendarColumnGroup calendarColumnGroup;
    if ((def.GroupType == "TimeRelated"))
    {
      Microsoft.AnalysisServices.Tabular.TimeRelatedColumnGroup relatedColumnGroup = new Microsoft.AnalysisServices.Tabular.TimeRelatedColumnGroup();
      if (def.TimeRelatedGroup?.Columns != null)
      {
        foreach (string column in def.TimeRelatedGroup.Columns)
          relatedColumnGroup.Columns.Add(table.Columns.Find(column) ?? throw new McpException($"Column '{column}' not found in table '{table.Name}'"));
      }
      calendarColumnGroup = (Microsoft.AnalysisServices.Tabular.CalendarColumnGroup) relatedColumnGroup;
    }
    else
    {
      if (!(def.GroupType == "TimeUnitAssociation"))
        throw new McpException("Invalid GroupType: " + def.GroupType);
      if (def.TimeUnitAssociation == null)
        throw new McpException("TimeUnitAssociation definition is required");
      TimeUnit timeUnit;
      if (!Enum.TryParse<TimeUnit>(def.TimeUnitAssociation.TimeUnit, out timeUnit))
        throw new McpException("Invalid TimeUnit: " + def.TimeUnitAssociation.TimeUnit);
      Microsoft.AnalysisServices.Tabular.TimeUnitColumnAssociation columnAssociation = new Microsoft.AnalysisServices.Tabular.TimeUnitColumnAssociation(timeUnit);
      if (!string.IsNullOrWhiteSpace(def.TimeUnitAssociation.PrimaryColumnName))
        columnAssociation.PrimaryColumn = table.Columns.Find(def.TimeUnitAssociation.PrimaryColumnName) ?? throw new McpException($"Primary column '{def.TimeUnitAssociation.PrimaryColumnName}' not found in table '{table.Name}'");
      if (def.TimeUnitAssociation.AssociatedColumns != null)
      {
        foreach (string associatedColumn in def.TimeUnitAssociation.AssociatedColumns)
          columnAssociation.AssociatedColumns.Add(table.Columns.Find(associatedColumn) ?? throw new McpException($"Associated column '{associatedColumn}' not found in table '{table.Name}'"));
      }
      calendarColumnGroup = (Microsoft.AnalysisServices.Tabular.CalendarColumnGroup) columnAssociation;
    }
    calendar.CalendarColumnGroups.Add(calendarColumnGroup);
    return CalendarOperations.CreateColumnGroupOperationResult(calendarColumnGroup, calendar.Name, calendar.CalendarColumnGroups.Count - 1);
  }

  private static CalendarColumnGroupOperationResult CreateColumnGroupOperationResult(
    Microsoft.AnalysisServices.Tabular.CalendarColumnGroup columnGroup,
    string calendarName,
    int groupIndex)
  {
    CalendarColumnGroupOperationResult groupOperationResult = new CalendarColumnGroupOperationResult()
    {
      CalendarName = calendarName,
      GroupIndex = groupIndex
    };
    switch (columnGroup)
    {
      case Microsoft.AnalysisServices.Tabular.TimeRelatedColumnGroup relatedColumnGroup:
        groupOperationResult.GroupType = "TimeRelated";
        groupOperationResult.ColumnCount = relatedColumnGroup.Columns.Count;
        break;
      case Microsoft.AnalysisServices.Tabular.TimeUnitColumnAssociation columnAssociation:
        groupOperationResult.GroupType = "TimeUnitAssociation";
        groupOperationResult.ColumnCount = columnAssociation.AssociatedColumns.Count;
        groupOperationResult.TimeUnit = columnAssociation.TimeUnit.ToString();
        groupOperationResult.PrimaryColumnName = columnAssociation.PrimaryColumn?.Name;
        break;
    }
    return groupOperationResult;
  }

  public static CalendarColumnGroupOperationResult UpdateColumnGroup(
    string? connectionName,
    string calendarName,
    string? tableName,
    int columnGroupIndex,
    CalendarColumnGroupUpdate update)
  {
    if (string.IsNullOrWhiteSpace(calendarName))
      throw new McpException("calendarName is required");
    if (columnGroupIndex < 0)
      throw new McpException("columnGroupIndex must be non-negative");
    if (update == null)
      throw new McpException("update definition is required");
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Model model = info.Database.Model;
    Microsoft.AnalysisServices.Tabular.Calendar calendar;
    Microsoft.AnalysisServices.Tabular.Table calendarTable;
    if (!string.IsNullOrWhiteSpace(tableName))
    {
      calendar = CalendarOperations.FindCalendar(model, tableName, calendarName);
      calendarTable = model.Tables.Find(tableName);
    }
    else
    {
      calendarTable = CalendarOperations.GetCalendarTable(model, calendarName);
      calendar = calendarTable.Calendars.Find(calendarName);
    }
    if (columnGroupIndex >= calendar.CalendarColumnGroups.Count)
      throw new McpException($"Column group index {columnGroupIndex} is out of range. Calendar has {calendar.CalendarColumnGroups.Count} column groups.");
    Microsoft.AnalysisServices.Tabular.CalendarColumnGroup calendarColumnGroup = calendar.CalendarColumnGroups[columnGroupIndex];
    bool flag1 = calendarColumnGroup is Microsoft.AnalysisServices.Tabular.TimeRelatedColumnGroup;
    bool flag2 = calendarColumnGroup is Microsoft.AnalysisServices.Tabular.TimeUnitColumnAssociation;
    if (flag1 && (update.GroupType != "TimeRelated") || flag2 && (update.GroupType != "TimeUnitAssociation"))
      throw new McpException($"Cannot change column group type from {(flag1 ? "TimeRelated" : "TimeUnitAssociation")} to {update.GroupType}");
    bool flag3 = false;
    if ((update.GroupType == "TimeRelated") && calendarColumnGroup is Microsoft.AnalysisServices.Tabular.TimeRelatedColumnGroup relatedColumnGroup)
    {
      if (update.TimeRelatedGroup?.Columns != null)
      {
        relatedColumnGroup.Columns.Clear();
        foreach (string column in update.TimeRelatedGroup.Columns)
          relatedColumnGroup.Columns.Add(calendarTable.Columns.Find(column) ?? throw new McpException($"Column '{column}' not found in table '{calendarTable.Name}'"));
        flag3 = true;
      }
    }
    else if ((update.GroupType == "TimeUnitAssociation") && calendarColumnGroup is Microsoft.AnalysisServices.Tabular.TimeUnitColumnAssociation columnAssociation && update.TimeUnitAssociation != null)
    {
      if (!string.IsNullOrWhiteSpace(update.TimeUnitAssociation.TimeUnit))
      {
        TimeUnit timeUnit;
        if (!Enum.TryParse<TimeUnit>(update.TimeUnitAssociation.TimeUnit, out timeUnit))
          throw new McpException("Invalid TimeUnit: " + update.TimeUnitAssociation.TimeUnit);
        if (columnAssociation.TimeUnit != timeUnit)
        {
          columnAssociation.TimeUnit = timeUnit;
          flag3 = true;
        }
      }
      if (update.TimeUnitAssociation.PrimaryColumnName != null)
      {
        Microsoft.AnalysisServices.Tabular.Column column = (Microsoft.AnalysisServices.Tabular.Column) null;
        if (!string.IsNullOrWhiteSpace(update.TimeUnitAssociation.PrimaryColumnName))
        {
          column = calendarTable.Columns.Find(update.TimeUnitAssociation.PrimaryColumnName);
          if (column == null)
            throw new McpException($"Primary column '{update.TimeUnitAssociation.PrimaryColumnName}' not found in table '{calendarTable.Name}'");
        }
        if (columnAssociation.PrimaryColumn != column)
        {
          columnAssociation.PrimaryColumn = column;
          flag3 = true;
        }
      }
      if (update.TimeUnitAssociation.AssociatedColumns != null)
      {
        columnAssociation.AssociatedColumns.Clear();
        foreach (string associatedColumn in update.TimeUnitAssociation.AssociatedColumns)
          columnAssociation.AssociatedColumns.Add(calendarTable.Columns.Find(associatedColumn) ?? throw new McpException($"Associated column '{associatedColumn}' not found in table '{calendarTable.Name}'"));
        flag3 = true;
      }
    }
    if (flag3)
    {
      TransactionOperations.RecordOperation(info, $"Updated column group {columnGroupIndex} in calendar '{calendarName}' in table '{calendarTable.Name}'");
      ConnectionOperations.SaveChangesWithRollback(info, "update column group");
    }
    return CalendarOperations.CreateColumnGroupOperationResult(calendarColumnGroup, calendarName, columnGroupIndex);
  }

  public static void DeleteColumnGroup(
    string? connectionName,
    string calendarName,
    string? tableName,
    int columnGroupIndex)
  {
    if (string.IsNullOrWhiteSpace(calendarName))
      throw new McpException("calendarName is required");
    if (columnGroupIndex < 0)
      throw new McpException("columnGroupIndex must be non-negative");
    ConnectionInfo info = ConnectionOperations.Get(connectionName);
    Microsoft.AnalysisServices.Tabular.Model model = info.Database.Model;
    Microsoft.AnalysisServices.Tabular.Calendar calendar;
    Microsoft.AnalysisServices.Tabular.Table calendarTable;
    if (!string.IsNullOrWhiteSpace(tableName))
    {
      calendar = CalendarOperations.FindCalendar(model, tableName, calendarName);
      calendarTable = model.Tables.Find(tableName);
    }
    else
    {
      calendarTable = CalendarOperations.GetCalendarTable(model, calendarName);
      calendar = calendarTable.Calendars.Find(calendarName);
    }
    if (columnGroupIndex >= calendar.CalendarColumnGroups.Count)
      throw new McpException($"Column group index {columnGroupIndex} is out of range. Calendar has {calendar.CalendarColumnGroups.Count} column groups.");
    Microsoft.AnalysisServices.Tabular.CalendarColumnGroup calendarColumnGroup = calendar.CalendarColumnGroups[columnGroupIndex];
    calendar.CalendarColumnGroups.Remove(calendarColumnGroup);
    TransactionOperations.RecordOperation(info, $"Deleted column group {columnGroupIndex} from calendar '{calendarName}' in table '{calendarTable.Name}'");
    ConnectionOperations.SaveChangesWithRollback(info, "delete column group");
  }
}
