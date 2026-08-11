namespace Crystal.Service.Gpu;

/// <summary>The live GPU core-load seam consumed by <see cref="GpuInfoBuilder"/>. Abstracted from
/// the concrete <see cref="GpuLoadSource"/> (which opens a LibreHardwareMonitor <c>Computer</c>) so
/// the builder can be unit-tested against a fake without real hardware.</summary>
public interface IGpuLoadSource {
  /// <summary>Re-samples every GPU and returns one reading per adapter, keyed by adapter name.</summary>
  IReadOnlyList<GpuLoadReading> Read();
}
