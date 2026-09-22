namespace Crystal.Service.Memory;

/// <summary>
/// Reads a live physical-memory load reading (used %, used/available GB, plus kernel-memory
/// figures). Extracted so <see cref="MemoryMonitor"/> can be unit-tested against a fake (the
/// concrete <see cref="MemoryLoadSource"/> opens hardware in its constructor).
/// </summary>
public interface IMemoryLoadSource {
  /// <summary>
  /// Takes a fresh sample of physical-memory load and returns it as a <see cref="MemoryLoadReading"/>.
  /// Called once per poll tick by <see cref="MemoryMonitor"/>; implementations re-sample the hardware
  /// on each call rather than returning a cached value.
  /// </summary>
  /// <returns>The current memory load reading.</returns>
  MemoryLoadReading Read();
}
