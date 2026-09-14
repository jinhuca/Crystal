using System.Collections.ObjectModel;
using System.Windows.Media;

namespace Crystal.ProcessModule.ViewModels;

/// <summary>
/// One process currently being monitored in the detail panel. Wraps its <see cref="ProcessRowViewModel"/>
/// (whose live metrics the legend binds to), a fixed <see cref="Color"/> used for both the legend swatch
/// and this process's line in every utilization graph, and one rolling history per metric feeding those
/// lines. Created when a row is selected and discarded when it's deselected or exits.
/// </summary>
public sealed class MonitoredProcessViewModel {
  public MonitoredProcessViewModel(ProcessRowViewModel row, Brush color) {
    Row = row;
    Color = color;
  }

  /// <summary>The underlying list row; the legend binds its live Name/PID/CPU/Memory/GPU through this.</summary>
  public ProcessRowViewModel Row { get; }

  /// <summary>This process's assigned color (frozen), shared by its legend swatch and its graph lines.</summary>
  public Brush Color { get; }

  /// <summary>Convenience passthroughs so the legend template can bind without a nested Row path.</summary>
  public uint ProcessId => Row.ProcessId;

  public ObservableCollection<double> CpuHistory { get; } = [];
  public ObservableCollection<double> GpuHistory { get; } = [];
  public ObservableCollection<double> MemoryHistory { get; } = [];
  public ObservableCollection<double> DiskHistory { get; } = [];
  public ObservableCollection<double> NetHistory { get; } = [];
}
