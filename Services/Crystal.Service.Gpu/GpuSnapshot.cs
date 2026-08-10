namespace Crystal.Service.Gpu;

/// <summary>A live reading for one adapter — core load (0-100%), core temperature (°C), core
/// clock (MHz) and package power (W) — each nullable when the GPU exposes no matching sensor.
/// Keyed by adapter name so a consumer can correlate it with the matching
/// <see cref="GpuAdapterInfo"/>.</summary>
public sealed record GpuLoadReading(
    string AdapterName,
    double CoreLoadPercent,
    double? TemperatureC,
    double? ClockMhz,
    double? PowerW);

/// <summary>One poll of the GPU subsystem: the static adapter inventory (stable across
/// polls) paired with each adapter's current load.</summary>
public sealed record GpuSnapshot(
    IReadOnlyList<GpuAdapterInfo> Adapters,
    IReadOnlyList<GpuLoadReading> Loads);
