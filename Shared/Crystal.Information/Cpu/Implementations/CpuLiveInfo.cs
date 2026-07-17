using Crystal.Information.Cpu.Interfaces;

namespace Crystal.Information.Cpu.Implementations;

public class CpuLiveInfo : ICpuLiveInfo {
  public ICpuOverallLiveInfo CpuOverallLiveInfo { get; set; } = new CpuOverallLiveInfo();
  public List<ICpuCoreLiveInfo> CpuCoreLiveInfo { get; set; } = new List<ICpuCoreLiveInfo>();
  public IOSLiveInfo OsLiveInfo { get; set; } = new OSLiveInfo();
}
