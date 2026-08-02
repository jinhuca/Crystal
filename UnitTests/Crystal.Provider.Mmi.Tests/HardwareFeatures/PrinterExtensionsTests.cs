using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Provider.Mmi.HardwareFeatures.Printer;
using Crystal.Provider.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Provider.Mmi.Tests.HardwareFeatures;

public class PrinterExtensionsTests
{
    private static System.Collections.Frozen.FrozenDictionary<string, WmiValue> OfficePrinterRow() => WmiRow.Build(
        ("Name", new WmiValue("HP LaserJet Pro")),
        ("Caption", new WmiValue("HP LaserJet Pro")),
        ("Description", new WmiValue("Office laser printer")),
        ("DeviceID", new WmiValue("HP LaserJet Pro")),
        ("Default", new WmiValue(true)),
        ("Shared", new WmiValue(true)),
        ("ShareName", new WmiValue("HPLASER")),
        ("Local", new WmiValue(true)),
        ("Network", new WmiValue(false)),
        ("PortName", new WmiValue("USB001")),
        ("DriverName", new WmiValue("HP Universal Printing PCL 6")),
        ("PrinterStatus", new WmiValue(3)),
        ("PrinterState", new WmiValue(0)),
        ("JobCountSinceLastReset", new WmiValue(42)),
        ("HorizontalResolution", new WmiValue(600)),
        ("VerticalResolution", new WmiValue(600)),
        ("PrinterPaperNames", new WmiValue(new[] { "A4", "Letter" })),
        ("PaperSizesSupported", new WmiValue(new ushort[] { 1, 2 })),
        ("Status", new WmiValue("OK")),
        ("StartTime", new WmiValue(new DateTime(2020, 1, 1, 8, 0, 0, DateTimeKind.Utc)))
    );

    [Fact]
    public async Task FullData_Maps_Name()
    {
        var provider = new FakeWmiProvider("Win32_Printer", new[] { OfficePrinterRow() });
        var results = await provider.ToSafePrinterMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("HP LaserJet Pro", results[0].Name);
    }

    [Fact]
    public async Task FullData_Maps_Default_True()
    {
        var provider = new FakeWmiProvider("Win32_Printer", new[] { OfficePrinterRow() });
        var results = await provider.ToSafePrinterMetricsAsync(CancellationToken.None);

        Assert.True(results[0].Default);
    }

    [Fact]
    public async Task FullData_Maps_Shared_True()
    {
        var provider = new FakeWmiProvider("Win32_Printer", new[] { OfficePrinterRow() });
        var results = await provider.ToSafePrinterMetricsAsync(CancellationToken.None);

        Assert.True(results[0].Shared);
    }

    [Fact]
    public async Task FullData_Maps_PrinterStatus_Ushort()
    {
        var provider = new FakeWmiProvider("Win32_Printer", new[] { OfficePrinterRow() });
        var results = await provider.ToSafePrinterMetricsAsync(CancellationToken.None);

        Assert.Equal((ushort)3, results[0].PrinterStatus);
    }

    [Fact]
    public async Task FullData_Maps_JobCountSinceLastReset_Uint()
    {
        var provider = new FakeWmiProvider("Win32_Printer", new[] { OfficePrinterRow() });
        var results = await provider.ToSafePrinterMetricsAsync(CancellationToken.None);

        Assert.Equal((uint)42, results[0].JobCountSinceLastReset);
    }

    [Fact]
    public async Task FullData_Maps_PortName()
    {
        var provider = new FakeWmiProvider("Win32_Printer", new[] { OfficePrinterRow() });
        var results = await provider.ToSafePrinterMetricsAsync(CancellationToken.None);

        Assert.Equal("USB001", results[0].PortName);
    }

    [Fact]
    public async Task FullData_Maps_PrinterPaperNames_StringArray()
    {
        var provider = new FakeWmiProvider("Win32_Printer", new[] { OfficePrinterRow() });
        var results = await provider.ToSafePrinterMetricsAsync(CancellationToken.None);

        Assert.Equal(new[] { "A4", "Letter" }, results[0].PrinterPaperNames);
    }

    [Fact]
    public async Task FullData_Maps_PaperSizesSupported_UShortArray()
    {
        var provider = new FakeWmiProvider("Win32_Printer", new[] { OfficePrinterRow() });
        var results = await provider.ToSafePrinterMetricsAsync(CancellationToken.None);

        Assert.Equal(new ushort[] { 1, 2 }, results[0].PaperSizesSupported);
    }

    [Fact]
    public async Task FullData_Maps_Status()
    {
        var provider = new FakeWmiProvider("Win32_Printer", new[] { OfficePrinterRow() });
        var results = await provider.ToSafePrinterMetricsAsync(CancellationToken.None);

        Assert.Equal("OK", results[0].Status);
    }

    [Fact]
    public async Task FullData_Maps_StartTime_DateTime()
    {
        var provider = new FakeWmiProvider("Win32_Printer", new[] { OfficePrinterRow() });
        var results = await provider.ToSafePrinterMetricsAsync(CancellationToken.None);

        Assert.Equal(new DateTime(2020, 1, 1, 8, 0, 0, DateTimeKind.Utc), results[0].StartTime);
    }

    [Fact]
    public async Task MultiplePrinters_Returns_All()
    {
        var p1 = WmiRow.Build(("Name", new WmiValue("printer1")), ("PortName", new WmiValue("USB001")));
        var p2 = WmiRow.Build(("Name", new WmiValue("printer2")), ("PortName", new WmiValue("USB002")));
        var p3 = WmiRow.Build(("Name", new WmiValue("printer3")), ("PortName", new WmiValue("LPT1")));

        var provider = new FakeWmiProvider("Win32_Printer", new[] { p1, p2, p3 });
        var results = await provider.ToSafePrinterMetricsAsync(CancellationToken.None);

        Assert.Equal(3, results.Count);
        Assert.Equal("printer1", results[0].Name);
        Assert.Equal("printer2", results[1].Name);
        Assert.Equal("printer3", results[2].Name);
    }

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("Win32_Printer", WmiRow.Empty());
        var results = await provider.ToSafePrinterMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MissingKeys_Return_Null_Fields()
    {
        var row = WmiRow.Build(
            ("Name", new WmiValue("minimal")),
            ("Default", new WmiValue(false)));
        var provider = new FakeWmiProvider("Win32_Printer", new[] { row });
        var results = await provider.ToSafePrinterMetricsAsync(CancellationToken.None);

        Assert.Equal("minimal", results[0].Name);
        Assert.False(results[0].Default);
        Assert.Null(results[0].PortName);
        Assert.Null(results[0].DriverName);
        Assert.Null(results[0].PrinterStatus);
        Assert.Null(results[0].PrinterPaperNames);
        Assert.Null(results[0].PaperSizesSupported);
    }

    [Fact]
    public async Task Local_Printer_Maps_Network_False()
    {
        var row = WmiRow.Build(
            ("Name", new WmiValue("localPrinter")),
            ("Local", new WmiValue(true)),
            ("Network", new WmiValue(false)));
        var provider = new FakeWmiProvider("Win32_Printer", new[] { row });
        var results = await provider.ToSafePrinterMetricsAsync(CancellationToken.None);

        Assert.True(results[0].Local);
        Assert.False(results[0].Network);
    }
}
