using Crystal.Mmi.HardwareFeatures.SMBIOSMemory;
using Crystal.Mmi.MmiEngine;
using Crystal.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Mmi.Tests.HardwareFeatures;

public sealed class SMBIOSMemoryExtensionsTests
{
    private static System.Collections.Frozen.FrozenDictionary<string, WmiValue> MemoryRow()
        => WmiRow.Build(
            ("Access", new WmiValue(3)),
            ("AdditionalErrorData", new WmiValue("None")),
            ("Availability", new WmiValue(3)),
            ("BlockSize", new WmiValue(1024UL)),
            ("Caption", new WmiValue("SMBIOS Memory")),
            ("ConfigManagerErrorCode", new WmiValue(0)),
            ("ConfigManagerUserConfig", new WmiValue(false)),
            ("CorrectableError", new WmiValue(false)),
            ("CreationClassName", new WmiValue("Win32_SMBIOSMemory")),
            ("Description", new WmiValue("SMBIOS Memory Device")),
            ("EndingAddress", new WmiValue(34_359_738_367UL)),
            ("ErrorAccess", new WmiValue(3)),
            ("ErrorAddress", new WmiValue(0UL)),
            ("ErrorCleared", new WmiValue(true)),
            ("ErrorCorrectType", new WmiValue(4)),
            ("ErrorData", new WmiValue("")),
            ("ErrorDataOrder", new WmiValue(1)),
            ("ErrorDescription", new WmiValue("No error")),
            ("ErrorInfo", new WmiValue(3)),
            ("ErrorMethodology", new WmiValue("None")),
            ("ErrorResolution", new WmiValue(0UL)),
            ("ErrorTime", new WmiValue(new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc))),
            ("ErrorTransferSize", new WmiValue(0)),
            ("InstallDate", new WmiValue(new DateTime(2023, 2, 10, 0, 0, 0, DateTimeKind.Utc))),
            ("LastErrorCode", new WmiValue(0)),
            ("Name", new WmiValue("Physical Memory Range")),
            ("NumberOfBlocks", new WmiValue(33_554_432UL)),
            ("OtherErrorDescription", new WmiValue("")),
            ("PNPDeviceID", new WmiValue("ROOT\\MEMORY\\0000")),
            ("PowerManagementCapabilities", new WmiValue(new ushort[] { 1, 2 })),
            ("PowerManagementSupported", new WmiValue(false)),
            ("Purpose", new WmiValue("System Memory")),
            ("StartingAddress", new WmiValue(0UL)),
            ("Status", new WmiValue("OK")),
            ("StatusInfo", new WmiValue(3)),
            ("SystemCreationClassName", new WmiValue("Win32_ComputerSystem")),
            ("SystemLevelAddress", new WmiValue(true)),
            ("SystemName", new WmiValue("DESKTOP-01"))
        );

    [Fact]
    public async Task FullData_Maps_Identity_Fields()
    {
        var provider = new FakeWmiProvider("Win32_SMBIOSMemory", new[] { MemoryRow() });

        var results = await provider.ToSafeSMBIOSMemoryMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("SMBIOS Memory", results[0].Caption);
        Assert.Equal("SMBIOS Memory Device", results[0].Description);
        Assert.Equal("Physical Memory Range", results[0].Name);
        Assert.Equal("OK", results[0].Status);
        Assert.Equal("DESKTOP-01", results[0].SystemName);
    }

    [Fact]
    public async Task FullData_Maps_Address_And_Block_Fields()
    {
        var provider = new FakeWmiProvider("Win32_SMBIOSMemory", new[] { MemoryRow() });

        var results = await provider.ToSafeSMBIOSMemoryMetricsAsync(CancellationToken.None);

        Assert.Equal(1024UL, results[0].BlockSize);
        Assert.Equal(33_554_432UL, results[0].NumberOfBlocks);
        Assert.Equal(0UL, results[0].StartingAddress);
        Assert.Equal(34_359_738_367UL, results[0].EndingAddress);
        Assert.Equal(34_359_738_368UL, results[0].CapacityBytes);
        Assert.Equal(32, results[0].CapacityInGB);
    }

    [Fact]
    public async Task FullData_Maps_Error_Fields()
    {
        var provider = new FakeWmiProvider("Win32_SMBIOSMemory", new[] { MemoryRow() });

        var results = await provider.ToSafeSMBIOSMemoryMetricsAsync(CancellationToken.None);

        Assert.False(results[0].CorrectableError);
        Assert.True(results[0].ErrorCleared);
        Assert.Equal((ushort)3, results[0].ErrorAccess);
        Assert.Equal(0UL, results[0].ErrorAddress);
        Assert.Equal((ushort)4, results[0].ErrorCorrectType);
        Assert.Equal("No error", results[0].ErrorDescription);
        Assert.Equal((ushort)3, results[0].ErrorInfo);
        Assert.Equal(new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc), results[0].ErrorTime);
    }

    [Fact]
    public async Task FullData_Maps_Power_Management_Fields()
    {
        var provider = new FakeWmiProvider("Win32_SMBIOSMemory", new[] { MemoryRow() });

        var results = await provider.ToSafeSMBIOSMemoryMetricsAsync(CancellationToken.None);

        Assert.Equal(new ushort[] { 1, 2 }, results[0].PowerManagementCapabilities);
        Assert.False(results[0].PowerManagementSupported);
    }

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("Win32_SMBIOSMemory", WmiRow.Empty());

        var results = await provider.ToSafeSMBIOSMemoryMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MissingClass_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("SomeOtherClass", WmiRow.Empty());

        var results = await provider.ToSafeSMBIOSMemoryMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MultipleInstances_Returns_All()
    {
        var first = WmiRow.Build(("Name", new WmiValue("Range 0")), ("BlockSize", new WmiValue(1024UL)));
        var second = WmiRow.Build(("Name", new WmiValue("Range 1")), ("BlockSize", new WmiValue(2048UL)));
        var provider = new FakeWmiProvider("Win32_SMBIOSMemory", new[] { first, second });

        var results = await provider.ToSafeSMBIOSMemoryMetricsAsync(CancellationToken.None);

        Assert.Equal(2, results.Count);
        Assert.Equal("Range 0", results[0].Name);
        Assert.Equal("Range 1", results[1].Name);
        Assert.Equal(1024UL, results[0].BlockSize);
        Assert.Equal(2048UL, results[1].BlockSize);
    }

    [Fact]
    public async Task PartialData_Leaves_Missing_Fields_Null()
    {
        var partial = WmiRow.Build(("Name", new WmiValue("Partial SMBIOS Memory")));
        var provider = new FakeWmiProvider("Win32_SMBIOSMemory", new[] { partial });

        var results = await provider.ToSafeSMBIOSMemoryMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("Partial SMBIOS Memory", results[0].Name);
        Assert.Null(results[0].BlockSize);
        Assert.Null(results[0].NumberOfBlocks);
        Assert.Null(results[0].CapacityBytes);
        Assert.Null(results[0].CapacityInGB);
    }

    [Fact]
    public async Task WrongTypeValue_Is_Ignored_Not_Miscast()
    {
        var badRow = WmiRow.Build(
            ("Name", new WmiValue(true)),
            ("BlockSize", new WmiValue("1024")),
            ("PowerManagementCapabilities", new WmiValue("1,2")),
            ("ErrorTime", new WmiValue("2024-01-02")));
        var provider = new FakeWmiProvider("Win32_SMBIOSMemory", new[] { badRow });

        var results = await provider.ToSafeSMBIOSMemoryMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Null(results[0].Name);
        Assert.Null(results[0].BlockSize);
        Assert.Null(results[0].PowerManagementCapabilities);
        Assert.Null(results[0].ErrorTime);
    }

  [Fact]
  public async Task Two_Identical_Records_Have_Equal_Field_Values() {
    var provider1 = new FakeWmiProvider(
        "Win32_SMBIOSMemory",
        new[] { MemoryRow() });

    var provider2 = new FakeWmiProvider(
        "Win32_SMBIOSMemory",
        new[] { MemoryRow() });

    var r1 =
        (await provider1.ToSafeSMBIOSMemoryMetricsAsync(
            CancellationToken.None))[0];

    var r2 =
        (await provider2.ToSafeSMBIOSMemoryMetricsAsync(
            CancellationToken.None))[0];

    Assert.Equal(r1 with { PowerManagementCapabilities = null },
                 r2 with { PowerManagementCapabilities = null });

    Assert.Equal(
        r1.PowerManagementCapabilities,
        r2.PowerManagementCapabilities);
  }
}
