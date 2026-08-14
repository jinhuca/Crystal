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
  private const string StateKeyPath = @"SYSTEM\CurrentControlSet\Control\SecureBoot\State";
  private const string EnabledValueName = "UEFISecureBootEnabled";

  public Task<SecureBootState> GetSecureBootStateAsync(CancellationToken cancellationToken) {
    cancellationToken.ThrowIfCancellationRequested();
    return Task.Run(ReadState, cancellationToken);
  }

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
