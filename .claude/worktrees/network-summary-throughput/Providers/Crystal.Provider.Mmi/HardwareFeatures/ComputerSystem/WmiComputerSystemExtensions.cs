using Crystal.Provider.Mmi.MmiEngine;

namespace Crystal.Provider.Mmi.HardwareFeatures.ComputerSystem;

public static class WmiComputerSystemExtensions {
  public static async Task<ComputerSystemMetrics> ToSafeComputerSystemMetricsAsync(
    this IWmiHardwareProvider provider,
    CancellationToken cancellationToken) {
    try {
      var instances = await provider.GetMultiMetricsForClassAsync(
        WmiComputerSystem.ClassName,
        cancellationToken);

      var first = instances.FirstOrDefault();

      if(first is null) {
        return new(null, null, null, null, null, null, null, null, null, null, null, null, null);
      }

      string? S(string name) => first.TryGetValue(name, out var v) && v.Type == WmiType.String
        ? v.AsString()
        : null;

      uint? UInt(string name) => first.TryGetValue(name, out var v) && v.Type == WmiType.Int
        ? (uint)v.AsInt()
        : null;

      ulong? ULong(string name) => first.TryGetValue(name, out var v) && v.Type == WmiType.ULong
        ? v.AsULong()
        : null;

      bool? Bool(string name) => first.TryGetValue(name, out var v) && v.Type == WmiType.Bool
        ? v.AsBool()
        : null;

      return new ComputerSystemMetrics(
        Name: S(WmiComputerSystem.Name),
        Manufacturer: S(WmiComputerSystem.Manufacturer),
        Model: S(WmiComputerSystem.Model),
        SystemType: S(WmiComputerSystem.SystemType),
        Domain: S(WmiComputerSystem.Domain),
        DNSHostName: S(WmiComputerSystem.DNSHostName),
        UserName: S(WmiComputerSystem.UserName),
        PrimaryOwnerName: S(WmiComputerSystem.PrimaryOwnerName),
        TotalPhysicalMemory: ULong(WmiComputerSystem.TotalPhysicalMemory),
        NumberOfProcessors: UInt(WmiComputerSystem.NumberOfProcessors),
        NumberOfLogicalProcessors: UInt(WmiComputerSystem.NumberOfLogicalProcessors),
        HypervisorPresent: Bool(WmiComputerSystem.HypervisorPresent),
        Status: S(WmiComputerSystem.Status));
    }
    catch(OperationCanceledException) {
      throw;
    }
    catch {
      return new(null, null, null, null, null, null, null, null, null, null, null, null, null);
    }
  }
}
