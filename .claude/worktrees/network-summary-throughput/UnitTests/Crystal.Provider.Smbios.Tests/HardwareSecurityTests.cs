using Xunit;
using Crystal.Provider.Smbios.Structures;
using Crystal.Provider.Smbios.Types;
using static Crystal.Provider.Smbios.Tests.TestHelpers;

namespace Crystal.Provider.Smbios.Tests;

public class HardwareSecurityTests
{
    [Fact]
    public void Decode_PopulatesAllFourStatuses()
    {
        // PowerOn(7:6)=Enabled(1), Keyboard(5:4)=Disabled(0), Admin(3:2)=NotImplemented(2), FrontPanel(1:0)=Unknown(3)
        byte raw = (byte)((1 << 6) | (0 << 4) | (2 << 2) | 3);
        var table = MakeTable(MakeStructure(24, 0x0160, new[] { raw }));
        var smbios = SmbiosTable.FromRawTableData(table);

        var h = smbios.HardwareSecurity;
        Assert.NotNull(h);
        Assert.Equal(HardwareSecurityStatus.Enabled, h!.PowerOnPasswordStatus);
        Assert.Equal(HardwareSecurityStatus.Disabled, h.KeyboardPasswordStatus);
        Assert.Equal(HardwareSecurityStatus.NotImplemented, h.AdministratorPasswordStatus);
        Assert.Equal(HardwareSecurityStatus.Unknown, h.FrontPanelResetStatus);
    }

    [Fact]
    public void Decode_AllDisabled()
    {
        var table = MakeTable(MakeStructure(24, 0x0161, new byte[] { 0x00 }));
        var smbios = SmbiosTable.FromRawTableData(table);

        var h = smbios.HardwareSecurityInformation[0];
        Assert.Equal(HardwareSecurityStatus.Disabled, h.PowerOnPasswordStatus);
        Assert.Equal(HardwareSecurityStatus.Disabled, h.KeyboardPasswordStatus);
        Assert.Equal(HardwareSecurityStatus.Disabled, h.AdministratorPasswordStatus);
        Assert.Equal(HardwareSecurityStatus.Disabled, h.FrontPanelResetStatus);
    }
}
