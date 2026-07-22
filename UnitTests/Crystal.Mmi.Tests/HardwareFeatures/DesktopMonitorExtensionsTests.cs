using Crystal.Mmi.HardwareFeatures.DesktopMonitor;
using Crystal.Mmi.MmiEngine;
using Crystal.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Mmi.Tests.HardwareFeatures;

public class DesktopMonitorExtensionsTests
{
    private static System.Collections.Frozen.FrozenDictionary<string, WmiValue> FullMonitorRow() => WmiRow.Build(
        ("Availability",                new WmiValue(3)),
        ("Bandwidth",                   new WmiValue(600)),
        ("Caption",                     new WmiValue("Dell U2722D")),
        ("ConfigManagerErrorCode",      new WmiValue(0)),
        ("ConfigManagerUserConfig",     new WmiValue(false)),
        ("CreationClassName",           new WmiValue("Win32_DesktopMonitor")),
        ("Description",                 new WmiValue("Dell U2722D")),
        ("DeviceID",                    new WmiValue("DesktopMonitor1")),
        ("DisplayType",                 new WmiValue(2)),   // 2 = Multiscan Color
        ("ErrorCleared",                new WmiValue(false)),
        ("ErrorDescription",            new WmiValue("")),
        ("InstallationDate",                 new WmiValue(new DateTime(2021, 5, 10, 0, 0, 0, DateTimeKind.Utc))),
        ("IsLocked",                    new WmiValue(false)),
        ("LastErrorCode",               new WmiValue(0)),
        ("MonitorManufacturer",         new WmiValue("Dell Inc.")),
        ("MonitorType",                 new WmiValue("Dell U2722D")),
        ("Name",                        new WmiValue("Dell U2722D")),
        ("PixelsPerXLogicalInch",       new WmiValue(96)),
        ("PixelsPerYLogicalInch",       new WmiValue(96)),
        ("PNPDeviceID",                 new WmiValue("DISPLAY\\DELA10C\\4&A1B2C3&0&UID256")),
        ("PowerManagementCapabilities", new WmiValue(new ushort[] { 1, 2, 4 })),
        ("PowerManagementSupported",    new WmiValue(true)),
        ("ScreenHeight",                new WmiValue(1440)),
        ("ScreenWidth",                 new WmiValue(2560)),
        ("Status",                      new WmiValue("OK")),
        ("StatusInfo",                  new WmiValue(3)),
        ("SystemCreationClassName",     new WmiValue("Win32_ComputerSystem")),
        ("SystemName",                  new WmiValue("DESKTOP-01"))
    );

    // --- Field mapping ---

    [Fact]
    public async Task FullData_Maps_Name()
    {
        var provider = new FakeWmiProvider("Win32_DesktopMonitor", new[] { FullMonitorRow() });
        var results = await provider.ToSafeDesktopMonitorMetricsAsync(CancellationToken.None);
        Assert.Equal("Dell U2722D", results[0].Name);
    }

    [Fact]
    public async Task FullData_Maps_Caption()
    {
        var provider = new FakeWmiProvider("Win32_DesktopMonitor", new[] { FullMonitorRow() });
        var results = await provider.ToSafeDesktopMonitorMetricsAsync(CancellationToken.None);
        Assert.Equal("Dell U2722D", results[0].Caption);
    }

    [Fact]
    public async Task FullData_Maps_MonitorManufacturer()
    {
        var provider = new FakeWmiProvider("Win32_DesktopMonitor", new[] { FullMonitorRow() });
        var results = await provider.ToSafeDesktopMonitorMetricsAsync(CancellationToken.None);
        Assert.Equal("Dell Inc.", results[0].MonitorManufacturer);
    }

    [Fact]
    public async Task FullData_Maps_MonitorType()
    {
        var provider = new FakeWmiProvider("Win32_DesktopMonitor", new[] { FullMonitorRow() });
        var results = await provider.ToSafeDesktopMonitorMetricsAsync(CancellationToken.None);
        Assert.Equal("Dell U2722D", results[0].MonitorType);
    }

    [Fact]
    public async Task FullData_Maps_Status()
    {
        var provider = new FakeWmiProvider("Win32_DesktopMonitor", new[] { FullMonitorRow() });
        var results = await provider.ToSafeDesktopMonitorMetricsAsync(CancellationToken.None);
        Assert.Equal("OK", results[0].Status);
    }

    [Fact]
    public async Task FullData_Maps_DeviceID()
    {
        var provider = new FakeWmiProvider("Win32_DesktopMonitor", new[] { FullMonitorRow() });
        var results = await provider.ToSafeDesktopMonitorMetricsAsync(CancellationToken.None);
        Assert.Equal("DesktopMonitor1", results[0].DeviceID);
    }

    [Fact]
    public async Task FullData_Maps_DisplayType_Ushort()
    {
        var provider = new FakeWmiProvider("Win32_DesktopMonitor", new[] { FullMonitorRow() });
        var results = await provider.ToSafeDesktopMonitorMetricsAsync(CancellationToken.None);
        Assert.Equal((ushort)2, results[0].DisplayType);
    }

    [Fact]
    public async Task FullData_Maps_ScreenWidth_Uint()
    {
        var provider = new FakeWmiProvider("Win32_DesktopMonitor", new[] { FullMonitorRow() });
        var results = await provider.ToSafeDesktopMonitorMetricsAsync(CancellationToken.None);
        Assert.Equal((uint)2560, results[0].ScreenWidth);
    }

    [Fact]
    public async Task FullData_Maps_ScreenHeight_Uint()
    {
        var provider = new FakeWmiProvider("Win32_DesktopMonitor", new[] { FullMonitorRow() });
        var results = await provider.ToSafeDesktopMonitorMetricsAsync(CancellationToken.None);
        Assert.Equal((uint)1440, results[0].ScreenHeight);
    }

    [Fact]
    public async Task FullData_Maps_PixelsPerXLogicalInch_Uint()
    {
        var provider = new FakeWmiProvider("Win32_DesktopMonitor", new[] { FullMonitorRow() });
        var results = await provider.ToSafeDesktopMonitorMetricsAsync(CancellationToken.None);
        Assert.Equal((uint)96, results[0].PixelsPerXLogicalInch);
    }

    [Fact]
    public async Task FullData_Maps_PixelsPerYLogicalInch_Uint()
    {
        var provider = new FakeWmiProvider("Win32_DesktopMonitor", new[] { FullMonitorRow() });
        var results = await provider.ToSafeDesktopMonitorMetricsAsync(CancellationToken.None);
        Assert.Equal((uint)96, results[0].PixelsPerYLogicalInch);
    }

    [Fact]
    public async Task FullData_Maps_Bandwidth_Uint()
    {
        var provider = new FakeWmiProvider("Win32_DesktopMonitor", new[] { FullMonitorRow() });
        var results = await provider.ToSafeDesktopMonitorMetricsAsync(CancellationToken.None);
        Assert.Equal((uint)600, results[0].Bandwidth);
    }

    [Fact]
    public async Task FullData_Maps_Availability_Ushort()
    {
        var provider = new FakeWmiProvider("Win32_DesktopMonitor", new[] { FullMonitorRow() });
        var results = await provider.ToSafeDesktopMonitorMetricsAsync(CancellationToken.None);
        Assert.Equal((ushort)3, results[0].Availability);
    }

    [Fact]
    public async Task FullData_Maps_StatusInfo_Ushort()
    {
        var provider = new FakeWmiProvider("Win32_DesktopMonitor", new[] { FullMonitorRow() });
        var results = await provider.ToSafeDesktopMonitorMetricsAsync(CancellationToken.None);
        Assert.Equal((ushort)3, results[0].StatusInfo);
    }

    [Fact]
    public async Task FullData_Maps_ConfigManagerErrorCode_Uint()
    {
        var provider = new FakeWmiProvider("Win32_DesktopMonitor", new[] { FullMonitorRow() });
        var results = await provider.ToSafeDesktopMonitorMetricsAsync(CancellationToken.None);
        Assert.Equal((uint)0, results[0].ConfigManagerErrorCode);
    }

    [Fact]
    public async Task FullData_Maps_ConfigManagerUserConfig_False()
    {
        var provider = new FakeWmiProvider("Win32_DesktopMonitor", new[] { FullMonitorRow() });
        var results = await provider.ToSafeDesktopMonitorMetricsAsync(CancellationToken.None);
        Assert.False(results[0].ConfigManagerUserConfig);
    }

    [Fact]
    public async Task FullData_Maps_IsLocked_False()
    {
        var provider = new FakeWmiProvider("Win32_DesktopMonitor", new[] { FullMonitorRow() });
        var results = await provider.ToSafeDesktopMonitorMetricsAsync(CancellationToken.None);
        Assert.False(results[0].IsLocked);
    }

    [Fact]
    public async Task FullData_Maps_ErrorCleared_False()
    {
        var provider = new FakeWmiProvider("Win32_DesktopMonitor", new[] { FullMonitorRow() });
        var results = await provider.ToSafeDesktopMonitorMetricsAsync(CancellationToken.None);
        Assert.False(results[0].ErrorCleared);
    }

    [Fact]
    public async Task FullData_Maps_PowerManagementSupported_True()
    {
        var provider = new FakeWmiProvider("Win32_DesktopMonitor", new[] { FullMonitorRow() });
        var results = await provider.ToSafeDesktopMonitorMetricsAsync(CancellationToken.None);
        Assert.True(results[0].PowerManagementSupported);
    }

    [Fact]
    public async Task FullData_Maps_PowerManagementCapabilities_Array()
    {
        var provider = new FakeWmiProvider("Win32_DesktopMonitor", new[] { FullMonitorRow() });
        var results = await provider.ToSafeDesktopMonitorMetricsAsync(CancellationToken.None);
        Assert.Equal(new ushort[] { 1, 2, 4 }, results[0].PowerManagementCapabilities);
    }

    [Fact]
    public async Task FullData_Maps_InstallationDate()
    {
        var provider = new FakeWmiProvider("Win32_DesktopMonitor", new[] { FullMonitorRow() });
        var results = await provider.ToSafeDesktopMonitorMetricsAsync(CancellationToken.None);
        Assert.Equal(new DateTime(2021, 5, 10, 0, 0, 0, DateTimeKind.Utc), results[0].InstallationDate);
    }

    [Fact]
    public async Task FullData_Maps_SystemName()
    {
        var provider = new FakeWmiProvider("Win32_DesktopMonitor", new[] { FullMonitorRow() });
        var results = await provider.ToSafeDesktopMonitorMetricsAsync(CancellationToken.None);
        Assert.Equal("DESKTOP-01", results[0].SystemName);
    }

    [Fact]
    public async Task FullData_Maps_PNPDeviceID()
    {
        var provider = new FakeWmiProvider("Win32_DesktopMonitor", new[] { FullMonitorRow() });
        var results = await provider.ToSafeDesktopMonitorMetricsAsync(CancellationToken.None);
        Assert.Contains("DELA10C", results[0].PNPDeviceID);
    }

    // --- DisplayTypeName computed property ---

    [Theory]
    [InlineData(0, "Unknown")]
    [InlineData(1, "Other")]
    [InlineData(2, "Multiscan Color")]
    [InlineData(3, "Multiscan Monochrome")]
    [InlineData(4, "Fixed Frequency Color")]
    [InlineData(5, "Fixed Frequency Monochrome")]
    public async Task DisplayTypeName_Known_Codes(int code, string expected)
    {
        var provider = new FakeWmiProvider("Win32_DesktopMonitor",
            new[] { WmiRow.Build(("DisplayType", new WmiValue(code))) });
        var results = await provider.ToSafeDesktopMonitorMetricsAsync(CancellationToken.None);
        Assert.Equal(expected, results[0].DisplayTypeName);
    }

    [Fact]
    public async Task DisplayTypeName_Unknown_Code_Returns_Null()
    {
        var provider = new FakeWmiProvider("Win32_DesktopMonitor",
            new[] { WmiRow.Build(("DisplayType", new WmiValue(99))) });
        var results = await provider.ToSafeDesktopMonitorMetricsAsync(CancellationToken.None);
        Assert.Null(results[0].DisplayTypeName);
    }

    [Fact]
    public async Task DisplayTypeName_Null_DisplayType_Returns_Null()
    {
        var provider = new FakeWmiProvider("Win32_DesktopMonitor",
            new[] { WmiRow.Build(("Name", new WmiValue("Monitor"))) });
        var results = await provider.ToSafeDesktopMonitorMetricsAsync(CancellationToken.None);
        Assert.Null(results[0].DisplayTypeName);
    }

    // --- Multi-monitor ---

    [Fact]
    public async Task Multiple_Monitors_Returns_All()
    {
        var m1 = WmiRow.Build(("Name", new WmiValue("Dell U2722D")), ("ScreenWidth", new WmiValue(2560)));
        var m2 = WmiRow.Build(("Name", new WmiValue("LG 27UK850")), ("ScreenWidth", new WmiValue(3840)));

        var provider = new FakeWmiProvider("Win32_DesktopMonitor", new[] { m1, m2 });
        var results = await provider.ToSafeDesktopMonitorMetricsAsync(CancellationToken.None);

        Assert.Equal(2, results.Count);
        Assert.Equal("Dell U2722D", results[0].Name);
        Assert.Equal("LG 27UK850", results[1].Name);
        Assert.Equal((uint)2560, results[0].ScreenWidth);
        Assert.Equal((uint)3840, results[1].ScreenWidth);
    }

    // --- Fallback behaviour ---

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("Win32_DesktopMonitor", WmiRow.Empty());
        var results = await provider.ToSafeDesktopMonitorMetricsAsync(CancellationToken.None);
        Assert.Empty(results);
    }

    [Fact]
    public async Task MissingKeys_Return_Null_For_Those_Fields()
    {
        var provider = new FakeWmiProvider("Win32_DesktopMonitor",
            new[] { WmiRow.Build(("Name", new WmiValue("Basic Monitor"))) });
        var results = await provider.ToSafeDesktopMonitorMetricsAsync(CancellationToken.None);

        Assert.Equal("Basic Monitor", results[0].Name);
        Assert.Null(results[0].MonitorManufacturer);
        Assert.Null(results[0].ScreenWidth);
        Assert.Null(results[0].ScreenHeight);
        Assert.Null(results[0].DisplayType);
        Assert.Null(results[0].PowerManagementCapabilities);
    }

    [Fact]
    public async Task Cancelled_Token_Returns_Empty_Fallback()
    {
        // DesktopMonitor extension uses generic catch — swallows OperationCanceledException
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var provider = new FakeWmiProvider("Win32_DesktopMonitor", new[] { FullMonitorRow() });
        var results = await provider.ToSafeDesktopMonitorMetricsAsync(cts.Token);

        Assert.Empty(results);
    }

    [Fact]
    public async Task WrongValueType_For_Key_Returns_Null()
    {
        // ScreenWidth stored as String instead of Int — GetInt returns null
        var provider = new FakeWmiProvider("Win32_DesktopMonitor",
            new[] { WmiRow.Build(("ScreenWidth", new WmiValue("2560"))) });
        var results = await provider.ToSafeDesktopMonitorMetricsAsync(CancellationToken.None);

        Assert.Null(results[0].ScreenWidth);
    }
}
