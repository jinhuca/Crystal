using Crystal.Controls.PerformanceGraphs;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Crystal.BiosModule.ViewModels;

/// <summary>
/// Root view model bound to the BIOS summary tile and detail view: firmware identity
/// fields and the two navigation commands the shell wires to.
/// </summary>
public interface IBiosViewModel {
  #region Core BIOS identity

  /// <summary>
  /// The BIOS vendor/manufacturer string, e.g. "American Megatrends Inc." or "Phoenix Technologies LTD".
  /// </summary>
  string Manufacturer { get; }

  /// <summary>
  /// The BIOS version string, e.g. "P1.20" or "2.15.1234".
  /// </summary>
  string Version { get; }

  /// <summary>
  /// The BIOS release date string, e.g. "12/31/2023" or "2023-12-31".
  /// </summary>
  string ReleaseDate { get; }

  /// <summary>
  /// The BIOS serial number string, e.g. "1234567890" or "ABCDEF123456".
  /// </summary>
  string SerialNumber { get; }

  /// <summary>
  /// The BIOS SMBIOS specification version string, e.g. "3.4" or "2.7".
  /// </summary>
  string SmbiosSpecVersion { get; }

  /// <summary>
  /// The BIOS status string, e.g. "OK" or "Degraded".
  /// </summary>
  string Status { get; }

  #endregion Core BIOS identity

  #region Firmware detail (SMBIOS)

  /// <summary>
  /// The BIOS ROM size string, e.g. "16 MB" or "32 MB".
  /// </summary>
  string RomSize { get; }

  /// <summary>
  /// The BIOS firmware interface string, e.g. "UEFI" or "Legacy".
  /// </summary>
  string FirmwareInterface { get; }

  /// <summary>
  /// The BIOS revision string, e.g. "1.0.0" or "2.1.3".
  /// </summary>
  string BiosRevision { get; }

  /// <summary>
  /// The embedded controller revision string, e.g. "1.0" or "2.5".
  /// </summary>
  string EmbeddedControllerRevision { get; }

  #endregion Firmware detail (SMBIOS)

  #region Capabilities

  /// <summary>
  /// The BIOS flash upgrade capability string, e.g. "Yes" or "No".
  /// </summary>
  string FlashUpgradeable { get; }

  /// <summary>
  /// The BIOS selectable boot capability string, e.g. "Yes" or "No".
  /// </summary>
  string SelectableBoot { get; }

  /// <summary>
  /// The BIOS boot from CD capability string, e.g. "Yes" or "No".
  /// </summary>
  string BootFromCd { get; }

  #endregion Capabilities

  #region System / baseboard / chassis identity

  /// <summary>
  /// The system manufacturer string, e.g. "Dell Inc." or "HP".
  /// </summary>
  string SystemManufacturer { get; }

  /// <summary>
  /// The system product name string, e.g. "XPS 15 9500" or "EliteBook 840 G7".
  /// </summary>
  string SystemProduct { get; }

  /// <summary>
  /// The system version string, e.g. "1.0" or "2.5".
  /// </summary>
  string BaseboardManufacturer { get; }

  /// <summary>
  /// The system serial number string, e.g. "1234567890" or "ABCDEF123456".
  /// </summary>
  string BaseboardProduct { get; }

  /// <summary>
  /// The system chassis type string, e.g. "Desktop" or "Laptop".
  /// </summary>
  string ChassisType { get; }

  #endregion System / baseboard / chassis identity

  #region Security / TPM / boot

  /// <summary>
  /// The BIOS secure boot capability string, e.g. "Enabled" or "Disabled".
  /// </summary>
  string SecureBoot { get; }

  /// <summary>
  /// The BIOS TPM capability string, e.g. "Enabled" or "Disabled".
  /// </summary>
  string Tpm { get; }

  /// <summary>
  /// The BIOS TPM manufacturer string, e.g. "Infineon" or "Intel".
  /// </summary>
  string TpmManufacturer { get; }

  /// <summary>
  /// The BIOS TPM version string, e.g. "2.0" or "1.2".
  /// </summary>
  string AdministratorPassword { get; }

  /// <summary>
  /// The BIOS power-on password capability string, e.g. "Enabled" or "Disabled".
  /// </summary>
  string PowerOnPassword { get; }

  /// <summary>
  /// The BIOS boot status string, e.g. "Normal" or "Recovery".
  /// </summary>
  string BootStatus { get; }

  #endregion Security / TPM / boot

  #region Firmware inventory (SMBIOS Type 45)

  /// <summary>
  /// The collection of firmware components reported by the BIOS, each with its own name, version, and status.
  /// </summary>
  ObservableCollection<FirmwareComponentViewModel> FirmwareInventory { get; }

  /// <summary>
  /// Indicates whether the firmware inventory is available (SMBIOS Type 45 is present).
  /// </summary>
  bool HasFirmwareInventory { get; }

  /// <summary>
  /// The count of firmware components in the inventory, or "N/A" if the inventory is not available.
  /// </summary>
  string FirmwareComponentCount { get; }

  #endregion Firmware inventory (SMBIOS Type 45)

  #region Live board telemetry (shared SensorMonitor, 1s cadence)

  /// <summary>
  /// Compact live board readings for the summary tile (1s cadence).
  /// </summary>
  string BoardTemperature { get; }

  /// <summary>
  /// Compact live board readings for the summary tile (1s cadence).
  /// </summary>
  string CmosVoltage { get; }

  /// <summary>
  /// Compact live board readings for the summary tile (1s cadence).
  /// </summary>
  string ChassisFanRpm { get; }

  /// <summary>
  /// Compact live board readings for the summary tile (1s cadence).
  /// </summary>
  string Rail3V3 { get; }

  /// <summary>
  /// Compact live board readings for the summary tile (1s cadence).
  /// </summary>
  string Rail5V { get; }

  /// <summary>
  /// Compact live board readings for the summary tile (1s cadence).
  /// </summary>
  string Rail12V { get; }

  /// <summary>
  /// Compact live board readings for the summary tile (1s cadence).
  /// </summary>
  string Rail3V3Range { get; }

  /// <summary>
  /// Compact live board readings for the summary tile (1s cadence).
  /// </summary>
  string Rail5VRange { get; }
  string Rail12VRange { get; }
  ReadingSeverity CmosSeverity { get; }
  ReadingSeverity Rail3V3Severity { get; }
  ReadingSeverity Rail5VSeverity { get; }
  ReadingSeverity Rail12VSeverity { get; }
  ReadingSeverity BoardHealth { get; }
  string BoardHealthDetail { get; }
  // Compact count of out-of-spec rows for the tile badge (e.g. "1 critical · 2 warnings"); empty when healthy.
  string BoardHealthSummary { get; }
  ReadingSeverity ChassisFanSeverity { get; }
  bool HasChassisFan { get; }
  ReadingSeverity BoardTemperatureSeverity { get; }
  ObservableCollection<BoardSensorRowViewModel> BoardSensors { get; }
  bool HasBoardSensors { get; }
  // False on laptop-class chassis, which have no ATX +3.3/+5/+12V rails or CMOS coin cell to plot;
  // the views hide those rail/CMOS rows accordingly.
  bool ShowVoltageRails { get; }

  // Session log of out-of-spec episodes (ongoing first), so a transient fault leaves a durable trail.
  ObservableCollection<BoardHealthEventViewModel> HealthEvents { get; }
  bool HasHealthEvents { get; }
  // When true, the event table is narrowed to critical episodes only; the count headline and tile
  // peak still reflect the full log. Two-way bound to the filter toggle above the table.
  bool ShowCriticalOnly { get; set; }
  // "N hidden" when the critical-only filter is suppressing rows; empty otherwise.
  string HealthEventsFilterHint { get; }
  // "+N older dropped" when the retention cap has evicted the oldest recovered episodes; empty otherwise.
  string HealthEventsCapNote { get; }
  // Compact count for the section header, e.g. "3 events · 1 ongoing"; empty when the log is empty.
  string HealthEventsSummary { get; }
  // The session's single worst episode for the tile, e.g. "+12V 10.4 V"; empty when nothing's logged.
  string SessionPeak { get; }
  ReadingSeverity SessionPeakSeverity { get; }
  // Tab-separated snapshot of the health-event log (header row + one row per episode) for pasting
  // into a bug report. Empty string when there are no events.
  string HealthEventsAsText();

  // Shown in place of empty live readings when board sensors can't be read.
  string BoardSensorStatus { get; }
  bool HasBoardSensorStatus { get; }

  #endregion Live board telemetry (shared SensorMonitor, 1s cadence)

  void AttachRailGraphs(PerformanceGraph rail3V3, PerformanceGraph rail5V, PerformanceGraph rail12V);
  void AttachFanGraph(PerformanceGraph fan);
  void AttachBoardTempGraph(PerformanceGraph boardTemp);
  void AttachCmosGraph(PerformanceGraph cmos);

  ICommand ShowDetailCommand { get; }
  ICommand ShowDashboardCommand { get; }

  // Resets the session health log and trend-graph buffers to start a fresh observation window.
  ICommand ClearHistoryCommand { get; }
}
