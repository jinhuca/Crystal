#nullable enable
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Crystal.Provider.Telemetry.Hardware.Network;

/// <summary>
/// Reads the current Wi-Fi association state from the native Windows WLAN AutoConfig service
/// (<c>wlanapi.dll</c>). All native marshalling is contained here.
/// </summary>
/// <remarks>
/// wlanapi's list structs (<c>WLAN_INTERFACE_INFO_LIST</c>) are variable-length: a fixed-size
/// header followed by <c>dwNumberOfItems</c> inline elements. They can't be marshalled as a plain
/// struct, so we read the handle-owned buffer element-by-element by pointer arithmetic, then hand
/// every buffer back to <c>WlanFreeMemory</c>. Any failure (no radio, service stopped, access
/// denied) is swallowed and reported as "no readings" rather than thrown — Wi-Fi telemetry is
/// best-effort and its absence is normal on desktops.
/// </remarks>
public sealed class WlanSource : IWlanSource {
  private const uint ClientVersion = 2; // Vista+ WLAN API
  private const int ErrorSuccess = 0;
  private const uint IntfStateConnected = 1; // wlan_interface_state_connected
  private const uint OpcodeCurrentConnection = 7; // wlan_intf_opcode_current_connection
  private const uint OpcodeChannelNumber = 8; // wlan_intf_opcode_channel_number

  /// <inheritdoc/>
  public IReadOnlyList<WlanReading> Read() {
    nint clientHandle = 0;
    try {
      if (WlanOpenHandle(ClientVersion, 0, out _, out clientHandle) != ErrorSuccess)
        return [];

      return ReadInterfaces(clientHandle);
    }
    catch (Exception e) when (e is DllNotFoundException or EntryPointNotFoundException) {
      // wlanapi.dll ships with the "Windows Wireless LAN Service" feature; it can be absent on
      // Server SKUs. Treat as "no Wi-Fi" rather than a hard failure.
      return [];
    }
    finally {
      if (clientHandle != 0)
        WlanCloseHandle(clientHandle, 0);
    }
  }

  private static IReadOnlyList<WlanReading> ReadInterfaces(nint clientHandle) {
    if (WlanEnumInterfaces(clientHandle, 0, out nint listPtr) != ErrorSuccess || listPtr == 0)
      return [];

    try {
      // WLAN_INTERFACE_INFO_LIST: DWORD dwNumberOfItems; DWORD dwIndex; WLAN_INTERFACE_INFO[].
      int count = Marshal.ReadInt32(listPtr);
      if (count <= 0)
        return [];

      var readings = new List<WlanReading>(count);
      nint firstEntry = listPtr + (2 * sizeof(int)); // skip the two leading DWORDs
      int entrySize = Marshal.SizeOf<WlanInterfaceInfo>();

      for (int i = 0; i < count; i++) {
        var info = Marshal.PtrToStructure<WlanInterfaceInfo>(firstEntry + (i * entrySize));
        if (info.isState != IntfStateConnected)
          continue;

        var reading = ReadConnection(clientHandle, info.InterfaceGuid);
        if (reading is not null)
          readings.Add(reading);
      }

      return readings;
    }
    finally {
      WlanFreeMemory(listPtr);
    }
  }

  private static WlanReading? ReadConnection(nint clientHandle, Guid interfaceGuid) {
    nint dataPtr = 0;
    try {
      if (WlanQueryInterface(clientHandle, in interfaceGuid, OpcodeCurrentConnection,
                             0, out _, out dataPtr, out _) != ErrorSuccess || dataPtr == 0)
        return null;

      var conn = Marshal.PtrToStructure<WlanConnectionAttributes>(dataPtr);
      var assoc = conn.wlanAssociationAttributes;

      int quality = (int)assoc.wlanSignalQuality; // 0..100
      int? channel = ReadChannelNumber(clientHandle, interfaceGuid);

      return new WlanReading(
          InterfaceGuid: interfaceGuid,
          Ssid: DecodeSsid(assoc.dot11Ssid),
          SignalQualityPercent: quality,
          RssiDbm: QualityToRssi(quality),
          PhyType: DescribePhyType(assoc.dot11PhyType),
          ChannelNumber: channel,
          Band: DescribeBand(channel),
          // wlanapi reports link rates in units of 1 Kbps; 0 means "not reported".
          RxRateKbps: assoc.ulRxRate > 0 ? (int)assoc.ulRxRate : null,
          TxRateKbps: assoc.ulTxRate > 0 ? (int)assoc.ulTxRate : null,
          Bssid: FormatBssid(assoc.dot11Bssid),
          Security: DescribeSecurity(conn.wlanSecurityAttributes));
    }
    finally {
      if (dataPtr != 0)
        WlanFreeMemory(dataPtr);
    }
  }

  // The channel isn't part of WLAN_CONNECTION_ATTRIBUTES; it's a separate opcode that returns a
  // single ULONG. Returns null when unavailable (e.g. mid-roam) so band falls back to "—".
  private static int? ReadChannelNumber(nint clientHandle, Guid interfaceGuid) {
    nint dataPtr = 0;
    try {
      if (WlanQueryInterface(clientHandle, in interfaceGuid, OpcodeChannelNumber,
                             0, out _, out dataPtr, out _) != ErrorSuccess || dataPtr == 0)
        return null;
      int channel = Marshal.ReadInt32(dataPtr);
      return channel > 0 ? channel : null;
    }
    finally {
      if (dataPtr != 0)
        WlanFreeMemory(dataPtr);
    }
  }

  private static string? DecodeSsid(Dot11Ssid ssid) {
    if (ssid.SSIDLength == 0)
      return null;
    int len = (int)Math.Min(ssid.SSIDLength, 32u);
    return System.Text.Encoding.UTF8.GetString(ssid.SSID, 0, len);
  }

  // wlanapi reports signal quality as a linear 0..100 percentage. Windows derives that from RSSI as
  // a linear map where 0% == -100 dBm and 100% == -50 dBm; invert it for an approximate dBm.
  private static int QualityToRssi(int quality) =>
      -100 + (int)Math.Round(quality / 2.0);

  private static string DescribePhyType(uint phyType) => phyType switch {
    1 => "802.11 FHSS",
    2 => "802.11 DSSS",
    3 => "802.11 IR",
    4 => "802.11a/g (OFDM)",
    5 => "802.11b (HR-DSSS)",
    6 => "802.11g (ERP)",
    7 => "Wi-Fi 4 (802.11n)",
    8 => "Wi-Fi 5 (802.11ac)",
    9 => "Wi-Fi 6 (802.11ax)",
    10 => "Wi-Fi 7 (802.11be)",
    _ => "Unknown",
  };

  private static string? DescribeBand(int? channel) {
    if (channel is not { } c || c <= 0) return null;
    if (c <= 14) return "2.4 GHz";
    if (c <= 177) return "5 GHz";
    return "6 GHz";
  }

  // The BSSID is the associated AP's MAC address (6 bytes). All-zero means unassociated/unknown.
  private static string? FormatBssid(byte[] bssid) {
    if (bssid is not { Length: 6 }) return null;
    bool allZero = true;
    foreach (var b in bssid) {
      if (b != 0) { allZero = false; break; }
    }
    if (allZero) return null;
    return string.Join(':', Array.ConvertAll(bssid, b => b.ToString("X2")));
  }

  // Collapses the auth algorithm to the familiar marketing name. Open networks report no cipher;
  // everything else pairs the auth name with the cipher for a compact "WPA2-Personal / CCMP" label.
  private static string? DescribeSecurity(WlanSecurityAttributes security) {
    if (!security.bSecurityEnabled) return "Open";
    string auth = security.dot11AuthAlgorithm switch {
      1 => "Open",
      2 => "Shared",
      3 => "WPA",
      4 => "WPA-Personal",
      5 => "WPA-None",
      6 => "WPA2",
      7 => "WPA2-Personal",
      8 => "WPA3",           // DOT11_AUTH_ALGO_WPA3 (SAE variant folded in below)
      9 => "WPA3-Personal",  // DOT11_AUTH_ALGO_WPA3_SAE
      10 => "OWE",
      _ => "Secured",
    };
    string? cipher = security.dot11CipherAlgorithm switch {
      0x00 => null,          // none
      0x01 => "WEP-40",
      0x02 => "TKIP",
      0x04 => "CCMP",        // AES
      0x05 => "WEP-104",
      0x08 => "GCMP",
      0x09 => "GCMP-256",
      0x0A => "CCMP-256",
      _ => null,
    };
    return cipher is null ? auth : $"{auth} / {cipher}";
  }

  // ---- native interop ----------------------------------------------------------------------

  // CharSet.Unicode is required: strInterfaceDescription is a WCHAR[256] field, so ByValTStr must
  // marshal it as 512 bytes. The Sequential default (Ansi) would size it at 256 bytes and push
  // every following field — including isState — to the wrong offset.
  [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
  private struct WlanInterfaceInfo {
    public Guid InterfaceGuid;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
    public string strInterfaceDescription;
    public uint isState;
  }

  [StructLayout(LayoutKind.Sequential)]
  private struct Dot11Ssid {
    public uint SSIDLength;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
    public byte[] SSID;
  }

  [StructLayout(LayoutKind.Sequential)]
  private struct WlanAssociationAttributes {
    public Dot11Ssid dot11Ssid;
    public uint dot11BssType;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
    public byte[] dot11Bssid;
    public uint dot11PhyType;
    public uint uDot11PhyIndex;
    public uint wlanSignalQuality;
    public uint ulRxRate;
    public uint ulTxRate;
  }

  [StructLayout(LayoutKind.Sequential)]
  private struct WlanSecurityAttributes {
    [MarshalAs(UnmanagedType.Bool)] public bool bSecurityEnabled;
    [MarshalAs(UnmanagedType.Bool)] public bool bOneXEnabled;
    public uint dot11AuthAlgorithm;
    public uint dot11CipherAlgorithm;
  }

  // CharSet.Unicode: strProfileName is a WCHAR[256]; without it the ANSI default halves the field's
  // size and misaligns wlanAssociationAttributes (SSID/signal/PHY) that follow.
  [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
  private struct WlanConnectionAttributes {
    public uint isState;
    public uint wlanConnectionMode;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
    public string strProfileName;
    public WlanAssociationAttributes wlanAssociationAttributes;
    public WlanSecurityAttributes wlanSecurityAttributes;
  }

  [DllImport("wlanapi.dll")]
  private static extern int WlanOpenHandle(
      uint dwClientVersion, nint pReserved, out uint pdwNegotiatedVersion, out nint phClientHandle);

  [DllImport("wlanapi.dll")]
  private static extern int WlanCloseHandle(nint hClientHandle, nint pReserved);

  [DllImport("wlanapi.dll")]
  private static extern int WlanEnumInterfaces(
      nint hClientHandle, nint pReserved, out nint ppInterfaceList);

  [DllImport("wlanapi.dll")]
  private static extern int WlanQueryInterface(
      nint hClientHandle, in Guid pInterfaceGuid, uint OpCode, nint pReserved,
      out uint pdwDataSize, out nint ppData, out uint pWlanOpcodeValueType);

  [DllImport("wlanapi.dll")]
  private static extern void WlanFreeMemory(nint pMemory);
}
