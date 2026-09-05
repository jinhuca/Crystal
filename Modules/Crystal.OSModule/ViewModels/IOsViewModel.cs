using System.Windows.Input;

namespace Crystal.OSModule.ViewModels;

/// <summary>Root view model bound to the OS summary tile and detail view: the essential identity
/// fields for the tile, the fuller set the detail view lists, the live uptime/clock readouts, and
/// the two navigation commands the shell wires to.</summary>
public interface IOsViewModel {
  // --- Essential identity (summary tile) ---
  /// <summary>Marketing name, e.g. "Windows 11 Pro".</summary>
  string OsName { get; }
  /// <summary>"22631.4169" build (with UBR suffix when available).</summary>
  string BuildLabel { get; }
  /// <summary>Feature-update label, e.g. "23H2".</summary>
  string DisplayVersion { get; }
  string Architecture { get; }

  // --- Live readouts ---
  /// <summary>"3d 21:22:12" uptime since last boot.</summary>
  string UptimeLabel { get; }
  /// <summary>Current wall-clock time, "yyyy-MM-dd HH:mm:ss".</summary>
  string CurrentTimeLabel { get; }

  // --- System-wide process totals (Processes sub-tile) ---
  /// <summary>Live process count across the system.</summary>
  string ProcessCountLabel { get; }
  /// <summary>Live summed thread count across every process.</summary>
  string ThreadCountLabel { get; }
  /// <summary>Live summed handle count across every process.</summary>
  string HandleCountLabel { get; }

  // --- Fuller identity (detail view) ---
  string Edition { get; }
  string VersionLabel { get; }
  string MachineName { get; }
  string UserName { get; }
  string RegisteredOwner { get; }
  string RegisteredOrganization { get; }
  string SystemDirectory { get; }
  string Locale { get; }
  string TimeZone { get; }
  string InstallDateLabel { get; }
  string LastBootTimeLabel { get; }

  ICommand ShowDetailCommand { get; }
  ICommand ShowDashboardCommand { get; }
  /// <summary>Opens the full process list in a detail window (Processes sub-tile double-click).</summary>
  ICommand ShowProcessesCommand { get; }
}
