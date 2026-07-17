using Crystal.Plot2D.Common;
using System;

namespace Crystal.Plot2D.ViewportConstraints;

public sealed class DateTimeHorizontalAxisConstraint : ViewportConstraint {
  private readonly double minSeconds = new TimeSpan(ticks: DateTime.MinValue.Ticks).TotalSeconds;
  private readonly double maxSeconds = new TimeSpan(ticks: DateTime.MaxValue.Ticks).TotalSeconds;

  public override DataRect Apply(DataRect previousDataRect, DataRect proposedDataRect, Viewport2D viewport) {
    var borderRect = DataRect.Create(xMin: minSeconds, yMin: proposedDataRect.YMin, xMax: maxSeconds, yMax: proposedDataRect.YMax);
    if(proposedDataRect.IntersectsWith(rect: borderRect)) {
      return DataRect.Intersect(rect1: proposedDataRect, rect2: borderRect);
    }

    return previousDataRect;
  }
}
