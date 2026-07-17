using Crystal.Plot2D.Common.Auxiliary;
using System;
using System.Collections.Generic;
using System.Windows;

namespace Crystal.Plot2D.Filters;

[Obsolete(message: "Works incorrectly", error: true)]
public sealed class InclinationFilter : PointsFilterBase {
  private double criticalAngle = 179;
  public double CriticalAngle {
    get => criticalAngle;
    set {
      if(criticalAngle != value) {
        criticalAngle = value;
        RaiseChanged();
      }
    }
  }

  #region IPointFilter Members

  public override List<Point> Filter(List<Point> points) {
    if(points.Count == 0) {
      return points;
    }

    List<Point> res = new() { points[index: 0] };

    var i = 1;
    while(i < points.Count) {
      var added = false;
      var j = i;
      while(!added && j < points.Count - 1) {
        var x1 = res[index: res.Count - 1];
        var x2 = points[index: j];
        var x3 = points[index: j + 1];

        var a = (x1 - x2).Length;
        var b = (x2 - x3).Length;
        var c = (x1 - x3).Length;

        var angle13 = Math.Acos(d: (a * a + b * b - c * c) / (2 * a * b));
        var degrees = 180 / Math.PI * angle13;
        if(degrees < criticalAngle) {
          res.Add(item: x2);
          added = true;
          i = j + 1;
        }
        else {
          j++;
        }
      }
      // reached the end of resultPoints
      if(!added) {
        res.Add(item: points.GetLast());
        break;
      }
    }
    return res;
  }

  #endregion
}
