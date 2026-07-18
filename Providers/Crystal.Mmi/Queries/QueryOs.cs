using Crystal.Mmi.Constants;
using Crystal.Mmi.Interfaces;
using Microsoft.Management.Infrastructure;
using static Crystal.Mmi.Constants.ErrorConstants;
using static Crystal.Mmi.Constants.OsConstants;

namespace Crystal.Mmi.Queries; 
public class QueryOs : IMmiQuery {
  private readonly CimSession _session;
  private readonly CimInstance _cimInstance;
  private static Dictionary<string, (string, string)> info = [];
  public Dictionary<string, string> InfoDictionary { get; } = [];
  public static string[]? MULLanguages;
  public static int? NumberOfProcesses;

  public QueryOs() {
    _session = CimSession.Create(MmiConstants.ComputerName);

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

  public Dictionary<string, Dictionary<string, (string, string)>> QueryMultiple(string query) {
    //using CimSession session = CimSession.Create(MmiConstants.ComputerName);
    //var osInstances = session.QueryInstances(MmiConstants.SessionNamespace, MmiConstants.QueryDialect, query);
    //var result = new Dictionary<string, Dictionary<string, (string, string)>>();
    //foreach (var instance in osInstances) {
    //  var properties = new Dictionary<string, (string, string)>();
    //  foreach (var property in instance.CimInstanceProperties) {
    //    properties[property.Name] = (property.Value?.ToString() ?? string.Empty, property.CimType.ToString());
    //  }
    //  result[instance.CimSystemProperties.InstanceId] = properties;
    //}
    //return result;
    throw new NotImplementedException();
  }

  public void Dispose() {
    _cimInstance?.Dispose();
    _session?.Dispose();
  }
}
