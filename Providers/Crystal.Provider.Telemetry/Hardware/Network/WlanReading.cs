#nullable enable
using System;

namespace Crystal.Provider.Telemetry.Hardware.Network;

/// <summary>
/// A snapshot of one associated Wi-Fi interface's radio state, keyed by the adapter's
/// <see cref="InterfaceGuid"/> (matches <see cref="System.Net.NetworkInformation.NetworkInterface.Id"/>).
/// Every field beyond the key is optional: a wired NIC produces no reading at all, and a Wi-Fi NIC
/// that is enabled but not associated yields nulls for the connection-specific values.
/// </summary>
public sealed record WlanReading(
    Guid InterfaceGuid,
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
