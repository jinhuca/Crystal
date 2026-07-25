using Crystal.Mmi.HardwareFeatures.CurrentProbe;
using Crystal.Mmi.MmiEngine;
using Crystal.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Mmi.Tests.HardwareFeatures;

public class CurrentProbeExtensionsTests
{
    private static System.Collections.Frozen.FrozenDictionary<string, WmiValue> ProbeRow() => WmiRow.Build(
        ("Accuracy", new WmiValue(20)),
        ("Availability", new WmiValue(3)),
        ("Caption", new WmiValue("12V Rail Current")),
        ("ConfigManagerErrorCode", new WmiValue(0)),
        ("ConfigManagerUserConfig", new WmiValue(false)),
        ("CreationClassName", new WmiValue("Win32_CurrentProbe")),
        ("CurrentReading", new WmiValue(0)),
        ("Description", new WmiValue("12V Rail Current")),
        ("DeviceID", new WmiValue("CurrentProbe1")),
        ("ErrorCleared", new WmiValue(false)),
        ("ErrorDescription", new WmiValue("")),
        ("InstallDate", new WmiValue(new DateTime(2022, 11, 11, 0, 0, 0, DateTimeKind.Utc))),
        ("IsLinear", new WmiValue(true)),
        ("LastErrorCode", new WmiValue(0)),
        ("LowerThresholdCritical", new WmiValue(-1)),
        ("LowerThresholdFatal", new WmiValue(-1)),
        ("LowerThresholdNonCritical", new WmiValue(-1)),
        ("MaxReadable", new WmiValue(300)),
        ("MinReadable", new WmiValue(0)),
        ("Name", new WmiValue("12V Rail Current")),
        ("NominalReading", new WmiValue(120)),
        ("NormalMax", new WmiValue(200)),
        ("NormalMin", new WmiValue(0)),
        ("PNPDeviceID", new WmiValue("ACPI\\CURRENTPROBE\\1")),
        ("PowerManagementCapabilities", new WmiValue(new ushort[] { 1 })),
        ("PowerManagementSupported", new WmiValue(false)),
        ("Resolution", new WmiValue(1)),
        ("Status", new WmiValue("OK")),
        ("StatusInfo", new WmiValue(3)),
        ("SystemCreationClassName", new WmiValue("Win32_ComputerSystem")),
        ("SystemName", new WmiValue("DESKTOP-01")),
        ("Tolerance", new WmiValue(-1)),
        ("UpperThresholdCritical", new WmiValue(250)),
        ("UpperThresholdFatal", new WmiValue(280)),
        ("UpperThresholdNonCritical", new WmiValue(220))
    );

    [Fact]
    public async Task FullData_Maps_Name()
    {
        var provider = new FakeWmiProvider("Win32_CurrentProbe", new[] { ProbeRow() });
        var results = await provider.ToSafeCurrentProbeMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("12V Rail Current", results[0].Name);
    }

    [Fact]
    public async Task FullData_Maps_NominalReading_Int()
    {
        var provider = new FakeWmiProvider("Win32_CurrentProbe", new[] { ProbeRow() });
        var results = await provider.ToSafeCurrentProbeMetricsAsync(CancellationToken.None);

        Assert.Equal(120, results[0].NominalReading);
    }

    [Fact]
    public async Task FullData_Maps_UpperThresholdFatal_Int()
    {
        var provider = new FakeWmiProvider("Win32_CurrentProbe", new[] { ProbeRow() });
        var results = await provider.ToSafeCurrentProbeMetricsAsync(CancellationToken.None);

        Assert.Equal(280, results[0].UpperThresholdFatal);
    }

    [Fact]
    public async Task FullData_Maps_IsLinear_True()
    {
        var provider = new FakeWmiProvider("Win32_CurrentProbe", new[] { ProbeRow() });
        var results = await provider.ToSafeCurrentProbeMetricsAsync(CancellationToken.None);

        Assert.True(results[0].IsLinear);
    }

    [Fact]
    public async Task FullData_Maps_MinReadable_Zero()
    {
        var provider = new FakeWmiProvider("Win32_CurrentProbe", new[] { ProbeRow() });
        var results = await provider.ToSafeCurrentProbeMetricsAsync(CancellationToken.None);

        Assert.Equal(0, results[0].MinReadable);
    }

    [Fact]
    public async Task FullData_Maps_Status_OK()
    {
        var provider = new FakeWmiProvider("Win32_CurrentProbe", new[] { ProbeRow() });
        var results = await provider.ToSafeCurrentProbeMetricsAsync(CancellationToken.None);

        Assert.Equal("OK", results[0].Status);
    }

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("Win32_CurrentProbe", WmiRow.Empty());
        var results = await provider.ToSafeCurrentProbeMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MissingClass_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("SomeOtherClass", WmiRow.Empty());
        var results = await provider.ToSafeCurrentProbeMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MultipleProbes_Returns_All()
    {
        var p1 = WmiRow.Build(("DeviceID", new WmiValue("CurrentProbe1")));
        var p2 = WmiRow.Build(("DeviceID", new WmiValue("CurrentProbe2")));

        var provider = new FakeWmiProvider("Win32_CurrentProbe", new[] { p1, p2 });
        var results = await provider.ToSafeCurrentProbeMetricsAsync(CancellationToken.None);

        Assert.Equal(2, results.Count);
        Assert.Equal("CurrentProbe1", results[0].DeviceID);
        Assert.Equal("CurrentProbe2", results[1].DeviceID);
    }

    [Fact]
    public async Task PartialData_Leaves_Missing_Fields_Null()
    {
        var partial = WmiRow.Build(("DeviceID", new WmiValue("CurrentProbe3")));

        var provider = new FakeWmiProvider("Win32_CurrentProbe", new[] { partial });
        var results = await provider.ToSafeCurrentProbeMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("CurrentProbe3", results[0].DeviceID);
        Assert.Null(results[0].NominalReading);
        Assert.Null(results[0].UpperThresholdFatal);
    }

    [Fact]
    public async Task WrongTypeValue_Is_Ignored_Not_Miscast()
    {
        // NominalReading stored as a bool instead of Int — should be treated as absent, not throw.
        var badRow = WmiRow.Build(("NominalReading", new WmiValue(true)));

        var provider = new FakeWmiProvider("Win32_CurrentProbe", new[] { badRow });
        var results = await provider.ToSafeCurrentProbeMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Null(results[0].NominalReading);
    }
}
