using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using BiosModule.ViewModels;
using Crystal.Controls.PerformanceGraphs;
using Crystal.Controls.PerformanceGraphs.Kinds;
using Crystal.Controls.PerformanceGraphs.Themes;

namespace BiosModule.Views;

/// <summary>Full-scale BIOS view: complete firmware identity. Reached by selecting the BIOS
/// summary strip; the Back control returns to the dashboard.</summary>
public partial class BiosDetailView : UserControl {
  public BiosDetailView() {
    InitializeComponent();
  }

  // Per-graph rather than a root Loaded handler: a root-element Loaded collides with the
  // BiosModule namespace/class name in WPF's generated code. The VM attach is idempotent.
  private void OnGraphLoaded(object sender, RoutedEventArgs e) {
    if (sender is PerformanceGraph graph)
      graph.ApplyTheme(GraphThemes.Sky(GraphKind.Line));
    if (DataContext is IBiosViewModel vm)
      vm.AttachRailGraphs(Rail3V3Graph, Rail5VGraph, Rail12VGraph);
  }

  private void OnFanGraphLoaded(object sender, RoutedEventArgs e) {
    if (sender is PerformanceGraph graph)
      graph.ApplyTheme(GraphThemes.Sky(GraphKind.Line));
    if (DataContext is IBiosViewModel vm)
      vm.AttachFanGraph(FanGraph);
  }

  private void OnBoardTempGraphLoaded(object sender, RoutedEventArgs e) {
    if (sender is PerformanceGraph graph)
      graph.ApplyTheme(GraphThemes.Sky(GraphKind.Line));
    if (DataContext is IBiosViewModel vm)
      vm.AttachBoardTempGraph(BoardTempGraph);
  }

  // Copies the session health-event log to the clipboard as tab-separated text. Guarded because the
  // clipboard can transiently throw if another process holds it open; a failed copy is a no-op rather
  // than a crash. Nothing to copy is a no-op too (the button only shows when events exist).
  private void OnCopyHealthEvents(object sender, RoutedEventArgs e) {
    if (DataContext is not IBiosViewModel vm) return;
    string text = vm.HealthEventsAsText();
    if (string.IsNullOrEmpty(text)) return;
    try {
      Clipboard.SetText(text);
    }
    catch (System.Runtime.InteropServices.COMException) {
    }
  }

  // Writes the log to a file the user picks — the same tab-separated text as Copy, so a fault trail
  // can be archived or attached to a bug report. The save dialog and file write are UI/IO side
  // effects, so they live here; the text itself comes from the (tested) view model. A failed write
  // (permissions, disk) is swallowed rather than crashing the detail window; nothing to save is a
  // no-op (the button is only shown when events exist).
  private void OnSaveHealthEvents(object sender, RoutedEventArgs e) {
    if (DataContext is not IBiosViewModel vm) return;
    string text = vm.HealthEventsAsText();
    if (string.IsNullOrEmpty(text)) return;
    // Stamp the default name with the local date/time so successive saves land as distinct files
    // instead of prompting to overwrite the last one. The user can still rename in the dialog.
    var dialog = new Microsoft.Win32.SaveFileDialog {
      Title = "Save board health log",
      Filter = "Tab-separated values (*.tsv)|*.tsv|CSV (*.csv)|*.csv|Text (*.txt)|*.txt",
      DefaultExt = ".tsv",
      FileName = $"board-health-log-{System.DateTime.Now:yyyy-MM-dd-HHmmss}.tsv",
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

  // Clearing the log is destructive and irreversible — it drops the session's accumulated
  // out-of-spec trail, which can't be rebuilt from live sensors. So a stray click confirms first.
  // The confirmation lives here (a UI side effect) while the reset itself stays testable in the VM.
  // When the log is empty there's nothing to lose, so skip the prompt and just reset the graphs.
  private void OnClearHistory(object sender, RoutedEventArgs e) {
    if (DataContext is not IBiosViewModel vm) return;
    if (vm.HasHealthEvents) {
      var answer = MessageBox.Show(
          $"Clear the board health log?\n\n{vm.HealthEventsSummary} will be discarded and can't be recovered.",
          "Clear history", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
      if (answer != MessageBoxResult.Yes) return;
    }
    if (vm.ClearHistoryCommand.CanExecute(null))
      vm.ClearHistoryCommand.Execute(null);
  }

  private string? _boardSortProperty;
  private ListSortDirection _boardSortDirection = ListSortDirection.Ascending;
  // Mirror of the event table's arrow tracking: the currently-arrowed column and its base header,
  // so the glyph can be stripped before the next header match and moved to the newly-sorted column.
  private GridViewColumn? _boardSortedColumn;
  private string? _boardSortedBaseHeader;

  // Re-sorts the board-sensor table by the clicked column. Value/Min/Max sort on their numeric
  // keys (not the formatted string) so ordering is by magnitude; Sensor sorts by name. Clicking the
  // same column again flips direction. The view model's worst-first default stands until first click.
  private void OnBoardSensorHeaderClick(object sender, RoutedEventArgs e) {
    if (e.OriginalSource is not GridViewColumnHeader header || header.Column is null) return;

    // Strip any prior arrow so the switch matches on the undecorated header name.
    if (_boardSortedColumn is not null && _boardSortedBaseHeader is not null)
      _boardSortedColumn.Header = _boardSortedBaseHeader;

    string baseHeader = header.Column.Header as string ?? "";
    string? property = baseHeader switch {
      "Sensor" => nameof(BoardSensorRowViewModel.Name),
      "Value" => nameof(BoardSensorRowViewModel.ValueSort),
      "Min" => nameof(BoardSensorRowViewModel.MinSort),
      "Max" => nameof(BoardSensorRowViewModel.MaxSort),
      _ => null,
    };
    if (property is null) { _boardSortedColumn = null; _boardSortedBaseHeader = null; return; }

    _boardSortDirection = property == _boardSortProperty && _boardSortDirection == ListSortDirection.Ascending
        ? ListSortDirection.Descending
        : ListSortDirection.Ascending;
    _boardSortProperty = property;

    var view = CollectionViewSource.GetDefaultView(BoardSensorList.ItemsSource);
    view.SortDescriptions.Clear();
    view.SortDescriptions.Add(new SortDescription(property, _boardSortDirection));
    view.Refresh();

    string arrow = _boardSortDirection == ListSortDirection.Ascending ? " ▲" : " ▼";
    header.Column.Header = baseHeader + arrow;
    _boardSortedColumn = header.Column;
    _boardSortedBaseHeader = baseHeader;
  }

  private string? _eventSortProperty;
  private ListSortDirection _eventSortDirection = ListSortDirection.Ascending;
  // The column currently showing a sort-direction arrow and its undecorated header text, so the
  // arrow can be stripped before the next header match (the match switches on the base text) and
  // moved to the newly-sorted column.
  private GridViewColumn? _eventSortedColumn;
  private string? _eventSortedBaseHeader;

  // Re-sorts the health-event table by the clicked column. Started/Duration sort on their numeric
  // keys (ticks/seconds), Peak on the severity enum, and Sensor by name; clicking the same column
  // again flips direction. The sort lives on the collection view, so it survives the per-tick rebuild
  // of HealthEvents. The view model's ongoing-first default stands until the first click.
  private void OnHealthEventHeaderClick(object sender, RoutedEventArgs e) {
    if (e.OriginalSource is not GridViewColumnHeader header || header.Column is null) return;

    // Restore any previously-arrowed header to its base text first, so the switch below matches on
    // the undecorated column name rather than a stale "Started ▲".
    if (_eventSortedColumn is not null && _eventSortedBaseHeader is not null)
      _eventSortedColumn.Header = _eventSortedBaseHeader;

    string baseHeader = header.Column.Header as string ?? "";
    string? property = baseHeader switch {
      "Started" => nameof(BoardHealthEventViewModel.StartedSort),
      "Age" => nameof(BoardHealthEventViewModel.StartedSort),  // age is just start time relative to now
      "Sensor" => nameof(BoardHealthEventViewModel.SensorName),
      "Peak" => nameof(BoardHealthEventViewModel.Severity),
      "Peak at" => nameof(BoardHealthEventViewModel.PeakAtSort),
      "Duration" => nameof(BoardHealthEventViewModel.DurationSort),
      _ => null,  // Reading has no natural order (mixed units) → not sortable
    };
    if (property is null) { _eventSortedColumn = null; _eventSortedBaseHeader = null; return; }

    _eventSortDirection = property == _eventSortProperty && _eventSortDirection == ListSortDirection.Ascending
        ? ListSortDirection.Descending
        : ListSortDirection.Ascending;
    _eventSortProperty = property;

    var view = CollectionViewSource.GetDefaultView(HealthEventList.ItemsSource);
    view.SortDescriptions.Clear();
    view.SortDescriptions.Add(new SortDescription(property, _eventSortDirection));
    view.Refresh();

    // Mark the sorted column with a direction arrow. Age and Started share a sort key, so the arrow
    // lands on whichever header was actually clicked.
    string arrow = _eventSortDirection == ListSortDirection.Ascending ? " ▲" : " ▼";
    header.Column.Header = baseHeader + arrow;
    _eventSortedColumn = header.Column;
    _eventSortedBaseHeader = baseHeader;
  }
}
