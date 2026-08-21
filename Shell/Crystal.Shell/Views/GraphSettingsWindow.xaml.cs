using Crystal.Shell.Settings;
using Crystal.Shell.ViewModels;
using System.Windows;
using System.Windows.Media;

namespace Crystal.Shell.Views;

/// <summary>
/// Modal popup for choosing the dashboard graph category (No Frills / Full Graph) and, per graph,
/// its kind (SegmentedBar / FilledLine) and accent colour. Edits a view-model built from the
/// persisted settings; Save writes back through the store, Cancel/Close discards.
/// </summary>
public partial class GraphSettingsWindow : Window {
  private readonly GraphSettingsStore _store;
  private readonly GraphSettingsViewModel _viewModel;

  public GraphSettingsWindow(GraphSettingsStore store) {
    _store = store;
    InitializeComponent();
    _viewModel = new GraphSettingsViewModel(store.Current);
    DataContext = _viewModel;
  }

  // The rainbow picker opens the in-app colour dialog for the clicked row.
  private void OnPickCustomColorClick(object sender, RoutedEventArgs e) {
    if (sender is not FrameworkElement { DataContext: GraphRowViewModel row }) return;

    var dialog = new ColorPickerDialog(row.CustomColor ?? Colors.Gray) { Owner = this };
    if (dialog.ShowDialog() == true) row.CustomColor = dialog.SelectedColor;
  }

  private void OnCloseClick(object sender, RoutedEventArgs e) => Close();

  private void OnCancelClick(object sender, RoutedEventArgs e) => Close();

  private void OnSaveClick(object sender, RoutedEventArgs e) {
    _store.Save(_viewModel.ToSettings());
    Close();
  }
}
