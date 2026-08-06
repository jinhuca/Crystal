using Crystal.Provider.Mmi.HardwareFeatures.Tpm;
using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Provider.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Provider.Mmi.Tests.HardwareFeatures;

public class TpmExtensionsTests
{
    // Fake ignores the namespace and looks up by class name.
    private static FakeWmiProvider FullRow() => new FakeWmiProvider("Win32_Tpm", WmiRow.Single(
        ("Caption",                     new WmiValue("TPM 2.0 Device")),
        ("Description",                 new WmiValue("Trusted Platform Module")),
        ("InstanceName",                new WmiValue("MSFT_Tpm")),
        ("IsActivated_InitialValue",    new WmiValue(true)),
        ("IsEnabled_InitialValue",      new WmiValue(true)),
        ("IsOwned_InitialValue",        new WmiValue(false)),
        ("ManufacturerId",              new WmiValue(1229346816)),
        ("ManufacturerIdTxt",           new WmiValue("INTC")),
        ("ManufacturerVersion",         new WmiValue("403.1.0.0")),
        ("ManufacturerVersionFull20",   new WmiValue("403.1.0.0 - 1.38")),
        ("ManufacturerVersionInfo",     new WmiValue("Intel")),
        ("PhysicalPresenceVersionInfo", new WmiValue("1.3")),
        ("SpecVersion",                 new WmiValue("2.0, 0, 1.38")),
        ("Status",                      new WmiValue("OK"))
    ));

    // --- Field mapping ---

    [Fact]
    public async Task FullData_Maps_Caption()
    {
        var result = await FullRow().ToSafeTpmMetricsAsync(CancellationToken.None);
        Assert.Equal("TPM 2.0 Device", result.Caption);
    }

    [Fact]
    public async Task FullData_Maps_Description()
    {
        var result = await FullRow().ToSafeTpmMetricsAsync(CancellationToken.None);
        Assert.Equal("Trusted Platform Module", result.Description);
    }

    [Fact]
    public async Task FullData_Maps_InstanceName()
    {
        var result = await FullRow().ToSafeTpmMetricsAsync(CancellationToken.None);
        Assert.Equal("MSFT_Tpm", result.InstanceName);
    }

    [Fact]
    public async Task FullData_Maps_IsEnabled_True()
    {
        var result = await FullRow().ToSafeTpmMetricsAsync(CancellationToken.None);
        Assert.True(result.IsEnabled_InitialValue);
    }

    [Fact]
    public async Task FullData_Maps_IsActivated_True()
    {
        var result = await FullRow().ToSafeTpmMetricsAsync(CancellationToken.None);
        Assert.True(result.IsActivated_InitialValue);
    }

    [Fact]
    public async Task FullData_Maps_IsOwned_False()
    {
        var result = await FullRow().ToSafeTpmMetricsAsync(CancellationToken.None);
        Assert.False(result.IsOwned_InitialValue);
    }

    [Fact]
    public async Task FullData_Maps_ManufacturerId_Uint()
    {
        var result = await FullRow().ToSafeTpmMetricsAsync(CancellationToken.None);
        Assert.Equal((uint)1229346816, result.ManufacturerId);
    }

    [Fact]
    public async Task FullData_Maps_ManufacturerIdTxt()
    {
        var result = await FullRow().ToSafeTpmMetricsAsync(CancellationToken.None);
        Assert.Equal("INTC", result.ManufacturerIdTxt);
    }

    [Fact]
    public async Task FullData_Maps_SpecVersion()
    {
        var result = await FullRow().ToSafeTpmMetricsAsync(CancellationToken.None);
        Assert.Equal("2.0, 0, 1.38", result.SpecVersion);
    }

    [Fact]
    public async Task FullData_Maps_Status()
    {
        var result = await FullRow().ToSafeTpmMetricsAsync(CancellationToken.None);
        Assert.Equal("OK", result.Status);
    }

    // --- Fallback behaviour ---

    [Fact]
    public async Task EmptyInstances_Returns_All_Null_Fields()
    {
        var provider = new FakeWmiProvider("Win32_Tpm", WmiRow.Empty());
        var result = await provider.ToSafeTpmMetricsAsync(CancellationToken.None);

        Assert.Null(result.Caption);
        Assert.Null(result.Description);
        Assert.Null(result.InstanceName);
        Assert.Null(result.IsActivated_InitialValue);
        Assert.Null(result.IsEnabled_InitialValue);
        Assert.Null(result.IsOwned_InitialValue);
        Assert.Null(result.ManufacturerId);
        Assert.Null(result.ManufacturerIdTxt);
        Assert.Null(result.SpecVersion);
        Assert.Null(result.Status);
    }

    [Fact]
    public async Task MissingKeys_Return_Null_For_Those_Fields()
    {
        var provider = new FakeWmiProvider("Win32_Tpm",
            WmiRow.Single(("SpecVersion", new WmiValue("2.0"))));
        var result = await provider.ToSafeTpmMetricsAsync(CancellationToken.None);

        Assert.Equal("2.0", result.SpecVersion);
        Assert.Null(result.Caption);
        Assert.Null(result.ManufacturerId);
        Assert.Null(result.IsEnabled_InitialValue);
    }

    [Fact]
    public async Task WrongValueType_For_Key_Returns_Null()
    {
        // ManufacturerId stored as String instead of Int — GetInt returns null
        var provider = new FakeWmiProvider("Win32_Tpm",
            WmiRow.Single(("ManufacturerId", new WmiValue("INTC"))));
        var result = await provider.ToSafeTpmMetricsAsync(CancellationToken.None);

        Assert.Null(result.ManufacturerId);
    }
}
