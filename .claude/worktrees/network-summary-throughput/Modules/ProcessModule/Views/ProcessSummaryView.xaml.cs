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

  // Clicking a column header sorts the list by that column (toggling asc/desc); the sort key lives
  // on the column via GridViewSort.SortProperty.
  private void OnColumnHeaderClick(object sender, RoutedEventArgs e) {
    if (e.OriginalSource is not GridViewColumnHeader header) return;
    if (header.Column is null) return;
    var sortProperty = GridViewSort.GetSortProperty(header.Column);
    if (string.IsNullOrEmpty(sortProperty)) return;
    if (DataContext is ProcessListViewModel vm) vm.SortBy(sortProperty);
  }
}
