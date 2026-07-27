using Crystal.Mmi.HardwareFeatures.PCMCIAController;
using Crystal.Mmi.MmiEngine;
using Crystal.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Mmi.Tests.HardwareFeatures;

public class PCMCIAControllerExtensionsTests
{
    private static System.Collections.Frozen.FrozenDictionary<string, WmiValue> FullRow() => WmiRow.Build(
        ("Availability", new WmiValue(3)),
        ("Caption", new WmiValue("PCMCIA Controller")),
        ("ConfigManagerErrorCode", new WmiValue(0)),
        ("ConfigManagerUserConfig", new WmiValue(false)),
        ("CreationClassName", new WmiValue("Win32_PCMCIAController")),
        ("Description", new WmiValue("PCMCIA Controller")),
        ("DeviceID", new WmiValue("PCMCIA0")),
        ("ErrorCleared", new WmiValue(false)),
        ("ErrorDescription", new WmiValue("")),
        ("InstallDate", new WmiValue(new DateTime(2021, 5, 1, 0, 0, 0, DateTimeKind.Utc))),
        ("LastErrorCode", new WmiValue(0)),
        ("Manufacturer", new WmiValue("Texas Instruments")),
        ("MaxNumberControlled", new WmiValue(4)),
        ("Name", new WmiValue("PCMCIA Controller")),
        ("PNPDeviceID", new WmiValue("PCI\\VEN_104C&DEV_AC50\\0")),
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
    public async Task FullData_Maps_DeviceID_And_Manufacturer()
    {
        var provider = new FakeWmiProvider("Win32_PCMCIAController", new[] { FullRow() });
        var results = await provider.ToSafePCMCIAControllerMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("PCMCIA0", results[0].DeviceID);
        Assert.Equal("Texas Instruments", results[0].Manufacturer);
    }

    [Fact]
    public async Task FullData_Maps_MaxNumberControlled_Uint()
    {
        var provider = new FakeWmiProvider("Win32_PCMCIAController", new[] { FullRow() });
        var results = await provider.ToSafePCMCIAControllerMetricsAsync(CancellationToken.None);

        Assert.Equal(4u, results[0].MaxNumberControlled);
    }

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("Win32_PCMCIAController", WmiRow.Empty());
        var results = await provider.ToSafePCMCIAControllerMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MissingClass_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("SomeOtherClass", WmiRow.Empty());
        var results = await provider.ToSafePCMCIAControllerMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task PartialData_Leaves_Missing_Fields_Null()
    {
        var partial = WmiRow.Build(("Name", new WmiValue("Bare PCMCIA Controller")));

        var provider = new FakeWmiProvider("Win32_PCMCIAController", new[] { partial });
        var results = await provider.ToSafePCMCIAControllerMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("Bare PCMCIA Controller", results[0].Name);
        Assert.Null(results[0].Manufacturer);
    }

    [Fact]
    public async Task WrongTypeValue_Is_Ignored_Not_Miscast()
    {
        // Availability stored as a string instead of Int — should be treated as absent, not throw.
        var badRow = WmiRow.Build(("Availability", new WmiValue("not-a-number")));

        var provider = new FakeWmiProvider("Win32_PCMCIAController", new[] { badRow });
        var results = await provider.ToSafePCMCIAControllerMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Null(results[0].Availability);
    }

    [Fact]
    public async Task MultipleControllers_Returns_All()
    {
        var a = WmiRow.Build(("DeviceID", new WmiValue("PCMCIA0")));
        var b = WmiRow.Build(("DeviceID", new WmiValue("PCMCIA1")));

        var provider = new FakeWmiProvider("Win32_PCMCIAController", new[] { a, b });
        var results = await provider.ToSafePCMCIAControllerMetricsAsync(CancellationToken.None);

        Assert.Equal(2, results.Count);
    }
}
