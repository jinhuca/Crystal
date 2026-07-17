using Crystal.Information.Cpu.Interfaces;

namespace Crystal.Information.Cpu.Implementations;

public class OSLiveInfo : IOSLiveInfo {
  public int ProcessNum { get; set; }
  public int ThreadsNum { get; set; }
  public int HandlesNum { get; set; }
  public TimeSpan UpTime { get; set; }
}
