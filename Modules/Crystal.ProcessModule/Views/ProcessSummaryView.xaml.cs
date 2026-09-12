using Crystal.ProcessModule.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Crystal.ProcessModule.Views;

/// <summary>
/// The Processes master-detail view: a Task Manager-style live list grouped into Apps /
/// Background / Windows processes, with clickable sortable columns and a detail panel showing live
/// metrics for the selected process, styled to match the dashboard tiles.
/// </summary>
public partial class ProcessSummaryView : UserControl {
  /// <summary>
  /// Smallest a column may be dragged to; WPF's header gripper writes GridViewColumn.Width directly
  /// and ignores the header's MinWidth, so the floor is enforced by coercing Width below.
  /// </summary>
  private const double MinColumnWidth = 44;

  /// <summary>
  /// Default layout captured right after XAML load (before the user can resize anything), so "Reset
  /// view" can restore it without hard-coding the design widths in a second place.
  /// </summary>
  private readonly double[] _defaultColumnWidths;

  /// <summary>
  /// Default width for the master column (the process list) in the list/detail split.
  /// </summary>
  public GridLength DefaultMasterWidth { get; } = new(1, GridUnitType.Star);

  /// <summary>
  /// Default width for the detail column (the process metrics) in the list/detail split. Equal to
  /// the master column so the list and the detail panel (with its utilization graphs) start at the
  /// same width; the user can drag the splitter and that split is persisted across sessions.
  /// </summary>
  public GridLength DefaultDetailWidth { get; } = new(1, GridUnitType.Star);

  /// <summary>Persists the user-dragged list/detail split across sessions (best-effort JSON under
  /// %AppData%\Crystal). Restored on load; re-saved whenever the splitter drag completes.</summary>
  private readonly ProcessLayoutStore _layoutStore = new();

  /// <summary>
  /// Guards the Width-coercion re-entrancy: setting Width from inside the change handler would fire the
  /// handler again.
  /// </summary>
  private bool _coercingWidth;

  /// <summary>
  /// Initializes a new instance of the <see cref="ProcessSummaryView"/> class.
  /// </summary>
  public ProcessSummaryView() {
    InitializeComponent();
    _defaultColumnWidths = [.. ProcessGridView.Columns.Select(c => c.Width)];

    // Feed the detail-panel utilization graphs from the view model's rolling history collections.
    // DataSeries is a plain DependencyObject with no DataContext, so its ValuesSource can't be bound
    // in XAML (see DataSeries remarks) — it's wired here instead. AutoWireViewModel may set the
    // DataContext during or after InitializeComponent, so handle both: wire now if it's already set,
    // and again whenever it changes.
    DataContextChanged += (_, e) => {
      if (e.NewValue is ProcessListViewModel vm) WireGraphSeries(vm);
    };
    if (DataContext is ProcessListViewModel current) {
      WireGraphSeries(current);
    }

    // Watch each column's Width so a gripper drag below the floor is snapped back up. The header's
    // own MinWidth doesn't clamp the drag, so we coerce the property itself.
    var widthProperty = DependencyPropertyDescriptor.FromProperty(
        GridViewColumn.WidthProperty, typeof(GridViewColumn));
    foreach (var column in ProcessGridView.Columns) {
      widthProperty.AddValueChanged(column, OnColumnWidthChanged);
    }

    // Restore the list/detail split the user last dragged, overriding the equal-width default.
    if (_layoutStore.Load() is { } saved) {
      MasterColumn.Width = new GridLength(saved.MasterStar, GridUnitType.Star);
      DetailColumn.Width = new GridLength(saved.DetailStar, GridUnitType.Star);
    }
  }

  /// <summary>
  /// Persists the list/detail split once the user finishes dragging the splitter. Both columns stay
  /// star-sized, so saving their star weights preserves the ratio regardless of window width. Saving
  /// on drag-completed (rather than on exit) captures the change reliably even if the app is closed
  /// abruptly.
  /// </summary>
  /// <param name="sender">The sender of the event.</param>
  /// <param name="e">The event arguments.</param>
  private void OnSplitterDragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e) {
    if (MasterColumn.Width.IsStar && DetailColumn.Width.IsStar) {
      _layoutStore.Save(new ProcessSplitLayout {
        MasterStar = MasterColumn.Width.Value,
        DetailStar = DetailColumn.Width.Value,
      });
    }
  }

  /// <summary>
  /// Points each detail-panel graph series at its backing history collection on the view model. The
  /// "Total" lines are system-wide (summed across processes); the "Process" lines follow the
  /// selected process and are cleared by the VM when the selection changes.
  /// </summary>
  /// <param name="vm">The process list view model supplying the history collections.</param>
  private void WireGraphSeries(ProcessListViewModel vm) {
    CpuTotalSeries.ValuesSource = vm.TotalCpuHistory;
    CpuProcessSeries.ValuesSource = vm.SelectedCpuHistory;
    GpuTotalSeries.ValuesSource = vm.TotalGpuHistory;
    GpuProcessSeries.ValuesSource = vm.SelectedGpuHistory;
    MemoryTotalSeries.ValuesSource = vm.TotalMemoryHistory;
    MemoryProcessSeries.ValuesSource = vm.SelectedMemoryHistory;
    DiskTotalSeries.ValuesSource = vm.TotalDiskHistory;
    DiskProcessSeries.ValuesSource = vm.SelectedDiskHistory;
    NetTotalSeries.ValuesSource = vm.TotalNetHistory;
    NetProcessSeries.ValuesSource = vm.SelectedNetHistory;
  }

  /// <summary>
  /// Coerces a column's Width to the minimum if the user drags it below the floor. WPF's header
  /// </summary>
  /// <param name="sender">The sender of the event.</param>
  /// <param name="e">The event arguments.</param>
  private void OnColumnWidthChanged(object? sender, EventArgs e) {
    if (_coercingWidth || sender is not GridViewColumn column) {
      return;
    }

    if (double.IsNaN(column.Width) || column.Width >= MinColumnWidth) {
      return;
    }

    _coercingWidth = true;
    column.Width = MinColumnWidth;
    _coercingWidth = false;
  }

  /// <summary>
  /// Restores the grid column widths and the list/detail split to their XAML defaults,
  /// undoing any header-drag or splitter-drag the user has done. Layout-only; touches no process
  /// state. Invoked from the Shell's title-bar Reset-layout button (which also resets the
  /// dashboard), so the whole layout resets from one place.
  /// </summary>
  public void ResetLayout() {
    var columns = ProcessGridView.Columns;
    for (int i = 0; i < columns.Count && i < _defaultColumnWidths.Length; i++) {
      columns[i].Width = _defaultColumnWidths[i];
    }

    MasterColumn.Width = DefaultMasterWidth;
    DetailColumn.Width = DefaultDetailWidth;
  }

  /// <summary>
  /// The column currently showing a sort-direction arrow and its undecorated header text, so the
  /// glyph can be stripped before the next click (the sort key lives in the attached property, not
  /// the header text, so the decoration is display-only) and moved to the newly-sorted column.
  /// </summary>
  private GridViewColumn? _sortedColumn;

  /// <summary>
  /// The original header text of the column currently showing a sort-direction arrow, so the glyph
  /// can be stripped before the next click.
  /// </summary>
  private object? _sortedBaseHeader;

  /// <summary>
  /// Handles a column-header click by sorting the list by that column, toggling ascending/descending.
  /// Clicking a column header sorts the list by that column (toggling asc/desc); the sort key lives
  /// on the column via GridViewSort.SortProperty. A ▲/▼ arrow marks the active column and direction.
  /// </summary>
  /// <param name="sender">The sender of the event.</param>
  /// <param name="e">The event arguments.</param>
  private void OnColumnHeaderClick(object sender, RoutedEventArgs e) {
    if (e.OriginalSource is not GridViewColumnHeader header) {
      return;
    }

    if (header.Column is null) {
      return;
    }

    var sortProperty = GridViewSort.GetSortProperty(header.Column);

    if (string.IsNullOrEmpty(sortProperty)) {
      return;
    }

    if (DataContext is not ProcessListViewModel vm) {
      return;
    }

    // Restore any previously-arrowed header before decorating the new one.
    _sortedColumn?.Header = _sortedBaseHeader;
    vm.SortBy(sortProperty);
    string arrow = vm.SortDirection == ListSortDirection.Ascending ? " ▲" : " ▼";
    _sortedBaseHeader = header.Column.Header;
    header.Column.Header = (_sortedBaseHeader as string ?? "") + arrow;
    _sortedColumn = header.Column;
  }

  /// <summary>
  /// Copies the visible rows to the clipboard as tab-separated text. Guarded because the clipboard
  /// can transiently throw if another process holds it open; a failed copy is a no-op rather than a
  /// crash. Nothing to copy is a no-op too (the button is disabled when the list is empty).
  /// </summary>
  /// <param name="sender">The sender of the event.</param>
  /// <param name="e">The event arguments.</param>
  private void OnCopyRows(object sender, RoutedEventArgs e) {
    if (DataContext is not ProcessListViewModel vm) {
      return;
    }

    string text = vm.RowsAsText();
    if (string.IsNullOrEmpty(text)) {
      return;
    }

    try {
      Clipboard.SetText(text);
    }
    catch (System.Runtime.InteropServices.COMException) {
    }
  }

  /// <summary>
  /// Writes the visible rows to a file the user picks — the same tab-separated text as Copy, so a
  /// process snapshot can be archived or attached to a report. The save dialog and file write are
  /// UI/IO side effects, so they live here; the text itself comes from the (tested) view model. A
  /// failed write (permissions, disk) is swallowed rather than crashing the dashboard.
  /// </summary>
  /// <param name="sender">The sender of the event.</param>
  /// <param name="e">The event arguments.</param>
  private void OnSaveRows(object sender, RoutedEventArgs e) {
    if (DataContext is not ProcessListViewModel vm) {
      return;
    }

    string text = vm.RowsAsText();
    if (string.IsNullOrEmpty(text)) {
      return;
    }

    var dialog = new Microsoft.Win32.SaveFileDialog {
      Title = "Save process list",
      Filter = "Tab-separated values (*.tsv)|*.tsv|CSV (*.csv)|*.csv|Text (*.txt)|*.txt",
      DefaultExt = ".tsv",
      FileName = $"processes-{System.DateTime.Now:yyyy-MM-dd-HHmmss}.tsv",
    };
    if (dialog.ShowDialog() != true) {
      return;
    }

    try {
      System.IO.File.WriteAllText(dialog.FileName, text);
    }
    catch (System.IO.IOException) {
    }
    catch (System.UnauthorizedAccessException) {
    }
  }

  /// <summary>
  /// Resets every row's session peaks to its current reading. Non-destructive (peaks re-establish 
  /// on the next poll) so there's no confirmation prompt; the reset itself is testable on the VM.
  /// </summary>
  /// <param name="sender">The sender of the event.</param>
  /// <param name="e">The event arguments.</param>
  private void OnResetPeaks(object sender, RoutedEventArgs e) {
    if (DataContext is ProcessListViewModel vm) {
      vm.ResetAllPeaks();
    }
  }

  /// <summary>
  /// Terminates the selected process after a confirmation prompt — ending a process is destructive
  /// and can lose unsaved work, so we always confirm (this is the one place in the module that acts
  /// on the machine, not just the view). The actual kill and any failure message live on the VM.
  /// </summary>
  /// <param name="sender">The sender of the event.</param>
  /// <param name="e">The event arguments.</param>
  private void OnEndTask(object sender, RoutedEventArgs e) {
    if (DataContext is not ProcessListViewModel vm) {
      return;
    }

    if (vm.SelectedRow is not { } row) {
      return;
    }

    var answer = MessageBox.Show(
        $"End \"{row.Name}\" (PID {row.ProcessId})?\n\nUnsaved data will be lost.",
        "End task", MessageBoxButton.OKCancel, MessageBoxImage.Warning, MessageBoxResult.Cancel);
    if (answer != MessageBoxResult.OK) {
      return;
    }

    vm.EndSelectedTask();
  }

  /// <summary>
  /// Prompts for a command line and launches it, like Task Manager's "Run new task". The prompt is a
  /// small modal window (WPF has no built-in input box); the launch and any failure message live on
  /// the VM. Cancelling the dialog is a no-op.
  /// </summary>
  /// <param name="sender">The sender of the event.</param>
  /// <param name="e">The event arguments.</param>
  private void OnRunNewTask(object sender, RoutedEventArgs e) {
    if (DataContext is not ProcessListViewModel vm) {
      return;
    }

    var dialog = new RunNewTaskDialog { Owner = Window.GetWindow(this) };
    if (dialog.ShowDialog() != true) {
      return;
    }

    vm.StartTask(dialog.Command, dialog.RunAsAdmin);
  }

  /// <summary>
  /// Toggles recording of the selected process's per-poll readings to a CSV. Starting prompts for a
  /// save location (the same UX as the Save button); the file write itself lives in the recorder on
  /// the VM. Stopping is a plain VM call. Cancelling the save dialog leaves recording off.
  /// </summary>
  /// <param name="sender">The sender of the event.</param>
  /// <param name="e">The event arguments.</param>
  private void OnToggleRecord(object sender, RoutedEventArgs e) {
    if (DataContext is not ProcessListViewModel vm) {
      return;
    }

    if (vm.IsRecording) {
      vm.StopRecording();
      return;
    }

    if (vm.SelectedRow is not { } row) {
      return;
    }

    var dialog = new Microsoft.Win32.SaveFileDialog {
      Title = "Record process to file",
      Filter = "CSV (*.csv)|*.csv|Text (*.txt)|*.txt",
      DefaultExt = ".csv",
      FileName = $"record-{SafeName(row.Name)}-{row.ProcessId}-{System.DateTime.Now:yyyy-MM-dd-HHmmss}.csv",
    };

    if (dialog.ShowDialog() != true) {
      return;
    }

    vm.StartRecording(dialog.FileName);
  }

  /// <summary>
  /// Strips characters that Windows won't allow in a filename so the suggested default name is always valid.
  /// </summary>
  /// <param name="name">The original filename.</param>
  /// <returns>The sanitized filename.</returns>
  private static string SafeName(string name) =>
      string.Concat(name.Split(System.IO.Path.GetInvalidFileNameChars()));

  /// <summary>
  /// Gets the ProcessRowViewModel associated with the context menu that was opened.
  /// The row the context menu was opened on. The menu is shared across rows, so its DataContext is
  /// the row VM WPF placed on it when the row was right-clicked.
  /// </summary>
  /// <param name="sender">The sender of the event.</param>
  /// <returns>The ProcessRowViewModel associated with the context menu.</returns>
  private static ProcessRowViewModel? MenuRow(object sender) =>
      (sender as FrameworkElement)?.DataContext as ProcessRowViewModel;

  /// <summary>
  /// Opens Explorer with the process's image selected, like Task Manager's "Open file location".
  /// No-op (with a status message) when the path is unknown — protected processes hide it.
  /// </summary>
  /// <param name="sender">The sender of the event.</param>
  /// <param name="e">The event arguments.</param>
  private void OnOpenFileLocation(object sender, RoutedEventArgs e) {
    if (DataContext is not ProcessListViewModel vm) {
      return;
    }

    if (MenuRow(sender) is not { } row) {
      return;
    }

    vm.OpenFileLocation(row.ExecutablePath);
  }

  /// <summary>
  /// Copies the process name to the clipboard. Guarded because the clipboard can transiently throw
  /// </summary>
  /// <param name="sender">The sender of the event.</param>
  /// <param name="e">The event arguments.</param>
  private void OnCopyName(object sender, RoutedEventArgs e) {
    if (MenuRow(sender) is { } row) {
      TrySetClipboard(row.Name);
    }
  }

  /// <summary>
  /// Copies the process ID to the clipboard. Guarded because the clipboard can transiently throw
  /// </summary>
  /// <param name="sender">The sender of the event.</param>
  /// <param name="e">The event arguments.</param>
  private void OnCopyPid(object sender, RoutedEventArgs e) {
    if (MenuRow(sender) is { } row) {
      TrySetClipboard(row.ProcessId.ToString());
    }
  }

  /// <summary>
  /// Copies the process image path to the clipboard. Guarded because the clipboard can transiently throw.
  /// </summary>
  /// <param name="sender">The sender of the event.</param>
  /// <param name="e">The event arguments.</param>
  private void OnCopyImagePath(object sender, RoutedEventArgs e) {
    if (MenuRow(sender) is { } row && !string.IsNullOrEmpty(row.ExecutablePath)) {
      TrySetClipboard(row.ExecutablePath);
    }
  }

  /// <summary>
  /// Attempts to set the clipboard text. Guarded because the clipboard can transiently throw.
  /// The clipboard can transiently throw if another process holds it open; a failed copy is a no-op
  /// rather than a crash (same guard as the header Copy button).
  /// </summary>
  /// <param name="text">The text to set on the clipboard.</param>
  private static void TrySetClipboard(string text) {
    if (string.IsNullOrEmpty(text)) {
      return;
    }

    try {
      Clipboard.SetText(text);
    }
    catch (System.Runtime.InteropServices.COMException) {
    }
  }
}
