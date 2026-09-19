using Crystal.Controls.Threading;
using Crystal.Infrastructure.Constants.Navigation;
using Crystal.ProcessModule.Models;
using Crystal.Service.Gpu;
using Crystal.Service.Process;
using Prism.Commands;
using Prism.Events;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Crystal.ProcessModule.ViewModels;

/// <summary>
/// Backs the Processes master-detail view: subscribes to the live sample stream and reconciles it
/// into a stable, PID-keyed row collection (add new, update existing in place, drop exited). Rows
/// are surfaced through <see cref="RowsView"/>, a grouped/sorted collection view: grouped into
/// Apps / Background Processes / Windows Processes, and sorted by whichever column the user clicked
/// (ascending, toggling to descending on a repeat click; Name ascending by default). The selected
/// row drives the detail panel; its live metrics keep updating in place while selected.
/// </summary>
public sealed class ProcessListViewModel : BindableBase, IDisposable {
  private readonly IDisposable _subscription;
  private readonly IDisposable _statsSubscription;
  // Null when no GpuMonitor was supplied (tests, or a build without the GPU service). The GPU
  // "Total" lines stay empty in that case; the ETW-based per-process line still works.
  private readonly IDisposable? _gpuSubscription;
  private readonly UiThreadMarshaller _ui = new();
  private readonly Dictionary<uint, ProcessRowViewModel> _rowsByPid = new();
  private int _processCount;
  private int _threadCount;
  private int _handleCount;
  private int _hogCount;

  // PID of the process hosting this dashboard, used to preselect our own row on first load.
  private readonly uint _ownPid = (uint)Environment.ProcessId;
  private bool _hasSelectedDefault;

  private ProcessRowViewModel? _selectedRow;
  private string _sortProperty = nameof(ProcessRowViewModel.Name);
  private ListSortDirection _sortDirection = ListSortDirection.Ascending;
  private string _nameFilter = string.Empty;
  private string _pidFilter = string.Empty;
  private bool _peaksResetThisSession;
  private readonly Func<DateTimeOffset> _clock;

  // Extracts per-process shell icons off the UI thread; null in tests where no provider is supplied.
  private readonly ProcessIconProvider? _iconProvider;

  // Terminates / launches processes. Defaults to the real Win32 controller; tests inject a fake to
  // assert on the calls without touching real processes.
  private readonly IProcessController _controller;
  private string? _actionStatus;

  // Records the monitored processes' per-poll readings to a CSV. Defaults to the real file-backed
  // recorder; tests inject a fake to assert on the writes without touching disk.
  private readonly IProcessRecorder _recorder;
  // PIDs being recorded, captured when recording starts so the recording keeps following those
  // processes even if the selection later changes. Empty when not recording. A PID is dropped as it
  // exits; recording auto-stops once the set empties.
  private readonly HashSet<uint> _recordingPids = [];
  private bool _isRecording;

  // Publishes the ShowDetailEvent that opens the benchmark window; null in tests (command disabled).
  private readonly IEventAggregator? _events;

  // clock, iconProvider, controller and recorder are optional so Unity's default registration works
  // (optional ctor params aren't injected); tests pass a fixed clock for a deterministic timestamp,
  // skip icons, and inject fakes.
  public ProcessListViewModel(IProcessModel model, SystemStatsMonitor systemStats,
                              ProcessIconProvider? iconProvider = null, Func<DateTimeOffset>? clock = null,
                              IProcessController? controller = null, IProcessRecorder? recorder = null,
                              GpuMonitor? gpuMonitor = null, IEventAggregator? events = null) {
    _clock = clock ?? (() => DateTimeOffset.Now);
    _iconProvider = iconProvider;
    _controller = controller ?? new ProcessController();
    _recorder = recorder ?? new ProcessRecorder();
    _events = events;
    OpenBenchmarkCommand = new DelegateCommand(OpenBenchmark, () => _events is not null);
    MetricsStatusError = model.MetricsStatusError;

    RowsView = new ListCollectionView(Rows);
    // Group into the three categories; the enum's order (App, Background, Windows) is used to
    // order the groups themselves via the leading sort description below.
    RowsView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(ProcessRowViewModel.CategoryName)));
    RowsView.Filter = MatchesFilters;
    ApplySortDescriptions();

    // Live shaping: let the view re-sort/re-group/re-filter incrementally as row properties change,
    // instead of a full Refresh() on every poll. Refresh() raises a CollectionChanged.Reset, which
    // regenerates the whole virtualized list and recomputes the scroll extent each second — that is
    // what makes the scrollbar flash. With live shaping the view emits fine-grained moves instead.
    EnableLiveShaping();

    _subscription = model.Processes.Subscribe(samples => OnUi(() => Apply(samples)));
    _statsSubscription = systemStats.Stats.Subscribe(s => OnUi(() => UpdateSystemStats(s)));
    // Real machine-wide GPU utilization from the sensor stack (the same source the dashboard GPU
    // tile reads), split into dedicated/integrated adapters. Replaces the old "sum of per-process
    // GPU%" total, which under-measured badly (near 0% while a card was pinned at 100%) because the
    // per-process signal is ETW DMA-packet timing, not the adapter load sensor.
    _gpuSubscription = gpuMonitor?.Sensors.Subscribe(s => OnUi(() => UpdateGpuStats(s)));
  }

  public ObservableCollection<ProcessRowViewModel> Rows { get; } = [];

  /// <summary>Opens the benchmark suite in its own detail window (via the shell's DetailWindowService).</summary>
  public ICommand OpenBenchmarkCommand { get; }

  private void OpenBenchmark() =>
      _events?.GetEvent<ShowDetailEvent>().Publish(DetailViewNames.Benchmark);

  /// <summary>Most processes that can be monitored (and plotted) at once.</summary>
  public const int MaxMonitored = 5;

  /// <summary>The processes currently selected for monitoring (1–<see cref="MaxMonitored"/>), each with
  /// its own color and rolling per-metric histories. Drives the detail-panel legend and the per-process
  /// line on every utilization graph. Selecting past the cap drops the oldest (FIFO).</summary>
  public ObservableCollection<MonitoredProcessViewModel> MonitoredProcesses { get; } = [];

  // Fixed line colors, one assigned per monitored process. Chosen to stay distinct from the amber
  // "Total" line (#E0B84C) and the green integrated-GPU line (#6FCF97). Frozen so they bind from any
  // thread; the palette size equals MaxMonitored so a free color always exists.
  private static readonly Brush[] Palette = [
      FrozenBrush("#4EA3F0"), FrozenBrush("#C77DFF"), FrozenBrush("#FF6FB5"),
      FrozenBrush("#35D0C0"), FrozenBrush("#9AA0FF"),
  ];

  private static Brush FrozenBrush(string hex) {
    var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
    brush.Freeze();
    return brush;
  }

  // First palette color not currently used by a monitored process. The FIFO drop in Select frees the
  // oldest color before this runs, so with cap == palette size one is always available.
  private Brush NextColor() {
    foreach (var brush in Palette)
      if (!MonitoredProcesses.Any(m => ReferenceEquals(m.Color, brush)))
        return brush;
    return Palette[0];
  }

  /// <summary>Number of running processes system-wide, shown in the summary header.</summary>
  public int ProcessCount { get => _processCount; private set => SetProperty(ref _processCount, value); }

  /// <summary>Total threads across every running process.</summary>
  public int ThreadCount { get => _threadCount; private set => SetProperty(ref _threadCount, value); }

  /// <summary>Total open handles across every running process.</summary>
  public int HandleCount { get => _handleCount; private set => SetProperty(ref _handleCount, value); }

  /// <summary>Number of processes whose session CPU or memory peak has crossed the sustained-hog
  /// threshold — the count of tinted rows. Shown in the header so spikes are visible without
  /// scanning the list.</summary>
  public int HogCount { get => _hogCount; private set => SetProperty(ref _hogCount, value); }

  /// <summary>
  /// Null when per-process GPU/Disk/Network are live; otherwise a short reason they're blank (ETW
  /// session couldn't start — typically "not elevated"). Bound to a warning banner in the view.
  /// </summary>
  public string? MetricsStatusError { get; }

  /// <summary>True when <see cref="MetricsStatusError"/> should be shown.</summary>
  public bool HasMetricsStatusError => !string.IsNullOrEmpty(MetricsStatusError);

  /// <summary>Grouped + sorted view over <see cref="Rows"/>; this is what the ListView binds to.</summary>
  public ListCollectionView RowsView { get; }

  /// <summary>Number of samples retained in each utilization history — one per poll (≈ one second),
  /// so the detail-panel graphs show roughly the last minute of activity.</summary>
  public const int HistoryCapacity = 60;

  // Latest machine-wide utilization, cached from the SystemStatsMonitor stream (a separate poll from
  // the process stream). The graph "Total" lines are appended on the process-stream cadence reading
  // these cached values, so both series share one append timeline.
  private double _systemCpuPercent;
  private double _systemMemoryPercent;
  // Latest per-kind GPU core load (%), cached from the GpuMonitor sensor stream (a separate poll
  // from the process stream). The GPU "Total" lines are appended on the process-stream cadence
  // reading these, so every series shares one append timeline. Presence flags gate whether that
  // adapter's series and legend entry show at all — a machine with no discrete card never plots a
  // flat-zero "Dedicated" line, and a desktop with no iGPU never plots "Integrated".
  private double _dedicatedGpuPercent;
  private double _integratedGpuPercent;
  private bool _hasDedicatedGpu;
  private bool _hasIntegratedGpu;
  // Seeded at 0 (unknown) rather than 1: the process stream can fire before the stats stream first
  // delivers total physical memory, and dividing a working set by a 1 MB "total" would clamp the
  // selected-process memory line to 100% for that first poll — the spike seen when the view opens.
  // The `> 0` guard below means an unknown total yields 0%, so the line stays flat until the real
  // total arrives.
  private double _systemMemoryTotalMb;
  // Storage/network are throughput rates (MB/s), not a fixed 0–100%, so each graph carries its own
  // dynamic upper bound tracking the windowed peak total. Seeded at 1 so an all-idle window still
  // gives the axis a sane, non-zero scale.
  private double _systemDiskMBps;
  private double _systemNetMBps;
  private double _diskMaxMBps = 1;
  private double _netMaxMBps = 1;

  /// <summary>Machine-wide CPU% (GetSystemTimes busy fraction) over the last polls. Feeds the "Total"
  /// line of the CPU graph.</summary>
  public ObservableCollection<double> TotalCpuHistory { get; } = [];

  /// <summary>Dedicated (discrete) GPU core-load% history, from the GPU sensor stream. Feeds the
  /// "Dedicated" GPU line; only appended (and shown) when a discrete adapter is present.</summary>
  public ObservableCollection<double> DedicatedGpuHistory { get; } = [];

  /// <summary>Integrated GPU core-load% history, from the GPU sensor stream. Feeds the "Integrated"
  /// GPU line; only appended (and shown) when an integrated adapter is present.</summary>
  public ObservableCollection<double> IntegratedGpuHistory { get; } = [];

  /// <summary>Machine-wide memory load percentage (GlobalMemoryStatusEx). Feeds the "Total" memory
  /// line.</summary>
  public ObservableCollection<double> TotalMemoryHistory { get; } = [];

  /// <summary>Latest machine-wide CPU utilization (%), shown live in the CPU graph header's "Total".</summary>
  public double SystemCpuPercent { get => _systemCpuPercent; private set => SetProperty(ref _systemCpuPercent, value); }

  /// <summary>Latest dedicated (discrete) GPU utilization (%), shown live in the GPU graph header's
  /// "Dedicated" and fed into <see cref="DedicatedGpuHistory"/>.</summary>
  public double DedicatedGpuPercent { get => _dedicatedGpuPercent; private set => SetProperty(ref _dedicatedGpuPercent, value); }

  /// <summary>Latest integrated GPU utilization (%), shown live in the GPU graph header's
  /// "Integrated" and fed into <see cref="IntegratedGpuHistory"/>.</summary>
  public double IntegratedGpuPercent { get => _integratedGpuPercent; private set => SetProperty(ref _integratedGpuPercent, value); }

  /// <summary>True when a discrete GPU is present; gates the "Dedicated" line and legend entry.</summary>
  public bool HasDedicatedGpu { get => _hasDedicatedGpu; private set => SetProperty(ref _hasDedicatedGpu, value); }

  /// <summary>True when an integrated GPU is present; gates the "Integrated" line and legend entry.</summary>
  public bool HasIntegratedGpu { get => _hasIntegratedGpu; private set => SetProperty(ref _hasIntegratedGpu, value); }

  /// <summary>Latest machine-wide memory utilization (%), shown live in the memory graph header's "Total".</summary>
  public double SystemMemoryPercent { get => _systemMemoryPercent; private set => SetProperty(ref _systemMemoryPercent, value); }

  /// <summary>Total disk throughput (MB/s, summed across every process) history. Feeds the "Total" storage line.</summary>
  public ObservableCollection<double> TotalDiskHistory { get; } = [];

  /// <summary>Total network throughput (MB/s, summed across every process) history. Feeds the "Total" network line.</summary>
  public ObservableCollection<double> TotalNetHistory { get; } = [];

  /// <summary>Latest total disk throughput (MB/s), shown live in the storage graph header's "Total".</summary>
  public double SystemDiskMBps { get => _systemDiskMBps; private set => SetProperty(ref _systemDiskMBps, value); }

  /// <summary>Latest total network throughput (MB/s), shown live in the network graph header's "Total".</summary>
  public double SystemNetMBps { get => _systemNetMBps; private set => SetProperty(ref _systemNetMBps, value); }

  /// <summary>Upper bound (MB/s) for the storage graph, tracking the windowed peak total with ~10%
  /// headroom (never below 1) so the small per-process line and the larger total stay on-scale.
  /// Storage/network are rates, not a fixed 0–100%, so their axes are dynamic.</summary>
  public double DiskMaxMBps { get => _diskMaxMBps; private set => SetProperty(ref _diskMaxMBps, value); }

  /// <summary>Upper bound (MB/s) for the network graph; windowed peak total with ~10% headroom (never below 1).</summary>
  public double NetMaxMBps { get => _netMaxMBps; private set => SetProperty(ref _netMaxMBps, value); }

  public string SortProperty => _sortProperty;
  public ListSortDirection SortDirection => _sortDirection;

  /// <summary>
  /// The primary (most-recently-selected) monitored process — the one <see cref="EndSelectedTask"/> and
  /// recording target, and null exactly when nothing is monitored. Getting returns the primary; setting
  /// is an <em>exclusive</em> select (used by the default-selection logic and tests): it deselects every
  /// other monitored row and selects the given one (null clears all). Multi-select from the list toggles
  /// individual rows' <see cref="ProcessRowViewModel.IsSelected"/>, which routes through
  /// <see cref="Select"/>/<see cref="Deselect"/> below without going through this setter.
  /// </summary>
  public ProcessRowViewModel? SelectedRow {
    get => _selectedRow;
    set {
      // Snapshot before mutating: Deselect edits MonitoredProcesses as each row is turned off.
      foreach (var monitor in MonitoredProcesses.ToList())
        if (!ReferenceEquals(monitor.Row, value)) monitor.Row.IsSelected = false;
      if (value is not null) value.IsSelected = true;
      else SetPrimary(null);
    }
  }

  // Update the primary and the commands/state that key off it. Split out so Select/Deselect can set it
  // without re-entering the exclusive-select SelectedRow setter.
  private void SetPrimary(ProcessRowViewModel? row) {
    // Pass the name explicitly: SetProperty's CallerMemberName would otherwise resolve to "SetPrimary",
    // so the SelectedRow binding (detail-panel visibility, empty-state hint) would never be notified.
    if (SetProperty(ref _selectedRow, row, nameof(SelectedRow))) {
      RaisePropertyChanged(nameof(CanEndSelectedTask));
      RaisePropertyChanged(nameof(CanStartRecording));
    }
  }

  // A row's IsSelected flag changed (bound to each ListViewItem.IsSelected): add or remove it from the
  // monitored set accordingly.
  private void OnRowPropertyChanged(object? sender, PropertyChangedEventArgs e) {
    if (e.PropertyName != nameof(ProcessRowViewModel.IsSelected) || sender is not ProcessRowViewModel row)
      return;
    if (row.IsSelected) Select(row);
    else Deselect(row);
  }

  // Begin monitoring a row. Already-monitored just becomes primary. At the cap, the oldest is dropped
  // FIFO (turning its IsSelected off routes back through Deselect, freeing its slot and color first).
  private void Select(ProcessRowViewModel row) {
    if (MonitoredProcesses.Any(m => ReferenceEquals(m.Row, row))) {
      SetPrimary(row);
      return;
    }

    while (MonitoredProcesses.Count >= MaxMonitored)
      MonitoredProcesses[0].Row.IsSelected = false;

    MonitoredProcesses.Add(new MonitoredProcessViewModel(row, NextColor()));
    SetPrimary(row);
  }

  // Stop monitoring a row: drop its monitor entry (freeing its color) and, if it was primary, hand the
  // primary role to the newest remaining monitor, or null when none are left.
  private void Deselect(ProcessRowViewModel row) {
    var monitor = MonitoredProcesses.FirstOrDefault(m => ReferenceEquals(m.Row, row));
    if (monitor is null) return;

    MonitoredProcesses.Remove(monitor);
    if (ReferenceEquals(_selectedRow, row))
      SetPrimary(MonitoredProcesses.Count > 0 ? MonitoredProcesses[^1].Row : null);
  }

  /// <summary>True when a row is selected and so eligible to be terminated. Bound to the End task
  /// button's enabled state.</summary>
  public bool CanEndSelectedTask => _selectedRow is not null;

  /// <summary>True when the Record button should be enabled: either a recording is running (so it can
  /// be stopped) or a row is selected to start recording. Bound to the Record button's enabled
  /// state.</summary>
  public bool CanStartRecording => _isRecording || _selectedRow is not null;

  /// <summary>True while a recording is in progress. Drives the Record/Stop button label and its
  /// active-state styling.</summary>
  public bool IsRecording {
    get => _isRecording;
    private set {
      if (SetProperty(ref _isRecording, value)) {
        RaisePropertyChanged(nameof(RecordButtonLabel));
        RaisePropertyChanged(nameof(CanStartRecording));
      }
    }
  }

  /// <summary>Label for the toggle button: "Record" when idle, "Stop rec" while recording.</summary>
  public string RecordButtonLabel => _isRecording ? "Stop rec" : "Record";

  /// <summary>Last End task / Run new task outcome message (a failure reason), or null when the last
  /// action succeeded or none has run. Bound to a transient status line in the header.</summary>
  public string? ActionStatus {
    get => _actionStatus;
    private set {
      if (SetProperty(ref _actionStatus, value)) RaisePropertyChanged(nameof(HasActionStatus));
    }
  }

  /// <summary>True when <see cref="ActionStatus"/> has a message to show.</summary>
  public bool HasActionStatus => !string.IsNullOrEmpty(_actionStatus);

  /// <summary>Terminates the selected process. No-op when nothing is selected. On failure the reason
  /// is surfaced through <see cref="ActionStatus"/>; the exited row drops on the next poll.</summary>
  public void EndSelectedTask() {
    if (_selectedRow is not { } row) return;
    var result = _controller.EndTask(row.ProcessId);
    ActionStatus = result.Succeeded ? null : result.Message;
  }

  /// <summary>Launches a new process from a command line (optionally elevated), like Task Manager's
  /// "Run new task". On failure the reason is surfaced through <see cref="ActionStatus"/>.</summary>
  public void StartTask(string command, bool runAsAdmin = false) {
    var result = _controller.StartTask(command, runAsAdmin);
    ActionStatus = result.Succeeded ? null : result.Message;
  }

  /// <summary>
  /// Begins recording every monitored process's per-poll readings to <paramref name="filePath"/>. The
  /// recording follows the PIDs monitored now, even if the selection later changes; each poll appends
  /// one CSV row per still-running process (tagged by PID/Name). No-op when nothing is monitored or a
  /// recording is already running. On failure to open the file the reason is surfaced through
  /// <see cref="ActionStatus"/>.
  /// </summary>
  public void StartRecording(string filePath) {
    if (_isRecording || MonitoredProcesses.Count == 0) return;

    var result = _recorder.Start(filePath, MetricsStatusError);
    if (!result.Succeeded) {
      ActionStatus = result.Message;
      return;
    }

    _recordingPids.Clear();
    foreach (var monitor in MonitoredProcesses) _recordingPids.Add(monitor.ProcessId);
    IsRecording = true;
    ActionStatus = _recordingPids.Count == 1
        ? $"Recording {MonitoredProcesses[0].Row.Name} (PID {MonitoredProcesses[0].ProcessId}) → {System.IO.Path.GetFileName(filePath)}"
        : $"Recording {_recordingPids.Count} processes → {System.IO.Path.GetFileName(filePath)}";
  }

  /// <summary>Stops the current recording and reports where it was saved and how many samples it
  /// captured. No-op when not recording.</summary>
  public void StopRecording() {
    if (!_isRecording) return;

    int samples = _recorder.SampleCount;
    string? file = _recorder.FilePath is { } p ? System.IO.Path.GetFileName(p) : null;
    _recorder.Stop();
    _recordingPids.Clear();
    IsRecording = false;
    ActionStatus = file is null
        ? $"Recording stopped ({samples} sample(s))"
        : $"Recording saved to {file} ({samples} sample(s))";
  }

  // Every recorded process has exited: close the file and report it, distinct from a user-initiated
  // stop so the user understands why recording ended on its own.
  private void StopRecordingOnExit() {
    int samples = _recorder.SampleCount;
    string? file = _recorder.FilePath is { } p ? System.IO.Path.GetFileName(p) : null;
    _recorder.Stop();
    _recordingPids.Clear();
    IsRecording = false;
    ActionStatus = file is null
        ? $"Recording ended: all recorded processes exited ({samples} sample(s))"
        : $"Recording ended: all recorded processes exited — saved to {file} ({samples} sample(s))";
  }

  /// <summary>Opens Explorer at the given process image, file selected. On failure (unknown path,
  /// file gone) the reason is surfaced through <see cref="ActionStatus"/>.</summary>
  public void OpenFileLocation(string? imagePath) {
    var result = _controller.OpenFileLocation(imagePath);
    ActionStatus = result.Succeeded ? null : result.Message;
  }

  /// <summary>Case-insensitive substring filter on the process name; empty shows all. Bound to the
  /// search box in the Name column header.</summary>
  public string NameFilter {
    get => _nameFilter;
    set {
      if (SetProperty(ref _nameFilter, value ?? string.Empty)) {
        RowsView.Refresh();
        RaisePropertyChanged(nameof(HasVisibleRows));
      }
    }
  }

  /// <summary>Substring filter on the PID (matched against its decimal text); empty shows all. Bound
  /// to the search box in the PID column header.</summary>
  public string PidFilter {
    get => _pidFilter;
    set {
      if (SetProperty(ref _pidFilter, value ?? string.Empty)) {
        RowsView.Refresh();
        RaisePropertyChanged(nameof(HasVisibleRows));
      }
    }
  }

  // Turn on live sorting/grouping/filtering and tell the view which properties to watch. Without
  // these the view can't know a row's sort key or filter result changed without a full Refresh().
  private void EnableLiveShaping() {
    RowsView.IsLiveSorting = true;
    RowsView.IsLiveGrouping = true;
    RowsView.IsLiveFiltering = true;

    foreach (var property in new[] {
        nameof(ProcessRowViewModel.Name),
        nameof(ProcessRowViewModel.ProcessId),
        nameof(ProcessRowViewModel.Category),
        nameof(ProcessRowViewModel.CategoryName),
        nameof(ProcessRowViewModel.Status),
        nameof(ProcessRowViewModel.CpuPercent),
        nameof(ProcessRowViewModel.PeakCpuPercent),
        nameof(ProcessRowViewModel.PeakWorkingSetMb),
        nameof(ProcessRowViewModel.GpuPercent),
        nameof(ProcessRowViewModel.WorkingSetMb),
        nameof(ProcessRowViewModel.DiskBytesPerSec),
        nameof(ProcessRowViewModel.NetBytesPerSec),
    }) {
      RowsView.LiveSortingProperties.Add(property);
      RowsView.LiveFilteringProperties.Add(property);
      RowsView.LiveGroupingProperties.Add(property);
    }
  }

  private bool MatchesFilters(object item) {
    if (item is not ProcessRowViewModel row) return false;
    if (_nameFilter.Length > 0 &&
        (row.Name is null ||
         row.Name.IndexOf(_nameFilter, StringComparison.OrdinalIgnoreCase) < 0)) {
      return false;
    }
    if (_pidFilter.Length > 0 &&
        !row.ProcessId.ToString().Contains(_pidFilter, StringComparison.Ordinal)) {
      return false;
    }
    return true;
  }

  /// <summary>
  /// Sort the list by <paramref name="propertyName"/>. Clicking the active column flips the
  /// direction; clicking a new column starts ascending. Grouping is preserved — the sort applies
  /// within each group.
  /// </summary>
  public void SortBy(string propertyName) {
    if (_sortProperty == propertyName) {
      _sortDirection = _sortDirection == ListSortDirection.Ascending
          ? ListSortDirection.Descending
          : ListSortDirection.Ascending;
    } else {
      _sortProperty = propertyName;
      _sortDirection = ListSortDirection.Ascending;
    }
    ApplySortDescriptions();
    RaisePropertyChanged(nameof(SortProperty));
    RaisePropertyChanged(nameof(SortDirection));
  }

  private void ApplySortDescriptions() {
    using (RowsView.DeferRefresh()) {
      RowsView.SortDescriptions.Clear();
      // Sort by Category first so the groups appear in enum order (Apps, Background, Windows)...
      RowsView.SortDescriptions.Add(new SortDescription(nameof(ProcessRowViewModel.Category), ListSortDirection.Ascending));
      // ...then by the user-chosen column within each group.
      if (_sortProperty != nameof(ProcessRowViewModel.Category)) {
        RowsView.SortDescriptions.Add(new SortDescription(_sortProperty, _sortDirection));
      }
    }
  }

  private void Apply(IReadOnlyList<ProcessSample> samples) {
    var live = new HashSet<uint>(samples.Count);

    foreach (var s in samples) {
      live.Add(s.ProcessId);
      if (_rowsByPid.TryGetValue(s.ProcessId, out var row)) {
        row.Update(s);
      } else {
        var created = new ProcessRowViewModel(s.ProcessId, s.Name);
        created.Update(s);
        created.PropertyChanged += OnRowPropertyChanged;
        _rowsByPid[s.ProcessId] = created;
        Rows.Add(created);
      }

      // Append this poll's reading for each recorded process, from the same sample stream the rows
      // update from — no extra sensor work.
      if (_isRecording && _recordingPids.Contains(s.ProcessId)) _recorder.WriteSample(s, _clock());
    }

    // Drop any recorded PIDs that are gone this poll; once they've all exited, end the recording
    // cleanly rather than leave it running against no live process.
    if (_isRecording) {
      _recordingPids.RemoveWhere(pid => !live.Contains(pid));
      if (_recordingPids.Count == 0) StopRecordingOnExit();
    }

    // Drop rows for processes that are gone. Stop monitoring any that were selected (Deselect frees
    // its line/legend/color and reassigns the primary), then unhook and remove the row.
    for (int i = Rows.Count - 1; i >= 0; i--) {
      var row = Rows[i];
      if (!live.Contains(row.ProcessId)) {
        Deselect(row);
        row.PropertyChanged -= OnRowPropertyChanged;
        _rowsByPid.Remove(row.ProcessId);
        Rows.RemoveAt(i);
      }
    }

    // No RowsView.Refresh() here: live shaping (see EnableLiveShaping) re-sorts/re-groups/re-filters
    // incrementally as each row's bound properties change, so the view stays current without the
    // full Reset that made the scrollbar flash every second.

    // Default the selection to this app's own process once it first appears in the list.
    if (!_hasSelectedDefault && _rowsByPid.TryGetValue(_ownPid, out var ownRow)) {
      SelectedRow = ownRow;
      _hasSelectedDefault = true;
    }

    ResolveIcons();

    RaisePropertyChanged(nameof(HasVisibleRows));
    RecomputeHogCount();
    UpdateUtilizationHistory(samples);
  }

  // Append this poll's readings to the rolling utilization histories that back the detail-panel
  // graphs: the system-wide totals (summed across every process) and the selected process's own
  // reading, so each graph plots "selected process vs. total" as two series. Runs on the UI thread
  // (called from Apply), so mutating the bound collections here is safe.
  private void UpdateUtilizationHistory(IReadOnlyList<ProcessSample> samples) {
    // GPU totals come from the GpuMonitor sensor stream (cached in UpdateGpuStats), not from summing
    // per-process GPU%: the per-process signal is ETW DMA-packet timing, which under-measures real
    // occupancy, and summing it hid a busy card as near-0. CPU and memory totals come from the
    // machine-wide sampler (GetSystemTimes / GlobalMemoryStatusEx) cached off the stats stream —
    // summing per-process CPU would double-count the idle process (~99% bug).
    // Storage/network have no machine-wide counter here, so the total is the summed per-process
    // throughput. Unlike CPU, summing disk/net I/O has no idle-process inflation, so the sum is a
    // faithful machine-wide rate. Bytes/sec is null until the ETW backend is live.
    double totalDiskBytes = 0;
    double totalNetBytes = 0;
    foreach (var s in samples) {
      totalDiskBytes += s.DiskBytesPerSec ?? 0;
      totalNetBytes += s.NetBytesPerSec ?? 0;
    }

    const double bytesPerMb = 1024 * 1024;
    double totalDisk = totalDiskBytes / bytesPerMb;
    double totalNet = totalNetBytes / bytesPerMb;

    Append(TotalCpuHistory, _systemCpuPercent);
    // Only plot a GPU-kind line when that adapter exists, so an absent card leaves an empty series
    // (no line) rather than a misleading flat zero.
    if (_hasDedicatedGpu) Append(DedicatedGpuHistory, _dedicatedGpuPercent);
    if (_hasIntegratedGpu) Append(IntegratedGpuHistory, _integratedGpuPercent);
    Append(TotalMemoryHistory, _systemMemoryPercent);
    Append(TotalDiskHistory, totalDisk);
    Append(TotalNetHistory, totalNet);

    // One reading appended per monitored process to its own five histories, so each graph plots a
    // line per selected process against the shared Total. The row's live metrics are read straight
    // off its ProcessRowViewModel (the same values the list and legend show).
    foreach (var monitor in MonitoredProcesses) {
      var row = monitor.Row;
      double mem = _systemMemoryTotalMb > 0
          ? Math.Min(100, row.WorkingSetMb / _systemMemoryTotalMb * 100)
          : 0;
      Append(monitor.CpuHistory, row.CpuPercent);
      Append(monitor.GpuHistory, row.GpuPercent ?? 0);
      Append(monitor.MemoryHistory, mem);
      Append(monitor.DiskHistory, (row.DiskBytesPerSec ?? 0) / bytesPerMb);
      Append(monitor.NetHistory, (row.NetBytesPerSec ?? 0) / bytesPerMb);
    }

    SystemDiskMBps = totalDisk;
    SystemNetMBps = totalNet;

    // Rescale each throughput axis to the current window's peak (+10% headroom, floor 1 MB/s) so the
    // small per-process lines and the larger total all stay readable as traffic rises and falls.
    DiskMaxMBps = AxisMax(TotalDiskHistory, MonitoredProcesses.Select(m => m.DiskHistory));
    NetMaxMBps = AxisMax(TotalNetHistory, MonitoredProcesses.Select(m => m.NetHistory));
  }

  // Dynamic upper bound for a throughput graph: the largest value across the total window and every
  // per-process window, with ~10% headroom so the peak doesn't touch the ceiling, never below 1 MB/s
  // so an idle window still has a sane scale.
  private static double AxisMax(
      ObservableCollection<double> total, IEnumerable<ObservableCollection<double>> perProcess) {
    double peak = 0;
    foreach (var v in total) if (v > peak) peak = v;
    foreach (var history in perProcess)
      foreach (var v in history) if (v > peak) peak = v;
    return Math.Max(1, peak * 1.1);
  }

  // Push a sample onto a fixed-length history, dropping the oldest once full. The graph's own buffer
  // is circular and ignores the resulting Remove, so trimming here only bounds the collection.
  private static void Append(ObservableCollection<double> history, double value) {
    history.Add(value);
    while (history.Count > HistoryCapacity) history.RemoveAt(0);
  }

  // Resolve shell icons for rows that don't have one yet but now know their executable path. The
  // provider caches by path and extraction touches disk/shell, so we do it on a background thread
  // and assign the frozen result back on the UI thread. A row's path can arrive a poll or two after
  // it first appears (WMI briefly returns it empty), so this runs every poll, not just on add — but
  // it only ever looks at rows still missing an icon, so steady state does no work.
  private void ResolveIcons() {
    if (_iconProvider is null) return;

    List<ProcessRowViewModel>? pending = null;
    foreach (var row in Rows) {
      if (row.IconSource is null && !string.IsNullOrEmpty(row.ExecutablePath))
        (pending ??= []).Add(row);
    }
    if (pending is null) return;

    var toResolve = pending;
    Task.Run(() => {
      foreach (var row in toResolve) {
        var path = row.ExecutablePath;
        var icon = _iconProvider.GetIcon(path);
        if (icon is not null) OnUi(() => row.IconSource = icon);
      }
    });
  }

  // Count the flagged rows across the whole list (not the filtered view) — a hog stays counted even
  // when a name/PID filter hides it, so the header reflects the true session state.
  private void RecomputeHogCount() {
    int count = 0;
    foreach (var row in Rows)
      if (row.IsSustainedCpuHog || row.IsMemoryHog) count++;
    HogCount = count;
  }

  /// <summary>True when there is at least one visible row (post-filter) to copy or save. Bound to
  /// the enabled state of the Copy/Save controls.</summary>
  public bool HasVisibleRows => !RowsView.IsEmpty;

  /// <summary>
  /// The currently-visible rows (post-filter, in the current group/sort order) as tab-separated
  /// text, with a provenance header. Mirrors what the list shows, so a filtered view exports only
  /// the shown rows. Null GPU/Disk/Network read as "-" (the same placeholder the grid shows).
  /// Returns "" when nothing is visible.
  /// </summary>
  public string RowsAsText() {
    if (RowsView.IsEmpty) return "";
    var sb = new System.Text.StringBuilder();
    sb.AppendLine($"# Exported {_clock().LocalDateTime:yyyy-MM-dd HH:mm:ss}");
    var shown = RowsView.Cast<ProcessRowViewModel>().ToList();
    sb.AppendLine($"# {shown.Count} process(es)");
    // Per-category breakdown of the exported rows (post-filter), so a pasted snapshot states its
    // composition. Only lists categories that are present.
    int apps = shown.Count(r => r.Category == ProcessCategory.App);
    int background = shown.Count(r => r.Category == ProcessCategory.BackgroundProcess);
    int windows = shown.Count(r => r.Category == ProcessCategory.WindowsProcess);
    var parts = new List<string>();
    if (apps > 0) parts.Add($"{apps} app(s)");
    if (background > 0) parts.Add($"{background} background");
    if (windows > 0) parts.Add($"{windows} windows");
    if (parts.Count > 0) sb.AppendLine($"# {string.Join(", ", parts)}");
    // State the ordering so a pasted snapshot isn't read as unsorted. Rows always group by category
    // first, then by the user-chosen column — call that out.
    string direction = _sortDirection == ListSortDirection.Ascending ? "ascending" : "descending";
    sb.AppendLine($"# Sorted by category, then {SortColumnLabel(_sortProperty)} {direction}");
    // If peaks were reset mid-session, the CPU pk / Mem pk columns cover only the window since the
    // reset, not the whole session — say so rather than let a partial peak read as the session max.
    if (_peaksResetThisSession)
      sb.AppendLine("# Peaks were reset this session — CPU pk / Mem pk cover only the window since the last reset");
    // Session-wide hog count (matches the header badge), not the visible subset — flags how many
    // processes spiked even if a filter is hiding some of them.
    if (HogCount > 0)
      sb.AppendLine($"# {HogCount} sustained hog(s): peak CPU ≥ {ProcessRowViewModel.SustainedCpuHogThreshold:0}% or peak memory ≥ {ProcessRowViewModel.MemoryHogThresholdMb:0} MB");
    // Explain the "-" placeholders when the ETW backend isn't live (typically not elevated), so a
    // reader doesn't take the blank GPU/Disk/Network columns for genuine zero activity.
    if (!string.IsNullOrEmpty(MetricsStatusError))
      sb.AppendLine($"# GPU/Disk/Network unavailable ({MetricsStatusError}) — shown as '-'");
    if (_nameFilter.Length > 0 || _pidFilter.Length > 0)
      sb.AppendLine("# Filtered view: only rows matching the active name/PID filter");
    sb.AppendLine("Group\tName\tPID\tStatus\tCPU%\tCPU pk%\tGPU%\tMemory MB\tMem pk MB\tDisk B/s\tNet B/s");
    foreach (var r in RowsView.Cast<ProcessRowViewModel>()) {
      sb.AppendLine(string.Join('\t',
          r.CategoryName,
          r.Name,
          r.ProcessId,
          r.Status ?? "",
          r.CpuPercent.ToString("0.0"),
          r.PeakCpuPercent.ToString("0.0"),
          Cell(r.GpuPercent, "0.0"),
          r.WorkingSetMb.ToString("0"),
          r.PeakWorkingSetMb.ToString("0"),
          Cell(r.DiskBytesPerSec, "0"),
          Cell(r.NetBytesPerSec, "0")));
    }
    return sb.ToString();
  }

  // A null metric (ETW not live) reads as "-", matching the grid's placeholder rather than a
  // misleading 0.
  private static string Cell(double? value, string format) =>
      value is { } v ? v.ToString(format) : "-";

  // Map a row-VM sort property to the column label shown in the grid, for the export's ordering note.
  private static string SortColumnLabel(string property) => property switch {
    nameof(ProcessRowViewModel.Name) => "Name",
    nameof(ProcessRowViewModel.ProcessId) => "PID",
    nameof(ProcessRowViewModel.Status) => "Status",
    nameof(ProcessRowViewModel.CpuPercent) => "CPU",
    nameof(ProcessRowViewModel.PeakCpuPercent) => "CPU pk",
    nameof(ProcessRowViewModel.GpuPercent) => "GPU",
    nameof(ProcessRowViewModel.WorkingSetMb) => "Memory",
    nameof(ProcessRowViewModel.PeakWorkingSetMb) => "Mem pk",
    nameof(ProcessRowViewModel.DiskBytesPerSec) => "Disk",
    nameof(ProcessRowViewModel.NetBytesPerSec) => "Network",
    _ => property,
  };

  /// <summary>Resets every row's session peaks to its current live reading — a fresh high-water
  /// window for the whole list. Peaks re-establish from live values on the next poll.</summary>
  public void ResetAllPeaks() {
    foreach (var row in Rows) row.ResetPeaks();
    RecomputeHogCount();
    _peaksResetThisSession = true;
  }

  private void UpdateSystemStats(SystemStats stats) {
    ProcessCount = stats.Processes;
    ThreadCount = stats.Threads;
    HandleCount = stats.Handles;
    if (stats.MemoryTotalMb > 0) _systemMemoryTotalMb = stats.MemoryTotalMb;
    // Set only through the properties: they write the backing fields (which the graph append reads)
    // and raise PropertyChanged for the headers. Assigning the fields directly first would make the
    // property setters see "no change" and skip the notification, freezing the headers at 0.
    SystemCpuPercent = stats.CpuPercent;
    SystemMemoryPercent = stats.MemoryPercent;
  }

  // Cache the latest per-kind GPU core load from the sensor snapshot. Each load reading is matched
  // to its adapter by name to learn its kind; when several adapters share a kind (rare), the busiest
  // one wins, matching how the dashboard tile reports a kind's utilization. Runs on the UI thread.
  private void UpdateGpuStats(GpuSnapshot snapshot) {
    double? dedicated = null, integrated = null;
    foreach (var reading in snapshot.Loads) {
      var adapter = snapshot.Adapters.FirstOrDefault(a => a.Name == reading.AdapterName);
      if (adapter is null) continue;
      if (adapter.Kind == GpuKind.Dedicated)
        dedicated = Math.Max(dedicated ?? 0, reading.CoreLoadPercent);
      else
        integrated = Math.Max(integrated ?? 0, reading.CoreLoadPercent);
    }

    HasDedicatedGpu = dedicated is not null;
    HasIntegratedGpu = integrated is not null;
    DedicatedGpuPercent = dedicated ?? 0;
    IntegratedGpuPercent = integrated ?? 0;
  }

  private void OnUi(Action action) => _ui.Post(action);

  public void Dispose() {
    _subscription.Dispose();
    _statsSubscription.Dispose();
    _gpuSubscription?.Dispose();
    // Flush and close any in-progress recording so the file isn't left open if the view is torn down.
    _recorder.Stop();
  }
}
