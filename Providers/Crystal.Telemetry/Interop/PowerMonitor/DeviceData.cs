using System;
using System.Linq;

namespace Crystal.Telemetry.Interop.PowerMonitor;

/// <summary>
/// Represents a snapshot of telemetry data read from a power monitor device, including per-pin
/// voltage and current, temperatures, fault status, and device configuration.
/// </summary>
public sealed class DeviceData {
  /// <summary>
  /// Gets or sets the UTC time at which this data snapshot was captured.
  /// </summary>
  public DateTime Timestamp { get; set; } = DateTime.UtcNow;

  /// <summary>
  /// Gets or sets a value indicating whether the device is currently connected.
  /// </summary>
  public bool Connected { get; set; }

  /// <summary>
  /// Gets or sets the hardware revision identifier of the device.
  /// </summary>
  public string HardwareRevision { get; set; } = "A0";

  /// <summary>
  /// Gets or sets the firmware version string reported by the device.
  /// </summary>
  public string FirmwareVersion { get; set; } = "0.0.0";

  /// <summary>
  /// Gets or sets the measured voltage, in volts, for each monitored pin.
  /// </summary>
  public double[] PinVoltage { get; set; } = new double[6];

  /// <summary>
  /// Gets or sets the measured current, in amperes, for each monitored pin.
  /// </summary>
  public double[] PinCurrent { get; set; } = new double[6];

  /// <summary>
  /// Gets or sets the onboard inlet temperature, in degrees Celsius.
  /// </summary>
  public double OnboardTempInC { get; set; }

  /// <summary>
  /// Gets or sets the onboard outlet temperature, in degrees Celsius.
  /// </summary>
  public double OnboardTempOutC { get; set; }

  /// <summary>
  /// Gets or sets the first external temperature sensor reading, in degrees Celsius.
  /// </summary>
  public double ExternalTemp1C { get; set; }

  /// <summary>
  /// Gets or sets the second external temperature sensor reading, in degrees Celsius.
  /// </summary>
  public double ExternalTemp2C { get; set; }

  /// <summary>
  /// Gets or sets the rated power capability of the power supply unit, in watts.
  /// </summary>
  public int PsuCapabilityW { get; set; }

  /// <summary>
  /// Gets the total current, in amperes, summed across all monitored pins.
  /// </summary>
  public double SumCurrentA => PinCurrent.Sum();

  /// <summary>
  /// Gets the total power, in watts, computed as the sum of voltage times current for each monitored pin.
  /// </summary>
  public double SumPowerW => PinVoltage.Zip(PinCurrent, (v, i) => v * i).Sum();

  /// <summary>
  /// Gets or sets the current fault status flags reported by the device.
  /// </summary>
  public ushort FaultStatus { get; set; }

  /// <summary>
  /// Gets or sets the latched fault log flags reported by the device.
  /// </summary>
  public ushort FaultLog { get; set; }

  /// <summary>
  /// The device configuration structure associated with this snapshot.
  /// </summary>
  public DeviceConfigStructV3 Config;
}
