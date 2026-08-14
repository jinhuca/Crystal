using Crystal.Provider.Mmi.HardwareFeatures.FirmwareSecurity;
using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Provider.Smbios.HardwareFeatures.Firmware;
using System.Collections.Frozen;

namespace Crystal.Service.Bios.Tests;

/// <summary>
/// Minimal <see cref="IWmiHardwareProvider"/> stub. The Bios service only touches the provider
/// through <c>ToSafeBiosMetricsAsync</c> (the namespace-less <c>GetMultiMetricsForClassAsync</c>
/// overload) and <c>ToSafeTpmMetricsAsync</c> (the namespaced overload), so those are the only two
/// members that need real behavior. Each returns the property bag it was seeded with; an empty bag
/// exercises the extensions' "no instance" fallback.
/// </summary>
internal sealed class FakeWmiProvider : IWmiHardwareProvider {
  private readonly FrozenDictionary<string, WmiValue> _bios;
  private readonly FrozenDictionary<string, WmiValue> _tpm;

  public FakeWmiProvider(
      IReadOnlyDictionary<string, WmiValue>? bios = null,
      IReadOnlyDictionary<string, WmiValue>? tpm = null) {
    _bios = (bios ?? new Dictionary<string, WmiValue>()).ToFrozenDictionary();
    _tpm = (tpm ?? new Dictionary<string, WmiValue>()).ToFrozenDictionary();
  }

  // Namespace-less overload — Win32_BIOS.
  public Task<IReadOnlyList<FrozenDictionary<string, WmiValue>>> GetMultiMetricsForClassAsync(
      string wmiClassName, CancellationToken cancellationToken,
      bool bypassCache = false, IReadOnlyList<string>? projection = null) =>
      Task.FromResult(Rows(_bios));

  // Namespaced overload — Win32_Tpm.
  public Task<IReadOnlyList<FrozenDictionary<string, WmiValue>>> GetMultiMetricsForClassAsync(
      string namespaceName, string wmiClassName, CancellationToken cancellationToken) =>
      Task.FromResult(Rows(_tpm));

  public Task<IReadOnlyList<FrozenDictionary<string, WmiValue>>> QueryAsync(
      string namespaceName, string wqlQuery, CancellationToken cancellationToken) =>
      Task.FromResult((IReadOnlyList<FrozenDictionary<string, WmiValue>>)[]);

  public Task<WmiMethodResult> InvokeStaticMethodAsync(
      string namespaceName, string wmiClassName, string methodName,
      IReadOnlyDictionary<string, WmiValue> inParameters, CancellationToken cancellationToken) =>
      Task.FromResult(WmiMethodResult.Empty);

  private static IReadOnlyList<FrozenDictionary<string, WmiValue>> Rows(
      FrozenDictionary<string, WmiValue> bag) =>
      bag.Count == 0 ? [] : [bag];
}

/// <summary>Returns a fixed SMBIOS snapshot, or throws to exercise graceful degradation.</summary>
internal sealed class FakeSmbiosProvider : ISmbiosFirmwareProvider {
  private readonly SmbiosFirmwareInfo? _info;
  private readonly bool _throw;

  public FakeSmbiosProvider(SmbiosFirmwareInfo? info) { _info = info; }
  private FakeSmbiosProvider(bool doThrow) { _throw = doThrow; }

  public static FakeSmbiosProvider Throwing() => new(doThrow: true);

  public Task<SmbiosFirmwareInfo> GetFirmwareInfoAsync(CancellationToken cancellationToken) {
    if (_throw) throw new InvalidOperationException("smbios read failed");
    return Task.FromResult(_info!);
  }
}

/// <summary>Returns a fixed Secure Boot state, or throws to exercise graceful degradation.</summary>
internal sealed class FakeSecurityProvider : IFirmwareSecurityProvider {
  private readonly SecureBootState _state;
  private readonly bool _throw;

  public FakeSecurityProvider(SecureBootState state) { _state = state; }
  private FakeSecurityProvider(bool doThrow) { _throw = doThrow; _state = SecureBootState.Unknown; }

  public static FakeSecurityProvider Throwing() => new(doThrow: true);

  public Task<SecureBootState> GetSecureBootStateAsync(CancellationToken cancellationToken) {
    if (_throw) throw new InvalidOperationException("secure boot read failed");
    return Task.FromResult(_state);
  }
}
