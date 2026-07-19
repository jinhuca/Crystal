namespace Crystal.Mmi.Interfaces; 
public interface IMmiQuery : IDisposable {
  public Dictionary<string, (string, string)> Query(string query);
  //public Dictionary<string, Dictionary<string, (string, string)>> QueryMultiple(string query);
}
