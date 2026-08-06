using Crystal.Provider.Mmi.HardwareFeatures.TemperatureProbe;
using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Provider.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Provider.Mmi.Tests.HardwareFeatures;

public class TemperatureProbeExtensionsTests
{
    private static System.Collections.Frozen.FrozenDictionary<string, WmiValue> ProbeRow() => WmiRow.Build(
        ("Accuracy", new WmiValue(50)),
        ("Availability", new WmiValue(3)),
        ("Caption", new WmiValue("CPU Temperature")),
        ("ConfigManagerErrorCode", new WmiValue(0)),
        ("ConfigManagerUserConfig", new WmiValue(false)),
        ("CreationClassName", new WmiValue("Win32_TemperatureProbe")),
        ("CurrentReading", new WmiValue(0)),
        ("Description", new WmiValue("CPU Temperature")),
        ("DeviceID", new WmiValue("TemperatureProbe1")),
        ("ErrorCleared", new WmiValue(false)),
        ("ErrorDescription", new WmiValue("")),
        ("InstallDate", new WmiValue(new DateTime(2022, 10, 5, 0, 0, 0, DateTimeKind.Utc))),
        ("IsLinear", new WmiValue(true)),
        ("LastErrorCode", new WmiValue(0)),
        ("LowerThresholdCritical", new WmiValue(-1)),
        ("LowerThresholdFatal", new WmiValue(-1)),
        ("LowerThresholdNonCritical", new WmiValue(-1)),
        ("MaxReadable", new WmiValue(1000)),
        ("MinReadable", new WmiValue(-1)),
        ("Name", new WmiValue("CPU Temperature")),
        ("NominalReading", new WmiValue(350)),
        ("NormalMax", new WmiValue(700)),
        ("NormalMin", new WmiValue(-1)),
        ("PNPDeviceID", new WmiValue("ACPI\\THERMALZONE\\1")),
        ("PowerManagementCapabilities", new WmiValue(new ushort[] { 1 })),
        ("PowerManagementSupported", new WmiValue(false)),
        ("Resolution", new WmiValue(1)),
        ("Status", new WmiValue("OK")),
        ("StatusInfo", new WmiValue(3)),
        ("SystemCreationClassName", new WmiValue("Win32_ComputerSystem")),
        ("SystemName", new WmiValue("DESKTOP-01")),
        ("Tolerance", new WmiValue(-1)),
        ("UpperThresholdCritical", new WmiValue(900)),
        ("UpperThresholdFatal", new WmiValue(1000)),
        ("UpperThresholdNonCritical", new WmiValue(800))
    );

    [Fact]
    public async Task FullData_Maps_Name()
    {
        var provider = new FakeWmiProvider("Win32_TemperatureProbe", new[] { ProbeRow() });
        var results = await provider.ToSafeTemperatureProbeMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("CPU Temperature", results[0].Name);
    }

    [Fact]
    public async Task FullData_Maps_UpperThresholdCritical_Int()
    {
        var provider = new FakeWmiProvider("Win32_TemperatureProbe", new[] { ProbeRow() });
        var results = await provider.ToSafeTemperatureProbeMetricsAsync(CancellationToken.None);

        Assert.Equal(900, results[0].UpperThresholdCritical);
    }

    [Fact]
    public async Task FullData_Maps_NominalReading_Int()
    {
        var provider = new FakeWmiProvider("Win32_TemperatureProbe", new[] { ProbeRow() });
        var results = await provider.ToSafeTemperatureProbeMetricsAsync(CancellationToken.None);

        Assert.Equal(350, results[0].NominalReading);
    }

    [Fact]
    public async Task FullData_Maps_IsLinear_True()
    {
        var provider = new FakeWmiProvider("Win32_TemperatureProbe", new[] { ProbeRow() });
        var results = await provider.ToSafeTemperatureProbeMetricsAsync(CancellationToken.None);

        Assert.True(results[0].IsLinear);
    }

    [Fact]
    public async Task FullData_Maps_MinReadable_NegativeInt()
    {
        var provider = new FakeWmiProvider("Win32_TemperatureProbe", new[] { ProbeRow() });
        var results = await provider.ToSafeTemperatureProbeMetricsAsync(CancellationToken.None);

        Assert.Equal(-1, results[0].MinReadable);
    }

    [Fact]
    public async Task FullData_Maps_Status_OK()
    {
        var provider = new FakeWmiProvider("Win32_TemperatureProbe", new[] { ProbeRow() });
        var results = await provider.ToSafeTemperatureProbeMetricsAsync(CancellationToken.None);

        Assert.Equal("OK", results[0].Status);
    }

    [Fact]
    public async Task FullData_Maps_InstallDate()
    {
        var provider = new FakeWmiProvider("Win32_TemperatureProbe", new[] { ProbeRow() });
        var results = await provider.ToSafeTemperatureProbeMetricsAsync(CancellationToken.None);

        Assert.Equal(new DateTime(2022, 10, 5, 0, 0, 0, DateTimeKind.Utc), results[0].InstallDate);
    }

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("Win32_TemperatureProbe", WmiRow.Empty());
        var results = await provider.ToSafeTemperatureProbeMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MissingClass_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("SomeOtherClass", WmiRow.Empty());
        var results = await provider.ToSafeTemperatureProbeMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MultipleProbes_Returns_All()
    {
        var probe1 = WmiRow.Build(("DeviceID", new WmiValue("TemperatureProbe1")));
        var probe2 = WmiRow.Build(("DeviceID", new WmiValue("TemperatureProbe2")));

        var provider = new FakeWmiProvider("Win32_TemperatureProbe", new[] { probe1, probe2 });
        var results = await provider.ToSafeTemperatureProbeMetricsAsync(CancellationToken.None);

        Assert.Equal(2, results.Count);
        Assert.Equal("TemperatureProbe1", results[0].DeviceID);
        Assert.Equal("TemperatureProbe2", results[1].DeviceID);
    }

    [Fact]
    public async Task PartialData_Leaves_Missing_Fields_Null()
    {
        var partial = WmiRow.Build(("DeviceID", new WmiValue("TemperatureProbe3")));

        var provider = new FakeWmiProvider("Win32_TemperatureProbe", new[] { partial });
        var results = await provider.ToSafeTemperatureProbeMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("TemperatureProbe3", results[0].DeviceID);
        Assert.Null(results[0].CurrentReading);
        Assert.Null(results[0].UpperThresholdCritical);
    }

    [Fact]
    public async Task WrongTypeValue_Is_Ignored_Not_Miscast()
    {
        // UpperThresholdCritical stored as a bool instead of Int — should be treated as absent, not throw.
        var badRow = WmiRow.Build(("UpperThresholdCritical", new WmiValue(true)));

        var provider = new FakeWmiProvider("Win32_TemperatureProbe", new[] { badRow });
        var results = await provider.ToSafeTemperatureProbeMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Null(results[0].UpperThresholdCritical);
    }
}
