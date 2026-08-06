using Xunit;
using Crystal.Provider.Smbios.Structures;
using Crystal.Provider.Smbios.Types;
using static Crystal.Provider.Smbios.Tests.TestHelpers;

namespace Crystal.Provider.Smbios.Tests;

public class IpmiDeviceInformationTests
{
    [Fact]
    public void Decode_PopulatesAllFields()
    {
        var payload = new byte[]
        {
            0x01,       // InterfaceType = Kcs
            0x20,       // IpmiSpecificationRevision = 2.0
            0x20,       // I2CSlaveAddress
            0xFF,       // NVStorageDeviceAddress = not present
            0xA3, 0x0C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // BaseAddress = 0x0CA3
            0x01,       // BaseAddressModifierAndInterruptInfo
            0x00,       // InterruptNumber
        };

        var table = MakeTable(MakeStructure(38, 0x0200, payload));
        var smbios = SmbiosTable.FromRawTableData(table);

        var i = smbios.IpmiDevices[0];
        Assert.Equal(BmcInterfaceType.Kcs, i.InterfaceType);
        Assert.Equal(2, i.IpmiSpecificationMajor);
        Assert.Equal(0, i.IpmiSpecificationMinor);
        Assert.Equal((byte)0x20, i.I2CSlaveAddress);
        Assert.False(i.HasNVStorage);
        Assert.Equal(0x0CA3ul, i.BaseAddress);
        Assert.Equal((byte)0x01, i.BaseAddressModifierAndInterruptInfo);
    }

    [Fact]
    public void Decode_NVStoragePresent()
    {
        var payload = new byte[]
        {
            0x03, 0x11, 0x10, 0x50,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00,
        };

        var table = MakeTable(MakeStructure(38, 0x0201, payload));
        var smbios = SmbiosTable.FromRawTableData(table);

        var i = smbios.IpmiDevices[0];
        Assert.Equal(BmcInterfaceType.Bt, i.InterfaceType);
        Assert.Equal((byte)0x50, i.NVStorageDeviceAddress);
        Assert.True(i.HasNVStorage);
    }
}
