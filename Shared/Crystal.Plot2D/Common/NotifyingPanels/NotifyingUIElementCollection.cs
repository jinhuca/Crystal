using Crystal.Plot2D.Common.Auxiliary;
using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace Crystal.Plot2D.Common.NotifyingPanels;

internal sealed class NotifyingUIElementCollection : UIElementCollection, INotifyCollectionChanged {
  public NotifyingUIElementCollection(UIElement visualParent, FrameworkElement logicalParent)
    : base(visualParent: visualParent, logicalParent: logicalParent) {
    Collection.CollectionChanged += OnCollectionChanged;
  }

  private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
    CollectionChanged.Raise(sender: this, e: e);
  }

  #region Overrides

  public override int Add(UIElement element) {
    Collection.Add(item: element);
    return base.Add(element: element);
  }

  public override void Clear() {
    Collection.Clear();
    base.Clear();
  }

  public override void Insert(int index, UIElement element) {
    Collection.Insert(index: index, item: element);
    base.Insert(index: index, element: element);
  }

  public override void Remove(UIElement element) {
    Collection.Remove(item: element);
    base.Remove(element: element);
  }

  public override void RemoveAt(int index) {
    Collection.RemoveAt(index: index);
    base.RemoveAt(index: index);
  }

  public override void RemoveRange(int index, int count) {
    for(var i = index; i < index + count; i++) {
      Collection.RemoveAt(index: i);
    }

    base.RemoveRange(index: index, count: count);
  }

  public override UIElement this[int index] {
    get => base[index: index];
    set {
      Collection[index: index] = value;
      base[index: index] = value;
    }
  }

  public override int Count => Collection.Count;

  internal D3UIElementCollection Collection { get; } = new();

  #endregion

  #region INotifyCollectionChanged Members

  public event NotifyCollectionChangedEventHandler CollectionChanged;

  #endregion
}

internal sealed class D3UIElementCollection : NotifiableCollection<UIElement> {
  protected override void OnItemAdding(UIElement item) {
    ArgumentNullException.ThrowIfNull(item);
  }
}
