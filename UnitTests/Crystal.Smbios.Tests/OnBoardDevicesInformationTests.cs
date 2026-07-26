using Xunit;
using Crystal.Smbios.Structures;
using Crystal.Smbios.Types;
using static Crystal.Smbios.Tests.TestHelpers;

namespace Crystal.Smbios.Tests;

public class OnBoardDevicesInformationTests
{
    [Fact]
    public void Decode_PopulatesEnabledAndDisabledDeviceEntries()
    {
        var payload = new byte[]
        {
            0x83, 1, // enabled, Video, description string #1
            0x05, 2, // disabled, Ethernet, description string #2
        };

        var table = MakeTable(MakeStructure(10, 0x0120, payload, new[] { "Onboard Video", "Onboard LAN" }));
        var smbios = SmbiosTable.FromRawTableData(table);

        var d = smbios.LegacyOnBoardDevices[0];
        Assert.Equal(2, d.Devices.Count);

        Assert.True(d.Devices[0].IsEnabled);
        Assert.Equal(OnboardDeviceType.Video, d.Devices[0].DeviceType);
        Assert.Equal("Onboard Video", d.Devices[0].Description);

        Assert.False(d.Devices[1].IsEnabled);
        Assert.Equal(OnboardDeviceType.Ethernet, d.Devices[1].DeviceType);
        Assert.Equal("Onboard LAN", d.Devices[1].Description);
    }

    [Fact]
    public void Decode_NoDevices_EmptyList()
    {
        var table = MakeTable(MakeStructure(10, 0x0121, System.Array.Empty<byte>()));
        var smbios = SmbiosTable.FromRawTableData(table);

        Assert.Empty(smbios.LegacyOnBoardDevices[0].Devices);
    }
}
