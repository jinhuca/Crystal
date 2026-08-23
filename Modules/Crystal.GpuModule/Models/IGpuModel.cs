using Crystal.Service.Gpu;

namespace Crystal.GpuModule.Models;

/// <summary>
/// The module's data source. Mirrors the CPU module's split: <see cref="Specs"/> emits the
/// static adapter inventory once, and <see cref="Sensors"/> re-emits a full snapshot (specs +
/// live load) on a fixed cadence while subscribed.
/// </summary>
public interface IGpuModel {
  /// <summary>
  /// Emits a single snapshot of the GPU adapter inventory, including static specs and the
  /// live sensor readings.
  /// </summary>
  IObservable<GpuSnapshot> Specs { get; }

  /// <summary>
  /// Emits a full snapshot of the GPU adapter inventory, including static specs and the live
  /// sensor readings.
  /// </summary>
  IObservable<GpuSnapshot> Sensors { get; }
}
