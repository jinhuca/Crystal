using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Crystal.Plot2D.Common.Auxiliary;

internal static class IEnumerableExtensions {
  public static bool CountGreaterOrEqual<T>(this IEnumerable<T> enumerable, int count) {
    var counter = 0;
    using(var enumerator = enumerable.GetEnumerator()) {
      while(counter < count && enumerator.MoveNext()) {
        counter++;
      }
    }

    return counter == count;
  }

  public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> source, int maxCount) {
    using(var enumerator = new FixedEnumeratorWrapper<T>(_enumerator: source.GetEnumerator())) {
      do {
        var enumerable = new FixedEnumerable<T>(_enumerator: enumerator);
        yield return enumerable.Take(count: maxCount);
      }
      while(enumerator.CanMoveNext);
    }
  }

  private sealed class FixedEnumeratorWrapper<T> : IEnumerator<T> {
    private readonly IEnumerator<T> enumerator;

    public FixedEnumeratorWrapper(IEnumerator<T> _enumerator) {
      enumerator = _enumerator;
    }

    #region IEnumerator<T> Members

    public T Current => enumerator.Current;

    #endregion

    #region IDisposable Members

    public void Dispose() {
      //enumerator.Dispose();
    }

    #endregion

    #region IEnumerator Members

    object IEnumerator.Current => throw new NotImplementedException();

    private bool canMoveNext;
    public bool CanMoveNext => canMoveNext;

    public bool MoveNext() {
      canMoveNext = enumerator.MoveNext();
      return canMoveNext;
    }

    public void Reset() {
      enumerator.Reset();
    }

    #endregion
  }

  private sealed class FixedEnumerable<T> : IEnumerable<T> {
    private readonly IEnumerator<T> enumerator;
    public FixedEnumerable(IEnumerator<T> _enumerator) {
      enumerator = _enumerator;
    }

    #region IEnumerable<T> Members

    public IEnumerator<T> GetEnumerator() => enumerator;

    #endregion

    #region IEnumerable Members

    IEnumerator IEnumerable.GetEnumerator() {
      throw new NotSupportedException();
    }

    #endregion
  }
}
