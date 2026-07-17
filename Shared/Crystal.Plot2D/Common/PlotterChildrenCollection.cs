using Crystal.Plot2D.Charts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Markup;

namespace Crystal.Plot2D.Common;

/// <summary>
/// Contains all charts added to Plotter.
/// </summary>
[ContentWrapper(contentWrapper: typeof(ViewportUIContainer))]
public sealed class PlotterChildrenCollection : NotifiableCollection<IPlotterElement>, IList {
  /// <summary>
  /// Initializes a new instance of the <see cref="PlotterChildrenCollection"/> class.
  /// </summary>
  internal PlotterChildrenCollection(PlotterBase plotter) {
    Plotter = plotter ?? throw new ArgumentNullException(paramName: nameof(plotter));
  }

  public PlotterBase Plotter { get; }

  /// <summary>
  /// Called before item added to collection. Enables to perform validation.
  /// </summary>
  /// <param name="item">The adding item.</param>
  protected override void OnItemAdding(IPlotterElement item) {
    ArgumentNullException.ThrowIfNull(item);
  }

  /// <summary>
  /// This override enables notifying about removing each element, instead of
  /// notifying about collection reset.
  /// </summary>
  protected override void ClearItems() {
    var items = new List<IPlotterElement>(collection: Items);
    foreach(var item in items) {
      Remove(item: item);
    }
  }

  #region Foreign content

  public void Add(FrameworkElement content) {
    ArgumentNullException.ThrowIfNull(content);

    if(content is IPlotterElement plotterElement) {
      Add(item: plotterElement);
    }
    else {
      ViewportUIContainer container = new(content: content);
      Add(item: container);
    }
  }

  #endregion // end of Foreign content

  #region IList Members

  int IList.Add(object value) {
    switch(value) {
      case null:
        throw new ArgumentNullException(paramName: nameof(value));
      case FrameworkElement content:
        Add(content: content);
        return 0;
      case IPlotterElement element:
        Add(item: element);
        return 0;
    }

    throw new ArgumentException(message: $"Children of type '{value.GetType()}' are not supported.");
  }

  void IList.Clear() => Clear();

  bool IList.Contains(object value) {
    return value is IPlotterElement element && Contains(item: element);
  }

  int IList.IndexOf(object value) {
    if(value is IPlotterElement element) {
      return IndexOf(item: element);
    }

    return -1;
  }

  void IList.Insert(int index, object value) {
    if(value is IPlotterElement element) {
      Insert(index: index, item: element);
    }
  }

  bool IList.IsFixedSize => false;

  bool IList.IsReadOnly => false;

  void IList.Remove(object value) {
    if(value is IPlotterElement element) {
      Remove(item: element);
    }
  }

  void IList.RemoveAt(int index) => RemoveAt(index: index);

  object IList.this[int index] {
    get => this[index: index];
    set {
      if(value is IPlotterElement element) {
        this[index: index] = element;
      }
    }
  }

  #endregion

  #region ICollection Members

  void ICollection.CopyTo(Array array, int index) {
    if(array is IPlotterElement[] elements) {
      CopyTo(array: elements, index: index);
    }
  }

  int ICollection.Count => Count;
  bool ICollection.IsSynchronized => false;
  object ICollection.SyncRoot => null;

  #endregion

  #region IEnumerable Members

  IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

  #endregion
}
