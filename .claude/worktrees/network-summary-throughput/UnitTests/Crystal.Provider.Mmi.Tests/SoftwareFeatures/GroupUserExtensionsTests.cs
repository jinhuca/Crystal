using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Provider.Mmi.SoftwareFeatures.GroupUser;
using Crystal.Provider.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Provider.Mmi.Tests.SoftwareFeatures;

public class GroupUserExtensionsTests
{
    private const string GroupPath = "Win32_Group.Domain=\"DESKTOP-01\",Name=\"Administrators\"";
    private const string PartPath = "Win32_UserAccount.Domain=\"DESKTOP-01\",Name=\"jdoe\"";

    [Fact]
    public async Task FullData_Maps_GroupComponent_And_PartComponent()
    {
        var row = WmiRow.Build(
            ("GroupComponent", new WmiValue(GroupPath)),
            ("PartComponent", new WmiValue(PartPath)));

        var provider = new FakeWmiProvider("Win32_GroupUser", new[] { row });
        var results = await provider.ToSafeGroupUserMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal(GroupPath, results[0].GroupComponent);
        Assert.Equal(PartPath, results[0].PartComponent);
    }

    [Fact]
    public async Task FullData_Extracts_GroupName()
    {
        var row = WmiRow.Build(("GroupComponent", new WmiValue(GroupPath)));

        var provider = new FakeWmiProvider("Win32_GroupUser", new[] { row });
        var results = await provider.ToSafeGroupUserMetricsAsync(CancellationToken.None);

        Assert.Equal("Administrators", results[0].GroupName);
    }

    [Fact]
    public async Task FullData_Extracts_MemberName()
    {
        var row = WmiRow.Build(("PartComponent", new WmiValue(PartPath)));

        var provider = new FakeWmiProvider("Win32_GroupUser", new[] { row });
        var results = await provider.ToSafeGroupUserMetricsAsync(CancellationToken.None);

        Assert.Equal("jdoe", results[0].MemberName);
    }

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("Win32_GroupUser", WmiRow.Empty());
        var results = await provider.ToSafeGroupUserMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MissingClass_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("SomeOtherClass", WmiRow.Empty());
        var results = await provider.ToSafeGroupUserMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MultipleMemberships_Returns_All()
    {
        var m1 = WmiRow.Build(("PartComponent", new WmiValue(PartPath)));
        var m2 = WmiRow.Build(("PartComponent", new WmiValue("Win32_UserAccount.Domain=\"DESKTOP-01\",Name=\"asmith\"")));

        var provider = new FakeWmiProvider("Win32_GroupUser", new[] { m1, m2 });
        var results = await provider.ToSafeGroupUserMetricsAsync(CancellationToken.None);

        Assert.Equal(2, results.Count);
        Assert.Equal("jdoe", results[0].MemberName);
        Assert.Equal("asmith", results[1].MemberName);
    }

    [Fact]
    public async Task MissingReference_Leaves_Field_And_ExtractedName_Null()
    {
        var partial = WmiRow.Build(("GroupComponent", new WmiValue(GroupPath)));

        var provider = new FakeWmiProvider("Win32_GroupUser", new[] { partial });
        var results = await provider.ToSafeGroupUserMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Null(results[0].PartComponent);
        Assert.Null(results[0].MemberName);
    }

    [Fact]
    public async Task WrongTypeValue_Is_Ignored_Not_Miscast()
    {
        // GroupComponent stored as a bool instead of String — should be treated as absent, not throw.
        var badRow = WmiRow.Build(("GroupComponent", new WmiValue(true)));

        var provider = new FakeWmiProvider("Win32_GroupUser", new[] { badRow });
        var results = await provider.ToSafeGroupUserMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Null(results[0].GroupComponent);
        Assert.Null(results[0].GroupName);
    }
}
