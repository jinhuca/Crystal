using Crystal.Mmi.HardwareFeatures.AssociatedProcessorMemory;
using Crystal.Mmi.MmiEngine;
using Crystal.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Mmi.Tests.HardwareFeatures;

public class AssociatedProcessorMemoryExtensionsTests
{
    private const string AntecedentPath = "Win32_CacheMemory.DeviceID=\"Cache Memory 0\"";
    private const string DependentPath = "Win32_Processor.DeviceID=\"CPU0\"";

    private static System.Collections.Frozen.FrozenDictionary<string, WmiValue> RelationshipRow() => WmiRow.Build(
        ("Antecedent", new WmiValue(AntecedentPath)),
        ("BusSpeed", new WmiValue(4000)),
        ("Dependent", new WmiValue(DependentPath))
    );

    [Fact]
    public async Task FullData_Maps_Antecedent_And_Dependent()
    {
        var provider = new FakeWmiProvider("Win32_AssociatedProcessorMemory", new[] { RelationshipRow() });
        var results = await provider.ToSafeAssociatedProcessorMemoryMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal(AntecedentPath, results[0].Antecedent);
        Assert.Equal(DependentPath, results[0].Dependent);
    }

    [Fact]
    public async Task FullData_Maps_BusSpeed_Uint()
    {
        var provider = new FakeWmiProvider("Win32_AssociatedProcessorMemory", new[] { RelationshipRow() });
        var results = await provider.ToSafeAssociatedProcessorMemoryMetricsAsync(CancellationToken.None);

        Assert.Equal(4000u, results[0].BusSpeed);
    }

    [Fact]
    public async Task FullData_Extracts_CacheMemoryDeviceId()
    {
        var provider = new FakeWmiProvider("Win32_AssociatedProcessorMemory", new[] { RelationshipRow() });
        var results = await provider.ToSafeAssociatedProcessorMemoryMetricsAsync(CancellationToken.None);

        Assert.Equal("Cache Memory 0", results[0].CacheMemoryDeviceId);
    }

    [Fact]
    public async Task FullData_Extracts_ProcessorDeviceId()
    {
        var provider = new FakeWmiProvider("Win32_AssociatedProcessorMemory", new[] { RelationshipRow() });
        var results = await provider.ToSafeAssociatedProcessorMemoryMetricsAsync(CancellationToken.None);

        Assert.Equal("CPU0", results[0].ProcessorDeviceId);
    }

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("Win32_AssociatedProcessorMemory", WmiRow.Empty());
        var results = await provider.ToSafeAssociatedProcessorMemoryMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MissingClass_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("SomeOtherClass", WmiRow.Empty());
        var results = await provider.ToSafeAssociatedProcessorMemoryMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MultipleRelationships_Returns_All()
    {
        var rel1 = WmiRow.Build(("Dependent", new WmiValue(DependentPath)));
        var rel2 = WmiRow.Build(("Dependent", new WmiValue("Win32_Processor.DeviceID=\"CPU1\"")));

        var provider = new FakeWmiProvider("Win32_AssociatedProcessorMemory", new[] { rel1, rel2 });
        var results = await provider.ToSafeAssociatedProcessorMemoryMetricsAsync(CancellationToken.None);

        Assert.Equal(2, results.Count);
        Assert.Equal("CPU0", results[0].ProcessorDeviceId);
        Assert.Equal("CPU1", results[1].ProcessorDeviceId);
    }

    [Fact]
    public async Task MissingReference_Leaves_Field_Null()
    {
        var partial = WmiRow.Build(("Dependent", new WmiValue(DependentPath)));

        var provider = new FakeWmiProvider("Win32_AssociatedProcessorMemory", new[] { partial });
        var results = await provider.ToSafeAssociatedProcessorMemoryMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Null(results[0].Antecedent);
        Assert.Null(results[0].CacheMemoryDeviceId);
        Assert.Null(results[0].BusSpeed);
    }

    [Fact]
    public async Task WrongTypeValue_Is_Ignored_Not_Miscast()
    {
        // BusSpeed stored as a bool instead of Int — should be treated as absent, not throw.
        var badRow = WmiRow.Build(("BusSpeed", new WmiValue(true)));

        var provider = new FakeWmiProvider("Win32_AssociatedProcessorMemory", new[] { badRow });
        var results = await provider.ToSafeAssociatedProcessorMemoryMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Null(results[0].BusSpeed);
    }
}
