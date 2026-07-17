using Crystal.Plot2D.Charts;
using System;
using System.Collections.Generic;
using System.Windows;

namespace Crystal.Plot2D.DataSources.OneDimensional;

public static class DataSourceHelper {
  public static IEnumerable<Point> GetPoints(IPointDataSource dataSource) => GetPoints(dataSource: dataSource, context: null);

  public static IEnumerable<Point> GetPoints(IPointDataSource dataSource, DependencyObject context) {
    ArgumentNullException.ThrowIfNull(dataSource);
    context ??= new DataSource2dContext();
    using var enumerator_ = dataSource.GetEnumerator(context: context);
    Point p_ = new();
    while(enumerator_.MoveNext()) {
      enumerator_.GetCurrent(p: ref p_);
      yield return p_;
      p_ = new Point();
    }
  }
}