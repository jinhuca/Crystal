using System.Reflection;
using Crystal.Mmi.HardwareFeatures.BaseBoard;
using Crystal.Mmi.HardwareFeatures.Battery;
using Crystal.Mmi.HardwareFeatures.Bios;
using Crystal.Mmi.HardwareFeatures.DesktopMonitor;
using Crystal.Mmi.HardwareFeatures.DiskDrive;
using Crystal.Mmi.HardwareFeatures.DiskPartition;
using Crystal.Mmi.HardwareFeatures.Fan;
using Crystal.Mmi.HardwareFeatures.HeatPipe;
using Crystal.Mmi.HardwareFeatures.LogicalDisk;
using Crystal.Mmi.HardwareFeatures.NetworkAdapter;
using Crystal.Mmi.HardwareFeatures.NetworkAdapterConfiguration;
using Crystal.Mmi.HardwareFeatures.ParallelPort;
using Crystal.Mmi.HardwareFeatures.PhysicalMemory;
using Crystal.Mmi.HardwareFeatures.PnPEntity;
using Crystal.Mmi.HardwareFeatures.Processor;
using Crystal.Mmi.HardwareFeatures.SerialPort;
using Crystal.Mmi.HardwareFeatures.SoundDevice;
using Crystal.Mmi.HardwareFeatures.SystemEnclosure;
using Crystal.Mmi.HardwareFeatures.USBController;
using Crystal.Mmi.HardwareFeatures.VideoController;
using Crystal.Mmi.MmiEngine;
using Crystal.Mmi.PerformanceFeatures.PerfCounter;
using Crystal.Mmi.PerformanceFeatures.PerfRawData;
using Crystal.Mmi.SoftwareFeatures.OperatingSystem;
using Crystal.Mmi.SoftwareFeatures.Process;
using Crystal.Mmi.SoftwareFeatures.Service;
using Crystal.Mmi.SoftwareFeatures.Thread;

namespace Crystal.Mmi.Cli;

public class Program {
  // Categories that can easily return hundreds of instances on a real machine.
  // Previewed by default; pass --full to dump every instance.
  private static readonly HashSet<string> HighVolumeSections = new(StringComparer.OrdinalIgnoreCase) {
    "Processes", "Threads", "Services", "Performance Counters", "Performance Raw Data", "Plug and Play Devices"
  };

  private const int PreviewCount = 15;

  public static async Task<int> Main(string[] args) {
    if (args.Contains("-h") || args.Contains("--help")) {
      PrintUsage();
      return 0;
    }

    if (!System.OperatingSystem.IsWindows()) {
      Console.WriteLine("Crystal.Mmi queries WMI via Microsoft.Management.Infrastructure and only runs on Windows.");
      return 1;
    }

    bool showAll = args.Contains("--full", StringComparer.OrdinalIgnoreCase);

    using var cts = new CancellationTokenSource();
    Console.CancelKeyPress += (_, e) => {
      e.Cancel = true;
      cts.Cancel();
    };

    try {
      await RunAsync(showAll, cts.Token);
      return 0;
    }
    catch (OperationCanceledException) {
      Console.WriteLine();
      Console.WriteLine("Cancelled.");
      return 1;
    }
  }

  private static async Task RunAsync(bool showAll, CancellationToken token) {
    IWmiHardwareProvider provider = new WmiHardwareProvider();

    PrintBanner();

    // --- Single-instance system information ---
    await RunSingleAsync("BIOS", ct => provider.ToSafeBiosMetricsAsync(ct), token);
    await RunSingleAsync("Base Board", ct => provider.ToSafeBaseBoardMetricsAsync(ct), token);
    await RunSingleAsync("Processor", ct => provider.ToSafeProcessorMetricsAsync(ct), token);
    await RunSingleAsync("Operating System", ct => provider.ToSafeOperatingSystemMetricsAsync(ct), token);
    await RunSingleAsync("Battery", ct => provider.ToSafeBatteryMetricsAsync(ct), token);

    // --- Hardware inventories (zero or more instances) ---
    await RunListAsync("Physical Memory", ct => provider.ToSafePhysicalMemoryMetricsAsync(ct), showAll, token);
    await RunListAsync("Disk Drives", ct => provider.ToSafeDiskDriveMetricsAsync(ct), showAll, token);
    await RunListAsync("Disk Partitions", ct => provider.ToSafeDiskPartitionMetricsAsync(ct), showAll, token);
    await RunListAsync("Logical Disks", ct => provider.ToSafeLogicalDiskMetricsAsync(ct), showAll, token);
    await PrintDriveTopologyAsync(provider, token);
    await RunListAsync("Network Adapters", ct => provider.ToSafeNetworkAdapterMetricsAsync(ct), showAll, token);
    await RunListAsync("Network Adapter Configurations", ct => provider.ToSafeNetworkAdapterConfigMetricsAsync(ct), showAll, token);
    await RunListAsync("Video Controllers", ct => provider.ToSafeVideoControllerMetricsAsync(ct), showAll, token);
    await RunListAsync("Desktop Monitors", ct => provider.ToSafeDesktopMonitorMetricsAsync(ct), showAll, token);
    await RunListAsync("Sound Devices", ct => provider.ToSafeSoundDeviceMetricsAsync(ct), showAll, token);
    await RunListAsync("USB Controllers", ct => provider.ToSafeUSBControllerMetricsAsync(ct), showAll, token);
    await RunListAsync("System Enclosures", ct => provider.ToSafeSystemEnclosureMetricsAsync(ct), showAll, token);
    await RunListAsync("Fans", ct => provider.ToSafeFanMetricsAsync(ct), showAll, token);
    await RunListAsync("Heat Pipes", ct => provider.ToSafeHeatPipeMetricsAsync(ct), showAll, token);
    await RunListAsync("Serial Ports", ct => provider.ToSafeSerialPortMetricsAsync(ct), showAll, token);
    await RunListAsync("Parallel Ports", ct => provider.ToSafeParallelPortMetricsAsync(ct), showAll, token);
    await RunListAsync("Plug and Play Devices", ct => provider.ToSafePnPEntityMetricsAsync(ct), showAll, token);

    // --- Software / runtime state ---
    await RunListAsync("Processes", ct => provider.ToSafeProcessMetricsAsync(ct), showAll, token);
    await RunListAsync("Threads", ct => provider.ToSafeThreadMetricsAsync(ct), showAll, token);
    await RunListAsync("Services", ct => provider.ToSafeServiceMetricsAsync(ct), showAll, token);

    // --- Raw performance counter base classes ---
    await RunListAsync("Performance Counters", ct => provider.ToSafePerfCounterMetricsAsync(ct), showAll, token);
    await RunListAsync("Performance Raw Data", ct => provider.ToSafePerfRawDataMetricsAsync(ct), showAll, token);

    Console.WriteLine();
    Console.WriteLine(showAll
        ? "Done (--full: every instance shown)."
        : "Done. Pass --full to show every instance of high-volume categories instead of a preview.");
  }

  private static void PrintUsage() {
    Console.WriteLine("Crystal.Mmi.Cli - dumps every metric exposed by the Crystal.Mmi library.");
    Console.WriteLine();
    Console.WriteLine("Usage:");
    Console.WriteLine("  Crystal.Mmi.Cli [--full]");
    Console.WriteLine();
    Console.WriteLine("  --full   Show every instance of high-volume categories (processes, threads,");
    Console.WriteLine("           services, PnP devices, perf counters) instead of a 15-item preview.");
  }

  private static void PrintBanner() {
    Console.WriteLine(new string('=', 60));
    Console.WriteLine(" Crystal.Mmi CLI - System Information Snapshot");
    Console.WriteLine($" Captured: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
    Console.WriteLine(new string('=', 60));
  }

  private static void PrintHeader(string title) {
    Console.WriteLine();
    Console.WriteLine($"-- {title} --");
  }

  private static async Task RunSingleAsync<T>(
      string title,
      Func<CancellationToken, Task<T>> fetch,
      CancellationToken token) {
    PrintHeader(title);
    try {
      var result = await fetch(token);
      PrintObject(result, "  ");
    }
    catch (OperationCanceledException) {
      throw;
    }
    catch (Exception ex) {
      Console.WriteLine($"  Unable to read {title}: {ex.Message}");
    }
  }

  private static async Task RunListAsync<T>(
      string title,
      Func<CancellationToken, Task<IReadOnlyList<T>>> fetch,
      bool showAll,
      CancellationToken token) {
    PrintHeader(title);
    try {
      var items = await fetch(token);
      Console.WriteLine($"  {items.Count} instance(s) found");

      if (items.Count == 0) return;

      bool isPreviewed = !showAll && HighVolumeSections.Contains(title) && items.Count > PreviewCount;
      int limit = isPreviewed ? PreviewCount : items.Count;

      for (int i = 0; i < limit; i++) {
        Console.WriteLine($"  --- [{i + 1}/{items.Count}] ---");
        PrintObject(items[i], "    ");
      }

      if (isPreviewed) {
        Console.WriteLine($"  ... and {items.Count - limit} more (pass --full to show all)");
      }
    }
    catch (OperationCanceledException) {
      throw;
    }
    catch (Exception ex) {
      Console.WriteLine($"  Unable to read {title}: {ex.Message}");
    }
  }

  // ToResolvedDriveTopologyAsync stitches DiskDrive + DiskPartition + the WMI association
  // classes together into a physical-drive -> partition -> volume-letter tree.
  private static async Task PrintDriveTopologyAsync(IWmiHardwareProvider provider, CancellationToken token) {
    PrintHeader("Disk Topology (Drive -> Partition -> Volume)");
    try {
      var drives = await provider.ToResolvedDriveTopologyAsync(token);
      if (drives.Count == 0) {
        Console.WriteLine("  (no drives found)");
        return;
      }

      foreach (var drive in drives) {
        var d = drive.DriveInfo;
        Console.WriteLine($"  {d.Model ?? d.Caption ?? d.DeviceID ?? "Unknown drive"}  [{FormatBytes(d.Size)}]");

        if (drive.Partitions.Count == 0) {
          Console.WriteLine("    (no partitions)");
          continue;
        }

        foreach (var partition in drive.Partitions) {
          var p = partition.PartitionInfo;
          var letters = partition.VolumeLetters.Count > 0
              ? string.Join(", ", partition.VolumeLetters)
              : "no drive letter assigned";
          Console.WriteLine($"    {p.Name ?? p.DeviceID ?? "Unknown partition"}  [{FormatBytes(p.Size)}]  -> {letters}");
        }
      }
    }
    catch (OperationCanceledException) {
      throw;
    }
    catch (Exception ex) {
      Console.WriteLine($"  Unable to resolve drive topology: {ex.Message}");
    }
  }

  // Reflection-based printer: every public property a metrics record exposes is shown
  // automatically, so newly added WMI fields on any record surface here with no CLI changes.
  private static void PrintObject(object? instance, string indent) {
    if (instance is null) {
      Console.WriteLine($"{indent}(unavailable)");
      return;
    }

    var properties = instance.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Where(p => p.GetIndexParameters().Length == 0);

    foreach (var property in properties) {
      object? value;
      try {
        value = property.GetValue(instance);
      }
      catch (TargetInvocationException) {
        value = null;
      }

      Console.WriteLine($"{indent}{property.Name,-38}: {FormatValue(value)}");
    }
  }

  private static string FormatValue(object? value) {
    switch (value) {
      case null:
        return "-";
      case string s:
        return string.IsNullOrWhiteSpace(s) ? "-" : s;
      case DateTime dt:
        return dt == default ? "-" : dt.ToString("u");
      case System.Collections.IEnumerable enumerable:
        var items = enumerable.Cast<object?>().Select(o => o?.ToString() ?? "").ToArray();
        return items.Length == 0 ? "-" : string.Join(", ", items);
      default:
        return value.ToString() ?? "-";
    }
  }

  private static string FormatBytes(ulong? bytes) {
    if (bytes is null or 0) return "size unknown";
    double gb = bytes.Value / 1024.0 / 1024.0 / 1024.0;
    return gb >= 1 ? $"{gb:F1} GB" : $"{bytes.Value / 1024.0:F0} KB";
  }
}
