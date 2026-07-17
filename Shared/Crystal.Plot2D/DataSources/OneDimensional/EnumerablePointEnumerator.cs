using System.Collections;
using System.Windows;

namespace Crystal.Plot2D.DataSources.OneDimensional;

/// <summary>
/// This enumerator enumerates given enumerable object as sequence of points
/// </summary>
/// <typeparam name="T">Type parameter of source IEnumerable</typeparam>
public sealed class EnumerablePointEnumerator<T> : IPointEnumerator {
  public EnumerableDataSource<T> DataSource { get; }

  public IEnumerator Enumerator { get; }

  public EnumerablePointEnumerator(EnumerableDataSource<T> dataSource) {
    DataSource = dataSource;
    Enumerator = dataSource.Data.GetEnumerator();
  }

  public bool MoveNext() => Enumerator.MoveNext();

  public void GetCurrent(ref Point p)
    => DataSource.FillPoint(elem: (T)Enumerator.Current, point: ref p);

  public void ApplyMappings(DependencyObject target)
    => DataSource.ApplyMappings(target: target, elem: (T)Enumerator.Current);

  public void Dispose() {
    //enumerator.Reset();
  }
}