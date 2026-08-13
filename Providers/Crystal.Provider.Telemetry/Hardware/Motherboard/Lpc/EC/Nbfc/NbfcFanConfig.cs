using System.Collections.Generic;

namespace Crystal.Provider.Telemetry.Hardware.Motherboard.Lpc.EC.Nbfc;

/// <summary>
/// A NoteBook FanControl (NBFC) config for one notebook model. NBFC ships one of these per
/// laptop model; it describes how to read (and write) the fan registers of that model's
/// embedded controller. Crystal only uses the read side to report fan-speed percentage.
/// </summary>
internal sealed class NbfcFanConfig {
  /// <summary>Gets or sets the notebook model string this config targets (matched against the SMBIOS system product name).</summary>
  public string NotebookModel { get; set; } = string.Empty;

  /// <summary>
  /// Gets or sets a value indicating whether registers are read/written as 16-bit words rather
  /// than single bytes.
  /// </summary>
  public bool ReadWriteWords { get; set; }

  /// <summary>
  /// Gets or sets a value indicating whether the read register already holds tachometer RPM rather
  /// than a value that must be mapped onto a 0-100% duty. This is a Crystal extension to the NBFC
  /// schema for laptops (e.g. ThinkPads) whose EC exposes true RPM: when set, the fan is surfaced as
  /// an RPM sensor and the min/max speed values are ignored on the read side.
  /// </summary>
  public bool ReadValueIsRpm { get; set; }

  /// <summary>Gets the fan entries described by this config.</summary>
  public IList<NbfcFanConfiguration> Fans { get; } = new List<NbfcFanConfiguration>();
}
