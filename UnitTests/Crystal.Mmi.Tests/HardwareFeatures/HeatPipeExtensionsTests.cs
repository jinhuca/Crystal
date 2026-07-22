using Crystal.Mmi.HardwareFeatures.HeatPipe;
using Crystal.Mmi.MmiEngine;
using Crystal.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Mmi.Tests.HardwareFeatures;

public class HeatPipeExtensionsTests
{
    private static System.Collections.Frozen.FrozenDictionary<string, WmiValue> HeatPipeRow() => WmiRow.Build(
        ("ActiveCooling", new WmiValue(false)),
        ("Availability", new WmiValue(3)),
        ("Caption", new WmiValue("Heat Pipe")),
        ("ConfigManagerErrorCode", new WmiValue(0)),
        ("ConfigManagerUserConfig", new WmiValue(false)),
        ("CreationClassName", new WmiValue("Win32_HeatPipe")),
        ("Description", new WmiValue("Heat Pipe")),
        ("DeviceID", new WmiValue("HeatPipe_1")),
        ("ErrorCleared", new WmiValue(false)),
        ("ErrorDescription", new WmiValue("")),
        ("InstallDate", new WmiValue(new DateTime(2023, 3, 1, 0, 0, 0, DateTimeKind.Utc))),
        ("LastErrorCode", new WmiValue(0)),
        ("Name", new WmiValue("Heat Pipe")),
        ("PNPDeviceID", new WmiValue("ACPI\\PNP0C0C\\1")),
        ("PowerManagementCapabilities", new WmiValue(new ushort[] { 1 })),
        ("PowerManagementSupported", new WmiValue(false)),
        ("Status", new WmiValue("OK")),
        ("StatusInfo", new WmiValue(3)),
        ("SystemCreationClassName", new WmiValue("Win32_ComputerSystem")),
        ("SystemName", new WmiValue("DESKTOP-01"))
    );

    [Fact]
    public async Task FullData_Maps_Name()
    {
        var provider = new FakeWmiProvider("Win32_HeatPipe", new[] { HeatPipeRow() });
        var results = await provider.ToSafeHeatPipeMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("Heat Pipe", results[0].Name);
    }

    [Fact]
    public async Task FullData_Maps_DeviceID()
    {
        var provider = new FakeWmiProvider("Win32_HeatPipe", new[] { HeatPipeRow() });
        var results = await provider.ToSafeHeatPipeMetricsAsync(CancellationToken.None);

        Assert.Equal("HeatPipe_1", results[0].DeviceID);
    }

    [Fact]
    public async Task FullData_Maps_ActiveCooling_False()
    {
        var provider = new FakeWmiProvider("Win32_HeatPipe", new[] { HeatPipeRow() });
        var results = await provider.ToSafeHeatPipeMetricsAsync(CancellationToken.None);

        Assert.False(results[0].ActiveCooling);
    }

    [Fact]
    public async Task FullData_Maps_PowerManagementCapabilities_Array()
    {
        var provider = new FakeWmiProvider("Win32_HeatPipe", new[] { HeatPipeRow() });
        var results = await provider.ToSafeHeatPipeMetricsAsync(CancellationToken.None);

        Assert.Equal(new ushort[] { 1 }, results[0].PowerManagementCapabilities);
    }

    [Fact]
    public async Task FullData_Maps_Status_OK()
    {
        var provider = new FakeWmiProvider("Win32_HeatPipe", new[] { HeatPipeRow() });
        var results = await provider.ToSafeHeatPipeMetricsAsync(CancellationToken.None);

        Assert.Equal("OK", results[0].Status);
    }

    [Fact]
    public async Task FullData_Maps_StatusInfo_Ushort()
    {
        var provider = new FakeWmiProvider("Win32_HeatPipe", new[] { HeatPipeRow() });
        var results = await provider.ToSafeHeatPipeMetricsAsync(CancellationToken.None);

        Assert.Equal((ushort)3, results[0].StatusInfo);
    }

    [Fact]
    public async Task FullData_Maps_Availability_Ushort()
    {
        var provider = new FakeWmiProvider("Win32_HeatPipe", new[] { HeatPipeRow() });
        var results = await provider.ToSafeHeatPipeMetricsAsync(CancellationToken.None);

        Assert.Equal((ushort)3, results[0].Availability);
    }

    [Fact]
    public async Task FullData_Maps_InstallDate()
    {
        var provider = new FakeWmiProvider("Win32_HeatPipe", new[] { HeatPipeRow() });
        var results = await provider.ToSafeHeatPipeMetricsAsync(CancellationToken.None);

        Assert.Equal(new DateTime(2023, 3, 1, 0, 0, 0, DateTimeKind.Utc), results[0].InstallDate);
    }

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("Win32_HeatPipe", WmiRow.Empty());
        var results = await provider.ToSafeHeatPipeMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MissingClass_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("SomeOtherClass", WmiRow.Empty());
        var results = await provider.ToSafeHeatPipeMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MultipleHeatPipes_Returns_All()
    {
        var pipe1 = WmiRow.Build(("DeviceID", new WmiValue("HeatPipe_1")), ("Name", new WmiValue("Heat Pipe 1")));
        var pipe2 = WmiRow.Build(("DeviceID", new WmiValue("HeatPipe_2")), ("Name", new WmiValue("Heat Pipe 2")));

        var provider = new FakeWmiProvider("Win32_HeatPipe", new[] { pipe1, pipe2 });
        var results = await provider.ToSafeHeatPipeMetricsAsync(CancellationToken.None);

        Assert.Equal(2, results.Count);
        Assert.Equal("Heat Pipe 1", results[0].Name);
        Assert.Equal("Heat Pipe 2", results[1].Name);
    }

    [Fact]
    public async Task PartialData_Leaves_Missing_Fields_Null()
    {
        var partial = WmiRow.Build(("Name", new WmiValue("Bare Heat Pipe")));

        var provider = new FakeWmiProvider("Win32_HeatPipe", new[] { partial });
        var results = await provider.ToSafeHeatPipeMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("Bare Heat Pipe", results[0].Name);
        Assert.Null(results[0].ActiveCooling);
        Assert.Null(results[0].PowerManagementCapabilities);
        Assert.Null(results[0].InstallDate);
    }

    [Fact]
    public async Task WrongTypeValue_Is_Ignored_Not_Miscast()
    {
        // Status stored as a bool instead of string — should be treated as absent, not throw.
        var badRow = WmiRow.Build(("Status", new WmiValue(true)));

        var provider = new FakeWmiProvider("Win32_HeatPipe", new[] { badRow });
        var results = await provider.ToSafeHeatPipeMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Null(results[0].Status);
    }
}
