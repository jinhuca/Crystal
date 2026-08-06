using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Provider.Mmi.SoftwareFeatures.OperatingSystem;
using Crystal.Provider.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Provider.Mmi.Tests.SoftwareFeatures;

public class OperatingSystemExtensionsTests
{
    private static System.Collections.Frozen.FrozenDictionary<string, WmiValue> Win11Row() => WmiRow.Build(
        ("Caption", new WmiValue("Microsoft Windows 11 Pro")),
        ("BuildNumber", new WmiValue("22631")),
        ("BuildType", new WmiValue("Multiprocessor Free")),
        ("Version", new WmiValue("10.0.22631")),
        ("OSArchitecture", new WmiValue("64-bit")),
        ("Manufacturer", new WmiValue("Microsoft Corporation")),
        ("CSName", new WmiValue("DESKTOP-01")),
        ("RegisteredUser", new WmiValue("User Name")),
        ("SerialNumber", new WmiValue("00330-80000-00000-AA819")),
        ("Status", new WmiValue("OK")),
        ("SystemDrive", new WmiValue("C:")),
        ("SystemDirectory", new WmiValue(@"C:\Windows\system32")),
        ("SystemDevice", new WmiValue(@"\Device\HarddiskVolume3")),
        ("WindowsDirectory", new WmiValue(@"C:\Windows")),
        ("Organization", new WmiValue("")),
        ("Locale", new WmiValue("0409")),
        ("CountryCode", new WmiValue("1")),
        ("CodeSet", new WmiValue("1252")),
        ("CreationClassName", new WmiValue("Win32_OperatingSystem")),
        ("CSCreationClassName", new WmiValue("Win32_ComputerSystem")),
        ("NumberOfProcesses", new WmiValue(250)),
        ("NumberOfUsers", new WmiValue(2)),
        ("NumberOfLicensedUsers", new WmiValue(0)),
        ("MaxNumberOfProcesses", new WmiValue(-1)),
        ("OSLanguage", new WmiValue(1033)),
        ("OSType", new WmiValue(18)),
        ("OSProductSuite", new WmiValue(256)),
        ("OperatingSystemSKU", new WmiValue(48)),
        ("ProductType", new WmiValue(1)),
        ("EncryptionLevel", new WmiValue(256)),
        ("CurrentTimeZone", new WmiValue(-300)),
        ("ForegroundApplicationBoostScheduling", new WmiValue(2)),
        ("LargeSystemCache", new WmiValue(0)),
        ("ServicePackMajorVersion", new WmiValue(0)),
        ("ServicePackMinorVersion", new WmiValue(0)),
        ("SuiteMask", new WmiValue(256)),
        ("DataExecutionPrevention_Available", new WmiValue(true)),
        ("DataExecutionPrevention_32BitApplications", new WmiValue(true)),
        ("DataExecutionPrevention_Drivers", new WmiValue(true)),
        ("DataExecutionPrevention_SupportPolicy", new WmiValue(3)),
        ("Distributed", new WmiValue(false)),
        ("Primary", new WmiValue(true)),
        ("PAEEnabled", new WmiValue(false)),
        ("TotalVisibleMemorySize", new WmiValue(32_505_856UL)),
        ("TotalVirtualMemorySize", new WmiValue(37_486_592UL)),
        ("FreePhysicalMemory", new WmiValue(8_388_608UL)),
        ("FreeVirtualMemory", new WmiValue(10_000_000UL)),
        ("FreeSpaceInPagingFiles", new WmiValue(4_980_736UL)),
        ("SizeStoredInPagingFiles", new WmiValue(4_980_736UL)),
        ("MaxProcessMemorySize", new WmiValue(137_438_953_344UL)),
        ("MUILanguages", new WmiValue(new[] { "en-US" })),
        ("InstallationDate", new WmiValue(new DateTime(2023, 5, 1, 0, 0, 0, DateTimeKind.Utc))),
        ("LastBootUpTime", new WmiValue(new DateTime(2024, 1, 10, 8, 0, 0, DateTimeKind.Utc))),
        ("LocalDateTime", new WmiValue(new DateTime(2024, 1, 15, 12, 0, 0, DateTimeKind.Utc)))
    );

    [Fact]
    public async Task FullData_Maps_Caption()
    {
        var provider = new FakeWmiProvider("Win32_OperatingSystem", new[] { Win11Row() });
        var result = await provider.ToSafeOperatingSystemMetricsAsync(CancellationToken.None);

        Assert.Equal("Microsoft Windows 11 Pro", result.Caption);
    }

    [Fact]
    public async Task FullData_Maps_BuildNumber()
    {
        var provider = new FakeWmiProvider("Win32_OperatingSystem", new[] { Win11Row() });
        var result = await provider.ToSafeOperatingSystemMetricsAsync(CancellationToken.None);

        Assert.Equal("22631", result.BuildNumber);
    }

    [Fact]
    public async Task FullData_Maps_OSArchitecture()
    {
        var provider = new FakeWmiProvider("Win32_OperatingSystem", new[] { Win11Row() });
        var result = await provider.ToSafeOperatingSystemMetricsAsync(CancellationToken.None);

        Assert.Equal("64-bit", result.OSArchitecture);
    }

    [Fact]
    public async Task FullData_Maps_Manufacturer()
    {
        var provider = new FakeWmiProvider("Win32_OperatingSystem", new[] { Win11Row() });
        var result = await provider.ToSafeOperatingSystemMetricsAsync(CancellationToken.None);

        Assert.Equal("Microsoft Corporation", result.Manufacturer);
    }

    [Fact]
    public async Task FullData_Maps_TotalVisibleMemorySize_ULong()
    {
        var provider = new FakeWmiProvider("Win32_OperatingSystem", new[] { Win11Row() });
        var result = await provider.ToSafeOperatingSystemMetricsAsync(CancellationToken.None);

        Assert.Equal(32_505_856UL, result.TotalVisibleMemorySize);
    }

    [Fact]
    public async Task FullData_Maps_FreePhysicalMemory_ULong()
    {
        var provider = new FakeWmiProvider("Win32_OperatingSystem", new[] { Win11Row() });
        var result = await provider.ToSafeOperatingSystemMetricsAsync(CancellationToken.None);

        Assert.Equal(8_388_608UL, result.FreePhysicalMemory);
    }

    [Fact]
    public async Task FullData_Maps_NumberOfProcesses_Uint()
    {
        var provider = new FakeWmiProvider("Win32_OperatingSystem", new[] { Win11Row() });
        var result = await provider.ToSafeOperatingSystemMetricsAsync(CancellationToken.None);

        Assert.Equal((uint)250, result.NumberOfProcesses);
    }

    [Fact]
    public async Task FullData_Maps_MUILanguages_StringArray()
    {
        var provider = new FakeWmiProvider("Win32_OperatingSystem", new[] { Win11Row() });
        var result = await provider.ToSafeOperatingSystemMetricsAsync(CancellationToken.None);

        Assert.Equal(new[] { "en-US" }, result.MUILanguages);
    }

    [Fact]
    public async Task FullData_Maps_InstallDate_DateTime()
    {
        var provider = new FakeWmiProvider("Win32_OperatingSystem", new[] { Win11Row() });
        var result = await provider.ToSafeOperatingSystemMetricsAsync(CancellationToken.None);

        Assert.Equal(new DateTime(2023, 5, 1, 0, 0, 0, DateTimeKind.Utc), result.InstallationDate);
    }

    [Fact]
    public async Task FullData_Maps_LastBootUpTime_DateTime()
    {
        var provider = new FakeWmiProvider("Win32_OperatingSystem", new[] { Win11Row() });
        var result = await provider.ToSafeOperatingSystemMetricsAsync(CancellationToken.None);

        Assert.Equal(new DateTime(2024, 1, 10, 8, 0, 0, DateTimeKind.Utc), result.LastBootUpTime);
    }

    [Fact]
    public async Task FullData_Maps_DataExecutionPrevention_Available_True()
    {
        var provider = new FakeWmiProvider("Win32_OperatingSystem", new[] { Win11Row() });
        var result = await provider.ToSafeOperatingSystemMetricsAsync(CancellationToken.None);

        Assert.True(result.DataExecutionPrevention_Available);
    }

    [Fact]
    public async Task FullData_Maps_Primary_True()
    {
        var provider = new FakeWmiProvider("Win32_OperatingSystem", new[] { Win11Row() });
        var result = await provider.ToSafeOperatingSystemMetricsAsync(CancellationToken.None);

        Assert.True(result.Primary);
    }

    [Fact]
    public async Task FullData_Maps_Distributed_False()
    {
        var provider = new FakeWmiProvider("Win32_OperatingSystem", new[] { Win11Row() });
        var result = await provider.ToSafeOperatingSystemMetricsAsync(CancellationToken.None);

        Assert.False(result.Distributed);
    }

    [Fact]
    public async Task FullData_Maps_CurrentTimeZone_Short()
    {
        var provider = new FakeWmiProvider("Win32_OperatingSystem", new[] { Win11Row() });
        var result = await provider.ToSafeOperatingSystemMetricsAsync(CancellationToken.None);

        Assert.Equal((short)-300, result.CurrentTimeZone);
    }

    [Fact]
    public async Task FullData_Maps_MaxProcessMemorySize_ULong()
    {
        var provider = new FakeWmiProvider("Win32_OperatingSystem", new[] { Win11Row() });
        var result = await provider.ToSafeOperatingSystemMetricsAsync(CancellationToken.None);

        Assert.Equal(137_438_953_344UL, result.MaxProcessMemorySize);
    }

    [Fact]
    public async Task FullData_Maps_SystemDrive()
    {
        var provider = new FakeWmiProvider("Win32_OperatingSystem", new[] { Win11Row() });
        var result = await provider.ToSafeOperatingSystemMetricsAsync(CancellationToken.None);

        Assert.Equal("C:", result.SystemDrive);
    }

    [Fact]
    public async Task EmptyInstances_Returns_All_Null_Fallback()
    {
        var provider = new FakeWmiProvider("Win32_OperatingSystem", WmiRow.Empty());
        var result = await provider.ToSafeOperatingSystemMetricsAsync(CancellationToken.None);

        Assert.Null(result.Caption);
        Assert.Null(result.BuildNumber);
        Assert.Null(result.Manufacturer);
        Assert.Null(result.TotalVisibleMemorySize);
    }

    [Fact]
    public async Task Cancelled_Token_Returns_Fallback_Not_Throw()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var provider = new FakeWmiProvider("Win32_OperatingSystem", new[] { Win11Row() });
        var result = await provider.ToSafeOperatingSystemMetricsAsync(cts.Token);

        Assert.NotNull(result);
        Assert.Null(result.Caption);
    }

    [Fact]
    public async Task MissingKeys_Return_Null()
    {
        var row = WmiRow.Build(("Caption", new WmiValue("Windows")));
        var provider = new FakeWmiProvider("Win32_OperatingSystem", new[] { row });
        var result = await provider.ToSafeOperatingSystemMetricsAsync(CancellationToken.None);

        Assert.Equal("Windows", result.Caption);
        Assert.Null(result.BuildNumber);
        Assert.Null(result.TotalVisibleMemorySize);
        Assert.Null(result.MUILanguages);
    }
}
