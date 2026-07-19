namespace Crystal.Mmi.Interfaces; 
public interface IMmiQuery : IDisposable {
  public string Id { get; }
  public Dictionary<string, string> InfoDictionary { get; }

  public Dictionary<string, string> GetInfo();

  public Dictionary<string, (string, string)> Query(string query);
  //public Dictionary<string, Dictionary<string, (string, string)>> QueryMultiple(string query);
}
