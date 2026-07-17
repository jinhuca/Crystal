using Crystal.Plot2D.Common;
using System;

namespace Crystal.Plot2D.ViewportConstraints;

internal sealed class DateTimeVerticalAxisConstraint : ViewportConstraint {
  private readonly double minSeconds = new TimeSpan(ticks: DateTime.MinValue.Ticks).TotalSeconds;
  private readonly double maxSeconds = new TimeSpan(ticks: DateTime.MaxValue.Ticks).TotalSeconds;

  public override DataRect Apply(DataRect previousDataRect, DataRect proposedDataRect, Viewport2D viewport) {
    var borderRect_ = DataRect.Create(xMin: proposedDataRect.XMin, yMin: minSeconds, xMax: proposedDataRect.XMax, yMax: maxSeconds);
    return proposedDataRect.IntersectsWith(rect: borderRect_) ? DataRect.Intersect(rect1: proposedDataRect, rect2: borderRect_) : previousDataRect;
  }
}
