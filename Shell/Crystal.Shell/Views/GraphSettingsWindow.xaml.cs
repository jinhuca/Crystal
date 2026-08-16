using Crystal.Shell.Settings;
using Crystal.Shell.ViewModels;
using System.Windows;

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

  private void OnCloseClick(object sender, RoutedEventArgs e) => Close();

  private void OnCancelClick(object sender, RoutedEventArgs e) => Close();

  private void OnSaveClick(object sender, RoutedEventArgs e) {
    _store.Save(_viewModel.ToSettings());
    Close();
  }
}
