using System.Diagnostics;

namespace Crystal.Service.Memory;

/// <summary>
/// Reads the page-list sizes behind Task Manager's "Memory composition" bar — modified, standby and
/// free — which <c>GetPerformanceInfo</c> does not expose. The values come from the Windows
/// "Memory" performance-counter category (instantaneous byte gauges, so a single read suffices).
/// Counters are created once and reused; any failure yields null so the UI falls back to the plain
/// in-use bar.
/// </summary>
internal sealed class MemoryCompositionReader : IDisposable {
  /// <summary>
  /// Divisor converting the counters' raw byte values to GB.
  /// </summary>
  private const double BytesPerGB = 1024.0 * 1024.0 * 1024.0;

  /// <summary>
  /// "Modified Page List Bytes" counter (dirty pages awaiting write-back); null if unavailable.
  /// </summary>
  private readonly PerformanceCounter? _modified;

  /// <summary>
  /// "Standby Cache Reserve Bytes" counter — one of the three standby priority buckets.
  /// </summary>
  private readonly PerformanceCounter? _standbyReserve;

  /// <summary>
  /// "Standby Cache Normal Priority Bytes" counter — one of the three standby buckets.
  /// </summary>
  private readonly PerformanceCounter? _standbyNormal;

  /// <summary>
  /// "Standby Cache Core Bytes" counter — one of the three standby buckets.
  /// </summary>
  private readonly PerformanceCounter? _standbyCore;

  /// <summary>
  /// "Free &amp; Zero Page List Bytes" counter (unused, immediately allocatable pages).
  /// </summary>
  private readonly PerformanceCounter? _free;

  private bool _disposed;

  /// <summary>
  /// Creates and validates the page-list counters up front so <see cref="Read"/> is a cheap
  /// sample. If the "Memory" category or any required counter is missing (or access is denied), the
  /// failure is swallowed, all counters are released, and every subsequent read returns null — the UI
  /// then falls back to the plain in-use bar rather than the composition breakdown.
  /// </summary>
  public MemoryCompositionReader() {
    try {
      // 1. Validate that the entire category exists first
      if (!PerformanceCounterCategory.Exists("Memory")) {
        throw new InvalidOperationException("The 'Memory' performance counter category does not exist on this system.");
      }

      PerformanceCounterCategory category = new("Memory");

      // 2. Validate all required counters exist
      if (!category.CounterExists("Modified Page List Bytes") ||
          !category.CounterExists("Standby Cache Reserve Bytes") ||
          !category.CounterExists("Standby Cache Normal Priority Bytes") ||
          !category.CounterExists("Standby Cache Core Bytes") ||
          !category.CounterExists("Free & Zero Page List Bytes")) {
        throw new InvalidOperationException("Missing one or more required Memory page list counters.");
      }

      // 3. Keep the counter creation INSIDE the try block to safely catch instantiation failures
      _modified = Create("Modified Page List Bytes");
      _standbyReserve = Create("Standby Cache Reserve Bytes");
      _standbyNormal = Create("Standby Cache Normal Priority Bytes");
      _standbyCore = Create("Standby Cache Core Bytes");
      _free = Create("Free & Zero Page List Bytes");
    }
    catch (InvalidOperationException ex) {
      // Gracefully handle missing counters, corrupted registry, or missing permissions
      Console.WriteLine($"Initialization failed: {ex.Message}");

      // Fallback: Ensure fields are explicitly set to null if initialization fails
      ReleaseCounters();
    }
    catch (Exception ex) {
      // Catch generic/unexpected exceptions (like security or localized OS issues)
      Console.WriteLine($"Unexpected error initializing counters: {ex.Message}");
      ReleaseCounters();
    }
  }

  /// <summary>
  /// Disposes any counters that were created before initialization failed, so a partially
  /// constructed reader leaks nothing and leaves all fields effectively null.
  /// </summary>
  private void ReleaseCounters() {
    _modified?.Dispose();
    _standbyReserve?.Dispose();
    _standbyNormal?.Dispose();
    _standbyCore?.Dispose();
    _free?.Dispose();
  }

  /// <summary>
  /// One composition sample, in GB, each nullable when its counter is unavailable.
  /// </summary>
  /// <param name="ModifiedGB">Modified (dirty) page-list size.</param>
  /// <param name="StandbyGB">Standby page-list size (the three priority buckets summed).</param>
  /// <param name="FreeGB">Free/zero page-list size.</param>
  public readonly record struct Reading(double? ModifiedGB, double? StandbyGB, double? FreeGB);

  /// <summary>
  /// Samples the page-list counters. Returns null members when a counter is unavailable;
  /// standby is the sum of its three priority buckets, matching Task Manager's single figure.
  /// </summary>
  public Reading Read() {
    double? modified = ReadGB(_modified);
    double? free = ReadGB(_free);
    double? standby = Sum(ReadGB(_standbyReserve), ReadGB(_standbyNormal), ReadGB(_standbyCore));
    return new Reading(modified, standby, free);
  }

  /// <summary>
  /// Adds the three standby buckets, treating null as zero — but returns null when all three
  /// are null, so a wholly unavailable standby figure stays null rather than collapsing to 0.
  /// </summary>
  private static double? Sum(double? a, double? b, double? c) =>
      a is null && b is null && c is null ? null : (a ?? 0) + (b ?? 0) + (c ?? 0);

  /// <summary>
  /// Samples one counter and converts bytes to GB. Returns null when the counter is absent
  /// or the sample throws, so a single failing counter does not fail the whole read.
  /// </summary>
  /// <param name="counter">The counter to sample, or null.</param>
  /// <returns>The value in GB, or null.</returns>
  private static double? ReadGB(PerformanceCounter? counter) {
    if (counter is null) return null;
    try {
      return counter.NextValue() / BytesPerGB;
    }
    catch {
      return null;
    }
  }

  /// <summary>
  /// Creates a read-only counter in the "Memory" category, returning null instead of
  /// throwing so construction can proceed with the remaining counters.
  /// </summary>
  /// <param name="counterName">The counter name within the "Memory" category.</param>
  /// <returns>The counter, or null if it could not be created.</returns>
  private static PerformanceCounter? Create(string counterName) {
    try {
      return new PerformanceCounter("Memory", counterName, readOnly: true);
    }
    catch {
      return null;
    }
  }

  /// <summary>
  /// Disposes all counters. Idempotent.
  /// </summary>
  public void Dispose() {
    if (_disposed) return;
    _disposed = true;
    _modified?.Dispose();
    _standbyReserve?.Dispose();
    _standbyNormal?.Dispose();
    _standbyCore?.Dispose();
    _free?.Dispose();
  }
}
