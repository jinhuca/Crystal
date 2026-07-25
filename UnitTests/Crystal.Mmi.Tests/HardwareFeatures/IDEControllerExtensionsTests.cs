using Crystal.Mmi.HardwareFeatures.IDEController;
using Crystal.Mmi.MmiEngine;
using Crystal.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Mmi.Tests.HardwareFeatures;

public class IDEControllerExtensionsTests
{
    private static System.Collections.Frozen.FrozenDictionary<string, WmiValue> ControllerRow() => WmiRow.Build(
        ("Availability", new WmiValue(3)),
        ("Caption", new WmiValue("Standard SATA AHCI Controller")),
        ("ConfigManagerErrorCode", new WmiValue(0)),
        ("ConfigManagerUserConfig", new WmiValue(false)),
        ("CreationClassName", new WmiValue("Win32_IDEController")),
        ("Description", new WmiValue("Standard SATA AHCI Controller")),
        ("DeviceID", new WmiValue("PCI\\VEN_8086&DEV_A102")),
        ("ErrorCleared", new WmiValue(false)),
        ("ErrorDescription", new WmiValue("")),
        ("InstallDate", new WmiValue(new DateTime(2022, 1, 1, 0, 0, 0, DateTimeKind.Utc))),
        ("LastErrorCode", new WmiValue(0)),
        ("Manufacturer", new WmiValue("(Standard IDE ATA/ATAPI controllers)")),
        ("MaxNumberControlled", new WmiValue(0)),
        ("Name", new WmiValue("Standard SATA AHCI Controller")),
        ("PNPDeviceID", new WmiValue("PCI\\VEN_8086&DEV_A102\\3&11583659&0&FA")),
        ("PowerManagementCapabilities", new WmiValue(new ushort[] { 1 })),
        ("PowerManagementSupported", new WmiValue(false)),
        ("ProtocolSupported", new WmiValue(2)),
        ("Status", new WmiValue("OK")),
        ("StatusInfo", new WmiValue(3)),
        ("SystemCreationClassName", new WmiValue("Win32_ComputerSystem")),
        ("SystemName", new WmiValue("DESKTOP-01")),
        ("TimeOfLastReset", new WmiValue(new DateTime(2024, 6, 1, 8, 0, 0, DateTimeKind.Utc)))
    );

    [Fact]
    public async Task FullData_Maps_Name()
    {
        var provider = new FakeWmiProvider("Win32_IDEController", new[] { ControllerRow() });
        var results = await provider.ToSafeIDEControllerMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("Standard SATA AHCI Controller", results[0].Name);
    }

    [Fact]
    public async Task FullData_Maps_DeviceID()
    {
        var provider = new FakeWmiProvider("Win32_IDEController", new[] { ControllerRow() });
        var results = await provider.ToSafeIDEControllerMetricsAsync(CancellationToken.None);

        Assert.Equal("PCI\\VEN_8086&DEV_A102", results[0].DeviceID);
    }

    [Fact]
    public async Task FullData_Maps_ProtocolSupported_Ushort()
    {
        var provider = new FakeWmiProvider("Win32_IDEController", new[] { ControllerRow() });
        var results = await provider.ToSafeIDEControllerMetricsAsync(CancellationToken.None);

        Assert.Equal((ushort)2, results[0].ProtocolSupported);
    }

    [Fact]
    public async Task FullData_Maps_Status_OK()
    {
        var provider = new FakeWmiProvider("Win32_IDEController", new[] { ControllerRow() });
        var results = await provider.ToSafeIDEControllerMetricsAsync(CancellationToken.None);

        Assert.Equal("OK", results[0].Status);
    }

    [Fact]
    public async Task FullData_Maps_TimeOfLastReset()
    {
        var provider = new FakeWmiProvider("Win32_IDEController", new[] { ControllerRow() });
        var results = await provider.ToSafeIDEControllerMetricsAsync(CancellationToken.None);

        Assert.Equal(new DateTime(2024, 6, 1, 8, 0, 0, DateTimeKind.Utc), results[0].TimeOfLastReset);
    }

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("Win32_IDEController", WmiRow.Empty());
        var results = await provider.ToSafeIDEControllerMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MissingClass_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("SomeOtherClass", WmiRow.Empty());
        var results = await provider.ToSafeIDEControllerMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MultipleControllers_Returns_All()
    {
        var c1 = WmiRow.Build(("DeviceID", new WmiValue("IDE1")));
        var c2 = WmiRow.Build(("DeviceID", new WmiValue("IDE2")));

        var provider = new FakeWmiProvider("Win32_IDEController", new[] { c1, c2 });
        var results = await provider.ToSafeIDEControllerMetricsAsync(CancellationToken.None);

        Assert.Equal(2, results.Count);
        Assert.Equal("IDE1", results[0].DeviceID);
        Assert.Equal("IDE2", results[1].DeviceID);
    }

    [Fact]
    public async Task PartialData_Leaves_Missing_Fields_Null()
    {
        var partial = WmiRow.Build(("DeviceID", new WmiValue("IDE3")));

        var provider = new FakeWmiProvider("Win32_IDEController", new[] { partial });
        var results = await provider.ToSafeIDEControllerMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("IDE3", results[0].DeviceID);
        Assert.Null(results[0].ProtocolSupported);
        Assert.Null(results[0].TimeOfLastReset);
    }

    [Fact]
    public async Task WrongTypeValue_Is_Ignored_Not_Miscast()
    {
        // ProtocolSupported stored as a string instead of Int — should be treated as absent, not throw.
        var badRow = WmiRow.Build(("ProtocolSupported", new WmiValue("2")));

        var provider = new FakeWmiProvider("Win32_IDEController", new[] { badRow });
        var results = await provider.ToSafeIDEControllerMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Null(results[0].ProtocolSupported);
    }
}
