namespace GpuModule.Models;

/// <summary>
/// The module's data source. Mirrors the CPU module's split: <see cref="Specs"/> emits the
/// static adapter inventory once, and <see cref="Sensors"/> re-emits a full snapshot (specs +
/// live load) on a fixed cadence while subscribed.
/// </summary>
public interface IGpuModel {
  IObservable<GpuSnapshot> Specs { get; }
  IObservable<GpuSnapshot> Sensors { get; }
}
