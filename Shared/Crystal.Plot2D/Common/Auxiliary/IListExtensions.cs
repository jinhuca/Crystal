using System;
using System.Collections.Generic;
using System.Linq;

namespace Crystal.Plot2D.Common.Auxiliary;

public static class IListExtensions {
  public static void AddMany<T>(this IList<T> collection, IEnumerable<T> addingItems) {
    foreach(var item in addingItems) {
      collection.Add(item: item);
    }
  }

  public static void AddMany<T>(this IList<T> collection, params T[] children) {
    foreach(var child in children) {
      collection.Add(item: child);
    }
  }

  public static void RemoveAll<T>(this IList<T> collection, Type type) {
    var children = collection.Where(predicate: el => type.IsInstanceOfType(o: el)).ToArray();
    foreach(var child in children) {
      collection.Remove(item: (T)child);
    }
  }

  public static void RemoveAll<T, TDelete>(this IList<T> collection) {
    var children = collection.OfType<TDelete>().ToArray();
    foreach(var child in children) {
      collection.Remove(item: (T)(object)child);
    }
  }
}
