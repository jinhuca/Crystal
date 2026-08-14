using Crystal.Controls.PerformanceGraphs;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Crystal.BiosModule.ViewModels;

/// <summary>Root view model bound to the BIOS summary tile and detail view: firmware identity
/// fields and the two navigation commands the shell wires to.</summary>
public interface IBiosViewModel {
  // Core BIOS identity
  string Manufacturer { get; }
  string Version { get; }
  string ReleaseDate { get; }
  string SerialNumber { get; }
  string SmbiosSpecVersion { get; }
  string Status { get; }

  // Firmware detail (SMBIOS)
  string RomSize { get; }
  string FirmwareInterface { get; }
  string BiosRevision { get; }
  string EmbeddedControllerRevision { get; }

  // Capabilities
  string FlashUpgradeable { get; }
  string SelectableBoot { get; }
  string BootFromCd { get; }

  // System / baseboard / chassis identity
  string SystemManufacturer { get; }
  string SystemProduct { get; }
  string BaseboardManufacturer { get; }
  string BaseboardProduct { get; }
  string ChassisType { get; }

  // Security / TPM / boot
  string SecureBoot { get; }
  string Tpm { get; }
  string TpmManufacturer { get; }
  string AdministratorPassword { get; }
  string PowerOnPassword { get; }
  string BootStatus { get; }

  // Firmware inventory (SMBIOS Type 45)
  ObservableCollection<FirmwareComponentViewModel> FirmwareInventory { get; }
  bool HasFirmwareInventory { get; }
  string FirmwareComponentCount { get; }

  // Live board telemetry (shared SensorMonitor, 1s cadence)
  string BoardTemperature { get; }
  string CmosVoltage { get; }
  string ChassisFanRpm { get; }
  string Rail3V3 { get; }
  string Rail5V { get; }
  string Rail12V { get; }
  string Rail3V3Range { get; }
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

  void AttachRailGraphs(PerformanceGraph rail3V3, PerformanceGraph rail5V, PerformanceGraph rail12V);
  void AttachFanGraph(PerformanceGraph fan);
  void AttachBoardTempGraph(PerformanceGraph boardTemp);

  ICommand ShowDetailCommand { get; }
  ICommand ShowDashboardCommand { get; }

  // Resets the session health log and trend-graph buffers to start a fresh observation window.
  ICommand ClearHistoryCommand { get; }
}
