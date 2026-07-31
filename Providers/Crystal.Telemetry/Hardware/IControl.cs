namespace Crystal.Telemetry.Hardware;

/// <summary>
/// Describes how a control is being driven.
/// </summary>
public enum ControlMode {
  /// <summary>
  /// The control mode has not been determined.
  /// </summary>
  Undefined,
  /// <summary>
  /// The control is driven by a user-defined software value.
  /// </summary>
  Software,
  /// <summary>
  /// The control uses the hardware's default behavior.
  /// </summary>
  Default
}

/// <summary>
/// Represents a controllable hardware element, such as a fan, whose value can be
/// left at its default or overridden with a software value.
/// </summary>
public interface IControl {
  /// <summary>
  /// Gets the current control mode.
  /// </summary>
  ControlMode ControlMode { get; }

  /// <summary>
  /// Gets the identifier of this control.
  /// </summary>
  Identifier Identifier { get; }

  /// <summary>
  /// Gets the maximum value that can be set via software control.
  /// </summary>
  float MaxSoftwareValue { get; }

  /// <summary>
  /// Gets the minimum value that can be set via software control.
  /// </summary>
  float MinSoftwareValue { get; }

  /// <summary>
  /// Gets the sensor associated with this control.
  /// </summary>
  ISensor Sensor { get; }

  /// <summary>
  /// Gets the value currently applied through software control.
  /// </summary>
  float SoftwareValue { get; }

  /// <summary>
  /// Switches the control back to its hardware default behavior.
  /// </summary>
  void SetDefault();

  /// <summary>
  /// Sets a software-controlled value for this control.
  /// </summary>
  /// <param name="value">The value to apply.</param>
  void SetSoftware(float value);
}
