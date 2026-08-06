using Crystal.Provider.Mmi.HardwareFeatures.USBHub;
using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Provider.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Provider.Mmi.Tests.HardwareFeatures;

public class USBHubExtensionsTests
{
    private static System.Collections.Frozen.FrozenDictionary<string, WmiValue> HubRow() => WmiRow.Build(
        ("Availability", new WmiValue(3)),
        ("Caption", new WmiValue("USB Root Hub")),
        ("ClassCode", new WmiValue(9)),
        ("ConfigManagerErrorCode", new WmiValue(0)),
        ("ConfigManagerUserConfig", new WmiValue(false)),
        ("CreationClassName", new WmiValue("Win32_USBHub")),
        ("CurrentAlternateSettings", new WmiValue(new ushort[] { 0 })),
        ("CurrentConfigValue", new WmiValue(1)),
        ("Description", new WmiValue("USB Root Hub")),
        ("DeviceID", new WmiValue("USB\\ROOT_HUB30\\4&1234")),
        ("ErrorCleared", new WmiValue(false)),
        ("ErrorDescription", new WmiValue("")),
        ("GangSwitched", new WmiValue(false)),
        ("InstallDate", new WmiValue(new DateTime(2022, 4, 4, 0, 0, 0, DateTimeKind.Utc))),
        ("LastErrorCode", new WmiValue(0)),
        ("Name", new WmiValue("USB Root Hub")),
        ("NumberOfConfigs", new WmiValue(1)),
        ("NumberOfPorts", new WmiValue(16)),
        ("PNPDeviceID", new WmiValue("USB\\ROOT_HUB30\\4&1234")),
        ("PowerManagementCapabilities", new WmiValue(new ushort[] { 1 })),
        ("PowerManagementSupported", new WmiValue(true)),
        ("ProtocolCode", new WmiValue(0)),
        ("Status", new WmiValue("OK")),
        ("StatusInfo", new WmiValue(3)),
        ("SubclassCode", new WmiValue(0)),
        ("SystemCreationClassName", new WmiValue("Win32_ComputerSystem")),
        ("SystemName", new WmiValue("DESKTOP-01")),
        ("USBVersion", new WmiValue(768))
    );

    [Fact]
    public async Task FullData_Maps_Name()
    {
        var provider = new FakeWmiProvider("Win32_USBHub", new[] { HubRow() });
        var results = await provider.ToSafeUSBHubMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("USB Root Hub", results[0].Name);
    }

    [Fact]
    public async Task FullData_Maps_NumberOfPorts_Byte()
    {
        var provider = new FakeWmiProvider("Win32_USBHub", new[] { HubRow() });
        var results = await provider.ToSafeUSBHubMetricsAsync(CancellationToken.None);

        Assert.Equal((byte)16, results[0].NumberOfPorts);
    }

    [Fact]
    public async Task FullData_Maps_ClassCode_Byte()
    {
        var provider = new FakeWmiProvider("Win32_USBHub", new[] { HubRow() });
        var results = await provider.ToSafeUSBHubMetricsAsync(CancellationToken.None);

        Assert.Equal((byte)9, results[0].ClassCode);
    }

    [Fact]
    public async Task FullData_Maps_CurrentAlternateSettings_ByteArray()
    {
        var provider = new FakeWmiProvider("Win32_USBHub", new[] { HubRow() });
        var results = await provider.ToSafeUSBHubMetricsAsync(CancellationToken.None);

        Assert.Equal(new byte[] { 0 }, results[0].CurrentAlternateSettings);
    }

    [Fact]
    public async Task FullData_Maps_GangSwitched_False()
    {
        var provider = new FakeWmiProvider("Win32_USBHub", new[] { HubRow() });
        var results = await provider.ToSafeUSBHubMetricsAsync(CancellationToken.None);

        Assert.False(results[0].GangSwitched);
    }

    [Fact]
    public async Task FullData_Maps_Status_OK()
    {
        var provider = new FakeWmiProvider("Win32_USBHub", new[] { HubRow() });
        var results = await provider.ToSafeUSBHubMetricsAsync(CancellationToken.None);

        Assert.Equal("OK", results[0].Status);
    }

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("Win32_USBHub", WmiRow.Empty());
        var results = await provider.ToSafeUSBHubMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MissingClass_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("SomeOtherClass", WmiRow.Empty());
        var results = await provider.ToSafeUSBHubMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MultipleHubs_Returns_All()
    {
        var h1 = WmiRow.Build(("DeviceID", new WmiValue("HUB1")));
        var h2 = WmiRow.Build(("DeviceID", new WmiValue("HUB2")));

        var provider = new FakeWmiProvider("Win32_USBHub", new[] { h1, h2 });
        var results = await provider.ToSafeUSBHubMetricsAsync(CancellationToken.None);

        Assert.Equal(2, results.Count);
        Assert.Equal("HUB1", results[0].DeviceID);
        Assert.Equal("HUB2", results[1].DeviceID);
    }

    [Fact]
    public async Task PartialData_Leaves_Missing_Fields_Null()
    {
        var partial = WmiRow.Build(("DeviceID", new WmiValue("HUB3")));

        var provider = new FakeWmiProvider("Win32_USBHub", new[] { partial });
        var results = await provider.ToSafeUSBHubMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("HUB3", results[0].DeviceID);
        Assert.Null(results[0].NumberOfPorts);
        Assert.Null(results[0].CurrentAlternateSettings);
    }

    [Fact]
    public async Task WrongTypeValue_Is_Ignored_Not_Miscast()
    {
        // NumberOfPorts stored as a string instead of Int — should be treated as absent, not throw.
        var badRow = WmiRow.Build(("NumberOfPorts", new WmiValue("16")));

        var provider = new FakeWmiProvider("Win32_USBHub", new[] { badRow });
        var results = await provider.ToSafeUSBHubMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Null(results[0].NumberOfPorts);
    }
}
