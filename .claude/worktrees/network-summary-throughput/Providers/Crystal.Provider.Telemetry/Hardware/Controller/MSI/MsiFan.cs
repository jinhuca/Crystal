using System;
using System.Runtime.InteropServices;

namespace Crystal.Provider.Telemetry.Hardware.Controller.MSI;

/// <summary>
/// Represents a single MSI fan, including its current speed and duty as well as the
/// duty and temperature curve configuration.
/// </summary>
public class MsiFan {
  /// <summary>
  /// Speed of Fan in RPM.
  /// </summary>
  public int Speed { get; set; }

  /// <summary>
  /// Current duty cycle of the fan in percentage 0-100.
  /// </summary>
  public int Duty { get; set; }

  /// <summary>
  /// Speed of Fan in percentage 0-100. This can e.g. be used to set fan curve when <see cref="MsiFanConfigure.Mode"/> is <see cref="MsiFanMode.Custom"/>.
  /// </summary>
  public MsiFanConfigure ConfigureDuty;

  /// <summary>
  /// Temperature of Fan in degrees Celsius. This can e.g. be used to set fan curve when <see cref="MsiFanConfigure.Mode"/> is <see cref="MsiFanMode.Custom"/>.
  /// </summary>
  public MsiFanConfigure ConfigureTemp;
}

/// <summary>
/// Represents an eight-byte fan curve configuration, consisting of a fan mode and
/// seven curve data points.
/// </summary>
public struct MsiFanConfigure {
  /// <summary>
  /// Initializes a new instance of the <see cref="MsiFanConfigure"/> struct and
  /// validates that its marshalled size matches the expected eight bytes.
  /// </summary>
  public MsiFanConfigure() {
    if (Marshal.SizeOf<MsiFanConfigure>() != 8) {
      throw new InvalidOperationException($"{nameof(MsiFanConfigure)} struct size is invalid.");
    }
  }

  /// <summary>The fan control mode.</summary>
  public MsiFanMode Mode = MsiFanMode.Unknown;

  /// <summary>The first fan curve data point.</summary>
  public byte Item0;

  /// <summary>The second fan curve data point.</summary>
  public byte Item1;

  /// <summary>The third fan curve data point.</summary>
  public byte Item2;

  /// <summary>The fourth fan curve data point.</summary>
  public byte Item3;

  /// <summary>The fifth fan curve data point.</summary>
  public byte Item4;

  /// <summary>The sixth fan curve data point.</summary>
  public byte Item5;

  /// <summary>The seventh fan curve data point.</summary>
  public byte Item6;
}
