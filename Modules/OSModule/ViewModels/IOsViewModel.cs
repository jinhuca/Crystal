using System.Windows.Input;

namespace OSModule.ViewModels;

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
}
