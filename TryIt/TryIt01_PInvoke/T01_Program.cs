namespace TryIt01_PInvoke; 
internal class T01_Program {
  static void Main(string[] args) {
    var monitor = new CpuMemoryMonitor();

    while (true) {
      var sample = monitor.GetSample();

      Console.WriteLine(
          $"CPU={sample.CpuUsagePercent:F1}%  " +
          $"RAM={sample.MemoryUsagePercent:F1}%");

      Thread.Sleep(1000);
    }
  }
}
