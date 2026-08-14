using Crystal.Provider.Smbios.Structures;
using Crystal.Provider.Smbios.Types;
using System;
using System.Linq;
using Xunit;
using static Crystal.Provider.Smbios.Tests.TestHelpers;

namespace Crystal.Provider.Smbios.Tests;

public class PortableBatteryInformationTests
{
    private static byte[] MakeFullPayload(
        BatteryChemistry chemistry = BatteryChemistry.LithiumIon,
        ushort designCapacityRaw = 4200,
        byte designCapacityMultiplier = 10,
        ushort designVoltageMv = 11400,
        byte maxErrorPercent = 3,
        ushort sbdsSerial = 0x1234,
        ushort sbdsManufactureDateRaw = 0,
        uint oemSpecific = 0)
    {
        var payload = new byte[0x1A - 4];
        payload[0x00] = 1; // Location
        payload[0x01] = 2; // Manufacturer
        payload[0x02] = 3; // ManufactureDate
        payload[0x03] = 4; // SerialNumber
        payload[0x04] = 5; // DeviceName
        payload[0x05] = (byte)chemistry;
        payload[0x06] = (byte)designCapacityRaw;
        payload[0x07] = (byte)(designCapacityRaw >> 8);
        payload[0x08] = (byte)designVoltageMv;
        payload[0x09] = (byte)(designVoltageMv >> 8);
        payload[0x0A] = 6; // SBDSVersionNumber
        payload[0x0B] = maxErrorPercent;
        payload[0x0C] = (byte)sbdsSerial;
        payload[0x0D] = (byte)(sbdsSerial >> 8);
        payload[0x0E] = (byte)sbdsManufactureDateRaw;
        payload[0x0F] = (byte)(sbdsManufactureDateRaw >> 8);
        payload[0x10] = 7; // SBDSDeviceChemistry
        payload[0x11] = designCapacityMultiplier;
        var oemBytes = BitConverter.GetBytes(oemSpecific);
        oemBytes.CopyTo(payload, 0x12);
        return payload;
    }

    [Fact]
    public void Decode_PopulatesAllFields()
    {
        var payload = MakeFullPayload(chemistry: BatteryChemistry.LithiumIon);
        var table   = MakeTable(MakeStructure(22, 0x0080, payload,
            new[] { "Front", "SIMPLO", "03/14/2024", "SN12345", "DELL X1YT8", "1.1", "LiP" }));
        var smbios  = SmbiosTable.FromRawTableData(table);

        var battery = smbios.Batteries.FirstOrDefault();
        Assert.NotNull(battery);
        Assert.Equal("Front", battery!.Location);
        Assert.Equal("SIMPLO", battery.Manufacturer);
        Assert.Equal("03/14/2024", battery.ManufactureDate);
        Assert.Equal("SN12345", battery.SerialNumber);
        Assert.Equal("DELL X1YT8", battery.DeviceName);
        Assert.Equal(BatteryChemistry.LithiumIon, battery.DeviceChemistry);
        Assert.Equal("1.1", battery.SbdsVersionNumber);
        Assert.Equal("LiP", battery.SbdsDeviceChemistry);
    }

    [Fact]
    public void DesignCapacityMilliwattHours_AppliesMultiplier()
    {
        // Raw 4200 × multiplier 10 = 42000 mWh — a realistic ~42 Wh laptop battery.
        var payload = MakeFullPayload(designCapacityRaw: 4200, designCapacityMultiplier: 10);
        var table   = MakeTable(MakeStructure(22, 0x0081, payload, new[] { "Front", "M", "D", "S", "N", "1.0", "LiP" }));
        var smbios  = SmbiosTable.FromRawTableData(table);

        Assert.Equal(42000L, smbios.Batteries.First().DesignCapacityMilliwattHours);
    }

    [Fact]
    public void DesignCapacityMilliwattHours_MultiplierZero_TreatedAsOne()
    {
        // v2.1-only structures have no multiplier field — must default to ×1, not ×0.
        var payload = MakeFullPayload(designCapacityRaw: 55000, designCapacityMultiplier: 0);
        var table   = MakeTable(MakeStructure(22, 0x0082, payload, new[] { "Front", "M", "D", "S", "N", "1.0", "LiP" }));
        var smbios  = SmbiosTable.FromRawTableData(table);

        Assert.Equal(55000L, smbios.Batteries.First().DesignCapacityMilliwattHours);
    }

    [Fact]
    public void DesignCapacityMilliwattHours_RawZero_ReturnsNull()
    {
        var payload = MakeFullPayload(designCapacityRaw: 0);
        var table   = MakeTable(MakeStructure(22, 0x0083, payload, new[] { "Front", "M", "D", "S", "N", "1.0", "LiP" }));
        var smbios  = SmbiosTable.FromRawTableData(table);

        Assert.Null(smbios.Batteries.First().DesignCapacityMilliwattHours);
    }

    [Fact]
    public void SbdsManufactureDate_DecodesPackedDate()
    {
        // Day=14, Month=3, Year=2024 → yearOffset = 2024-1980 = 44
        // packed = day | (month << 5) | (yearOffset << 9)
        int day = 14, month = 3, yearOffset = 44;
        ushort packed = (ushort)(day | (month << 5) | (yearOffset << 9));

        var payload = MakeFullPayload(sbdsManufactureDateRaw: packed);
        var table   = MakeTable(MakeStructure(22, 0x0084, payload, new[] { "Front", "M", "D", "S", "N", "1.0", "LiP" }));
        var smbios  = SmbiosTable.FromRawTableData(table);

        var date = smbios.Batteries.First().SbdsManufactureDate;
        Assert.NotNull(date);
        Assert.Equal(new DateOnly(2024, 3, 14), date);
    }

    [Fact]
    public void SbdsManufactureDate_RawZero_ReturnsNull()
    {
        var payload = MakeFullPayload(sbdsManufactureDateRaw: 0);
        var table   = MakeTable(MakeStructure(22, 0x0085, payload, new[] { "Front", "M", "D", "S", "N", "1.0", "LiP" }));
        var smbios  = SmbiosTable.FromRawTableData(table);

        Assert.Null(smbios.Batteries.First().SbdsManufactureDate);
    }

    [Fact]
    public void SbdsManufactureDate_InvalidDayOrMonth_ReturnsNullNotThrows()
    {
        // Month = 13 (invalid) — day=1, month=13, yearOffset=0
        ushort packed = (ushort)(1 | (13 << 5) | (0 << 9));
        var payload   = MakeFullPayload(sbdsManufactureDateRaw: packed);
        var table     = MakeTable(MakeStructure(22, 0x0086, payload, new[] { "Front", "M", "D", "S", "N", "1.0", "LiP" }));
        var smbios    = SmbiosTable.FromRawTableData(table);

        Assert.Null(smbios.Batteries.First().SbdsManufactureDate);
    }

    [Fact]
    public void Decode_LegacyV21Structure_NoSbdsFields_UsesDefaults()
    {
        // A v2.1-era structure — stops right after MaximumErrorInBatteryData (offset 0x10 total length).
        var payload = new byte[0x10 - 4];
        payload[0x00] = 1;
        payload[0x01] = 2;
        payload[0x05] = (byte)BatteryChemistry.NickelMetalHydride;
        payload[0x06] = 0x88; payload[0x07] = 0x13; // DesignCapacity = 0x1388 = 5000
        payload[0x0B] = 5; // MaximumErrorPercent

        var table  = MakeTable(MakeStructure(22, 0x0087, payload, new[] { "Rear", "Sanyo" }));
        var smbios = SmbiosTable.FromRawTableData(table);

        var battery = smbios.Batteries.First();
        Assert.Equal(BatteryChemistry.NickelMetalHydride, battery.DeviceChemistry);
        Assert.Equal(5000, battery.DesignCapacityRaw);
        Assert.Equal(5000L, battery.DesignCapacityMilliwattHours); // multiplier defaults to 1
        Assert.Null(battery.SbdsDeviceChemistry);
        Assert.Equal(0, battery.SbdsSerialNumber);
        Assert.Null(battery.SbdsManufactureDate);
    }

    [Fact]
    public void Decode_VeryMinimalV20Structure_OnlyLocationAndManufacturer()
    {
        // Some ancient boards report only Location + Manufacturer (6-byte structure).
        var payload = new byte[0x06 - 4];
        payload[0x00] = 1;
        payload[0x01] = 2;

        var table  = MakeTable(MakeStructure(22, 0x0088, payload, new[] { "Internal", "Generic" }));
        var smbios = SmbiosTable.FromRawTableData(table);

        var battery = smbios.Batteries.First();
        Assert.Equal("Internal", battery.Location);
        Assert.Equal("Generic", battery.Manufacturer);
        Assert.Null(battery.ManufactureDate);
        Assert.Null(battery.DesignCapacityMilliwattHours);
        Assert.Equal(BatteryChemistry.Unknown, battery.DeviceChemistry);
    }
}
