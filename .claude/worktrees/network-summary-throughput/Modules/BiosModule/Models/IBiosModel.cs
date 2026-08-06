namespace BiosModule.Models;

/// <summary>Static BIOS identity as a replayed stream (built once). No live sensor.</summary>
public interface IBiosModel {
  IObservable<BiosSnapshot> Specs { get; }
}
