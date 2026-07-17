using System;
using System.Collections.Generic;
using System.Windows;

namespace Crystal.Plot2D.Common.Auxiliary;

public static class ListGenerator {
  public static IEnumerable<Point> GeneratePoints(int length, Func<int, Point> generator) {
    for(var i = 0; i < length; i++) {
      yield return generator(arg: i);
    }
  }

  public static IEnumerable<Point> GeneratePoints(int length, Func<int, double> x, Func<int, double> y) {
    for(var i = 0; i < length; i++) {
      yield return new Point(x: x(arg: i), y: y(arg: i));
    }
  }

  public static IEnumerable<T> Generate<T>(int length, Func<int, T> generator) {
    for(var i = 0; i < length; i++) {
      yield return generator(arg: i);
    }
  }
}
