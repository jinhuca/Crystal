using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Crystal.Plot2D.Common.Auxiliary;

internal static class ObservableCollectionHelper {
  public static void ApplyChanges<T>(this ObservableCollection<T> collection, NotifyCollectionChangedEventArgs args) {
    if(args.NewItems != null) {
      var startingIndex = args.NewStartingIndex;
      var newItems = args.NewItems;

      for(var i = 0; i < newItems.Count; i++) {
        var addedItem = (T)newItems[index: i];
        collection.Insert(index: startingIndex + i, item: addedItem);
      }
    }
    if(args.OldItems != null) {
      for(var i = 0; i < args.OldItems.Count; i++) {
        var removedItem = (T)args.OldItems[index: i];
        collection.Remove(item: removedItem);
      }
    }
  }
}
