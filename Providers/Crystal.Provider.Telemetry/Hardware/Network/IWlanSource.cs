#nullable enable
using System.Collections.Generic;

namespace Crystal.Provider.Telemetry.Hardware.Network;

/// <summary>
/// Reads the current Wi-Fi association state of every WLAN interface on the machine. Abstracted so
/// consumers can be tested against a fake without touching <c>wlanapi.dll</c>.
/// </summary>
public interface IWlanSource {
  /// <summary>
  /// Returns one <see cref="WlanReading"/> per WLAN interface, keyed by interface GUID. A machine
  /// with no wireless radio (or where the WLAN service is unavailable) returns an empty list.
  /// </summary>
  IReadOnlyList<WlanReading> Read();
}
