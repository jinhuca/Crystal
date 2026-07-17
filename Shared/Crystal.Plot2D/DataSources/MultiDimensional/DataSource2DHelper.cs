using System;
using System.Windows;

namespace Crystal.Plot2D.DataSources.MultiDimensional;

public static class DataSource2DHelper {
  public static Point[,] CreateUniformGrid(int width, int height, double gridWidth, double gridHeight) {
    return CreateUniformGrid(width: width, height: height, xStart: 0, yStart: 0, xStep: gridWidth / width, yStep: gridHeight / height);
  }

  public static Point[,] CreateUniformGrid(int width, int height, double xStart, double yStart, double xStep, double yStep) {
    var result = new Point[width, height];

    var x = xStart;
    for(var ix = 0; ix < width; ix++) {
      var y = yStart;
      for(var iy = 0; iy < height; iy++) {
        result[ix, iy] = new Point(x: x, y: y);
        y += yStep;
      }
      x += xStep;
    }

    return result;
  }

  public static Vector[,] CreateVectorData(int width, int height, Func<int, int, Vector> generator) {
    var result = new Vector[width, height];

    for(var ix = 0; ix < width; ix++) {
      for(var iy = 0; iy < height; iy++) {
        result[ix, iy] = generator(arg1: ix, arg2: iy);
      }
    }

    return result;
  }
}