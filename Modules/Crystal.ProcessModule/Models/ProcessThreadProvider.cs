using ThreadState = System.Diagnostics.ThreadState;
using System.ComponentModel;
using System.Diagnostics;

namespace Crystal.ProcessModule.Models;

/// <summary>
/// System.Diagnostics-backed <see cref="IProcessThreadProvider"/>: enumerates a process's
/// <see cref="ProcessThread"/> collection. The collection is a snapshot, but each thread's fields are
/// read live from the OS object, so any of them can throw once the thread (or the whole process) exits
/// mid-read. Every read is therefore guarded — a thread that vanishes is skipped and a missing field
/// falls back to 0 / null, rather than crashing the poll.
/// </summary>
public sealed class ProcessThreadProvider : IProcessThreadProvider {
  public IReadOnlyList<ThreadSample> GetThreads(uint processId) {
    try {
      using var process = Process.GetProcessById((int)processId);
      var threads = process.Threads;
      var samples = new List<ThreadSample>(threads.Count);
      foreach (ProcessThread thread in threads) {
        if (TryRead(thread) is { } sample) samples.Add(sample);
      }
      return samples;
    }
    // PID gone between expand and read, already exited, or a protected process we can't enumerate.
    catch (ArgumentException) { return []; }
    catch (InvalidOperationException) { return []; }
    catch (Win32Exception) { return []; }
  }

  // Read one thread's fields, each guarded. A thread whose state can't be read has exited between the
  // snapshot and now — skip it (null). WaitReason is only valid while the thread is actually waiting.
  private static ThreadSample? TryRead(ProcessThread thread) {
    int id;
    try { id = thread.Id; }
    catch (InvalidOperationException) { return null; }

    ThreadState state;
    try { state = thread.ThreadState; }
    catch (InvalidOperationException) { return null; }

    ThreadWaitReason? wait = null;
    if (state == ThreadState.Wait) {
      try { wait = thread.WaitReason; }
      catch (InvalidOperationException) { }
      catch (Win32Exception) { }
    }

    TimeSpan cpu;
    try { cpu = thread.TotalProcessorTime; }
    catch (InvalidOperationException) { cpu = TimeSpan.Zero; }
    catch (Win32Exception) { cpu = TimeSpan.Zero; }

    DateTime? start;
    try { start = thread.StartTime; }
    catch (InvalidOperationException) { start = null; }
    catch (Win32Exception) { start = null; }

    return new ThreadSample(id, cpu, state, wait, TryReadInt(() => thread.BasePriority),
        TryReadInt(() => thread.CurrentPriority), start);
  }

  private static int TryReadInt(Func<int> read) {
    try { return read(); }
    catch (InvalidOperationException) { return 0; }
    catch (Win32Exception) { return 0; }
  }
}
