using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace BiosModule.Models;

/// <summary>Builds the BIOS identity once and replays it to every subscriber. Static data,
/// so there is no polling loop.</summary>
public sealed class BiosModel : IBiosModel, IDisposable {
  private readonly IConnectableObservable<BiosSnapshot> _specs;
  private readonly IDisposable _connection;

  public BiosModel(BiosInfoBuilder builder) {
    ArgumentNullException.ThrowIfNull(builder);
    _specs = Observable.FromAsync(builder.BuildAsync).Replay(1);
    _connection = _specs.Connect();
  }

  public IObservable<BiosSnapshot> Specs => _specs.AsObservable();

  public void Dispose() => _connection.Dispose();
}
