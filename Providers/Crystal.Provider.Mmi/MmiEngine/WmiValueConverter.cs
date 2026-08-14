using Microsoft.Management.Infrastructure;
using System.Collections.Frozen;

namespace Crystal.Provider.Mmi.MmiEngine;

/// <summary>
/// Converts native <see cref="CimInstance"/> property bags into the immutable
/// <see cref="WmiValue"/> dictionaries used throughout Crystal.Provider.Mmi.
/// </summary>
internal static class WmiValueConverter {
  /// <summary>
  /// Projects the properties of a single <see cref="CimInstance"/> into a
  /// <see cref="FrozenDictionary{TKey, TValue}"/> of <see cref="WmiValue"/> entries.
  /// Null-valued properties are skipped.
  /// </summary>
  public static FrozenDictionary<string, WmiValue> ToWmiValues(CimInstance instance) {
    var tempDict = new Dictionary<string, WmiValue>(StringComparer.Ordinal);

    foreach (var property in instance.CimInstanceProperties) {
      if (property.Value == null) continue;

      switch (property.Value) {
        case bool b: tempDict[property.Name] = new WmiValue(b); break;
        case string s: tempDict[property.Name] = new WmiValue(s); break;
        case string[] arr: tempDict[property.Name] = new WmiValue(arr); break;
        case int i: tempDict[property.Name] = new WmiValue(i); break;
        case uint ui: tempDict[property.Name] = new WmiValue((int)ui); break;
        case ushort us: tempDict[property.Name] = new WmiValue((int)us); break;
        case ulong ul: tempDict[property.Name] = new WmiValue(ul); break;
        case DateTime dt: tempDict[property.Name] = new WmiValue(dt); break;
        default: tempDict[property.Name] = new WmiValue(property.Value.ToString() ?? string.Empty); break;
      }
    }

    return tempDict.ToFrozenDictionary();
  }
}
