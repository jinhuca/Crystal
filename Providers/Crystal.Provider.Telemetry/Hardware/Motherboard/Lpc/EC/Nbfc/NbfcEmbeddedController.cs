using System;
using System.Collections.Generic;
using System.Globalization;

namespace Crystal.Provider.Telemetry.Hardware.Motherboard.Lpc.EC.Nbfc;

/// <summary>
/// An <see cref="EmbeddedController"/> driven by a NoteBook FanControl (NBFC) config, for laptops
/// (typically HP/Lenovo) whose fan is behind the ACPI embedded controller rather than a SuperIO
/// chip. NBFC configs describe the raw read register and the raw values that map to 0%/100% fan
/// speed — not tachometer RPM — so each fan is normally exposed as a <see cref="SensorType.Control"/>
/// (%) sensor: the raw reading is linearly mapped onto 0–100% and clamped. As a Crystal extension, a
/// config may set <see cref="NbfcFanConfig.ReadValueIsRpm"/> for laptops (e.g. ThinkPads) whose EC
/// reports true RPM, in which case the fan is exposed as a <see cref="SensorType.Fan"/> (RPM) sensor
/// instead. Reuses the Windows ACPI-EC I/O of <see cref="WindowsEmbeddedController"/>.
/// </summary>
internal sealed class NbfcEmbeddedController : WindowsEmbeddedController {
  private NbfcEmbeddedController(IEnumerable<EmbeddedControllerSource> sources, ISettings settings) : base(sources, settings) { }

  /// <summary>
  /// Creates a controller for the given NBFC config, or <see langword="null"/> when the config
  /// yields no usable fan sources (missing fans, or degenerate min==max scales).
  /// </summary>
  /// <param name="config">The NBFC config matched to the current machine.</param>
  /// <param name="settings">Additional settings passed by the <see cref="IComputer"/>.</param>
  /// <returns>A new controller, or <see langword="null"/>.</returns>
  public static NbfcEmbeddedController Create(NbfcFanConfig config, ISettings settings) {
    if (config == null)
      return null;

    IReadOnlyList<EmbeddedControllerSource> sources = BuildSources(config);
    return sources.Count == 0 ? null : new NbfcEmbeddedController(sources, settings);
  }

  /// <summary>
  /// Converts an NBFC config's fan entries into embedded-controller sources that read a raw
  /// register and scale it onto a clamped 0–100% control value. Fans with a degenerate scale
  /// (identical min/max, which would divide by zero) are skipped.
  /// </summary>
  /// <param name="config">The NBFC config to convert.</param>
  /// <returns>The fan sources, possibly empty.</returns>
  public static IReadOnlyList<EmbeddedControllerSource> BuildSources(NbfcFanConfig config) {
    var sources = new List<EmbeddedControllerSource>();
    if (config == null)
      return sources;

    // NBFC word reads pack the value low-byte-first at the read register.
    byte size = config.ReadWriteWords ? (byte)2 : (byte)1;

    int index = 0;
    foreach (NbfcFanConfiguration fan in config.Fans) {
      index++;

      string name = string.IsNullOrWhiteSpace(fan.FanDisplayName)
          ? "Fan #" + index.ToString(CultureInfo.InvariantCulture)
          : fan.FanDisplayName;

      // When the EC reports true RPM (Crystal extension), surface the raw reading as an RPM fan
      // sensor unchanged; the min/max duty scale does not apply.
      if (config.ReadValueIsRpm) {
        sources.Add(new EmbeddedControllerSource(
            name,
            SensorType.Fan,
            (ushort)fan.ReadRegister,
            size,
            isLittleEndian: size == 2,
            clampMin: 0f));
        continue;
      }

      int min = fan.EffectiveReadMin;
      int max = fan.EffectiveReadMax;
      int span = max - min;
      if (span == 0)
        continue; // degenerate scale; cannot derive a percentage.

      // percentage = (raw - min) / (max - min) * 100  =>  raw * factor + offset
      float factor = 100f / span;
      float offset = -min * factor;

      sources.Add(new EmbeddedControllerSource(
          name,
          SensorType.Control,
          (ushort)fan.ReadRegister,
          size,
          factor,
          offset,
          isLittleEndian: size == 2,
          clampMin: 0f,
          clampMax: 100f));
    }

    return sources;
  }
}
