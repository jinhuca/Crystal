using System;
using System.Collections.Generic;

namespace Crystal.Telemetry.Hardware;

/// <summary>
/// Category of what type the selected sensor is.
/// </summary>
public enum SensorType {
  /// <summary>A voltage sensor, measured in volts (V).</summary>
  Voltage, // V

  /// <summary>An electric current sensor, measured in amperes (A).</summary>
  Current, // A

  /// <summary>A power sensor, measured in watts (W).</summary>
  Power, // W

  /// <summary>A clock frequency sensor, measured in megahertz (MHz).</summary>
  Clock, // MHz

  /// <summary>A temperature sensor, measured in degrees Celsius (°C).</summary>
  Temperature, // °C

  /// <summary>A load sensor, measured as a percentage (%).</summary>
  Load, // %

  /// <summary>A frequency sensor, measured in hertz (Hz).</summary>
  Frequency, // Hz

  /// <summary>A fan speed sensor, measured in revolutions per minute (RPM).</summary>
  Fan, // RPM

  /// <summary>A flow rate sensor, measured in liters per hour (L/h).</summary>
  Flow, // L/h

  /// <summary>A control sensor, expressed as a percentage (%).</summary>
  Control, // %

  /// <summary>A level sensor, expressed as a percentage (%).</summary>
  Level, // %

  /// <summary>A dimensionless factor sensor.</summary>
  Factor, // 1

  /// <summary>A data size sensor, measured in gigabytes (GB = 2^30 bytes).</summary>
  Data, // GB = 2^30 Bytes

  /// <summary>A small data size sensor, measured in megabytes (MB = 2^20 bytes).</summary>
  SmallData, // MB = 2^20 Bytes

  /// <summary>A throughput sensor, measured in bytes per second (B/s).</summary>
  Throughput, // B/s

  /// <summary>A duration sensor, measured in seconds.</summary>
  TimeSpan, // Seconds

  /// <summary>A timing sensor, measured in nanoseconds (ns).</summary>
  Timing, // ns

  /// <summary>An energy sensor, measured in milliwatt-hours (mWh).</summary>
  Energy, // milliwatt-hour (mWh)

  /// <summary>A noise sensor, measured in A-weighted decibels (dBA).</summary>
  Noise, // dBA

  /// <summary>A conductivity sensor, measured in microsiemens per centimeter (µS/cm).</summary>
  Conductivity, // µS/cm

  /// <summary>A humidity sensor, expressed as a percentage (%).</summary>
  Humidity // %
}

/// <summary>
/// Stores the readed value and the time in which it was recorded.
/// </summary>
public struct SensorValue {
  /// <param name="value"><see cref="Value"/> of the sensor.</param>
  /// <param name="time">The time code during which the <see cref="Value"/> was recorded.</param>
  public SensorValue(float value, DateTime time) {
    Value = value;
    Time = time;
  }

  /// <summary>
  /// Gets the value of the sensor
  /// </summary>
  public float Value { get; }

  /// <summary>
  /// Gets the time code during which the <see cref="Value"/> was recorded.
  /// </summary>
  public DateTime Time { get; }
}

/// <summary>
/// Stores information about the readed values and the time in which they were collected.
/// </summary>
public interface ISensor : IElement {
  /// <summary>
  /// Gets the control associated with this sensor, if any.
  /// </summary>
  IControl Control { get; }

  /// <summary>
  /// <inheritdoc cref="IHardware"/>
  /// </summary>
  IHardware Hardware { get; }

  /// <summary>
  /// Gets the unique identifier of this sensor.
  /// </summary>
  Identifier Identifier { get; }

  /// <summary>
  /// Gets the unique identifier of this sensor for a given <see cref="IHardware"/>.
  /// </summary>
  int Index { get; }

  /// <summary>
  /// Gets a value indicating whether this sensor is hidden by default.
  /// </summary>
  bool IsDefaultHidden { get; }

  /// <summary>
  /// Gets / sets a maximum value recorded for the given sensor.
  /// </summary>
  float? Max { get; set; }

  /// <summary>
  /// Gets / sets a minimum value recorded for the given sensor.
  /// </summary>
  float? Min { get; set; }

  /// <summary>
  /// Gets or sets a sensor name.
  /// <para>By default determined by the library.</para>
  /// </summary>
  string Name { get; set; }

  /// <summary>
  /// Gets the list of parameters that configure this sensor.
  /// </summary>
  IReadOnlyList<IParameter> Parameters { get; }

  /// <summary>
  /// <inheritdoc cref="Crystal.Telemetry.Hardware.SensorType"/>
  /// </summary>
  SensorType SensorType { get; }

  /// <summary>
  /// Gets the last recorded value for the given sensor.
  /// </summary>
  float? Value { get; }

  /// <summary>
  /// Gets a list of recorded values for the given sensor.
  /// </summary>
  IEnumerable<SensorValue> Values { get; }

  /// <summary>
  /// Gets or sets the time window over which recorded values are retained.
  /// </summary>
  TimeSpan ValuesTimeWindow { get; set; }

  /// <summary>
  /// Resets a value stored in <see cref="Min"/>.
  /// </summary>
  void ResetMin();

  /// <summary>
  /// Resets a value stored in <see cref="Max"/>.
  /// </summary>
  void ResetMax();

  /// <summary>
  /// Clears the values stored in <see cref="Values"/>.
  /// </summary>
  void ClearValues();
}
