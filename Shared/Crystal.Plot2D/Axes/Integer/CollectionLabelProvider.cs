using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;

namespace Crystal.Plot2D.Axes.Integer;

public class CollectionLabelProvider<T> : LabelProviderBase<int> {
  private IList<T> collection;

  [SuppressMessage(category: "Microsoft.Usage", checkId: "CA2227:CollectionPropertiesShouldBeReadOnly")]
  public IList<T> Collection {
    get => collection;
    set {
      ArgumentNullException.ThrowIfNull(value);

      if(collection != value) {
        DetachCollection();

        collection = value;

        AttachCollection();

        RaiseChanged();
      }
    }
  }

  #region Collection changed

  private void AttachCollection() {
    if(collection is INotifyCollectionChanged observableCollection) {
      observableCollection.CollectionChanged += OnCollectionChanged;
    }
  }

  private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
    RaiseChanged();
  }

  private void DetachCollection() {
    if(collection is INotifyCollectionChanged observableCollection) {
      observableCollection.CollectionChanged -= OnCollectionChanged;
    }
  }

  #endregion

  /// <summary>
  /// Initializes a new instance of the <see cref="CollectionLabelProvider&lt;T&gt;"/> class with empty labels collection.
  /// </summary>
  public CollectionLabelProvider() { }

  public CollectionLabelProvider(IList<T> collection)
    : this() {
    Collection = collection;
  }

  public CollectionLabelProvider(params T[] collection) {
    Collection = collection;
  }

  public override UIElement[] CreateLabels(ITicksInfo<int> ticksInfo) {
    var ticks = ticksInfo.Ticks;

    var res = new UIElement[ticks.Length];

    var tickInfo = new LabelTickInfo<int> { Info = ticksInfo.Info };

    for(var i = 0; i < res.Length; i++) {
      var tick = ticks[i];
      tickInfo.Tick = tick;

      if(0 <= tick && tick < collection.Count) {
        var text = collection[index: tick].ToString();
        res[i] = new TextBlock {
          Text = text,
          ToolTip = text
        };
      }
      else {
        res[i] = null;
      }
    }
    return res;
  }
}
