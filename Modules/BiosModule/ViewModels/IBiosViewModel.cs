using System.Collections.ObjectModel;
using System.Windows.Input;

namespace BiosModule.ViewModels;

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
  ObservableCollection<BoardSensorRowViewModel> BoardSensors { get; }
  bool HasBoardSensors { get; }

  // Shown in place of empty live readings when board sensors can't be read.
  string BoardSensorStatus { get; }
  bool HasBoardSensorStatus { get; }

  ICommand ShowDetailCommand { get; }
  ICommand ShowDashboardCommand { get; }
}
