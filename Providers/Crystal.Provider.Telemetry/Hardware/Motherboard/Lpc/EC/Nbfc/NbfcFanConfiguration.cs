namespace Crystal.Provider.Telemetry.Hardware.Motherboard.Lpc.EC.Nbfc;

/// <summary>
/// A single fan entry parsed from a NoteBook FanControl (NBFC) config. NBFC stores the raw
/// embedded-controller register that reports fan state plus the raw values that correspond to
/// 0% and 100% fan speed; it does not store tachometer RPM. This type carries the read-side
/// fields needed to turn a raw register reading into a fan-speed percentage.
/// </summary>
internal sealed class NbfcFanConfiguration {
  /// <summary>Gets or sets the embedded-controller register that reports the current fan state.</summary>
  public int ReadRegister { get; set; }

  /// <summary>Gets or sets the raw register value NBFC treats as minimum (0%) fan speed on the write side.</summary>
  public int MinSpeedValue { get; set; }

  /// <summary>Gets or sets the raw register value NBFC treats as maximum (100%) fan speed on the write side.</summary>
  public int MaxSpeedValue { get; set; }

  /// <summary>
  /// Gets or sets a value indicating whether the read register uses its own min/max scale
  /// (<see cref="MinSpeedValueRead"/>/<see cref="MaxSpeedValueRead"/>) distinct from the write scale.
  /// </summary>
  public bool IndependentReadMinMaxValues { get; set; }

  /// <summary>Gets or sets the raw read-register value corresponding to minimum (0%) fan speed.</summary>
  public int MinSpeedValueRead { get; set; }

  /// <summary>Gets or sets the raw read-register value corresponding to maximum (100%) fan speed.</summary>
  public int MaxSpeedValueRead { get; set; }

  /// <summary>Gets or sets the display name for this fan (e.g. "CPU"). May be empty.</summary>
  public string FanDisplayName { get; set; } = string.Empty;

  /// <summary>Gets the raw register value that maps to 0% for the read register, honoring <see cref="IndependentReadMinMaxValues"/>.</summary>
  public int EffectiveReadMin => IndependentReadMinMaxValues ? MinSpeedValueRead : MinSpeedValue;

  /// <summary>Gets the raw register value that maps to 100% for the read register, honoring <see cref="IndependentReadMinMaxValues"/>.</summary>
  public int EffectiveReadMax => IndependentReadMinMaxValues ? MaxSpeedValueRead : MaxSpeedValue;
}
