using System.Collections.Generic;
using System.Windows;

namespace Crystal.Plot2D.DataSources.OneDimensional;

public static class DataSourceExtensions {
  public static RawDataSource AsDataSource(this IEnumerable<Point> points)
    => new(data: points);

  public static EnumerableDataSource<T> AsDataSource<T>(this IEnumerable<T> collection)
    => new(data: collection);

  public static EnumerableDataSource<T> AsXDataSource<T>(this IEnumerable<T> collection) {
    if(typeof(T) == typeof(double)) {
      return ((IEnumerable<double>)collection).AsXDataSource() as EnumerableDataSource<T>;
    }
    if(typeof(T) == typeof(float)) {
      return ((IEnumerable<float>)collection).AsXDataSource<float>() as EnumerableDataSource<T>;
    }
    return new EnumerableXDataSource<T>(data: collection);
  }

  public static EnumerableDataSource<T> AsYDataSource<T>(this IEnumerable<T> collection) {
    if(typeof(T) == typeof(double)) {
      return ((IEnumerable<double>)collection).AsYDataSource() as EnumerableDataSource<T>;
    }
    if(typeof(T) == typeof(float)) {
      return ((IEnumerable<float>)collection).AsYDataSource<float>() as EnumerableDataSource<T>;
    }
    return new EnumerableYDataSource<T>(data: collection);
  }

  public static EnumerableDataSource<float> AsXDataSource(this IEnumerable<float> collection)
    => new(data: collection) { XMapping = f => f };

  public static EnumerableDataSource<float> AsYDataSource(this IEnumerable<float> collection)
    => new(data: collection) { YMapping = f => f };

  public static EnumerableDataSource<double> AsXDataSource(this IEnumerable<double> collection)
    => new(data: collection) { XMapping = x => x };

  public static EnumerableDataSource<double> AsYDataSource(this IEnumerable<double> collection)
    => new(data: collection) { YMapping = y => y };

  public static CompositeDataSource Join(this IPointDataSource ds1, IPointDataSource ds2)
    => new(dataSources: new[] { ds1, ds2 });

  public static IEnumerable<Point> GetPoints(this IPointDataSource dataSource)
    => DataSourceHelper.GetPoints(dataSource: dataSource);

  public static IEnumerable<Point> GetPoints(this IPointDataSource dataSource, DependencyObject context)
    => DataSourceHelper.GetPoints(dataSource: dataSource, context: context);
}