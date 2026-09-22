namespace Crystal.Service.Storage;

/// <summary>
/// Reads a live per-disk activity reading (one entry per physical disk). Extracted so
/// <see cref="StorageMonitor"/> can be unit-tested against a fake (the concrete
/// <see cref="StorageLoadSource"/> opens hardware in its constructor).
/// </summary>
public interface IStorageLoadSource {
  /// <summary>
  /// Samples every physical disk once and returns the current activity, transfer rates, and
  /// SMART/space readings — one <see cref="StorageDiskLoad"/> per disk. Called on the monitor's fast
  /// poll cadence, so implementations must be cheap and safe to invoke repeatedly.
  /// </summary>
  StorageLoadReading Read();

  /// <summary>
  /// Re-scans physical hardware so drives attached or removed since the source was opened
  /// start (or stop) producing readings. The concrete source opens its hardware session once in its
  /// constructor, so <see cref="StorageMonitor"/> calls this when the WMI inventory's drive set
  /// changes (a hotplug), keeping the live stream in step with the freshly enumerated inventory.
  /// </summary>
  void Refresh();
}
