using Crystal.Mmi.HardwareFeatures.ParallelPort;
using Crystal.Mmi.MmiEngine;
using Crystal.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Mmi.Tests.HardwareFeatures;

public class ParallelPortExtensionsTests
{
    private static System.Collections.Frozen.FrozenDictionary<string, WmiValue> Lpt1Row() => WmiRow.Build(
        ("Availability", new WmiValue(3)),
        ("Capabilities", new WmiValue(new ushort[] { 4 })),
        ("CapabilityDescriptions", new WmiValue(new[] { "ECP" })),
        ("Caption", new WmiValue("Printer Port (LPT1)")),
        ("ConfigManagerErrorCode", new WmiValue(0)),
        ("ConfigManagerUserConfig", new WmiValue(false)),
        ("CreationClassName", new WmiValue("Win32_ParallelPort")),
        ("Description", new WmiValue("Printer Port")),
        ("DeviceID", new WmiValue("LPT1")),
        ("DMASupport", new WmiValue(true)),
        ("ErrorCleared", new WmiValue(false)),
        ("ErrorDescription", new WmiValue("")),
        ("InstallDate", new WmiValue(new DateTime(2023, 2, 10, 0, 0, 0, DateTimeKind.Utc))),
        ("LastErrorCode", new WmiValue(0)),
        ("MaxNumberControlled", new WmiValue(0)),
        ("Name", new WmiValue("Printer Port (LPT1)")),
        ("OSAutoDiscovered", new WmiValue(true)),
        ("PNPDeviceID", new WmiValue("ACPI\\PNP0400\\1")),
        ("PowerManagementCapabilities", new WmiValue(new ushort[] { 1 })),
        ("PowerManagementSupported", new WmiValue(false)),
        ("ProtocolSupported", new WmiValue(17)),
        ("Status", new WmiValue("OK")),
        ("StatusInfo", new WmiValue(3)),
        ("SystemCreationClassName", new WmiValue("Win32_ComputerSystem")),
        ("SystemName", new WmiValue("DESKTOP-01")),
        ("TimeOfLastReset", new WmiValue(new DateTime(2024, 5, 20, 9, 0, 0, DateTimeKind.Utc)))
    );

    [Fact]
    public async Task FullData_Maps_Name()
    {
        var provider = new FakeWmiProvider("Win32_ParallelPort", new[] { Lpt1Row() });
        var results = await provider.ToSafeParallelPortMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("Printer Port (LPT1)", results[0].Name);
    }

    [Fact]
    public async Task FullData_Maps_DeviceID()
    {
        var provider = new FakeWmiProvider("Win32_ParallelPort", new[] { Lpt1Row() });
        var results = await provider.ToSafeParallelPortMetricsAsync(CancellationToken.None);

        Assert.Equal("LPT1", results[0].DeviceID);
    }

    [Fact]
    public async Task FullData_Maps_Availability_Ushort()
    {
        var provider = new FakeWmiProvider("Win32_ParallelPort", new[] { Lpt1Row() });
        var results = await provider.ToSafeParallelPortMetricsAsync(CancellationToken.None);

        Assert.Equal((ushort)3, results[0].Availability);
    }

    [Fact]
    public async Task FullData_Maps_DMASupport_True()
    {
        var provider = new FakeWmiProvider("Win32_ParallelPort", new[] { Lpt1Row() });
        var results = await provider.ToSafeParallelPortMetricsAsync(CancellationToken.None);

        Assert.True(results[0].DMASupport);
    }

    [Fact]
    public async Task FullData_Maps_Capabilities_Array()
    {
        var provider = new FakeWmiProvider("Win32_ParallelPort", new[] { Lpt1Row() });
        var results = await provider.ToSafeParallelPortMetricsAsync(CancellationToken.None);

        Assert.Equal(new ushort[] { 4 }, results[0].Capabilities);
    }

    [Fact]
    public async Task FullData_Maps_CapabilityDescriptions_Flattened()
    {
        var provider = new FakeWmiProvider("Win32_ParallelPort", new[] { Lpt1Row() });
        var results = await provider.ToSafeParallelPortMetricsAsync(CancellationToken.None);

        Assert.Equal("ECP", results[0].CapabilityDescriptions);
    }

    [Fact]
    public async Task FullData_Maps_ProtocolSupported_Ushort()
    {
        var provider = new FakeWmiProvider("Win32_ParallelPort", new[] { Lpt1Row() });
        var results = await provider.ToSafeParallelPortMetricsAsync(CancellationToken.None);

        Assert.Equal((ushort)17, results[0].ProtocolSupported);
    }

    [Fact]
    public async Task FullData_Maps_Status_OK()
    {
        var provider = new FakeWmiProvider("Win32_ParallelPort", new[] { Lpt1Row() });
        var results = await provider.ToSafeParallelPortMetricsAsync(CancellationToken.None);

        Assert.Equal("OK", results[0].Status);
    }

    [Fact]
    public async Task FullData_Maps_InstallDate()
    {
        var provider = new FakeWmiProvider("Win32_ParallelPort", new[] { Lpt1Row() });
        var results = await provider.ToSafeParallelPortMetricsAsync(CancellationToken.None);

        Assert.Equal(new DateTime(2023, 2, 10, 0, 0, 0, DateTimeKind.Utc), results[0].InstallDate);
    }

    [Fact]
    public async Task FullData_Maps_TimeOfLastReset()
    {
        var provider = new FakeWmiProvider("Win32_ParallelPort", new[] { Lpt1Row() });
        var results = await provider.ToSafeParallelPortMetricsAsync(CancellationToken.None);

        Assert.Equal(new DateTime(2024, 5, 20, 9, 0, 0, DateTimeKind.Utc), results[0].TimeOfLastReset);
    }

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("Win32_ParallelPort", WmiRow.Empty());
        var results = await provider.ToSafeParallelPortMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MissingClass_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("SomeOtherClass", WmiRow.Empty());
        var results = await provider.ToSafeParallelPortMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MultipleParallelPorts_Returns_All()
    {
        var lpt1 = WmiRow.Build(("DeviceID", new WmiValue("LPT1")), ("Name", new WmiValue("Printer Port (LPT1)")));
        var lpt2 = WmiRow.Build(("DeviceID", new WmiValue("LPT2")), ("Name", new WmiValue("Printer Port (LPT2)")));

        var provider = new FakeWmiProvider("Win32_ParallelPort", new[] { lpt1, lpt2 });
        var results = await provider.ToSafeParallelPortMetricsAsync(CancellationToken.None);

        Assert.Equal(2, results.Count);
        Assert.Equal("Printer Port (LPT1)", results[0].Name);
        Assert.Equal("Printer Port (LPT2)", results[1].Name);
    }

    [Fact]
    public async Task PartialData_Leaves_Missing_Fields_Null()
    {
        var partial = WmiRow.Build(("Name", new WmiValue("Port Without Capabilities")));

        var provider = new FakeWmiProvider("Win32_ParallelPort", new[] { partial });
        var results = await provider.ToSafeParallelPortMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("Port Without Capabilities", results[0].Name);
        Assert.Null(results[0].Capabilities);
        Assert.Null(results[0].CapabilityDescriptions);
        Assert.Null(results[0].DMASupport);
    }

    [Fact]
    public async Task WrongTypeValue_Is_Ignored_Not_Miscast()
    {
        // MaxNumberControlled stored as a bool instead of Int — should be treated as absent, not throw.
        var badRow = WmiRow.Build(("MaxNumberControlled", new WmiValue(true)));

        var provider = new FakeWmiProvider("Win32_ParallelPort", new[] { badRow });
        var results = await provider.ToSafeParallelPortMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Null(results[0].MaxNumberControlled);
    }
}
