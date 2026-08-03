namespace GpuModule.Models;

/// <summary>A live core-load reading (0-100%) for one adapter, keyed by adapter name so a
/// consumer can correlate it with the matching <see cref="GpuAdapterInfo"/>.</summary>
public sealed record GpuLoadReading(string AdapterName, double CoreLoadPercent);

/// <summary>One poll of the GPU subsystem: the static adapter inventory (stable across
/// polls) paired with each adapter's current load.</summary>
public sealed record GpuSnapshot(
    IReadOnlyList<GpuAdapterInfo> Adapters,
    IReadOnlyList<GpuLoadReading> Loads);
