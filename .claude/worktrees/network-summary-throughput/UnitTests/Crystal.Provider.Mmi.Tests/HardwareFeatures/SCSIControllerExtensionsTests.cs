using Crystal.Provider.Mmi.HardwareFeatures.SCSIController;
using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Provider.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Provider.Mmi.Tests.HardwareFeatures;

public class SCSIControllerExtensionsTests
{
    private static System.Collections.Frozen.FrozenDictionary<string, WmiValue> ControllerRow() => WmiRow.Build(
        ("Availability", new WmiValue(3)),
        ("Caption", new WmiValue("LSI Adapter, SAS 3000 series")),
        ("ConfigManagerErrorCode", new WmiValue(0)),
        ("ConfigManagerUserConfig", new WmiValue(false)),
        ("ControllerTimeouts", new WmiValue(0)),
        ("CreationClassName", new WmiValue("Win32_SCSIController")),
        ("Description", new WmiValue("LSI Adapter, SAS 3000 series")),
        ("DeviceID", new WmiValue("PCI\\VEN_1000&DEV_0072")),
        ("DeviceMap", new WmiValue("")),
        ("DriverName", new WmiValue("lsi_sas.sys")),
        ("ErrorCleared", new WmiValue(false)),
        ("ErrorDescription", new WmiValue("")),
        ("HardwareVersion", new WmiValue("Rev A")),
        ("Index", new WmiValue(0)),
        ("InstallDate", new WmiValue(new DateTime(2022, 2, 2, 0, 0, 0, DateTimeKind.Utc))),
        ("LastErrorCode", new WmiValue(0)),
        ("Manufacturer", new WmiValue("LSI")),
        ("MaxDataWidth", new WmiValue(16)),
        ("MaxNumberControlled", new WmiValue(0)),
        ("MaxTransferRate", new WmiValue(300000000UL)),
        ("Name", new WmiValue("LSI Adapter, SAS 3000 series")),
        ("PNPDeviceID", new WmiValue("PCI\\VEN_1000&DEV_0072\\4&1234")),
        ("PowerManagementCapabilities", new WmiValue(new ushort[] { 1 })),
        ("PowerManagementSupported", new WmiValue(false)),
        ("ProtectionManagement", new WmiValue(0)),
        ("ProtocolSupported", new WmiValue(5)),
        ("Status", new WmiValue("OK")),
        ("StatusInfo", new WmiValue(3)),
        ("SystemCreationClassName", new WmiValue("Win32_ComputerSystem")),
        ("SystemName", new WmiValue("DESKTOP-01")),
        ("TimeOfLastReset", new WmiValue(new DateTime(2024, 6, 1, 8, 0, 0, DateTimeKind.Utc)))
    );

    [Fact]
    public async Task FullData_Maps_Name()
    {
        var provider = new FakeWmiProvider("Win32_SCSIController", new[] { ControllerRow() });
        var results = await provider.ToSafeSCSIControllerMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("LSI Adapter, SAS 3000 series", results[0].Name);
    }

    [Fact]
    public async Task FullData_Maps_MaxTransferRate_Ulong()
    {
        var provider = new FakeWmiProvider("Win32_SCSIController", new[] { ControllerRow() });
        var results = await provider.ToSafeSCSIControllerMetricsAsync(CancellationToken.None);

        Assert.Equal(300000000UL, results[0].MaxTransferRate);
    }

    [Fact]
    public async Task FullData_Maps_MaxDataWidth_Uint()
    {
        var provider = new FakeWmiProvider("Win32_SCSIController", new[] { ControllerRow() });
        var results = await provider.ToSafeSCSIControllerMetricsAsync(CancellationToken.None);

        Assert.Equal(16u, results[0].MaxDataWidth);
    }

    [Fact]
    public async Task FullData_Maps_DriverName()
    {
        var provider = new FakeWmiProvider("Win32_SCSIController", new[] { ControllerRow() });
        var results = await provider.ToSafeSCSIControllerMetricsAsync(CancellationToken.None);

        Assert.Equal("lsi_sas.sys", results[0].DriverName);
    }

    [Fact]
    public async Task FullData_Maps_Status_OK()
    {
        var provider = new FakeWmiProvider("Win32_SCSIController", new[] { ControllerRow() });
        var results = await provider.ToSafeSCSIControllerMetricsAsync(CancellationToken.None);

        Assert.Equal("OK", results[0].Status);
    }

    [Fact]
    public async Task FullData_Maps_TimeOfLastReset()
    {
        var provider = new FakeWmiProvider("Win32_SCSIController", new[] { ControllerRow() });
        var results = await provider.ToSafeSCSIControllerMetricsAsync(CancellationToken.None);

        Assert.Equal(new DateTime(2024, 6, 1, 8, 0, 0, DateTimeKind.Utc), results[0].TimeOfLastReset);
    }

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("Win32_SCSIController", WmiRow.Empty());
        var results = await provider.ToSafeSCSIControllerMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MissingClass_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("SomeOtherClass", WmiRow.Empty());
        var results = await provider.ToSafeSCSIControllerMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MultipleControllers_Returns_All()
    {
        var c1 = WmiRow.Build(("DeviceID", new WmiValue("SCSI1")));
        var c2 = WmiRow.Build(("DeviceID", new WmiValue("SCSI2")));

        var provider = new FakeWmiProvider("Win32_SCSIController", new[] { c1, c2 });
        var results = await provider.ToSafeSCSIControllerMetricsAsync(CancellationToken.None);

        Assert.Equal(2, results.Count);
        Assert.Equal("SCSI1", results[0].DeviceID);
        Assert.Equal("SCSI2", results[1].DeviceID);
    }

    [Fact]
    public async Task PartialData_Leaves_Missing_Fields_Null()
    {
        var partial = WmiRow.Build(("DeviceID", new WmiValue("SCSI3")));

        var provider = new FakeWmiProvider("Win32_SCSIController", new[] { partial });
        var results = await provider.ToSafeSCSIControllerMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("SCSI3", results[0].DeviceID);
        Assert.Null(results[0].MaxTransferRate);
        Assert.Null(results[0].DriverName);
    }

    [Fact]
    public async Task WrongTypeValue_Is_Ignored_Not_Miscast()
    {
        // MaxTransferRate stored as an Int instead of ULong — should be treated as absent, not throw.
        var badRow = WmiRow.Build(("MaxTransferRate", new WmiValue(300000)));

        var provider = new FakeWmiProvider("Win32_SCSIController", new[] { badRow });
        var results = await provider.ToSafeSCSIControllerMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Null(results[0].MaxTransferRate);
    }
}
