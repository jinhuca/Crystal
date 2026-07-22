using System.Runtime.CompilerServices;
using Microsoft.Management.Infrastructure.Generic;

namespace Crystal.Mmi.MmiEngine;

public static class CimAsyncExtensions {
  // Transforms MMI observables into native C# IAsyncEnumerable streams safely
  public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(
      this CimAsyncMultipleResults<T> observable,
      [EnumeratorCancellation] CancellationToken cancellationToken) {
    using var semaphore = new SemaphoreSlim(0);
    var queue = new Queue<T>();
    Exception? error = null;
    bool completed = false;

    // Subscribe to the streaming WMI driver events
    using var subscription = observable.Subscribe(
        onNext: item => { lock (queue) queue.Enqueue(item); semaphore.Release(); },
        onError: ex => { error = ex; semaphore.Release(); },
        onCompleted: () => { completed = true; semaphore.Release(); }
    );

    while (true) {
      cancellationToken.ThrowIfCancellationRequested();

      // Wait until a new device instance arrives from the driver layer
      await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

      if (error != null) throw error;

      lock (queue) {
        if (queue.Count > 0) {
          yield return queue.Dequeue();
          continue;
        }
      }

      if (completed) break;
    }
  }
}
