using Xunit;
using Crystal.Smbios.Structures;
using Crystal.Smbios.Types;
using static Crystal.Smbios.Tests.TestHelpers;

namespace Crystal.Smbios.Tests;

public class SystemPowerControlsTests
{
    [Fact]
    public void Decode_PopulatesBcdFields()
    {
        var payload = new byte[] { 0x12, 0x25, 0x14, 0x30, 0x00 }; // Dec 25, 14:30:00

        var table = MakeTable(MakeStructure(25, 0x0170, payload));
        var smbios = SmbiosTable.FromRawTableData(table);

        var p = smbios.PowerControls;
        Assert.NotNull(p);
        Assert.Equal(12, p!.Month);
        Assert.Equal(25, p.DayOfMonth);
        Assert.Equal(14, p.Hour);
        Assert.Equal(30, p.Minute);
        Assert.Equal(0, p.Second);
    }

    [Fact]
    public void DecodeBcd_InvalidNibble_ReturnsNull()
    {
        Assert.Null(T025_SystemPowerControls.DecodeBcd(0x1A)); // 'A' nibble isn't valid BCD
        Assert.Equal(99, T025_SystemPowerControls.DecodeBcd(0x99));
    }
}
