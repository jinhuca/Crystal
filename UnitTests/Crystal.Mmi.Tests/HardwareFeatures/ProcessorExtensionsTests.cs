using Crystal.Mmi.HardwareFeatures.Processor;
using Crystal.Mmi.MmiEngine;
using Crystal.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Mmi.Tests.HardwareFeatures;

public class ProcessorExtensionsTests
{
    private static FakeWmiProvider FullRow() => new FakeWmiProvider("Win32_Processor", WmiRow.Single(
        ("Name", new WmiValue("Intel(R) Core(TM) i9-13900K")),
        ("Manufacturer", new WmiValue("GenuineIntel")),
        ("AddressWidth", new WmiValue(64)),
        ("Architecture", new WmiValue(9)),
        ("NumberOfCores", new WmiValue(24)),
        ("NumberOfLogicalProcessors", new WmiValue(32)),
        ("MaxClockSpeed", new WmiValue(5800)),
        ("CurrentClockSpeed", new WmiValue(3000)),
        ("LoadPercentage", new WmiValue(12)),
        ("SocketDesignation", new WmiValue("LGA1700")),
        ("ProcessorId", new WmiValue("BFEBFBFF000B06A2")),
        ("Status", new WmiValue("OK")),
        ("DeviceID", new WmiValue("CPU0")),
        ("ConfigManagerUserConfig", new WmiValue(false)),
        ("ErrorCleared", new WmiValue(true)),
        ("PowerManagementSupported", new WmiValue(false)),
        ("VirtualizationFirmwareEnabled", new WmiValue(true)),
        ("VMMonitorModeExtensions", new WmiValue(true)),
        ("SecondLevelAddressTranslationExtensions", new WmiValue(true)),
        ("InstallationDate", new WmiValue(new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc))),
        ("PowerManagementCapabilities", new WmiValue(new ushort[] { 1, 2 })),
        ("L2CacheSize", new WmiValue(2048)),
        ("L3CacheSize", new WmiValue(36864)),
        ("ThreadCount", new WmiValue(32)),
        ("Caption", new WmiValue("Intel64 Family 6 Model 183 Stepping 1"))
    ));

    [Fact]
    public async Task FullData_Maps_Name()
    {
        var result = await FullRow().ToSafeProcessorMetricsAsync(CancellationToken.None);
        Assert.Equal("Intel(R) Core(TM) i9-13900K", result.Name);
    }

    [Fact]
    public async Task FullData_Maps_Manufacturer()
    {
        var result = await FullRow().ToSafeProcessorMetricsAsync(CancellationToken.None);
        Assert.Equal("GenuineIntel", result.Manufacturer);
    }

    [Fact]
    public async Task FullData_Maps_NumberOfCores_Cast_To_Uint()
    {
        var result = await FullRow().ToSafeProcessorMetricsAsync(CancellationToken.None);
        Assert.Equal((uint)24, result.NumberOfCores);
    }

    [Fact]
    public async Task FullData_Maps_NumberOfLogicalProcessors()
    {
        var result = await FullRow().ToSafeProcessorMetricsAsync(CancellationToken.None);
        Assert.Equal((uint)32, result.NumberOfLogicalProcessors);
    }

    [Fact]
    public async Task FullData_Maps_AddressWidth_Cast_To_Ushort()
    {
        var result = await FullRow().ToSafeProcessorMetricsAsync(CancellationToken.None);
        Assert.Equal((ushort)64, result.AddressWidth);
    }

    [Fact]
    public async Task FullData_Maps_LoadPercentage_Cast_To_Ushort()
    {
        var result = await FullRow().ToSafeProcessorMetricsAsync(CancellationToken.None);
        Assert.Equal((ushort)12, result.LoadPercentage);
    }

    [Fact]
    public async Task FullData_Maps_Bool_VirtualizationFirmwareEnabled()
    {
        var result = await FullRow().ToSafeProcessorMetricsAsync(CancellationToken.None);
        Assert.True(result.VirtualizationFirmwareEnabled);
    }

    [Fact]
    public async Task FullData_Maps_Bool_ConfigManagerUserConfig_False()
    {
        var result = await FullRow().ToSafeProcessorMetricsAsync(CancellationToken.None);
        Assert.False(result.ConfigManagerUserConfig);
    }

    [Fact]
    public async Task FullData_Maps_PowerManagementCapabilities_Array()
    {
        var result = await FullRow().ToSafeProcessorMetricsAsync(CancellationToken.None);
        Assert.Equal(new ushort[] { 1, 2 }, result.PowerManagementCapabilities);
    }

    [Fact]
    public async Task FullData_Maps_InstallDate()
    {
        var result = await FullRow().ToSafeProcessorMetricsAsync(CancellationToken.None);
        Assert.Equal(new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc), result.InstallationDate);
    }

    [Fact]
    public async Task FullData_Maps_L2CacheSize_Cast_To_Uint()
    {
        var result = await FullRow().ToSafeProcessorMetricsAsync(CancellationToken.None);
        Assert.Equal((uint)2048, result.L2CacheSize);
    }

    [Fact]
    public async Task FullData_Maps_Status_OK()
    {
        var result = await FullRow().ToSafeProcessorMetricsAsync(CancellationToken.None);
        Assert.Equal("OK", result.Status);
    }

    [Fact]
    public async Task EmptyInstances_Returns_All_Null_Fallback()
    {
        var provider = new FakeWmiProvider("Win32_Processor", WmiRow.Empty());
        var result = await provider.ToSafeProcessorMetricsAsync(CancellationToken.None);

        Assert.Null(result.Name);
        Assert.Null(result.Manufacturer);
        Assert.Null(result.NumberOfCores);
        Assert.Null(result.LoadPercentage);
        Assert.Null(result.Status);
    }

    [Fact]
    public async Task Cancelled_Token_Throws_OperationCanceledException()
    {
        // Processor extension re-throws OperationCanceledException (unlike Bios)
        var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => FullRow().ToSafeProcessorMetricsAsync(cts.Token));
    }

    [Fact]
    public async Task MissingKeys_Return_Null()
    {
        var provider = new FakeWmiProvider("Win32_Processor",
            WmiRow.Single(("Name", new WmiValue("Only Name"))));
        var result = await provider.ToSafeProcessorMetricsAsync(CancellationToken.None);

        Assert.Equal("Only Name", result.Name);
        Assert.Null(result.Manufacturer);
        Assert.Null(result.NumberOfCores);
        Assert.Null(result.PowerManagementCapabilities);
    }

    [Fact]
    public async Task WrongTypeForKey_Returns_Null()
    {
        // Name stored as Int — should not crash, returns null
        var provider = new FakeWmiProvider("Win32_Processor",
            WmiRow.Single(("Name", new WmiValue(999))));
        var result = await provider.ToSafeProcessorMetricsAsync(CancellationToken.None);

        Assert.Null(result.Name);
    }
}
