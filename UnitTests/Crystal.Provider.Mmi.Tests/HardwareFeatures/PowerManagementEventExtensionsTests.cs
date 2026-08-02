using Crystal.Provider.Mmi.HardwareFeatures.PowerManagementEvent;
using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Provider.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Provider.Mmi.Tests.HardwareFeatures;

public class PowerManagementEventExtensionsTests
{
    [Fact]
    public async Task FullData_Maps_EventType_Ushort()
    {
        var row = WmiRow.Build(("EventType", new WmiValue(7))); // 7 = Resume From Suspend

        var provider = new FakeWmiProvider("Win32_PowerManagementEvent", new[] { row });
        var results = await provider.ToSafePowerManagementEventMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal((ushort)7, results[0].EventType);
    }

    [Fact]
    public async Task FullData_Maps_OEMEventCode()
    {
        var row = WmiRow.Build(
            ("EventType", new WmiValue(11)), // 11 = OEM Event
            ("OEMEventCode", new WmiValue(42)));

        var provider = new FakeWmiProvider("Win32_PowerManagementEvent", new[] { row });
        var results = await provider.ToSafePowerManagementEventMetricsAsync(CancellationToken.None);

        Assert.Equal((ushort)42, results[0].OEMEventCode);
    }

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        // Represents the common real-world case: no event is queued outside an active subscription.
        var provider = new FakeWmiProvider("Win32_PowerManagementEvent", WmiRow.Empty());
        var results = await provider.ToSafePowerManagementEventMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MissingClass_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("SomeOtherClass", WmiRow.Empty());
        var results = await provider.ToSafePowerManagementEventMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MultipleEvents_Returns_All()
    {
        var e1 = WmiRow.Build(("EventType", new WmiValue(4)));
        var e2 = WmiRow.Build(("EventType", new WmiValue(7)));

        var provider = new FakeWmiProvider("Win32_PowerManagementEvent", new[] { e1, e2 });
        var results = await provider.ToSafePowerManagementEventMetricsAsync(CancellationToken.None);

        Assert.Equal(2, results.Count);
        Assert.Equal((ushort)4, results[0].EventType);
        Assert.Equal((ushort)7, results[1].EventType);
    }

    [Fact]
    public async Task PartialData_Leaves_Missing_Fields_Null()
    {
        var partial = WmiRow.Build(("EventType", new WmiValue(10)));

        var provider = new FakeWmiProvider("Win32_PowerManagementEvent", new[] { partial });
        var results = await provider.ToSafePowerManagementEventMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal((ushort)10, results[0].EventType);
        Assert.Null(results[0].OEMEventCode);
    }

    [Fact]
    public async Task WrongTypeValue_Is_Ignored_Not_Miscast()
    {
        // EventType stored as a string instead of Int — should be treated as absent, not throw.
        var badRow = WmiRow.Build(("EventType", new WmiValue("7")));

        var provider = new FakeWmiProvider("Win32_PowerManagementEvent", new[] { badRow });
        var results = await provider.ToSafePowerManagementEventMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Null(results[0].EventType);
    }
}
