namespace Crystal.Provider.Telemetry.Hardware.Controller.MSI;

/// <summary>
/// Defines the fan control modes supported by MSI motherboards.
/// </summary>
public enum MsiFanMode : byte {
  /// <summary>
  /// The fan runs in silent mode, prioritizing low noise.
  /// </summary>
  Silent = 0,

  /// <summary>
  /// Fan control is delegated to the system BIOS.
  /// </summary>
  Bios = 1,

  /// <summary>
  /// The fan runs in gaming mode, prioritizing cooling performance.
  /// </summary>
  Game = 2,

  /// <summary>
  /// The fan follows a user-defined custom curve.
  /// </summary>
  Custom = 3,

  /// <summary>
  /// The fan mode is unknown or unrecognized.
  /// </summary>
  Unknown = 4,

  /// <summary>
  /// The fan runs in smart mode, automatically balancing noise and cooling.
  /// </summary>
  Smart = 5,
}
