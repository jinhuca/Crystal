namespace Crystal.Service.Storage;

/// <summary>
/// Reads a live per-disk activity reading (one entry per physical disk). Extracted so
/// <see cref="StorageMonitor"/> can be unit-tested against a fake (the concrete
/// <see cref="StorageLoadSource"/> opens hardware in its constructor).
/// </summary>
public interface IStorageLoadSource {
  StorageLoadReading Read();
}
