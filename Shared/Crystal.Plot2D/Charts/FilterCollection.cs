using Crystal.Plot2D.Common;
using Crystal.Plot2D.Filters;
using Crystal.Plot2D.Graphs;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;

namespace Crystal.Plot2D.Charts;

/// <summary>
///   Represents a collection of point filters of <see cref="LineGraph"/>.
/// </summary>
public sealed class FilterCollection : NotifiableCollection<IPointsFilter> {
  protected override void OnItemAdding(IPointsFilter item) {
    ArgumentNullException.ThrowIfNull(item);
  }

  protected override void OnItemAdded(IPointsFilter item) {
    item.Changed += OnItemChanged;
  }

  private void OnItemChanged(object sender, EventArgs e) {
    OnCollectionChanged(e: new NotifyCollectionChangedEventArgs(action: NotifyCollectionChangedAction.Reset));
  }

  protected override void OnItemRemoving(IPointsFilter item) {
    item.Changed -= OnItemChanged;
  }

  internal List<Point> Filter(List<Point> points, Rect screenRect) {
    foreach(var filter_ in Items) {
      filter_.SetScreenRect(screenRect: screenRect);
      points = filter_.Filter(points: points);
    }

    return points;
  }
}
