namespace ProcessModule.Models;

/// <summary>Exposes the live per-poll process list as an observable stream.</summary>
public interface IProcessModel {
  IObservable<IReadOnlyList<ProcessSample>> Processes { get; }
}
