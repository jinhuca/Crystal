using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Crystal.Plot2D.Common;

[DebuggerDisplay(value: "Count = {Count}")]
internal sealed class ResourcePool<T> {
  private readonly List<T> pool = new();

  public T Get() {
    T item;

    if(pool.Count < 1) {
      item = default;
    }
    else {
      var index = pool.Count - 1;
      item = pool[index: index];
      pool.RemoveAt(index: index);
    }

    return item;
  }

  public void Put(T item) {
    if(item == null) {
      throw new ArgumentNullException(paramName: nameof(item));
    }

#if DEBUG
    if(pool.IndexOf(item: item) != -1) {
      Debugger.Break();
    }
#endif

    pool.Add(item: item);
  }

  public int Count => pool.Count;

  public void Clear() => pool.Clear();
}
