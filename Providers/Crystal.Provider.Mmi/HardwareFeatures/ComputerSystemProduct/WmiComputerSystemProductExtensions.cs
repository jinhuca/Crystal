using Crystal.Provider.Mmi.MmiEngine;

namespace Crystal.Provider.Mmi.HardwareFeatures.ComputerSystemProduct;

/// <summary>
/// Provides extension methods for <see cref="IWmiHardwareProvider"/> to read the WMI class
/// </summary>
public static class WmiComputerSystemProductExtensions {
  /// <summary>
  /// Reads the WMI class <c>Win32_ComputerSystemProduct</c> and returns a <see cref="ComputerSystemProductMetrics"/> 
  /// object with the metrics.
  /// </summary>
  /// <param name="provider">The WMI hardware provider.</param>
  /// <param name="cancellationToken">The cancellation token.</param>
  /// <returns>The computer system product metrics.</returns>
  public static async Task<ComputerSystemProductMetrics> ToSafeComputerSystemProductMetricsAsync(
    this IWmiHardwareProvider provider, 
    CancellationToken cancellationToken) {
    try {
      var instances = await provider.GetMultiMetricsForClassAsync(
        WmiComputerSystemProduct.ClassName,
        cancellationToken);

      var first = instances.FirstOrDefault();
      
      if(first is null) {
        return new(null, null, null, null, null, null, null, null);
      }

      string? S(string n) => first.TryGetValue(n, out var v) && v.Type == WmiType.String 
        ? v.AsString() : null;

      return new(
        Name: S(WmiComputerSystemProduct.Name),
        Vendor: S(WmiComputerSystemProduct.Vendor),
        Version: S(WmiComputerSystemProduct.Version),
        UUID: S(WmiComputerSystemProduct.UUID),
        IdentifyingNumber: S(WmiComputerSystemProduct.IdentifyingNumber),
        SKUNumber: S(WmiComputerSystemProduct.SKUNumber),
        Caption: S(WmiComputerSystemProduct.Caption),
        Description: S(WmiComputerSystemProduct.Description));
    }
    catch(OperationCanceledException) { 
      throw; 
    }
    catch { 
      return new(null, null, null, null, null, null, null, null); 
    }
  }
}
