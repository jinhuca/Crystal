using ThreadState = System.Diagnostics.ThreadState;
using Crystal.ProcessModule.Models;
using System.Diagnostics;

namespace Crystal.ProcessModule.ViewModels;

/// <summary>
/// One thread row beneath an expanded process in the list. Keyed by <see cref="ThreadId"/> and
/// refreshed in place each poll (the list VM reconciles by TID), so the row survives and just updates
/// its live CPU time / state / priority instead of being torn down and rebuilt each second.
/// </summary>
public sealed class ThreadRowViewModel : BindableBase {
  private TimeSpan _totalProcessorTime;
  private ThreadState _state;
  private ThreadWaitReason? _waitReason;
  private int _basePriority;
  private int _currentPriority;

  public ThreadRowViewModel(int threadId, DateTime? startTime) {
    ThreadId = threadId;
    StartTime = startTime;
  }

  public int ThreadId { get; }

  /// <summary>When the thread started, or null when the OS wouldn't report it (protected/exited). Set
  /// once at creation — a thread's start time doesn't change.</summary>
  public DateTime? StartTime { get; }

  /// <summary>Cumulative user+kernel CPU time; drives <see cref="CpuTimeLabel"/>.</summary>
  public TimeSpan TotalProcessorTime {
    get => _totalProcessorTime;
    set { if (SetProperty(ref _totalProcessorTime, value)) RaisePropertyChanged(nameof(CpuTimeLabel)); }
  }

  /// <summary>The thread's scheduling state; drives <see cref="StateLabel"/>.</summary>
  public ThreadState State {
    get => _state;
    set { if (SetProperty(ref _state, value)) RaisePropertyChanged(nameof(StateLabel)); }
  }

  /// <summary>Why the thread is waiting (null when it isn't); drives <see cref="StateLabel"/>.</summary>
  public ThreadWaitReason? WaitReason {
    get => _waitReason;
    set { if (SetProperty(ref _waitReason, value)) RaisePropertyChanged(nameof(StateLabel)); }
  }

  public int BasePriority { get => _basePriority; set => SetProperty(ref _basePriority, value); }
  public int CurrentPriority { get => _currentPriority; set => SetProperty(ref _currentPriority, value); }

  /// <summary>CPU time as h:mm:ss.f — enough resolution to see a busy thread tick without noise.</summary>
  public string CpuTimeLabel => _totalProcessorTime.ToString(@"hh\:mm\:ss\.f");

  /// <summary>State text, with the wait reason appended when the thread is waiting
  /// (e.g. "Wait (UserRequest)").</summary>
  public string StateLabel =>
      _state == ThreadState.Wait && _waitReason is { } reason ? $"Wait ({reason})" : _state.ToString();

  /// <summary>Start-time text (HH:mm:ss), or an em-dash when the OS wouldn't report it.</summary>
  public string StartTimeLabel => StartTime is { } t ? t.ToString("HH:mm:ss") : "—";

  public void Update(ThreadSample sample) {
    TotalProcessorTime = sample.TotalProcessorTime;
    State = sample.State;
    WaitReason = sample.WaitReason;
    BasePriority = sample.BasePriority;
    CurrentPriority = sample.CurrentPriority;
  }
}
