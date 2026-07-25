using Crystal.Mmi.MmiEngine;
using Crystal.Mmi.SoftwareFeatures.Directory;
using Crystal.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Mmi.Tests.SoftwareFeatures;

public class DirectoryExtensionsTests
{
    private static System.Collections.Frozen.FrozenDictionary<string, WmiValue> TempDirRow() => WmiRow.Build(
        ("AccessMask", new WmiValue(1179785)),
        ("Archive", new WmiValue(false)),
        ("Caption", new WmiValue("C:\\Temp")),
        ("Compressed", new WmiValue(false)),
        ("CompressionMethod", new WmiValue("Not Compressed")),
        ("CreationClassName", new WmiValue("Win32_Directory")),
        ("CreationDate", new WmiValue(new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc))),
        ("CSCreationClassName", new WmiValue("Win32_ComputerSystem")),
        ("CSName", new WmiValue("DESKTOP-01")),
        ("Description", new WmiValue("C:\\Temp")),
        ("Drive", new WmiValue("c:")),
        ("EightDotThreeFileName", new WmiValue("c:\\temp")),
        ("Encrypted", new WmiValue(false)),
        ("EncryptionMethod", new WmiValue("Not Encrypted")),
        ("Extension", new WmiValue("")),
        ("FileName", new WmiValue("Temp")),
        ("FileSize", new WmiValue(0UL)),
        ("FileType", new WmiValue("File Folder")),
        ("FSCreationClassName", new WmiValue("Win32_FileSystem")),
        ("FSName", new WmiValue("NTFS")),
        ("Hidden", new WmiValue(false)),
        ("InstallDate", new WmiValue(new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc))),
        ("InUseCount", new WmiValue(0UL)),
        ("LastAccessed", new WmiValue(new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc))),
        ("LastModified", new WmiValue(new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc))),
        ("Name", new WmiValue("C:\\Temp")),
        ("Path", new WmiValue("\\")),
        ("Readable", new WmiValue(true)),
        ("Status", new WmiValue("OK")),
        ("System", new WmiValue(false)),
        ("Writeable", new WmiValue(true))
    );

    [Fact]
    public async Task FullData_Maps_Name()
    {
        var provider = new FakeWmiProvider("Win32_Directory", new[] { TempDirRow() });
        var results = await provider.ToSafeDirectoryMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("C:\\Temp", results[0].Name);
    }

    [Fact]
    public async Task FullData_Maps_FileSize_Ulong()
    {
        var provider = new FakeWmiProvider("Win32_Directory", new[] { TempDirRow() });
        var results = await provider.ToSafeDirectoryMetricsAsync(CancellationToken.None);

        Assert.Equal(0UL, results[0].FileSize);
    }

    [Fact]
    public async Task FullData_Maps_Drive()
    {
        var provider = new FakeWmiProvider("Win32_Directory", new[] { TempDirRow() });
        var results = await provider.ToSafeDirectoryMetricsAsync(CancellationToken.None);

        Assert.Equal("c:", results[0].Drive);
    }

    [Fact]
    public async Task FullData_Maps_Writeable_True()
    {
        var provider = new FakeWmiProvider("Win32_Directory", new[] { TempDirRow() });
        var results = await provider.ToSafeDirectoryMetricsAsync(CancellationToken.None);

        Assert.True(results[0].Writeable);
    }

    [Fact]
    public async Task FullData_Maps_FSName()
    {
        var provider = new FakeWmiProvider("Win32_Directory", new[] { TempDirRow() });
        var results = await provider.ToSafeDirectoryMetricsAsync(CancellationToken.None);

        Assert.Equal("NTFS", results[0].FSName);
    }

    [Fact]
    public async Task FullData_Maps_LastModified()
    {
        var provider = new FakeWmiProvider("Win32_Directory", new[] { TempDirRow() });
        var results = await provider.ToSafeDirectoryMetricsAsync(CancellationToken.None);

        Assert.Equal(new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc), results[0].LastModified);
    }

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("Win32_Directory", WmiRow.Empty());
        var results = await provider.ToSafeDirectoryMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MissingClass_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("SomeOtherClass", WmiRow.Empty());
        var results = await provider.ToSafeDirectoryMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MultipleDirectories_Returns_All()
    {
        var d1 = WmiRow.Build(("Name", new WmiValue("C:\\Temp")));
        var d2 = WmiRow.Build(("Name", new WmiValue("C:\\Windows")));

        var provider = new FakeWmiProvider("Win32_Directory", new[] { d1, d2 });
        var results = await provider.ToSafeDirectoryMetricsAsync(CancellationToken.None);

        Assert.Equal(2, results.Count);
        Assert.Equal("C:\\Temp", results[0].Name);
        Assert.Equal("C:\\Windows", results[1].Name);
    }

    [Fact]
    public async Task PartialData_Leaves_Missing_Fields_Null()
    {
        var partial = WmiRow.Build(("Name", new WmiValue("C:\\Partial")));

        var provider = new FakeWmiProvider("Win32_Directory", new[] { partial });
        var results = await provider.ToSafeDirectoryMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("C:\\Partial", results[0].Name);
        Assert.Null(results[0].FileSize);
        Assert.Null(results[0].LastModified);
    }

    [Fact]
    public async Task WrongTypeValue_Is_Ignored_Not_Miscast()
    {
        // FileSize stored as an Int instead of ULong — should be treated as absent, not throw.
        var badRow = WmiRow.Build(("FileSize", new WmiValue(0)));

        var provider = new FakeWmiProvider("Win32_Directory", new[] { badRow });
        var results = await provider.ToSafeDirectoryMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Null(results[0].FileSize);
    }
}
