using Crystal.Mmi.Constants;
using Crystal.Mmi.Interfaces;
using Microsoft.Management.Infrastructure;
using static Crystal.Mmi.Constants.ErrorConstants;
using static Crystal.Mmi.Constants.ServiceConstants;

namespace Crystal.Mmi.Queries; 
public class QueryService : IMmiQuery {
  private readonly CimSession _session;
  private readonly CimInstance _cimInstance;
  public Dictionary<string, string> InfoDictionary { get; } = [];

  public QueryService() {
    _session = CimSession.Create(MmiConstants.ComputerName);

    // NOTE: same caveat as QueryProcess -- Win32_Service is a list of every installed Windows
    // service, and FirstOrDefault() here just grabs whichever one WMI enumerates first. Not
    // useful on its own until a real multi-instance/filtered query is implemented.
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
