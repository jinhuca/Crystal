using Xunit;
using Crystal.Smbios.Structures;
using Crystal.Smbios.Types;
using static Crystal.Smbios.Tests.TestHelpers;

namespace Crystal.Smbios.Tests;

public class MemoryChannelTests
{
    [Fact]
    public void Decode_PopulatesDeviceLoadsAndTotal()
    {
        var payload = new byte[]
        {
            0x03, // ChannelType = RamBus
            0x08, // MaximumChannelLoad
            0x02, // MemoryDeviceCount
            0x04, 0x11, 0x00, // DeviceLoad=4, handle 0x0011
            0x04, 0x12, 0x00, // DeviceLoad=4, handle 0x0012
        };

        var table = MakeTable(MakeStructure(37, 0x01F0, payload));
        var smbios = SmbiosTable.FromRawTableData(table);

        var c = smbios.MemoryChannels[0];
        Assert.Equal(MemoryChannelType.RamBus, c.ChannelType);
        Assert.Equal((byte)8, c.MaximumChannelLoad);
        Assert.Equal(2, c.Devices.Count);
        Assert.Equal((byte)4, c.Devices[0].DeviceLoad);
        Assert.Equal((ushort)0x0011, c.Devices[0].MemoryDeviceHandle);
        Assert.Equal((ushort)0x0012, c.Devices[1].MemoryDeviceHandle);
        Assert.Equal(8, c.TotalLoad);
    }

    [Fact]
    public void Decode_NoDevices_EmptyListZeroTotal()
    {
        var payload = new byte[] { 0x04, 0x08, 0x00 }; // SyncLink, load 8, 0 devices
        var table = MakeTable(MakeStructure(37, 0x01F1, payload));
        var smbios = SmbiosTable.FromRawTableData(table);

        var c = smbios.MemoryChannels[0];
        Assert.Equal(MemoryChannelType.SyncLink, c.ChannelType);
        Assert.Empty(c.Devices);
        Assert.Equal(0, c.TotalLoad);
    }
}
