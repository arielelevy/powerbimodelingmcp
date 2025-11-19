// Copyright (c) 2025 Power BI Modeling MCP
// Licensed under the MIT License
//
using PowerBIModelingMCP.Library.Common.DataStructures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

#nullable enable
namespace PowerBIModelingMCP.Library.Common;

public static class ExportContentProcessor
{
  public static (string Content, bool IsTruncated, string? SavedFilePath, List<string> Warnings) ProcessExportContent(
    string content,
    ExportOptionsBase options)
  {
    List<string> stringList1 = new List<string>();
    string str1 = (string) null;
    if (!string.IsNullOrWhiteSpace(options.FilePath))
    {
      List<string> stringList2 = ExportContentProcessor.ValidateFilePath(options.FilePath);
      if (stringList2.Count > 0)
      {
        stringList1.AddRange(Enumerable.Select<string, string>((IEnumerable<string>) stringList2, (e => "File path validation error: " + e)));
      }
      else
      {
        try
        {
          str1 = ExportContentProcessor.SaveToFile(content, options.FilePath);
          if (str1 != null)
            stringList1.Add("Content saved to file: " + str1);
          else
            stringList1.Add("Failed to save content to file: " + options.FilePath);
        }
        catch (Exception ex)
        {
          stringList1.Add("Failed to save content to file: " + ex.Message);
        }
      }
    }
    bool flag = false;
    string str2 = content;
    int returnCharacters = options.MaxReturnCharacters;
    if (returnCharacters <= 0)
    {
      if (returnCharacters != -1 && returnCharacters == 0)
      {
        str2 = string.Empty;
        if (!string.IsNullOrWhiteSpace(str1))
          stringList1.Add("Content not returned in response - saved to file only");
      }
    }
    else if (content.Length > options.MaxReturnCharacters)
    {
      str2 = content.Substring(0, options.MaxReturnCharacters);
      flag = true;
      stringList1.Add($"Content truncated to {options.MaxReturnCharacters} characters (original length: {content.Length})");
    }
    return (str2, flag, str1, stringList1);
  }

  private static string? SaveToFile(string content, string filePath)
  {
    try
    {
      string file = Path.IsPathRooted(filePath) ? filePath : Path.GetFullPath(filePath);
      string directoryName = Path.GetDirectoryName(file);
      if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
        Directory.CreateDirectory(directoryName);
      File.WriteAllText(file, content);
      return file;
    }
    catch (Exception ex)
    {
      return (string) null;
    }
  }

  public static List<string> ValidateFilePath(string? filePath)
  {
    List<string> stringList = new List<string>();
    if (string.IsNullOrWhiteSpace(filePath))
      return stringList;
    try
    {
      if (filePath.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
        stringList.Add("File path contains invalid characters");
      if (filePath.Length > 260)
        stringList.Add("File path is too long (maximum 260 characters)");
      try
      {
        Path.GetFullPath(filePath);
      }
      catch (Exception ex)
      {
        stringList.Add("Invalid file path format");
      }
    }
    catch (Exception ex)
    {
      stringList.Add("Invalid file path format");
    }
    return stringList;
  }
}
