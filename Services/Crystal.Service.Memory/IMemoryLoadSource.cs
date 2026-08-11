namespace Crystal.Service.Memory;

/// <summary>
/// Reads a live physical-memory load reading (used %, used/available GB, plus kernel-memory
/// figures). Extracted so <see cref="MemoryMonitor"/> can be unit-tested against a fake (the
/// concrete <see cref="MemoryLoadSource"/> opens hardware in its constructor).
/// </summary>
public interface IMemoryLoadSource {
  MemoryLoadReading Read();
}
