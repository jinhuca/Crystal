namespace Crystal.Infrastructure.DataStructures.Sensors;

/// <summary>
/// Application-neutral sensor category. Mirrors the telemetry provider's own sensor-type
/// enumeration so the provider (a LibreHardwareMonitor fork) stays a standalone package with no
/// dependency on this layer; the telemetry-aware service maps provider values onto these at the
/// boundary. Member names are kept identical to the provider's so the mapping is 1:1.
/// </summary>
public enum SensorType {
  /// <summary>A voltage sensor, measured in volts (V).</summary>
  Voltage,

  /// <summary>An electric current sensor, measured in amperes (A).</summary>
  Current,

  /// <summary>A power sensor, measured in watts (W).</summary>
  Power,

  /// <summary>A clock frequency sensor, measured in megahertz (MHz).</summary>
  Clock,

  /// <summary>A temperature sensor, measured in degrees Celsius (°C).</summary>
  Temperature,

  /// <summary>A load sensor, measured as a percentage (%).</summary>
  Load,

  /// <summary>A frequency sensor, measured in hertz (Hz).</summary>
  Frequency,

  /// <summary>A fan speed sensor, measured in revolutions per minute (RPM).</summary>
  Fan,

  /// <summary>A flow rate sensor, measured in liters per hour (L/h).</summary>
  Flow,

  /// <summary>A control sensor, expressed as a percentage (%).</summary>
  Control,

  /// <summary>A level sensor, expressed as a percentage (%).</summary>
  Level,

  /// <summary>A dimensionless factor sensor.</summary>
  Factor,

  /// <summary>A data size sensor, measured in gigabytes (GB = 2^30 bytes).</summary>
  Data,

  /// <summary>A small data size sensor, measured in megabytes (MB = 2^20 bytes).</summary>
  SmallData,

  /// <summary>A throughput sensor, measured in bytes per second (B/s).</summary>
  Throughput,

  /// <summary>A duration sensor, measured in seconds.</summary>
  TimeSpan,

  /// <summary>A timing sensor, measured in nanoseconds (ns).</summary>
  Timing,

  /// <summary>An energy sensor, measured in milliwatt-hours (mWh).</summary>
  Energy,

  /// <summary>A noise sensor, measured in A-weighted decibels (dBA).</summary>
  Noise,

  /// <summary>A conductivity sensor, measured in microsiemens per centimeter (µS/cm).</summary>
  Conductivity,

  /// <summary>A humidity sensor, expressed as a percentage (%).</summary>
  Humidity
}
