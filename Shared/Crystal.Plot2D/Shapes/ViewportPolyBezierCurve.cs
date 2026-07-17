using Crystal.Plot2D.Common;
using Crystal.Plot2D.Common.Auxiliary;
using Crystal.Plot2D.Transforms;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Crystal.Plot2D.Shapes;

public class ViewportPolyBezierCurve : ViewportPolylineBase {
  /// <summary>
  /// Initializes a new instance of the <see cref="ViewportPolyBezierCurve"/> class.
  /// </summary>
  public ViewportPolyBezierCurve() { }

  public PointCollection BezierPoints {
    get => (PointCollection)GetValue(dp: BezierPointsProperty);
    set => SetValue(dp: BezierPointsProperty, value: value);
  }

  public static readonly DependencyProperty BezierPointsProperty = DependencyProperty.Register(
    name: nameof(BezierPoints),
    propertyType: typeof(PointCollection),
    ownerType: typeof(ViewportPolyBezierCurve),
    typeMetadata: new FrameworkPropertyMetadata(defaultValue: null, propertyChangedCallback: OnPropertyChanged));

  private bool buildBezierPoints = true;

  public bool BuildBezierPoints {
    get => buildBezierPoints;
    set => buildBezierPoints = value;
  }

  private bool updating;

  protected override void UpdateUIRepresentationCore() {
    if(updating) {
      return;
    }

    updating = true;

    var transform_ = Plotter.Viewport.Transform;
    var geometry_ = PathGeometry;
    var points_ = Points;

    geometry_.Clear();

    if(BezierPoints != null) {
      points_ = BezierPoints;

      var screenPoints_ = points_.DataToScreen(transform: transform_).ToArray();
      PathFigure figure_ = new() {
        StartPoint = screenPoints_[0]
      };
      figure_.Segments.Add(value: new PolyBezierSegment(points: screenPoints_.Skip(skipCount: 1), isStroked: true));
      geometry_.Figures.Add(value: figure_);
      geometry_.FillRule = FillRule;
    }
    else if(points_ == null) {
    }
    else {
      PathFigure figure_ = new();
      if(points_.Count > 0) {
        Point[] bezierPoints_ = null;
        figure_.StartPoint = points_[index: 0].DataToScreen(transform: transform_);
        if(points_.Count > 1) {
          var screenPoints_ = points_.DataToScreen(transform: transform_).ToArray();

          bezierPoints_ = BezierBuilder.GetBezierPoints(points: screenPoints_).Skip(count: 1).ToArray();

          figure_.Segments.Add(value: new PolyBezierSegment(points: bezierPoints_, isStroked: true));
        }

        if(bezierPoints_ != null && buildBezierPoints) {
          Array.Resize(array: ref bezierPoints_, newSize: bezierPoints_.Length + 1);
          Array.Copy(sourceArray: bezierPoints_, sourceIndex: 0, destinationArray: bezierPoints_, destinationIndex: 1, length: bezierPoints_.Length - 1);
          bezierPoints_[0] = figure_.StartPoint;

          BezierPoints = new PointCollection(collection: bezierPoints_.ScreenToData(transform: transform_));
        }
      }

      geometry_.Figures.Add(value: figure_);
      geometry_.FillRule = FillRule;
    }

    updating = false;
  }
}
