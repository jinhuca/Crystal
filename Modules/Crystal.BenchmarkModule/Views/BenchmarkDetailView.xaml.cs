using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Crystal.BenchmarkModule.ViewModels;
using Microsoft.Win32;

namespace Crystal.BenchmarkModule.Views;

/// <summary>
/// The benchmark dashboard (the module's summary view): lists CPU / GPU / Memory / Storage suites
/// grouped by category, each with a selection checkbox, live progress bar, and score. Opened in its
/// own window from the shell title bar and the Processes detail toolbar.
/// </summary>
public partial class BenchmarkDetailView : UserControl {
  public BenchmarkDetailView() {
    InitializeComponent();
  }

  // Exports the last run's results to a CSV document. The dialog/IO live here (a UI concern, matching
  // the Processes view's Save button); the CSV itself is built by the view model.
  private void OnSaveReport(object sender, RoutedEventArgs e) {
    if (DataContext is not BenchmarkDashboardViewModel vm || !vm.CanExportReport) return;

    var dialog = new SaveFileDialog {
      Title = "Save benchmark report",
      Filter = "CSV (*.csv)|*.csv|Text (*.txt)|*.txt",
      DefaultExt = ".csv",
      FileName = $"benchmark-{DateTime.Now:yyyyMMdd-HHmmss}.csv",
    };
    if (dialog.ShowDialog() != true) return;

    try {
      File.WriteAllText(dialog.FileName, vm.BuildReportCsv());
    } catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) {
      MessageBox.Show(Window.GetWindow(this), $"Could not save the report:\n{ex.Message}",
          "Save benchmark report", MessageBoxButton.OK, MessageBoxImage.Warning);
    }
  }
}
