using Crystal.Mmi.HardwareFeatures.VoltageProbe;
using Crystal.Mmi.MmiEngine;
using Crystal.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Mmi.Tests.HardwareFeatures;

public class VoltageProbeExtensionsTests
{
    private static System.Collections.Frozen.FrozenDictionary<string, WmiValue> ProbeRow() => WmiRow.Build(
        ("Accuracy", new WmiValue(10)),
        ("Availability", new WmiValue(3)),
        ("Caption", new WmiValue("CPU Core Voltage")),
        ("ConfigManagerErrorCode", new WmiValue(0)),
        ("ConfigManagerUserConfig", new WmiValue(false)),
        ("CreationClassName", new WmiValue("Win32_VoltageProbe")),
        ("CurrentReading", new WmiValue(0)),
        ("Description", new WmiValue("CPU Core Voltage")),
        ("DeviceID", new WmiValue("VoltageProbe1")),
        ("ErrorCleared", new WmiValue(false)),
        ("ErrorDescription", new WmiValue("")),
        ("InstallDate", new WmiValue(new DateTime(2022, 12, 12, 0, 0, 0, DateTimeKind.Utc))),
        ("IsLinear", new WmiValue(true)),
        ("LastErrorCode", new WmiValue(0)),
        ("LowerThresholdCritical", new WmiValue(-1)),
        ("LowerThresholdFatal", new WmiValue(-1)),
        ("LowerThresholdNonCritical", new WmiValue(-1)),
        ("MaxReadable", new WmiValue(2000)),
        ("MinReadable", new WmiValue(0)),
        ("Name", new WmiValue("CPU Core Voltage")),
        ("NominalReading", new WmiValue(1200)),
        ("NormalMax", new WmiValue(1400)),
        ("NormalMin", new WmiValue(1000)),
        ("PNPDeviceID", new WmiValue("ACPI\\VOLTAGEPROBE\\1")),
        ("PowerManagementCapabilities", new WmiValue(new ushort[] { 1 })),
        ("PowerManagementSupported", new WmiValue(false)),
        ("Resolution", new WmiValue(1)),
        ("Status", new WmiValue("OK")),
        ("StatusInfo", new WmiValue(3)),
        ("SystemCreationClassName", new WmiValue("Win32_ComputerSystem")),
        ("SystemName", new WmiValue("DESKTOP-01")),
        ("Tolerance", new WmiValue(-1)),
        ("UpperThresholdCritical", new WmiValue(1450)),
        ("UpperThresholdFatal", new WmiValue(1500)),
        ("UpperThresholdNonCritical", new WmiValue(1400))
    );

    [Fact]
    public async Task FullData_Maps_Name()
    {
        var provider = new FakeWmiProvider("Win32_VoltageProbe", new[] { ProbeRow() });
        var results = await provider.ToSafeVoltageProbeMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("CPU Core Voltage", results[0].Name);
    }

    [Fact]
    public async Task FullData_Maps_NominalReading_Int()
    {
        var provider = new FakeWmiProvider("Win32_VoltageProbe", new[] { ProbeRow() });
        var results = await provider.ToSafeVoltageProbeMetricsAsync(CancellationToken.None);

        Assert.Equal(1200, results[0].NominalReading);
    }

    [Fact]
    public async Task FullData_Maps_NormalMax_Int()
    {
        var provider = new FakeWmiProvider("Win32_VoltageProbe", new[] { ProbeRow() });
        var results = await provider.ToSafeVoltageProbeMetricsAsync(CancellationToken.None);

        Assert.Equal(1400, results[0].NormalMax);
    }

    [Fact]
    public async Task FullData_Maps_IsLinear_True()
    {
        var provider = new FakeWmiProvider("Win32_VoltageProbe", new[] { ProbeRow() });
        var results = await provider.ToSafeVoltageProbeMetricsAsync(CancellationToken.None);

        Assert.True(results[0].IsLinear);
    }

    [Fact]
    public async Task FullData_Maps_Status_OK()
    {
        var provider = new FakeWmiProvider("Win32_VoltageProbe", new[] { ProbeRow() });
        var results = await provider.ToSafeVoltageProbeMetricsAsync(CancellationToken.None);

        Assert.Equal("OK", results[0].Status);
    }

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("Win32_VoltageProbe", WmiRow.Empty());
        var results = await provider.ToSafeVoltageProbeMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MissingClass_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("SomeOtherClass", WmiRow.Empty());
        var results = await provider.ToSafeVoltageProbeMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MultipleProbes_Returns_All()
    {
        var p1 = WmiRow.Build(("DeviceID", new WmiValue("VoltageProbe1")));
        var p2 = WmiRow.Build(("DeviceID", new WmiValue("VoltageProbe2")));

        var provider = new FakeWmiProvider("Win32_VoltageProbe", new[] { p1, p2 });
        var results = await provider.ToSafeVoltageProbeMetricsAsync(CancellationToken.None);

        Assert.Equal(2, results.Count);
        Assert.Equal("VoltageProbe1", results[0].DeviceID);
        Assert.Equal("VoltageProbe2", results[1].DeviceID);
    }

    [Fact]
    public async Task PartialData_Leaves_Missing_Fields_Null()
    {
        var partial = WmiRow.Build(("DeviceID", new WmiValue("VoltageProbe3")));

        var provider = new FakeWmiProvider("Win32_VoltageProbe", new[] { partial });
        var results = await provider.ToSafeVoltageProbeMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("VoltageProbe3", results[0].DeviceID);
        Assert.Null(results[0].NominalReading);
        Assert.Null(results[0].NormalMax);
    }

    [Fact]
    public async Task WrongTypeValue_Is_Ignored_Not_Miscast()
    {
        // NormalMax stored as a bool instead of Int — should be treated as absent, not throw.
        var badRow = WmiRow.Build(("NormalMax", new WmiValue(true)));

        var provider = new FakeWmiProvider("Win32_VoltageProbe", new[] { badRow });
        var results = await provider.ToSafeVoltageProbeMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Null(results[0].NormalMax);
    }
}
