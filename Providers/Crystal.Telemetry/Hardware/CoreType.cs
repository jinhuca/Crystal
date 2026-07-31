namespace Crystal.Telemetry.Hardware;

/// <summary>
/// Specifies the type of a processor core.
/// </summary>
public enum CoreType {
  /// <summary>
  /// The core type is unknown.
  /// </summary>
  Unknown = 0,

  /// <summary>
  /// A performance ("P") core.
  /// </summary>
  Performance = 0x40,

  /// <summary>
  /// An efficient ("E") core.
  /// </summary>
  Efficient = 0x20
}
