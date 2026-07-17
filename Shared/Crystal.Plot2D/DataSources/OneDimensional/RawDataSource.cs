using System.Collections.Generic;
using System.Windows;

namespace Crystal.Plot2D.DataSources.OneDimensional;

public sealed class RawDataSource : EnumerableDataSourceBase<Point> {
  public RawDataSource(params Point[] data) : base(data: data) { }
  public RawDataSource(IEnumerable<Point> data) : base(data: data) { }

  public override IPointEnumerator GetEnumerator(DependencyObject context) {
    return new RawPointEnumerator(dataSource: this);
  }
}
