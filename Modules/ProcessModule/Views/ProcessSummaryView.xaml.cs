using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using ProcessModule.ViewModels;

namespace ProcessModule.Views;

/// <summary>The Processes master-detail view: a Task Manager-style live list grouped into Apps /
/// Background / Windows processes, with clickable sortable columns and a detail panel showing live
/// metrics for the selected process, styled to match the dashboard tiles.</summary>
public partial class ProcessSummaryView : UserControl {
  public ProcessSummaryView() => InitializeComponent();

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
}
