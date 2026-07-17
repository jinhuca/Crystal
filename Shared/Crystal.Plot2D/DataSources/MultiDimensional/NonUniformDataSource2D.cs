using Crystal.Plot2D.Common;
using System;
using System.Windows;

namespace Crystal.Plot2D.DataSources.MultiDimensional;

public class NonUniformDataSource2D<T> : INonUniformDataSource2D<T> where T : struct {
  public NonUniformDataSource2D(double[] xCoordinates, double[] yCoordinates, T[,] data) {
    XCoordinates = xCoordinates ?? throw new ArgumentNullException(paramName: nameof(xCoordinates));
    YCoordinates = yCoordinates ?? throw new ArgumentNullException(paramName: nameof(yCoordinates));
    BuildGrid();
    Data = data ?? throw new ArgumentNullException(paramName: nameof(data));
  }

  private void BuildGrid() {
    Grid = new Point[Width, Height];
    for(var iy = 0; iy < Height; iy++) {
      for(var ix = 0; ix < Width; ix++) {
        Grid[ix, iy] = new Point(x: XCoordinates[ix], y: YCoordinates[iy]);
      }
    }
  }

  public double[] XCoordinates { get; }
  public double[] YCoordinates { get; }
  public T[,] Data { get; }
  public IDataSource2D<T> GetSubset() => throw new NotImplementedException();
  public void ApplyMappings() => throw new NotImplementedException();
  public Point[,] Grid { get; private set; }
  public int Width => XCoordinates.Length;
  public int Height => YCoordinates.Length;
#pragma warning disable CS0067 // The event 'NonUniformDataSource2D<T>.Changed' is never used
  public event EventHandler Changed;
#pragma warning restore CS0067 // The event 'NonUniformDataSource2D<T>.Changed' is never used

  #region IDataSource2D<T> Members

  public Range<T>? Range => throw new NotImplementedException();
  public T? MissingValue => throw new NotImplementedException();

  #endregion
}
