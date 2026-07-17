using System;
using System.Collections.Generic;

namespace Crystal.Plot2D.Common.Auxiliary;

internal static class ArrayExtensions {
  internal static T Last<T>(this T[] array) {
    return array[^1];
  }

  internal static T[] CreateArray<T>(int length, T defaultValue) {
    var res = new T[length];
    for(var i = 0; i < res.Length; i++) {
      res[i] = defaultValue;
    }

    return res;
  }

  internal static IEnumerable<Range<T>> GetPairs<T>(this IList<T> array) {
    ArgumentNullException.ThrowIfNull(array);

    for(var i = 0; i < array.Count - 1; i++) {
      yield return new Range<T>(min: array[index: i], max: array[i + 1]);
    }
  }
}
