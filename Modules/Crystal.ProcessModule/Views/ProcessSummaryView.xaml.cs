using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Crystal.ProcessModule.ViewModels;

namespace Crystal.ProcessModule.Views;

/// <summary>The Processes master-detail view: a Task Manager-style live list grouped into Apps /
/// Background / Windows processes, with clickable sortable columns and a detail panel showing live
/// metrics for the selected process, styled to match the dashboard tiles.</summary>
public partial class ProcessSummaryView : UserControl {
  // Smallest a column may be dragged to; WPF's header gripper writes GridViewColumn.Width directly
  // and ignores the header's MinWidth, so the floor is enforced by coercing Width below.
  private const double MinColumnWidth = 44;

  // Default layout captured right after XAML load (before the user can resize anything), so "Reset
  // view" can restore it without hard-coding the design widths in a second place.
  private readonly double[] _defaultColumnWidths;
  private readonly GridLength _defaultMasterWidth;
  private readonly GridLength _defaultDetailWidth;

  // Guards the Width-coercion re-entrancy: setting Width from inside the change handler would fire the
  // handler again.
  private bool _coercingWidth;

  public ProcessSummaryView() {
    InitializeComponent();

    _defaultColumnWidths = [.. ProcessGridView.Columns.Select(c => c.Width)];
    _defaultMasterWidth = MasterColumn.Width;
    _defaultDetailWidth = DetailColumn.Width;

    // Watch each column's Width so a gripper drag below the floor is snapped back up. The header's
    // own MinWidth doesn't clamp the drag, so we coerce the property itself.
    var widthProperty = DependencyPropertyDescriptor.FromProperty(
        GridViewColumn.WidthProperty, typeof(GridViewColumn));
    foreach (var column in ProcessGridView.Columns)
      widthProperty.AddValueChanged(column, OnColumnWidthChanged);
  }

  private void OnColumnWidthChanged(object? sender, EventArgs e) {
    if (_coercingWidth || sender is not GridViewColumn column) return;
    if (double.IsNaN(column.Width) || column.Width >= MinColumnWidth) return;
    _coercingWidth = true;
    column.Width = MinColumnWidth;
    _coercingWidth = false;
  }

  /// <summary>Restores the grid column widths and the list/detail split to their XAML defaults,
  /// undoing any header-drag or splitter-drag the user has done. Layout-only; touches no process
  /// state. Invoked from the Shell's title-bar Reset-layout button (which also resets the
  /// dashboard), so the whole layout resets from one place.</summary>
  public void ResetLayout() {
    var columns = ProcessGridView.Columns;
    for (int i = 0; i < columns.Count && i < _defaultColumnWidths.Length; i++)
      columns[i].Width = _defaultColumnWidths[i];
    MasterColumn.Width = _defaultMasterWidth;
    DetailColumn.Width = _defaultDetailWidth;
  }

  // The column currently showing a sort-direction arrow and its undecorated header text, so the
  // glyph can be stripped before the next click (the sort key lives in the attached property, not
  // the header text, so the decoration is display-only) and moved to the newly-sorted column.
  private GridViewColumn? _sortedColumn;
  private object? _sortedBaseHeader;

  // Clicking a column header sorts the list by that column (toggling asc/desc); the sort key lives
  // on the column via GridViewSort.SortProperty. A ▲/▼ arrow marks the active column and direction.
  private void OnColumnHeaderClick(object sender, RoutedEventArgs e) {
    if (e.OriginalSource is not GridViewColumnHeader header) return;
    if (header.Column is null) return;
    var sortProperty = GridViewSort.GetSortProperty(header.Column);
    if (string.IsNullOrEmpty(sortProperty)) return;
    if (DataContext is not ProcessListViewModel vm) return;

    // Restore any previously-arrowed header before decorating the new one.
    if (_sortedColumn is not null) _sortedColumn.Header = _sortedBaseHeader;

    vm.SortBy(sortProperty);

    string arrow = vm.SortDirection == ListSortDirection.Ascending ? " ▲" : " ▼";
    _sortedBaseHeader = header.Column.Header;
    header.Column.Header = (_sortedBaseHeader as string ?? "") + arrow;
    _sortedColumn = header.Column;
  }

  // Copies the visible rows to the clipboard as tab-separated text. Guarded because the clipboard
  // can transiently throw if another process holds it open; a failed copy is a no-op rather than a
  // crash. Nothing to copy is a no-op too (the button is disabled when the list is empty).
  private void OnCopyRows(object sender, RoutedEventArgs e) {
    if (DataContext is not ProcessListViewModel vm) return;
    string text = vm.RowsAsText();
    if (string.IsNullOrEmpty(text)) return;
    try {
      Clipboard.SetText(text);
    }
    catch (System.Runtime.InteropServices.COMException) {
    }
  }

  // Writes the visible rows to a file the user picks — the same tab-separated text as Copy, so a
  // process snapshot can be archived or attached to a report. The save dialog and file write are
  // UI/IO side effects, so they live here; the text itself comes from the (tested) view model. A
  // failed write (permissions, disk) is swallowed rather than crashing the dashboard.
  private void OnSaveRows(object sender, RoutedEventArgs e) {
    if (DataContext is not ProcessListViewModel vm) return;
    string text = vm.RowsAsText();
    if (string.IsNullOrEmpty(text)) return;
    var dialog = new Microsoft.Win32.SaveFileDialog {
      Title = "Save process list",
      Filter = "Tab-separated values (*.tsv)|*.tsv|CSV (*.csv)|*.csv|Text (*.txt)|*.txt",
      DefaultExt = ".tsv",
      FileName = $"processes-{System.DateTime.Now:yyyy-MM-dd-HHmmss}.tsv",
    };
    if (dialog.ShowDialog() != true) return;
    try {
      System.IO.File.WriteAllText(dialog.FileName, text);
    }
    catch (System.IO.IOException) {
    }
    catch (System.UnauthorizedAccessException) {
    }
  }

  // Resets every row's session peaks to its current reading. Non-destructive (peaks re-establish on
  // the next poll) so there's no confirmation prompt; the reset itself is testable on the VM.
  private void OnResetPeaks(object sender, RoutedEventArgs e) {
    if (DataContext is ProcessListViewModel vm) vm.ResetAllPeaks();
  }

  // Terminates the selected process after a confirmation prompt — ending a process is destructive
  // and can lose unsaved work, so we always confirm (this is the one place in the module that acts
  // on the machine, not just the view). The actual kill and any failure message live on the VM.
  private void OnEndTask(object sender, RoutedEventArgs e) {
    if (DataContext is not ProcessListViewModel vm) return;
    if (vm.SelectedRow is not { } row) return;

    var answer = MessageBox.Show(
        $"End \"{row.Name}\" (PID {row.ProcessId})?\n\nUnsaved data will be lost.",
        "End task", MessageBoxButton.OKCancel, MessageBoxImage.Warning, MessageBoxResult.Cancel);
    if (answer != MessageBoxResult.OK) return;

    vm.EndSelectedTask();
  }

  // Prompts for a command line and launches it, like Task Manager's "Run new task". The prompt is a
  // small modal window (WPF has no built-in input box); the launch and any failure message live on
  // the VM. Cancelling the dialog is a no-op.
  private void OnRunNewTask(object sender, RoutedEventArgs e) {
    if (DataContext is not ProcessListViewModel vm) return;

    var dialog = new RunNewTaskDialog { Owner = Window.GetWindow(this) };
    if (dialog.ShowDialog() != true) return;

    vm.StartTask(dialog.Command, dialog.RunAsAdmin);
  }

  // Toggles recording of the selected process's per-poll readings to a CSV. Starting prompts for a
  // save location (the same UX as the Save button); the file write itself lives in the recorder on
  // the VM. Stopping is a plain VM call. Cancelling the save dialog leaves recording off.
  private void OnToggleRecord(object sender, RoutedEventArgs e) {
    if (DataContext is not ProcessListViewModel vm) return;

    if (vm.IsRecording) {
      vm.StopRecording();
      return;
    }

    if (vm.SelectedRow is not { } row) return;
    var dialog = new Microsoft.Win32.SaveFileDialog {
      Title = "Record process to file",
      Filter = "CSV (*.csv)|*.csv|Text (*.txt)|*.txt",
      DefaultExt = ".csv",
      FileName = $"record-{SafeName(row.Name)}-{row.ProcessId}-{System.DateTime.Now:yyyy-MM-dd-HHmmss}.csv",
    };
    if (dialog.ShowDialog() != true) return;

    vm.StartRecording(dialog.FileName);
  }

  // Strip characters Windows won't allow in a filename so the suggested default name is always valid.
  private static string SafeName(string name) =>
      string.Concat(name.Split(System.IO.Path.GetInvalidFileNameChars()));

  // The row the context menu was opened on. The menu is shared across rows, so its DataContext is
  // the row VM WPF placed on it when the row was right-clicked.
  private static ProcessRowViewModel? MenuRow(object sender) =>
      (sender as FrameworkElement)?.DataContext as ProcessRowViewModel;

  // Opens Explorer with the process's image selected, like Task Manager's "Open file location".
  // No-op (with a status message) when the path is unknown — protected processes hide it.
  private void OnOpenFileLocation(object sender, RoutedEventArgs e) {
    if (DataContext is not ProcessListViewModel vm) return;
    if (MenuRow(sender) is not { } row) return;
    vm.OpenFileLocation(row.ExecutablePath);
  }

  private void OnCopyName(object sender, RoutedEventArgs e) {
    if (MenuRow(sender) is { } row) TrySetClipboard(row.Name);
  }

  private void OnCopyPid(object sender, RoutedEventArgs e) {
    if (MenuRow(sender) is { } row) TrySetClipboard(row.ProcessId.ToString());
  }

  private void OnCopyImagePath(object sender, RoutedEventArgs e) {
    if (MenuRow(sender) is { } row && !string.IsNullOrEmpty(row.ExecutablePath))
      TrySetClipboard(row.ExecutablePath);
  }

  // The clipboard can transiently throw if another process holds it open; a failed copy is a no-op
  // rather than a crash (same guard as the header Copy button).
  private static void TrySetClipboard(string text) {
    if (string.IsNullOrEmpty(text)) return;
    try {
      Clipboard.SetText(text);
    }
    catch (System.Runtime.InteropServices.COMException) {
    }
  }
}
