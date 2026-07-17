using Crystal.Plot2D.DataSources.MultiDimensional;
using System;
using System.Windows;

namespace Crystal.Plot2D.Common.Auxiliary;

public static class DataSource2DExtensions {
  public static Range<double> GetMinMax(this double[,] data) {
    data.VerifyNotNull(paramName: nameof(data));

    var width_ = data.GetLength(dimension: 0);
    var height_ = data.GetLength(dimension: 1);
    Verify.IsTrueWithMessage(condition: width_ > 0, message: Strings.Exceptions.ArrayWidthShouldBePositive);
    Verify.IsTrueWithMessage(condition: height_ > 0, message: Strings.Exceptions.ArrayHeightShouldBePositive);

    var min_ = data[0, 0];
    var max_ = data[0, 0];
    for(var x_ = 0; x_ < width_; x_++) {
      for(var y_ = 0; y_ < height_; y_++) {
        if(data[x_, y_] < min_) {
          min_ = data[x_, y_];
        }

        if(data[x_, y_] > max_) {
          max_ = data[x_, y_];
        }
      }
    }

    Range<double> res_ = new(min: min_, max: max_);
    return res_;
  }

  private static Range<double> GetMinMax(this double[,] data, double missingValue) {
    data.VerifyNotNull(paramName: nameof(data));

    var width_ = data.GetLength(dimension: 0);
    var height_ = data.GetLength(dimension: 1);
    Verify.IsTrueWithMessage(condition: width_ > 0, message: Strings.Exceptions.ArrayWidthShouldBePositive);
    Verify.IsTrueWithMessage(condition: height_ > 0, message: Strings.Exceptions.ArrayHeightShouldBePositive);

    var min_ = double.MaxValue;
    var max_ = double.MinValue;
    for(var x_ = 0; x_ < width_; x_++) {
      for(var y_ = 0; y_ < height_; y_++) {
        if(Math.Abs(data[x_, y_] - missingValue) > Constants.Constants.FloatComparisonTolerance && data[x_, y_] < min_) {
          min_ = data[x_, y_];
        }

        if(Math.Abs(data[x_, y_] - missingValue) > Constants.Constants.FloatComparisonTolerance && data[x_, y_] > max_) {
          max_ = data[x_, y_];
        }
      }
    }

    Range<double> res_ = new(min: min_, max: max_);
    return res_;
  }

  internal static Range<double> GetMinMax(this IDataSource2D<double> dataSource) {
    dataSource.VerifyNotNull(paramName: "dataSource");
    return GetMinMax(data: dataSource.Data);
  }

  internal static Range<double> GetMinMax(this IDataSource2D<double> dataSource, double missingValue) {
    dataSource.VerifyNotNull(paramName: "dataSource");
    return GetMinMax(data: dataSource.Data, missingValue: missingValue);
  }

  internal static Range<double> GetMinMax(this IDataSource2D<double> dataSource, DataRect area) {
    ArgumentNullException.ThrowIfNull(dataSource);

    var min_ = double.PositiveInfinity;
    var max_ = double.NegativeInfinity;
    var width_ = dataSource.Width;
    var height_ = dataSource.Height;
    var grid_ = dataSource.Grid;
    var data_ = dataSource.Data;
    for(var ix_ = 0; ix_ < width_; ix_++) {
      for(var iy_ = 0; iy_ < height_; iy_++) {
        if(area.Contains(point: grid_[ix_, iy_])) {
          var value_ = data_[ix_, iy_];
          if(value_ < min_) {
            min_ = value_;
          }

          if(value_ > max_) {
            max_ = value_;
          }
        }
      }
    }

    return min_ < max_ ? new Range<double>(min: min_, max: max_) : new Range<double>();
  }


  private static DataRect GetGridBounds(this Point[,] grid) {
    var minX_ = grid[0, 0].X;
    var maxX_ = minX_;
    var minY_ = grid[0, 0].Y;
    var maxY_ = minY_;

    var width_ = grid.GetLength(dimension: 0);
    var height_ = grid.GetLength(dimension: 1);
    for(var ix_ = 0; ix_ < width_; ix_++) {
      for(var iy_ = 0; iy_ < height_; iy_++) {
        var pt_ = grid[ix_, iy_];
        var x_ = pt_.X;
        var y_ = pt_.Y;

        if(x_ < minX_) {
          minX_ = x_;
        }

        if(x_ > maxX_) {
          maxX_ = x_;
        }

        if(y_ < minY_) {
          minY_ = y_;
        }

        if(y_ > maxY_) {
          maxY_ = y_;
        }
      }
    }

    return new DataRect(point1: new Point(x: minX_, y: minY_), point2: new Point(x: maxX_, y: maxY_));
  }

  public static DataRect GetGridBounds<T>(this IDataSource2D<T> dataSource) where T : struct => dataSource.Grid.GetGridBounds();

  public static DataRect GetGridBounds<T>(this INonUniformDataSource2D<T> dataSource) where T : struct {
    var xCoordinates_ = dataSource.XCoordinates;
    var yCoordinates_ = dataSource.YCoordinates;
    var xMin_ = xCoordinates_[0];
    var xMax_ = xCoordinates_[xCoordinates_.Length - 1];

    var yMin_ = yCoordinates_[0];
    var yMax_ = yCoordinates_[yCoordinates_.Length - 1];

    var contentBounds_ = DataRect.FromPoints(x1: xMin_, y1: yMin_, x2: xMax_, y2: yMax_);
    return contentBounds_;
  }
}
