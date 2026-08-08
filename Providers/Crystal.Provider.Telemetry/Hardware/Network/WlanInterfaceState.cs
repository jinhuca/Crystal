#nullable enable

namespace Crystal.Provider.Telemetry.Hardware.Network;

/// <summary>
/// The coarse association state of a WLAN interface, distilled from the native
/// <c>WLAN_INTERFACE_STATE</c> enum. Only the distinctions the UI cares about are kept: whether the
/// radio is usable at all, and whether it is currently associated.
/// </summary>
public enum WlanInterfaceState {
  /// <summary>Radio is off or the interface isn't ready (airplane mode, disabled adapter).</summary>
  Disabled,

  /// <summary>Radio is on but not associated to an access point (idle, roaming, authenticating).</summary>
  Disconnected,

  /// <summary>Associated to an access point; connection attributes are available.</summary>
  Connected,
}
