using System;
using System.Collections.Generic;
using System.Windows;

namespace Crystal.Plot2D.Filters;

public class FrequencyFilter2 : PointsFilterBase {
  private Rect screenRect;

  public override void SetScreenRect(Rect screenRect) {
    this.screenRect = screenRect;
  }

  public override List<Point> Filter(List<Point> points) {
    List<Point> result_ = new();

    using var enumerator_ = points.GetEnumerator();
    var currentX_ = double.NegativeInfinity;

    double maxX_ = 0, minY_ = 0, maxY_ = 0;

    Point left_ = new(), right_ = new(), top_ = new(), bottom_ = new();

    var isFirstPoint_ = true;
    while(enumerator_.MoveNext()) {
      var currPoint_ = enumerator_.Current;
      var x_ = currPoint_.X;
      var y_ = currPoint_.Y;
      var xInt_ = Math.Floor(d: x_);
      if(Math.Abs(xInt_ - currentX_) < Constants.Constants.FloatComparisonTolerance) {
        if(x_ > maxX_) {
          maxX_ = x_;
          right_ = currPoint_;
        }

        if(y_ > maxY_) {
          maxY_ = y_;
          top_ = currPoint_;
        }
        else if(y_ < minY_) {
          minY_ = y_;
          bottom_ = currPoint_;
        }
      }
      else {
        if(!isFirstPoint_) {
          result_.Add(item: left_);

          var leftY_ = top_.X < bottom_.X ? top_ : bottom_;
          var rightY_ = top_.X > bottom_.X ? top_ : bottom_;

          if(top_ != bottom_) {
            result_.Add(item: leftY_);
            result_.Add(item: rightY_);
          }
          else if(top_ != left_) {
            result_.Add(item: top_);
          }

          if(right_ != rightY_) {
            result_.Add(item: right_);
          }
        }

        currentX_ = xInt_;
        left_ = right_ = top_ = bottom_ = currPoint_;
        maxX_ = x_;
        minY_ = maxY_ = y_;
      }

      isFirstPoint_ = false;
    }

    return result_;
  }
}
