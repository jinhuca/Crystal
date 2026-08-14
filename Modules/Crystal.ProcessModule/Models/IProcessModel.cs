using Crystal.Service.Process;

namespace Crystal.ProcessModule.Models;

/// <summary>Exposes the live per-poll process list as an observable stream.</summary>
public interface IProcessModel {
  IObservable<IReadOnlyList<ProcessSample>> Processes { get; }

  /// <summary>Null when per-process GPU/Disk/Network are live; otherwise a short reason they aren't.</summary>
  string? MetricsStatusError { get; }
}
