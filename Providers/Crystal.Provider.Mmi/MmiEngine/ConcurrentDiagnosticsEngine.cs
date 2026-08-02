using Crystal.Provider.Mmi.HardwareFeatures.Bios;
using Crystal.Provider.Mmi.HardwareFeatures.DiskDrive;
using Crystal.Provider.Mmi.HardwareFeatures.Processor;

namespace Crystal.Provider.Mmi.MmiEngine;

// An immutable container to hold the completely parsed system profile snapshot
public record SystemProfile(
    BiosMetrics Bios,
    ProcessorMetrics Processor,
    IReadOnlyList<DiskDriveMetrics> Disks
);

public class ConcurrentDiagnosticsEngine {
  private readonly IWmiHardwareProvider _wmiProvider;

  public ConcurrentDiagnosticsEngine(IWmiHardwareProvider wmiProvider) {
    _wmiProvider = wmiProvider;
  }

  public async Task<SystemProfile> RunFullAuditParallelAsync(CancellationToken cancellationToken) {
    // 1. Fire off tasks concurrently, feeding them the shared Token
    Task<BiosMetrics> biosTask = _wmiProvider.ToSafeBiosMetricsAsync(cancellationToken);
    Task<ProcessorMetrics> cpuTask = _wmiProvider.ToSafeProcessorMetricsAsync(cancellationToken);
    Task<IReadOnlyList<DiskDriveMetrics>> diskTask = _wmiProvider.ToSafeDiskDriveMetricsAsync(cancellationToken);

    // 2. Cohesively await evaluation
    await Task.WhenAll(biosTask, cpuTask, diskTask);

    return new SystemProfile(biosTask.Result, cpuTask.Result, diskTask.Result);
  }
}

