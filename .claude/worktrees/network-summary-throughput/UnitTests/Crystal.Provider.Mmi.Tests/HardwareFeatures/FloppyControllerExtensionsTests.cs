using Crystal.Provider.Mmi.HardwareFeatures.FloppyController;
using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Provider.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Provider.Mmi.Tests.HardwareFeatures;

public class FloppyControllerExtensionsTests
{
    private static System.Collections.Frozen.FrozenDictionary<string, WmiValue> FullRow() => WmiRow.Build(
        ("Availability", new WmiValue(3)),
        ("Caption", new WmiValue("Standard floppy disk controller")),
        ("ConfigManagerErrorCode", new WmiValue(0)),
        ("ConfigManagerUserConfig", new WmiValue(false)),
        ("CreationClassName", new WmiValue("Win32_FloppyController")),
        ("Description", new WmiValue("Standard floppy disk controller")),
        ("DeviceID", new WmiValue("FloppyController0")),
        ("ErrorCleared", new WmiValue(false)),
        ("ErrorDescription", new WmiValue("")),
        ("InstallDate", new WmiValue(new DateTime(2021, 5, 1, 0, 0, 0, DateTimeKind.Utc))),
        ("LastErrorCode", new WmiValue(0)),
        ("Manufacturer", new WmiValue("(Standard floppy disk controllers)")),
        ("MaxNumberControlled", new WmiValue(2)),
        ("Name", new WmiValue("Standard floppy disk controller")),
        ("PNPDeviceID", new WmiValue("FDC\\GENERIC_FLOPPY_DRIVE\\0")),
        ("PowerManagementCapabilities", new WmiValue(new ushort[] { 1 })),
        ("PowerManagementSupported", new WmiValue(false)),
        ("ProtocolSupported", new WmiValue(0)),
        ("Status", new WmiValue("OK")),
        ("StatusInfo", new WmiValue(3)),
        ("SystemCreationClassName", new WmiValue("Win32_ComputerSystem")),
        ("SystemName", new WmiValue("DESKTOP-01")),
        ("TimeOfLastReset", new WmiValue(new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)))
    );

    [Fact]
    public async Task FullData_Maps_DeviceID_And_Name()
    {
        var provider = new FakeWmiProvider("Win32_FloppyController", new[] { FullRow() });
        var results = await provider.ToSafeFloppyControllerMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("FloppyController0", results[0].DeviceID);
        Assert.Equal("Standard floppy disk controller", results[0].Name);
    }

    [Fact]
    public async Task FullData_Maps_MaxNumberControlled_Uint()
    {
        var provider = new FakeWmiProvider("Win32_FloppyController", new[] { FullRow() });
        var results = await provider.ToSafeFloppyControllerMetricsAsync(CancellationToken.None);

        Assert.Equal(2u, results[0].MaxNumberControlled);
    }

    [Fact]
    public async Task FullData_Maps_TimeOfLastReset()
    {
        var provider = new FakeWmiProvider("Win32_FloppyController", new[] { FullRow() });
        var results = await provider.ToSafeFloppyControllerMetricsAsync(CancellationToken.None);

        Assert.Equal(new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), results[0].TimeOfLastReset);
    }

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("Win32_FloppyController", WmiRow.Empty());
        var results = await provider.ToSafeFloppyControllerMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MissingClass_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("SomeOtherClass", WmiRow.Empty());
        var results = await provider.ToSafeFloppyControllerMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task PartialData_Leaves_Missing_Fields_Null()
    {
        var partial = WmiRow.Build(("Name", new WmiValue("Bare Controller")));

        var provider = new FakeWmiProvider("Win32_FloppyController", new[] { partial });
        var results = await provider.ToSafeFloppyControllerMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("Bare Controller", results[0].Name);
        Assert.Null(results[0].MaxNumberControlled);
        Assert.Null(results[0].PowerManagementCapabilities);
    }

    [Fact]
    public async Task WrongTypeValue_Is_Ignored_Not_Miscast()
    {
        // MaxNumberControlled stored as a bool instead of Int — should be treated as absent, not throw.
        var badRow = WmiRow.Build(("MaxNumberControlled", new WmiValue(true)));

        var provider = new FakeWmiProvider("Win32_FloppyController", new[] { badRow });
        var results = await provider.ToSafeFloppyControllerMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Null(results[0].MaxNumberControlled);
    }

    [Fact]
    public async Task MultipleControllers_Returns_All()
    {
        var a = WmiRow.Build(("DeviceID", new WmiValue("FloppyController0")));
        var b = WmiRow.Build(("DeviceID", new WmiValue("FloppyController1")));

        var provider = new FakeWmiProvider("Win32_FloppyController", new[] { a, b });
        var results = await provider.ToSafeFloppyControllerMetricsAsync(CancellationToken.None);

        Assert.Equal(2, results.Count);
    }
}
