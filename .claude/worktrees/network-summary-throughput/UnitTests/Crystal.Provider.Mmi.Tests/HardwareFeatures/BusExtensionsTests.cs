using Crystal.Provider.Mmi.HardwareFeatures.Bus;
using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Provider.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Provider.Mmi.Tests.HardwareFeatures;

public class BusExtensionsTests
{
    private static System.Collections.Frozen.FrozenDictionary<string, WmiValue> PciBusRow() => WmiRow.Build(
        ("Availability", new WmiValue(3)),
        ("BusNum", new WmiValue(0)),
        ("BusType", new WmiValue(5)), // 5 = PCI Bus
        ("Caption", new WmiValue("PCI Bus")),
        ("ConfigManagerErrorCode", new WmiValue(0)),
        ("ConfigManagerUserConfig", new WmiValue(false)),
        ("CreationClassName", new WmiValue("Win32_Bus")),
        ("Description", new WmiValue("PCI Bus")),
        ("DeviceID", new WmiValue("PCIBus")),
        ("ErrorCleared", new WmiValue(false)),
        ("ErrorDescription", new WmiValue("")),
        ("InstallDate", new WmiValue(new DateTime(2022, 3, 1, 0, 0, 0, DateTimeKind.Utc))),
        ("LastErrorCode", new WmiValue(0)),
        ("Name", new WmiValue("PCI Bus")),
        ("PNPDeviceID", new WmiValue("ACPI\\PNP0A03\\0")),
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
        var provider = new FakeWmiProvider("Win32_Bus", new[] { PciBusRow() });
        var results = await provider.ToSafeBusMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("PCI Bus", results[0].Name);
    }

    [Fact]
    public async Task FullData_Maps_DeviceID()
    {
        var provider = new FakeWmiProvider("Win32_Bus", new[] { PciBusRow() });
        var results = await provider.ToSafeBusMetricsAsync(CancellationToken.None);

        Assert.Equal("PCIBus", results[0].DeviceID);
    }

    [Fact]
    public async Task FullData_Maps_BusType_Uint()
    {
        var provider = new FakeWmiProvider("Win32_Bus", new[] { PciBusRow() });
        var results = await provider.ToSafeBusMetricsAsync(CancellationToken.None);

        Assert.Equal(5u, results[0].BusType);
    }

    [Fact]
    public async Task FullData_Maps_BusNum_Uint()
    {
        var provider = new FakeWmiProvider("Win32_Bus", new[] { PciBusRow() });
        var results = await provider.ToSafeBusMetricsAsync(CancellationToken.None);

        Assert.Equal(0u, results[0].BusNum);
    }

    [Fact]
    public async Task FullData_Maps_Availability_Ushort()
    {
        var provider = new FakeWmiProvider("Win32_Bus", new[] { PciBusRow() });
        var results = await provider.ToSafeBusMetricsAsync(CancellationToken.None);

        Assert.Equal((ushort)3, results[0].Availability);
    }

    [Fact]
    public async Task FullData_Maps_PowerManagementCapabilities_Array()
    {
        var provider = new FakeWmiProvider("Win32_Bus", new[] { PciBusRow() });
        var results = await provider.ToSafeBusMetricsAsync(CancellationToken.None);

        Assert.Equal(new ushort[] { 1 }, results[0].PowerManagementCapabilities);
    }

    [Fact]
    public async Task FullData_Maps_Status_OK()
    {
        var provider = new FakeWmiProvider("Win32_Bus", new[] { PciBusRow() });
        var results = await provider.ToSafeBusMetricsAsync(CancellationToken.None);

        Assert.Equal("OK", results[0].Status);
    }

    [Fact]
    public async Task FullData_Maps_InstallDate()
    {
        var provider = new FakeWmiProvider("Win32_Bus", new[] { PciBusRow() });
        var results = await provider.ToSafeBusMetricsAsync(CancellationToken.None);

        Assert.Equal(new DateTime(2022, 3, 1, 0, 0, 0, DateTimeKind.Utc), results[0].InstallDate);
    }

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("Win32_Bus", WmiRow.Empty());
        var results = await provider.ToSafeBusMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MissingClass_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("SomeOtherClass", WmiRow.Empty());
        var results = await provider.ToSafeBusMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MultipleBuses_Returns_All()
    {
        var pci = WmiRow.Build(("DeviceID", new WmiValue("PCIBus")), ("Name", new WmiValue("PCI Bus")));
        var isa = WmiRow.Build(("DeviceID", new WmiValue("ISABus")), ("Name", new WmiValue("ISA Bus")));

        var provider = new FakeWmiProvider("Win32_Bus", new[] { pci, isa });
        var results = await provider.ToSafeBusMetricsAsync(CancellationToken.None);

        Assert.Equal(2, results.Count);
        Assert.Equal("PCI Bus", results[0].Name);
        Assert.Equal("ISA Bus", results[1].Name);
    }

    [Fact]
    public async Task PartialData_Leaves_Missing_Fields_Null()
    {
        var partial = WmiRow.Build(("Name", new WmiValue("Bus Without Type")));

        var provider = new FakeWmiProvider("Win32_Bus", new[] { partial });
        var results = await provider.ToSafeBusMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("Bus Without Type", results[0].Name);
        Assert.Null(results[0].BusType);
        Assert.Null(results[0].PowerManagementCapabilities);
    }

    [Fact]
    public async Task WrongTypeValue_Is_Ignored_Not_Miscast()
    {
        // BusType stored as a bool instead of Int — should be treated as absent, not throw.
        var badRow = WmiRow.Build(("BusType", new WmiValue(true)));

        var provider = new FakeWmiProvider("Win32_Bus", new[] { badRow });
        var results = await provider.ToSafeBusMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Null(results[0].BusType);
    }
}
