using Crystal.Mmi.HardwareFeatures.USBController;
using Crystal.Mmi.MmiEngine;
using Crystal.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Mmi.Tests.HardwareFeatures;

public class USBControllerExtensionsTests
{
    private static System.Collections.Frozen.FrozenDictionary<string, WmiValue> FullControllerRow() => WmiRow.Build(
        ("Availability",                new WmiValue(3)),
        ("Caption",                     new WmiValue("Intel(R) USB 3.0 eXtensible Host Controller")),
        ("ConfigManagerErrorCode",      new WmiValue(0)),
        ("ConfigManagerUserConfig",     new WmiValue(false)),
        ("CreationClassName",           new WmiValue("Win32_USBController")),
        ("Description",                 new WmiValue("USB xHCI Compliant Host Controller")),
        ("DeviceID",                    new WmiValue("PCI\\VEN_8086&DEV_A36D&SUBSYS_86941043&REV_10\\3&11583659&0&A0")),
        ("ErrorCleared",                new WmiValue(false)),
        ("ErrorDescription",            new WmiValue("")),
        ("InstallationDate",                 new WmiValue(new DateTime(2020, 3, 15, 0, 0, 0, DateTimeKind.Utc))),
        ("LastErrorCode",               new WmiValue(0)),
        ("Manufacturer",                new WmiValue("Intel Corporation")),
        ("MaxNumberControlled",         new WmiValue(255)),
        ("Name",                        new WmiValue("Intel(R) USB 3.0 eXtensible Host Controller")),
        ("PNPDeviceID",                 new WmiValue("PCI\\VEN_8086&DEV_A36D&SUBSYS_86941043&REV_10\\3&11583659&0&A0")),
        ("PowerManagementCapabilities", new WmiValue(new ushort[] { 1, 4 })),
        ("PowerManagementSupported",    new WmiValue(false)),
        ("ProtocolSupported",           new WmiValue(16)),  // 16 = Universal Serial Bus
        ("Status",                      new WmiValue("OK")),
        ("StatusInfo",                  new WmiValue(3)),
        ("SystemCreationClassName",     new WmiValue("Win32_ComputerSystem")),
        ("SystemName",                  new WmiValue("DESKTOP-01")),
        ("TimeOfLastReset",             new WmiValue(new DateTime(2024, 1, 1, 8, 0, 0, DateTimeKind.Utc)))
    );

    // --- Field mapping ---

    [Fact]
    public async Task FullData_Maps_Name()
    {
        var provider = new FakeWmiProvider("Win32_USBController", new[] { FullControllerRow() });
        var results = await provider.ToSafeUSBControllerMetricsAsync(CancellationToken.None);
        Assert.Equal("Intel(R) USB 3.0 eXtensible Host Controller", results[0].Name);
    }

    [Fact]
    public async Task FullData_Maps_Caption()
    {
        var provider = new FakeWmiProvider("Win32_USBController", new[] { FullControllerRow() });
        var results = await provider.ToSafeUSBControllerMetricsAsync(CancellationToken.None);
        Assert.Equal("Intel(R) USB 3.0 eXtensible Host Controller", results[0].Caption);
    }

    [Fact]
    public async Task FullData_Maps_Manufacturer()
    {
        var provider = new FakeWmiProvider("Win32_USBController", new[] { FullControllerRow() });
        var results = await provider.ToSafeUSBControllerMetricsAsync(CancellationToken.None);
        Assert.Equal("Intel Corporation", results[0].Manufacturer);
    }

    [Fact]
    public async Task FullData_Maps_Status()
    {
        var provider = new FakeWmiProvider("Win32_USBController", new[] { FullControllerRow() });
        var results = await provider.ToSafeUSBControllerMetricsAsync(CancellationToken.None);
        Assert.Equal("OK", results[0].Status);
    }

    [Fact]
    public async Task FullData_Maps_DeviceID()
    {
        var provider = new FakeWmiProvider("Win32_USBController", new[] { FullControllerRow() });
        var results = await provider.ToSafeUSBControllerMetricsAsync(CancellationToken.None);
        Assert.Contains("VEN_8086", results[0].DeviceID);
    }

    [Fact]
    public async Task FullData_Maps_Description()
    {
        var provider = new FakeWmiProvider("Win32_USBController", new[] { FullControllerRow() });
        var results = await provider.ToSafeUSBControllerMetricsAsync(CancellationToken.None);
        Assert.Equal("USB xHCI Compliant Host Controller", results[0].Description);
    }

    [Fact]
    public async Task FullData_Maps_ProtocolSupported_Ushort()
    {
        var provider = new FakeWmiProvider("Win32_USBController", new[] { FullControllerRow() });
        var results = await provider.ToSafeUSBControllerMetricsAsync(CancellationToken.None);
        Assert.Equal((ushort)16, results[0].ProtocolSupported);
    }

    [Fact]
    public async Task FullData_Maps_Availability_Ushort()
    {
        var provider = new FakeWmiProvider("Win32_USBController", new[] { FullControllerRow() });
        var results = await provider.ToSafeUSBControllerMetricsAsync(CancellationToken.None);
        Assert.Equal((ushort)3, results[0].Availability);
    }

    [Fact]
    public async Task FullData_Maps_StatusInfo_Ushort()
    {
        var provider = new FakeWmiProvider("Win32_USBController", new[] { FullControllerRow() });
        var results = await provider.ToSafeUSBControllerMetricsAsync(CancellationToken.None);
        Assert.Equal((ushort)3, results[0].StatusInfo);
    }

    [Fact]
    public async Task FullData_Maps_ConfigManagerErrorCode_Uint()
    {
        var provider = new FakeWmiProvider("Win32_USBController", new[] { FullControllerRow() });
        var results = await provider.ToSafeUSBControllerMetricsAsync(CancellationToken.None);
        Assert.Equal((uint)0, results[0].ConfigManagerErrorCode);
    }

    [Fact]
    public async Task FullData_Maps_ConfigManagerUserConfig_False()
    {
        var provider = new FakeWmiProvider("Win32_USBController", new[] { FullControllerRow() });
        var results = await provider.ToSafeUSBControllerMetricsAsync(CancellationToken.None);
        Assert.False(results[0].ConfigManagerUserConfig);
    }

    [Fact]
    public async Task FullData_Maps_MaxNumberControlled_Uint()
    {
        var provider = new FakeWmiProvider("Win32_USBController", new[] { FullControllerRow() });
        var results = await provider.ToSafeUSBControllerMetricsAsync(CancellationToken.None);
        Assert.Equal((uint)255, results[0].MaxNumberControlled);
    }

    [Fact]
    public async Task FullData_Maps_ErrorCleared_False()
    {
        var provider = new FakeWmiProvider("Win32_USBController", new[] { FullControllerRow() });
        var results = await provider.ToSafeUSBControllerMetricsAsync(CancellationToken.None);
        Assert.False(results[0].ErrorCleared);
    }

    [Fact]
    public async Task FullData_Maps_PowerManagementSupported_False()
    {
        var provider = new FakeWmiProvider("Win32_USBController", new[] { FullControllerRow() });
        var results = await provider.ToSafeUSBControllerMetricsAsync(CancellationToken.None);
        Assert.False(results[0].PowerManagementSupported);
    }

    [Fact]
    public async Task FullData_Maps_PowerManagementCapabilities_Array()
    {
        var provider = new FakeWmiProvider("Win32_USBController", new[] { FullControllerRow() });
        var results = await provider.ToSafeUSBControllerMetricsAsync(CancellationToken.None);
        Assert.Equal(new ushort[] { 1, 4 }, results[0].PowerManagementCapabilities);
    }

    [Fact]
    public async Task FullData_Maps_InstallDate()
    {
        var provider = new FakeWmiProvider("Win32_USBController", new[] { FullControllerRow() });
        var results = await provider.ToSafeUSBControllerMetricsAsync(CancellationToken.None);
        Assert.Equal(new DateTime(2020, 3, 15, 0, 0, 0, DateTimeKind.Utc), results[0].InstallDate);
    }

    [Fact]
    public async Task FullData_Maps_TimeOfLastReset()
    {
        var provider = new FakeWmiProvider("Win32_USBController", new[] { FullControllerRow() });
        var results = await provider.ToSafeUSBControllerMetricsAsync(CancellationToken.None);
        Assert.Equal(new DateTime(2024, 1, 1, 8, 0, 0, DateTimeKind.Utc), results[0].TimeOfLastReset);
    }

    [Fact]
    public async Task FullData_Maps_SystemName()
    {
        var provider = new FakeWmiProvider("Win32_USBController", new[] { FullControllerRow() });
        var results = await provider.ToSafeUSBControllerMetricsAsync(CancellationToken.None);
        Assert.Equal("DESKTOP-01", results[0].SystemName);
    }

    // --- ProtocolSupportedName computed property ---

    [Theory]
    [InlineData(1,  "Other")]
    [InlineData(2,  "Unknown")]
    [InlineData(3,  "EISA")]
    [InlineData(4,  "ISA")]
    [InlineData(5,  "PCI")]
    [InlineData(16, "Universal Serial Bus")]
    [InlineData(17, "Parallel Protocol")]
    [InlineData(37, "IDE")]
    [InlineData(43, "AGP")]
    public async Task ProtocolSupportedName_Known_Codes(int code, string expected)
    {
        var provider = new FakeWmiProvider("Win32_USBController",
            new[] { WmiRow.Build(("ProtocolSupported", new WmiValue(code))) });
        var results = await provider.ToSafeUSBControllerMetricsAsync(CancellationToken.None);
        Assert.Equal(expected, results[0].ProtocolSupportedName);
    }

    [Fact]
    public async Task ProtocolSupportedName_Unknown_Code_Returns_Null()
    {
        var provider = new FakeWmiProvider("Win32_USBController",
            new[] { WmiRow.Build(("ProtocolSupported", new WmiValue(99))) });
        var results = await provider.ToSafeUSBControllerMetricsAsync(CancellationToken.None);
        Assert.Null(results[0].ProtocolSupportedName);
    }

    [Fact]
    public async Task ProtocolSupportedName_Null_ProtocolSupported_Returns_Null()
    {
        var provider = new FakeWmiProvider("Win32_USBController",
            new[] { WmiRow.Build(("Name", new WmiValue("USB Controller"))) });
        var results = await provider.ToSafeUSBControllerMetricsAsync(CancellationToken.None);
        Assert.Null(results[0].ProtocolSupportedName);
    }

    // --- Multi-controller ---

    [Fact]
    public async Task Multiple_Controllers_Returns_All()
    {
        var c1 = WmiRow.Build(("Name", new WmiValue("xHCI Controller")),  ("ProtocolSupported", new WmiValue(16)));
        var c2 = WmiRow.Build(("Name", new WmiValue("EHCI Controller")), ("ProtocolSupported", new WmiValue(16)));

        var provider = new FakeWmiProvider("Win32_USBController", new[] { c1, c2 });
        var results = await provider.ToSafeUSBControllerMetricsAsync(CancellationToken.None);

        Assert.Equal(2, results.Count);
        Assert.Equal("xHCI Controller",  results[0].Name);
        Assert.Equal("EHCI Controller", results[1].Name);
    }

    // --- Fallback behaviour ---

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("Win32_USBController", WmiRow.Empty());
        var results = await provider.ToSafeUSBControllerMetricsAsync(CancellationToken.None);
        Assert.Empty(results);
    }

    [Fact]
    public async Task MissingKeys_Return_Null_For_Those_Fields()
    {
        var provider = new FakeWmiProvider("Win32_USBController",
            new[] { WmiRow.Build(("Name", new WmiValue("Minimal Controller"))) });
        var results = await provider.ToSafeUSBControllerMetricsAsync(CancellationToken.None);

        Assert.Equal("Minimal Controller", results[0].Name);
        Assert.Null(results[0].Manufacturer);
        Assert.Null(results[0].ProtocolSupported);
        Assert.Null(results[0].MaxNumberControlled);
        Assert.Null(results[0].TimeOfLastReset);
        Assert.Null(results[0].PowerManagementCapabilities);
    }

    [Fact]
    public async Task Cancelled_Token_Returns_Empty_Fallback()
    {
        // USBController extension uses generic catch — swallows OperationCanceledException
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var provider = new FakeWmiProvider("Win32_USBController", new[] { FullControllerRow() });
        var results = await provider.ToSafeUSBControllerMetricsAsync(cts.Token);

        Assert.Empty(results);
    }

    [Fact]
    public async Task WrongValueType_For_Key_Returns_Null()
    {
        // ProtocolSupported stored as String instead of Int — GetInt returns null
        var provider = new FakeWmiProvider("Win32_USBController",
            new[] { WmiRow.Build(("ProtocolSupported", new WmiValue("USB"))) });
        var results = await provider.ToSafeUSBControllerMetricsAsync(CancellationToken.None);

        Assert.Null(results[0].ProtocolSupported);
    }
}
