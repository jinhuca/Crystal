using System;

namespace Crystal.Provider.Smbios.Types;

/// <summary>
/// Type 22 — describes one portable (removable or built-in) battery.
/// Standalone structure — does not reference or get referenced by another
/// structure's handle.
///
/// SMBIOS only reports the battery's *design* characteristics (as printed on
/// the battery/reported by its firmware at manufacture time), not live state
/// like current charge or cycle count — those come from the OS battery API
/// (e.g. <c>System.Windows.Forms.SystemInformation.PowerStatus</c> or
/// <c>GetSystemPowerStatus</c>), not SMBIOS. This structure is useful as
/// static context (design capacity, chemistry, voltage) to pair with live
/// battery telemetry, not as a source of live battery state itself.
/// </summary>
public sealed class T022_PortableBatteryInformation : ISmbiosDecodedStructure
{
    public SmbiosStructureType StructureType { get; init; }
    public byte Length { get; init; }
    public ushort Handle { get; init; }

    /// <summary>Where the battery is located, e.g. "Front", "Rear", "Internal".</summary>
    public string? Location { get; init; }
    public string? Manufacturer { get; init; }
    /// <summary>Free-text manufacture date string (v2.1+); distinct from <see cref="SbdsManufactureDate"/>.</summary>
    public string? ManufactureDate { get; init; }
    public string? SerialNumber { get; init; }
    /// <summary>Device/model name, e.g. "DELL X1YT8".</summary>
    public string? DeviceName { get; init; }

    /// <summary>Battery chemistry; if <see cref="BatteryChemistry.Unknown"/>, check <see cref="SbdsDeviceChemistry"/>.</summary>
    public BatteryChemistry DeviceChemistry { get; init; }

    /// <summary>Raw Design Capacity field (v2.1+); combine with <see cref="DesignCapacityMultiplier"/> — use <see cref="DesignCapacityMilliwattHours"/> instead of this directly.</summary>
    public ushort DesignCapacityRaw { get; init; }
    /// <summary>Multiplier applied to <see cref="DesignCapacityRaw"/> (v2.2+); 0 is treated as 1 per spec.</summary>
    public byte DesignCapacityMultiplier { get; init; }

    /// <summary>Design voltage in mV (v2.1+); 0 = unknown.</summary>
    public ushort DesignVoltageMv { get; init; }

    /// <summary>Smart Battery Data Specification version string (v2.1+), e.g. "1.1".</summary>
    public string? SbdsVersionNumber { get; init; }
    /// <summary>Maximum error in battery data, as a percentage (v2.1+); 0xFF = unknown.</summary>
    public byte MaximumErrorPercent { get; init; }

    /// <summary>SBDS serial number (v2.2+); 0 if not SBDS-compliant.</summary>
    public ushort SbdsSerialNumber { get; init; }
    /// <summary>Raw packed SBDS manufacture date (v2.2+); use <see cref="SbdsManufactureDate"/> instead.</summary>
    public ushort SbdsManufactureDateRaw { get; init; }
    /// <summary>SBDS chemistry string (v2.2+) — authoritative when <see cref="DeviceChemistry"/> is Unknown.</summary>
    public string? SbdsDeviceChemistry { get; init; }
    /// <summary>OEM-specific bitfield (v2.2+).</summary>
    public uint OemSpecific { get; init; }

    /// <summary>
    /// Design capacity in mWh, or <see langword="null"/> if unknown (raw field is 0).
    /// Applies the v2.2+ multiplier: actual = <see cref="DesignCapacityRaw"/> ×
    /// (<see cref="DesignCapacityMultiplier"/> == 0 ? 1 : DesignCapacityMultiplier).
    /// </summary>
    public long? DesignCapacityMilliwattHours =>
        DesignCapacityRaw == 0
            ? null
            : DesignCapacityRaw * (long)(DesignCapacityMultiplier == 0 ? 1 : DesignCapacityMultiplier);

    /// <summary>
    /// Decoded SBDS manufacture date, or <see langword="null"/> if not present (raw field is 0).
    /// Packed as a DOS-style date: bits 0-4 = day, bits 5-8 = month, bits 9-15 = year offset from 1980.
    /// </summary>
    public DateOnly? SbdsManufactureDate
    {
        get
        {
            if (SbdsManufactureDateRaw == 0) return null;

            int day   = SbdsManufactureDateRaw & 0x1F;
            int month = (SbdsManufactureDateRaw >> 5) & 0x0F;
            int year  = 1980 + (SbdsManufactureDateRaw >> 9);

            // Guard against firmware writing an invalid packed value.
            if (day is < 1 or > 31 || month is < 1 or > 12) return null;

            try   { return new DateOnly(year, month, day); }
            catch (ArgumentOutOfRangeException) { return null; }
        }
    }

    internal static T022_PortableBatteryInformation Decode(SmbiosRawStructure s)
    {
        // DSP0134 §7.23 formatted-area layout:
        // 04 Location                     STRING
        // 05 Manufacturer                 STRING
        // 06 ManufactureDate               STRING  (v2.1+)
        // 07 SerialNumber                  STRING  (v2.1+)
        // 08 DeviceName                    STRING  (v2.1+)
        // 09 DeviceChemistry               BYTE    (v2.1+)
        // 0A DesignCapacity                WORD    (v2.1+)
        // 0C DesignVoltage                 WORD    (v2.1+)
        // 0E SBDSVersionNumber             STRING  (v2.1+)
        // 0F MaximumErrorInBatteryData     BYTE    (v2.1+)
        // 10 SBDSSerialNumber              WORD    (v2.2+)
        // 12 SBDSManufactureDate           WORD    (v2.2+)
        // 14 SBDSDeviceChemistry           STRING  (v2.2+)
        // 15 DesignCapacityMultiplier      BYTE    (v2.2+)
        // 16 OEMSpecific                   DWORD   (v2.2+)
        return new T022_PortableBatteryInformation
        {
            StructureType            = s.Type,
            Length                   = s.Length,
            Handle                   = s.Handle,
            Location                 = s.GetString(s.ReadByte(0x04)),
            Manufacturer             = s.GetString(s.ReadByte(0x05)),
            ManufactureDate          = s.Length > 0x06 ? s.GetString(s.ReadByte(0x06)) : null,
            SerialNumber             = s.Length > 0x07 ? s.GetString(s.ReadByte(0x07)) : null,
            DeviceName               = s.Length > 0x08 ? s.GetString(s.ReadByte(0x08)) : null,
            DeviceChemistry          = s.Length > 0x09 ? (BatteryChemistry)s.ReadByte(0x09) : BatteryChemistry.Unknown,
            DesignCapacityRaw        = s.Length > 0x0B ? s.ReadWord(0x0A) : (ushort)0,
            DesignVoltageMv          = s.Length > 0x0D ? s.ReadWord(0x0C) : (ushort)0,
            SbdsVersionNumber        = s.Length > 0x0E ? s.GetString(s.ReadByte(0x0E)) : null,
            MaximumErrorPercent      = s.Length > 0x0F ? s.ReadByte(0x0F) : (byte)0xFF,
            SbdsSerialNumber         = s.Length > 0x11 ? s.ReadWord(0x10) : (ushort)0,
            SbdsManufactureDateRaw   = s.Length > 0x13 ? s.ReadWord(0x12) : (ushort)0,
            SbdsDeviceChemistry      = s.Length > 0x14 ? s.GetString(s.ReadByte(0x14)) : null,
            DesignCapacityMultiplier = s.Length > 0x15 ? s.ReadByte(0x15) : (byte)0,
            OemSpecific              = s.Length > 0x19 ? s.ReadDWord(0x16) : 0u,
        };
    }
}
