using Crystal.Plot2D.Common;
using System;
using System.Windows;

namespace Crystal.Plot2D.DataSources.MultiDimensional;

/// <summary>
///   Defines empty two-dimensional data source.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class EmptyDataSource2D<T> : IDataSource2D<T> where T : struct {
  public T[,] Data { get; } = new T[0, 0];
  public Point[,] Grid { get; } = new Point[0, 0];
  public int Width => 0;
  public int Height => 0;
  private void RaiseChanged() => Changed?.Invoke(sender: this, e: EventArgs.Empty);
  public event EventHandler Changed;

  #region IDataSource2D<T> Members

  public Range<T>? Range => throw new NotImplementedException();
  public T? MissingValue => throw new NotImplementedException();

  #endregion

  #region IDataSource2D<T> Members

  public IDataSource2D<T> GetSubset()
    => throw new NotImplementedException();

  #endregion
}