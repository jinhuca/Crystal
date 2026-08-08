using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Crystal.Service.Bios;

/// <summary>
/// Builds the platform firmware identity once and replays it to every subscriber.
/// Firmware data is static, so there is no poll loop — the build is connected
/// eagerly and its single emission is cached via <c>Replay(1)</c>.
/// </summary>
public sealed class BiosMonitor : IDisposable {
  private readonly IConnectableObservable<FirmwareSnapshot> _firmware;
  private readonly IDisposable _connection;

  public BiosMonitor(FirmwareInfoBuilder builder) {
    ArgumentNullException.ThrowIfNull(builder);
    _firmware = Observable.FromAsync(builder.BuildAsync).Replay(1);
    _connection = _firmware.Connect();
  }

  /// <summary>Static firmware identity; emits once and replays to new subscribers.</summary>
  public IObservable<FirmwareSnapshot> Firmware => _firmware.AsObservable();

  public void Dispose() => _connection.Dispose();
}
