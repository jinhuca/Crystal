using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Crystal.Service.Bios;

/// <summary>
/// Builds the platform firmware identity once and replays it to every subscriber.
/// Firmware data is static, so there is no poll loop — the build is connected
/// eagerly and its single emission is cached via <c>Replay(1)</c>.
/// </summary>
public sealed class BiosMonitor : IDisposable {
  /// <summary>
  /// The connectable, single-emission firmware stream. <c>Replay(1)</c> caches the one snapshot the
  /// builder produces so every subscriber — including late ones — receives it without re-running the build.
  /// </summary>
  private readonly IConnectableObservable<FirmwareSnapshot> _firmware;

  /// <summary>
  /// Handle to the eager <see cref="IConnectableObservable{T}.Connect"/> subscription. Disposing it
  /// tears down the underlying build subscription; released in <see cref="Dispose"/>.
  /// </summary>
  private readonly IDisposable _connection;

  /// <summary>
  /// Creates the monitor and immediately connects the firmware stream, kicking off the one-time build.
  /// Building eagerly (rather than on first subscribe) means the snapshot is ready by the time the UI
  /// subscribes, and the result is shared across all subscribers via replay.
  /// </summary>
  /// <param name="builder">Composes the firmware snapshot from WMI, the registry and SMBIOS.</param>
  public BiosMonitor(FirmwareInfoBuilder builder) {
    ArgumentNullException.ThrowIfNull(builder);
    _firmware = Observable.FromAsync(builder.BuildAsync).Replay(1);
    _connection = _firmware.Connect();
  }

  /// <summary>
  /// Static firmware identity; emits once and replays to new subscribers.
  /// </summary>
  public IObservable<FirmwareSnapshot> Firmware => _firmware.AsObservable();

  /// <summary>
  /// Disposes the eager connection, ending the shared firmware subscription.
  /// </summary>
  public void Dispose() => _connection.Dispose();
}
