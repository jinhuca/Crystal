using Crystal.Plot2D.Common;
using Crystal.Plot2D.Common.Auxiliary;
using System;
using System.Windows;

namespace Crystal.Plot2D.DataSources.MultiDimensional;

/// <summary>
///   Defines warped two-dimensional data source.
/// </summary>
/// <typeparam name="T">
///   Data piece type.
/// </typeparam>
public sealed class WarpedDataSource2D<T> : IDataSource2D<T> where T : struct {
  /// <summary>
  ///   Initializes a new instance of the <see cref="WarpedDataSource2D&lt;T&gt;"/> class.
  /// </summary>
  /// <param name="data">Data.</param>
  /// <param name="grid">Grid.</param>
  public WarpedDataSource2D(T[,] data, Point[,] grid) {
    ArgumentNullException.ThrowIfNull(data);
    ArgumentNullException.ThrowIfNull(grid);

    Verify.IsTrue(condition: data.GetLength(dimension: 0) == grid.GetLength(dimension: 0));
    Verify.IsTrue(condition: data.GetLength(dimension: 1) == grid.GetLength(dimension: 1));

    Data = data;
    Grid = grid;
    Width = data.GetLength(dimension: 0);
    Height = data.GetLength(dimension: 1);
  }

  /// <summary>
  ///   Gets two-dimensional data array.
  /// </summary>
  /// <value>
  ///   The data.
  /// </value>
  public T[,] Data { get; }

  /// <summary>
  ///   Gets the grid of data source.
  /// </summary>
  /// <value>
  ///   The grid.
  /// </value>
  public Point[,] Grid { get; }

  /// <summary>
  ///   Gets data grid width.
  /// </summary>
  /// <value>
  ///   The width.
  /// </value>
  public int Width { get; }

  /// <summary>
  ///   Gets data grid height.
  /// </summary>
  /// <value>
  ///   The height.
  /// </value>
  public int Height { get; }

  public IDataSource2D<T> GetSubset() => throw new NotImplementedException();
  private void RaiseChanged() => Changed?.Invoke(sender: this, e: EventArgs.Empty);

  /// <summary>
  /// Occurs when data source changes.
  /// </summary>
  public event EventHandler Changed;
  public Range<T>? Range { get; } = null;
  public T? MissingValue { get; } = null;
}