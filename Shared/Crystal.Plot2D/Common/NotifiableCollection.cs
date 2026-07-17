using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Crystal.Plot2D.Common;

/// <summary>
///   This is a base class for some of collections in Crystal.Plot2D assembly.
///   It provides means to be notified when item adding and added events, which enables successors to, for example,
///   check if adding item is not equal to null.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class NotifiableCollection<T> : ObservableCollection<T> {
  #region Overrides

  protected override void InsertItem(int index, T item) {
    OnItemAdding(item: item);
    base.InsertItem(index: index, item: item);
    OnItemAdded(item: item);
  }

  protected override void ClearItems() {
    foreach(var item in Items) {
      OnItemRemoving(item: item);
    }
    base.ClearItems();
  }

  protected override void RemoveItem(int index) {
    var item = Items[index: index];
    OnItemRemoving(item: item);
    base.RemoveItem(index: index);
  }

  protected override void SetItem(int index, T item) {
    var oldItem = Items[index: index];
    OnItemRemoving(item: oldItem);
    OnItemAdding(item: item);
    base.SetItem(index: index, item: item);
    OnItemAdded(item: item);
  }

  protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e) {
    attemptsToRaiseEvent++;
    if(raiseCollectionChangedEvent) {
      base.OnCollectionChanged(e: e);
    }
  }

  #endregion

  /// <summary>
  ///   Called before item added to collection. Enables to perform validation.
  /// </summary>
  /// <param name="item">
  ///   The adding item.
  /// </param>
  protected virtual void OnItemAdding(T item) { }

  /// <summary>
  ///   Called when item is added.
  /// </summary>
  /// <param name="item">
  ///   The added item.
  /// </param>
  protected virtual void OnItemAdded(T item) { }

  /// <summary>
  ///   Called when item is being removed, but before it is actually removed.
  /// </summary>
  /// <param name="item">
  ///   The removing item.
  /// </param>
  protected virtual void OnItemRemoving(T item) { }

  private int attemptsToRaiseEvent;
  private bool raiseCollectionChangedEvent = true;

  #region Public

  public void BeginUpdate() {
    attemptsToRaiseEvent = 0;
    raiseCollectionChangedEvent = false;
  }

  public void EndUpdate(bool raiseReset) {
    raiseCollectionChangedEvent = true;
    if(attemptsToRaiseEvent > 0 && raiseReset) {
      OnCollectionChanged(e: new NotifyCollectionChangedEventArgs(action: NotifyCollectionChangedAction.Reset));
    }
  }

  public IDisposable BlockEvents(bool raiseReset) => new EventBlocker<T>(_collection: this, _raiseReset: raiseReset);

  private sealed class EventBlocker<TT> : IDisposable {
    private readonly NotifiableCollection<TT> collection;
    private readonly bool raiseReset = true;

    public EventBlocker(NotifiableCollection<TT> _collection, bool _raiseReset) {
      collection = _collection;
      raiseReset = _raiseReset;
      _collection.BeginUpdate();
    }

    #region IDisposable Members

    public void Dispose() => collection.EndUpdate(raiseReset: raiseReset);

    #endregion
  }

  #endregion
}
