using Microsoft.Win32;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Crystal.Provider.Mmi.HardwareFeatures.FirmwareSecurity;

/// <summary>
/// Reads Secure Boot state from
/// <c>HKLM\SYSTEM\CurrentControlSet\Control\SecureBoot\State</c>. The
/// <c>UEFISecureBootEnabled</c> DWORD is 1 when Secure Boot is on, 0 when off;
/// the key is absent entirely on legacy/BIOS boots (Secure Boot unsupported).
/// </summary>
public sealed class FirmwareSecurityProvider : IFirmwareSecurityProvider {
  /// <summary>
  /// The registry key path that contains the Secure Boot state. This key is located at
  /// <c>HKLM\SYSTEM\CurrentControlSet\Control\SecureBoot\State</c>.
  /// </summary>
  private const string StateKeyPath = @"SYSTEM\CurrentControlSet\Control\SecureBoot\State";

  /// <summary>
  /// The registry value name that indicates whether Secure Boot is enabled. This value is a DWORD
  /// </summary>
  private const string EnabledValueName = "UEFISecureBootEnabled";

  /// <summary>
  /// Gets the current Secure Boot state asynchronously. This method reads the Secure Boot state 
  /// from the registry and returns a <see cref="SecureBootState"/> object that indicates whether 
  /// Secure Boot is supported and enabled.
  /// </summary>
  /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
  /// <returns>A task that represents the asynchronous operation and returns the Secure Boot state.</returns>
  public Task<SecureBootState> GetSecureBootStateAsync(CancellationToken cancellationToken) {
    cancellationToken.ThrowIfCancellationRequested();
    return Task.Run(ReadState, cancellationToken);
  }

  /// <summary>
  /// Reads the Secure Boot state from the registry. If the registry key is not found, it indicates that 
  /// Secure Boot is not supported. If the key is found, it checks the value of <c>UEFISecureBootEnabled</c>
  /// to determine if Secure Boot is enabled or disabled.
  /// </summary>
  /// <returns>The Secure Boot state.</returns>
  private static SecureBootState ReadState() {
    if (!OperatingSystem.IsWindows()) return SecureBootState.Unknown;
    try {
      using var key = Registry.LocalMachine.OpenSubKey(StateKeyPath);
      if (key is null) {
        // No SecureBoot state key → legacy/BIOS boot, Secure Boot not supported.
        return new SecureBootState(Supported: false, Enabled: null);
      }

      return key.GetValue(EnabledValueName) is int raw
          ? new SecureBootState(Supported: true, Enabled: raw != 0)
          : new SecureBootState(Supported: true, Enabled: null);
    } catch {
      return SecureBootState.Unknown;
    }
  }
}
