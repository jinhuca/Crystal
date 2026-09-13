using Microsoft.Management.Infrastructure.Generic;
using System.Runtime.CompilerServices;

namespace Crystal.Provider.Mmi.MmiEngine;

public static class CimAsyncExtensions {
  // Transforms MMI observables into native C# IAsyncEnumerable streams safely
  public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(
      this CimAsyncMultipleResults<T> observable,
      [EnumeratorCancellation] CancellationToken cancellationToken) {
    using var semaphore = new SemaphoreSlim(0);
    var queue = new Queue<T>();
    Exception? error = null;
    bool completed = false;

    // MMI does not cancel its native callbacks synchronously when the subscription is disposed, so
    // an in-flight onNext/onError/onCompleted can still fire while (or just after) this enumerator
    // tears down and disposes the semaphore. Releasing a disposed SemaphoreSlim throws
    // ObjectDisposedException on the native callback thread, which nothing can observe and crashes
    // the process (seen when closing ProcessDetailView). Swallow it: a late release only matters if
    // someone is still awaiting, and by then the enumerator is gone.
    void ReleaseSafe() {
      try { semaphore.Release(); }
      catch (ObjectDisposedException) { }
    }

    // Subscribe to the streaming WMI driver events
    using var subscription = observable.Subscribe(
        onNext: item => { lock (queue) queue.Enqueue(item); ReleaseSafe(); },
        onError: ex => { error = ex; ReleaseSafe(); },
        onCompleted: () => { completed = true; ReleaseSafe(); }
    );

    while (true) {
      cancellationToken.ThrowIfCancellationRequested();

      // Wait until a new device instance arrives from the driver layer
      await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

      if (error != null) throw error;

      T item;
      bool hasItem;
      lock (queue) {
        hasItem = queue.Count > 0;
        item = hasItem ? queue.Dequeue() : default!;
      }

      if (hasItem) {
        yield return item;
        continue;
      }

      if (completed) break;
    }
  }
}
