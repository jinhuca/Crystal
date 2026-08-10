using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Generic;
using Microsoft.Management.Infrastructure.Options;
using System.Collections.Concurrent;
using System.Collections.Frozen;

namespace Crystal.Provider.Mmi.MmiEngine;

public class WmiHardwareProvider : IWmiHardwareProvider, IDisposable {
  private const string DefaultNamespace = @"root\cimv2";

  // The cache now stores the executing Tasks to allow async re-use
  private readonly ConcurrentDictionary<string, Task<IReadOnlyList<FrozenDictionary<string, WmiValue>>>> _asyncCache = new(StringComparer.OrdinalIgnoreCase);

  // One CimSession reused across every query. CimSession.Create opens a DCOM/WMI connection whose
  // handshake dwarfs the query itself, so creating one per call — as the volatile process poll does
  // every second — is the dominant WMI cost. CimSession is thread-safe for concurrent queries, so a
  // single shared instance is safe. Recreated lazily if a query faults it (see RunQueryAsync).
  private readonly object _sessionGate = new();
  private CimSession? _session;
  private bool _disposed;

  private CimSession GetSession() {
    lock (_sessionGate) {
      ObjectDisposedException.ThrowIf(_disposed, this);
      return _session ??= CimSession.Create(null);
    }
  }

  // Drop the shared session so the next call rebuilds it. Called when a query throws, since a
  // faulted session would otherwise fail every subsequent poll for the app's lifetime.
  private void ResetSession() {
    lock (_sessionGate) {
      _session?.Dispose();
      _session = null;
    }
  }

  public Task<IReadOnlyList<FrozenDictionary<string, WmiValue>>> GetMultiMetricsForClassAsync(
      string wmiClassName,
      CancellationToken cancellationToken,
      bool bypassCache = false,
      IReadOnlyList<string>? projection = null) {
    // Check for cancellation before doing any dictionary lookups
    cancellationToken.ThrowIfCancellationRequested();

    // Volatile classes (Win32_Process) change every poll, so a cached first result would freeze the
    // data — and any value derived from a per-poll delta (per-process CPU%) would read a constant
    // zero. Re-query live and never seed the cache for these.
    if (bypassCache) {
      return CreateMultiMetricsForClassAsync(wmiClassName, cancellationToken, projection);
    }

    // The cache is keyed by class name alone, so a projected (partial-column) result must never be
    // cached — a later full-column caller would silently get the trimmed bag. Projection is only
    // honored on the bypass path above; ignore it here.
    return _asyncCache.GetOrAdd(wmiClassName, (key) => CreateMultiMetricsForClassAsync(key, cancellationToken, projection: null));
  }

  public Task<IReadOnlyList<FrozenDictionary<string, WmiValue>>> GetMultiMetricsForClassAsync(
      string namespaceName,
      string wmiClassName,
      CancellationToken cancellationToken) {
    cancellationToken.ThrowIfCancellationRequested();

    var cacheKey = $"{namespaceName}::{wmiClassName}";
    return _asyncCache.GetOrAdd(cacheKey, _ =>
        RunQueryAsync(namespaceName, $"SELECT * FROM {wmiClassName}", cancellationToken));
  }

  public Task<IReadOnlyList<FrozenDictionary<string, WmiValue>>> QueryAsync(
      string namespaceName,
      string wqlQuery,
      CancellationToken cancellationToken) {
    cancellationToken.ThrowIfCancellationRequested();

    // Arbitrary queries (associators/filters) are not cached because their
    // result set depends on the specific target instance and filters.
    return RunQueryAsync(namespaceName, wqlQuery, cancellationToken);
  }

  public async Task<WmiMethodResult> InvokeStaticMethodAsync(
      string namespaceName,
      string wmiClassName,
      string methodName,
      IReadOnlyDictionary<string, WmiValue> inParameters,
      CancellationToken cancellationToken) {
    cancellationToken.ThrowIfCancellationRequested();

    try {
      return await Task.Run(() => {
        // Reuse the shared session (see GetSession) instead of opening one per invocation.
        var session = GetSession();

        using var methodParameters = new CimMethodParametersCollection();
        foreach (var (name, value) in inParameters) {
          methodParameters.Add(CimMethodParameter.Create(name, ToNativeValue(value), CimFlags.In));
        }

        cancellationToken.ThrowIfCancellationRequested();

        using CimMethodResult result =
            session.InvokeMethod(namespaceName, wmiClassName, methodName, methodParameters);

        WmiValue? returnValue = result.ReturnValue?.Value is { } rv
            ? ConvertNative(rv)
            : null;

        var outputs = new Dictionary<string, WmiValue>(StringComparer.Ordinal);
        foreach (var outParam in result.OutParameters) {
          if (outParam.Value == null) continue;
          outputs[outParam.Name] = ConvertNative(outParam.Value);
        }

        return new WmiMethodResult(returnValue, outputs.ToFrozenDictionary());
      }, cancellationToken).ConfigureAwait(false);
    }
    catch (OperationCanceledException) {
      throw;
    }
    catch (Exception ex) {
      // Drop a possibly-faulted session so the next call rebuilds it (see RunQueryAsync).
      ResetSession();
      Console.WriteLine($"[DEBUG] WMI InvokeMethod failure for {wmiClassName}.{methodName}: {ex.Message}");
      return WmiMethodResult.Empty;
    }
  }

  private Task<IReadOnlyList<FrozenDictionary<string, WmiValue>>> CreateMultiMetricsForClassAsync(
      string wmiClassName,
      CancellationToken cancellationToken,
      IReadOnlyList<string>? projection = null) {
    // Selecting only the consumed columns (vs *) cuts WMI marshaling and per-instance allocation.
    // Property names come from trusted nameof constants, not user input, so string-joining them into
    // the SELECT list carries no injection risk.
    var columns = projection is { Count: > 0 } ? string.Join(", ", projection) : "*";
    return RunQueryAsync(DefaultNamespace, $"SELECT {columns} FROM {wmiClassName}", cancellationToken);
  }

  private async Task<IReadOnlyList<FrozenDictionary<string, WmiValue>>> RunQueryAsync(
      string namespaceName,
      string wqlQuery,
      CancellationToken cancellationToken) {
    var instancesList = new List<FrozenDictionary<string, WmiValue>>();

    try {
      // Reuse the shared session instead of creating one per query (see GetSession).
      var session = GetSession();

      // Fetch the native MMI asynchronous observable instance stream
      CimAsyncMultipleResults<CimInstance> cimObserver = session.QueryInstancesAsync(namespaceName, "WQL", wqlQuery);

      // Subscribe to the async streaming source while binding our CancellationToken
      await foreach (var instance in cimObserver.ToAsyncEnumerable(cancellationToken)) {
        using (instance) {
          instancesList.Add(WmiValueConverter.ToWmiValues(instance));
        }
      }
    }
    catch (OperationCanceledException) {
      // Explicitly pass cancellation up the stack
      throw;
    }
    catch (Exception ex) {
      // A faulted session (e.g. WMI service restart, broken connection) would fail every subsequent
      // poll for the app's lifetime, so drop it — the next call rebuilds a fresh one.
      ResetSession();
      Console.WriteLine($"[DEBUG] WMI Query failure for '{wqlQuery}' in {namespaceName}: {ex.Message}");
    }

    return instancesList.AsReadOnly();
  }

  private static object? ToNativeValue(WmiValue value) => value.Type switch {
    WmiType.Bool => value.AsBool(),
    WmiType.Int => value.AsInt(),
    WmiType.String => value.AsString(),
    WmiType.StringArray => value.AsStringArray(),
    WmiType.DateTime => value.AsDateTime(),
    WmiType.UShortArray => value.AsUShortArray(),
    WmiType.ULong => value.AsULong(),
    _ => null
  };

  private static WmiValue ConvertNative(object value) => value switch {
    bool b => new WmiValue(b),
    string s => new WmiValue(s),
    string[] arr => new WmiValue(arr),
    int i => new WmiValue(i),
    uint ui => new WmiValue((int)ui),
    ushort us => new WmiValue((int)us),
    ulong ul => new WmiValue(ul),
    DateTime dt => new WmiValue(dt),
    _ => new WmiValue(value.ToString() ?? string.Empty)
  };

  public void Dispose() {
    lock (_sessionGate) {
      if (_disposed) return;
      _disposed = true;
      _session?.Dispose();
      _session = null;
    }
    GC.SuppressFinalize(this);
  }
}
