using Crystal.Plot2D.Common;
using System;

namespace Crystal.Plot2D.ViewportConstraints;

public class ScaleInjectionConstraint : ViewportConstraint {
  private readonly Viewport2D parentViewport;

  public ScaleInjectionConstraint(Viewport2D parentViewport) {
    this.parentViewport = parentViewport ?? throw new ArgumentNullException(paramName: nameof(parentViewport));
    parentViewport.PropertyChanged += parentViewport_PropertyChanged;
  }

  private void parentViewport_PropertyChanged(object sender, ExtendedPropertyChangedEventArgs e) {
    if(e.PropertyName == "Visible") {
      RaiseChanged();
    }
  }

  public void SetHorizontalTransform(double parentMin, double childMin, double parentMax, double childMax) {
    xScale = (childMax - childMin) / (parentMax - parentMin);
    xShift = childMin - parentMin;
  }

  public void SetVerticalTransform(double parentMin, double childMin, double parentMax, double childMax) {
    yScale = (childMax - childMin) / (parentMax - parentMin);
    yShift = childMin - parentMin;
  }

  private double xShift;
  private double xScale = 1;
  private double yShift;
  private double yScale = 1;

  public override DataRect Apply(DataRect previousDataRect, DataRect proposedDataRect, Viewport2D viewport) {
    var parentVisible_ = parentViewport.Visible;

    var xMin_ = parentVisible_.XMin * xScale + xShift;
    var xMax_ = parentVisible_.XMax * xScale + xShift;
    var yMin_ = parentVisible_.YMin * yScale + yShift;
    var yMax_ = parentVisible_.YMax * yScale + yShift;

    return DataRect.Create(xMin: xMin_, yMin: yMin_, xMax: xMax_, yMax: yMax_);
  }
}
