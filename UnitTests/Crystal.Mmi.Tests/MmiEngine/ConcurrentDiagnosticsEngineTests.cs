using System.Collections.Frozen;
using Crystal.Mmi.HardwareFeatures.Bios;
using Crystal.Mmi.HardwareFeatures.DiskDrive;
using Crystal.Mmi.HardwareFeatures.Processor;
using Crystal.Mmi.MmiEngine;
using Crystal.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Mmi.Tests.MmiEngine;

public class ConcurrentDiagnosticsEngineTests
{
    private static FakeWmiProvider MakeProvider()
    {
        var biosRow = WmiRow.Build(
            ("Manufacturer", new WmiValue("AMI")),
            ("Version", new WmiValue("1.0")));

        var cpuRow = WmiRow.Build(
            ("Name", new WmiValue("Intel Core i7")),
            ("NumberOfCores", new WmiValue(8)));

        var diskRow = WmiRow.Build(
            ("Model", new WmiValue("Samsung SSD")),
            ("Size", new WmiValue(500_000_000_000UL)));

        return new FakeWmiProvider(new Dictionary<string, IReadOnlyList<FrozenDictionary<string, WmiValue>>>
        {
            ["Win32_Bios"] = new[] { biosRow },
            ["Win32_Processor"] = new[] { cpuRow },
            ["Win32_DiskDrive"] = new[] { diskRow }
        });
    }

    [Fact]
    public async Task RunFullAuditParallelAsync_Returns_NonNull_SystemProfile()
    {
        var engine = new ConcurrentDiagnosticsEngine(MakeProvider());
        var profile = await engine.RunFullAuditParallelAsync(CancellationToken.None);

        Assert.NotNull(profile);
    }

    [Fact]
    public async Task RunFullAuditParallelAsync_Maps_Bios_Manufacturer()
    {
        var engine = new ConcurrentDiagnosticsEngine(MakeProvider());
        var profile = await engine.RunFullAuditParallelAsync(CancellationToken.None);

        Assert.Equal("AMI", profile.Bios.Manufacturer);
    }

    [Fact]
    public async Task RunFullAuditParallelAsync_Maps_Processor_Name()
    {
        var engine = new ConcurrentDiagnosticsEngine(MakeProvider());
        var profile = await engine.RunFullAuditParallelAsync(CancellationToken.None);

        Assert.Equal("Intel Core i7", profile.Processor.Name);
    }

    [Fact]
    public async Task RunFullAuditParallelAsync_Maps_Disks_Count()
    {
        var engine = new ConcurrentDiagnosticsEngine(MakeProvider());
        var profile = await engine.RunFullAuditParallelAsync(CancellationToken.None);

        Assert.Single(profile.Disks);
        Assert.Equal("Samsung SSD", profile.Disks[0].Model);
    }

    [Fact]
    public async Task RunFullAuditParallelAsync_Empty_Provider_Returns_Null_Fields()
    {
        // Bios and Processor return null-filled fallbacks; Disks returns empty list
        var emptyProvider = new FakeWmiProvider(new Dictionary<string, IReadOnlyList<FrozenDictionary<string, WmiValue>>>());
        var engine = new ConcurrentDiagnosticsEngine(emptyProvider);
        var profile = await engine.RunFullAuditParallelAsync(CancellationToken.None);

        Assert.NotNull(profile);
        Assert.Null(profile.Bios.Manufacturer);
        Assert.Null(profile.Processor.Name);
        Assert.Empty(profile.Disks);
    }

    [Fact]
    public async Task RunFullAuditParallelAsync_MultipleDisks_Returns_All()
    {
        var disk1 = WmiRow.Build(("Model", new WmiValue("SSD_1")));
        var disk2 = WmiRow.Build(("Model", new WmiValue("SSD_2")));
        var provider = new FakeWmiProvider(new Dictionary<string, IReadOnlyList<FrozenDictionary<string, WmiValue>>>
        {
            ["Win32_Bios"] = WmiRow.Single(),
            ["Win32_Processor"] = WmiRow.Single(),
            ["Win32_DiskDrive"] = new[] { disk1, disk2 }
        });

        var engine = new ConcurrentDiagnosticsEngine(provider);
        var profile = await engine.RunFullAuditParallelAsync(CancellationToken.None);

        Assert.Equal(2, profile.Disks.Count);
    }

    [Fact]
    public async Task RunFullAuditParallelAsync_Cancelled_Token_Throws_OperationCanceledException()
    {
        // Processor extension re-throws OperationCanceledException; Task.WhenAll propagates it
        var engine = new ConcurrentDiagnosticsEngine(MakeProvider());
        var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => engine.RunFullAuditParallelAsync(cts.Token));
    }

    [Fact]
    public void SystemProfile_Record_Equality_Works()
    {
        var bios = new BiosMetrics(null, null, null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null, null, null);
        var cpu = new ProcessorMetrics(
            null, null, null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null);
        var disks = Array.Empty<DiskDriveMetrics>();

        var p1 = new SystemProfile(bios, cpu, disks);
        var p2 = new SystemProfile(bios, cpu, disks);

        Assert.Equal(p1, p2);
    }
}
