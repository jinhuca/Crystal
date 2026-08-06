using System.Linq;
using Crystal.Provider.Smbios.Tests;
using static Crystal.Provider.Smbios.Tests.TestHelpers;
using Xunit;
using Crystal.Provider.Smbios.Structures;

namespace Crystal.Provider.Smbios.Tests;

public class MemoryErrorAndSensorsTests
{
    [Fact]
    public void Decode_MemoryError32_BasicFields()
    {
        var payload = MakeStructure(18, 0x1800, new byte[] { 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC });
        var raw = MakeTable(payload);
        var table = SmbiosTable.FromRawTableData(raw);
        var list = table.MemoryErrorInformation;
        Assert.Single(list);
        var obj = list.First();
        Assert.False(obj.Is64Bit);
        Assert.Equal(0x12, obj.ErrorType);
        Assert.Equal(0x34, obj.ErrorGranularity);
    }

    [Fact]
    public void Decode_MemoryError64_BasicFields()
    {
        // create a longer formatted area to be interpreted as 64-bit variant
        var header = new byte[] { 33, 0x18, 0, 0, 0x01, 0x02, 0x03, 0x04 };
        var payload = new byte[0x20 - 4];
        payload[0] = 0x01; payload[1] = 0x02;
        var structBytes = MakeStructure(33, 0x1801, payload);
        var raw2 = MakeTable(structBytes);
        var table2 = SmbiosTable.FromRawTableData(raw2);
        var list = table2.MemoryErrorInformation;
        Assert.Single(list);
        var obj = list.First();
        Assert.True(obj.Is64Bit);
        Assert.Equal(0x01, obj.ErrorType);
        Assert.Equal(0x02, obj.ErrorGranularity);
    }

    [Fact]
    public void Decode_MemoryError32_DecodesAddressesAndSyndrome()
    {
        // DSP0134 §7.19: Error Type @0x04, Granularity @0x05, Operation @0x06,
        // Vendor Syndrome DWORD @0x07, Array Error Address DWORD @0x0B,
        // Device Error Address DWORD @0x0F, Error Resolution DWORD @0x13.
        // Payload index = offset - 4; structure length = 0x17.
        var payload = new byte[0x17 - 4];
        payload[0x00] = 0x03; // ErrorType (Multi-bit)
        payload[0x01] = 0x04; // Granularity
        payload[0x02] = 0x05; // Operation
        payload[0x03] = 0x11; payload[0x04] = 0x22; payload[0x05] = 0x33; payload[0x06] = 0x44; // Syndrome @0x07
        payload[0x07] = 0x00; payload[0x08] = 0x10; payload[0x09] = 0x00; payload[0x0A] = 0x00; // ArrayErrAddr @0x0B = 0x1000
        payload[0x0B] = 0x00; payload[0x0C] = 0x20; payload[0x0D] = 0x00; payload[0x0E] = 0x00; // DeviceErrAddr @0x0F = 0x2000
        payload[0x0F] = 0x40; payload[0x10] = 0x00; payload[0x11] = 0x00; payload[0x12] = 0x00; // Resolution @0x13 = 0x40

        var table = SmbiosTable.FromRawTableData(MakeTable(MakeStructure(18, 0x1810, payload)));
        var obj = (Crystal.Provider.Smbios.Types.T018_MemoryErrorInformation32)table.MemoryErrorInformation.First();
        Assert.Equal(0x03, obj.ErrorType);
        Assert.Equal(0x04, obj.ErrorGranularity);
        Assert.Equal(0x05, obj.ErrorOperation);
        Assert.Equal(0x44332211u, obj.VendorSyndrome);
        Assert.Equal(0x1000u, obj.MemoryArrayErrorAddress);
        Assert.Equal(0x2000u, obj.DeviceErrorAddress);
        Assert.Equal(0x40u, obj.ErrorResolution);
    }

    [Fact]
    public void Decode_MemoryError64_DecodesQWordAddresses()
    {
        // DSP0134 §7.34: Array Error Address QWORD @0x0B, Device Error Address
        // QWORD @0x13, Error Resolution DWORD @0x1B. Structure length = 0x1F.
        var payload = new byte[0x1F - 4];
        payload[0x00] = 0x03; // ErrorType
        // Array Error Address QWORD @0x0B (payload index 0x07) = 0x0000_0001_0000_0000
        payload[0x07 + 4] = 0x01;
        // Device Error Address QWORD @0x13 (payload index 0x0F) = 0x0000_0002_0000_0000
        payload[0x0F + 4] = 0x02;
        // Error Resolution DWORD @0x1B (payload index 0x17) = 0x80
        payload[0x17] = 0x80;

        var table = SmbiosTable.FromRawTableData(MakeTable(MakeStructure(33, 0x1811, payload)));
        var obj = (Crystal.Provider.Smbios.Types.T033_MemoryErrorInformation64)table.MemoryErrorInformation.First();
        Assert.Equal(0x0000_0001_0000_0000UL, obj.MemoryArrayErrorAddress);
        Assert.Equal(0x0000_0002_0000_0000UL, obj.DeviceErrorAddress);
        Assert.Equal(0x80u, obj.ErrorResolution);
    }

    //[Fact]
    //public void Decode_CoolingDevice_And_TemperatureProbe()
    //{
    //    var cooling = MakeStructure(27, 0x2700, new byte[] { 0x05, 0x06, 0x10, 0x00, 0x00 });
    //    var probe = MakeStructure(28, 0x2800, new byte[] { 0x01, 0x02, 0x1E, 0x01 });
    //    var raw3 = MakeTable(cooling, probe);
    //    var table3 = SmbiosTable.FromRawTableData(raw3);
    //    Assert.Single(table3.CoolingDevices);
    //    Assert.Single(table3.TemperatureProbes);
    //    //var c = table3.CoolingDevices.First();
    //    //Assert.Equal(0x05, c.DeviceType.CoolingUnitType);
    //    var p = table3.TemperatureProbes.First();
    //    Assert.Equal(0x1E, p.CurrentTemperatureC);
    //}

    [Fact]
    public void Decode_SystemPowerSupply_Basic()
    {
        // Payload index = offset - 4. Layout:
        //   0x00 PowerUnitGroup, 0x01-0x07 string indices (Location..RevisionLevel),
        //   0x08 MaxPowerCapacity WORD (@0x0C), 0x0A Characteristics WORD (@0x0E),
        //   0x0C/0x0E/0x10 the three probe/cooling handles (@0x10/0x12/0x14).
        // Characteristics 0x00E2 = Present (bit1) | Type=Switching (bits5:3=0b100)
        //                          | Status=OK (bits8:6=0b011).
        var payload = new byte[]
        {
            0x01,             // PowerUnitGroup
            0x01,             // Location -> string 1
            0x02,             // DeviceName -> string 2
            0x03,             // Manufacturer -> string 3
            0x00,             // SerialNumber -> none
            0x00,             // AssetTagNumber -> none
            0x00,             // ModelPartNumber -> none
            0x00,             // RevisionLevel -> none
            0xF4, 0x01,       // MaxPowerCapacity = 500 W
            0xE2, 0x00,       // Characteristics
            0x10, 0x00,       // InputVoltageProbeHandle = 0x0010
            0x11, 0x00,       // CoolingDeviceHandle = 0x0011
            0x12, 0x00,       // InputCurrentProbeHandle = 0x0012
        };
        var s = MakeStructure(39, 0x3900, payload, new[] { "Slot 0", "PSU1", "Acme" });
        var raw4 = MakeTable(s);
        var table4 = SmbiosTable.FromRawTableData(raw4);
        Assert.Single(table4.PowerSupplies);
        var ps = table4.PowerSupplies.First();
        Assert.Equal(0x01, ps.PowerUnitGroup);
        Assert.Equal("Slot 0", ps.Location);
        Assert.Equal("PSU1", ps.DeviceName);
        Assert.Equal("Acme", ps.Manufacturer);
        Assert.Equal((ushort)500, ps.MaxPowerCapacityWatts);
        Assert.True(ps.IsMaxPowerKnown);
        Assert.True(ps.IsPresent);
        Assert.Equal(Crystal.Provider.Smbios.Types.PowerSupplyType.Switching, ps.SupplyType);
        Assert.Equal(Crystal.Provider.Smbios.Types.PowerSupplyStatus.OK, ps.Status);
        Assert.Equal((ushort)0x0010, ps.InputVoltageProbeHandle);
        Assert.Equal((ushort)0x0011, ps.CoolingDeviceHandle);
        Assert.Equal((ushort)0x0012, ps.InputCurrentProbeHandle);
    }
}
