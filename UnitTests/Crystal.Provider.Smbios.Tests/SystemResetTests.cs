using Crystal.Provider.Smbios.Structures;
using Crystal.Provider.Smbios.Types;
using Xunit;
using static Crystal.Provider.Smbios.Tests.TestHelpers;

namespace Crystal.Provider.Smbios.Tests;

public class SystemResetTests
{
    [Fact]
    public void Decode_PopulatesCapabilitiesAndCounters()
    {
        // bit0 enabled | bits2:1 BootOption=OperatingSystem(1) | bits4:3 BootOptionOnLimit=DoNotReboot(3) | bit5 watchdog
        byte capabilities = (byte)(0x01 | (1 << 1) | (3 << 3) | 0x20);
        var payload = new byte[]
        {
            capabilities,
            0x05, 0x00, // ResetCount = 5
            0x0A, 0x00, // ResetLimit = 10
            0x1E, 0x00, // TimerInterval = 30
            0x05, 0x00, // Timeout = 5
        };

        var table = MakeTable(MakeStructure(23, 0x0150, payload));
        var smbios = SmbiosTable.FromRawTableData(table);

        var r = smbios.SystemResets[0];
        Assert.True(r.IsEnabled);
        Assert.True(r.HasWatchdogTimer);
        Assert.Equal(SystemResetBootOption.OperatingSystem, r.BootOption);
        Assert.Equal(SystemResetBootOption.DoNotReboot, r.BootOptionOnLimit);
        Assert.Equal((ushort)5, r.ResetCount);
        Assert.Equal((ushort)10, r.ResetLimit);
        Assert.Equal((ushort)30, r.TimerIntervalMinutes);
        Assert.Equal((ushort)5, r.TimeoutMinutes);
    }

    [Fact]
    public void Decode_DisabledNoWatchdog()
    {
        var payload = new byte[] { 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
        var table = MakeTable(MakeStructure(23, 0x0151, payload));
        var smbios = SmbiosTable.FromRawTableData(table);

        var r = smbios.SystemResets[0];
        Assert.False(r.IsEnabled);
        Assert.False(r.HasWatchdogTimer);
        Assert.Equal((ushort)0xFFFF, r.ResetCount);
    }
}
