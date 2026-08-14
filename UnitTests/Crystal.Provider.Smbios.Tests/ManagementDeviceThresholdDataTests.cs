using Crystal.Provider.Smbios.Structures;
using Xunit;
using static Crystal.Provider.Smbios.Tests.TestHelpers;

namespace Crystal.Provider.Smbios.Tests;

public class ManagementDeviceThresholdDataTests
{
    [Fact]
    public void Decode_MixedKnownAndUnknownThresholds()
    {
        var payload = new byte[]
        {
            0x00, 0x80, // LowerNonCritical = unknown (0x8000)
            0x88, 0x13, // UpperNonCritical = 5000
            0x00, 0x80, // LowerCritical = unknown
            0x70, 0x17, // UpperCritical = 6000
            0x00, 0x80, // LowerNonRecoverable = unknown
            0x58, 0x1B, // UpperNonRecoverable = 7000
        };

        var table = MakeTable(MakeStructure(36, 0x01E0, payload));
        var smbios = SmbiosTable.FromRawTableData(table);

        var t = smbios.ManagementDeviceThresholds[0];
        Assert.Equal((ushort)0x8000, t.LowerThresholdNonCritical);
        Assert.Equal((ushort)5000, t.UpperThresholdNonCritical);
        Assert.Equal((ushort)6000, t.UpperThresholdCritical);
        Assert.Equal((ushort)7000, t.UpperThresholdNonRecoverable);
        Assert.True(t.HasNonCriticalThresholds);
        Assert.True(t.HasCriticalThresholds);
        Assert.True(t.HasNonRecoverableThresholds);
    }

    [Fact]
    public void Decode_AllUnknown_HasFlagsAllFalse()
    {
        var payload = new byte[]
        {
            0x00, 0x80, 0x00, 0x80,
            0x00, 0x80, 0x00, 0x80,
            0x00, 0x80, 0x00, 0x80,
        };

        var table = MakeTable(MakeStructure(36, 0x01E1, payload));
        var smbios = SmbiosTable.FromRawTableData(table);

        var t = smbios.ManagementDeviceThresholds[0];
        Assert.False(t.HasNonCriticalThresholds);
        Assert.False(t.HasCriticalThresholds);
        Assert.False(t.HasNonRecoverableThresholds);
    }
}
