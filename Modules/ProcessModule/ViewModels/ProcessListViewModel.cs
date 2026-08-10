using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using ProcessModule.Models;

namespace ProcessModule.ViewModels;

/// <summary>
/// Backs the Processes master-detail view: subscribes to the live sample stream and reconciles it
/// into a stable, PID-keyed row collection (add new, update existing in place, drop exited). Rows
/// are surfaced through <see cref="RowsView"/>, a grouped/sorted collection view: grouped into
/// Apps / Background Processes / Windows Processes, and sorted by whichever column the user clicked
/// (ascending, toggling to descending on a repeat click; CPU descending by default). The selected
/// row drives the detail panel; its live metrics keep updating in place while selected.
/// </summary>
public sealed class ProcessListViewModel : BindableBase, IDisposable {
  private readonly IDisposable _subscription;
  private readonly IDisposable _statsSubscription;
  private readonly Dictionary<uint, ProcessRowViewModel> _rowsByPid = new();
  private int _processCount;
  private int _threadCount;
  private int _handleCount;
  private int _hogCount;

  // PID of the process hosting this dashboard, used to preselect our own row on first load.
  private readonly uint _ownPid = (uint)Environment.ProcessId;
  private bool _hasSelectedDefault;

  private ProcessRowViewModel? _selectedRow;
  private string _sortProperty = nameof(ProcessRowViewModel.CpuPercent);
  private ListSortDirection _sortDirection = ListSortDirection.Descending;
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

  // Records the tracked process's per-poll readings to a CSV. Defaults to the real file-backed
  // recorder; tests inject a fake to assert on the writes without touching disk.
  private readonly IProcessRecorder _recorder;
  // PID being recorded, captured when recording starts so it keeps following that process even if
  // the selection moves to another row. Null when not recording.
  private uint? _recordingPid;
  private bool _isRecording;

  // clock, iconProvider, controller and recorder are optional so Unity's default registration works
  // (optional ctor params aren't injected); tests pass a fixed clock for a deterministic timestamp,
  // skip icons, and inject fakes.
  public ProcessListViewModel(IProcessModel model, SystemStatsMonitor systemStats,
                              ProcessIconProvider? iconProvider = null, Func<DateTimeOffset>? clock = null,
                              IProcessController? controller = null, IProcessRecorder? recorder = null) {
    _clock = clock ?? (() => DateTimeOffset.Now);
    _iconProvider = iconProvider;
    _controller = controller ?? new ProcessController();
    _recorder = recorder ?? new ProcessRecorder();
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
  }

  public ObservableCollection<ProcessRowViewModel> Rows { get; } = [];

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

  public string SortProperty => _sortProperty;
  public ListSortDirection SortDirection => _sortDirection;

  public ProcessRowViewModel? SelectedRow {
    get => _selectedRow;
    set {
      if (SetProperty(ref _selectedRow, value)) {
        RaisePropertyChanged(nameof(CanEndSelectedTask));
        RaisePropertyChanged(nameof(CanStartRecording));
      }
    }
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
  /// Begins recording the selected process's per-poll readings to <paramref name="filePath"/>. The
  /// recording follows the PID selected now, even if the selection later moves. No-op when nothing is
  /// selected or a recording is already running. On failure to open the file the reason is surfaced
  /// through <see cref="ActionStatus"/>.
  /// </summary>
  public void StartRecording(string filePath) {
    if (_isRecording || _selectedRow is not { } row) return;

    var result = _recorder.Start(filePath, MetricsStatusError);
    if (!result.Succeeded) {
      ActionStatus = result.Message;
      return;
    }

    _recordingPid = row.ProcessId;
    IsRecording = true;
    ActionStatus = $"Recording {row.Name} (PID {row.ProcessId}) → {System.IO.Path.GetFileName(filePath)}";
  }

  /// <summary>Stops the current recording and reports where it was saved and how many samples it
  /// captured. No-op when not recording.</summary>
  public void StopRecording() {
    if (!_isRecording) return;

    int samples = _recorder.SampleCount;
    string? file = _recorder.FilePath is { } p ? System.IO.Path.GetFileName(p) : null;
    _recorder.Stop();
    _recordingPid = null;
    IsRecording = false;
    ActionStatus = file is null
        ? $"Recording stopped ({samples} sample(s))"
        : $"Recording saved to {file} ({samples} sample(s))";
  }

  // The recorded process exited: close the file and report it, distinct from a user-initiated stop so
  // the user understands why recording ended on its own.
  private void StopRecordingOnExit() {
    int samples = _recorder.SampleCount;
    uint pid = _recordingPid ?? 0;
    string? file = _recorder.FilePath is { } p ? System.IO.Path.GetFileName(p) : null;
    _recorder.Stop();
    _recordingPid = null;
    IsRecording = false;
    ActionStatus = file is null
        ? $"Recording ended: PID {pid} exited ({samples} sample(s))"
        : $"Recording ended: PID {pid} exited — saved to {file} ({samples} sample(s))";
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
        _rowsByPid[s.ProcessId] = created;
        Rows.Add(created);
      }

      // Append this poll's reading for the process being recorded, from the same sample stream the
      // rows update from — no extra sensor work.
      if (_isRecording && s.ProcessId == _recordingPid) _recorder.WriteSample(s, _clock());
    }

    // The recorded process exited (its PID is absent this poll): end the recording cleanly rather
    // than leave it running against a gone process.
    if (_isRecording && _recordingPid is { } pid && !live.Contains(pid)) StopRecordingOnExit();

    // Drop rows for processes that are gone. Clear the selection if it was one of them.
    for (int i = Rows.Count - 1; i >= 0; i--) {
      if (!live.Contains(Rows[i].ProcessId)) {
        if (ReferenceEquals(SelectedRow, Rows[i])) SelectedRow = null;
        _rowsByPid.Remove(Rows[i].ProcessId);
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
  }

  private static void OnUi(Action action) {
    var dispatcher = Application.Current?.Dispatcher;
    if (dispatcher is null || dispatcher.CheckAccess()) action();
    else dispatcher.Invoke(action);
  }

  public void Dispose() {
    _subscription.Dispose();
    _statsSubscription.Dispose();
    // Flush and close any in-progress recording so the file isn't left open if the view is torn down.
    _recorder.Stop();
  }
}
