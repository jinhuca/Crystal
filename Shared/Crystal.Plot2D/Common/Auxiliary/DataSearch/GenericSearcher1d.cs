using System;
using System.Collections.Generic;

namespace Crystal.Plot2D.Common.Auxiliary.DataSearch;

internal sealed class GenericSearcher1d<TCollection, TMember> where TMember : IComparable<TMember> {
  public GenericSearcher1d(IList<TCollection> _collection, Func<TCollection, TMember> _selector) {
    Collection = _collection ?? throw new ArgumentNullException(paramName: "collection");
    Selector1 = _selector ?? throw new ArgumentNullException(paramName: "selector");
  }

  public Func<TCollection, TMember> Selector => Selector1;

  public Func<TCollection, TMember> Selector1 { get; }

  public IList<TCollection> Collection { get; }

  public SearchResult1d SearchBetween(TMember x) => SearchBetween(_x: x, _result: SearchResult1d.Empty);

  public SearchResult1d SearchBetween(TMember _x, SearchResult1d _result) {
    if(Collection.Count == 0) {
      return SearchResult1d.Empty;
    }

    var lastIndex = Collection.Count - 1;

    if(_x.CompareTo(other: Selector(arg: Collection[index: 0])) < 0) {
      return SearchResult1d.Empty;
    }

    if(Selector(arg: Collection[index: lastIndex]).CompareTo(other: _x) < 0) {
      return SearchResult1d.Empty;
    }

    var startIndex = !_result.IsEmpty ? Math.Min(val1: _result.Index, val2: lastIndex) : 0;

    // searching ascending
    if(Selector(arg: Collection[index: startIndex]).CompareTo(other: _x) < 0) {
      for(var i = startIndex + 1; i <= lastIndex; i++) {
        if(Selector(arg: Collection[index: i]).CompareTo(other: _x) >= 0) {
          return new SearchResult1d { Index = i - 1 };
        }
      }
    }
    else // searching descending
    {
      for(var i = startIndex - 1; i >= 0; i--) {
        if(Selector(arg: Collection[index: i]).CompareTo(other: _x) <= 0) {
          return new SearchResult1d { Index = i };
        }
      }
    }

    throw new InvalidOperationException(message: "Should not appear here.");
  }

  public SearchResult1d SearchFirstLess(TMember x) {
    if(Collection.Count == 0) {
      return SearchResult1d.Empty;
    }

    var result = SearchResult1d.Empty;
    for(var i = 0; i < Collection.Count; i++) {
      if(Selector(arg: Collection[index: i]).CompareTo(other: x) >= 0) {
        result.Index = i;
        break;
      }
    }

    return result;
  }

  public SearchResult1d SearchGreater(TMember x) {
    if(Collection.Count == 0) {
      return SearchResult1d.Empty;
    }

    var result = SearchResult1d.Empty;
    for(var i = Collection.Count - 1; i >= 0; i--) {
      if(Selector(arg: Collection[index: i]).CompareTo(other: x) <= 0) {
        result.Index = i;
        break;
      }
    }

    return result;
  }
}