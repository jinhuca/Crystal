using Crystal.Provider.Mmi.HardwareFeatures.Bios;
using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Provider.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Provider.Mmi.Tests.HardwareFeatures;

public class BiosExtensionsTests
{
    private static FakeWmiProvider FullRow() => new FakeWmiProvider("Win32_Bios", WmiRow.Single(
        ("BiosCharacteristics", new WmiValue(new ushort[] { 7, 11, 15 })),
        ("BIOSVersion", new WmiValue(new[] { "AMI - 5002015", "ALASKA - 1072009" })),
        ("BuildNumber", new WmiValue("build42")),
        ("Caption", new WmiValue("BIOS Date: 08/06/19")),
        ("CodeSet", new WmiValue("cs1")),
        ("CurrentLanguage", new WmiValue("en-US")),
        ("Description", new WmiValue("BIOS description")),
        ("EmbeddedControllerMajorVersion", new WmiValue("1")),
        ("EmbeddedControllerMinorVersion", new WmiValue("0")),
        ("IdentificationCode", new WmiValue("id123")),
        ("InstallableLanguages", new WmiValue(3)),
        ("InstallationDate", new WmiValue(new DateTime(2019, 8, 6, 0, 0, 0, DateTimeKind.Utc))),
        ("LanguageEdition", new WmiValue("LE1")),
        ("ListOfLanguages", new WmiValue(new[] { "en-US", "fr-FR" })),
        ("Manufacturer", new WmiValue("American Megatrends")),
        ("Name", new WmiValue("BIOS name")),
        ("OtherTargetOS", new WmiValue("None")),
        ("PartNumber", new WmiValue("part-99")),
        ("PrimaryBIOS", new WmiValue(true)),
        ("ReleaseDate", new WmiValue("20190806000000.000000+000")),
        ("SerialNumber", new WmiValue("SN-12345")),
        ("SMBIOSBIOSVersion", new WmiValue("F2d")),
        ("SMBIOSPresent", new WmiValue(true)),
        ("SMBIOSMajorVersion", new WmiValue(3)),
        ("SMBIOSMinorVersion", new WmiValue(1)),
        ("Status", new WmiValue("OK")),
        ("SystemBiosMajorVersion", new WmiValue("5")),
        ("SystemBiosMinorVersion", new WmiValue("2")),
        ("TargetOperatingSystem", new WmiValue(0)),
        ("Version", new WmiValue("F2d"))
    ));

    [Fact]
    public async Task FullData_Maps_Manufacturer()
    {
        var result = await FullRow().ToSafeBiosMetricsAsync(CancellationToken.None);
        Assert.Equal("American Megatrends", result.Manufacturer);
    }

    [Fact]
    public async Task FullData_Maps_Caption()
    {
        var result = await FullRow().ToSafeBiosMetricsAsync(CancellationToken.None);
        Assert.Equal("BIOS Date: 08/06/19", result.Caption);
    }

    [Fact]
    public async Task FullData_Maps_PrimaryBIOS_True()
    {
        var result = await FullRow().ToSafeBiosMetricsAsync(CancellationToken.None);
        Assert.True(result.PrimaryBIOS);
    }

    [Fact]
    public async Task FullData_Maps_SMBIOSPresent_True()
    {
        var result = await FullRow().ToSafeBiosMetricsAsync(CancellationToken.None);
        Assert.True(result.SMBIOSPresent);
    }

    [Fact]
    public async Task FullData_Maps_InstallDate()
    {
        var result = await FullRow().ToSafeBiosMetricsAsync(CancellationToken.None);
        Assert.Equal(new DateTime(2019, 8, 6, 0, 0, 0, DateTimeKind.Utc), result.InstallDate);
    }

    [Fact]
    public async Task FullData_Maps_BiosCharacteristics_FirstElement()
    {
        // BiosCharacteristics takes FirstOrDefault of the UShortArray
        var result = await FullRow().ToSafeBiosMetricsAsync(CancellationToken.None);
        Assert.Equal((ushort)7, result.BiosCharacteristics);
    }

    [Fact]
    public async Task FullData_Maps_BIOSVersion_Flattened()
    {
        var result = await FullRow().ToSafeBiosMetricsAsync(CancellationToken.None);
        Assert.Equal("AMI - 5002015, ALASKA - 1072009", result.BIOSVersion);
    }

    [Fact]
    public async Task FullData_Maps_ListOfLanguages_Flattened()
    {
        var result = await FullRow().ToSafeBiosMetricsAsync(CancellationToken.None);
        Assert.Equal("en-US, fr-FR", result.ListOfLanguages);
    }

    [Fact]
    public async Task FullData_Maps_SMBIOSMajorVersion_Cast()
    {
        var result = await FullRow().ToSafeBiosMetricsAsync(CancellationToken.None);
        Assert.Equal((ushort)3, result.SMBIOSMajorVersion);
    }

    [Fact]
    public async Task FullData_Maps_Version()
    {
        var result = await FullRow().ToSafeBiosMetricsAsync(CancellationToken.None);
        Assert.Equal("F2d", result.Version);
    }

    [Fact]
    public async Task FullData_Maps_Status()
    {
        var result = await FullRow().ToSafeBiosMetricsAsync(CancellationToken.None);
        Assert.Equal("OK", result.Status);
    }

    [Fact]
    public async Task EmptyInstances_Returns_All_Null_Fields()
    {
        var provider = new FakeWmiProvider("Win32_Bios", WmiRow.Empty());
        var result = await provider.ToSafeBiosMetricsAsync(CancellationToken.None);

        Assert.Null(result.Manufacturer);
        Assert.Null(result.Caption);
        Assert.Null(result.Version);
        Assert.Null(result.Status);
        Assert.Null(result.PrimaryBIOS);
        Assert.Null(result.InstallDate);
        Assert.Null(result.BiosCharacteristics);
    }

    [Fact]
    public async Task MissingKeys_Return_Null_For_Those_Fields()
    {
        // Only Caption present; everything else should be null
        var provider = new FakeWmiProvider("Win32_Bios",
            WmiRow.Single(("Caption", new WmiValue("My BIOS"))));
        var result = await provider.ToSafeBiosMetricsAsync(CancellationToken.None);

        Assert.Equal("My BIOS", result.Caption);
        Assert.Null(result.Manufacturer);
        Assert.Null(result.Version);
        Assert.Null(result.PrimaryBIOS);
        Assert.Null(result.BiosCharacteristics);
        Assert.Null(result.BIOSVersion);
    }

    [Fact]
    public async Task Cancelled_Token_Returns_Fallback_Not_Throw()
    {
        // Bios extension uses a generic catch — it swallows OperationCanceledException
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await FullRow().ToSafeBiosMetricsAsync(cts.Token);

        // Should NOT throw; returns null-filled fallback
        Assert.NotNull(result);
        Assert.Null(result.Manufacturer);
    }

    [Fact]
    public async Task WrongValueType_For_Key_Returns_Null()
    {
        // Manufacturer stored as Int instead of String — GetStr returns null
        var provider = new FakeWmiProvider("Win32_Bios",
            WmiRow.Single(("Manufacturer", new WmiValue(42))));
        var result = await provider.ToSafeBiosMetricsAsync(CancellationToken.None);

        Assert.Null(result.Manufacturer);
    }

    [Fact]
    public async Task EmptyUShortArray_BiosCharacteristics_Returns_Zero()
    {
        // FirstOrDefault on empty ushort[] returns 0 (the default)
        var provider = new FakeWmiProvider("Win32_Bios",
            WmiRow.Single(("BiosCharacteristics", new WmiValue(Array.Empty<ushort>()))));
        var result = await provider.ToSafeBiosMetricsAsync(CancellationToken.None);

        Assert.Equal((ushort)0, result.BiosCharacteristics);
    }
}
