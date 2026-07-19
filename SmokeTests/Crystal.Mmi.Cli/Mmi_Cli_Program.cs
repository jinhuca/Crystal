using System;
using System.Linq;
using Microsoft.Management.Infrastructure;

class Program {
  static void Main() {
    //Test1();
    //var result = GetOsInfo();
    TestMmiProvider();
  }

  private static void Test0() {
    try {
      // Create a local session (null targets the local machine)
      using (CimSession session = CimSession.Create("localhost")) {
        string namespaceName = @"root\cimv2";
        string wqlQuery = "SELECT * FROM Win32_OperatingSystem";

        // Execute the query
        var osInstances = session.QueryInstances(namespaceName, "WQL", wqlQuery);

        // Get the first matching instance
        CimInstance osInfo = osInstances.FirstOrDefault();

        if (osInfo != null) {
          // Extract properties using their strongly-typed names
          string osName = osInfo.CimInstanceProperties["Caption"].Value?.ToString();
          string version = osInfo.CimInstanceProperties["Version"].Value?.ToString();
          string architecture = osInfo.CimInstanceProperties["OSArchitecture"].Value?.ToString();
          string installDate = osInfo.CimInstanceProperties["InstallDate"].Value?.ToString();

          Console.WriteLine($"Operating System: {osName}");
          Console.WriteLine($"Version: {version}");
          Console.WriteLine($"Architecture: {architecture}");
          Console.WriteLine($"Installation Date: {installDate}");
        }
        else {
          Console.WriteLine("No operating system information found.");
        }
      }
    }
    catch (Exception ex) {
      Console.WriteLine($"An error occurred: {ex.Message}");
    }
  }

  private static void Test1() {
    using CimSession session = CimSession.Create(null); // null = local machine

    foreach (CimInstance instance in session.QueryInstances(
        @"root\cimv2", "WQL", "SELECT * FROM Win32_OperatingSystem")) {
      using (instance) {
        foreach (CimProperty property in instance.CimInstanceProperties)
          Console.WriteLine($"{property.Name} = {property.Value ?? "(null)"}");
      }
    }
  }

  private static Dictionary<string, string> GetOsInfo() {
    var osInfo = new Dictionary<string, string>();
    using (CimSession session = CimSession.Create("localhost")) {
      string namespaceName = @"root\cimv2";
      string wqlQuery = "SELECT * FROM Win32_OperatingSystem";
      var osInstances = session.QueryInstances(namespaceName, "WQL", wqlQuery);
      CimInstance osInstance = osInstances.FirstOrDefault();
      if (osInstance != null) {
        foreach (var property in osInstance.CimInstanceProperties) {
          osInfo[property.Name] = property.Value?.ToString() ?? string.Empty;
        }
      }
    }
    return osInfo;
  }

  private static void TestMmiProvider() {
    using var queryOs = new Crystal.Mmi.Queries.QueryOs();
    var info = queryOs.InfoDictionary;
    foreach (var kvp in info) {
      Console.WriteLine($"{kvp.Key}: {kvp.Value}");
    }
  }
}
