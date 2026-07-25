using Crystal.Mmi.HardwareFeatures.DMAChannel;
using Crystal.Mmi.MmiEngine;
using Crystal.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Mmi.Tests.HardwareFeatures;

public class DMAChannelExtensionsTests
{
    private static System.Collections.Frozen.FrozenDictionary<string, WmiValue> ChannelRow() => WmiRow.Build(
        ("AddressSize", new WmiValue(24)),
        ("Availability", new WmiValue(3)),
        ("BurstMode", new WmiValue(false)),
        ("ByteMode", new WmiValue(1)),
        ("Caption", new WmiValue("DMA Channel 4")),
        ("ChannelTiming", new WmiValue(0)),
        ("CreationClassName", new WmiValue("Win32_DMAChannel")),
        ("CSCreationClassName", new WmiValue("Win32_ComputerSystem")),
        ("CSName", new WmiValue("DESKTOP-01")),
        ("Description", new WmiValue("DMA Channel 4")),
        ("DMAChannel", new WmiValue(4)),
        ("InstallDate", new WmiValue(new DateTime(2020, 3, 3, 0, 0, 0, DateTimeKind.Utc))),
        ("MaxTransferSize", new WmiValue(65536)),
        ("Name", new WmiValue("DMA Channel 4")),
        ("Port", new WmiValue(0)),
        ("Status", new WmiValue("OK")),
        ("TransferWidths", new WmiValue(new ushort[] { 8, 16 })),
        ("TypeCTiming", new WmiValue(0)),
        ("WordMode", new WmiValue(1))
    );

    [Fact]
    public async Task FullData_Maps_Name()
    {
        var provider = new FakeWmiProvider("Win32_DMAChannel", new[] { ChannelRow() });
        var results = await provider.ToSafeDMAChannelMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("DMA Channel 4", results[0].Name);
    }

    [Fact]
    public async Task FullData_Maps_DMAChannel_Uint()
    {
        var provider = new FakeWmiProvider("Win32_DMAChannel", new[] { ChannelRow() });
        var results = await provider.ToSafeDMAChannelMetricsAsync(CancellationToken.None);

        Assert.Equal(4u, results[0].DMAChannel);
    }

    [Fact]
    public async Task FullData_Maps_CSName()
    {
        var provider = new FakeWmiProvider("Win32_DMAChannel", new[] { ChannelRow() });
        var results = await provider.ToSafeDMAChannelMetricsAsync(CancellationToken.None);

        Assert.Equal("DESKTOP-01", results[0].CSName);
    }

    [Fact]
    public async Task FullData_Maps_TransferWidths_Array()
    {
        var provider = new FakeWmiProvider("Win32_DMAChannel", new[] { ChannelRow() });
        var results = await provider.ToSafeDMAChannelMetricsAsync(CancellationToken.None);

        Assert.Equal(new ushort[] { 8, 16 }, results[0].TransferWidths);
    }

    [Fact]
    public async Task FullData_Maps_Status_OK()
    {
        var provider = new FakeWmiProvider("Win32_DMAChannel", new[] { ChannelRow() });
        var results = await provider.ToSafeDMAChannelMetricsAsync(CancellationToken.None);

        Assert.Equal("OK", results[0].Status);
    }

    [Fact]
    public async Task FullData_Maps_InstallDate()
    {
        var provider = new FakeWmiProvider("Win32_DMAChannel", new[] { ChannelRow() });
        var results = await provider.ToSafeDMAChannelMetricsAsync(CancellationToken.None);

        Assert.Equal(new DateTime(2020, 3, 3, 0, 0, 0, DateTimeKind.Utc), results[0].InstallDate);
    }

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("Win32_DMAChannel", WmiRow.Empty());
        var results = await provider.ToSafeDMAChannelMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MissingClass_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("SomeOtherClass", WmiRow.Empty());
        var results = await provider.ToSafeDMAChannelMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MultipleChannels_Returns_All()
    {
        var ch1 = WmiRow.Build(("DMAChannel", new WmiValue(1)));
        var ch2 = WmiRow.Build(("DMAChannel", new WmiValue(2)));

        var provider = new FakeWmiProvider("Win32_DMAChannel", new[] { ch1, ch2 });
        var results = await provider.ToSafeDMAChannelMetricsAsync(CancellationToken.None);

        Assert.Equal(2, results.Count);
        Assert.Equal(1u, results[0].DMAChannel);
        Assert.Equal(2u, results[1].DMAChannel);
    }

    [Fact]
    public async Task PartialData_Leaves_Missing_Fields_Null()
    {
        var partial = WmiRow.Build(("DMAChannel", new WmiValue(3)));

        var provider = new FakeWmiProvider("Win32_DMAChannel", new[] { partial });
        var results = await provider.ToSafeDMAChannelMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal(3u, results[0].DMAChannel);
        Assert.Null(results[0].TransferWidths);
        Assert.Null(results[0].CSName);
    }

    [Fact]
    public async Task WrongTypeValue_Is_Ignored_Not_Miscast()
    {
        // DMAChannel stored as a string instead of Int — should be treated as absent, not throw.
        var badRow = WmiRow.Build(("DMAChannel", new WmiValue("4")));

        var provider = new FakeWmiProvider("Win32_DMAChannel", new[] { badRow });
        var results = await provider.ToSafeDMAChannelMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Null(results[0].DMAChannel);
    }
}
