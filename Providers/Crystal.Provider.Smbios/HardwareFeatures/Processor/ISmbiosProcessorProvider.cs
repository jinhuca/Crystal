using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Crystal.Provider.Smbios.HardwareFeatures.Processor;

/// <summary>
/// Exposes the populated processor sockets decoded from the system SMBIOS table.
/// </summary>
public interface ISmbiosProcessorProvider {
  /// <summary>
  /// Returns one entry per populated socket (SMBIOS Type 4 with the "socket
  /// populated" status bit set), correlated with its Type 7 cache structures.
  /// </summary>
  Task<IReadOnlyList<SmbiosProcessorInfo>> GetAllProcessorsAsync(CancellationToken cancellationToken);
}
