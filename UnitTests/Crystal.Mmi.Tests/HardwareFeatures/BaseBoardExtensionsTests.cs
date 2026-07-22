using Crystal.Mmi.HardwareFeatures.BaseBoard;
using Crystal.Mmi.MmiEngine;
using Crystal.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Mmi.Tests.HardwareFeatures;

public class BaseBoardExtensionsTests
{
    private static System.Collections.Frozen.FrozenDictionary<string, WmiValue> FullRow() => WmiRow.Build(
        ("Caption", new WmiValue("Base Board")),
        ("CreationClassName", new WmiValue("Win32_BaseBoard")),
        ("Description", new WmiValue("Base Board")),
        ("HostingBoard", new WmiValue(true)),
        ("HotSwappable", new WmiValue(false)),
        ("InstallationDate", new WmiValue("N/A")),
        ("Manufacturer", new WmiValue("ASUSTeK COMPUTER INC.")),
        ("Model", new WmiValue("")),
        ("Name", new WmiValue("Base Board")),
        ("PartNumber", new WmiValue("0 ")),
        ("Removable", new WmiValue(false)),
        ("Replaceable", new WmiValue(true)),
        ("Requirements", new WmiValue("")),
        ("SerialNumber", new WmiValue("SN-MOBO-01")),
        ("SKU", new WmiValue("")),
        ("SlotLayout", new WmiValue("")),
        ("SpecialRequirements", new WmiValue("")),
        ("Status", new WmiValue("OK")),
        ("Tag", new WmiValue("Base Board")),
        ("Version", new WmiValue("Rev X.0x"))
        // Note: Weight and Width are float but stored via Int in WMI mapper
    );

    [Fact]
    public async Task FullData_Maps_Manufacturer()
    {
        var provider = new FakeWmiProvider("Win32_BaseBoard", new[] { FullRow() });
        var result = await provider.ToSafeBaseBoardMetricsAsync(CancellationToken.None);

        Assert.Equal("ASUSTeK COMPUTER INC.", result.Manufacturer);
    }

    [Fact]
    public async Task FullData_Maps_Caption()
    {
        var provider = new FakeWmiProvider("Win32_BaseBoard", new[] { FullRow() });
        var result = await provider.ToSafeBaseBoardMetricsAsync(CancellationToken.None);

        Assert.Equal("Base Board", result.Caption);
    }

    [Fact]
    public async Task FullData_Maps_SerialNumber()
    {
        var provider = new FakeWmiProvider("Win32_BaseBoard", new[] { FullRow() });
        var result = await provider.ToSafeBaseBoardMetricsAsync(CancellationToken.None);

        Assert.Equal("SN-MOBO-01", result.SerialNumber);
    }

    [Fact]
    public async Task FullData_Maps_HostingBoard_True()
    {
        var provider = new FakeWmiProvider("Win32_BaseBoard", new[] { FullRow() });
        var result = await provider.ToSafeBaseBoardMetricsAsync(CancellationToken.None);

        Assert.True(result.HostingBoard);
    }

    [Fact]
    public async Task FullData_Maps_HotSwappable_False()
    {
        var provider = new FakeWmiProvider("Win32_BaseBoard", new[] { FullRow() });
        var result = await provider.ToSafeBaseBoardMetricsAsync(CancellationToken.None);

        Assert.False(result.HotSwappable);
    }

    [Fact]
    public async Task FullData_Maps_Removable_False()
    {
        var provider = new FakeWmiProvider("Win32_BaseBoard", new[] { FullRow() });
        var result = await provider.ToSafeBaseBoardMetricsAsync(CancellationToken.None);

        Assert.False(result.Removable);
    }

    [Fact]
    public async Task FullData_Maps_Replaceable_True()
    {
        var provider = new FakeWmiProvider("Win32_BaseBoard", new[] { FullRow() });
        var result = await provider.ToSafeBaseBoardMetricsAsync(CancellationToken.None);

        Assert.True(result.Replaceable);
    }

    [Fact]
    public async Task FullData_Maps_Version()
    {
        var provider = new FakeWmiProvider("Win32_BaseBoard", new[] { FullRow() });
        var result = await provider.ToSafeBaseBoardMetricsAsync(CancellationToken.None);

        Assert.Equal("Rev X.0x", result.Version);
    }

    [Fact]
    public async Task FullData_Maps_Tag()
    {
        var provider = new FakeWmiProvider("Win32_BaseBoard", new[] { FullRow() });
        var result = await provider.ToSafeBaseBoardMetricsAsync(CancellationToken.None);

        Assert.Equal("Base Board", result.Tag);
    }

    [Fact]
    public async Task FullData_Maps_Status()
    {
        var provider = new FakeWmiProvider("Win32_BaseBoard", new[] { FullRow() });
        var result = await provider.ToSafeBaseBoardMetricsAsync(CancellationToken.None);

        Assert.Equal("OK", result.Status);
    }

    [Fact]
    public async Task Weight_Maps_Via_Int_Cast_To_Float()
    {
        // The extension casts int WMI value to float for Weight/Width
        var row = WmiRow.Build(
            ("Caption", new WmiValue("Base Board")),
            ("Weight", new WmiValue(2)));   // stored as Int in WMI mapper
        var provider = new FakeWmiProvider("Win32_BaseBoard", new[] { row });
        var result = await provider.ToSafeBaseBoardMetricsAsync(CancellationToken.None);

        Assert.Equal(2.0f, result.Weight);
    }

    [Fact]
    public async Task Width_Maps_Via_Int_Cast_To_Float()
    {
        var row = WmiRow.Build(
            ("Caption", new WmiValue("Base Board")),
            ("Width", new WmiValue(305)));
        var provider = new FakeWmiProvider("Win32_BaseBoard", new[] { row });
        var result = await provider.ToSafeBaseBoardMetricsAsync(CancellationToken.None);

        Assert.Equal(305.0f, result.Width);
    }

    [Fact]
    public async Task Weight_Null_When_Key_Missing()
    {
        var provider = new FakeWmiProvider("Win32_BaseBoard", new[] { FullRow() });
        var result = await provider.ToSafeBaseBoardMetricsAsync(CancellationToken.None);

        Assert.Null(result.Weight);
    }

    [Fact]
    public async Task EmptyInstances_Returns_All_Null_Fallback()
    {
        var provider = new FakeWmiProvider("Win32_BaseBoard", WmiRow.Empty());
        var result = await provider.ToSafeBaseBoardMetricsAsync(CancellationToken.None);

        Assert.Null(result.Manufacturer);
        Assert.Null(result.Caption);
        Assert.Null(result.SerialNumber);
        Assert.Null(result.HostingBoard);
    }

    [Fact]
    public async Task Cancelled_Token_Returns_Fallback_Not_Throw()
    {
        // BaseBoardExtensions uses generic catch — swallows OperationCanceledException
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var provider = new FakeWmiProvider("Win32_BaseBoard", new[] { FullRow() });
        var result = await provider.ToSafeBaseBoardMetricsAsync(cts.Token);

        Assert.NotNull(result);
        Assert.Null(result.Manufacturer);
    }

    [Fact]
    public async Task MissingKeys_Return_Null_For_Those_Fields()
    {
        var row = WmiRow.Build(("Manufacturer", new WmiValue("Gigabyte")));
        var provider = new FakeWmiProvider("Win32_BaseBoard", new[] { row });
        var result = await provider.ToSafeBaseBoardMetricsAsync(CancellationToken.None);

        Assert.Equal("Gigabyte", result.Manufacturer);
        Assert.Null(result.Caption);
        Assert.Null(result.SerialNumber);
        Assert.Null(result.HostingBoard);
        Assert.Null(result.Weight);
    }
}
