using System;
using System.Collections.Generic;
using System.Windows;

namespace Crystal.Plot2D.Common.Auxiliary.DataSearch;

internal sealed class SortedXSearcher1d {
  public SortedXSearcher1d(IList<Point> _collection) {
    Collection = _collection ?? throw new ArgumentNullException(paramName: "collection");
  }

  public IList<Point> Collection { get; }

  public SearchResult1d SearchXBetween(double x) => SearchXBetween(_x: x, _result: SearchResult1d.Empty);

  public SearchResult1d SearchXBetween(double _x, SearchResult1d _result) {
    if(Collection.Count == 0) {
      return SearchResult1d.Empty;
    }

    var lastIndex = Collection.Count - 1;

    if(_x < Collection[index: 0].X) {
      return SearchResult1d.Empty;
    }

    if(Collection[index: lastIndex].X < _x) {
      return SearchResult1d.Empty;
    }

    var startIndex = !_result.IsEmpty ? Math.Min(val1: _result.Index, val2: lastIndex) : 0;

    // searching ascending
    if(Collection[index: startIndex].X < _x) {
      for(var i = startIndex + 1; i <= lastIndex; i++) {
        if(Collection[index: i].X >= _x) {
          return new SearchResult1d { Index = i - 1 };
        }
      }
    }
    else // searching descending
    {
      for(var i = startIndex - 1; i >= 0; i--) {
        if(Collection[index: i].X <= _x) {
          return new SearchResult1d { Index = i };
        }
      }
    }

    throw new InvalidOperationException(message: "Should not appear here.");
  }
}