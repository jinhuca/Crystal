using Crystal.Mmi.MmiEngine;
using Crystal.Mmi.SoftwareFeatures.Service;
using Crystal.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Mmi.Tests.SoftwareFeatures;

public class ServiceExtensionsTests
{
    private static System.Collections.Frozen.FrozenDictionary<string, WmiValue> WindowsUpdateRow() => WmiRow.Build(
        ("Name", new WmiValue("wuauserv")),
        ("Caption", new WmiValue("Windows Update")),
        ("DisplayName", new WmiValue("Windows Update")),
        ("Description", new WmiValue("Enables the detection, download, and installation of updates.")),
        ("State", new WmiValue("Running")),
        ("Status", new WmiValue("OK")),
        ("StatusInfo", new WmiValue(3)),
        ("Started", new WmiValue(true)),
        ("StartMode", new WmiValue("Automatic")),
        ("StartName", new WmiValue("LocalSystem")),
        ("ServiceType", new WmiValue("Share Process")),
        ("PathName", new WmiValue("C:\\Windows\\system32\\svchost.exe -k netsvcs")),
        ("ProcessId", new WmiValue(1240)),
        ("ExitCode", new WmiValue(0)),
        ("ServiceSpecificExitCode", new WmiValue(0)),
        ("AcceptPause", new WmiValue(false)),
        ("AcceptStop", new WmiValue(true)),
        ("DesktopInteract", new WmiValue(false)),
        ("TagId", new WmiValue(0)),
        ("ErrorControl", new WmiValue("Normal")),
        ("SystemName", new WmiValue("DESKTOP-01")),
        ("SystemCreationClassName", new WmiValue("Win32_ComputerSystem")),
        ("CreationClassName", new WmiValue("Win32_Service")),
        ("InstallationDate", new WmiValue(new DateTime(2019, 11, 1, 0, 0, 0, DateTimeKind.Utc)))
    );

    [Fact]
    public async Task FullData_Maps_Name()
    {
        var provider = new FakeWmiProvider("Win32_Service", new[] { WindowsUpdateRow() });
        var results = await provider.ToSafeServiceMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("wuauserv", results[0].Name);
    }

    [Fact]
    public async Task FullData_Maps_DisplayName()
    {
        var provider = new FakeWmiProvider("Win32_Service", new[] { WindowsUpdateRow() });
        var results = await provider.ToSafeServiceMetricsAsync(CancellationToken.None);

        Assert.Equal("Windows Update", results[0].DisplayName);
    }

    [Fact]
    public async Task FullData_Maps_State()
    {
        var provider = new FakeWmiProvider("Win32_Service", new[] { WindowsUpdateRow() });
        var results = await provider.ToSafeServiceMetricsAsync(CancellationToken.None);

        Assert.Equal("Running", results[0].State);
    }

    [Fact]
    public async Task FullData_Maps_Status()
    {
        var provider = new FakeWmiProvider("Win32_Service", new[] { WindowsUpdateRow() });
        var results = await provider.ToSafeServiceMetricsAsync(CancellationToken.None);

        Assert.Equal("OK", results[0].Status);
    }

    [Fact]
    public async Task FullData_Maps_Started_True()
    {
        var provider = new FakeWmiProvider("Win32_Service", new[] { WindowsUpdateRow() });
        var results = await provider.ToSafeServiceMetricsAsync(CancellationToken.None);

        Assert.True(results[0].Started);
    }

    [Fact]
    public async Task FullData_Maps_AcceptStop_True()
    {
        var provider = new FakeWmiProvider("Win32_Service", new[] { WindowsUpdateRow() });
        var results = await provider.ToSafeServiceMetricsAsync(CancellationToken.None);

        Assert.True(results[0].AcceptStop);
    }

    [Fact]
    public async Task FullData_Maps_AcceptPause_False()
    {
        var provider = new FakeWmiProvider("Win32_Service", new[] { WindowsUpdateRow() });
        var results = await provider.ToSafeServiceMetricsAsync(CancellationToken.None);

        Assert.False(results[0].AcceptPause);
    }

    [Fact]
    public async Task FullData_Maps_StartMode()
    {
        var provider = new FakeWmiProvider("Win32_Service", new[] { WindowsUpdateRow() });
        var results = await provider.ToSafeServiceMetricsAsync(CancellationToken.None);

        Assert.Equal("Automatic", results[0].StartMode);
    }

    [Fact]
    public async Task FullData_Maps_StartName()
    {
        var provider = new FakeWmiProvider("Win32_Service", new[] { WindowsUpdateRow() });
        var results = await provider.ToSafeServiceMetricsAsync(CancellationToken.None);

        Assert.Equal("LocalSystem", results[0].StartName);
    }

    [Fact]
    public async Task FullData_Maps_PathName()
    {
        var provider = new FakeWmiProvider("Win32_Service", new[] { WindowsUpdateRow() });
        var results = await provider.ToSafeServiceMetricsAsync(CancellationToken.None);

        Assert.Contains("svchost.exe", results[0].PathName);
    }

    [Fact]
    public async Task FullData_Maps_ProcessId_Uint()
    {
        var provider = new FakeWmiProvider("Win32_Service", new[] { WindowsUpdateRow() });
        var results = await provider.ToSafeServiceMetricsAsync(CancellationToken.None);

        Assert.Equal((uint)1240, results[0].ProcessId);
    }

    [Fact]
    public async Task FullData_Maps_ExitCode_Uint()
    {
        var provider = new FakeWmiProvider("Win32_Service", new[] { WindowsUpdateRow() });
        var results = await provider.ToSafeServiceMetricsAsync(CancellationToken.None);

        Assert.Equal((uint)0, results[0].ExitCode);
    }

    [Fact]
    public async Task FullData_Maps_StatusInfo_Ushort()
    {
        var provider = new FakeWmiProvider("Win32_Service", new[] { WindowsUpdateRow() });
        var results = await provider.ToSafeServiceMetricsAsync(CancellationToken.None);

        Assert.Equal((ushort)3, results[0].StatusInfo);
    }

    [Fact]
    public async Task FullData_Maps_InstallDate_DateTime()
    {
        var provider = new FakeWmiProvider("Win32_Service", new[] { WindowsUpdateRow() });
        var results = await provider.ToSafeServiceMetricsAsync(CancellationToken.None);

        Assert.Equal(new DateTime(2019, 11, 1, 0, 0, 0, DateTimeKind.Utc), results[0].InstallDate);
    }

    [Fact]
    public async Task MultipleServices_Returns_All()
    {
        var svc1 = WmiRow.Build(("Name", new WmiValue("svc1")), ("State", new WmiValue("Running")));
        var svc2 = WmiRow.Build(("Name", new WmiValue("svc2")), ("State", new WmiValue("Stopped")));
        var svc3 = WmiRow.Build(("Name", new WmiValue("svc3")), ("State", new WmiValue("Paused")));

        var provider = new FakeWmiProvider("Win32_Service", new[] { svc1, svc2, svc3 });
        var results = await provider.ToSafeServiceMetricsAsync(CancellationToken.None);

        Assert.Equal(3, results.Count);
        Assert.Equal("svc1", results[0].Name);
        Assert.Equal("svc2", results[1].Name);
        Assert.Equal("svc3", results[2].Name);
    }

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("Win32_Service", WmiRow.Empty());
        var results = await provider.ToSafeServiceMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MissingKeys_Return_Null_Fields()
    {
        var row = WmiRow.Build(
            ("Name", new WmiValue("minimal")),
            ("Started", new WmiValue(false)));
        var provider = new FakeWmiProvider("Win32_Service", new[] { row });
        var results = await provider.ToSafeServiceMetricsAsync(CancellationToken.None);

        Assert.Equal("minimal", results[0].Name);
        Assert.False(results[0].Started);
        Assert.Null(results[0].DisplayName);
        Assert.Null(results[0].State);
        Assert.Null(results[0].StartMode);
        Assert.Null(results[0].ProcessId);
    }

    [Fact]
    public async Task Stopped_Service_Maps_Started_False()
    {
        var row = WmiRow.Build(
            ("Name", new WmiValue("stoppedSvc")),
            ("State", new WmiValue("Stopped")),
            ("Started", new WmiValue(false)));
        var provider = new FakeWmiProvider("Win32_Service", new[] { row });
        var results = await provider.ToSafeServiceMetricsAsync(CancellationToken.None);

        Assert.False(results[0].Started);
        Assert.Equal("Stopped", results[0].State);
    }
}
