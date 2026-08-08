using System.Diagnostics;

namespace Crystal.Provider.Etw;

/// <summary>
/// TraceEvent (Microsoft.Diagnostics.Tracing) writes benign manifest/TDH parse diagnostics
/// straight to <see cref="System.Diagnostics.Trace"/> when it lays out a manifest-based provider
/// whose payload template it can't fully decode. The DxgKrnl GPU-scheduler provider we enable for
/// per-process GPU busy time (see <see cref="ProcessEtwReader"/>) has one such event, so its
/// registration emits, a handful of times:
/// <para>"Error: Array is variable sized and does not follow prefix convention."</para>
/// The events themselves still flow and our payload access is already guarded, so this is pure
/// noise. It reaches the output because <c>Trace.WriteLine(string)</c> bypasses
/// <see cref="TraceFilter"/> — the only way to drop it is to wrap the trace listeners. This wraps
/// each existing listener so those specific lines are swallowed while every other Trace/Debug
/// message passes through untouched. Installed once, from the ETW reader.
/// </summary>
internal sealed class TraceEventNoiseFilter : TraceListener {
  // Match on the stable core of the message (avoids brittleness around its internal double-space).
  private static readonly string[] BenignFragments = [
    "variable sized and does not follow",
  ];

  private static readonly object InstallGate = new();
  private static bool _installed;

  private readonly TraceListener _inner;

  private TraceEventNoiseFilter(TraceListener inner) => _inner = inner;

  /// <summary>Wraps the current trace listeners so TraceEvent's benign parse spew is dropped. Idempotent.</summary>
  public static void Install() {
    lock (InstallGate) {
      if (_installed) return;
      _installed = true;

      var wrapped = new TraceListener[Trace.Listeners.Count];
      for (int i = 0; i < wrapped.Length; i++)
        wrapped[i] = new TraceEventNoiseFilter(Trace.Listeners[i]);

      Trace.Listeners.Clear();
      Trace.Listeners.AddRange(wrapped);
    }
  }

  private static bool IsBenign(string? message) {
    if (message is null) return false;
    foreach (var fragment in BenignFragments)
      if (message.Contains(fragment, StringComparison.Ordinal)) return true;
    return false;
  }

  public override void Write(string? message) {
    if (!IsBenign(message)) _inner.Write(message);
  }

  public override void WriteLine(string? message) {
    if (!IsBenign(message)) _inner.WriteLine(message);
  }

  public override void Flush() => _inner.Flush();
  public override void Close() => _inner.Close();
}
