using Crystal.Controls.PerformanceGraphs;
using GpuModule.Models;

namespace GpuModule.ViewModels;

/// <summary>
/// One GPU column in the view: an adapter's static identity plus its live core-load value and
/// history graph. The graph is a ring buffer owned by <see cref="PerformanceGraph"/>, so the
/// view hands the instance in via <see cref="AttachGraph"/> and the VM pushes samples into it.
/// </summary>
public sealed class GpuAdapterViewModel : BindableBase {
  private string _name = "—";
  private string _kindLabel = string.Empty;
  private double? _videoRamGB;
  private string _displayMode = string.Empty;
  private string? _driverVersion;
  private double _load;

  private PerformanceGraph? _loadGraph;

  public string Name { get => _name; private set => SetProperty(ref _name, value); }
  public string KindLabel { get => _kindLabel; private set => SetProperty(ref _kindLabel, value); }
  public double? VideoRamGB { get => _videoRamGB; private set => SetProperty(ref _videoRamGB, value); }
  public string DisplayMode { get => _displayMode; private set => SetProperty(ref _displayMode, value); }
  public string? DriverVersion { get => _driverVersion; private set => SetProperty(ref _driverVersion, value); }
  public double Load { get => _load; private set => SetProperty(ref _load, value); }

  public void AttachGraph(PerformanceGraph graph) => _loadGraph = graph;

  /// <summary>Refreshes the static identity from the inventory row.</summary>
  public void UpdateSpecs(GpuAdapterInfo info) {
    Name = info.Name;
    KindLabel = info.Kind == GpuKind.Integrated ? "Integrated GPU" : "Dedicated GPU";
    VideoRamGB = info.VideoRamGB;
    DisplayMode = info.DisplayMode;
    DriverVersion = info.DriverVersion;
  }

  /// <summary>Pushes a fresh load reading into the value and the history graph.</summary>
  public void UpdateLoad(double loadPercent) {
    Load = loadPercent;
    _loadGraph?.AddValue(loadPercent);
  }
}
