using System.Windows.Controls;

namespace ProcessModule.Views;

/// <summary>The Processes tile: a Task Manager-style live list of processes with per-process
/// CPU, GPU, memory, disk and network columns, styled to match the dashboard tiles.</summary>
public partial class ProcessSummaryView : UserControl {
  public ProcessSummaryView() => InitializeComponent();
}
