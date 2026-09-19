using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using System.Windows.Input;
using Crystal.Controls.Threading;
using Crystal.Service.Benchmark;
using Prism.Commands;
using Prism.Mvvm;

namespace Crystal.BenchmarkModule.ViewModels;

/// <summary>
/// Backs the benchmark dashboard (the module's summary view): lists every suite from the catalog
/// grouped by category, lets the user pick some or all, and drives a sequential run through
/// <see cref="BenchmarkRunner"/>. Suite callbacks arrive on a background thread and are marshalled
/// onto the UI thread (<see cref="UiThreadMarshaller"/>) before touching the bound rows. Disposing
/// the VM — which the shell does when the benchmark window closes — cancels any in-flight run so no
/// benchmark thread outlives the window.
/// </summary>
public sealed class BenchmarkDashboardViewModel : BindableBase, IDisposable {
  private readonly BenchmarkRunner _runner;
  private readonly UiThreadMarshaller _ui = new();
  private readonly ObservableCollection<BenchmarkItemViewModel> _items = [];

  private CancellationTokenSource? _cts;
  private int _batchTotal;
  private int _batchDone;

  public BenchmarkDashboardViewModel(IBenchmarkCatalog catalog, BenchmarkRunner runner) {
    ArgumentNullException.ThrowIfNull(catalog);
    _runner = runner ?? throw new ArgumentNullException(nameof(runner));

    foreach (var suite in catalog.All) {
      var item = new BenchmarkItemViewModel(suite);
      item.PropertyChanged += OnItemPropertyChanged;
      _items.Add(item);
    }

    Items = CollectionViewSource.GetDefaultView(_items);
    Items.GroupDescriptions.Add(new PropertyGroupDescription(nameof(BenchmarkItemViewModel.CategoryName)));

    RunSelectedCommand = new DelegateCommand(() => _ = RunAsync(Selected()), () => !IsRunning && Selected().Count > 0);
    RunAllCommand = new DelegateCommand(() => _ = RunAsync(_items.ToList()), () => !IsRunning && _items.Count > 0);
    CancelCommand = new DelegateCommand(Cancel, () => IsRunning);
    SelectAllCommand = new DelegateCommand(() => SetAllSelected(true), () => !IsRunning);
    ClearSelectionCommand = new DelegateCommand(() => SetAllSelected(false), () => !IsRunning);
  }

  /// <summary>Suites grouped by category for the itemscontrol's <c>GroupStyle</c>.</summary>
  public ICollectionView Items { get; }

  public DelegateCommand RunSelectedCommand { get; }
  public DelegateCommand RunAllCommand { get; }
  public DelegateCommand CancelCommand { get; }
  public DelegateCommand SelectAllCommand { get; }
  public DelegateCommand ClearSelectionCommand { get; }

  private bool _isRunning;
  public bool IsRunning {
    get => _isRunning;
    private set { if (SetProperty(ref _isRunning, value)) RaiseCommandStates(); }
  }

  private bool _canExportReport;
  /// <summary>True once a batch has finished and left at least one result to export. The view's
  /// "Save report" button binds its enabled state to this.</summary>
  public bool CanExportReport {
    get => _canExportReport;
    private set => SetProperty(ref _canExportReport, value);
  }

  private double _overallProgress;
  public double OverallProgress {
    get => _overallProgress;
    private set => SetProperty(ref _overallProgress, value);
  }

  private string _overallStatus = "Select suites and press Run.";
  public string OverallStatus {
    get => _overallStatus;
    private set => SetProperty(ref _overallStatus, value);
  }

  private List<BenchmarkItemViewModel> Selected() => _items.Where(i => i.IsSelected).ToList();

  private void SetAllSelected(bool selected) {
    foreach (var item in _items) item.IsSelected = selected;
  }

  private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e) {
    // A selection change can flip whether "Run selected" is enabled.
    if (e.PropertyName == nameof(BenchmarkItemViewModel.IsSelected))
      RunSelectedCommand.RaiseCanExecuteChanged();
  }

  private async Task RunAsync(IReadOnlyList<BenchmarkItemViewModel> items) {
    if (IsRunning || items.Count == 0) return;

    _cts = new CancellationTokenSource();
    IsRunning = true;
    CanExportReport = false;
    _batchTotal = items.Count;
    _batchDone = 0;
    OverallProgress = 0;
    foreach (var item in items) item.ResetForRun();

    var byId = items.ToDictionary(i => i.Benchmark.Id);
    var suites = items.Select(i => i.Benchmark).ToList();

    try {
      await _runner.RunAsync(
          suites,
          onSuiteStarted: b => _ui.Post(() => {
            byId[b.Id].MarkRunning();
            OverallStatus = $"Running {b.Name}…";
          }),
          onSuiteProgress: (b, p) => _ui.Post(() => {
            var vm = byId[b.Id];
            vm.ReportProgress(p);
            OverallProgress = (_batchDone + p.Fraction) / _batchTotal;
          }),
          onSuiteCompleted: (b, r) => _ui.Post(() => {
            byId[b.Id].ApplyResult(r);
            _batchDone++;
            OverallProgress = (double)_batchDone / _batchTotal;
          }),
          _cts.Token).ConfigureAwait(false);
      _ui.Post(() => OverallStatus = "Finished.");
    } catch (OperationCanceledException) {
      _ui.Post(() => OverallStatus = "Cancelled.");
    } finally {
      _ui.Post(() => {
        IsRunning = false;
        if (_batchTotal > 0) OverallProgress = (double)_batchDone / _batchTotal;
        CanExportReport = _items.Any(i => i.LastResult is not null);
      });
      _cts?.Dispose();
      _cts = null;
    }
  }

  private void Cancel() => _cts?.Cancel();

  /// <summary>Builds a CSV report of the last batch: one row per suite that ran, with its numeric
  /// score, unit, detail, elapsed seconds and status. Values are RFC 4180-escaped so scores and
  /// details containing commas/quotes survive a spreadsheet round-trip.</summary>
  public string BuildReportCsv() {
    var sb = new StringBuilder();
    sb.Append("Category,Suite,Status,Score,Unit,Detail,ElapsedSeconds,Error\n");
    foreach (var item in _items) {
      if (item.LastResult is not { } r) continue;
      sb.Append(Csv(item.CategoryName)).Append(',')
        .Append(Csv(item.Name)).Append(',')
        .Append(Csv(item.Status.ToString())).Append(',')
        .Append(r.Succeeded ? r.Score.ToString("R", CultureInfo.InvariantCulture) : string.Empty).Append(',')
        .Append(Csv(r.Unit)).Append(',')
        .Append(Csv(r.Detail)).Append(',')
        .Append(r.Elapsed.TotalSeconds.ToString("R", CultureInfo.InvariantCulture)).Append(',')
        .Append(Csv(r.Error ?? string.Empty))
        .Append('\n');
    }
    return sb.ToString();
  }

  // RFC 4180: wrap in quotes and double any embedded quote when the field holds a comma, quote or newline.
  private static string Csv(string value) {
    if (value.IndexOfAny([',', '"', '\n', '\r']) < 0) return value;
    return '"' + value.Replace("\"", "\"\"") + '"';
  }

  private void RaiseCommandStates() {
    RunSelectedCommand.RaiseCanExecuteChanged();
    RunAllCommand.RaiseCanExecuteChanged();
    CancelCommand.RaiseCanExecuteChanged();
    SelectAllCommand.RaiseCanExecuteChanged();
    ClearSelectionCommand.RaiseCanExecuteChanged();
  }

  public void Dispose() {
    _cts?.Cancel();
    _cts?.Dispose();
    _cts = null;
    foreach (var item in _items) item.PropertyChanged -= OnItemPropertyChanged;
  }
}
