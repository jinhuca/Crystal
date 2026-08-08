#nullable enable
using System;

namespace Crystal.Provider.Telemetry.Hardware.Network;

/// <summary>
/// A snapshot of one WLAN interface's radio state, keyed by the adapter's
/// <see cref="InterfaceGuid"/> (matches <see cref="System.Net.NetworkInformation.NetworkInterface.Id"/>).
/// One reading is produced per present WLAN interface regardless of association; <see cref="State"/>
/// says whether it is connected, idle, or radio-off. All connection-specific fields (SSID, signal,
/// rates, …) are null unless <see cref="State"/> is <see cref="WlanInterfaceState.Connected"/>.
/// </summary>
public sealed record WlanReading(
    Guid InterfaceGuid,
    WlanInterfaceState State,
    string? Ssid,
    int? SignalQualityPercent,
    int? RssiDbm,
    string? PhyType,
    int? ChannelNumber,
    string? Band,
    int? RxRateKbps,
    int? TxRateKbps,
    string? Bssid,
    string? Security);
