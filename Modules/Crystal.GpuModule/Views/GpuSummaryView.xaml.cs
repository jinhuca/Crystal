using Crystal.GpuModule.ViewModels;
using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Crystal.GpuModule.Views;

/// <summary>
/// Compact GPU tile on the dashboard: one column per adapter (integrated / dedicated),
/// composing the per-metric tiles (utilization, temperature, clock, power) defined in
/// Views/SummaryViews. Double-clicking opens the full GPU detail view.
/// </summary>
public partial class GpuSummaryView : UserControl {
  /// <summary>
  /// Initializes a new instance of the <see cref="GpuSummaryView"/> class.
  /// </summary>
  public GpuSummaryView() {
    InitializeComponent();

    // Register with handledEventsToo so the double-click still reaches us after the adapter strip's
    // ScrollViewer/ScrollContentPresenter marks the button-down handled; a plain bubbling handler on
    // the root border would never fire for clicks that land on a tile inside the scroll viewer.
    AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(OnTileClick), handledEventsToo: true);
  }

  /// <summary>
  /// Handles the MouseDoubleClick event on the GPU summary tile. If the DataContext is 
  /// an IGpuViewModel and the ShowDetailCommand can be executed, it executes the command
  /// to show the detailed GPU view.
  /// </summary>
  /// <param name="sender">The source of the event.</param>
  /// <param name="e">The event data.</param>
  private void OnTileClick(object sender, MouseButtonEventArgs e) {
    if (e.ClickCount >= 2 && DataContext is IGpuViewModel vm && vm.ShowDetailCommand.CanExecute(null)) {
      vm.ShowDetailCommand.Execute(null);
    }
  }

  /// <summary>
  /// Translates vertical wheel movement into horizontal scrolling so the adapter strip can be
  /// panned with the wheel even though its vertical scrolling is disabled and scrollbars are hidden.
  /// </summary>
  /// <param name="sender">The scroll viewer hosting the adapter strip.</param>
  /// <param name="e">The wheel event data.</param>
  private void OnScrollHorizontally(object sender, MouseWheelEventArgs e) {
    if (sender is ScrollViewer scroller) {
      scroller.ScrollToHorizontalOffset(scroller.HorizontalOffset - e.Delta);
      e.Handled = true;
    }
  }

  /// <summary>
  /// Hides the integrated adapter from the summary strip while leaving the shared adapter list (and
  /// so the detail view, which shows every adapter) untouched. Wraps the bound collection in a
  /// private <see cref="CollectionViewSource"/> view so the filter is local to this ItemsControl
  /// rather than applied to the default view. The integrated tile is disabled for now.
  /// </summary>
  private void OnStripLoaded(object sender, RoutedEventArgs e) {
    if (sender is not ItemsControl strip) return;
    // Already wrapped on a prior Loaded (the strip can be re-added to the tree): leave it be.
    if (strip.ItemsSource is ICollectionView) return;
    if (strip.ItemsSource is not IEnumerable adapters) return;

    var view = new CollectionViewSource { Source = adapters }.View;
    view.Filter = o => o is GpuAdapterViewModel { IsIntegrated: false };
    strip.ItemsSource = view;
  }
}
