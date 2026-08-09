using System.Reactive.Subjects;
using ProcessModule.Models;

namespace ProcessModule.Tests;

// Hand-driven IProcessModel: push poll snapshots through Samples.OnNext(...) to simulate the live
// stream without any ETW/WMI backend. MetricsStatusError is settable so the elevation-banner state
// can be exercised too.
internal sealed class FakeProcessModel : IProcessModel {
  public Subject<IReadOnlyList<ProcessSample>> Samples { get; } = new();
  public IObservable<IReadOnlyList<ProcessSample>> Processes => Samples;
  public string? MetricsStatusError { get; set; }
}
