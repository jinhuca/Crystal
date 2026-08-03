using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace Crystal.Service.Sensors;

/// <summary>
/// Periodically polls the Telemetry provider and publishes a grouped
/// <see cref="SensorSnapshot"/> of all system sensors as an
/// <see cref="IObservable{T}"/> that modules can subscribe to.
/// <para>
/// The stream is cold/ref-counted: the poll timer only runs while at least one
/// observer is subscribed, and the underlying hardware session is disposed when
/// the monitor is disposed.
/// </para>
/// </summary>
public sealed class SensorMonitor : IDisposable {
  private readonly ISensorTelemetrySource _source;
  private readonly IObservable<SensorSnapshot> _snapshots;
  private bool _disposed;

  /// <summary>
  /// Creates a monitor over the real hardware Telemetry source, polling once per second.
  /// </summary>
  public SensorMonitor() : this(new TelemetrySensorSource()) { }

  /// <param name="source">Sensor source to poll. The monitor takes ownership and disposes it.</param>
  /// <param name="pollInterval">Sampling cadence; defaults to one second.</param>
  /// <param name="scheduler">Scheduler driving the poll timer; defaults to the shared default scheduler.</param>
  public SensorMonitor(ISensorTelemetrySource source, TimeSpan? pollInterval = null, IScheduler? scheduler = null) {
    ArgumentNullException.ThrowIfNull(source);
    _source = source;
    var interval = pollInterval ?? TimeSpan.FromSeconds(1);
    scheduler ??= DefaultScheduler.Instance;

    _snapshots = Observable
        .Interval(interval, scheduler)
        .Select(_ => new SensorSnapshot(_source.Read()))
        .Publish()
        .RefCount();
  }

  /// <summary>Emits a fresh grouped snapshot of all system sensors on each poll.</summary>
  public IObservable<SensorSnapshot> Snapshots => _snapshots;

  public void Dispose() {
    if (_disposed) return;
    _disposed = true;
    _source.Dispose();
  }
}
