// Decompiled with JetBrains decompiler
// Type: PowerBIModelingMCP.Library.Common.DataStructures.AnnotationHelpers
// Assembly: PowerBIModelingMCP.Library, Version=0.1.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E95465B-D3DD-4CA6-9488-1512B31258DC
// Assembly location: PowerBIModelingMCP.Library.dll inside D:\mcp\powerbi-modeling-mcp\extension\server\powerbi-modeling-mcp.exe)

using Microsoft.AnalysisServices.Tabular;
using ModelContextProtocol;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable
namespace PowerBIModelingMCP.Library.Common.DataStructures;

public static class AnnotationHelpers
{
  public static void ValidateAnnotations(
    List<KeyValuePair<string, string>>? annotations,
    string? errorPrefix = null)
  {
    if (annotations == null)
      return;
    string str = string.IsNullOrEmpty(errorPrefix) ? "" : errorPrefix + " ";
    HashSet<string> stringSet = new HashSet<string>();
    foreach (KeyValuePair<string, string> annotation in annotations)
    {
      if (string.IsNullOrWhiteSpace(annotation.Key))
        throw new McpException(str + "annotation key cannot be null or empty");
      if (!stringSet.Add(annotation.Key))
        throw new McpException($"{str}has duplicate annotation key: {annotation.Key}");
    }
  }

  public static void ApplyAnnotations<T>(
    T target,
    List<KeyValuePair<string, string>> annotations,
    Func<T, ICollection<Annotation>> annotationsAccessor)
  {
    if (annotations.Count == 0)
      return;
    ICollection<Annotation> annotations1 = annotationsAccessor(target);
    foreach (KeyValuePair<string, string> annotation1 in annotations)
    {
      ICollection<Annotation> annotations2 = annotations1;
      Annotation annotation2 = new Annotation { Name = annotation1.Key };
      annotation2.Value = annotation1.Value;
      annotations2.Add(annotation2);
    }
  }

  public static bool ReplaceAnnotations<T>(
    T target,
    List<KeyValuePair<string, string>> updates,
    Func<T, ICollection<Annotation>> annotationsAccessor)
  {
    ICollection<Annotation> annotations1 = annotationsAccessor(target);
    if (updates.Count == 0)
    {
      int num = annotations1.Count > 0 ? 1 : 0;
      if (num == 0)
        return num != 0;
      annotations1.Clear();
      return num != 0;
    }
    bool flag = false;
    Dictionary<string, string> updatesDict = new Dictionary<string, string>((IEqualityComparer<string>) StringComparer.Ordinal);
    foreach (KeyValuePair<string, string> update in updates)
      updatesDict[update.Key] = update.Value;
    List<Annotation> list = Enumerable.ToList<Annotation>(Enumerable.Where<Annotation>((IEnumerable<Annotation>) annotations1, (a => !updatesDict.ContainsKey(a.Name))));
    if (list.Count > 0)
    {
      foreach (Annotation annotation in list)
        annotations1.Remove(annotation);
      flag = true;
    }
    foreach (KeyValuePair<string, string> keyValuePair in updatesDict)
    {
      KeyValuePair<string, string> kv = keyValuePair;
      Annotation annotation1 = Enumerable.FirstOrDefault<Annotation>((IEnumerable<Annotation>) annotations1, (a => (a.Name == kv.Key)));
      if (annotation1 == null)
      {
        ICollection<Annotation> annotations2 = annotations1;
        Annotation annotation2 = new Annotation { Name = kv.Key };
        annotation2.Value = kv.Value;
        annotations2.Add(annotation2);
        flag = true;
      }
      else if ((annotation1.Value != kv.Value))
      {
        annotation1.Value = kv.Value;
        flag = true;
      }
    }
    return flag;
  }
}
