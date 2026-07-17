using Crystal.Telemetry.Interop.PowerMonitor;

namespace Crystal.Telemetry.Hardware.PowerMonitor;

internal delegate float GetWireViewPro2SensorValue(DeviceData wvp);

internal class WireViewPro2Sensor : Sensor {
  readonly GetWireViewPro2SensorValue _getValue;

  public WireViewPro2Sensor(string name, int index, SensorType sensorType, Hardware hardware, ISettings settings, GetWireViewPro2SensorValue getValue)
      : base(name, index, sensorType, hardware, settings) {
    _getValue = getValue;
  }

  internal void Update(DeviceData wvp) {
    float value = _getValue(wvp);

    Value = value;
  }
}
