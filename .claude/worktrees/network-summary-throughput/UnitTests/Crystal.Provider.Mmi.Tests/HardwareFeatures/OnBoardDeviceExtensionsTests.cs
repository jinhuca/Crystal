using Crystal.Provider.Mmi.HardwareFeatures.OnBoardDevice;
using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Provider.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Provider.Mmi.Tests.HardwareFeatures;

public class OnBoardDeviceExtensionsTests
{
    private static System.Collections.Frozen.FrozenDictionary<string, WmiValue> OnboardNicRow() => WmiRow.Build(
        ("Caption", new WmiValue("Onboard Ethernet")),
        ("CreationClassName", new WmiValue("Win32_OnBoardDevice")),
        ("Description", new WmiValue("Onboard Ethernet")),
        ("DeviceType", new WmiValue(3)), // 3 = Ethernet
        ("Enabled", new WmiValue(true)),
        ("HotSwappable", new WmiValue(false)),
        ("InstallDate", new WmiValue(new DateTime(2021, 8, 8, 0, 0, 0, DateTimeKind.Utc))),
        ("Manufacturer", new WmiValue("ASUSTeK COMPUTER INC.")),
        ("Model", new WmiValue("")),
        ("Name", new WmiValue("Onboard Ethernet")),
        ("OtherIdentifyingInfo", new WmiValue("")),
        ("PartNumber", new WmiValue("")),
        ("PoweredOn", new WmiValue(true)),
        ("Removable", new WmiValue(false)),
        ("Replaceable", new WmiValue(false)),
        ("SerialNumber", new WmiValue("")),
        ("SKU", new WmiValue("")),
        ("Status", new WmiValue("OK")),
        ("Tag", new WmiValue("Onboard Ethernet")),
        ("Version", new WmiValue(""))
    );

    [Fact]
    public async Task FullData_Maps_Name()
    {
        var provider = new FakeWmiProvider("Win32_OnBoardDevice", new[] { OnboardNicRow() });
        var results = await provider.ToSafeOnBoardDeviceMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("Onboard Ethernet", results[0].Name);
    }

    [Fact]
    public async Task FullData_Maps_DeviceType_Ushort()
    {
        var provider = new FakeWmiProvider("Win32_OnBoardDevice", new[] { OnboardNicRow() });
        var results = await provider.ToSafeOnBoardDeviceMetricsAsync(CancellationToken.None);

        Assert.Equal((ushort)3, results[0].DeviceType);
    }

    [Fact]
    public async Task FullData_Maps_Enabled_True()
    {
        var provider = new FakeWmiProvider("Win32_OnBoardDevice", new[] { OnboardNicRow() });
        var results = await provider.ToSafeOnBoardDeviceMetricsAsync(CancellationToken.None);

        Assert.True(results[0].Enabled);
    }

    [Fact]
    public async Task FullData_Maps_Manufacturer()
    {
        var provider = new FakeWmiProvider("Win32_OnBoardDevice", new[] { OnboardNicRow() });
        var results = await provider.ToSafeOnBoardDeviceMetricsAsync(CancellationToken.None);

        Assert.Equal("ASUSTeK COMPUTER INC.", results[0].Manufacturer);
    }

    [Fact]
    public async Task FullData_Maps_Tag()
    {
        var provider = new FakeWmiProvider("Win32_OnBoardDevice", new[] { OnboardNicRow() });
        var results = await provider.ToSafeOnBoardDeviceMetricsAsync(CancellationToken.None);

        Assert.Equal("Onboard Ethernet", results[0].Tag);
    }

    [Fact]
    public async Task FullData_Maps_Status_OK()
    {
        var provider = new FakeWmiProvider("Win32_OnBoardDevice", new[] { OnboardNicRow() });
        var results = await provider.ToSafeOnBoardDeviceMetricsAsync(CancellationToken.None);

        Assert.Equal("OK", results[0].Status);
    }

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("Win32_OnBoardDevice", WmiRow.Empty());
        var results = await provider.ToSafeOnBoardDeviceMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MissingClass_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("SomeOtherClass", WmiRow.Empty());
        var results = await provider.ToSafeOnBoardDeviceMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MultipleDevices_Returns_All()
    {
        var d1 = WmiRow.Build(("Tag", new WmiValue("Onboard Ethernet")));
        var d2 = WmiRow.Build(("Tag", new WmiValue("Onboard Video")));

        var provider = new FakeWmiProvider("Win32_OnBoardDevice", new[] { d1, d2 });
        var results = await provider.ToSafeOnBoardDeviceMetricsAsync(CancellationToken.None);

        Assert.Equal(2, results.Count);
        Assert.Equal("Onboard Ethernet", results[0].Tag);
        Assert.Equal("Onboard Video", results[1].Tag);
    }

    [Fact]
    public async Task PartialData_Leaves_Missing_Fields_Null()
    {
        var partial = WmiRow.Build(("Tag", new WmiValue("Onboard Audio")));

        var provider = new FakeWmiProvider("Win32_OnBoardDevice", new[] { partial });
        var results = await provider.ToSafeOnBoardDeviceMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("Onboard Audio", results[0].Tag);
        Assert.Null(results[0].DeviceType);
        Assert.Null(results[0].Enabled);
    }

    [Fact]
    public async Task WrongTypeValue_Is_Ignored_Not_Miscast()
    {
        // Enabled stored as a string instead of Bool — should be treated as absent, not throw.
        var badRow = WmiRow.Build(("Enabled", new WmiValue("true")));

        var provider = new FakeWmiProvider("Win32_OnBoardDevice", new[] { badRow });
        var results = await provider.ToSafeOnBoardDeviceMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Null(results[0].Enabled);
    }
}
