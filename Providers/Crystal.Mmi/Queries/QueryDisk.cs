using Crystal.Mmi.Constants;
using Crystal.Mmi.Interfaces;
using Microsoft.Management.Infrastructure;
using static Crystal.Mmi.Constants.ErrorConstants;
using static Crystal.Mmi.Constants.DiskConstants;

namespace Crystal.Mmi.Queries; 
public class QueryDisk : IMmiQuery {
  private readonly CimSession _session;
  private readonly CimInstance _cimInstance;
  public Dictionary<string, string> InfoDictionary { get; } = [];

  public QueryDisk() {
    _session = CimSession.Create(MmiConstants.ComputerName);

    // NOTE: Win32_DiskDrive returns one instance per physical drive, so on any multi-drive
    // machine this only captures the first one WMI happens to enumerate. Same caveat as
    // QueryCpu/QueryMemory -- a real "list every drive" view needs the multi-instance path
    // (QueryMultiple).
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
