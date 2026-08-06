using Xunit;
using Crystal.Provider.Smbios.Structures;
using Crystal.Provider.Smbios.Types;
using static Crystal.Provider.Smbios.Tests.TestHelpers;

namespace Crystal.Provider.Smbios.Tests;

public class SystemBootInformationTests
{
    [Fact]
    public void Decode_KnownStatus_MapsToEnum()
    {
        var payload = new byte[7]; // 6 reserved bytes
        payload[6] = 5; // UserRequestedBoot

        var table = MakeTable(MakeStructure(32, 0x01B0, payload));
        var smbios = SmbiosTable.FromRawTableData(table);

        var b = smbios.BootInformation;
        Assert.NotNull(b);
        Assert.Equal((byte)5, b!.BootStatusRaw);
        Assert.Equal(SystemBootStatus.UserRequestedBoot, b.Status);
    }

    [Fact]
    public void Decode_OemSpecificStatus_StatusIsNull()
    {
        var payload = new byte[7];
        payload[6] = 0x81; // OEM-specific range

        var table = MakeTable(MakeStructure(32, 0x01B1, payload));
        var smbios = SmbiosTable.FromRawTableData(table);

        var b = smbios.SystemBootInformation[0];
        Assert.Equal((byte)0x81, b.BootStatusRaw);
        Assert.Null(b.Status);
    }
}
