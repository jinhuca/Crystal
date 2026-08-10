using System.Windows.Media;
using Crystal.Service.Process;

namespace ProcessModule.ViewModels;

/// <summary>One row in the process list, keyed by PID. Metrics refresh in place each poll so the
/// bound row (and any selection) survives instead of being torn down and rebuilt.</summary>
public sealed class ProcessRowViewModel : BindableBase {
  private double _cpuPercent;
  private double _workingSetMb;
  private double _peakCpuPercent;
  private double _peakWorkingSetMb;
  private ProcessCategory _category;
  private string? _status;
  private double? _gpuPercent;
  private double? _diskBytesPerSec;
  private double? _netBytesPerSec;
  private string? _executablePath;
  private ImageSource? _iconSource;

  public ProcessRowViewModel(uint processId, string name) {
    ProcessId = processId;
    Name = name;
  }

  public uint ProcessId { get; }
  public string Name { get; }

  /// <summary>Full path to the process image on disk, or null for processes WMI can't read the path
  /// of. Drives icon resolution and is refreshed each poll (it can arrive late for a process that
  /// was briefly unreadable at creation).</summary>
  public string? ExecutablePath { get => _executablePath; set => SetProperty(ref _executablePath, value); }

  /// <summary>The process's shell icon (Task Manager-style), or null when unresolved — the view
  /// shows a generic placeholder then. Frozen, so it binds safely from any thread.</summary>
  public ImageSource? IconSource { get => _iconSource; set => SetProperty(ref _iconSource, value); }

  public double CpuPercent { get => _cpuPercent; set => SetProperty(ref _cpuPercent, value); }
  public double WorkingSetMb { get => _workingSetMb; set => SetProperty(ref _workingSetMb, value); }

  /// <summary>Peak CPU% at or above which a process is flagged as a sustained hog (the cell tints).</summary>
  public const double SustainedCpuHogThreshold = 50;

  /// <summary>Peak working-set (MB) at or above which a process is flagged as a memory hog.</summary>
  public const double MemoryHogThresholdMb = 1024;

  /// <summary>Highest CPU% this process has reached while the list has been observing it. Never
  /// decreases; a running high-water mark that outlives the current-poll dip back down.</summary>
  public double PeakCpuPercent {
    get => _peakCpuPercent;
    private set { if (SetProperty(ref _peakCpuPercent, value)) RaisePropertyChanged(nameof(IsSustainedCpuHog)); }
  }

  /// <summary>Highest working-set (MB) this process has reached while observed. Never decreases.</summary>
  public double PeakWorkingSetMb {
    get => _peakWorkingSetMb;
    private set { if (SetProperty(ref _peakWorkingSetMb, value)) RaisePropertyChanged(nameof(IsMemoryHog)); }
  }

  /// <summary>True once this process's peak CPU crossed the sustained-hog threshold — a cue that it
  /// spiked hard at some point this session even if it's idle now. Drives the CPU-peak cell tint.</summary>
  public bool IsSustainedCpuHog => _peakCpuPercent >= SustainedCpuHogThreshold;

  /// <summary>True once this process's peak working-set crossed the memory-hog threshold. Drives the
  /// memory-peak cell tint.</summary>
  public bool IsMemoryHog => _peakWorkingSetMb >= MemoryHogThresholdMb;
  public ProcessCategory Category {
    get => _category;
    set { if (SetProperty(ref _category, value)) RaisePropertyChanged(nameof(CategoryName)); }
  }
  public string CategoryName => _category.ToDisplayName();
  public string? Status { get => _status; set => SetProperty(ref _status, value); }
  public double? GpuPercent { get => _gpuPercent; set => SetProperty(ref _gpuPercent, value); }
  public double? DiskBytesPerSec { get => _diskBytesPerSec; set => SetProperty(ref _diskBytesPerSec, value); }
  public double? NetBytesPerSec { get => _netBytesPerSec; set => SetProperty(ref _netBytesPerSec, value); }

  /// <summary>Collapses both session peaks back to the current live reading, starting a fresh
  /// high-water window. The next <see cref="Update"/> re-establishes them from live values.</summary>
  public void ResetPeaks() {
    PeakCpuPercent = _cpuPercent;
    PeakWorkingSetMb = _workingSetMb;
  }

  public void Update(ProcessSample s) {
    CpuPercent = s.CpuPercent;
    WorkingSetMb = s.WorkingSetMb;
    if (s.CpuPercent > _peakCpuPercent) PeakCpuPercent = s.CpuPercent;
    if (s.WorkingSetMb > _peakWorkingSetMb) PeakWorkingSetMb = s.WorkingSetMb;
    Category = s.Category;
    Status = s.Status;
    GpuPercent = s.GpuPercent;
    DiskBytesPerSec = s.DiskBytesPerSec;
    NetBytesPerSec = s.NetBytesPerSec;
    ExecutablePath = s.ExecutablePath;
  }
}
