using Crystal.Plot2D.Common.Auxiliary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Crystal.Plot2D.Common;

public class ObservableCollectionWrapper<T> : INotifyCollectionChanged, IList<T> {
  public ObservableCollectionWrapper() : this(_collection: new ObservableCollection<T>()) { }

  private readonly ObservableCollection<T> collection;

  public ObservableCollectionWrapper(ObservableCollection<T> _collection) {
    collection = _collection ?? throw new ArgumentNullException(paramName: nameof(_collection));
    _collection.CollectionChanged += collection_CollectionChanged;
  }

  private int attemptsToRaiseChanged;
  public bool RaisingEvents { get; private set; } = true;

  private void collection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
    attemptsToRaiseChanged++;
    if(RaisingEvents) {
      CollectionChanged.Raise(sender: this, e: e);
    }
  }

  #region Update methods

  public void BeginUpdate() {
    attemptsToRaiseChanged = 0;
    RaisingEvents = false;
  }
  public void EndUpdate() {
    RaisingEvents = true;
    if(attemptsToRaiseChanged > 0) {
      CollectionChanged.Raise(sender: this);
    }
  }

  public IDisposable BlockEvents() => new EventBlocker<T>(_collection: this);

  private sealed class EventBlocker<TT> : IDisposable {
    private readonly ObservableCollectionWrapper<TT> collection;
    public EventBlocker(ObservableCollectionWrapper<TT> _collection) {
      collection = _collection;
      _collection.BeginUpdate();
    }

    #region IDisposable Members

    public void Dispose() => collection.EndUpdate();

    #endregion
  }

  #endregion // end of Update methods

  #region IList<T> Members

  public int IndexOf(T item) => collection.IndexOf(item: item);

  public void Insert(int index, T item) => collection.Insert(index: index, item: item);

  public void RemoveAt(int index) => collection.RemoveAt(index: index);

  public T this[int index] {
    get => collection[index: index];
    set => collection[index: index] = value;
  }

  #endregion

  #region ICollection<T> Members

  public void Add(T item) => collection.Add(item: item);

  public void Clear() => collection.Clear();

  public bool Contains(T item) => collection.Contains(item: item);

  public void CopyTo(T[] array, int arrayIndex) => collection.CopyTo(array: array, index: arrayIndex);

  public int Count => collection.Count;

  public bool IsReadOnly => throw new NotImplementedException();

  public bool Remove(T item) => collection.Remove(item: item);

  #endregion

  #region IEnumerable<T> Members

  public IEnumerator<T> GetEnumerator() => collection.GetEnumerator();

  #endregion

  #region IEnumerable Members

  System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

  #endregion

  #region INotifyCollectionChanged Members

  public event NotifyCollectionChangedEventHandler CollectionChanged;

  #endregion
}