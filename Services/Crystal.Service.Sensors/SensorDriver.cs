using Crystal.Provider.Telemetry.PawnIo;

namespace Crystal.Service.Sensors;

/// <summary>Reports whether the kernel driver that ring-0 sensors depend on is present. SuperIO /
/// motherboard, MSR voltage/power/temp readings are served only through PawnIO; when the driver is
/// absent those sensors silently report nothing, so callers surface this to explain an empty tile
/// rather than showing a wall of dashes.</summary>
public static class SensorDriver {
  /// <summary>True when the PawnIO kernel driver is installed (registry check only — does not imply
  /// it can be opened by this process).</summary>
  public static bool IsInstalled => PawnIo.IsInstalled;

  /// <summary>True when the PawnIO driver device can actually be opened right now. This is the
  /// authoritative "can we read ring-0 sensors" signal: it additionally requires the driver to be
  /// running and the process to be elevated.</summary>
  public static bool IsAccessible => PawnIo.IsAccessible;
}
