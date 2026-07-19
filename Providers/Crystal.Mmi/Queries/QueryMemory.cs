using Crystal.Mmi.Constants;
using Crystal.Mmi.Interfaces;
using Microsoft.Management.Infrastructure;
using static Crystal.Mmi.Constants.ErrorConstants;
using static Crystal.Mmi.Constants.MemoryConstants;

namespace Crystal.Mmi.Queries; 
public class QueryMemory : IMmiQuery {
  private readonly CimSession _session;
  private readonly CimInstance _cimInstance;
  public Dictionary<string, string> InfoDictionary { get; } = [];

  public QueryMemory() {
    _session = CimSession.Create(MmiConstants.ComputerName);

    // NOTE: Win32_PhysicalMemory returns one instance per installed RAM stick, so on any
    // multi-stick machine this only captures the first one WMI happens to enumerate. Same
    // caveat as QueryCpu -- fine as a single-instance snapshot, but a real "list every stick"
    // view needs the multi-instance path (QueryMultiple).
    _cimInstance = _session
      .QueryInstances(MmiConstants.SessionNamespace, MmiConstants.QueryDialect, QueryString)
      .FirstOrDefault()
      ?? throw new ArgumentNullException(nameof(_cimInstance), WmiArgumentNullException);
  }

  public Dictionary<string, (string, string)> Query(string query) {
    throw new NotImplementedException();
  }

  public Dictionary<string, string> GetInfoDictionary() {
    InfoDictionary.Clear();

    foreach (var property in _cimInstance.CimInstanceProperties) {
      InfoDictionary[property.Name] = property.Value?.ToString() ?? string.Empty;
    }

    return InfoDictionary;
  }

  public void Dispose() {
    _cimInstance?.Dispose();
    _session?.Dispose();
  }
}
