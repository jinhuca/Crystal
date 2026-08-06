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
  private readonly Dictionary<uint, ProcessRowViewModel> _rowsByPid = new();

  // PID of the process hosting this dashboard, used to preselect our own row on first load.
  private readonly uint _ownPid = (uint)Environment.ProcessId;
  private bool _hasSelectedDefault;

  private ProcessRowViewModel? _selectedRow;
  private string _sortProperty = nameof(ProcessRowViewModel.CpuPercent);
  private ListSortDirection _sortDirection = ListSortDirection.Descending;
  private string _nameFilter = string.Empty;
  private string _pidFilter = string.Empty;

  public ProcessListViewModel(IProcessModel model) {
    MetricsStatusError = model.MetricsStatusError;

    RowsView = new ListCollectionView(Rows);
    // Group into the three categories; the enum's order (App, Background, Windows) is used to
    // order the groups themselves via the leading sort description below.
    RowsView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(ProcessRowViewModel.CategoryName)));
    RowsView.Filter = MatchesFilters;
    ApplySortDescriptions();

    _subscription = model.Processes.Subscribe(samples => OnUi(() => Apply(samples)));
  }

  public ObservableCollection<ProcessRowViewModel> Rows { get; } = [];

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
    set => SetProperty(ref _selectedRow, value);
  }

  /// <summary>Case-insensitive substring filter on the process name; empty shows all. Bound to the
  /// search box in the Name column header.</summary>
  public string NameFilter {
    get => _nameFilter;
    set { if (SetProperty(ref _nameFilter, value ?? string.Empty)) RowsView.Refresh(); }
  }

  /// <summary>Substring filter on the PID (matched against its decimal text); empty shows all. Bound
  /// to the search box in the PID column header.</summary>
  public string PidFilter {
    get => _pidFilter;
    set { if (SetProperty(ref _pidFilter, value ?? string.Empty)) RowsView.Refresh(); }
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
    }

    // Drop rows for processes that are gone. Clear the selection if it was one of them.
    for (int i = Rows.Count - 1; i >= 0; i--) {
      if (!live.Contains(Rows[i].ProcessId)) {
        if (ReferenceEquals(SelectedRow, Rows[i])) SelectedRow = null;
        _rowsByPid.Remove(Rows[i].ProcessId);
        Rows.RemoveAt(i);
      }
    }

    // Live metric values changed in place, so re-apply grouping/sorting for this poll.
    RowsView.Refresh();

    // Default the selection to this app's own process once it first appears in the list.
    if (!_hasSelectedDefault && _rowsByPid.TryGetValue(_ownPid, out var ownRow)) {
      SelectedRow = ownRow;
      _hasSelectedDefault = true;
    }
  }

  private static void OnUi(Action action) {
    var dispatcher = Application.Current?.Dispatcher;
    if (dispatcher is null || dispatcher.CheckAccess()) action();
    else dispatcher.Invoke(action);
  }

  public void Dispose() => _subscription.Dispose();
}
