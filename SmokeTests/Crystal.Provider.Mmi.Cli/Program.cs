using System.Reflection;
using Crystal.Provider.Mmi.HardwareFeatures.AssociatedProcessorMemory;
using Crystal.Provider.Mmi.HardwareFeatures.BaseBoard;
using Crystal.Provider.Mmi.HardwareFeatures.Battery;
using Crystal.Provider.Mmi.HardwareFeatures.Bios;
using Crystal.Provider.Mmi.HardwareFeatures.Bus;
using Crystal.Provider.Mmi.HardwareFeatures.CurrentProbe;
using Crystal.Provider.Mmi.HardwareFeatures.DesktopMonitor;
using Crystal.Provider.Mmi.HardwareFeatures.DeviceBus;
using Crystal.Provider.Mmi.HardwareFeatures.DeviceSettings;
using Crystal.Provider.Mmi.HardwareFeatures.DiskDrive;
using Crystal.Provider.Mmi.HardwareFeatures.DiskPartition;
using Crystal.Provider.Mmi.HardwareFeatures.DisplayControllerConfiguration;
using Crystal.Provider.Mmi.HardwareFeatures.DMAChannel;
using Crystal.Provider.Mmi.HardwareFeatures.Fan;
using Crystal.Provider.Mmi.HardwareFeatures.FloppyController;
using Crystal.Provider.Mmi.HardwareFeatures.FloppyDrive;
using Crystal.Provider.Mmi.HardwareFeatures.HeatPipe;
using Crystal.Provider.Mmi.HardwareFeatures.IDEController;
using Crystal.Provider.Mmi.HardwareFeatures.IDEControllerDevice;
using Crystal.Provider.Mmi.HardwareFeatures.InfraredDevice;
using Crystal.Provider.Mmi.HardwareFeatures.Keyboard;
using Crystal.Provider.Mmi.HardwareFeatures.LogicalDisk;
using Crystal.Provider.Mmi.HardwareFeatures.MappedLogicalDisk;
using Crystal.Provider.Mmi.HardwareFeatures.MotherboardDevice;
using Crystal.Provider.Mmi.HardwareFeatures.NetworkAdapter;
using Crystal.Provider.Mmi.HardwareFeatures.NetworkAdapterConfiguration;
using Crystal.Provider.Mmi.HardwareFeatures.OnBoardDevice;
using Crystal.Provider.Mmi.HardwareFeatures.ParallelPort;
using Crystal.Provider.Mmi.HardwareFeatures.PCMCIAController;
using Crystal.Provider.Mmi.HardwareFeatures.PhysicalMedia;
using Crystal.Provider.Mmi.HardwareFeatures.PhysicalMemory;
using Crystal.Provider.Mmi.HardwareFeatures.PnPEntity;
using Crystal.Provider.Mmi.HardwareFeatures.PointingDevice;
using Crystal.Provider.Mmi.HardwareFeatures.PortableBattery;
using Crystal.Provider.Mmi.HardwareFeatures.PowerManagementEvent;
using Crystal.Provider.Mmi.HardwareFeatures.Processor;
using Crystal.Provider.Mmi.HardwareFeatures.Refrigeration;
using Crystal.Provider.Mmi.HardwareFeatures.SCSIController;
using Crystal.Provider.Mmi.HardwareFeatures.SCSIControllerDevice;
using Crystal.Provider.Mmi.HardwareFeatures.SerialPort;
using Crystal.Provider.Mmi.HardwareFeatures.SoundDevice;
using Crystal.Provider.Mmi.HardwareFeatures.SystemEnclosure;
using Crystal.Provider.Mmi.HardwareFeatures.TemperatureProbe;
using Crystal.Provider.Mmi.HardwareFeatures.USBController;
using Crystal.Provider.Mmi.HardwareFeatures.USBControllerDevice;
using Crystal.Provider.Mmi.HardwareFeatures.USBHub;
using Crystal.Provider.Mmi.HardwareFeatures.VideoController;
using Crystal.Provider.Mmi.HardwareFeatures.VideoSettings;
using Crystal.Provider.Mmi.HardwareFeatures.VoltageProbe;
using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Provider.Mmi.PerformanceFeatures.PerfCounter;
using Crystal.Provider.Mmi.PerformanceFeatures.PerfFormattedData;
using Crystal.Provider.Mmi.PerformanceFeatures.PerfRawData;
using Crystal.Provider.Mmi.SoftwareFeatures.COMClass;
using Crystal.Provider.Mmi.SoftwareFeatures.Desktop;
using Crystal.Provider.Mmi.SoftwareFeatures.Directory;
using Crystal.Provider.Mmi.SoftwareFeatures.Environment;
using Crystal.Provider.Mmi.SoftwareFeatures.Group;
using Crystal.Provider.Mmi.SoftwareFeatures.GroupUser;
using Crystal.Provider.Mmi.SoftwareFeatures.LogonSession;
using Crystal.Provider.Mmi.SoftwareFeatures.NetworkClient;
using Crystal.Provider.Mmi.SoftwareFeatures.NetworkConnection;
using Crystal.Provider.Mmi.SoftwareFeatures.NetworkLoginProfile;
using Crystal.Provider.Mmi.SoftwareFeatures.NetworkProtocol;
using Crystal.Provider.Mmi.SoftwareFeatures.OperatingSystem;
using Crystal.Provider.Mmi.SoftwareFeatures.OSRecoveryConfiguration;
using Crystal.Provider.Mmi.SoftwareFeatures.Process;
using Crystal.Provider.Mmi.SoftwareFeatures.QuickFixEngineering;
using Crystal.Provider.Mmi.SoftwareFeatures.Registry;
using Crystal.Provider.Mmi.SoftwareFeatures.Service;
using Crystal.Provider.Mmi.SoftwareFeatures.Share;
using Crystal.Provider.Mmi.SoftwareFeatures.StartupCommand;
using Crystal.Provider.Mmi.SoftwareFeatures.SystemDriver;
using Crystal.Provider.Mmi.SoftwareFeatures.Thread;
using Crystal.Provider.Mmi.SoftwareFeatures.TimeZone;
using Crystal.Provider.Mmi.SoftwareFeatures.UserAccount;
using Crystal.Provider.Mmi.SoftwareFeatures.UserDesktop;

namespace Crystal.Provider.Mmi.Cli;

public class Program {
  // Categories that can easily return hundreds of instances on a real machine.
  // Previewed by default; pass --full to dump every instance.
  private static readonly HashSet<string> HighVolumeSections = new(StringComparer.OrdinalIgnoreCase) {
    "Processes", "Threads", "Services", "Performance Counters", "Performance Raw Data", "Performance Formatted Data",
    "Plug and Play Devices", "Device-Bus Associations", "Device Settings Associations", "Video Settings Associations",
    "Network Login Profiles", "Environment Variables", "IDE Controller-Device Associations",
    "SCSI Controller-Device Associations", "USB Controller-Device Associations", "Associated Processor Memory",
    "DMA Channels", "User Desktop Associations", "Group Membership Associations", "Quick Fix Engineering (Hotfixes)",
    "COM Classes"
  };

  private const int PreviewCount = 15;

  public static async Task<int> Main(string[] args) {
    if (args.Contains("-h") || args.Contains("--help")) {
      PrintUsage();
      return 0;
    }

    if (!System.OperatingSystem.IsWindows()) {
      Console.WriteLine("Crystal.Provider.Mmi queries WMI via Microsoft.Management.Infrastructure and only runs on Windows.");
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
    await RunSingleAsync("OS Recovery Configuration", ct => provider.ToSafeOSRecoveryConfigurationMetricsAsync(ct), token);

    // --- Hardware inventories (zero or more instances) ---
    await RunListAsync("Physical Memory", ct => provider.ToSafePhysicalMemoryMetricsAsync(ct), showAll, token);
    await RunListAsync("Disk Drives", ct => provider.ToSafeDiskDriveMetricsAsync(ct), showAll, token);
    await RunListAsync("Disk Partitions", ct => provider.ToSafeDiskPartitionMetricsAsync(ct), showAll, token);
    await RunListAsync("Logical Disks", ct => provider.ToSafeLogicalDiskMetricsAsync(ct), showAll, token);
    await RunListAsync("Mapped Logical Disks", ct => provider.ToSafeMappedLogicalDiskMetricsAsync(ct), showAll, token);
    await RunListAsync("Physical Media", ct => provider.ToSafePhysicalMediaMetricsAsync(ct), showAll, token);
    await PrintDriveTopologyAsync(provider, token);
    await RunListAsync("Network Adapters", ct => provider.ToSafeNetworkAdapterMetricsAsync(ct), showAll, token);
    await RunListAsync("Network Adapter Configurations", ct => provider.ToSafeNetworkAdapterConfigMetricsAsync(ct), showAll, token);
    await RunListAsync("Network Clients", ct => provider.ToSafeNetworkClientMetricsAsync(ct), showAll, token);
    await RunListAsync("Network Connections", ct => provider.ToSafeNetworkConnectionMetricsAsync(ct), showAll, token);
    await RunListAsync("Network Protocols", ct => provider.ToSafeNetworkProtocolMetricsAsync(ct), showAll, token);
    await RunListAsync("Video Controllers", ct => provider.ToSafeVideoControllerMetricsAsync(ct), showAll, token);
    await RunListAsync("Display Controller Configurations", ct => provider.ToSafeDisplayControllerConfigurationMetricsAsync(ct), showAll, token);
    await RunListAsync("Desktop Monitors", ct => provider.ToSafeDesktopMonitorMetricsAsync(ct), showAll, token);
    await RunListAsync("Sound Devices", ct => provider.ToSafeSoundDeviceMetricsAsync(ct), showAll, token);
    await RunListAsync("USB Controllers", ct => provider.ToSafeUSBControllerMetricsAsync(ct), showAll, token);
    await RunListAsync("USB Hubs", ct => provider.ToSafeUSBHubMetricsAsync(ct), showAll, token);
    await RunListAsync("IDE Controllers", ct => provider.ToSafeIDEControllerMetricsAsync(ct), showAll, token);
    await RunListAsync("SCSI Controllers", ct => provider.ToSafeSCSIControllerMetricsAsync(ct), showAll, token);
    await RunListAsync("Floppy Controllers", ct => provider.ToSafeFloppyControllerMetricsAsync(ct), showAll, token);
    await RunListAsync("Floppy Drives", ct => provider.ToSafeFloppyDriveMetricsAsync(ct), showAll, token);
    await RunListAsync("PCMCIA Controllers", ct => provider.ToSafePCMCIAControllerMetricsAsync(ct), showAll, token);
    await RunListAsync("DMA Channels", ct => provider.ToSafeDMAChannelMetricsAsync(ct), showAll, token);
    await RunListAsync("OnBoard Devices", ct => provider.ToSafeOnBoardDeviceMetricsAsync(ct), showAll, token);
    await RunListAsync("System Enclosures", ct => provider.ToSafeSystemEnclosureMetricsAsync(ct), showAll, token);
    await RunListAsync("Motherboard Devices", ct => provider.ToSafeMotherboardDeviceMetricsAsync(ct), showAll, token);
    await RunListAsync("Buses", ct => provider.ToSafeBusMetricsAsync(ct), showAll, token);
    await RunListAsync("Fans", ct => provider.ToSafeFanMetricsAsync(ct), showAll, token);
    await RunListAsync("Heat Pipes", ct => provider.ToSafeHeatPipeMetricsAsync(ct), showAll, token);
    await RunListAsync("Refrigeration Devices", ct => provider.ToSafeRefrigerationMetricsAsync(ct), showAll, token);
    await RunListAsync("Temperature Probes", ct => provider.ToSafeTemperatureProbeMetricsAsync(ct), showAll, token);
    await RunListAsync("Current Probes", ct => provider.ToSafeCurrentProbeMetricsAsync(ct), showAll, token);
    await RunListAsync("Voltage Probes", ct => provider.ToSafeVoltageProbeMetricsAsync(ct), showAll, token);
    await RunListAsync("Portable Batteries", ct => provider.ToSafePortableBatteryMetricsAsync(ct), showAll, token);
    await RunListAsync("Power Management Events", ct => provider.ToSafePowerManagementEventMetricsAsync(ct), showAll, token);
    await RunListAsync("Infrared Devices", ct => provider.ToSafeInfraredDeviceMetricsAsync(ct), showAll, token);
    await RunListAsync("Keyboards", ct => provider.ToSafeKeyboardMetricsAsync(ct), showAll, token);
    await RunListAsync("Pointing Devices", ct => provider.ToSafePointingDeviceMetricsAsync(ct), showAll, token);
    await RunListAsync("Serial Ports", ct => provider.ToSafeSerialPortMetricsAsync(ct), showAll, token);
    await RunListAsync("Parallel Ports", ct => provider.ToSafeParallelPortMetricsAsync(ct), showAll, token);
    await RunListAsync("Plug and Play Devices", ct => provider.ToSafePnPEntityMetricsAsync(ct), showAll, token);
    await RunListAsync("Device-Bus Associations", ct => provider.ToSafeDeviceBusMetricsAsync(ct), showAll, token);
    await RunListAsync("Device Settings Associations", ct => provider.ToSafeDeviceSettingsMetricsAsync(ct), showAll, token);
    await RunListAsync("Video Settings Associations", ct => provider.ToSafeVideoSettingsMetricsAsync(ct), showAll, token);
    await RunListAsync("IDE Controller-Device Associations", ct => provider.ToSafeIDEControllerDeviceMetricsAsync(ct), showAll, token);
    await RunListAsync("SCSI Controller-Device Associations", ct => provider.ToSafeSCSIControllerDeviceMetricsAsync(ct), showAll, token);
    await RunListAsync("USB Controller-Device Associations", ct => provider.ToSafeUSBControllerDeviceMetricsAsync(ct), showAll, token);
    await RunListAsync("Associated Processor Memory", ct => provider.ToSafeAssociatedProcessorMemoryMetricsAsync(ct), showAll, token);

    // --- Software / runtime state ---
    await RunListAsync("Processes", ct => provider.ToSafeProcessMetricsAsync(ct), showAll, token);
    await RunListAsync("Threads", ct => provider.ToSafeThreadMetricsAsync(ct), showAll, token);
    await RunListAsync("Services", ct => provider.ToSafeServiceMetricsAsync(ct), showAll, token);
    await RunListAsync("User Accounts", ct => provider.ToSafeUserAccountMetricsAsync(ct), showAll, token);
    await RunListAsync("Groups", ct => provider.ToSafeGroupMetricsAsync(ct), showAll, token);
    await RunListAsync("Group Membership Associations", ct => provider.ToSafeGroupUserMetricsAsync(ct), showAll, token);
    await RunListAsync("Shares", ct => provider.ToSafeShareMetricsAsync(ct), showAll, token);
    await RunListAsync("Quick Fix Engineering (Hotfixes)", ct => provider.ToSafeQuickFixEngineeringMetricsAsync(ct), showAll, token);
    await RunListAsync("Logon Sessions", ct => provider.ToSafeLogonSessionMetricsAsync(ct), showAll, token);
    await RunListAsync("Network Login Profiles", ct => provider.ToSafeNetworkLoginProfileMetricsAsync(ct), showAll, token);
    await RunListAsync("Startup Commands", ct => provider.ToSafeStartupCommandMetricsAsync(ct), showAll, token);
    await RunListAsync("Environment Variables", ct => provider.ToSafeEnvironmentMetricsAsync(ct), showAll, token);
    await RunListAsync("System Drivers", ct => provider.ToSafeSystemDriverMetricsAsync(ct), showAll, token);
    await RunListAsync("Desktops", ct => provider.ToSafeDesktopMetricsAsync(ct), showAll, token);
    await RunListAsync("User Desktop Associations", ct => provider.ToSafeUserDesktopMetricsAsync(ct), showAll, token);
    await RunListAsync("Time Zones", ct => provider.ToSafeTimeZoneMetricsAsync(ct), showAll, token);
    await RunListAsync("Registry", ct => provider.ToSafeRegistryMetricsAsync(ct), showAll, token);
    // Directories are intentionally NOT enumerated here: Win32_Directory has no built-in scope,
    // so a bare SELECT * walks the entire file system. Callers should query
    // ToSafeDirectoryMetricsAsync with a provider that scopes the WQL WHERE clause
    // (e.g. WHERE Drive='C:' or a specific path) rather than enumerating everything.

    // --- Raw performance counter base classes ---
    await RunListAsync("Performance Counters", ct => provider.ToSafePerfCounterMetricsAsync(ct), showAll, token);
    await RunListAsync("Performance Raw Data", ct => provider.ToSafePerfRawDataMetricsAsync(ct), showAll, token);
    await RunListAsync("Performance Formatted Data", ct => provider.ToSafePerfFormattedDataMetricsAsync(ct), showAll, token);

    // --- WMI / COM registration ---
    await RunListAsync("COM Classes", ct => provider.ToSafeCOMClassMetricsAsync(ct), showAll, token);

    Console.WriteLine();
    Console.WriteLine(showAll
        ? "Done (--full: every instance shown)."
        : "Done. Pass --full to show every instance of high-volume categories instead of a preview.");
  }

  private static void PrintUsage() {
    Console.WriteLine("Crystal.Provider.Mmi.Cli - dumps every metric exposed by the Crystal.Provider.Mmi library.");
    Console.WriteLine();
    Console.WriteLine("Usage:");
    Console.WriteLine("  Crystal.Provider.Mmi.Cli [--full]");
    Console.WriteLine();
    Console.WriteLine("  --full   Show every instance of high-volume categories (processes, threads,");
    Console.WriteLine("           services, PnP devices, perf counters) instead of a 15-item preview.");
  }

  private static void PrintBanner() {
    Console.WriteLine(new string('=', 60));
    Console.WriteLine(" Crystal.Provider.Mmi CLI - System Information Snapshot");
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
