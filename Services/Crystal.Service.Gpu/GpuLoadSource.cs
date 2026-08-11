using Crystal.Provider.Telemetry.Hardware;

namespace Crystal.Service.Gpu;

/// <summary>
/// Reads live GPU core-load from the Telemetry provider (a LibreHardwareMonitor fork).
/// Every GPU vendor class exposes a "GPU Core" <see cref="SensorType.Load"/> sensor; we read
/// that per adapter and key it by the adapter's reported name so the view model can pair it
/// with the matching WMI inventory row.
/// <para>Core load does not require the ring-0 driver or elevation, unlike CPU MSR sensors.</para>
/// </summary>
public sealed class GpuLoadSource : IGpuLoadSource, IDisposable {
  private readonly Computer _computer;
  private bool _disposed;

  public GpuLoadSource() {
    // GPU groups depend on the CPU group being present to detect Intel integrated GPUs
    // (see Computer.IsGpuEnabled), so enable both.
    _computer = new Computer { IsCpuEnabled = true, IsGpuEnabled = true };
    _computer.Open();
  }

  /// <summary>Re-samples every GPU and returns one load + temperature + clock + power reading
  /// per adapter.</summary>
  public IReadOnlyList<GpuLoadReading> Read() {
    var readings = new List<GpuLoadReading>();
    foreach (var gpu in EnumerateGpus()) {
      gpu.Update();
      var temp = GpuSensorSelector.SelectCoreTemperature(gpu.Sensors)
                 ?? (gpu.HardwareType == HardwareType.GpuIntel ? ReadCpuPackageTemperature() : null);
      readings.Add(new GpuLoadReading(
          gpu.Name,
          GpuSensorSelector.SelectCoreLoad(gpu.Sensors),
          temp,
          GpuSensorSelector.SelectCoreClock(gpu.Sensors),
          GpuSensorSelector.SelectPackagePower(gpu.Sensors)));
    }
    return readings;
  }

  // An Intel integrated GPU only exposes its own temperature sensor when IGCL telemetry is
  // available; without it, the adapter reports no temperature at all. Since the iGPU shares the
  // CPU die, the CPU package temperature is the standard proxy (what HWiNFO shows for the iGPU).
  // We enable the CPU group anyway (for iGPU detection), so read it here as a fallback.
  private double? ReadCpuPackageTemperature() {
    foreach (var cpu in _computer.Hardware.Where(h => h.HardwareType == HardwareType.Cpu)) {
      cpu.Update();
      if (GpuSensorSelector.SelectCpuPackageTemperature(cpu.Sensors) is { } v) return v;
    }
    return null;
  }

  private IEnumerable<IHardware> EnumerateGpus() =>
      _computer.Hardware.Where(h => h.HardwareType is HardwareType.GpuNvidia
                                                    or HardwareType.GpuAmd
                                                    or HardwareType.GpuIntel);

  public void Dispose() {
    if (_disposed) return;
    _disposed = true;
    _computer.Close();
  }
}
