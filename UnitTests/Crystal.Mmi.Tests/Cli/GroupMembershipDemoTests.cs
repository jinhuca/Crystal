using Crystal.Mmi.Cli;
using Crystal.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Mmi.Tests.Cli;

public sealed class GroupMembershipDemoTests
{
    [Fact]
    public async Task DumpLocalGroupMembership_Writes_Header()
    {
        using var console = new TestConsoleWriter();

        await CliDemos.DumpLocalGroupMembershipAsync(DemoProviders.GroupsWithMembership(), console.Writer, TestContext.Current.CancellationToken);

        Assert.Contains("Local Group Membership", console.Output);
    }

    [Fact]
    public async Task DumpLocalGroupMembership_Lists_Groups_And_Resolved_Members()
    {
        using var console = new TestConsoleWriter();

        await CliDemos.DumpLocalGroupMembershipAsync(DemoProviders.GroupsWithMembership(), console.Writer, TestContext.Current.CancellationToken);

        Assert.Contains("Group: Administrators", console.Output);
        Assert.Contains("- jdoe", console.Output);
        Assert.Contains("- asmith", console.Output);
    }

    [Fact]
    public async Task DumpLocalGroupMembership_Group_Without_Members_Shows_Placeholder()
    {
        using var console = new TestConsoleWriter();

        await CliDemos.DumpLocalGroupMembershipAsync(DemoProviders.GroupsWithMembership(), console.Writer, TestContext.Current.CancellationToken);

        Assert.Contains("Group: Users", console.Output);
        Assert.Contains("(no members found)", console.Output);
    }

    [Fact]
    public async Task DumpLocalGroupMembership_When_Empty_Writes_Header_Only()
    {
        using var console = new TestConsoleWriter();

        await CliDemos.DumpLocalGroupMembershipAsync(DemoProviders.Empty("Win32_Group"), console.Writer, TestContext.Current.CancellationToken);

        Assert.Contains("Local Group Membership", console.Output);
        Assert.DoesNotContain("Group:", console.Output);
    }
}
