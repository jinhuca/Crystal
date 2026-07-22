using Crystal.Mmi.HardwareFeatures.DeviceBus;
using Crystal.Mmi.MmiEngine;
using Crystal.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Mmi.Tests.HardwareFeatures;

public class DeviceBusExtensionsTests
{
    // Runtime value: Win32_Bus.DeviceID="PCIBus"
    private const string AntecedentPath = "Win32_Bus.DeviceID=\"PCIBus\"";

    // Runtime value: Win32_PnPEntity.DeviceID="PCI\VEN_10DE&DEV_1234"
    private const string DependentPath = "Win32_PnPEntity.DeviceID=\"PCI\\VEN_10DE&DEV_1234\"";

    [Fact]
    public async Task FullData_Maps_Antecedent_And_Dependent()
    {
        var row = WmiRow.Build(
            ("Antecedent", new WmiValue(AntecedentPath)),
            ("Dependent", new WmiValue(DependentPath)));

        var provider = new FakeWmiProvider("Win32_DeviceBus", new[] { row });
        var results = await provider.ToSafeDeviceBusMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal(AntecedentPath, results[0].Antecedent);
        Assert.Equal(DependentPath, results[0].Dependent);
    }

    [Fact]
    public async Task FullData_Extracts_BusDeviceId()
    {
        var row = WmiRow.Build(("Antecedent", new WmiValue(AntecedentPath)));

        var provider = new FakeWmiProvider("Win32_DeviceBus", new[] { row });
        var results = await provider.ToSafeDeviceBusMetricsAsync(CancellationToken.None);

        Assert.Equal("PCIBus", results[0].BusDeviceId);
    }

    [Fact]
    public async Task FullData_Extracts_DeviceId()
    {
        var row = WmiRow.Build(("Dependent", new WmiValue(DependentPath)));

        var provider = new FakeWmiProvider("Win32_DeviceBus", new[] { row });
        var results = await provider.ToSafeDeviceBusMetricsAsync(CancellationToken.None);

        Assert.Equal("PCI\\VEN_10DE&DEV_1234", results[0].DeviceId);
    }

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("Win32_DeviceBus", WmiRow.Empty());
        var results = await provider.ToSafeDeviceBusMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MissingClass_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("SomeOtherClass", WmiRow.Empty());
        var results = await provider.ToSafeDeviceBusMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MultipleRelationships_Returns_All()
    {
        var rel1 = WmiRow.Build(("Antecedent", new WmiValue("Win32_Bus.DeviceID=\"PCIBus\"")));
        var rel2 = WmiRow.Build(("Antecedent", new WmiValue("Win32_Bus.DeviceID=\"ISABus\"")));

        var provider = new FakeWmiProvider("Win32_DeviceBus", new[] { rel1, rel2 });
        var results = await provider.ToSafeDeviceBusMetricsAsync(CancellationToken.None);

        Assert.Equal(2, results.Count);
        Assert.Equal("PCIBus", results[0].BusDeviceId);
        Assert.Equal("ISABus", results[1].BusDeviceId);
    }

    [Fact]
    public async Task MissingReference_Leaves_Field_Null()
    {
        var partial = WmiRow.Build(("Antecedent", new WmiValue(AntecedentPath)));

        var provider = new FakeWmiProvider("Win32_DeviceBus", new[] { partial });
        var results = await provider.ToSafeDeviceBusMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Null(results[0].Dependent);
        Assert.Null(results[0].DeviceId);
    }

    [Fact]
    public async Task WrongTypeValue_Is_Ignored_Not_Miscast()
    {
        // Antecedent stored as a bool instead of String — should be treated as absent, not throw.
        var badRow = WmiRow.Build(("Antecedent", new WmiValue(true)));

        var provider = new FakeWmiProvider("Win32_DeviceBus", new[] { badRow });
        var results = await provider.ToSafeDeviceBusMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Null(results[0].Antecedent);
        Assert.Null(results[0].BusDeviceId);
    }
}
