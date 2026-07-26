using Xunit;
using Crystal.Smbios.Structures;
using static Crystal.Smbios.Tests.TestHelpers;

namespace Crystal.Smbios.Tests;

public class ManagementDeviceComponentTests
{
    [Fact]
    public void Decode_NoThreshold_HasThresholdFalse()
    {
        var payload = new byte[]
        {
            1,          // Description string #1
            0x50, 0x00, // ManagementDeviceHandle = 0x0050
            0x26, 0x00, // ComponentHandle = 0x0026
            0xFF, 0xFF, // ThresholdHandle = 0xFFFF (none)
        };

        var table = MakeTable(MakeStructure(35, 0x01D0, payload, new[] { "CPU Temp Component" }));
        var smbios = SmbiosTable.FromRawTableData(table);

        var c = smbios.ManagementDeviceComponents[0];
        Assert.Equal("CPU Temp Component", c.Description);
        Assert.Equal((ushort)0x0050, c.ManagementDeviceHandle);
        Assert.Equal((ushort)0x0026, c.ComponentHandle);
        Assert.False(c.HasThreshold);
    }

    [Fact]
    public void Decode_WithThreshold_HasThresholdTrue()
    {
        var payload = new byte[] { 0, 0x50, 0x00, 0x26, 0x00, 0x60, 0x00 };
        var table = MakeTable(MakeStructure(35, 0x01D1, payload));
        var smbios = SmbiosTable.FromRawTableData(table);

        var c = smbios.ManagementDeviceComponents[0];
        Assert.Equal((ushort)0x0060, c.ThresholdHandle);
        Assert.True(c.HasThreshold);
    }
}
