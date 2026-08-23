using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Crystal.Service.Gpu;

/// <summary>
/// Exposes GPU information as two streams, mirroring the CPU service's <c>CpuMonitor</c>:
/// <see cref="Specs"/> (built once, replayed to every subscriber) and <see cref="Sensors"/>
/// (re-sampled on a cadence, ref-counted so polling only runs while subscribed).
/// </summary>
public sealed class GpuMonitor : IDisposable {
  /// <summary>
  /// Emits a single snapshot of the GPU adapter inventory, including static specs and the live
  /// sensor readings.
  /// </summary>
  private readonly IConnectableObservable<GpuSnapshot> _specs;

  /// <summary>
  /// Emits a full snapshot of the GPU adapter inventory, including static specs and the live
  /// sensor readings.
  /// </summary>
  private readonly IObservable<GpuSnapshot> _sensors;

  /// <summary>
  /// Disposes the Specs replay cache connection when the monitor is disposed.
  /// </summary>
  private readonly IDisposable _specsConnection;

  /// <summary>
  /// Initializes a new instance of the <see cref="GpuMonitor"/> class.
  /// </summary>
  /// <param name="builder">The GPU info builder.</param>
  /// <param name="pollInterval">The polling interval.</param>
  /// <param name="scheduler">The scheduler.</param>
  public GpuMonitor(
    GpuInfoBuilder builder,
    TimeSpan? pollInterval = null,
    IScheduler? scheduler = null) {
    ArgumentNullException.ThrowIfNull(builder);
    var interval = pollInterval ?? TimeSpan.FromSeconds(1);
    scheduler ??= DefaultScheduler.Instance;

    _specs = Observable.FromAsync(builder.BuildAsync).Replay(1);
    _specsConnection = _specs.Connect();

    _sensors = Observable
      .Interval(interval, scheduler)
      .SelectMany(_ => Observable.FromAsync(builder.BuildAsync))
      .Publish()
      .RefCount();
  }

  /// <summary>
  /// Emits a single snapshot of the GPU adapter inventory, including static specs and the live
  /// sensor readings.
  /// </summary>
  public IObservable<GpuSnapshot> Specs => _specs.AsObservable();

  /// <summary>
  /// Emits a full snapshot of the GPU adapter inventory, including static specs and the live
  /// sensor readings.
  /// </summary>
  public IObservable<GpuSnapshot> Sensors => _sensors;

  /// <summary>
  /// Disposes the monitor, which owns the polling lifetime and the Specs replay cache.
  /// </summary>
  public void Dispose() => _specsConnection.Dispose();
}
