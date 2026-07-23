using Crystal.Mmi.HardwareFeatures.Refrigeration;
using Crystal.Mmi.MmiEngine;
using Crystal.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Mmi.Tests.HardwareFeatures;

public class RefrigerationExtensionsTests
{
    private static System.Collections.Frozen.FrozenDictionary<string, WmiValue> RefrigerationRow() => WmiRow.Build(
        ("ActiveCooling", new WmiValue(true)),
        ("Availability", new WmiValue(3)),
        ("Caption", new WmiValue("Cooling Device")),
        ("ConfigManagerErrorCode", new WmiValue(0)),
        ("ConfigManagerUserConfig", new WmiValue(false)),
        ("CreationClassName", new WmiValue("Win32_Refrigeration")),
        ("Description", new WmiValue("Cooling Device")),
        ("DeviceID", new WmiValue("Refrigeration1")),
        ("ErrorCleared", new WmiValue(false)),
        ("ErrorDescription", new WmiValue("")),
        ("InstallDate", new WmiValue(new DateTime(2022, 9, 10, 0, 0, 0, DateTimeKind.Utc))),
        ("LastErrorCode", new WmiValue(0)),
        ("Name", new WmiValue("Cooling Device")),
        ("PNPDeviceID", new WmiValue("ACPI\\THERMAL\\1")),
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
        var provider = new FakeWmiProvider("Win32_Refrigeration", new[] { RefrigerationRow() });
        var results = await provider.ToSafeRefrigerationMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("Cooling Device", results[0].Name);
    }

    [Fact]
    public async Task FullData_Maps_ActiveCooling_True()
    {
        var provider = new FakeWmiProvider("Win32_Refrigeration", new[] { RefrigerationRow() });
        var results = await provider.ToSafeRefrigerationMetricsAsync(CancellationToken.None);

        Assert.True(results[0].ActiveCooling);
    }

    [Fact]
    public async Task FullData_Maps_Availability_Ushort()
    {
        var provider = new FakeWmiProvider("Win32_Refrigeration", new[] { RefrigerationRow() });
        var results = await provider.ToSafeRefrigerationMetricsAsync(CancellationToken.None);

        Assert.Equal((ushort)3, results[0].Availability);
    }

    [Fact]
    public async Task FullData_Maps_DeviceID()
    {
        var provider = new FakeWmiProvider("Win32_Refrigeration", new[] { RefrigerationRow() });
        var results = await provider.ToSafeRefrigerationMetricsAsync(CancellationToken.None);

        Assert.Equal("Refrigeration1", results[0].DeviceID);
    }

    [Fact]
    public async Task FullData_Maps_Status_OK()
    {
        var provider = new FakeWmiProvider("Win32_Refrigeration", new[] { RefrigerationRow() });
        var results = await provider.ToSafeRefrigerationMetricsAsync(CancellationToken.None);

        Assert.Equal("OK", results[0].Status);
    }

    [Fact]
    public async Task FullData_Maps_InstallDate()
    {
        var provider = new FakeWmiProvider("Win32_Refrigeration", new[] { RefrigerationRow() });
        var results = await provider.ToSafeRefrigerationMetricsAsync(CancellationToken.None);

        Assert.Equal(new DateTime(2022, 9, 10, 0, 0, 0, DateTimeKind.Utc), results[0].InstallDate);
    }

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("Win32_Refrigeration", WmiRow.Empty());
        var results = await provider.ToSafeRefrigerationMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MissingClass_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("SomeOtherClass", WmiRow.Empty());
        var results = await provider.ToSafeRefrigerationMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MultipleDevices_Returns_All()
    {
        var dev1 = WmiRow.Build(("DeviceID", new WmiValue("Refrigeration1")));
        var dev2 = WmiRow.Build(("DeviceID", new WmiValue("Refrigeration2")));

        var provider = new FakeWmiProvider("Win32_Refrigeration", new[] { dev1, dev2 });
        var results = await provider.ToSafeRefrigerationMetricsAsync(CancellationToken.None);

        Assert.Equal(2, results.Count);
        Assert.Equal("Refrigeration1", results[0].DeviceID);
        Assert.Equal("Refrigeration2", results[1].DeviceID);
    }

    [Fact]
    public async Task PartialData_Leaves_Missing_Fields_Null()
    {
        var partial = WmiRow.Build(("DeviceID", new WmiValue("Refrigeration3")));

        var provider = new FakeWmiProvider("Win32_Refrigeration", new[] { partial });
        var results = await provider.ToSafeRefrigerationMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("Refrigeration3", results[0].DeviceID);
        Assert.Null(results[0].ActiveCooling);
        Assert.Null(results[0].Availability);
    }

    [Fact]
    public async Task WrongTypeValue_Is_Ignored_Not_Miscast()
    {
        // ActiveCooling stored as a string instead of Bool — should be treated as absent, not throw.
        var badRow = WmiRow.Build(("ActiveCooling", new WmiValue("true")));

        var provider = new FakeWmiProvider("Win32_Refrigeration", new[] { badRow });
        var results = await provider.ToSafeRefrigerationMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Null(results[0].ActiveCooling);
    }
}
