using ProcessModule.Models;

namespace ProcessModule.ViewModels;

/// <summary>One row in the process list, keyed by PID. Metrics refresh in place each poll so the
/// bound row (and any selection) survives instead of being torn down and rebuilt.</summary>
public sealed class ProcessRowViewModel : BindableBase {
  private double _cpuPercent;
  private double _workingSetMb;
  private double? _gpuPercent;
  private double? _diskBytesPerSec;
  private double? _netBytesPerSec;

  public ProcessRowViewModel(uint processId, string name) {
    ProcessId = processId;
    Name = name;
  }

  public uint ProcessId { get; }
  public string Name { get; }

  public double CpuPercent { get => _cpuPercent; set => SetProperty(ref _cpuPercent, value); }
  public double WorkingSetMb { get => _workingSetMb; set => SetProperty(ref _workingSetMb, value); }
  public double? GpuPercent { get => _gpuPercent; set => SetProperty(ref _gpuPercent, value); }
  public double? DiskBytesPerSec { get => _diskBytesPerSec; set => SetProperty(ref _diskBytesPerSec, value); }
  public double? NetBytesPerSec { get => _netBytesPerSec; set => SetProperty(ref _netBytesPerSec, value); }

  public void Update(ProcessSample s) {
    CpuPercent = s.CpuPercent;
    WorkingSetMb = s.WorkingSetMb;
    GpuPercent = s.GpuPercent;
    DiskBytesPerSec = s.DiskBytesPerSec;
    NetBytesPerSec = s.NetBytesPerSec;
  }
}
