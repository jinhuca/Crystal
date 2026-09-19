namespace Crystal.Service.Gpu;

/// <summary>
/// Utilization of a single GPU engine (e.g. "3D", "Video Decode", "Copy",
/// "Render/Compute", "Media"), 0-100%. The aggregate <see cref="GpuLoadReading.CoreLoadPercent"/>
/// is the max across these engines; the breakdown shows which one is driving it.
/// </summary>
/// <param name="Name">The engine name.</param>
public sealed record GpuEngineLoad(string Name, double LoadPercent);

/// <summary>
/// Power draw of a single GPU rail beyond the aggregate package power (e.g. AMD "PPT" /
/// "SoC" / "Core", NVIDIA "12VHPWR Connector" and per-pin rails), in watts. Each rail maps to a
/// single sensor, so its session <paramref name="MinW"/>/<paramref name="MaxW"/> come straight from
/// the provider (null when the provider hasn't recorded an extreme yet).
/// </summary>
/// <param name="Name">The rail name.</param>
/// <param name="PowerW">The power draw in watts.</param>
/// <param name="MinW">The lowest power seen this session, in watts.</param>
/// <param name="MaxW">The highest power seen this session, in watts.</param>
public sealed record GpuPowerRail(string Name, double PowerW, double? MinW = null, double? MaxW = null);

/// <summary>
/// A live reading for one adapter — core load (0-100%), core temperature (°C), core
/// clock (MHz), package power (W), VRAM used/total (GB), memory clock (MHz), fan speed (RPM) and
/// core voltage (V), hot-spot temperature (°C) and memory temperature (°C), and PCIe Rx/Tx
/// throughput (MB/s) — each nullable when the GPU exposes no matching sensor. <see cref="EngineLoads"/>
/// is the per-engine utilization breakdown (empty when the adapter exposes none). Keyed by adapter
/// name so a consumer can correlate it with the matching <see cref="GpuAdapterInfo"/>.
/// <para>The single-sensor metrics (core/hot-spot/memory temperature, core/memory clock, core
/// voltage, package power) also carry the session Min/Max the provider tracks alongside the current
/// value, so a consumer can render CPU-style value/min/max tables. Aggregate metrics (core load,
/// VRAM used/total, fan) have no well-defined session range and expose value only.</para>
/// </summary>
public sealed record GpuLoadReading(
  string AdapterName,
  double CoreLoadPercent,
  double? TemperatureC,
  double? ClockMhz,
  double? PowerW,
  double? MemoryUsedGB = null,
  double? MemoryTotalGB = null,
  double? MemoryClockMhz = null,
  double? FanRpm = null,
  double? CoreVoltageV = null,
  double? HotSpotTemperatureC = null,
  double? MemoryTemperatureC = null,
  IReadOnlyList<GpuEngineLoad>? EngineLoads = null,
  double? PcieRxMBps = null,
  double? PcieTxMBps = null,
  IReadOnlyList<GpuPowerRail>? PowerRails = null,
  double? TemperatureMinC = null,
  double? TemperatureMaxC = null,
  double? HotSpotTemperatureMinC = null,
  double? HotSpotTemperatureMaxC = null,
  double? MemoryTemperatureMinC = null,
  double? MemoryTemperatureMaxC = null,
  double? ClockMinMhz = null,
  double? ClockMaxMhz = null,
  double? MemoryClockMinMhz = null,
  double? MemoryClockMaxMhz = null,
  double? CoreVoltageMinV = null,
  double? CoreVoltageMaxV = null,
  double? PowerMinW = null,
  double? PowerMaxW = null);

/// <summary>
/// One poll of the GPU subsystem: the static adapter inventory (stable across
/// polls) paired with each adapter's current load.
/// </summary>
public sealed record GpuSnapshot(
  IReadOnlyList<GpuAdapterInfo> Adapters,
  IReadOnlyList<GpuLoadReading> Loads);
