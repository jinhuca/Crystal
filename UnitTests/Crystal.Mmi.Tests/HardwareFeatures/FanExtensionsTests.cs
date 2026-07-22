using Crystal.Mmi.HardwareFeatures.Fan;
using Crystal.Mmi.MmiEngine;
using Crystal.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Mmi.Tests.HardwareFeatures;

public class FanExtensionsTests
{
    private static System.Collections.Frozen.FrozenDictionary<string, WmiValue> CpuFanRow() => WmiRow.Build(
        ("ActiveCooling", new WmiValue(true)),
        ("Availability", new WmiValue(3)),
        ("Caption", new WmiValue("CPU Fan")),
        ("ConfigManagerErrorCode", new WmiValue(0)),
        ("ConfigManagerUserConfig", new WmiValue(false)),
        ("CreationClassName", new WmiValue("Win32_Fan")),
        ("Description", new WmiValue("CPU Fan")),
        ("DesiredSpeed", new WmiValue(1800UL)),
        ("DeviceID", new WmiValue("Fan_1")),
        ("ErrorCleared", new WmiValue(false)),
        ("ErrorDescription", new WmiValue("")),
        ("InstallDate", new WmiValue(new DateTime(2023, 3, 1, 0, 0, 0, DateTimeKind.Utc))),
        ("LastErrorCode", new WmiValue(0)),
        ("Name", new WmiValue("CPU Fan")),
        ("PNPDeviceID", new WmiValue("ACPI\\PNP0C0B\\1")),
        ("PowerManagementCapabilities", new WmiValue(new ushort[] { 1 })),
        ("PowerManagementSupported", new WmiValue(false)),
        ("Status", new WmiValue("OK")),
        ("StatusInfo", new WmiValue(3)),
        ("SystemCreationClassName", new WmiValue("Win32_ComputerSystem")),
        ("SystemName", new WmiValue("DESKTOP-01")),
        ("VariableSpeed", new WmiValue(true))
    );

    [Fact]
    public async Task FullData_Maps_Name()
    {
        var provider = new FakeWmiProvider("Win32_Fan", new[] { CpuFanRow() });
        var results = await provider.ToSafeFanMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("CPU Fan", results[0].Name);
    }

    [Fact]
    public async Task FullData_Maps_ActiveCooling_True()
    {
        var provider = new FakeWmiProvider("Win32_Fan", new[] { CpuFanRow() });
        var results = await provider.ToSafeFanMetricsAsync(CancellationToken.None);

        Assert.True(results[0].ActiveCooling);
    }

    [Fact]
    public async Task FullData_Maps_VariableSpeed_True()
    {
        var provider = new FakeWmiProvider("Win32_Fan", new[] { CpuFanRow() });
        var results = await provider.ToSafeFanMetricsAsync(CancellationToken.None);

        Assert.True(results[0].VariableSpeed);
    }

    [Fact]
    public async Task FullData_Maps_DesiredSpeed_Ulong()
    {
        var provider = new FakeWmiProvider("Win32_Fan", new[] { CpuFanRow() });
        var results = await provider.ToSafeFanMetricsAsync(CancellationToken.None);

        Assert.Equal(1800UL, results[0].DesiredSpeed);
    }

    [Fact]
    public async Task FullData_Maps_PowerManagementCapabilities_Array()
    {
        var provider = new FakeWmiProvider("Win32_Fan", new[] { CpuFanRow() });
        var results = await provider.ToSafeFanMetricsAsync(CancellationToken.None);

        Assert.Equal(new ushort[] { 1 }, results[0].PowerManagementCapabilities);
    }

    [Fact]
    public async Task FullData_Maps_Status_OK()
    {
        var provider = new FakeWmiProvider("Win32_Fan", new[] { CpuFanRow() });
        var results = await provider.ToSafeFanMetricsAsync(CancellationToken.None);

        Assert.Equal("OK", results[0].Status);
    }

    [Fact]
    public async Task FullData_Maps_StatusInfo_Ushort()
    {
        var provider = new FakeWmiProvider("Win32_Fan", new[] { CpuFanRow() });
        var results = await provider.ToSafeFanMetricsAsync(CancellationToken.None);

        Assert.Equal((ushort)3, results[0].StatusInfo);
    }

    [Fact]
    public async Task FullData_Maps_Availability_Ushort()
    {
        var provider = new FakeWmiProvider("Win32_Fan", new[] { CpuFanRow() });
        var results = await provider.ToSafeFanMetricsAsync(CancellationToken.None);

        Assert.Equal((ushort)3, results[0].Availability);
    }

    [Fact]
    public async Task FullData_Maps_ConfigManagerErrorCode_Uint()
    {
        var provider = new FakeWmiProvider("Win32_Fan", new[] { CpuFanRow() });
        var results = await provider.ToSafeFanMetricsAsync(CancellationToken.None);

        Assert.Equal(0u, results[0].ConfigManagerErrorCode);
    }

    [Fact]
    public async Task FullData_Maps_InstallDate()
    {
        var provider = new FakeWmiProvider("Win32_Fan", new[] { CpuFanRow() });
        var results = await provider.ToSafeFanMetricsAsync(CancellationToken.None);

        Assert.Equal(new DateTime(2023, 3, 1, 0, 0, 0, DateTimeKind.Utc), results[0].InstallDate);
    }

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("Win32_Fan", WmiRow.Empty());
        var results = await provider.ToSafeFanMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MissingClass_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("SomeOtherClass", WmiRow.Empty());
        var results = await provider.ToSafeFanMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MultipleFans_Returns_All()
    {
        var fan1 = WmiRow.Build(("DeviceID", new WmiValue("Fan_1")), ("Name", new WmiValue("CPU Fan")));
        var fan2 = WmiRow.Build(("DeviceID", new WmiValue("Fan_2")), ("Name", new WmiValue("Case Fan")));

        var provider = new FakeWmiProvider("Win32_Fan", new[] { fan1, fan2 });
        var results = await provider.ToSafeFanMetricsAsync(CancellationToken.None);

        Assert.Equal(2, results.Count);
        Assert.Equal("CPU Fan", results[0].Name);
        Assert.Equal("Case Fan", results[1].Name);
    }

    [Fact]
    public async Task PartialData_Leaves_Missing_Fields_Null()
    {
        var partial = WmiRow.Build(("Name", new WmiValue("Fan Without Speed")));

        var provider = new FakeWmiProvider("Win32_Fan", new[] { partial });
        var results = await provider.ToSafeFanMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("Fan Without Speed", results[0].Name);
        Assert.Null(results[0].DesiredSpeed);
        Assert.Null(results[0].VariableSpeed);
        Assert.Null(results[0].PowerManagementCapabilities);
    }

    [Fact]
    public async Task WrongTypeValue_Is_Ignored_Not_Miscast()
    {
        // DesiredSpeed stored as a string instead of ULong — should be treated as absent, not throw.
        var badRow = WmiRow.Build(("DesiredSpeed", new WmiValue("not-a-number")));

        var provider = new FakeWmiProvider("Win32_Fan", new[] { badRow });
        var results = await provider.ToSafeFanMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Null(results[0].DesiredSpeed);
    }
}
