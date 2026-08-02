using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Provider.Mmi.SoftwareFeatures.OSRecoveryConfiguration;
using Crystal.Provider.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Provider.Mmi.Tests.SoftwareFeatures;

public class OSRecoveryConfigurationExtensionsTests
{
    private static FakeWmiProvider FullRow() => new FakeWmiProvider("Win32_OSRecoveryConfiguration", WmiRow.Single(
        ("AutoReboot", new WmiValue(true)),
        ("Caption", new WmiValue("DESKTOP-01 - (C:\\Windows, C:\\)")),
        ("DebugFilePath", new WmiValue("%SystemRoot%\\MEMORY.DMP")),
        ("DebugInfoType", new WmiValue(1)),
        ("Description", new WmiValue("OS Recovery Settings")),
        ("ExpandedDebugFilePath", new WmiValue("C:\\WINDOWS\\MEMORY.DMP")),
        ("ExpandedMiniDumpDirectory", new WmiValue("C:\\WINDOWS\\Minidump")),
        ("KernelDumpOnly", new WmiValue(false)),
        ("MiniDumpDirectory", new WmiValue("%SystemRoot%\\Minidump")),
        ("Name", new WmiValue("DESKTOP-01|C:\\WINDOWS|1")),
        ("OverwriteExistingDebugFile", new WmiValue(true)),
        ("SendAdminAlert", new WmiValue(false)),
        ("SettingID", new WmiValue("")),
        ("WriteDebugInfo", new WmiValue(true)),
        ("WriteToSystemLog", new WmiValue(true))
    ));

    [Fact]
    public async Task FullData_Maps_AutoReboot_True()
    {
        var result = await FullRow().ToSafeOSRecoveryConfigurationMetricsAsync(CancellationToken.None);
        Assert.True(result.AutoReboot);
    }

    [Fact]
    public async Task FullData_Maps_DebugFilePath()
    {
        var result = await FullRow().ToSafeOSRecoveryConfigurationMetricsAsync(CancellationToken.None);
        Assert.Equal("%SystemRoot%\\MEMORY.DMP", result.DebugFilePath);
    }

    [Fact]
    public async Task FullData_Maps_DebugInfoType_Uint()
    {
        var result = await FullRow().ToSafeOSRecoveryConfigurationMetricsAsync(CancellationToken.None);
        Assert.Equal(1u, result.DebugInfoType);
    }

    [Fact]
    public async Task FullData_Maps_WriteToSystemLog_True()
    {
        var result = await FullRow().ToSafeOSRecoveryConfigurationMetricsAsync(CancellationToken.None);
        Assert.True(result.WriteToSystemLog);
    }

    [Fact]
    public async Task EmptyInstances_Returns_AllNull_Not_Throw()
    {
        var provider = new FakeWmiProvider("Win32_OSRecoveryConfiguration", WmiRow.Empty());
        var result = await provider.ToSafeOSRecoveryConfigurationMetricsAsync(CancellationToken.None);

        Assert.Null(result.AutoReboot);
        Assert.Null(result.DebugFilePath);
        Assert.Null(result.Name);
    }

    [Fact]
    public async Task MissingClass_Returns_AllNull_Not_Throw()
    {
        var provider = new FakeWmiProvider("SomeOtherClass", WmiRow.Empty());
        var result = await provider.ToSafeOSRecoveryConfigurationMetricsAsync(CancellationToken.None);

        Assert.Null(result.DebugFilePath);
    }

    [Fact]
    public async Task PartialData_Leaves_Missing_Fields_Null()
    {
        var provider = new FakeWmiProvider("Win32_OSRecoveryConfiguration",
            WmiRow.Single(("AutoReboot", new WmiValue(false))));
        var result = await provider.ToSafeOSRecoveryConfigurationMetricsAsync(CancellationToken.None);

        Assert.False(result.AutoReboot);
        Assert.Null(result.DebugFilePath);
        Assert.Null(result.MiniDumpDirectory);
    }

    [Fact]
    public async Task WrongTypeValue_Is_Ignored_Not_Miscast()
    {
        // AutoReboot stored as a String instead of Bool — should be treated as absent, not throw.
        var provider = new FakeWmiProvider("Win32_OSRecoveryConfiguration",
            WmiRow.Single(("AutoReboot", new WmiValue("true"))));
        var result = await provider.ToSafeOSRecoveryConfigurationMetricsAsync(CancellationToken.None);

        Assert.Null(result.AutoReboot);
    }
}
