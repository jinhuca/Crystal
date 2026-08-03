using CpuModule.ViewModels.Interfaces;
using Crystal.Controls.PerformanceGraphs;
using Crystal.Infrastructure.DataStructures.Cpu.Interfaces;

namespace CpuModule.ViewModels.Implementations;

public sealed class CpuSensorsViewModel : BindableBase, ICpuSensorViewModel {
  private double _load;
  private double _voltage;
  private double _speedGhz;
  private double _power;
  private double _temperature;

  private PerformanceGraph? _utilizationGraph;
  private PerformanceGraph? _voltageGraph;
  private PerformanceGraph? _clockGraph;
  private PerformanceGraph? _powerGraph;
  private PerformanceGraph? _temperatureGraph;

  public double Load { get => _load; private set => SetProperty(ref _load, value); }
  public double Voltage { get => _voltage; private set => SetProperty(ref _voltage, value); }
  public double SpeedGhz { get => _speedGhz; private set => SetProperty(ref _speedGhz, value); }
  public double Power { get => _power; private set => SetProperty(ref _power, value); }
  public double Temperature { get => _temperature; private set => SetProperty(ref _temperature, value); }

  public void AttachGraphs(PerformanceGraph utilization, PerformanceGraph voltage,
                           PerformanceGraph clock, PerformanceGraph power, PerformanceGraph temperature) {
    _utilizationGraph = utilization;
    _voltageGraph = voltage;
    _clockGraph = clock;
    _powerGraph = power;
    _temperatureGraph = temperature;
  }

  public void Update(ISystemCpuInfo info) {
    var socket = info.Sockets.FirstOrDefault();
    if (socket is null) return;

    var sensors = socket.Sensors;

    Load = sensors.TotalLoad.Value ?? 0;
    Voltage = sensors.Voltage.Value ?? 0;
    // CpuSpeed reads in MHz; the Speed gauge/graph are scaled in GHz.
    SpeedGhz = (sensors.CpuSpeed.Value ?? 0) / 1000.0;
    Power = sensors.PackagePower.Value ?? 0;
    Temperature = sensors.PackageTemperature.Value ?? 0;

    _utilizationGraph?.AddValue(Load);
    _voltageGraph?.AddValue(Voltage);
    _clockGraph?.AddValue(SpeedGhz);
    _powerGraph?.AddValue(Power);
    _temperatureGraph?.AddValue(Temperature);
  }
}
