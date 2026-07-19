using Crystal.Mmi.Constants;
using Crystal.Mmi.Interfaces;
using Microsoft.Management.Infrastructure;
using static Crystal.Mmi.Constants.ErrorConstants;
using static Crystal.Mmi.Constants.EventLogConstants;

namespace Crystal.Mmi.Queries; 
public class QueryEventLog : IMmiQuery {
  private readonly CimSession _session;
  private readonly CimInstance _cimInstance;
  public Dictionary<string, string> InfoDictionary { get; } = [];

  public QueryEventLog() {
    _session = CimSession.Create(MmiConstants.ComputerName);

    // NOTE: this targets Win32_NTEventLogFile (one instance per log -- Application, System,
    // Security, Setup, etc.), not Win32_NTLogEvent (one instance per individual log entry,
    // which would be many thousands of rows and isn't practical to snapshot this way). Like
    // Process/Service/User/Group, this is still a list underneath -- FirstOrDefault() here
    // grabs whichever log WMI enumerates first (commonly "Application"), not a specific one by
    // name. If you need a specific log (e.g. "System"), that needs a WHERE clause via the
    // multi-instance path, not this single-instance shape.
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
