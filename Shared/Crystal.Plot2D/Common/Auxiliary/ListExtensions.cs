using System;
using System.Collections.Generic;

namespace Crystal.Plot2D.Common.Auxiliary;

internal static class ListExtensions {
  /// <summary>
  ///   Gets last element of list.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="list"></param>
  /// <returns></returns>
  internal static T GetLast<T>(this List<T> list) {
    ArgumentNullException.ThrowIfNull(list);
    return list.Count == 0
      ? throw new InvalidOperationException(message: Strings.Exceptions.CannotGetLastElement)
      : list[index: list.Count - 1];
  }

  internal static void ForEach<T>(this IEnumerable<T> source, Action<T> action) {
    ArgumentNullException.ThrowIfNull(action);
    ArgumentNullException.ThrowIfNull(source);
    foreach(var item_ in source) {
      action(obj: item_);
    }
  }
}
