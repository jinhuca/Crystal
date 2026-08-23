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
  /// <summary>
  /// The Telemetry provider owns the lifetime of the hardware tree and the sensor readings. 
  /// We enable both CPU and GPU groups so we can read the CPU package temperature as a fallback 
  /// for Intel integrated GPUs that don't expose their own temperature sensor.
  /// </summary>
  private readonly Computer _computer;

  /// <summary>
  /// True if the object has been disposed; false otherwise.
  /// </summary>
  private bool _disposed;

  /// <summary>
  /// Initializes a new instance of the <see cref="GpuLoadSource"/> class, and opening the Telemetry provider.
  /// </summary>
  public GpuLoadSource() {
    // GPU groups depend on the CPU group being present to detect Intel integrated GPUs
    // (see Computer.IsGpuEnabled), so enable both.
    _computer = new Computer { IsCpuEnabled = true, IsGpuEnabled = true };
    _computer.Open();
  }

  /// <summary>
  /// Re-samples every GPU and returns one load + temperature + clock + power reading per adapter.
  /// </summary>
  public IReadOnlyList<GpuLoadReading> Read() {
    var readings = new List<GpuLoadReading>();
    foreach (var gpu in EnumerateGpus()) {
      gpu.Update();
      var temp = GpuSensorSelector.SelectCoreTemperature(gpu.Sensors)
        ?? (gpu.HardwareType == HardwareType.GpuIntel ? ReadCpuPackageTemperature() : null);
      readings.Add(new GpuLoadReading(
        AdapterName: gpu.Name,
        CoreLoadPercent: GpuSensorSelector.SelectCoreLoad(gpu.Sensors),
        TemperatureC: temp,
        ClockMhz: GpuSensorSelector.SelectCoreClock(gpu.Sensors),
        PowerW: GpuSensorSelector.SelectPackagePower(gpu.Sensors),
        MemoryUsedGB: GpuSensorSelector.SelectMemoryUsedGB(gpu.Sensors),
        MemoryTotalGB: GpuSensorSelector.SelectMemoryTotalGB(gpu.Sensors),
        MemoryClockMhz: GpuSensorSelector.SelectMemoryClock(gpu.Sensors),
        FanRpm: GpuSensorSelector.SelectFanRpm(gpu.Sensors),
        CoreVoltageV: GpuSensorSelector.SelectCoreVoltage(gpu.Sensors),
        HotSpotTemperatureC: GpuSensorSelector.SelectHotSpotTemperature(gpu.Sensors),
        MemoryTemperatureC: GpuSensorSelector.SelectMemoryTemperature(gpu.Sensors),
        EngineLoads: GpuSensorSelector.SelectEngineLoads(gpu.Sensors),
        PcieRxMBps: GpuSensorSelector.SelectPcieRxMBps(gpu.Sensors),
        PcieTxMBps: GpuSensorSelector.SelectPcieTxMBps(gpu.Sensors),
        PowerRails: GpuSensorSelector.SelectPowerRails(gpu.Sensors)));
    }
    return readings;
  }

  /// <summary>
  /// Reads the CPU package temperature as a fallback for Intel integrated GPUs.
  /// An Intel integrated GPU only exposes its own temperature sensor when IGCL telemetry is
  /// available; without it, the adapter reports no temperature at all. Since the iGPU shares the
  /// CPU die, the CPU package temperature is the standard proxy (what HWiNFO shows for the iGPU).
  /// We enable the CPU group anyway (for iGPU detection), so read it here as a fallback.
  /// </summary>
  /// <returns>The CPU package temperature in degrees Celsius, or null if not available.</returns>
  private double? ReadCpuPackageTemperature() {
    foreach (var cpu in _computer.Hardware.Where(h => h.HardwareType == HardwareType.Cpu)) {
      cpu.Update();
      if (GpuSensorSelector.SelectCpuPackageTemperature(cpu.Sensors) is { } v) {
        return v;
      }
    }
    return null;
  }

  /// <summary>
  /// Enumerates all GPU adapters in the hardware tree, including NVIDIA, AMD, and Intel.
  /// </summary>
  /// <returns></returns>
  private IEnumerable<IHardware> EnumerateGpus() =>
    _computer.Hardware.Where(h => h.HardwareType is HardwareType.GpuNvidia or HardwareType.GpuAmd or HardwareType.GpuIntel);

  /// <summary>
  /// Disposes the <see cref="GpuLoadSource"/> instance, closing the Telemetry provider and releasing resources.
  /// </summary>
  public void Dispose() {
    if (_disposed) return;
    _disposed = true;
    _computer.Close();
  }
}
