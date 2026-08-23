using System.Threading;
using System.Threading.Tasks;

namespace Crystal.Provider.Mmi.HardwareFeatures.FirmwareSecurity;

/// <summary>
/// Reads platform firmware-security state that is not exposed via WMI/SMBIOS —
/// currently the Secure Boot posture from the firmware state registry key.
/// </summary>
public interface IFirmwareSecurityProvider {
  /// <summary>
  /// Reads the Secure Boot posture; never throws (returns <see cref="SecureBootState.Unknown"/> on failure).
  /// </summary>
  /// <param name="cancellationToken">The cancellation token.</param>
  /// <returns>The Secure Boot posture.</returns>
  Task<SecureBootState> GetSecureBootStateAsync(CancellationToken cancellationToken);
}
