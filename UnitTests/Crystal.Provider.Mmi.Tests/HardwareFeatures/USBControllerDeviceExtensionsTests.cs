using Crystal.Provider.Mmi.HardwareFeatures.USBControllerDevice;
using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Provider.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Provider.Mmi.Tests.HardwareFeatures;

public class USBControllerDeviceExtensionsTests
{
    private const string AntecedentPath = "Win32_USBController.DeviceID=\"PCI\\VEN_8086&DEV_A2AF\"";
    private const string DependentPath = "Win32_USBHub.DeviceID=\"USB\\ROOT_HUB30\\4&1234\"";

    private static System.Collections.Frozen.FrozenDictionary<string, WmiValue> RelationshipRow() => WmiRow.Build(
        ("AccessState", new WmiValue(0)),
        ("Antecedent", new WmiValue(AntecedentPath)),
        ("Dependent", new WmiValue(DependentPath)),
        ("NegotiatedDataWidth", new WmiValue(8)),
        ("NegotiatedSpeed", new WmiValue(5000000000UL)),
        ("NumberOfHardResets", new WmiValue(0)),
        ("NumberOfSoftResets", new WmiValue(0))
    );

    [Fact]
    public async Task FullData_Maps_Antecedent_And_Dependent()
    {
        var provider = new FakeWmiProvider("Win32_USBControllerDevice", new[] { RelationshipRow() });
        var results = await provider.ToSafeUSBControllerDeviceMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal(AntecedentPath, results[0].Antecedent);
        Assert.Equal(DependentPath, results[0].Dependent);
    }

    [Fact]
    public async Task FullData_Maps_NegotiatedSpeed_Ulong()
    {
        var provider = new FakeWmiProvider("Win32_USBControllerDevice", new[] { RelationshipRow() });
        var results = await provider.ToSafeUSBControllerDeviceMetricsAsync(CancellationToken.None);

        Assert.Equal(5000000000UL, results[0].NegotiatedSpeed);
    }

    [Fact]
    public async Task FullData_Extracts_ControllerDeviceId()
    {
        var provider = new FakeWmiProvider("Win32_USBControllerDevice", new[] { RelationshipRow() });
        var results = await provider.ToSafeUSBControllerDeviceMetricsAsync(CancellationToken.None);

        Assert.Equal("PCI\\VEN_8086&DEV_A2AF", results[0].ControllerDeviceId);
    }

    [Fact]
    public async Task FullData_Extracts_DeviceId()
    {
        var provider = new FakeWmiProvider("Win32_USBControllerDevice", new[] { RelationshipRow() });
        var results = await provider.ToSafeUSBControllerDeviceMetricsAsync(CancellationToken.None);

        Assert.Equal("USB\\ROOT_HUB30\\4&1234", results[0].DeviceId);
    }

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("Win32_USBControllerDevice", WmiRow.Empty());
        var results = await provider.ToSafeUSBControllerDeviceMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MissingClass_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("SomeOtherClass", WmiRow.Empty());
        var results = await provider.ToSafeUSBControllerDeviceMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MultipleRelationships_Returns_All()
    {
        var rel1 = WmiRow.Build(("Antecedent", new WmiValue(AntecedentPath)));
        var rel2 = WmiRow.Build(("Antecedent", new WmiValue("Win32_USBController.DeviceID=\"USB2\"")));

        var provider = new FakeWmiProvider("Win32_USBControllerDevice", new[] { rel1, rel2 });
        var results = await provider.ToSafeUSBControllerDeviceMetricsAsync(CancellationToken.None);

        Assert.Equal(2, results.Count);
        Assert.Equal("PCI\\VEN_8086&DEV_A2AF", results[0].ControllerDeviceId);
        Assert.Equal("USB2", results[1].ControllerDeviceId);
    }

    [Fact]
    public async Task MissingReference_Leaves_Field_Null()
    {
        var partial = WmiRow.Build(("Antecedent", new WmiValue(AntecedentPath)));

        var provider = new FakeWmiProvider("Win32_USBControllerDevice", new[] { partial });
        var results = await provider.ToSafeUSBControllerDeviceMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Null(results[0].Dependent);
        Assert.Null(results[0].DeviceId);
        Assert.Null(results[0].NegotiatedSpeed);
    }

    [Fact]
    public async Task WrongTypeValue_Is_Ignored_Not_Miscast()
    {
        // NegotiatedSpeed stored as an Int instead of ULong — should be treated as absent, not throw.
        var badRow = WmiRow.Build(("NegotiatedSpeed", new WmiValue(500000)));

        var provider = new FakeWmiProvider("Win32_USBControllerDevice", new[] { badRow });
        var results = await provider.ToSafeUSBControllerDeviceMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Null(results[0].NegotiatedSpeed);
    }
}
