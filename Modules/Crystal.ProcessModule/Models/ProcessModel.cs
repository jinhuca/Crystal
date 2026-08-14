using Crystal.Service.Process;

namespace Crystal.ProcessModule.Models;

/// <summary>Adapts <see cref="ProcessMonitor"/> into <see cref="IProcessModel"/>; the monitor
/// owns the polling lifetime and this type just forwards its stream.</summary>
public sealed class ProcessModel : IProcessModel {
  private readonly ProcessMonitor _monitor;

  public ProcessModel(ProcessMonitor monitor) => _monitor = monitor;

  public IObservable<IReadOnlyList<ProcessSample>> Processes => _monitor.Samples;

  public string? MetricsStatusError => _monitor.MetricsStatusError;
}
