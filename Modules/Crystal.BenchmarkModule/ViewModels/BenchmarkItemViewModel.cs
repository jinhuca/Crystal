using Crystal.Service.Benchmark;
using Prism.Mvvm;

namespace Crystal.BenchmarkModule.ViewModels;

/// <summary>
/// One selectable benchmark row on the dashboard. Wraps an <see cref="IBenchmark"/> from the
/// catalog and holds the live run state (status, progress, result) the view binds to. The dashboard
/// VM drives the state transitions (<see cref="MarkRunning"/> → <see cref="ReportProgress"/>* →
/// <see cref="ApplyResult"/>) on the UI thread.
/// </summary>
public sealed class BenchmarkItemViewModel : BindableBase {
  public BenchmarkItemViewModel(IBenchmark benchmark) {
    Benchmark = benchmark;
  }

  public IBenchmark Benchmark { get; }

  /// <summary>The result of this row's most recent run, or null if it hasn't run in the last batch.
  /// Retained so a completed batch can be exported with numeric score and elapsed time.</summary>
  public BenchmarkResult? LastResult { get; private set; }

  public string Name => Benchmark.Name;
  public string Description => Benchmark.Description;
  public string CategoryName => Benchmark.Category.ToString();
  public string Unit => Benchmark.Unit;

  private bool _isSelected = true;
  /// <summary>Whether this suite is included in a "Run selected" batch. Defaults to selected.</summary>
  public bool IsSelected {
    get => _isSelected;
    set => SetProperty(ref _isSelected, value);
  }

  private BenchmarkRunStatus _status = BenchmarkRunStatus.Idle;
  public BenchmarkRunStatus Status {
    get => _status;
    private set { if (SetProperty(ref _status, value)) RaisePropertyChanged(nameof(IsRunning)); }
  }

  public bool IsRunning => Status == BenchmarkRunStatus.Running;

  private double _progress;
  /// <summary>0–1 progress of the current run; drives the row's progress bar.</summary>
  public double Progress {
    get => _progress;
    private set => SetProperty(ref _progress, value);
  }

  private string _statusText = "Not run";
  /// <summary>Short phase/status line (e.g. "Round 3/8", "No device").</summary>
  public string StatusText {
    get => _statusText;
    private set => SetProperty(ref _statusText, value);
  }

  private string _scoreLabel = "—";
  /// <summary>Headline score with unit once complete, else a dash.</summary>
  public string ScoreLabel {
    get => _scoreLabel;
    private set => SetProperty(ref _scoreLabel, value);
  }

  private string _detail = string.Empty;
  /// <summary>Secondary breakdown line shown under the score.</summary>
  public string Detail {
    get => _detail;
    private set => SetProperty(ref _detail, value);
  }

  public void ResetForRun() {
    LastResult = null;
    Status = BenchmarkRunStatus.Idle;
    Progress = 0;
    StatusText = "Queued";
    ScoreLabel = "—";
    Detail = string.Empty;
  }

  public void MarkRunning() {
    Status = BenchmarkRunStatus.Running;
    Progress = 0;
    StatusText = "Starting";
  }

  public void ReportProgress(BenchmarkProgress progress) {
    Progress = progress.Fraction;
    StatusText = progress.Status;
  }

  public void ApplyResult(BenchmarkResult result) {
    LastResult = result;
    if (result.Succeeded) {
      Status = BenchmarkRunStatus.Completed;
      Progress = 1;
      ScoreLabel = $"{result.Score:N1} {result.Unit}";
      Detail = result.Detail;
      StatusText = $"Done in {result.Elapsed.TotalSeconds:N1} s";
    } else if (string.Equals(result.Error, "Cancelled", StringComparison.Ordinal)) {
      Status = BenchmarkRunStatus.Cancelled;
      StatusText = "Cancelled";
    } else {
      Status = BenchmarkRunStatus.Failed;
      ScoreLabel = "—";
      Detail = result.Error ?? "Failed";
      StatusText = "Failed";
    }
  }
}
