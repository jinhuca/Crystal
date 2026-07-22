using Crystal.Mmi.HardwareFeatures.VideoController;
using Crystal.Mmi.MmiEngine;
using Crystal.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Mmi.Tests.HardwareFeatures;

public class VideoControllerExtensionsTests
{
    private static System.Collections.Frozen.FrozenDictionary<string, WmiValue> GpuRow() => WmiRow.Build(
        ("Name", new WmiValue("NVIDIA GeForce RTX 4070")),
        ("AdapterCompatibility", new WmiValue("NVIDIA")),
        ("AdapterDACType", new WmiValue("Integrated RAMDAC")),
        ("AdapterRAM", new WmiValue(536_870_912)),       // ~512 MB as int
        ("Caption", new WmiValue("NVIDIA GeForce RTX 4070")),
        ("CurrentHorizontalResolution", new WmiValue(1920)),
        ("CurrentVerticalResolution", new WmiValue(1080)),
        ("CurrentRefreshRate", new WmiValue(144)),
        ("CurrentBitsPerPixel", new WmiValue(32)),
        ("CurrentNumberOfColors", new WmiValue(4_294_967_296UL)),
        ("CurrentNumberOfColumns", new WmiValue(0)),
        ("CurrentNumberOfRows", new WmiValue(0)),
        ("DeviceID", new WmiValue("VideoController1")),
        ("DriverVersion", new WmiValue("31.0.15.3179")),
        ("InstalledDisplayDrivers", new WmiValue("C:\\Windows\\System32\\DriverStore\\nvldumdx.dll")),
        ("VideoProcessor", new WmiValue("NVIDIA GeForce RTX 4070")),
        ("VideoArchitecture", new WmiValue(5)),
        ("VideoMemoryType", new WmiValue(3)),
        ("Status", new WmiValue("OK")),
        ("StatusInfo", new WmiValue(3)),
        ("ConfigManagerErrorCode", new WmiValue(0)),
        ("ConfigManagerUserConfig", new WmiValue(false)),
        ("ErrorCleared", new WmiValue(false)),
        ("PowerManagementSupported", new WmiValue(false)),
        ("Availability", new WmiValue(3)),
        ("MaxRefreshRate", new WmiValue(240)),
        ("MinRefreshRate", new WmiValue(50)),
        ("SystemName", new WmiValue("DESKTOP-01")),
        ("ColorTableEntries", new WmiValue(0)),
        ("DitherType", new WmiValue(4)),
        ("Architecture", new WmiValue(5)),
        ("InfSection", new WmiValue("Section1")),
        ("InfDate", new WmiValue(new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc))),
        ("InstallationDate", new WmiValue(new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc))),
        ("PowerManagementCapabilities", new WmiValue(new ushort[] { 1, 2 })),
        ("CreationClassName", new WmiValue("Win32_VideoController")),
        ("SystemCreationClassName", new WmiValue("Win32_ComputerSystem"))
    );

    [Fact]
    public async Task FullData_Maps_Name()
    {
        var provider = new FakeWmiProvider("Win32_VideoController", new[] { GpuRow() });
        var results = await provider.ToSafeVideoControllerMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("NVIDIA GeForce RTX 4070", results[0].Name);
    }

    [Fact]
    public async Task FullData_Maps_AdapterCompatibility()
    {
        var provider = new FakeWmiProvider("Win32_VideoController", new[] { GpuRow() });
        var results = await provider.ToSafeVideoControllerMetricsAsync(CancellationToken.None);

        Assert.Equal("NVIDIA", results[0].AdapterCompatibility);
    }

    [Fact]
    public async Task FullData_Maps_CurrentHorizontalResolution_Uint()
    {
        var provider = new FakeWmiProvider("Win32_VideoController", new[] { GpuRow() });
        var results = await provider.ToSafeVideoControllerMetricsAsync(CancellationToken.None);

        Assert.Equal((uint)1920, results[0].CurrentHorizontalResolution);
    }

    [Fact]
    public async Task FullData_Maps_CurrentVerticalResolution_Uint()
    {
        var provider = new FakeWmiProvider("Win32_VideoController", new[] { GpuRow() });
        var results = await provider.ToSafeVideoControllerMetricsAsync(CancellationToken.None);

        Assert.Equal((uint)1080, results[0].CurrentVerticalResolution);
    }

    [Fact]
    public async Task FullData_Maps_CurrentRefreshRate_Uint()
    {
        var provider = new FakeWmiProvider("Win32_VideoController", new[] { GpuRow() });
        var results = await provider.ToSafeVideoControllerMetricsAsync(CancellationToken.None);

        Assert.Equal((uint)144, results[0].CurrentRefreshRate);
    }

    [Fact]
    public async Task FullData_Maps_CurrentBitsPerPixel_Uint()
    {
        var provider = new FakeWmiProvider("Win32_VideoController", new[] { GpuRow() });
        var results = await provider.ToSafeVideoControllerMetricsAsync(CancellationToken.None);

        Assert.Equal((uint)32, results[0].CurrentBitsPerPixel);
    }

    [Fact]
    public async Task FullData_Maps_DriverVersion()
    {
        var provider = new FakeWmiProvider("Win32_VideoController", new[] { GpuRow() });
        var results = await provider.ToSafeVideoControllerMetricsAsync(CancellationToken.None);

        Assert.Equal("31.0.15.3179", results[0].DriverVersion);
    }

    [Fact]
    public async Task FullData_Maps_CurrentNumberOfColors_ULong()
    {
        var provider = new FakeWmiProvider("Win32_VideoController", new[] { GpuRow() });
        var results = await provider.ToSafeVideoControllerMetricsAsync(CancellationToken.None);

        Assert.Equal(4_294_967_296UL, results[0].CurrentNumberOfColors);
    }

    [Fact]
    public async Task FullData_Maps_InfDate_DateTime()
    {
        var provider = new FakeWmiProvider("Win32_VideoController", new[] { GpuRow() });
        var results = await provider.ToSafeVideoControllerMetricsAsync(CancellationToken.None);

        Assert.Equal(new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), results[0].InfDate);
    }

    [Fact]
    public async Task FullData_Maps_PowerManagementCapabilities_Array()
    {
        var provider = new FakeWmiProvider("Win32_VideoController", new[] { GpuRow() });
        var results = await provider.ToSafeVideoControllerMetricsAsync(CancellationToken.None);

        Assert.Equal(new ushort[] { 1, 2 }, results[0].PowerManagementCapabilities);
    }

    [Fact]
    public async Task FullData_Maps_AdapterRAM_Uint()
    {
        var provider = new FakeWmiProvider("Win32_VideoController", new[] { GpuRow() });
        var results = await provider.ToSafeVideoControllerMetricsAsync(CancellationToken.None);

        Assert.Equal((uint)536_870_912, results[0].AdapterRAM);
    }

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("Win32_VideoController", WmiRow.Empty());
        var results = await provider.ToSafeVideoControllerMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MultipleGPUs_Returns_All()
    {
        var gpu1 = WmiRow.Build(("Name", new WmiValue("GPU1")));
        var gpu2 = WmiRow.Build(("Name", new WmiValue("GPU2")));

        var provider = new FakeWmiProvider("Win32_VideoController", new[] { gpu1, gpu2 });
        var results = await provider.ToSafeVideoControllerMetricsAsync(CancellationToken.None);

        Assert.Equal(2, results.Count);
        Assert.Equal("GPU1", results[0].Name);
        Assert.Equal("GPU2", results[1].Name);
    }

    // ── VideoRamInGB ───────────────────────────────────────────────────────

    [Fact]
    public async Task VideoRamInGB_Calculated_Correctly()
    {
        // 536_870_912 bytes = 0.5 GB
        var provider = new FakeWmiProvider("Win32_VideoController", new[] { GpuRow() });
        var results = await provider.ToSafeVideoControllerMetricsAsync(CancellationToken.None);

        Assert.NotNull(results[0].VideoRamInGB);
        Assert.Equal(0.5, results[0].VideoRamInGB!.Value, precision: 2);
    }

    [Fact]
    public async Task VideoRamInGB_Null_When_AdapterRAM_Missing()
    {
        var row = WmiRow.Build(("Name", new WmiValue("GPU")));
        var provider = new FakeWmiProvider("Win32_VideoController", new[] { row });
        var results = await provider.ToSafeVideoControllerMetricsAsync(CancellationToken.None);

        Assert.Null(results[0].VideoRamInGB);
    }

    // ── FormattedDisplayMode ───────────────────────────────────────────────

    [Fact]
    public async Task FormattedDisplayMode_With_Resolution()
    {
        var provider = new FakeWmiProvider("Win32_VideoController", new[] { GpuRow() });
        var results = await provider.ToSafeVideoControllerMetricsAsync(CancellationToken.None);

        Assert.Equal("1920 x 1080 @ 144Hz (32-bit color)", results[0].FormattedDisplayMode);
    }

    [Fact]
    public async Task FormattedDisplayMode_Without_Resolution_Returns_Headless()
    {
        var row = WmiRow.Build(("Name", new WmiValue("Headless GPU")));
        var provider = new FakeWmiProvider("Win32_VideoController", new[] { row });
        var results = await provider.ToSafeVideoControllerMetricsAsync(CancellationToken.None);

        Assert.Equal("No Monitor Active / Headless Display Mode", results[0].FormattedDisplayMode);
    }

    [Fact]
    public void FormattedDisplayMode_Uses_Zero_When_RefreshRate_Null()
    {
        // Manually build a metrics with resolution but null refresh rate
        // VideoControllerMetrics has 44 constructor parameters (positions 0-43)
        // Position 11 = CurrentHorizontalResolution, 16 = CurrentVerticalResolution, 15 = CurrentRefreshRate (null)
        var m = new VideoControllerMetrics(
            null, null, null, null, null, null, null, null, null, null,  // 0-9
            null, 2560u, null, null, null, null, 1440u,                   // 10-16
            null, null, null, null, null, null, null, null, null,          // 17-25
            null, null, null, null, null, null, null, null, null,          // 26-34
            null, null, null, null, null, null, null, null, null);         // 35-43

        // RefreshRate is null → uses ?? 0 → "2560 x 1440 @ 0Hz"
        Assert.Contains("2560 x 1440", m.FormattedDisplayMode);
        Assert.Contains("@ 0Hz", m.FormattedDisplayMode);
    }
}
