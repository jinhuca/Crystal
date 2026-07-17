using System;
using System.Collections.Generic;

namespace Crystal.Plot2D.Common.UndoSystem;

public sealed class CollectionAddAction<T> : UndoAction {
  public CollectionAddAction(ICollection<T> collection, T item) {
    if(item == null) {
      throw new ArgumentNullException(paramName: "addedItem");
    }

    Collection = collection ?? throw new ArgumentNullException(paramName: nameof(collection));
    Item = item;
  }

  public ICollection<T> Collection { get; }
  public T Item { get; }
  public override void Do() => Collection.Add(item: Item);
  public override void Undo() => Collection.Remove(item: Item);
}
