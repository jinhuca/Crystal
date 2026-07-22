using Crystal.Mmi.MmiEngine;
using Crystal.Mmi.SoftwareFeatures.NetworkLoginProfile;
using Crystal.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Mmi.Tests.SoftwareFeatures;

public class NetworkLoginProfileExtensionsTests
{
    private static System.Collections.Frozen.FrozenDictionary<string, WmiValue> ProfileRow() => WmiRow.Build(
        ("AccountExpires", new WmiValue(new DateTime(2052, 12, 1, 0, 0, 0, DateTimeKind.Utc))),
        ("AuthorizationFlags", new WmiValue(8)),
        ("BadPasswordCount", new WmiValue(0)),
        ("Caption", new WmiValue("johndoe")),
        ("CodePage", new WmiValue(437)),
        ("Comment", new WmiValue("Standard user profile")),
        ("CountryCode", new WmiValue(1)),
        ("Description", new WmiValue("johndoe")),
        ("Flags", new WmiValue(512)),
        ("FullName", new WmiValue("John Doe")),
        ("HomeDirectory", new WmiValue("\\HOMEDIR")),
        ("HomeDirectoryDrive", new WmiValue("C:")),
        ("LastLogoff", new WmiValue(new DateTime(2024, 6, 1, 17, 0, 0, DateTimeKind.Utc))),
        ("LastLogon", new WmiValue(new DateTime(2024, 6, 1, 8, 0, 0, DateTimeKind.Utc))),
        ("LogonHours", new WmiValue("")),
        ("LogonServer", new WmiValue("\\\\DC01")),
        ("MaximumStorage", new WmiValue(10000000UL)),
        ("Name", new WmiValue("somedomain\\johndoe")),
        ("NumberOfLogons", new WmiValue(4)),
        ("Parameters", new WmiValue("")),
        ("PasswordAge", new WmiValue(new DateTime(1601, 1, 12, 0, 0, 0, DateTimeKind.Utc))), // interval, not absolute
        ("PasswordExpires", new WmiValue(new DateTime(2052, 12, 1, 0, 0, 0, DateTimeKind.Utc))),
        ("PrimaryGroupId", new WmiValue(513)),
        ("Privileges", new WmiValue(1)),
        ("Profile", new WmiValue("C:\\Windows")),
        ("ScriptPath", new WmiValue("C:\\win\\profiles\\johndoe")),
        ("SettingID", new WmiValue("somedomain\\johndoe")),
        ("UnitsPerWeek", new WmiValue(168)),
        ("UserComment", new WmiValue("")),
        ("UserId", new WmiValue(1001)),
        ("UserType", new WmiValue("Normal Account")),
        ("Workstations", new WmiValue(""))
    );

    [Fact]
    public async Task FullData_Maps_Name()
    {
        var provider = new FakeWmiProvider("Win32_NetworkLoginProfile", new[] { ProfileRow() });
        var results = await provider.ToSafeNetworkLoginProfileMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("somedomain\\johndoe", results[0].Name);
    }

    [Fact]
    public async Task FullData_Maps_FullName()
    {
        var provider = new FakeWmiProvider("Win32_NetworkLoginProfile", new[] { ProfileRow() });
        var results = await provider.ToSafeNetworkLoginProfileMetricsAsync(CancellationToken.None);

        Assert.Equal("John Doe", results[0].FullName);
    }

    [Fact]
    public async Task FullData_Maps_MaximumStorage_Ulong()
    {
        var provider = new FakeWmiProvider("Win32_NetworkLoginProfile", new[] { ProfileRow() });
        var results = await provider.ToSafeNetworkLoginProfileMetricsAsync(CancellationToken.None);

        Assert.Equal(10000000UL, results[0].MaximumStorage);
    }

    [Fact]
    public async Task FullData_Maps_UserId_Uint()
    {
        var provider = new FakeWmiProvider("Win32_NetworkLoginProfile", new[] { ProfileRow() });
        var results = await provider.ToSafeNetworkLoginProfileMetricsAsync(CancellationToken.None);

        Assert.Equal(1001u, results[0].UserId);
    }

    [Fact]
    public async Task FullData_Maps_UserType()
    {
        var provider = new FakeWmiProvider("Win32_NetworkLoginProfile", new[] { ProfileRow() });
        var results = await provider.ToSafeNetworkLoginProfileMetricsAsync(CancellationToken.None);

        Assert.Equal("Normal Account", results[0].UserType);
    }

    [Fact]
    public async Task FullData_Maps_LastLogon()
    {
        var provider = new FakeWmiProvider("Win32_NetworkLoginProfile", new[] { ProfileRow() });
        var results = await provider.ToSafeNetworkLoginProfileMetricsAsync(CancellationToken.None);

        Assert.Equal(new DateTime(2024, 6, 1, 8, 0, 0, DateTimeKind.Utc), results[0].LastLogon);
    }

    [Fact]
    public async Task FullData_Maps_SettingID()
    {
        var provider = new FakeWmiProvider("Win32_NetworkLoginProfile", new[] { ProfileRow() });
        var results = await provider.ToSafeNetworkLoginProfileMetricsAsync(CancellationToken.None);

        Assert.Equal("somedomain\\johndoe", results[0].SettingID);
    }

    [Fact]
    public async Task FullData_Maps_UnitsPerWeek_Uint()
    {
        var provider = new FakeWmiProvider("Win32_NetworkLoginProfile", new[] { ProfileRow() });
        var results = await provider.ToSafeNetworkLoginProfileMetricsAsync(CancellationToken.None);

        Assert.Equal(168u, results[0].UnitsPerWeek);
    }

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("Win32_NetworkLoginProfile", WmiRow.Empty());
        var results = await provider.ToSafeNetworkLoginProfileMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MissingClass_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("SomeOtherClass", WmiRow.Empty());
        var results = await provider.ToSafeNetworkLoginProfileMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MultipleProfiles_Returns_All()
    {
        var profile1 = WmiRow.Build(("Name", new WmiValue("somedomain\\johndoe")));
        var profile2 = WmiRow.Build(("Name", new WmiValue("somedomain\\janedoe")));

        var provider = new FakeWmiProvider("Win32_NetworkLoginProfile", new[] { profile1, profile2 });
        var results = await provider.ToSafeNetworkLoginProfileMetricsAsync(CancellationToken.None);

        Assert.Equal(2, results.Count);
        Assert.Equal("somedomain\\johndoe", results[0].Name);
        Assert.Equal("somedomain\\janedoe", results[1].Name);
    }

    [Fact]
    public async Task PartialData_Leaves_Missing_Fields_Null()
    {
        var partial = WmiRow.Build(("Name", new WmiValue("Profile Without Storage Info")));

        var provider = new FakeWmiProvider("Win32_NetworkLoginProfile", new[] { partial });
        var results = await provider.ToSafeNetworkLoginProfileMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("Profile Without Storage Info", results[0].Name);
        Assert.Null(results[0].MaximumStorage);
        Assert.Null(results[0].UserId);
    }

    [Fact]
    public async Task WrongTypeValue_Is_Ignored_Not_Miscast()
    {
        // MaximumStorage stored as an Int instead of ULong — should be treated as absent, not throw.
        var badRow = WmiRow.Build(("MaximumStorage", new WmiValue(10000000)));

        var provider = new FakeWmiProvider("Win32_NetworkLoginProfile", new[] { badRow });
        var results = await provider.ToSafeNetworkLoginProfileMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Null(results[0].MaximumStorage);
    }
}
