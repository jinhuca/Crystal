using Crystal.ProcessModule.Models;

namespace Crystal.ProcessModule.Tests;

// Returns canned thread lists keyed by PID so the expandable thread-row behavior can be exercised
// without touching real OS threads. Set Threads[pid] before expanding a row; Calls records each
// lookup so a test can assert a refresh happened (or didn't).
internal sealed class FakeProcessThreadProvider : IProcessThreadProvider {
  public Dictionary<uint, IReadOnlyList<ThreadSample>> Threads { get; } = new();
  public List<uint> Calls { get; } = new();

  public IReadOnlyList<ThreadSample> GetThreads(uint processId) {
    Calls.Add(processId);
    return Threads.TryGetValue(processId, out var threads) ? threads : [];
  }
}
