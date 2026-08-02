using Crystal.Provider.Mmi.HardwareFeatures.InfraredDevice;
using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Provider.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Provider.Mmi.Tests.HardwareFeatures;

public class InfraredDeviceExtensionsTests
{
    private static System.Collections.Frozen.FrozenDictionary<string, WmiValue> InfraredRow() => WmiRow.Build(
        ("Availability", new WmiValue(3)),
        ("Caption", new WmiValue("Infrared Port")),
        ("ConfigManagerErrorCode", new WmiValue(0)),
        ("ConfigManagerUserConfig", new WmiValue(false)),
        ("CreationClassName", new WmiValue("Win32_InfraredDevice")),
        ("Description", new WmiValue("Infrared Port")),
        ("DeviceID", new WmiValue("Infrared1")),
        ("ErrorCleared", new WmiValue(false)),
        ("ErrorDescription", new WmiValue("")),
        ("InstallDate", new WmiValue(new DateTime(2021, 6, 15, 0, 0, 0, DateTimeKind.Utc))),
        ("LastErrorCode", new WmiValue(0)),
        ("Manufacturer", new WmiValue("Microsoft")),
        ("MaxNumberControlled", new WmiValue(0)),
        ("Name", new WmiValue("Infrared Port")),
        ("PNPDeviceID", new WmiValue("ACPI\\IR0001\\1")),
        ("PowerManagementCapabilities", new WmiValue(new ushort[] { 1 })),
        ("PowerManagementSupported", new WmiValue(false)),
        ("ProtocolSupported", new WmiValue(1)),
        ("Status", new WmiValue("OK")),
        ("StatusInfo", new WmiValue(3)),
        ("SystemCreationClassName", new WmiValue("Win32_ComputerSystem")),
        ("SystemName", new WmiValue("DESKTOP-01")),
        ("TimeOfLastReset", new WmiValue(new DateTime(2024, 6, 1, 8, 0, 0, DateTimeKind.Utc)))
    );

    [Fact]
    public async Task FullData_Maps_Name()
    {
        var provider = new FakeWmiProvider("Win32_InfraredDevice", new[] { InfraredRow() });
        var results = await provider.ToSafeInfraredDeviceMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("Infrared Port", results[0].Name);
    }

    [Fact]
    public async Task FullData_Maps_DeviceID()
    {
        var provider = new FakeWmiProvider("Win32_InfraredDevice", new[] { InfraredRow() });
        var results = await provider.ToSafeInfraredDeviceMetricsAsync(CancellationToken.None);

        Assert.Equal("Infrared1", results[0].DeviceID);
    }

    [Fact]
    public async Task FullData_Maps_Manufacturer()
    {
        var provider = new FakeWmiProvider("Win32_InfraredDevice", new[] { InfraredRow() });
        var results = await provider.ToSafeInfraredDeviceMetricsAsync(CancellationToken.None);

        Assert.Equal("Microsoft", results[0].Manufacturer);
    }

    [Fact]
    public async Task FullData_Maps_Status_OK()
    {
        var provider = new FakeWmiProvider("Win32_InfraredDevice", new[] { InfraredRow() });
        var results = await provider.ToSafeInfraredDeviceMetricsAsync(CancellationToken.None);

        Assert.Equal("OK", results[0].Status);
    }

    [Fact]
    public async Task FullData_Maps_InstallDate()
    {
        var provider = new FakeWmiProvider("Win32_InfraredDevice", new[] { InfraredRow() });
        var results = await provider.ToSafeInfraredDeviceMetricsAsync(CancellationToken.None);

        Assert.Equal(new DateTime(2021, 6, 15, 0, 0, 0, DateTimeKind.Utc), results[0].InstallDate);
    }

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("Win32_InfraredDevice", WmiRow.Empty());
        var results = await provider.ToSafeInfraredDeviceMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MissingClass_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("SomeOtherClass", WmiRow.Empty());
        var results = await provider.ToSafeInfraredDeviceMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MultipleDevices_Returns_All()
    {
        var d1 = WmiRow.Build(("DeviceID", new WmiValue("Infrared1")));
        var d2 = WmiRow.Build(("DeviceID", new WmiValue("Infrared2")));

        var provider = new FakeWmiProvider("Win32_InfraredDevice", new[] { d1, d2 });
        var results = await provider.ToSafeInfraredDeviceMetricsAsync(CancellationToken.None);

        Assert.Equal(2, results.Count);
        Assert.Equal("Infrared1", results[0].DeviceID);
        Assert.Equal("Infrared2", results[1].DeviceID);
    }

    [Fact]
    public async Task PartialData_Leaves_Missing_Fields_Null()
    {
        var partial = WmiRow.Build(("DeviceID", new WmiValue("Infrared3")));

        var provider = new FakeWmiProvider("Win32_InfraredDevice", new[] { partial });
        var results = await provider.ToSafeInfraredDeviceMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("Infrared3", results[0].DeviceID);
        Assert.Null(results[0].Manufacturer);
        Assert.Null(results[0].ProtocolSupported);
    }

    [Fact]
    public async Task WrongTypeValue_Is_Ignored_Not_Miscast()
    {
        // ProtocolSupported stored as a bool instead of Int — should be treated as absent, not throw.
        var badRow = WmiRow.Build(("ProtocolSupported", new WmiValue(true)));

        var provider = new FakeWmiProvider("Win32_InfraredDevice", new[] { badRow });
        var results = await provider.ToSafeInfraredDeviceMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Null(results[0].ProtocolSupported);
    }
}
