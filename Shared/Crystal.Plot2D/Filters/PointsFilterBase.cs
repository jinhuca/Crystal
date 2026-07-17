using Crystal.Plot2D.Common.Auxiliary;
using System;
using System.Collections.Generic;
using System.Windows;

namespace Crystal.Plot2D.Filters;

public abstract class PointsFilterBase : IPointsFilter {
  #region IPointsFilter Members

  public abstract List<Point> Filter(List<Point> points);

  public virtual void SetScreenRect(Rect screenRect) { }

  protected void RaiseChanged() {
    Changed.Raise(sender: this);
  }

  public event EventHandler Changed;

  #endregion
}
