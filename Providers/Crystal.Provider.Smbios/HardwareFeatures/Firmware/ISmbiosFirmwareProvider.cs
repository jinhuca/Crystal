using System.Threading;
using System.Threading.Tasks;

namespace Crystal.Provider.Smbios.HardwareFeatures.Firmware;

/// <summary>
/// Exposes the firmware-related structures decoded from the system SMBIOS table
/// (BIOS/UEFI, system/baseboard identity, hardware security, boot status, TPM
/// and the firmware inventory) as a single projected snapshot.
/// </summary>
public interface ISmbiosFirmwareProvider {
  /// <summary>Reads and projects the firmware-related SMBIOS structures.</summary>
  Task<SmbiosFirmwareInfo> GetFirmwareInfoAsync(CancellationToken cancellationToken);
}
