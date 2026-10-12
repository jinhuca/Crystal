namespace Crystal.ProcessModule.Models;

/// <summary>
/// Reads the live thread list for a single process on demand — the source for the expandable
/// thread rows under each process. Kept behind an interface so the view model (which decides
/// <em>which</em> processes are expanded and <em>when</em> to refresh) stays free of Win32 /
/// System.Diagnostics side effects and can be unit-tested with a fake.
/// </summary>
public interface IProcessThreadProvider {
  /// <summary>Snapshots the threads owned by <paramref name="processId"/>. Returns an empty list
  /// (never throws) when the process has exited or its threads can't be read.</summary>
  IReadOnlyList<ThreadSample> GetThreads(uint processId);
}
