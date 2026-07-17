using Crystal.Plot2D.Common.Auxiliary;
using System;
using System.Collections.Generic;
using System.Windows;

namespace Crystal.Plot2D.Filters;

internal sealed class FrequencyFilter : PointsFilterBase {
  /// <summary>
  ///   Visible region in screen coordinates.
  /// </summary>
  private Rect screenRect;

  #region IPointFilter Members

  public override void SetScreenRect(Rect screenRect) {
    this.screenRect = screenRect;
  }

  // todo probably use LINQ here.
  public override List<Point> Filter(List<Point> points) {
    if(points.Count == 0) {
      return points;
    }

    var resultPoints_ = points;
    List<Point> currentChain_ = new();

    if(points.Count > 2 * screenRect.Width) {
      resultPoints_ = new List<Point>();

      var currentX_ = Math.Floor(d: points[index: 0].X);
      foreach(var p_ in points) {
        if(Math.Floor(d: p_.X) == currentX_) {
          currentChain_.Add(item: p_);
        }
        else {
          // Analyze current chain
          if(currentChain_.Count <= 2) {
            resultPoints_.AddRange(collection: currentChain_);
          }
          else {
            var first_ = MinByX(points: currentChain_);
            var last_ = MaxByX(points: currentChain_);
            var min_ = MinByY(points: currentChain_);
            var max_ = MaxByY(points: currentChain_);
            resultPoints_.Add(item: first_);

            var smaller_ = min_.X < max_.X ? min_ : max_;
            var greater_ = min_.X > max_.X ? min_ : max_;
            if(smaller_ != resultPoints_.GetLast()) {
              resultPoints_.Add(item: smaller_);
            }

            if(greater_ != resultPoints_.GetLast()) {
              resultPoints_.Add(item: greater_);
            }

            if(last_ != resultPoints_.GetLast()) {
              resultPoints_.Add(item: last_);
            }
          }

          currentChain_.Clear();
          currentChain_.Add(item: p_);
          currentX_ = Math.Floor(d: p_.X);
        }
      }
    }

    resultPoints_.AddRange(collection: currentChain_);

    return resultPoints_;
  }

  #endregion

  private static Point MinByX(IList<Point> points) {
    var minPoint_ = points[index: 0];
    foreach(var p_ in points) {
      if(p_.X < minPoint_.X) {
        minPoint_ = p_;
      }
    }

    return minPoint_;
  }

  private static Point MaxByX(IList<Point> points) {
    var maxPoint_ = points[index: 0];
    foreach(var p_ in points) {
      if(p_.X > maxPoint_.X) {
        maxPoint_ = p_;
      }
    }

    return maxPoint_;
  }

  private static Point MinByY(IList<Point> points) {
    var minPoint_ = points[index: 0];
    foreach(var p_ in points) {
      if(p_.Y < minPoint_.Y) {
        minPoint_ = p_;
      }
    }

    return minPoint_;
  }

  private static Point MaxByY(IList<Point> points) {
    var maxPoint_ = points[index: 0];
    foreach(var p_ in points) {
      if(p_.Y > maxPoint_.Y) {
        maxPoint_ = p_;
      }
    }
    return maxPoint_;
  }
}
