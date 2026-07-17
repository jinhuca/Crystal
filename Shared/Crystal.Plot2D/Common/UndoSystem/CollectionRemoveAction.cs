using System;
using System.Collections.Generic;

namespace Crystal.Plot2D.Common.UndoSystem;

public sealed class CollectionRemoveAction<T> : UndoAction {
  public CollectionRemoveAction(IList<T> collection, T item, int index) {
    if(item == null) {
      throw new ArgumentNullException(paramName: "addedItem");
    }

    Collection = collection ?? throw new ArgumentNullException(paramName: nameof(collection));
    Item = item;
    Index = index;
  }

  public IList<T> Collection { get; }
  public T Item { get; }
  public int Index { get; }
  public override void Do() => Collection.Remove(item: Item);
  public override void Undo() => Collection.Insert(index: Index, item: Item);
}
