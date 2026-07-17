//using Crystal.Plot2D.Isolines;
using System.Collections.Generic;

namespace Crystal.Plot2D.Common.Auxiliary;

internal static class DictionaryExtensions {
  internal static void Add<TKey, TValue>(this Dictionary<TKey, TValue> dict, TValue value, params TKey[] keys) {
    foreach(var key in keys) {
      dict.Add(key: key, value: value);
    }
  }
}
