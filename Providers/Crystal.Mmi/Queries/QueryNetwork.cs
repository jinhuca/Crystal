using Crystal.Mmi.Constants;
using Crystal.Mmi.Interfaces;
using Microsoft.Management.Infrastructure;
using static Crystal.Mmi.Constants.ErrorConstants;
using static Crystal.Mmi.Constants.NetworkConstants;

namespace Crystal.Mmi.Queries; 
public class QueryNetwork : IMmiQuery {
  private readonly CimSession _session;
  private readonly CimInstance _cimInstance;
  public Dictionary<string, string> InfoDictionary { get; } = [];

  public QueryNetwork() {
    _session = CimSession.Create(MmiConstants.ComputerName);

    // NOTE: Win32_NetworkAdapter returns one instance per adapter (physical and virtual), so
    // this only captures the first one WMI happens to enumerate -- often not the adapter
    // that's actually connected. A real "list every adapter" / "find the active one" view
    // needs the multi-instance path (QueryMultiple), most likely filtered on NetEnabled/
    // NetConnectionStatus. IP/DNS/gateway details live on the separate
    // Win32_NetworkAdapterConfiguration class, not this one, if that's needed later.
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
