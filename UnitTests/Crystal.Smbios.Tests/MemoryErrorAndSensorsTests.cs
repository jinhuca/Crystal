using System.Linq;
using Crystal.Smbios.Tests;
using static Crystal.Smbios.Tests.TestHelpers;
using Xunit;
using Crystal.Smbios.Structures;

namespace Crystal.Smbios.Tests;

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
        var s = MakeStructure(39, 0x3900, new byte[] { 0x01, 0x02, 0x03, 0x04, 0x20, 0x00 });
        var raw4 = MakeTable(s);
        var table4 = SmbiosTable.FromRawTableData(raw4);
        Assert.Single(table4.PowerSupplies);
        var ps = table4.PowerSupplies.First();
        Assert.Equal(0x01, ps.PowerUnitGroup);
        Assert.Equal((ushort)0x0020, ps.CapacityWatts);
    }
}
