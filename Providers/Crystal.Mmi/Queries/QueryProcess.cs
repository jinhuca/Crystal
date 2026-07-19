using Crystal.Mmi.Constants;
using Crystal.Mmi.Interfaces;
using Microsoft.Management.Infrastructure;
using static Crystal.Mmi.Constants.ErrorConstants;
using static Crystal.Mmi.Constants.ProcessConstants;

namespace Crystal.Mmi.Queries; 
public class QueryProcess : IMmiQuery {
  private readonly CimSession _session;
  private readonly CimInstance _cimInstance;
  public Dictionary<string, string> InfoDictionary { get; } = [];

  public QueryProcess() {
    _session = CimSession.Create(MmiConstants.ComputerName);

    // NOTE: unlike Bios/Os (genuinely singular) or Cpu/Memory/Disk/Network (multiple, but
    // "first" is at least a real physical component), Win32_Process is fundamentally a *list*
    // -- every running process on the system. Grabbing FirstOrDefault() here just returns
    // whatever process WMI happens to enumerate first (frequently PID 0, the System Idle
    // Process), which isn't meaningful info on its own. This class exists to match the
    // requested "one query class per MmiCategory" shape, but it isn't useful until
    // QueryMultiple (or an equivalent list-returning query) is implemented -- don't wire this
    // one into anything that expects real process data yet.
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
