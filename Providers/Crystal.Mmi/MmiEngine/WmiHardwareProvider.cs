using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Generic;
using System.Collections.Concurrent;
using System.Collections.Frozen;

namespace Crystal.Mmi.MmiEngine;

public class WmiHardwareProvider : IWmiHardwareProvider {
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

  private static async Task<IReadOnlyList<FrozenDictionary<string, WmiValue>>> CreateMultiMetricsForClassAsync(
      string wmiClassName,
      CancellationToken cancellationToken) {
    var instancesList = new List<FrozenDictionary<string, WmiValue>>();

    try {
      using var session = CimSession.Create(null);
      var query = $"SELECT * FROM {wmiClassName}";

     // Fetch the native MMI asynchronous observable instance stream
      CimAsyncMultipleResults<CimInstance> cimObserver = session.QueryInstancesAsync(@"root\cimv2", "WQL", query);

      // Subscribe to the async streaming source while binding our CancellationToken
      await foreach (var instance in cimObserver.ToAsyncEnumerable(cancellationToken)) {
        var tempDict = new Dictionary<string, WmiValue>(StringComparer.Ordinal);

        foreach (var property in instance.CimInstanceProperties) {
          if (property.Value == null) continue;

          switch (property.Value) {
            case bool b: tempDict[property.Name] = new WmiValue(b); break;
            case string s: tempDict[property.Name] = new WmiValue(s); break;
            case string[] arr: tempDict[property.Name] = new WmiValue(arr); break;
            case int i: tempDict[property.Name] = new WmiValue(i); break;
            case uint ui: tempDict[property.Name] = new WmiValue((int)ui); break;
            case ushort us: tempDict[property.Name] = new WmiValue((int)us); break;
            case ulong ul: tempDict[property.Name] = new WmiValue(ul); break;
            case DateTime dt: tempDict[property.Name] = new WmiValue(dt); break;
            default: tempDict[property.Name] = new WmiValue(property.Value.ToString() ?? string.Empty); break;
          }
        }

        instancesList.Add(tempDict.ToFrozenDictionary());
      }
    }
    catch (OperationCanceledException) {
      // Explicitly pass cancellation up the stack
      throw;
    }
    catch (Exception ex) {
      Console.WriteLine($"[DEBUG] WMI Query failure for {wmiClassName}: {ex.Message}");
    }

    return instancesList.AsReadOnly();
  }
}
