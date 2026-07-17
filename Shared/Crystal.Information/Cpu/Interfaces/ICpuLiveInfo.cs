namespace Crystal.Information.Cpu.Interfaces;

public interface ICpuLiveInfo {
  IOSLiveInfo OsLiveInfo { get; set; }
  ICpuOverallLiveInfo CpuOverallLiveInfo { get; set; }
  List<ICpuCoreLiveInfo> CpuCoreLiveInfo { get; set; }
}
