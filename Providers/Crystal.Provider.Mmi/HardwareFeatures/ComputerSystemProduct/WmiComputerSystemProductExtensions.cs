using Crystal.Provider.Mmi.MmiEngine;

namespace Crystal.Provider.Mmi.HardwareFeatures.ComputerSystemProduct;

public static class WmiComputerSystemProductExtensions {
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
