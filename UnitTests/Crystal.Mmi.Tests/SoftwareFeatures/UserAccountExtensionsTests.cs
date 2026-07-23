using Crystal.Mmi.MmiEngine;
using Crystal.Mmi.SoftwareFeatures.UserAccount;
using Crystal.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Mmi.Tests.SoftwareFeatures;

public class UserAccountExtensionsTests
{
    private static System.Collections.Frozen.FrozenDictionary<string, WmiValue> AccountRow() => WmiRow.Build(
        ("AccountType", new WmiValue(512)),
        ("Caption", new WmiValue("SOMEDOMAIN\\johndoe")),
        ("Description", new WmiValue("")),
        ("Disabled", new WmiValue(false)),
        ("Domain", new WmiValue("SOMEDOMAIN")),
        ("FullName", new WmiValue("John Doe")),
        ("InstallDate", new WmiValue(new DateTime(2020, 3, 15, 0, 0, 0, DateTimeKind.Utc))),
        ("LocalAccount", new WmiValue(false)),
        ("Lockout", new WmiValue(false)),
        ("Name", new WmiValue("johndoe")),
        ("PasswordChangeable", new WmiValue(true)),
        ("PasswordExpires", new WmiValue(true)),
        ("PasswordRequired", new WmiValue(true)),
        ("SID", new WmiValue("S-1-5-21-1579938362-1064596589-3161144252-1006")),
        ("SIDType", new WmiValue(1)),
        ("Status", new WmiValue("OK"))
    );

    [Fact]
    public async Task FullData_Maps_Name()
    {
        var provider = new FakeWmiProvider("Win32_UserAccount", new[] { AccountRow() });
        var results = await provider.ToSafeUserAccountMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("johndoe", results[0].Name);
    }

    [Fact]
    public async Task FullData_Maps_FullName()
    {
        var provider = new FakeWmiProvider("Win32_UserAccount", new[] { AccountRow() });
        var results = await provider.ToSafeUserAccountMetricsAsync(CancellationToken.None);

        Assert.Equal("John Doe", results[0].FullName);
    }

    [Fact]
    public async Task FullData_Maps_SID()
    {
        var provider = new FakeWmiProvider("Win32_UserAccount", new[] { AccountRow() });
        var results = await provider.ToSafeUserAccountMetricsAsync(CancellationToken.None);

        Assert.Equal("S-1-5-21-1579938362-1064596589-3161144252-1006", results[0].SID);
    }

    [Fact]
    public async Task FullData_Maps_SIDType_Byte()
    {
        var provider = new FakeWmiProvider("Win32_UserAccount", new[] { AccountRow() });
        var results = await provider.ToSafeUserAccountMetricsAsync(CancellationToken.None);

        Assert.Equal((byte)1, results[0].SIDType);
    }

    [Fact]
    public async Task FullData_Maps_AccountType_Uint()
    {
        var provider = new FakeWmiProvider("Win32_UserAccount", new[] { AccountRow() });
        var results = await provider.ToSafeUserAccountMetricsAsync(CancellationToken.None);

        Assert.Equal(512u, results[0].AccountType);
    }

    [Fact]
    public async Task FullData_Maps_Disabled_False()
    {
        var provider = new FakeWmiProvider("Win32_UserAccount", new[] { AccountRow() });
        var results = await provider.ToSafeUserAccountMetricsAsync(CancellationToken.None);

        Assert.False(results[0].Disabled);
    }

    [Fact]
    public async Task FullData_Maps_Domain()
    {
        var provider = new FakeWmiProvider("Win32_UserAccount", new[] { AccountRow() });
        var results = await provider.ToSafeUserAccountMetricsAsync(CancellationToken.None);

        Assert.Equal("SOMEDOMAIN", results[0].Domain);
    }

    [Fact]
    public async Task FullData_Maps_InstallDate()
    {
        var provider = new FakeWmiProvider("Win32_UserAccount", new[] { AccountRow() });
        var results = await provider.ToSafeUserAccountMetricsAsync(CancellationToken.None);

        Assert.Equal(new DateTime(2020, 3, 15, 0, 0, 0, DateTimeKind.Utc), results[0].InstallDate);
    }

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("Win32_UserAccount", WmiRow.Empty());
        var results = await provider.ToSafeUserAccountMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MissingClass_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("SomeOtherClass", WmiRow.Empty());
        var results = await provider.ToSafeUserAccountMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MultipleAccounts_Returns_All()
    {
        var acct1 = WmiRow.Build(("Name", new WmiValue("johndoe")));
        var acct2 = WmiRow.Build(("Name", new WmiValue("janedoe")));

        var provider = new FakeWmiProvider("Win32_UserAccount", new[] { acct1, acct2 });
        var results = await provider.ToSafeUserAccountMetricsAsync(CancellationToken.None);

        Assert.Equal(2, results.Count);
        Assert.Equal("johndoe", results[0].Name);
        Assert.Equal("janedoe", results[1].Name);
    }

    [Fact]
    public async Task PartialData_Leaves_Missing_Fields_Null()
    {
        var partial = WmiRow.Build(("Name", new WmiValue("Account Without SID")));

        var provider = new FakeWmiProvider("Win32_UserAccount", new[] { partial });
        var results = await provider.ToSafeUserAccountMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("Account Without SID", results[0].Name);
        Assert.Null(results[0].SID);
        Assert.Null(results[0].SIDType);
    }

    [Fact]
    public async Task WrongTypeValue_Is_Ignored_Not_Miscast()
    {
        // Disabled stored as a string instead of Bool — should be treated as absent, not throw.
        var badRow = WmiRow.Build(("Disabled", new WmiValue("false")));

        var provider = new FakeWmiProvider("Win32_UserAccount", new[] { badRow });
        var results = await provider.ToSafeUserAccountMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Null(results[0].Disabled);
    }
}
