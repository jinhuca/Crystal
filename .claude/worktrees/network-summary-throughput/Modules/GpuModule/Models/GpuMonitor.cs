using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace GpuModule.Models;

/// <summary>
/// Exposes GPU information as two streams, mirroring the CPU module's <c>CpuMonitor</c>:
/// <see cref="Specs"/> (built once, replayed to every subscriber) and <see cref="Sensors"/>
/// (re-sampled on a cadence, ref-counted so polling only runs while subscribed).
/// </summary>
public sealed class GpuMonitor : IDisposable {
  private readonly IConnectableObservable<GpuSnapshot> _specs;
  private readonly IObservable<GpuSnapshot> _sensors;
  private readonly IDisposable _specsConnection;

  public GpuMonitor(GpuInfoBuilder builder, TimeSpan? pollInterval = null, IScheduler? scheduler = null) {
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

  public IObservable<GpuSnapshot> Specs => _specs.AsObservable();
  public IObservable<GpuSnapshot> Sensors => _sensors;

  public void Dispose() => _specsConnection.Dispose();
}
