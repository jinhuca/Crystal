namespace Crystal.Service.Storage;

/// <summary>
/// Reads a live per-disk activity reading (one entry per physical disk). Extracted so
/// <see cref="StorageMonitor"/> can be unit-tested against a fake (the concrete
/// <see cref="StorageLoadSource"/> opens hardware in its constructor).
/// </summary>
public interface IStorageLoadSource {
  StorageLoadReading Read();

  /// <summary>Re-scans physical hardware so drives attached or removed since the source was opened
  /// start (or stop) producing readings. The concrete source opens its hardware session once in its
  /// constructor, so <see cref="StorageMonitor"/> calls this when the WMI inventory's drive set
  /// changes (a hotplug), keeping the live stream in step with the freshly enumerated inventory.</summary>
  void Refresh();
}
