using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Provider.Mmi.SoftwareFeatures.UserDesktop;
using Crystal.Provider.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Provider.Mmi.Tests.SoftwareFeatures;

public class UserDesktopExtensionsTests
{
    private const string ElementPath = "Win32_UserAccount.Domain=\"SOMEDOMAIN\",Name=\"johndoe\"";
    private const string SettingPath = "Win32_Desktop.Name=\"SOMEDOMAIN\\johndoe\"";

    [Fact]
    public async Task FullData_Maps_Element_And_Setting()
    {
        var row = WmiRow.Build(
            ("Element", new WmiValue(ElementPath)),
            ("Setting", new WmiValue(SettingPath)));

        var provider = new FakeWmiProvider("Win32_UserDesktop", new[] { row });
        var results = await provider.ToSafeUserDesktopMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal(ElementPath, results[0].Element);
        Assert.Equal(SettingPath, results[0].Setting);
    }

    [Fact]
    public async Task FullData_Extracts_UserAccountName()
    {
        var row = WmiRow.Build(("Element", new WmiValue(ElementPath)));

        var provider = new FakeWmiProvider("Win32_UserDesktop", new[] { row });
        var results = await provider.ToSafeUserDesktopMetricsAsync(CancellationToken.None);

        Assert.Equal("johndoe", results[0].UserAccountName);
    }

    [Fact]
    public async Task FullData_Extracts_DesktopSettingName()
    {
        var row = WmiRow.Build(("Setting", new WmiValue(SettingPath)));

        var provider = new FakeWmiProvider("Win32_UserDesktop", new[] { row });
        var results = await provider.ToSafeUserDesktopMetricsAsync(CancellationToken.None);

        Assert.Equal("SOMEDOMAIN\\johndoe", results[0].DesktopSettingName);
    }

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("Win32_UserDesktop", WmiRow.Empty());
        var results = await provider.ToSafeUserDesktopMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MissingClass_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("SomeOtherClass", WmiRow.Empty());
        var results = await provider.ToSafeUserDesktopMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MultipleRelationships_Returns_All()
    {
        var rel1 = WmiRow.Build(("Element", new WmiValue(ElementPath)));
        var rel2 = WmiRow.Build(("Element", new WmiValue("Win32_UserAccount.Domain=\"SOMEDOMAIN\",Name=\"janedoe\"")));

        var provider = new FakeWmiProvider("Win32_UserDesktop", new[] { rel1, rel2 });
        var results = await provider.ToSafeUserDesktopMetricsAsync(CancellationToken.None);

        Assert.Equal(2, results.Count);
        Assert.Equal("johndoe", results[0].UserAccountName);
        Assert.Equal("janedoe", results[1].UserAccountName);
    }

    [Fact]
    public async Task MissingReference_Leaves_Field_Null()
    {
        var partial = WmiRow.Build(("Element", new WmiValue(ElementPath)));

        var provider = new FakeWmiProvider("Win32_UserDesktop", new[] { partial });
        var results = await provider.ToSafeUserDesktopMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Null(results[0].Setting);
        Assert.Null(results[0].DesktopSettingName);
    }

    [Fact]
    public async Task WrongTypeValue_Is_Ignored_Not_Miscast()
    {
        // Element stored as a bool instead of String — should be treated as absent, not throw.
        var badRow = WmiRow.Build(("Element", new WmiValue(true)));

        var provider = new FakeWmiProvider("Win32_UserDesktop", new[] { badRow });
        var results = await provider.ToSafeUserDesktopMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Null(results[0].Element);
        Assert.Null(results[0].UserAccountName);
    }
}
