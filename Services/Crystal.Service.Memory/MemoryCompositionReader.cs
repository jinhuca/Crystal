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
  private const double BytesPerGB = 1024.0 * 1024.0 * 1024.0;

  private readonly PerformanceCounter? _modified;
  private readonly PerformanceCounter? _standbyReserve;
  private readonly PerformanceCounter? _standbyNormal;
  private readonly PerformanceCounter? _standbyCore;
  private readonly PerformanceCounter? _free;
  private bool _disposed;

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

  // Helper to safely clean up if partial initialization occurs
  private void ReleaseCounters() {
    _modified?.Dispose();
    _standbyReserve?.Dispose();
    _standbyNormal?.Dispose();
    _standbyCore?.Dispose();
    _free?.Dispose();
  }

  public readonly record struct Reading(double? ModifiedGB, double? StandbyGB, double? FreeGB);

  /// <summary>Samples the page-list counters. Returns null members when a counter is unavailable;
  /// standby is the sum of its three priority buckets, matching Task Manager's single figure.</summary>
  public Reading Read() {
    double? modified = ReadGB(_modified);
    double? free = ReadGB(_free);
    double? standby = Sum(ReadGB(_standbyReserve), ReadGB(_standbyNormal), ReadGB(_standbyCore));
    return new Reading(modified, standby, free);
  }

  private static double? Sum(double? a, double? b, double? c) =>
      a is null && b is null && c is null ? null : (a ?? 0) + (b ?? 0) + (c ?? 0);

  private static double? ReadGB(PerformanceCounter? counter) {
    if (counter is null) return null;
    try {
      return counter.NextValue() / BytesPerGB;
    }
    catch {
      return null;
    }
  }

  private static PerformanceCounter? Create(string counterName) {
    try {
      return new PerformanceCounter("Memory", counterName, readOnly: true);
    }
    catch {
      return null;
    }
  }

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
