namespace Crystal.Provider.Mmi.HardwareFeatures.NetworkAdapter;

#nullable enable
using System;

public record NetworkAdapterMetrics(
    ushort? Availability,
    string? Caption,
    uint? ConfigManagerErrorCode,
    bool? ConfigManagerUserConfig,
    string? CreationClassName,
    string? Description,
    string? DeviceID,            // Key index identifier (e.g., "1", "2")
    bool? ErrorCleared,
    string? ErrorDescription,
    string? GUID,
    uint? Index,
    DateTime? InstallDate,
    bool? Installed,
    uint? InterfaceIndex,        // Maps matching items to IPv4 configuration classes
    string? LastErrorCode,
    string? MACAddress,          // Physical hardware hardware address marker
    string? Manufacturer,
    uint? MaxNumberControlled,
    uint? MaxSpeed,
    string? Name,
    string? NetConnectionID,     // The user-facing connection name (e.g., "Wi-Fi", "Ethernet")
    ushort? NetConnectionStatus, // 2 = Connected, 0 = Disconnected, 1 = Connecting
    bool? NetEnabled,
    string? PNPDeviceID,
    ushort[]? PowerManagementCapabilities,
    bool? PowerManagementSupported,
    string? ProductName,
    string? ProviderName,
    bool? PhysicalAdapter,       // Identifies physical hardware cards vs software tunnels
    string? ServiceName,
    ulong? Speed,                // Connection bandwidth speed in bits per second (uint64)
    string? Status,
    ushort? StatusInfo,
    string? SystemCreationClassName,
    string? SystemName,
    DateTime? TimeOfLastReset
) {
  // --- RUNTIME STATE CONVERTERS ---

  // Translates the numeric connection state into a human-readable status phrase
  public string ConnectionStatePhrase => NetConnectionStatus switch {
    0 => "Disconnected",
    1 => "Connecting...",
    2 => "Connected (Active)",
    3 => "Disconnecting...",
    7 => "Hardware Not Present",
    11 => "Authentication Failed",
    _ => "Idle / Sleeping"
  };

  // Computes connection link speed up into a clean megabits or gigabits presentation format
  public string FormattedLinkSpeed => Speed switch {
    null or 0 => "Unknown / Link Down",
    >= 1_000_000_000 => $"{Speed.Value / 1_000_000_000.0:F1} Gbps",
    _ => $"{Speed.Value / 1_000_000.0:F0} Mbps"
  };
}
