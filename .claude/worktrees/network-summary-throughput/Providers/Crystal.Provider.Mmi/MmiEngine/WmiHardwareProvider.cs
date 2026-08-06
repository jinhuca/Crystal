using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Generic;
using Microsoft.Management.Infrastructure.Options;
using System.Collections.Concurrent;
using System.Collections.Frozen;

namespace Crystal.Provider.Mmi.MmiEngine;

public class WmiHardwareProvider : IWmiHardwareProvider {
  private const string DefaultNamespace = @"root\cimv2";

  // The cache now stores the executing Tasks to allow async re-use
  private readonly ConcurrentDictionary<string, Task<IReadOnlyList<FrozenDictionary<string, WmiValue>>>> _asyncCache = new(StringComparer.OrdinalIgnoreCase);

  public Task<IReadOnlyList<FrozenDictionary<string, WmiValue>>> GetMultiMetricsForClassAsync(
      string wmiClassName,
      CancellationToken cancellationToken) {
    // Check for cancellation before doing any dictionary lookups
    cancellationToken.ThrowIfCancellationRequested();

    // Get or add the asynchronous operation pipeline task
    return _asyncCache.GetOrAdd(wmiClassName, (key) => CreateMultiMetricsForClassAsync(key, cancellationToken));
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
        using var session = CimSession.Create(null);

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
      Console.WriteLine($"[DEBUG] WMI InvokeMethod failure for {wmiClassName}.{methodName}: {ex.Message}");
      return WmiMethodResult.Empty;
    }
  }

  private static Task<IReadOnlyList<FrozenDictionary<string, WmiValue>>> CreateMultiMetricsForClassAsync(
      string wmiClassName,
      CancellationToken cancellationToken)
      => RunQueryAsync(DefaultNamespace, $"SELECT * FROM {wmiClassName}", cancellationToken);

  private static async Task<IReadOnlyList<FrozenDictionary<string, WmiValue>>> RunQueryAsync(
      string namespaceName,
      string wqlQuery,
      CancellationToken cancellationToken) {
    var instancesList = new List<FrozenDictionary<string, WmiValue>>();

    try {
      using var session = CimSession.Create(null);

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
}
