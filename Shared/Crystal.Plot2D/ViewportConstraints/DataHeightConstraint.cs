using Crystal.Plot2D.Common;
using Crystal.Plot2D.Common.Auxiliary;
using Crystal.Plot2D.Graphs;
using System;
using System.Collections.Specialized;
using System.Windows;

namespace Crystal.Plot2D.ViewportConstraints;

public class DataHeightConstraint : ViewportConstraint, ISupportAttachToViewport {
  private double yEnlargeCoeff = 1.1;
  public double YEnlargeCoeff {
    get => yEnlargeCoeff;
    set {
      if(Math.Abs(yEnlargeCoeff - value) > Constants.Constants.FloatComparisonTolerance) {
        yEnlargeCoeff = value;
        RaiseChanged();
      }
    }
  }

  public override DataRect Apply(DataRect oldDataRect, DataRect newDataRect, Viewport2D viewport) {
    var overallBounds_ = DataRect.Empty;

    foreach(var chart_ in viewport.ContentBoundsHosts) {
      var plotterElement_ = chart_ as IPlotterElement;
      var visual_ = viewport.Plotter.VisualBindings[element: plotterElement_];
      var points_ = PointsGraphBase.GetVisiblePoints(obj: visual_);
      if(points_ != null) {
        // searching for indices of chart's visible points which are near left and right borders of newDataRect
        var startX_ = newDataRect.XMin;
        var endX_ = newDataRect.XMax;

        if(points_[index: 0].X > endX_ || points_[index: points_.Count - 1].X < startX_) {
          continue;
        }

        var startIndex_ = -1;

        // we assume that points are sorted by x values ascending
        if(startX_ <= points_[index: 0].X) {
          startIndex_ = 0;
        }
        else {
          for(var i_ = 1; i_ < points_.Count - 1; i_++) {
            if(points_[index: i_].X <= startX_ && startX_ < points_[index: i_ + 1].X) {
              startIndex_ = i_;
              break;
            }
          }
        }

        var endIndex_ = points_.Count;

        if(points_[index: points_.Count - 1].X < endX_) {
          endIndex_ = points_.Count;
        }
        else {
          for(var i_ = points_.Count - 1; i_ >= 1; i_--) {
            if(points_[index: i_ - 1].X <= endX_ && endX_ < points_[index: i_].X) {
              endIndex_ = i_;
              break;
            }
          }
        }

        var bounds_ = Rect.Empty;
        for(var i_ = startIndex_; i_ < endIndex_; i_++) {
          bounds_.Union(point: points_[index: i_]);
        }
        if(startIndex_ > 0) {
          var pt_ = GetInterpolatedPoint(x: startX_, p1: points_[index: startIndex_], p2: points_[index: startIndex_ - 1]);
          bounds_.Union(point: pt_);
        }
        if(endIndex_ < points_.Count - 1) {
          var pt_ = GetInterpolatedPoint(x: endX_, p1: points_[index: endIndex_], p2: points_[index: endIndex_ + 1]);
          bounds_.Union(point: pt_);
        }

        overallBounds_.Union(rect: bounds_);
      }
    }

    if(!overallBounds_.IsEmpty) {
      var y_ = overallBounds_.YMin;
      var height_ = overallBounds_.Height;

      if(height_ == 0) {
        height_ = newDataRect.Height;
        y_ -= height_ / 2;
      }

      newDataRect = new DataRect(xMin: newDataRect.XMin, yMin: y_, width: newDataRect.Width, height: height_);
      newDataRect = DataRectExtensions.ZoomY(rect: newDataRect, to: newDataRect.GetCenter(), ratio: yEnlargeCoeff);
    }

    return newDataRect;
  }

  private static Point GetInterpolatedPoint(double x, Point p1, Point p2) {
    var xRatio_ = (x - p1.X) / (p2.X - p1.X);
    var y_ = (1 - xRatio_) * p1.Y + xRatio_ * p2.Y;

    return new Point(x: x, y: y_);
  }

  #region ISupportAttach Members

  void ISupportAttachToViewport.Attach(Viewport2D viewport) {
    ((INotifyCollectionChanged)viewport.ContentBoundsHosts).CollectionChanged += OnContentBoundsHostsChanged;

    foreach(var item_ in viewport.ContentBoundsHosts) {
      if(item_ is PointsGraphBase chart_) {
        chart_.ProvideVisiblePoints = true;
      }
    }
  }

  private void OnContentBoundsHostsChanged(object sender, NotifyCollectionChangedEventArgs e) {
    if(e.NewItems != null) {
      foreach(var item_ in e.NewItems) {
        if(item_ is PointsGraphBase chart_) {
          chart_.ProvideVisiblePoints = true;
        }
      }
    }

    // todo probably set ProvideVisiblePoints to false on OldItems
  }

  void ISupportAttachToViewport.Detach(Viewport2D viewport) {
    ((INotifyCollectionChanged)viewport.ContentBoundsHosts).CollectionChanged -= OnContentBoundsHostsChanged;
  }

  #endregion
}
