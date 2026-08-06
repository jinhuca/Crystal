using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Provider.Mmi.SoftwareFeatures.LogonSession;
using Crystal.Provider.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Provider.Mmi.Tests.SoftwareFeatures;

public class LogonSessionExtensionsTests
{
    private static System.Collections.Frozen.FrozenDictionary<string, WmiValue> SessionRow() => WmiRow.Build(
        ("AuthenticationPackage", new WmiValue("Negotiate")),
        ("Caption", new WmiValue("")),
        ("Description", new WmiValue("")),
        ("InstallDate", new WmiValue(new DateTime(2024, 6, 1, 8, 0, 0, DateTimeKind.Utc))),
        ("LogonId", new WmiValue("999888")),
        ("LogonType", new WmiValue(2)),
        ("Name", new WmiValue("")),
        ("StartTime", new WmiValue(new DateTime(2024, 6, 1, 8, 0, 5, DateTimeKind.Utc))),
        ("Status", new WmiValue("OK"))
    );

    [Fact]
    public async Task FullData_Maps_LogonId()
    {
        var provider = new FakeWmiProvider("Win32_LogonSession", new[] { SessionRow() });
        var results = await provider.ToSafeLogonSessionMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("999888", results[0].LogonId);
    }

    [Fact]
    public async Task FullData_Maps_LogonType_Uint()
    {
        var provider = new FakeWmiProvider("Win32_LogonSession", new[] { SessionRow() });
        var results = await provider.ToSafeLogonSessionMetricsAsync(CancellationToken.None);

        Assert.Equal(2u, results[0].LogonType);
    }

    [Fact]
    public async Task FullData_Maps_AuthenticationPackage()
    {
        var provider = new FakeWmiProvider("Win32_LogonSession", new[] { SessionRow() });
        var results = await provider.ToSafeLogonSessionMetricsAsync(CancellationToken.None);

        Assert.Equal("Negotiate", results[0].AuthenticationPackage);
    }

    [Fact]
    public async Task FullData_Maps_StartTime()
    {
        var provider = new FakeWmiProvider("Win32_LogonSession", new[] { SessionRow() });
        var results = await provider.ToSafeLogonSessionMetricsAsync(CancellationToken.None);

        Assert.Equal(new DateTime(2024, 6, 1, 8, 0, 5, DateTimeKind.Utc), results[0].StartTime);
    }

    [Fact]
    public async Task FullData_Maps_Status_OK()
    {
        var provider = new FakeWmiProvider("Win32_LogonSession", new[] { SessionRow() });
        var results = await provider.ToSafeLogonSessionMetricsAsync(CancellationToken.None);

        Assert.Equal("OK", results[0].Status);
    }

    [Fact]
    public async Task FullData_Maps_InstallDate()
    {
        var provider = new FakeWmiProvider("Win32_LogonSession", new[] { SessionRow() });
        var results = await provider.ToSafeLogonSessionMetricsAsync(CancellationToken.None);

        Assert.Equal(new DateTime(2024, 6, 1, 8, 0, 0, DateTimeKind.Utc), results[0].InstallDate);
    }

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("Win32_LogonSession", WmiRow.Empty());
        var results = await provider.ToSafeLogonSessionMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MissingClass_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("SomeOtherClass", WmiRow.Empty());
        var results = await provider.ToSafeLogonSessionMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MultipleSessions_Returns_All()
    {
        var session1 = WmiRow.Build(("LogonId", new WmiValue("111")));
        var session2 = WmiRow.Build(("LogonId", new WmiValue("222")));

        var provider = new FakeWmiProvider("Win32_LogonSession", new[] { session1, session2 });
        var results = await provider.ToSafeLogonSessionMetricsAsync(CancellationToken.None);

        Assert.Equal(2, results.Count);
        Assert.Equal("111", results[0].LogonId);
        Assert.Equal("222", results[1].LogonId);
    }

    [Fact]
    public async Task PartialData_Leaves_Missing_Fields_Null()
    {
        var partial = WmiRow.Build(("LogonId", new WmiValue("333")));

        var provider = new FakeWmiProvider("Win32_LogonSession", new[] { partial });
        var results = await provider.ToSafeLogonSessionMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("333", results[0].LogonId);
        Assert.Null(results[0].LogonType);
        Assert.Null(results[0].StartTime);
    }

    [Fact]
    public async Task WrongTypeValue_Is_Ignored_Not_Miscast()
    {
        // LogonType stored as a string instead of Int — should be treated as absent, not throw.
        var badRow = WmiRow.Build(("LogonType", new WmiValue("2")));

        var provider = new FakeWmiProvider("Win32_LogonSession", new[] { badRow });
        var results = await provider.ToSafeLogonSessionMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Null(results[0].LogonType);
    }
}
