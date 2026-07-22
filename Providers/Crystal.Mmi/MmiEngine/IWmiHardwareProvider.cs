using System.Collections.Frozen;

namespace Crystal.Mmi.MmiEngine;

public interface IWmiHardwareProvider {
  // Asynchronous signatures accepting a cancellation token parameter
  public Task<IReadOnlyList<FrozenDictionary<string, WmiValue>>> GetMultiMetricsForClassAsync(
      string wmiClassName,
      CancellationToken cancellationToken);
}
