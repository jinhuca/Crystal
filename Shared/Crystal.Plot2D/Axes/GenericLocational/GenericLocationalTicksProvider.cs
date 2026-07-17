using Crystal.Plot2D.Common;
using Crystal.Plot2D.Common.Auxiliary;
using Crystal.Plot2D.Common.Auxiliary.DataSearch;
using System;
using System.Collections.Generic;

namespace Crystal.Plot2D.Axes.GenericLocational;

public class GenericLocationalTicksProvider<TCollection, TAxis> : ITicksProvider<TAxis> where TAxis : IComparable<TAxis> {
  private IList<TCollection> collection;
  public IList<TCollection> Collection {
    get => collection;
    set {
      ArgumentNullException.ThrowIfNull(value);

      Changed.Raise(sender: this);
      collection = value;
    }
  }

  private Func<TCollection, TAxis> axisMapping;
  public Func<TCollection, TAxis> AxisMapping {
    get => axisMapping;
    set {
      ArgumentNullException.ThrowIfNull(value);

      Changed.Raise(sender: this);
      axisMapping = value;
    }
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="GenericLocationalTicksProvider&lt;T&gt;"/> class.
  /// </summary>
  public GenericLocationalTicksProvider() { }

  /// <summary>
  /// Initializes a new instance of the <see cref="GenericLocationalTicksProvider&lt;T&gt;"/> class.
  /// </summary>
  /// <param name="collection">The collection of axis ticks and labels.</param>
  public GenericLocationalTicksProvider(IList<TCollection> collection) {
    Collection = collection;
  }

  public GenericLocationalTicksProvider(IList<TCollection> collection, Func<TCollection, TAxis> coordinateMapping) {
    Collection = collection;
    AxisMapping = coordinateMapping;
  }

  #region ITicksProvider<T> Members

  private SearchResult1d minResult = SearchResult1d.Empty;
  private SearchResult1d maxResult = SearchResult1d.Empty;
  private GenericSearcher1d<TCollection, TAxis> searcher;
  /// <summary>
  /// Generates ticks for given range and preferred ticks count.
  /// </summary>
  /// <param name="range">The range.</param>
  /// <param name="ticksCount">The ticks count.</param>
  /// <returns></returns>
  public ITicksInfo<TAxis> GetTicks(Range<TAxis> range, int ticksCount) {
    EnsureSearcher();

    //minResult = searcher.SearchBetween(range.Min, minResult);
    //maxResult = searcher.SearchBetween(range.Max, maxResult);

    minResult = searcher.SearchFirstLess(x: range.Min);
    maxResult = searcher.SearchGreater(x: range.Max);

    if(!(minResult.IsEmpty && maxResult.IsEmpty)) {
      var startIndex = !minResult.IsEmpty ? minResult.Index : 0;
      var endIndex = !maxResult.IsEmpty ? maxResult.Index : collection.Count - 1;

      var count = endIndex - startIndex + 1;

      var ticks = new TAxis[count];
      for(var i = startIndex; i <= endIndex; i++) {
        ticks[i - startIndex] = axisMapping(arg: collection[index: i]);
      }

      TicksInfo<TAxis> result = new() {
        Info = startIndex,
        TickSizes = ArrayExtensions.CreateArray(length: count, defaultValue: 1.0),
        Ticks = ticks
      };

      return result;
    }

    return TicksInfo<TAxis>.Empty;
  }

  private void EnsureSearcher() {
    if(searcher == null) {
      if(collection == null || axisMapping == null) {
        throw new InvalidOperationException(message: Strings.Exceptions.GenericLocationalProviderInvalidState);
      }

      searcher = new GenericSearcher1d<TCollection, TAxis>(_collection: collection, _selector: axisMapping);
    }
  }

  public int DecreaseTickCount(int ticksCount) {
    return collection.Count;
  }

  public int IncreaseTickCount(int ticksCount) {
    return collection.Count;
  }

  public ITicksProvider<TAxis> MinorProvider => null;

  public ITicksProvider<TAxis> MajorProvider => null;

  public event EventHandler Changed;

  #endregion
}
