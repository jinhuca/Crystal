using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace Crystal.Plot2D.Charts;

[DebuggerDisplay(value: "Count = {Count}")]
public sealed class FakePointList : IList<Point> {
  private int first;
  private int last;
  private int count;
  private Point startPoint;
  private bool hasPoints;

  private double leftBound;
  private double rightBound;
  private readonly List<Point> points;

  internal FakePointList(List<Point> points, double left, double right) {
    this.points = points;
    leftBound = left;
    rightBound = right;

    Calc();
  }

  internal void SetXBorders(double left, double right) {
    leftBound = left;
    rightBound = right;
    Calc();
  }

  private void Calc() {
    Debug.Assert(condition: leftBound <= rightBound);

    first = points.FindIndex(match: p => p.X > leftBound);
    if(first > 0) {
      first--;
    }

    last = points.FindLastIndex(match: p => p.X < rightBound);

    if(last < points.Count - 1) {
      last++;
    }

    count = last - first;
    hasPoints = first >= 0 && last > 0;

    if(hasPoints) {
      startPoint = points[index: first];
    }
  }

  public Point StartPoint => startPoint;

  public bool HasPoints => hasPoints;

  #region IList<Point> Members

  public int IndexOf(Point item) {
    throw new NotSupportedException();
  }

  public void Insert(int index, Point item) {
    throw new NotSupportedException();
  }

  public void RemoveAt(int index) {
    throw new NotSupportedException();
  }

  public Point this[int index] {
    get => points[index: first + 1 + index];
    set => throw new NotSupportedException();
  }

  #endregion

  #region ICollection<Point> Members

  public void Add(Point item) {
    throw new NotSupportedException();
  }

  public void Clear() {
    throw new NotSupportedException();
  }

  public bool Contains(Point item) {
    throw new NotSupportedException();
  }

  public void CopyTo(Point[] array, int arrayIndex) {
    throw new NotSupportedException();
  }

  public int Count => count;

  public bool IsReadOnly => throw new NotSupportedException();

  public bool Remove(Point item) {
    throw new NotSupportedException();
  }

  #endregion

  #region IEnumerable<Point> Members

  public IEnumerator<Point> GetEnumerator() {
    for(var i_ = first + 1; i_ <= last; i_++) {
      yield return points[index: i_];
    }
  }

  #endregion

  #region IEnumerable Members

  System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

  #endregion
}
